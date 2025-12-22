using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HandMoveProvider : MonoBehaviour
{
    [SerializeField] private InputActionReference grabAction;
    [SerializeField] private Rigidbody playerRigidbody;

    [Tooltip("물리 손, 없으면 현재 트랜스폼 위치를 사용")]
    public Transform handTrackingTransform;

    [Tooltip("Near-far Interactor 연결")]
    public XRBaseInteractor handInteractor;

    [Header("Physics Options")]
    [Tooltip("이동 감도 (1.0 = 정직함, 1.5 = 빠름)")]
    [Range(0.5f, 3f)]
    public float sensitivity = 1.3f;
    [Tooltip("움직임 부드러움 정도")]
    [Range(0.1f, 1f)]
    public float movementSmoothness = 0.6f;

    [Tooltip("던지기 힘 배율")]
    public float throwMultiplier = 1.2f;

    [Header("limits")]
    public bool allowVerticalMovement = true;
    public float maxVelocity = 20f; // 속도 제한

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
    public int historyLength = 5; // X프레임 평균 사용

    private static int grabbingHandCount = 0;

    void Awake()
    {
        // 안 넣어놨을 때
        if (handTrackingTransform == null)
            currentHand = transform;
        else
            currentHand = handTrackingTransform;
    }

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
        //손에 물건 쥐고있을 때 바닥 이동 X
        if(handInteractor != null && handInteractor.hasSelection)
        {
            if (isGrabbing) EndGrab();
            return;
        }

        bool isTouching = Physics.CheckSphere(transform.position, grabRadius, grabLayer);

        if (isPressed && isTouching && !isGrabbing) StartGrab();
        else if (!isPressed && isGrabbing) EndGrab();
    }

    void FixedUpdate()
    {
        if (isGrabbing)
        {
            ApplyClimbingLogic();
        }

        previousHandPos = transform.position;
    }

    private void ApplyClimbingLogic()
    {
        // 손 이동량
        Vector3 handDelta = previousHandPos - currentHand.position;
        
        if (!allowVerticalMovement) handDelta.y = 0;

        // 속도
        Vector3 targetVelocity = (handDelta / Time.fixedDeltaTime) * sensitivity;

        //
        //최대 속도 제한
        if (targetVelocity.magnitude > maxVelocity)
        {
            targetVelocity = targetVelocity.normalized * maxVelocity;
        }

        bool isFeetOnGround = Physics.Raycast(playerRigidbody.position + Vector3.up * 0.1f, Vector3.down, 0.2f, grabLayer);
        if (isFeetOnGround && targetVelocity.y < 0)
        {
            targetVelocity.y = 0;
        }
        Vector3 smoothedVelocity = Vector3.Lerp(playerRigidbody.linearVelocity, targetVelocity, movementSmoothness);
        playerRigidbody.linearVelocity = smoothedVelocity;
        RecordVelocity(targetVelocity);
    }

    void RecordVelocity(Vector3 v)  //평균 속도 기록용
    {
        if (velocityHistory.Count >= historyLength)
            velocityHistory.Dequeue();
        velocityHistory.Enqueue(v);
    }

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
        previousHandPos = currentHand.position;

        grabbingHandCount++;
        velocityHistory.Clear(); // 기록 초기화

        if (playerRigidbody != null)
        {
            playerRigidbody.useGravity = false;
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