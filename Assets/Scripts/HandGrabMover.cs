using UnityEngine;
using UnityEngine.InputSystem;

public class HandGrabMover : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("검지 트리거 (Activate Action) 등을 연결하세요")]
    [SerializeField] private InputActionReference grabAction;

    [Header("Physics Settings")]
    [SerializeField] private Rigidbody playerRigidbody;
    
    [Tooltip("당길 때 이동 속도 배율 (1.0 = 1:1 리얼함, 1.2 = 약간 빠르게)")]
    public float pullSensitivity = 1.1f;

    [Tooltip("손을 놓을 때 던져지는 힘 (0.0 = 뚝 떨어짐, 0.5 = 적당히 날아감)")]
    public float throwMultiplier = 0.5f;

    [Header("Ground Check")]
    public LayerMask grabLayer; // 바닥이나 벽 레이어
    public float grabRadius = 0.1f;

    // 내부 변수
    private bool isPressed = false;
    private bool isGrabbing = false;
    private Vector3 previousHandPos;
    private Vector3 currentVelocity; // 놓을 때를 위한 속도 저장

    void OnEnable()
    {
        if (grabAction != null)
        {
            grabAction.action.performed += ctx => isPressed = true;
            grabAction.action.canceled += ctx => isPressed = false;
            grabAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (grabAction != null) grabAction.action.Disable();
    }

    void Update()
    {
        // 바닥에 닿았는지 체크
        bool isTouching = Physics.CheckSphere(transform.position, grabRadius, grabLayer);

        // [잡기 시작 조건] : 버튼 누름 + 바닥에 닿음 + 아직 안 잡음
        if (isPressed && isTouching && !isGrabbing)
        {
            StartGrab();
        }
        // [놓기 조건] : 버튼 땜 + 잡고 있었음
        else if (!isPressed && isGrabbing)
        {
            EndGrab();
        }
    }

    void FixedUpdate()
    {
        if (isGrabbing)
        {
            // 1. 손의 이동 거리 계산 (이전 위치 - 현재 위치)
            // 내가 손을 뒤로(-Z) 당기면, 몸은 앞으로(+Z) 가야 함 -> 그래서 (이전 - 현재)
            Vector3 handDelta = previousHandPos - transform.position;

            // 2. 이동 거리를 속도로 변환
            Vector3 targetVelocity = handDelta / Time.fixedDeltaTime * pullSensitivity;

            // 3. 리지드바디에 속도 직접 주입 (이게 '고정'되는 느낌의 핵심)
            playerRigidbody.linearVelocity = targetVelocity;

            // 4. 놓을 때를 위해 현재 속도 저장
            currentVelocity = targetVelocity;
        }

        previousHandPos = transform.position;
    }

    void StartGrab()
    {
        isGrabbing = true;
        playerRigidbody.useGravity = false; // 매달려야 하니까 중력 끄기
        playerRigidbody.linearVelocity = Vector3.zero; // 미끄러짐 방지
        previousHandPos = transform.position;
    }

    void EndGrab()
    {
        isGrabbing = false;
        playerRigidbody.useGravity = true; // 중력 복구
        
        // 잡고 있던 속도의 일부를 유지하며 날아가기 (관성)
        playerRigidbody.linearVelocity = currentVelocity * throwMultiplier;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrabbing ? Color.green : (isPressed ? Color.yellow : Color.red);
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}