using UnityEngine;
using UnityEngine.InputSystem; // Input System 필수

public class VRRecenter : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("이동시킬 대상 (XR Origin 전체)")]
    public Transform xrOrigin;
    [Tooltip("기준이 될 머리 (Main Camera)")]
    public Transform headCamera;

    [Header("Input")]
    [Tooltip("리셋 버튼 (보통 오른쪽 컨트롤러 A버튼 = Primary Button)")]
    public InputActionProperty resetInput;

    [Header("Settings")]
    [Tooltip("체크하면 바라보는 방향(회전)도 정면(0도)으로 초기화함")]
    public bool resetRotation = true;

    void Update()
    {
        // 버튼을 누른 순간 실행 (WasPressedThisFrame)
        if (resetInput.action != null && resetInput.action.WasPressedThisFrame())
        {
            RecenterPosition();
        }
    }

    public void RecenterPosition()
    {
        headCamera.localPosition = Vector3.zero;
        // // 1. 회전 초기화 (옵션)
        // // 카메라가 바라보는 방향을 XR Origin의 정면(0도)과 일치시킴
        // if (resetRotation)
        // {
        //     // 현재 머리의 Y축 회전값
        //     float currentHeadY = headCamera.rotation.eulerAngles.y;
        //     // 현재 Origin의 Y축 회전값
        //     float originY = xrOrigin.rotation.eulerAngles.y;

        //     // 차이만큼 Origin을 반대로 돌림
        //     float rotationDiff = originY - currentHeadY;
        //     xrOrigin.Rotate(0, rotationDiff, 0);
        // }

        // // 2. 위치 초기화 (핵심)
        // // "카메라가 (0,0,0)에 있으려면 Origin은 어디로 가야 하는가?"
        
        // // 머리와 오리진의 거리 차이 계산 (높이 Y는 제외하고 수평 거리만)
        // Vector3 offset = headCamera.position - xrOrigin.position;
        // offset.y = 0; // 높이는 건드리지 않음 (바닥 꺼짐 방지)

        // // 오리진을 (0,0,0) 위치에서 오프셋만큼 뺀 곳으로 이동
        // // 결론: Origin이 이동하면서 Camera는 (0,0,0) 위치에 오게 됨
        // xrOrigin.position = Vector3.zero - offset;

        // Debug.Log("위치 리셋 완료!");
    }
}