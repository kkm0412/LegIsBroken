using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;
using Unity.VisualScripting;

public class HandMoveProvider : MonoBehaviour
{
    [SerializeField]
    private InputActionReference gripAction;

    [Header("Physics Settings")]    //물리 관련
    [SerializeField]
    private Rigidbody playerRigidbody;
    public float forceMultiplier = 2500f;   //총 힘
    // public float jumpThreshold = 1500f;     //점프 임계값
    // public float VerticalDamping = 0.1f;    //점프 임계값 못넘을시 y 억제값

    private Vector3 previousHandPosition;   //(속도계산용) 손의 이전 프레임 위치

    [Header("Ground Check Settings")]
    [SerializeField]
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.1f;

    private bool isGripPressed = false;     //중지버튼을 눌렀는가
    private bool isHandGrounded = false;    //손이 땅에 있는가
    private bool wasHandGrounded = false;   //이전 프레임에 땅이 닿았는지


    void OnEnable()
    {
        //입력 액션 이벤트 등록
        gripAction.action.performed += OnGripPressed;
        gripAction.action.canceled += OnGripReleased;
        gripAction.action.Enable();

        //손 위치 저장
        previousHandPosition = transform.position;
    }
    void OnDisable()
    {
        //입력 액션 이벤트 해제
        gripAction.action.performed -= OnGripPressed;
        gripAction.action.canceled -= OnGripReleased;
        gripAction.action.Disable();

    }
    //중지 누를때
    private void OnGripPressed(InputAction.CallbackContext context)
    {
        isGripPressed = true;
    }
    //중지 땔 때
    private void OnGripReleased(InputAction.CallbackContext context)
    {
        isGripPressed = false;
    }


    //물리 업데이트
    void FixedUpdate()
    {
        bool currentGrounded = Physics.CheckSphere(transform.position, groundCheckRadius, groundLayer);

        if (currentGrounded && !wasHandGrounded)
        {
            previousHandPosition = transform.position;
        }
        //손이 바닥에 닿았는지 확인
        isHandGrounded = Physics.CheckSphere(transform.position, groundCheckRadius, groundLayer);
        //손이 닿고, 검지부분 눌렀을 때
        if (isHandGrounded && isGripPressed)
        {
            ForceToBody();
        }
        //현재손 위치 이전손위치로 저장
        previousHandPosition = this.transform.position;
    }

    //몸에 물리력 적용
    private void ForceToBody()
    {
        //손 이동속도 계산
        Vector3 handVelocity = (previousHandPosition - this.transform.position);
        //총 힘을 계산하고 플레이어 몸에 힘을 가함
        Vector3 forceToApply = handVelocity * forceMultiplier / Time.fixedDeltaTime;
        // if (forceToApply.y < jumpThreshold)
        // {
        //     forceToApply.y *= VerticalDamping;
        // }
        playerRigidbody.AddForce(forceToApply, ForceMode.Impulse);
        //playerRigidbody.AddForce(forceToApply, ForceMode.Acceleration); //질량계산 X
    }

    //디버깅용
    private void OnDrawGizmos()
    {
        Gizmos.color = isHandGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
    }
}