using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Collections.Generic; // 평균값 계산용

public class HandMoveProvider : MonoBehaviour
{
    [SerializeField] private InputActionReference grabAction;
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Phase 2 Prep: Physics Hand")]
    [Tooltip("비워두면 현재 컨트롤러 위치를 사용합니다. 나중에 Physics Hand를 여기에 넣을 예정입니다.")]
    public Transform handTrackingTransform;
    
    [Header("Physics Options")]
    [Tooltip("이동 감도 (1.0 = 정직함, 1.5 = 빠름)")]
    public float sensitivity = 1.3f;
    [Tooltip("움직임 부드러움 정도")]
    [Range(0.1f, 1f)]
    public float movementSmoothness = 0.6f;

    [Tooltip("던지기 힘 배율")]
    public float throwMultiplier = 1.2f;

    [Header("limits")]
    public bool allowVerticalMovement = true;
    public float maxVelocity = 15f; // 너무 빠르면 물리 뚫림 발생하므로 제한

    [Header("GrabAbles")]
    public LayerMask grabLayer;
    public float grabRadius = 0.2f;

    // 내부 변수
    private bool isPressed = false;
    private bool isGrabbing = false;
    private Vector3 previousHandPos;
    private Transform currentHand;

    // 던지기 방향 보정용 (평균값 계산)
    private Queue<Vector3> velocityHistory = new Queue<Vector3>();
    private int historyLength = 5; // X프레임 평균 사용

    private static int grabbingHandCount = 0;

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
        if (isGrabbing) ReleaseGrab();
    }

    void Update()
    {
        bool isTouching = Physics.CheckSphere(transform.position, grabRadius, grabLayer);

        if (isPressed && isTouching && !isGrabbing) StartGrab();
        else if (!isPressed && isGrabbing) EndGrab();
    }

    void FixedUpdate()
    {
        if (isGrabbing)
        {
            // 손 이동량 계산
            Vector3 handDelta = previousHandPos - transform.position;

            // 속도 계산
            Vector3 targetVelocity = handDelta / Time.fixedDeltaTime * sensitivity;

            // 바닥 체크 (발밑 0.1m 레이캐스트)
            // LayerMask는 스크립트의 grabLayer를 재활용 or 새로 구축
            bool isFeetOnGround = Physics.Raycast(playerRigidbody.position + Vector3.up * 0.1f, Vector3.down, 0.2f, grabLayer);

            if (isFeetOnGround && targetVelocity.y < 0)
            {
                // 아래로 가는 속도만 0으로 만듦 (앞뒤좌우는 허용)
                targetVelocity.y = 0;
            }

            // 속도 적용
            playerRigidbody.linearVelocity = targetVelocity;
        }

        previousHandPos = transform.position;
    }

    void ApplyClimbingMovement()
    {
        // 손 이동량 계산
        Vector3 handDelta = previousHandPos - transform.position;

        // Y축 제한 (필요시)
        if (!allowVerticalMovement) handDelta.y = 0;

        // 목표 속도 계산
        Vector3 targetVelocity = (handDelta / Time.fixedDeltaTime) * sensitivity;

        // 최대 속도 제한 (튀는 거 방지)
        if (targetVelocity.magnitude > maxVelocity)
        {
            targetVelocity = targetVelocity.normalized * maxVelocity;
        }

        // Smoothness 값으로 조절 가능, Lerp없을시 튐, 수치 낮을시 답답함

        Vector3 smoothedVelocity = Vector3.Lerp(playerRigidbody.linearVelocity, targetVelocity, movementSmoothness);

        // Unity 버전에 따른 속도 적용

        playerRigidbody.linearVelocity = smoothedVelocity;


        //던지기 방향 안정화를 위해 속도 기록
        RecordVelocity(targetVelocity);
    }

    // "이상한 방향"으로 튀는 걸 막기 위해 평균 속도를 기록
    void RecordVelocity(Vector3 v)
    {
        if (velocityHistory.Count >= historyLength)
            velocityHistory.Dequeue();
        velocityHistory.Enqueue(v);
    }

    // 기록된 속도들의 평균을 구함 (손떨림 보정)
    Vector3 GetAverageVelocity()
    {
        if (velocityHistory.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        foreach (Vector3 v in velocityHistory) sum += v;
        return sum / velocityHistory.Count;
    }

    void StartGrab()
    {
        isGrabbing = true;
        previousHandPos = transform.position;
        grabbingHandCount++;
        velocityHistory.Clear(); // 기록 초기화

        if (playerRigidbody != null)
        {
            playerRigidbody.useGravity = false;
            // 잡는 순간 속도 줄이는 코드 삭제 -> 흐름 끊김 방지
        }
    }

    void EndGrab()
    {
        ReleaseGrab();
    }

    void ReleaseGrab()
    {
        if (!isGrabbing) return;

        isGrabbing = false;
        grabbingHandCount--;
        if (grabbingHandCount < 0) grabbingHandCount = 0;

        if (grabbingHandCount == 0 && playerRigidbody != null)
        {
            playerRigidbody.useGravity = true;

            Vector3 throwVelocity = GetAverageVelocity() * throwMultiplier;  //평균속도로 던짐

            playerRigidbody.linearVelocity = throwVelocity;

        }
    }
    //디버그
    private void OnDrawGizmos()
    {
        Gizmos.color = isGrabbing ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}