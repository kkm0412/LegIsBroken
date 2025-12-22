using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class FakeClimbAble : MonoBehaviour
{
    private Rigidbody rb;
    private XRGrabInteractable interactable;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interactable = GetComponent<XRGrabInteractable>();

        //물리엔진 끄기
        rb.isKinematic = true; 
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnGrab);
        interactable.selectExited.RemoveListener(OnRelease);
    }

    // 잡는 순간
    private void OnGrab(SelectEnterEventArgs args)
    {
        rb.isKinematic = false;

        // XRGrabInteractable이 자동으로 처리해주지만 확실하게 하기 위함
    }

    // 놓는 순간 
    private void OnRelease(SelectExitEventArgs args)
    {
        rb.isKinematic = false; 
        rb.useGravity = true;
    }
}