using System.Collections;
using UnityEngine;

public class FakeClimbAble : MonoBehaviour
{
    private bool isGrabbed = false;
    private void Awake()
    {
        isGrabbed = false;
    }


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
        Destroy(gameObject);
    }
    //이 블록을 잡았을 때
    //3초후 블록이 사라지고
    //다시 재생성 되어야함
    //
}