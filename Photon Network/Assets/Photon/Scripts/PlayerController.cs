using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;    // 플레이어 이동 속도
    private Vector3 moveDir;                          // 플레이어의 이동 방향
    private Vector3 moveMent;                         // 이동 변수에 의한 결과
    private Rigidbody rigidbody;

    [Header("View")]
    public Transform viewPoint;                       // View Point를 통해서 캐릭터의 상하 회전을 구현
    public float mouseSensitiy = 1f;                  // 마우스의 회전 속도 제어 값
    public float verticalRotation;                    // 회전 변경 사항 저장 변수
    private Vector2 mouseInput;                      // Mouse의 Input값을 저장하는 변수
    public bool inverseMouse;                        // 마우스를 위로 움직일 때 아래로 회전할지, 위로 회전할지 결정하는 변수
    public Camera cam;                               // Player 본인이 소유한 카메라

    [Header("Jump")]
    public KeyCode JumpkeyCode = KeyCode.Space;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 5f;
    public bool isGrounded;
    private float yVel;

    [Header("photon Component")]
    PhotonView PV;                                   // PV객체를 이용하여 인스턴스된 오브젝트의 소유권 확인
    public GameObject hiddenObject;                  // 1인칭 시점에서 숨기고 싶은 오브젝트

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
        if (!photonView.IsMine)  // 내 플레이어가 아니면 카메라를 비활성화
        {
            cam.gameObject.SetActive(false);
            if(hiddenObject!= null)
                hiddenObject.layer = 0;
        }
    }

    // Update is called once per frame 
    // 컴퓨터 마다 Frame을 생성하는 성능이 다르기 때문에 Time.deltaTime 컴퓨터간의 같은 시간에 같은 횟수를 보장해주었다.
    void Update()
    {
        if (photonView.IsMine)
        {
            CheckCollider();

            // 플레이어의 인풋 
            HandleInput();
            HandleView();

            PlayerAttack();
        }
    }

    private void FixedUpdate()
    {
        // Rigidbody AddForce(관성의 힘을 제어해주는 함수) - moveSpeed 만큼만 움직일
        Move();
        LimitSpeed();
    }


    private void HandleInput()
    {
        // 캐릭터 이동 방향
        float Horizontal = Input.GetAxisRaw("Horizontal");
        float Vertical = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(Horizontal, 0, Vertical);

        // 캐릭터 회전
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitiy;
        moveMent = (transform.forward * moveDir.z) + (transform.right * moveDir.x).normalized;

        // 캐릭터 점프
        ButtonJump();
    }

    private void HandleView()
    {
        // 캐릭터의 좌우 회전
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + mouseInput.x, transform.eulerAngles.z);
        // 캐릭터의 상하 회전
        verticalRotation += mouseInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60);

        if(inverseMouse)
            viewPoint.rotation = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
        else
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    private void LimitSpeed()
    {
        // Rigidbody. Velocity : 현재 Rigidbody 객체의 속도
        // Rigidbody.velocity 현재 캐릭터의 속도, 방향을 즉시 변경시킨다. 비현실적인 움직임으로 보일 수 있다.
        Vector3 curretSpeed = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);

        if(curretSpeed.magnitude > moveSpeed)
        {
            Vector3 limitSpeed = curretSpeed.normalized * moveSpeed;
            rigidbody.velocity = new Vector3(limitSpeed.x, rigidbody.velocity.y, limitSpeed.z);
        }
    }

    private void Move()
    {
        rigidbody.AddForce(moveMent * moveSpeed, ForceMode.Impulse);  // 점점 속도가 빨리지는
    }

    private void ButtonJump()
    {
        // 조건문 :  키를 입력 + 현재 상태 땅인지 아닌지
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
        // 에디터 실행해서.  DrawLine 땅에 닿는 길이를 설정.
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

            Debug.Log($"충돌한 오브젝트의 이름 {hit.collider.gameObject.name}");
        }
    }

    #endregion
}
