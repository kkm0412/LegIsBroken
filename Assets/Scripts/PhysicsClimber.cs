using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactors; // XRIT 버전에 맞게 수정
using System.Collections.Generic;

// LocomotionProvider를 상속받아 XR 시스템과 연동
public class PhysicsClimber : UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider
{
    [Header("Hands")]
    [Tooltip("왼손 Direct Interactor (물리 잡기용)")]
    public XRBaseInteractor leftHandInteractor;
    [Tooltip("오른손 Direct Interactor")]
    public XRBaseInteractor rightHandInteractor;

    [Header("Physics Settings")]
    public Rigidbody playerRigidbody;
    [Tooltip("던지기 힘 배율 (반동)")]
    public float throwMultiplier = 1.5f;
    
    // 내부 변수
    private bool isClimbing = false;
    private XRBaseInteractor climbingHand; // 현재 매달려 있는 손
    private ClimbAble currentClimbAble;    // 현재 잡고 있는 돌
    private Vector3 previousHandPos;
    
    // 던지기 평균값 계산용
    private Queue<Vector3> velocityHistory = new Queue<Vector3>();
    private int historyLength = 5;

    protected override void Awake()
    {
        base.Awake();
        if (playerRigidbody == null) 
            playerRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 1. 현재 등반 중이 아니라면 -> 잡았는지 체크
        if (!isClimbing)
        {
            CheckForGrab(leftHandInteractor);
            CheckForGrab(rightHandInteractor);
        }
        // 2. 등반 중이라면 -> 놓았는지 or 계속 잡고 있는지 체크
        else
        {
            ProcessClimbing();
        }
    }

    private void CheckForGrab(XRBaseInteractor hand)
    {
        // 손이 무언가를 잡고 있는가? (hasSelection)
        if (hand != null && hand.hasSelection)
        {
            // 잡은 물체가 무엇인가?
            var interactable = hand.interactablesSelected[0] as MonoBehaviour;
            if (interactable == null) return;

            // 그 물체에 ClimbAble 컴포넌트가 있는가?
            ClimbAble climbAble = interactable.GetComponent<ClimbAble>();
            
            if (climbAble != null)
            {
                StartClimbing(hand, climbAble);
            }
        }
    }

    private void StartClimbing(XRBaseInteractor hand, ClimbAble objectToClimb)
    {
        // 시스템에 이동 권한 요청
        if (!BeginLocomotion()) return;

        isClimbing = true;
        climbingHand = hand;
        currentClimbAble = objectToClimb;

        // 돌에게 "나 너 잡았어"라고 알림 (부서지는 타이머 시작)
        currentClimbAble.OnGrabStart();

        // 초기 위치 저장
        previousHandPos = climbingHand.transform.position;
        velocityHistory.Clear();

        // 중력 끄기
        playerRigidbody.useGravity = false;
        playerRigidbody.linearVelocity = Vector3.zero;
    }

    private void ProcessClimbing()
    {
        // 1. 손을 놓았거나, 돌이 부서져서 사라졌는지 체크
        if (!climbingHand.hasSelection || currentClimbAble == null || !currentClimbAble.gameObject.activeInHierarchy)
        {
            StopClimbing();
            return;
        }
        
        // 2. 다른 손으로 갈아타기 로직 (옵션: 몽키바 처럼)
        // (여기선 단순하게 현재 손이 놓아질 때까지 유지)

        // 3. 물리 이동 계산 (FixedUpdate에서 하는 게 정석이지만 반응성을 위해 Update도 가능, 여기선 Force 사용을 위해 물리 틱 고려)
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            ApplyClimbingPhysics();
        }
    }

    private void ApplyClimbingPhysics()
    {
        // 손의 이동량 (이전 프레임 위치 - 현재 위치)
        // *원리: 내가 손을 아래로 내리면(-Y), 몸은 위로(+Y) 가야 함 -> 반대 방향
        Vector3 handDelta = previousHandPos - climbingHand.transform.position;
        
        // 속도로 변환 (거리 / 시간)
        Vector3 targetVelocity = handDelta / Time.fixedDeltaTime;

        // 몸에 적용 (1:1 반응)
        playerRigidbody.linearVelocity = targetVelocity;

        // 던지기 계산을 위해 기록
        RecordVelocity(targetVelocity);

        // 위치 갱신
        previousHandPos = climbingHand.transform.position;
    }

    private void StopClimbing()
    {
        isClimbing = false;
        
        // 시스템에 이동 종료 알림
        EndLocomotion();

        // 돌에게 "나 너 놓았어" 알림
        if (currentClimbAble != null)
        {
            currentClimbAble.OnGrabEnd();
            currentClimbAble = null;
        }

        climbingHand = null;

        // 중력 복구
        playerRigidbody.useGravity = true;

        // 🔥 반동(던지기) 적용
        // 기록된 평균 속도만큼 플레이어를 날려버림
        Vector3 throwForce = GetAverageVelocity() * throwMultiplier;
        playerRigidbody.linearVelocity = throwForce;
    }

    // --- 유틸리티 (평균 속도 계산) ---
    void RecordVelocity(Vector3 v)
    {
        if (velocityHistory.Count >= historyLength) velocityHistory.Dequeue();
        velocityHistory.Enqueue(v);
    }

    Vector3 GetAverageVelocity()
    {
        if (velocityHistory.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        foreach (Vector3 v in velocityHistory) sum += v;
        return sum / velocityHistory.Count;
    }
}