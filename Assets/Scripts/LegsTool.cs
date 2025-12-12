using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class LegsTool : MonoBehaviour
{
    [Header("Settings")]
    public Transform tipTransform;
    public LayerMask climbableLayer;
    public float triggerThreshold = 0.5f;

    [Header("Physics")]
    public float throwForceMultiplier = 1.5f; 
    public Rigidbody playerRigidbody;

    [Header("Inputs (반드시 둘 다 할당하세요!)")]
    [Tooltip("XRI LeftHand Interaction/Activate Value (또는 Activate)")]
    public InputActionProperty leftHandActivate;
    
    [Tooltip("XRI RightHand Interaction/Activate Value (또는 Activate)")]
    public InputActionProperty rightHandActivate;

    // 내부 변수
    private Rigidbody rb;
    private XRGrabInteractable interactable;
    private bool isStuck = false;
    private Vector3 previousHandPos;
    private XRBaseInteractor currentInteractor;
    
    // 현재 잡은 손이 왼손인지 오른손인지 기억하는 변수
    private bool isLeftHand = false; 

    public Transform holsterPoint;  //안 잡을 시 돌아가는 위치

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interactable = GetComponent<XRGrabInteractable>();

        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnRelease);
    }

    private void Start()
    {
        if (playerRigidbody == null)
        {
            var origin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (origin != null) playerRigidbody = origin.GetComponent<Rigidbody>();
        }
        SetupIgnoreCollision();
    }

    private void SetupIgnoreCollision()
    {
        Collider[] myColliders = GetComponentsInChildren<Collider>();
        if (playerRigidbody != null)
        {
            Collider[] playerColliders = playerRigidbody.GetComponentsInChildren<Collider>();
            foreach (Collider toolCol in myColliders)
            {
                foreach (Collider playerCol in playerColliders)
                    Physics.IgnoreCollision(toolCol, playerCol, true);
            }
        }
    }

    void Update()
    {
        // 1. 잡고 있을 때 (등반 로직 체크)
        if (interactable.isSelected) 
        {
            // 붙어있는데 트리거를 놓으면 -> 떨어짐
            float triggerValue = GetCurrentTriggerValue();
            if (triggerValue < triggerThreshold && isStuck)
            {
                Unstick();
            }
            return; // 잡고 있으면 아래 홀스터 로직 실행 안 함
        }

        // 2. [추가됨] 안 잡고 있고 + 벽에 안 박혀 있으면 -> 홀스터로 복귀
        if (!isStuck) 
        {
            MoveToHolster();
        }
    }

    void FixedUpdate()
    {
        if (isStuck && interactable.isSelected && currentInteractor != null)
        {
            ApplyClimbingForce();
        }
    }

    private void MoveToHolster()
    {
        if (holsterPoint == null) return;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;

        transform.position = holsterPoint.position;
        transform.rotation = holsterPoint.rotation;
    }
    public void AttemptStick(Collider other)
    {
        if (isStuck || !interactable.isSelected) return;

        float triggerValue = GetCurrentTriggerValue();
        if (triggerValue < triggerThreshold) return;

        if (((1 << other.gameObject.layer) & climbableLayer) != 0)
        {
            StickToWall();
        }
    }

    // [핵심] 왼손이면 왼손 액션, 오른손이면 오른손 액션 읽기
    private float GetCurrentTriggerValue()
    {
        if (isLeftHand)
            return leftHandActivate.action != null ? leftHandActivate.action.ReadValue<float>() : 0f;
        else
            return rightHandActivate.action != null ? rightHandActivate.action.ReadValue<float>() : 0f;
    }

    private void StickToWall()
    {
        isStuck = true;
        rb.isKinematic = true; 
        rb.linearVelocity = Vector3.zero;

        if (currentInteractor != null)
            previousHandPos = currentInteractor.transform.position;
            
        SendHapticImpulse(1.0f, 0.2f);
        if (playerRigidbody != null) playerRigidbody.useGravity = false;
        
        Debug.Log("⛏️ 벽에 박힘! (Stuck)");
    }

    private void Unstick()
    {
        if (!isStuck) return;

        isStuck = false;
        rb.isKinematic = false;

        if (playerRigidbody != null) 
        {
            playerRigidbody.useGravity = true;
            
            // 던지기
            Vector3 handDelta = currentInteractor.transform.position - previousHandPos;
            Vector3 throwVelocity = handDelta / Time.fixedDeltaTime;
            
            // 몸을 던지는 방향으로 밀기
            playerRigidbody.linearVelocity = -throwVelocity * throwForceMultiplier; 
        }
        Debug.Log("⬇️ 벽에서 떨어짐");
    }

    private void ApplyClimbingForce()
    {
        Transform currentHandTransform = currentInteractor.transform;
        
        Vector3 handDelta = currentHandTransform.position - previousHandPos;
        Vector3 targetMove = -handDelta; 
        Vector3 targetVelocity = targetMove / Time.fixedDeltaTime;

        playerRigidbody.linearVelocity = targetVelocity;

        previousHandPos = currentHandTransform.position;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject as XRBaseInteractor;
        
        // 잡은 손이 왼손인지 오른손인지 판별
        if (currentInteractor != null)
        {
            // 1. NearFarInteractor라면 내장된 Handedness 속성 확인 (가장 정확)
            if (currentInteractor is NearFarInteractor nearFar)
            {
                isLeftHand = (nearFar.handedness == InteractorHandedness.Left);
            }
            // 2. 아니라면 이름이나 태그로 추측 (보조 수단)
            else 
            {
                if (currentInteractor.name.Contains("Left") || currentInteractor.CompareTag("LeftHand"))
                    isLeftHand = true;
                else
                    isLeftHand = false;
            }
            
            Debug.Log($"잡은 손: {(isLeftHand ? "왼손" : "오른손")}");
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        Unstick();
        currentInteractor = null;
    }

    private void SendHapticImpulse(float amplitude, float duration)
    {
        if (currentInteractor is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(amplitude, duration);
        }
    }
}