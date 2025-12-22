using UnityEngine;

public class FollowBodyMain : MonoBehaviour
{
    [SerializeField] private Transform bodyTarget; //추적할 자식(몸)개체
    void LateUpdate()
    {
        if(bodyTarget == null) return;  //디버깅용
        
        Vector3 targetWorldPos = bodyTarget.position;
        Quaternion targetWorldRot = bodyTarget.rotation;

        transform.position = targetWorldPos;
        // transform.rotation = targetWorldRot; //회전 필요시

        bodyTarget.localPosition = Vector3.zero;
        //bodyTarget.localRotation = Quaternion.identity;   //회전도 필요시에만

    }
}