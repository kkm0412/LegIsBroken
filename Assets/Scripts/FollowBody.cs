using UnityEngine;

public class FollowBody : MonoBehaviour
{
    [SerializeField] private Transform bodyTarget; //추적할 자식(몸)개체
    void LateUpdate()
    {
        if(bodyTarget == null) return;  //디버깅용
        
        Vector3 targetWorldPos = bodyTarget.position;
        Quaternion targetWorldRot = bodyTarget.rotation;

        transform.position = targetWorldPos;
        // transform.rotation = targetWorldRot; 

        bodyTarget.localPosition = Vector3.zero;
        //bodyTarget.localRotation = Quaternion.identity;  

    }
}