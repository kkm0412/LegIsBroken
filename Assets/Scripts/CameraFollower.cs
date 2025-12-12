using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [Header("Targets")]
    public Transform headCamera; // Main Camera
    public CapsuleCollider bodyCollider; // XR Origin에 있는 캡슐 콜라이더
    public Transform visualBody; // (선택) 눈에 보이는 고릴라 몸통 모델

    [Header("Settings")]
    public float bodyHeightOffset = 0.0f; // 머리 대비 몸통 높이 조절
    public float turnSmoothness = 5.0f; // 몸통 회전 부드러움

    void FixedUpdate()
    {
        UpdateCollider();
        UpdateVisualBody();
    }

    // 1. 물리 콜라이더는 머리 위치를 "즉시" 따라가야 함 (충돌 오차 방지)
    void UpdateCollider()
    {
        if (headCamera == null || bodyCollider == null) return;

        // XR Origin 기준 로컬 좌표로 변환
        Vector3 headLocalPos = transform.InverseTransformPoint(headCamera.position);

        // 캡슐 콜라이더의 중심(Center)을 머리의 X, Z 좌표로 이동
        // Y축(높이)은 머리 높이를 따라갈지, 고정할지 선택 가능 (고릴라 태그는 머리 따라감)
        bodyCollider.center = new Vector3(headLocalPos.x, bodyCollider.center.y, headLocalPos.z);
    }

    // 2. 눈에 보이는 몸통(비주얼)은 회전 제약이 필요함
    void UpdateVisualBody()
    {
        if (visualBody == null) return;

        // 위치: 머리 바로 아래 (살짝 오프셋)
        Vector3 targetPosition = headCamera.position;
        targetPosition.y += bodyHeightOffset; 
        visualBody.position = targetPosition;

        // 회전: 고개를 숙여도 몸은 기울어지지 않게 (Y축 회전만 반영)
        Vector3 targetEuler = headCamera.eulerAngles;
        // X, Z 회전은 0으로 고정 (오직 좌우 회전만 따라감)
        Quaternion targetRotation = Quaternion.Euler(0, targetEuler.y, 0);

        // 부드럽게 회전 (Slerp)
        visualBody.rotation = Quaternion.Slerp(visualBody.rotation, targetRotation, Time.fixedDeltaTime * turnSmoothness);
    }
}