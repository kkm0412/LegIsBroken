using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class HandMoveProvider : MonoBehaviour
{
    [SerializeField]
    private InputActionReference gripAction;

    [SerializeField]
    private Rigidbody playerRigidbody;

    public float forceMultiplier = 150f;

    public LayerMask groundLayer;

    public float groundCheckRadius = 0.1f;

    private bool isGripPressed = false;     //중지버튼을 눌렀는가
    private bool isHandGrounded = false;    //손이 땅에 있는가

    private Vector3 previousHandPosition;   //(속도계산용) 손의 이전 프레임 위치

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
        //손이 바닥에 닿았는지 확인
        isHandGrounded = Physics.CheckSphere(transform.position, groundCheckRadius, groundLayer);

        //손이 닿았을 때
        if (isHandGrounded)
        {
            //손 이동속도 계산
            Vector3 handVelocity = (previousHandPosition - this.transform.position);

            //총 힘을 계산하고 플레이어 몸에 힘을 가함
            Vector3 forceToApply = handVelocity * forceMultiplier / Time.fixedDeltaTime;
            playerRigidbody.AddForce(forceToApply);
            //playerRigidbody.AddForce(forceToApply, ForceMode.Acceleration); //질량계산 X
        }
        //현재손 위치 이전손위치로 저장
        previousHandPosition = this.transform.position;
    }

    //디버깅용
    private void OnDrawGizmos()
    {
        Gizmos.color = isHandGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
    }

    
}