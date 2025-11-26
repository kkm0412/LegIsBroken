using System.Collections;
using UnityEditor.EditorTools;
using UnityEngine;

public class FakeClimbAble : MonoBehaviour
{
    
    [Tooltip("플레이어 손 정보 넣어놓기")]
    [SerializeField] private GameObject hand;

    [Tooltip("부서지는 소리")]
    [SerializeField] private AudioClip breakSound;

    private bool isGrabbed = false; //손 반응(안 쓸수도 있음)

    private void Awake()
    {
        isGrabbed = false;
    }

    //Issue: 충돌로 구현했는데 잡았을 때로 교체할수도 있음!
    private void OnTriggerEnter(Collider other)
    {
        if (other == hand)
        {
            BlockBreaks();
        }
    }

    private IEnumerator BlockBreaks()
    {
        //부서지는 소리 재생
        //2초 기다리고

        //소리 재생
        yield return new WaitForSeconds(2);
        Destroy(gameObject);

    }
    //이 블록을 잡았을 때
    //3초후 블록이 사라지고
    //다시 재생성 되어야함
    //
}