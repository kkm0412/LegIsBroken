using UnityEngine;
using UnityEngine.Events;

public class ClimbAble : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("잡으면 부서지는가?")]
    public bool isBreakable = false;
    
    [Tooltip("잡고 나서 부서질 때까지 걸리는 시간 (초)")]
    public float timeToBreak = 2.0f;

    [Header("Events")]
    public UnityEvent onBreak; // 부서질 때 파티클 재생 위해서

    // 내부 상태
    private bool isBeingHeld = false;
    private float currentHoldTime = 0f;
    private Collider myCollider;
    private Renderer myRenderer;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        //타이머 작동
        if (isBeingHeld && isBreakable)
        {
            currentHoldTime += Time.deltaTime;
            
            // 흔들림 효과
            if (myRenderer != null)
            {
                float shake = (currentHoldTime / timeToBreak) * 0.05f;
                myRenderer.transform.localPosition += Random.insideUnitSphere * shake;
            }

            if (currentHoldTime >= timeToBreak)
            {
                BreakObject();
            }
        }
    }
    // 잡기 시작할 때
    public void OnGrabStart()
    {
        isBeingHeld = true;
        currentHoldTime = 0f;
    }

    // 잡기 끝날 때
    public void OnGrabEnd()
    {
        isBeingHeld = false;
        currentHoldTime = 0f; 
        
    }

    // 부서지는 처리
    private void BreakObject()
    {
        isBeingHeld = false; 
        onBreak.Invoke();

        if (myCollider != null) myCollider.enabled = false;
        if (myRenderer != null) myRenderer.enabled = false;

        Destroy(gameObject, 1f); 
        
    }
}