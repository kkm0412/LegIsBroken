using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("주행 설정")]
    public float moveSpeed = 5f;
    public float turnSpeed = 5f;
    public float turnSmoothness = 5f;

    [Header("자연스러운 흔들림 옵션")]
    public float steeringNoiseAmount = 1.5f;
    public float steeringNoiseSpeed = 0.7f;
    public float speedNoiseAmount = 0.2f;

    [Header("자동 운전 옵션 (AutoDrive)")]
    public bool autoDrive = false;
    public Transform driveTarget;
    public float autoTurnSensitivity = 2f;

    private float currentTurnAngle = 0f;

    void Update()
    {
        float horizontal = 0f;
        float speed = moveSpeed;

     
        if (autoDrive && driveTarget != null)
        {
            Vector3 dir = (driveTarget.position - transform.position).normalized;
            Vector3 localDir = transform.InverseTransformDirection(dir);

            horizontal = Mathf.Clamp(localDir.x * autoTurnSensitivity, -1f, 1f);
        }

        float noise = (Mathf.PerlinNoise(Time.time * steeringNoiseSpeed, 0) - 0.5f) * 2f;
        noise *= steeringNoiseAmount;

        float targetTurn = (horizontal * turnSpeed) + noise;
        currentTurnAngle = Mathf.Lerp(currentTurnAngle, targetTurn, Time.deltaTime * turnSmoothness);

        transform.Rotate(0, currentTurnAngle * Time.deltaTime, 0);

        float speedNoise = (Mathf.PerlinNoise(Time.time * 1.2f, 10) - 0.5f) * speedNoiseAmount;

        transform.position += transform.forward * (speed + speedNoise) * Time.deltaTime;
    }
}
