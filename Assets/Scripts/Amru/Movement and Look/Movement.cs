using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Movement : MonoBehaviour
{
    #region Serialized Fields

    [Header("Audio Clips")]
    [Tooltip("The audio clips that are played while walking.")]
    [SerializeField] 
    private AudioClip[] walkClips;

    [Tooltip("The audio clip that is played when landing.")]
    [SerializeField] 
    private AudioClip audioClipLanding;

    [Header("Speeds")]
    [SerializeField] 
    private float speedWalking = 5.0f;

    [Tooltip("The force applied when the player jumps.")]
    [SerializeField] 
    private float jumpForce = 5.0f;

    [Header("Ground Detection")]
    [Tooltip("Layers to consider as ground.")]
    [SerializeField] 
    private LayerMask groundLayerMask;

    #endregion

    #region Private Fields

    private Rigidbody rigidBody;
    private CapsuleCollider capsule;
    private AudioSource audioSource;

    private bool grounded;
    private bool wasGrounded;
    private bool isJumping;
    private float walkAudioTimer = 0.0f;
    private float walkAudioSpeed = 0.4f;
    private int currentClipIndex = 0;

    public bool isMoving;

    private RaycastHit[] groundHits = new RaycastHit[10]; 
    
    // private float previousVelocityY;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Initialize components
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidBody.velocity = Vector3.zero; // Ensure initial velocity is zero

        capsule = GetComponent<CapsuleCollider>();

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;

        // Perform initial ground check
        GroundCheck();
    }

    private void Start()
    {
       // previousVelocityY = rigidBody.velocity.y;
    }

    private void Update()
    {
        // Track vertical velocity changes for debuging
        //float currentVelocityY = rigidBody.velocity.y;
        //float velocityChangeY = currentVelocityY - previousVelocityY;

        // Debug.Log($"Velocity change Y: {velocityChangeY:F4}");

       // previousVelocityY = currentVelocityY;

        // Play landing sound if player was airborne and is now grounded
        if (!wasGrounded && grounded && !isJumping)
        {
            PlayLandingSound();
        }

        // Handle footsteps audio
        PlayFootsteps();
        wasGrounded = grounded;

        // Handle jumping input
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Jump();
        }

        // Reset jumping state if the player is grounded
        if (grounded)
        {
            isJumping = false;
        }
    }

    private void FixedUpdate()
    {
        // Move the character
        MoveCharacter();

        // Reset grounded state to be updated in GroundCheck
        grounded = false;
    }

    private void OnCollisionStay()
    {
        // Perform ground check while collision is ongoing
        GroundCheck();
    }

    #endregion

    #region Movement Methods

    private void MoveCharacter()
    {
        // Get movement input
        Vector2 frameInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 movement = new Vector3(frameInput.x, 0.0f, frameInput.y);

        // Normalize movement to ensure consistent speed in all directions
        if (movement.magnitude > 1.0f)
        {
            movement.Normalize();
        }

        // Scale movement by walking speed and transform it to world space
        movement *= speedWalking;
        movement = transform.TransformDirection(movement);

        // Apply movement only if grounded
        if (grounded)
        {
            isMoving = movement.sqrMagnitude > 0.1f;
            rigidBody.velocity = new Vector3(movement.x, rigidBody.velocity.y, movement.z);
        }
        else
        {
            isMoving = false;
        }
    }

    private void Jump()
    {
        // Apply jump force and set states
        rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        grounded = false;
        isJumping = true;
    }

    #endregion

    #region Audio Methods

    private void PlayFootsteps()
    {
        // Play walking sound if moving and grounded
        if (grounded && isMoving)
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

    #endregion

    #region Ground Detection Methods

    private void GroundCheck()
    {
        // Get capsule bounds and calculate radius
        Bounds bounds = capsule.bounds;
        Vector3 extents = bounds.extents;
        float radius = extents.x - 0.01f;

        // Perform sphere cast to check for ground
        int hitCount = Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
            groundHits, extents.y - radius * 0.5f, groundLayerMask, QueryTriggerInteraction.Ignore);

        // Determine if grounded based on hits
        grounded = groundHits.Take(hitCount).Any(hit => hit.collider != null && hit.collider != capsule);

        // Reset ground hits array if grounded
        if (grounded)
        {
            for (int i = 0; i < groundHits.Length; i++)
                groundHits[i] = new RaycastHit();
        }
    }

    #endregion
}
