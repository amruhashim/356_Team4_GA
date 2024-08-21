using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private string horizontalInputName = "Horizontal";
    [SerializeField] private string verticalInputName = "Vertical";
    [SerializeField] private float movementSpeed = 6.0f;

    [Header("Slope Settings")]
    [SerializeField] private float slopeForce = 5.0f;
    [SerializeField] private float slopeForceRayLength = 1.5f;

    [Header("Jump Settings")]
    [SerializeField] private AnimationCurve jumpFallOff;
    [SerializeField] private float jumpMultiplier = 10.0f;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField] private AudioClip audioClipJump;
    [SerializeField] private AudioClip audioClipLanding;
    [SerializeField] private float walkAudioSpeed = 0.4f;

    private CharacterController charController;
    private AudioSource audioSource;
    private Vector3 moveDirection = Vector3.zero;

    private bool isJumping;
    public bool isMoving; // Boolean to track if the player is moving
    private float walkAudioTimer = 0.0f;
    private int currentClipIndex = 0;

    private void Awake()
    {
        charController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        PlayerMovement();
        PlayFootsteps();
    }

    private void PlayerMovement()
    {
        float horizInput = Input.GetAxis(horizontalInputName);
        float vertInput = Input.GetAxis(verticalInputName);

        Vector3 forwardMovement = transform.forward * vertInput;
        Vector3 rightMovement = transform.right * horizInput;

        // Determine movement direction and set isMoving boolean
        Vector3 combinedMovement = Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * movementSpeed;
        isMoving = combinedMovement.magnitude > 0.1f;

        charController.SimpleMove(combinedMovement);

        // Handle slope movement
        if ((vertInput != 0 || horizInput != 0) && OnSlope())
        {
            charController.Move(Vector3.down * charController.height / 2 * slopeForce * Time.deltaTime);
        }

        JumpInput();
    }

    private bool OnSlope()
    {
        if (isJumping)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, charController.height / 2 * slopeForceRayLength))
        {
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    private void JumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && !isJumping)
        {
            isJumping = true;
            PlayJumpSound();
            StartCoroutine(JumpEvent());
        }
    }

    private IEnumerator JumpEvent()
    {
        charController.slopeLimit = 90.0f; // Allow jumping up slopes
        float timeInAir = 0.0f;
        do
        {
            float jumpForce = jumpFallOff.Evaluate(timeInAir);
            charController.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);
            timeInAir += Time.deltaTime;
            yield return null;
        } while (!charController.isGrounded && charController.collisionFlags != CollisionFlags.Above);

        charController.slopeLimit = 45.0f; // Reset slope limit after jump
        isJumping = false;
        PlayLandingSound(); // Play landing sound when grounded
    }

    private void PlayFootsteps()
    {
        if (charController.isGrounded && isMoving)
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

    private void PlayJumpSound()
    {
        if (audioClipJump != null)
        {
            audioSource.PlayOneShot(audioClipJump);
        }
    }

    private void PlayLandingSound()
    {
        if (audioClipLanding != null)
        {
            audioSource.PlayOneShot(audioClipLanding);
        }
    }
}
