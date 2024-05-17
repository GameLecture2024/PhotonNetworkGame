using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;    // �÷��̾� �̵� �ӵ�
    private Vector3 moveDir;                          // �÷��̾��� �̵� ����
    private Vector3 moveMent;                         // �̵� ������ ���� ���
    private Rigidbody rigidbody;

    [Header("View")]
    public Transform viewPoint;                       // View Point�� ���ؼ� ĳ������ ���� ȸ���� ����
    public float mouseSensitiy = 1f;                  // ���콺�� ȸ�� �ӵ� ���� ��
    public float verticalRotation;                    // ȸ�� ���� ���� ���� ����
    private Vector2 mouseInput;                      // Mouse�� Input���� �����ϴ� ����
    public bool inverseMouse;                        // ���콺�� ���� ������ �� �Ʒ��� ȸ������, ���� ȸ������ �����ϴ� ����
    public Camera cam;                               // Player ������ ������ ī�޶�

    [Header("Jump")]
    public KeyCode JumpkeyCode = KeyCode.Space;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 5f;
    public bool isGrounded;
    private float yVel;

    [Header("photon Component")]
    PhotonView PV;                                   // PV��ü�� �̿��Ͽ� �ν��Ͻ��� ������Ʈ�� ������ Ȯ��
    public GameObject hiddenObject;                  // 1��Ī �������� ����� ���� ������Ʈ

    private void Awake()
    {
        InitializeCompoments();
        PhotonSetup();
    }

    private void InitializeCompoments()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void PhotonSetup()
    {
        if (!photonView.IsMine)  // �� �÷��̾ �ƴϸ� ī�޶� ��Ȱ��ȭ
        {
            cam.gameObject.SetActive(false);
            if(hiddenObject!= null)
                hiddenObject.layer = 0;
        }
    }

    // Update is called once per frame 
    // ��ǻ�� ���� Frame�� �����ϴ� ������ �ٸ��� ������ Time.deltaTime ��ǻ�Ͱ��� ���� �ð��� ���� Ƚ���� �������־���.
    void Update()
    {
        if (photonView.IsMine)
        {
            CheckCollider();

            // �÷��̾��� ��ǲ 
            HandleInput();
            HandleView();

            PlayerAttack();
        }
    }

    private void FixedUpdate()
    {
        // Rigidbody AddForce(������ ���� �������ִ� �Լ�) - moveSpeed ��ŭ�� ������
        Move();
        LimitSpeed();
    }


    private void HandleInput()
    {
        // ĳ���� �̵� ����
        float Horizontal = Input.GetAxisRaw("Horizontal");
        float Vertical = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(Horizontal, 0, Vertical);

        // ĳ���� ȸ��
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitiy;
        moveMent = (transform.forward * moveDir.z) + (transform.right * moveDir.x).normalized;

        // ĳ���� ����
        ButtonJump();
    }

    private void HandleView()
    {
        // ĳ������ �¿� ȸ��
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + mouseInput.x, transform.eulerAngles.z);
        // ĳ������ ���� ȸ��
        verticalRotation += mouseInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60);

        if(inverseMouse)
            viewPoint.rotation = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
        else
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    private void LimitSpeed()
    {
        // Rigidbody. Velocity : ���� Rigidbody ��ü�� �ӵ�
        // Rigidbody.velocity ���� ĳ������ �ӵ�, ������ ��� �����Ų��. ���������� ���������� ���� �� �ִ�.
        Vector3 curretSpeed = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);

        if(curretSpeed.magnitude > moveSpeed)
        {
            Vector3 limitSpeed = curretSpeed.normalized * moveSpeed;
            rigidbody.velocity = new Vector3(limitSpeed.x, rigidbody.velocity.y, limitSpeed.z);
        }
    }

    private void Move()
    {
        rigidbody.AddForce(moveMent * moveSpeed, ForceMode.Impulse);  // ���� �ӵ��� ��������
    }

    private void ButtonJump()
    {
        // ���ǹ� :  Ű�� �Է� + ���� ���� ������ �ƴ���
        if(Input.GetKeyDown(JumpkeyCode) && isGrounded)
        {
            Jump();
        }
    }
    private void Jump()
    {
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);

        rigidbody.AddForce(transform.up * jumpPower, ForceMode.Impulse);
    }

    private void CheckCollider()
    {
        // Physics
        isGrounded = Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * groundCheckDistance));
        // ������ �����ؼ�.  DrawLine ���� ��� ���̸� ����.
    }

    #region Player Attack

    private void PlayerAttack()
    {
        Shoot();
    }

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 100f);

            Debug.Log($"�浹�� ������Ʈ�� �̸� {hit.collider.gameObject.name}");
        }
    }

    #endregion
}
