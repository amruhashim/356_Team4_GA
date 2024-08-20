using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Movement : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField] private AudioClip audioClipLanding;

    [Header("Movement Settings")]
    public float forwardSpeed = 8.0f;
    public float backwardSpeed = 4.0f;
    public float strafeSpeed = 4.0f;
    public float jumpHeight = 10f;
    public float jumpDuration = 1f;
    public float airControlMultiplier = 0.5f; // Air control multiplier
    public AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));

    [Header("Advanced Settings")]
    public float groundCheckDistance = 0.1f;
    public float stickToGroundHelperDistance = 0.5f;
    public bool airControl;
    public float shellOffset = 0.1f; // Reduce radius in sphere cast
    public float frictionDampening = 0.9f; // Friction dampening factor

    [Header("Gravity Settings")]
    public float airDrag = 0.5f; // Drag applied when airborne

    private Rigidbody rigidBody;
    private CapsuleCollider capsule;
    private AudioSource audioSource;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isJumping;
    private float walkAudioTimer = 0.0f;
    private float walkAudioSpeed = 0.4f;
    private int currentClipIndex = 0;
    public bool isMoving;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidBody.useGravity = true; // Use Unity's built-in gravity

        capsule = GetComponent<CapsuleCollider>();
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;

        GroundCheck();
    }

    private void Update()
    {
        if (!wasGrounded && isGrounded && !isJumping)
        {
            PlayLandingSound();
        }

        PlayFootsteps();
        wasGrounded = isGrounded;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        if (isGrounded)
        {
            isJumping = false;
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Vector2 input = GetInput();

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon))
        {
            Vector3 desiredMove = transform.forward * input.y + transform.right * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, Vector3.up).normalized;

            float targetSpeed = input.y > 0 ? forwardSpeed : backwardSpeed;
            if (Mathf.Abs(input.x) > 0) targetSpeed = strafeSpeed;

            desiredMove *= targetSpeed * SlopeMultiplier();

            if (isGrounded)
            {
                // Apply ground movement
                rigidBody.velocity = new Vector3(desiredMove.x, rigidBody.velocity.y, desiredMove.z);
                isMoving = rigidBody.velocity.magnitude > 0.1f;
            }
            else if (airControl && !isJumping)
            {
                // Apply air control only when airborne and not in the initial jump phase
                Vector3 airMove = desiredMove * airControlMultiplier;
                rigidBody.AddForce(airMove, ForceMode.Acceleration);
            }

            // Apply air resistance when not grounded
            if (!isGrounded)
            {
                Vector3 airResistance = -rigidBody.velocity * airDrag;
                rigidBody.AddForce(airResistance, ForceMode.Acceleration);
            }
        }
        else if (isGrounded && !isMoving)
        {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x * frictionDampening, rigidBody.velocity.y, rigidBody.velocity.z * frictionDampening);
            isMoving = rigidBody.velocity.magnitude > 0.1f;
        }
        else
        {
            isMoving = false;
        }
    }

private void Jump()
{
    float gravity = -Physics.gravity.y;
    float velocity = Mathf.Sqrt(gravity * jumpHeight * 2);

    // Increase the jump height based on the jump duration
    float heightMultiplier = 1 + (jumpDuration / 2f);
    velocity *= heightMultiplier;

    rigidBody.velocity = new Vector3(rigidBody.velocity.x, velocity, rigidBody.velocity.z);

    StartCoroutine(JumpCoroutine(jumpDuration));
}

private IEnumerator JumpCoroutine(float duration)
{
    float timer = 0f;

    while (timer < duration)
    {
        timer += Time.deltaTime;

        float t = timer / duration;
        t = t * t * (3f - 2f * t); // Smoothstep function

        // Adjust the jump curve value to make the jump feel more natural
        float height = Mathf.Lerp(jumpHeight, 0f, t * 0.5f);
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, Mathf.Sqrt(-Physics.gravity.y * height * 2), rigidBody.velocity.z);

        yield return null;
    }
}

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void PlayFootsteps()
    {
        if (isGrounded && isMoving)
        {
            if (walkAudioTimer > walkAudioSpeed)
            {
                currentClipIndex = (currentClipIndex + 1) % walkClips.Length;
                audioSource.clip = walkClips[currentClipIndex];
                audioSource.Play();
                walkAudioTimer = 0.0f;
            }
            walkAudioTimer += Time.deltaTime;
        }
    }

    private void PlayLandingSound()
    {
        if (audioClipLanding != null)
        {
            audioSource.PlayOneShot(audioClipLanding);
        }
    }

    private void GroundCheck()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius * (1f - shellOffset), Vector3.down, out hitInfo, ((capsule.height / 2f) - capsule.radius) + groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private float SlopeMultiplier()
    {
        Vector3 normal = Vector3.up; // Default to straight up if no ground contact normal is calculated
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, capsule.height * 0.5f))
        {
            normal = hitInfo.normal;
        }
        float angle = Vector3.Angle(normal, Vector3.up);
        return slopeCurveModifier.Evaluate(angle);
    }

    #if UNITY_EDITOR
private void OnDrawGizmos()
{
    // Draw ground check sphere
    Gizmos.color = Color.red;
    Vector3 groundCheckPosition = transform.position + Vector3.down * (((capsule.height / 2f) - capsule.radius) + groundCheckDistance);
    Gizmos.DrawWireSphere(groundCheckPosition, capsule.radius * (1f - shellOffset));

    // Draw forward direction
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2.0f);

    // Draw a simple jump arc visualization
    Gizmos.color = Color.blue;
    Vector3 startPosition = transform.position + Vector3.up * capsule.height / 2;
    Vector3 peakPosition = startPosition + Vector3.up * jumpHeight;
    Gizmos.DrawLine(startPosition, peakPosition);
    Gizmos.DrawLine(peakPosition, peakPosition + transform.forward * 2.0f);
}
#endif

}