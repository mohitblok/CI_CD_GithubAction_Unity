using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCharacterController : MonoSingleton<SimpleCharacterController>
{
    [Tooltip("Maximum slope the character can jump on")]
    [Range(5f, 60f)]
    public float slopeLimit = 45f;
    [Tooltip("Move speed in meters/second")]
    public float moveSpeed = 5f;
    [Tooltip("Turn speed in degrees/second, left (+) or right (-)")]
    public float turnSpeed = 300;
    [Tooltip("Whether the character can jump")]
    public bool allowJump = false;
    [Tooltip("Upward speed to apply when jumping in meters/second")]
    public float jumpSpeed = 4f;


    public bool IsGrounded { get; private set; }
    public float ForwardInput { get; set; }
    public float TurnInput { get; set; }
    public bool JumpInput { get; set; }


    new private Rigidbody rigidbody;
    private CapsuleCollider capsuleCollider;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ProcessActions();
    }

    /// <summary>
    /// Checks whether the character is on the ground and updates <see cref="IsGrounded"/>
    /// </summary>
    private void CheckGrounded()
    {
        IsGrounded = false;

        //raycast to check for collision under user (ground)
        if (Physics.Raycast(transform.position + Vector3.up *1.5f, Vector3.down,out var hit, 1.7f))
        {
            
            rigidbody.position = hit.point +Vector3.up * 0.1f;
            IsGrounded = true;
        }

        //if there is no flow dont let them fall forever
        if (!IsGrounded)
        {
            if (!Physics.Raycast(transform.position + Vector3.up * 0.25f, Vector3.down, 100f))
            {

                IsGrounded = true;
            }
        }
    }

    /// <summary>
    /// Processes input actions and converts them into movement
    /// </summary>
    private void ProcessActions()
    {
        // Process Turning
        if (TurnInput != 0f)
        {
            float angle = Mathf.Clamp(TurnInput, -1f, 1f) * turnSpeed;
            rigidbody.rotation *= Quaternion.Euler(0f, angle * Time.fixedDeltaTime, 0f);
        }
        
        rigidbody.position += transform.forward * Mathf.Clamp(ForwardInput, -1f, 1f) * moveSpeed * Time.fixedDeltaTime;

        if (!IsGrounded)
        {
            rigidbody.position += Physics.gravity * Time.fixedDeltaTime;
        }
    }
}
