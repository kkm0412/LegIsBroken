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
    public UnityEvent onBreak; // 부서질 때 파티클 재생 등을 위해

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
        // 잡혀있고 + 부서지는 물체라면 타이머 작동
        if (isBeingHeld && isBreakable)
        {
            currentHoldTime += Time.deltaTime;
            
            // 흔들리는 연출 (옵션)
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

    // 플레이어가 잡았을 때 호출
    public void OnGrabStart()
    {
        isBeingHeld = true;
        currentHoldTime = 0f;
    }

    // 플레이어가 놓았을 때 호출
    public void OnGrabEnd()
    {
        isBeingHeld = false;
        // 놓으면 타이머 초기화 (취향에 따라 유지 가능)
        currentHoldTime = 0f; 
        
        // 흔들림 복구 등 추가 로직 가능
    }

    private void BreakObject()
    {
        isBeingHeld = false;
        
        // 1. 이벤트 발생 (소리, 파티클)
        onBreak.Invoke();

        // 2. 더 이상 못 잡게 콜라이더 끄기 or 오브젝트 파괴
        if (myCollider != null) myCollider.enabled = false;
        if (myRenderer != null) myRenderer.enabled = false;
        
        // 3. 잠시 후 삭제 or 비활성화
        Destroy(gameObject, 1f); 
        
        // *중요*: 플레이어 손에서 강제로 놓게 하는 건 Climber 스크립트에서 처리됨 (콜라이더가 꺼지면 놓아짐)
    }
}