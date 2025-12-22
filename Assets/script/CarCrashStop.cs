using UnityEngine;

public class CarCrashStop : MonoBehaviour
{
    public CarControl autoDrive;    // �ڵ��� ��Ʈ�ѷ�
    public Rigidbody carRb;          // ���� Rigidbody
    //public PlayerMovement player;    // �÷��̾� ��ũ��Ʈ
    public GameObject player;
    public Rigidbody playerRB;

    public FixedJoint joint;

    private bool crashed = false;

    void Start()
    {
        // Inspector�� ���� �� �Ǿ� ������ �ڵ����� ã�ƺ���
        // if (autoDrive == null)
        //     autoDrive = GetComponent<CarControl>() ?? FindObjectOfType<CarControl>();

        if (carRb == null)
            carRb = GetComponent<Rigidbody>();

        // if (player == null)
        //     player = FindObjectOfType<PlayerMovement>();
        joint = GetComponent<FixedJoint>();
        
        joint.connectedBody = playerRB;

    }

    void OnCollisionEnter(Collision col)
    {
        if (crashed) return;

        if (col.collider.CompareTag("Wall"))
        {
            crashed = true;
            Destroy(joint);

            // ���� �ӵ� 0
            if (autoDrive != null)
                autoDrive.moveSpeed = 0f;

            // ���� �ڷ� �и��� �ʵ��� ������ ����
            if (carRb != null)
            {
                carRb.linearVelocity = Vector3.zero;
                carRb.angularVelocity = Vector3.zero;
                carRb.isKinematic = true;
            }

            // �÷��̾� ������ Ƣ�������
            if (playerRB != null)
            {
                Vector3 forwardForce = transform.forward * 30f + Vector3.up * 10f;
                Debug.Log("플레이어 튕겨나감: " + forwardForce);
                playerRB.isKinematic = false;
                playerRB.useGravity = true;
                playerRB.AddForce(forwardForce, ForceMode.Impulse);
                //playerRB.Fall(forwardForce);

            }
        }
    }
}
// using System.Collections;
// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;

// public class CarCrashStop : MonoBehaviour
// {
//     [Header("Scripts")]
//     public CarControl carControl; // 위에서 수정한 스크립트 연결

//     [Header("Player Settings")]
//     public Transform playerRoot;     // XR Origin
//     public Rigidbody playerRb;       // XR Origin의 리지드바디
//     public Transform seatPoint;      // 차 내부 좌석 위치 (빈 오브젝트)

//     [Header("Crash Settings")]
//     public Transform carRoot;
//     public float ejectForce = 15f;   // 튕겨나가는 힘
//     public float upForce = 5f;       // 위로 솟는 힘

//     private bool hasCrashed = false;

//     void Start()
//     {
//         carRoot.GetComponent<FixedJoint>().connectedBody = playerRb.GetComponent<Rigidbody>();
//     }
//     void Update()
//     {
        
//     }
//     // ▶ 이 함수를 버튼(Button) 이벤트에 연결하세요!
//     public void OnPressStartButton()
//     {
//         if (carControl.isRunning || hasCrashed) return;

//         Debug.Log("🚗 탑승 및 출발!");

//         // 1. 플레이어 차에 태우기 (자식으로 만들기)
//         //playerRoot.position = seatPoint.position;
//         //playerRoot.rotation = seatPoint.rotation;
//         //layerRoot.SetParent(transform); // 차의 자식이 됨

//         // 2. 플레이어 물리 끄기 (차 안에서 충돌 방지)
//         playerRb.isKinematic = true;

//         // 3. 자동차 주행 시작 (isRunning = true)
//         carControl.isRunning = true;
//     }

//     // 벽에 박았을 때 자동 실행
//     void OnCollisionEnter(Collision collision)
//     {
//         // 이미 멈췄거나, 벽이 아니면 무시
//         if (hasCrashed || !carControl.isRunning) return;

//         if (collision.gameObject.CompareTag("Wall"))
//         {
//             Debug.Log("💥 쾅! 충돌 발생!");
//             hasCrashed = true;
//             carRoot.GetComponent<FixedJoint>().connectedBody = null;

//             // 1. 자동차 정지
//             //carControl.StopCar();

//             // 2. 플레이어 탈출 (부모 관계 끊기)
//             playerRoot.SetParent(null);

//             // 3. 플레이어 물리 켜고 날려버리기
//             if (playerRb != null)
//             {
//                 playerRb.isKinematic = false;
//                 playerRb.useGravity = true; // 중력 켜기

//                 // 관성 + 튕겨나가는 힘 적용
//                 Vector3 finalForce = (transform.forward * ejectForce) + (Vector3.up * upForce);
//                 playerRb.AddForce(finalForce, ForceMode.Impulse);
//             }
//         }
//     }
// }