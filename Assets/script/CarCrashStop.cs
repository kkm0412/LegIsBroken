using UnityEngine;

public class CarCrashStop : MonoBehaviour
{
    public CarControl autoDrive;    // �ڵ��� ��Ʈ�ѷ�
    public Rigidbody carRb;          // ���� Rigidbody
    public PlayerMovement player;    // �÷��̾� ��ũ��Ʈ

    private bool crashed = false;

    void Start()
    {
        // Inspector�� ���� �� �Ǿ� ������ �ڵ����� ã�ƺ���
        if (autoDrive == null)
            autoDrive = GetComponent<CarControl>() ?? FindObjectOfType<CarControl>();

        if (carRb == null)
            carRb = GetComponent<Rigidbody>();

        if (player == null)
            player = FindObjectOfType<PlayerMovement>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (crashed) return;

        if (col.collider.CompareTag("Wall"))
        {
            crashed = true;

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
            if (player != null)
            {
                Vector3 forwardForce = transform.forward * 15f + Vector3.up * 5f;
                player.Fall(forwardForce);

            }
        }
    }
}
