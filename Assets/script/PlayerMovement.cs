using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState { Idle, Running, Jumping, Falling, Landing }
    public PlayerState state;

    public Rigidbody rb;
    public Transform groundChecker;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    private bool isGrounded;

    void Update()
    {
        GroundCheck();
        StateMachine();
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, groundDistance, groundMask);
    }

    /// <summary>
    /// 차량 충돌 시 호출: 플레이어를 앞으로 튕겨보내고 중력 적용
    /// </summary>
    /// <param name="collisionForce">충돌로 적용할 힘</param>
    public void Fall(Vector3 collisionForce)
    {
        transform.SetParent(null);       // 차량에서 분리
        rb.isKinematic = false;          // 물리 활성화
        rb.useGravity = true;            // 중력 켜기

        rb.linearVelocity = Vector3.zero;      // 기존 속도 초기화
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(collisionForce, ForceMode.Impulse); // 충돌 힘 적용

        state = PlayerState.Falling;
    }

    void StateMachine()
    {
        // 낙하 중이면 Falling 상태 유지
        if (!isGrounded && rb.linearVelocity.y < -0.1f)
        {
            state = PlayerState.Falling;
            return;
        }

        // Falling -> Landing 전환
        if (state == PlayerState.Falling && isGrounded)
        {
            state = PlayerState.Landing;
            StartCoroutine(LandingSequence());
            return;
        }
    }

    IEnumerator LandingSequence()
    {
        yield return new WaitForSeconds(0.3f);
        state = PlayerState.Idle;
    }
}
