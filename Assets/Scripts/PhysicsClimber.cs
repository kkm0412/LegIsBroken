// using UnityEngine;

// using UnityEngine.XR.Interaction.Toolkit.Interactors; // XRIT 버전에 맞게 수정
// using System.Collections.Generic;

// public class PhysicsClimber : UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider
// {
//     [Header("Hands")]
//     [Tooltip("왼손 Direct Interactor")]
//     public XRBaseInteractor leftHandInteractor;
//     [Tooltip("오른손 Direct Interactor")]
//     public XRBaseInteractor rightHandInteractor;

//     [Header("Physics Settings")]
//     public Rigidbody playerRigidbody;
//     public float throwMultiplier = 1.5f;
    
//     // 내부 변수
//     private bool isClimbing = false;
//     private XRBaseInteractor climbingHand; // 현재 매달려 있는 손
//     private ClimbAble currentClimbAble;    // 현재 잡고 있는 돌
//     private Vector3 previousHandPos;
    
//     // 던지기 평균값 계산용
//     private Queue<Vector3> velocityHistory = new Queue<Vector3>();
//     private int historyLength = 5;

//     protected override void Awake()
//     {
//         base.Awake();
//         if (playerRigidbody == null) 
//             playerRigidbody = GetComponent<Rigidbody>();
//     }

//     private void Update()
//     {
//         if (!isClimbing)
//         {
//             CheckForGrab(leftHandInteractor);
//             CheckForGrab(rightHandInteractor);
//         }
//         else
//         {
//             ProcessClimbing();
//         }
//     }

//     private void CheckForGrab(XRBaseInteractor hand)
//     {
//         if (hand != null && hand.hasSelection)
//         {
//             var interactable = hand.interactablesSelected[0] as MonoBehaviour;
//             if (interactable == null) return;

//             ClimbAble climbAble = interactable.GetComponent<ClimbAble>();
            
//             if (climbAble != null)
//             {
//                 StartClimbing(hand, climbAble);
//             }
//         }
//     }

//     private void StartClimbing(XRBaseInteractor hand, ClimbAble objectToClimb)
//     {
//         if (!BeginLocomotion()) return;

//         isClimbing = true;
//         climbingHand = hand;
//         currentClimbAble = objectToClimb;

//         currentClimbAble.OnGrabStart();

//         previousHandPos = climbingHand.transform.position;
//         velocityHistory.Clear();

//         playerRigidbody.useGravity = false;
//         playerRigidbody.linearVelocity = Vector3.zero;
//     }

//     private void ProcessClimbing()
//     {
//         if (!climbingHand.hasSelection || currentClimbAble == null || !currentClimbAble.gameObject.activeInHierarchy)
//         {
//             StopClimbing();
//             return;
//         }
        
//     }

//     private void FixedUpdate()
//     {
//         if (isClimbing)
//         {
//             ApplyClimbingPhysics();
//         }
//     }

//     private void ApplyClimbingPhysics()
//     {
//         Vector3 handDelta = previousHandPos - climbingHand.transform.position;
//         Vector3 targetVelocity = handDelta / Time.fixedDeltaTime;
//         playerRigidbody.linearVelocity = targetVelocity;

//         RecordVelocity(targetVelocity);

//         previousHandPos = climbingHand.transform.position;
//     }

//     private void StopClimbing()
//     {
//         isClimbing = false;
        
//         EndLocomotion();

//         if (currentClimbAble != null)
//         {
//             currentClimbAble.OnGrabEnd();
//             currentClimbAble = null;
//         }

//         climbingHand = null;

//         playerRigidbody.useGravity = true;

//         Vector3 throwForce = GetAverageVelocity() * throwMultiplier;
//         playerRigidbody.linearVelocity = throwForce;
//     }

//     void RecordVelocity(Vector3 v)
//     {
//         if (velocityHistory.Count >= historyLength) velocityHistory.Dequeue();
//         velocityHistory.Enqueue(v);
//     }

//     Vector3 GetAverageVelocity()
//     {
//         if (velocityHistory.Count == 0) return Vector3.zero;
//         Vector3 sum = Vector3.zero;
//         foreach (Vector3 v in velocityHistory) sum += v;
//         return sum / velocityHistory.Count;
//     }
// }