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
            if (hiddenObject != null)
                hiddenObject.layer = 0;
        }
    }

    private void Start()
    {
        InitalizeAttackInfo();
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

        if (inverseMouse)
            viewPoint.rotation = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
        else
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    private void LimitSpeed()
    {
        // Rigidbody. Velocity : 현재 Rigidbody 객체의 속도
        // Rigidbody.velocity 현재 캐릭터의 속도, 방향을 즉시 변경시킨다. 비현실적인 움직임으로 보일 수 있다.
        Vector3 curretSpeed = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);

        if (curretSpeed.magnitude > moveSpeed)
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
    public GameObject bulletImpact;      // 플레이어의 공격의 피격 효과 인스턴스
    public float shootDistance = 10f;    // 최대 사격 거리
    public float fireCoolTime = 0.1f;
    private float fireCounter;

    // 플레이어의 입력 -> 로직 -> (조건 - Physics.Raycast)실제 효과 처리
    // 공격했다는 사실
    private void PlayerAttack()
    {
        InputAttack();
    }

    private void InputAttack()
    {
        fireCounter -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {            
            if(fireCounter <= 0)
             Shoot();
        }
    }

    private void InitalizeAttackInfo()
    {
        fireCounter = fireCoolTime;
    }
    // 들여 쓰기 핫 키 : 드래그 후 ctrl + K + D 
    private void Shoot()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, shootDistance))
        {
            // Raycast가 Hit한 지점에 object가 생성된다.
            // 생성된 각도... 
            // 생성되는 위치가 오브젝트랑 겹쳐보이는 현상...
            GameObject bulletObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));

            // 일정 시간 후에 인스턴스한 오브젝트를 파괴한다.
            Destroy(bulletObject, 0.5f);
        }

        Debug.Log($"충돌한 오브젝트의 이름 {hit.collider.gameObject.name}");            // raycasthit에 의해 충돌한 지점에 collider가 있으면 반환해주는 코드
        Debug.Log($"충돌한 지점의 Vector3을 반환하다 : {hit.point}");                   // Raycast에 의해서 감지된 위치를 반환
        Debug.Log($"카메라와 충돌한 지점 사이의 거리를 반환 : {hit.distance}");          //  두 벡터의 차이
        Debug.Log($"충돌한 오브젝트의 법선(normal)을 반환 :{hit.normal} ");             //  cam - 충돌한 오브젝트 평면의 벡터 외적.. normal        

        //  사격이 끝날 때, 사격 쿨타임을 리셋
        fireCounter = fireCoolTime;
    }

    #endregion
    // Alt + 방향키(아래) : 코드를 움직일 수 있다.
    private void OnDrawGizmos()
    {
        // 에디터 실행해서.  DrawLine 땅에 닿는 길이를 설정. 땅과 충돌을 하는지 보기 위한 Gizmo 함수
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * groundCheckDistance));
        // 플레이어의 사격 범위를 파악하기 위한 Gizmo 함수
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(cam.transform.position, cam.transform.forward * shootDistance);
    }

}
