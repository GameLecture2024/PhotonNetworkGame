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
            if (hiddenObject != null)
                hiddenObject.layer = 0;
        }
    }

    private void Start()
    {
        InitalizeAttackInfo();
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

        if (inverseMouse)
            viewPoint.rotation = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
        else
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    private void LimitSpeed()
    {
        // Rigidbody. Velocity : ���� Rigidbody ��ü�� �ӵ�
        // Rigidbody.velocity ���� ĳ������ �ӵ�, ������ ��� �����Ų��. ���������� ���������� ���� �� �ִ�.
        Vector3 curretSpeed = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);

        if (curretSpeed.magnitude > moveSpeed)
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
        if (Input.GetKeyDown(JumpkeyCode) && isGrounded)
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

    #region Player Attack
    public GameObject bulletImpact;      // �÷��̾��� ������ �ǰ� ȿ�� �ν��Ͻ�
    public float bulletAliveTime = 2f;
    public float shootDistance = 10f;    // �ִ� ��� �Ÿ�
    public float fireCoolTime = 0.1f;
    private float fireCounter;
    public bool isAutomatic;             // True�� �� ���� ���� ����. false�� �� ���� Ŭ������ ���� ����

    [Header("������Ʈ �ý���")]
    public float maxHeat = 10f, heatCount, heatPerShot;    // ���⸦ �����ϴ� ����
    public float coolRate, overHeatCoolRate;  // ���⸦ ������ ���� ���� : overHeatcoolRate�� coolRate���� Ŀ�� �Ѵ�.
    private bool overHeated = false;          // maxHeat�� �����ϸ� true, heatCount <= 0 �ٽ� false

    public Gun[] allGuns;
    private int currentGunIndex = 0;
    private MuzzleFlash currentMuzzle;

    public PlayerUI playerUI;
    

    // �÷��̾��� �Է� -> ���� -> (���� - Physics.Raycast)���� ȿ�� ó��
    // �����ߴٴ� ���
    private void PlayerAttack()
    {
        CoolDownFunction();
        SelectGun();
        InputAttack();
    }

    private void CoolDownFunction()
    {
        fireCounter -= Time.deltaTime;
        OverHeatedCoolDown();
    }

    // Update����, ���� HeatCount ����ؼ�, OverHeat���� �Ǻ��ϴ� �Լ�
    private void OverHeatedCoolDown()
    {
        // ���� OverHeat ����    
        if (overHeated)
        {
            heatCount -= overHeatCoolRate * Time.deltaTime;

            if (heatCount <= 0)
            {
                heatCount = 0;
                overHeated = false;
                // UI���� OverHeat ǥ�ø� ����
                playerUI.overHeatTextObject.SetActive(false);
            }
        }
        // !overHeated
        else 
        {
            heatCount -= coolRate * Time.deltaTime;
            if (heatCount <= 0)
                heatCount = 0;
        }

        playerUI.currentWeaponSlider.value = heatCount;
    }

    private void SelectGun() // ���콺 �� ��ư �̿��ؼ� 1 ~ N ��ϵ� ���⸦ ���� ��� 
                             // 1�� -> 1�� ����, 2�� -> 2�� ����, 3�� 3������
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            currentGunIndex++;

            // ���� ����.. 
            // �迭�� ���̺��� ū ���  // 0 ~ n(Length - 1) 
            if (currentGunIndex >= allGuns.Length)
                currentGunIndex = 0;

            // ���� ����
            SwitchGun();
            playerUI.SetWeaponSlot(currentGunIndex);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            currentGunIndex--;

            // ���� ����.. 
            // �迭�� ���̺��� ū ���  // 0 ~ n(Length - 1) 
            if (currentGunIndex < 0)
                currentGunIndex = allGuns.Length - 1;

            // ���� ����
            SwitchGun();
            playerUI.SetWeaponSlot(currentGunIndex);
        }

        // Ű���� ���� �Է�(1,2,3)���� ���� �����ϱ�

        // Ű������ ���ڸ� �Է� �޴´�. => ���ڸ� ������ �ٲ۴� => if , else if �ݺ��� ��ȯ
        // �� �ڵ带 �����ϰ�, ������ ������ ������ �˷��� (�ڵ� ����)

        // Todo : allGuns �迭�� �����ͷ� ó���ϴ� ����� ���� �ȵ�
        for(int i = 0; i < allGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                currentGunIndex = i;
                SwitchGun();
                playerUI.SetWeaponSlot(currentGunIndex);
            }
        }
        // �Է� ���� ���ڸ� allGuns �迭�� Gun �����ͷ� ��ȯ�Ѵ�. 

    }

    private void InputAttack()
    {
        if (Input.GetMouseButtonDown(0) && !isAutomatic && !overHeated)  // ���콺�� ������ ��
        {            
            if(fireCounter <= 0)
             Shoot();
        }

        if (Input.GetMouseButton(0) && isAutomatic && !overHeated)  // Mouse Up�Ǳ� �� ���� ���.. �ڵ� �� ����
        {
            if (fireCounter <= 0)
                Shoot();
        }
    }

    private void InitalizeAttackInfo()
    {
        Cursor.lockState = CursorLockMode.Locked;   // Boolean �ɼ� - Check Locked true, false
        Cursor.visible = false;
        fireCounter = fireCoolTime;
        currentGunIndex = 0;
        SwitchGun();
    }
    // �鿩 ���� �� Ű : �巡�� �� ctrl + K + D 
    private void Shoot()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, shootDistance))
        {
            // Raycast�� Hit�� ������ object�� �����ȴ�.
            // ������ ����... 
            // �����Ǵ� ��ġ�� ������Ʈ�� ���ĺ��̴� ����...
            GameObject bulletObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));

            // ������ �� Muzzle Effect �߻�
            currentMuzzle.gameObject.SetActive(true);
            // ���� �ð� �Ŀ� �ν��Ͻ��� ������Ʈ�� �ı��Ѵ�.
            Destroy(bulletObject, bulletAliveTime);
        }

        //  ����� ���� ��, ��� ��Ÿ���� ����
        fireCounter = fireCoolTime;
        // OverHeat���� ��� �Լ�
        ShootHeatSystem();
    }

    private void ShootHeatSystem()
    {
        heatCount = heatCount + heatPerShot;

        if(heatCount >= maxHeat)
        {
            heatCount = maxHeat;
            overHeated = true;
            // ������Ʈ ui Ȱ��ȭ
            playerUI.overHeatTextObject.SetActive(true);
        }       
    }

    private void SwitchGun()
    {
        // allGuns�ȿ� �ִ� ��� ������Ʈ ��Ȱ��ȭ.
        foreach (var gun in allGuns)
            gun.gameObject.SetActive(false);
        // allGuns[currentGunIndex] �ش��ϴ� ������Ʈ Ȱ��ȭ.
        allGuns[currentGunIndex].gameObject.SetActive(true);

        // Gun�� �Ű� ������ ����ϴ� Gun ���� ����ȭ �Լ�
        SetGunAttribute(allGuns[currentGunIndex]);
    }

    private void SetGunAttribute(Gun gun) // Class -> Data 
    {
        fireCoolTime = gun.fireCoolTime;
        // Gun�� ���� �ִ� �Ӽ��� Player Controller ������ ���� ��Ű��
        isAutomatic = gun.isAutomatic;
        currentMuzzle = gun.MuzzleFlash.GetComponent<MuzzleFlash>();
        heatPerShot = gun.heatPerShot;

        // maxHeat Value�� �����ǰ� ���� �ۼ�
        playerUI.currentWeaponSlider.maxValue = maxHeat;
    }

    #endregion
    // Alt + ����Ű(�Ʒ�) : �ڵ带 ������ �� �ִ�.
    private void OnDrawGizmos()
    {
        // ������ �����ؼ�.  DrawLine ���� ��� ���̸� ����. ���� �浹�� �ϴ��� ���� ���� Gizmo �Լ�
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * groundCheckDistance));
        // �÷��̾��� ��� ������ �ľ��ϱ� ���� Gizmo �Լ�
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(cam.transform.position, cam.transform.forward * shootDistance);
    }

}
