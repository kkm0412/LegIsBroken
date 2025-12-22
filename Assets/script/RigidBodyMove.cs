using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class RigidBodyMove : MonoBehaviour
{
    // 이동 속도
    [SerializeField] private float moveSpeed = 5f;

    // 물리 컴포넌트
    private Rigidbody rb;
    
    private InputAction moveAction;
    
    private Vector2 inputVector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");

        moveAction.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void Update()
    {
        inputVector = moveAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        // 입력값(x, y)을 3차원 이동 벡터(x, 0, z)로 변환
        Vector3 direction = new Vector3(inputVector.x, 0f, inputVector.y);
        
        if(direction.sqrMagnitude > 1) direction.Normalize();

        Vector3 targetVelocity = direction * moveSpeed;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }
}