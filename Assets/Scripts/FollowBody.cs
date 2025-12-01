using UnityEngine;

public class FollowBody : MonoBehaviour
{
    //추적할 자식(몸)개체
    [SerializeField] private Transform bodyTarget;
    //자식의 몸을 따라가는 코드이므로 조심

    void LateUpdate()
    {
        if(bodyTarget == null) return;  //디버깅용
        Vector3 targetWorldPos = bodyTarget.position;
        Quaternion targetWorldRot = bodyTarget.rotation;

        transform.position = targetWorldPos;
        //회전 필요시
        // transform.rotation = targetWorldRot;

        bodyTarget.localPosition = Vector3.zero;
        //bodyTarget.localRotation = Quaternion.identity;   //회전도 필요시에만

    }
}