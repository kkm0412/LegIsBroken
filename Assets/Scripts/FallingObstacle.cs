using UnityEngine;

public class FallingObstacle : MonoBehaviour
{
    //장애물의 오브젝트 불러옴.
    // 만약 장애물이 바닥이나 플레이어블에 닿으면 부서짐.
    //플레이어와 부딪힐시 플레이어에게 부정적 효과 적용
    //ㄴ플레이어블에게 이벤트 시스템이 필요함
    [Tooltip("플레이어 정보 넣어놓기")]
    [SerializeField] private GameObject player;
    [Tooltip("바닥 정보 넣어놓기")]
    [SerializeField] private GameObject ground;



    private void OnTriggerEnter(Collider other)
    {
        if (other == player)
        {
            //TODO 플레이어에게 효과 부여
            Destroy(gameObject);
        }
        else if(other == ground)
        {
            Destroy(gameObject);
        }
    }
    
}