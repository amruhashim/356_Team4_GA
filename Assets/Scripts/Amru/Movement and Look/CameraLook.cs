using UnityEngine;

public class CameraLook : MonoBehaviour
{
    #region FIELDS SERIALIZED
    [Header("Settings")]

    [Tooltip("Sensitivity when looking around.")]
    [SerializeField]
    private Vector2 sensitivity = new Vector2(1, 1);

    [Tooltip("Minimum and maximum up/down rotation angle the camera can have.")]
    [SerializeField]
    private Vector2 yClamp = new Vector2(-60, 60);

    [Tooltip("Should the look rotation be interpolated?")]
    [SerializeField]
    private bool useSmoothRotation;

    [Tooltip("The speed at which the look rotation is interpolated.")]
    [SerializeField]
    private float interpolationSpeed = 25.0f;

    [Header("References")]

    [Tooltip("The camera's transform.")]
    public Transform playerCamera;
    #endregion

    #region FIELDS
    private CharacterController characterController;
    private Quaternion rotationCharacter;
    private Quaternion rotationCamera;
    private bool isCursorLocked = false;
    
    public static CameraLook Instance { get; private set; } // Singleton instance
    #endregion

    #region UNITY
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: If you want the instance to persist across scenes
        }
    }

    private void Start()
    {
        // Initialize cursor state
        SetCursorState(false);

        // Cache the CharacterController reference and initial rotations
        characterController = GetComponent<CharacterController>();
        rotationCharacter = characterController.transform.localRotation;
        rotationCamera = playerCamera.localRotation;
    }

    private void Update()
    {
        // Unlock the cursor when the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorState(false);
        }
        else if (Input.GetMouseButtonDown(0) && !isCursorLocked)
        {
            // Lock and hide the cursor when the left mouse button is clicked
            SetCursorState(true);
        }
    }

    private void LateUpdate()
    {
        // Return if the cursor is not locked or the menu is open
        if (!isCursorLocked || (MenuManager.Instance != null && MenuManager.Instance.isMenuOpen))
            return;

        // Frame Input
        Vector2 frameInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * sensitivity;

        // Yaw and Pitch
        Quaternion rotationYaw = Quaternion.Euler(0.0f, frameInput.x, 0.0f);
        Quaternion rotationPitch = Quaternion.Euler(-frameInput.y, 0.0f, 0.0f);

        // Save rotation for smooth rotation
        rotationCamera *= rotationPitch;
        rotationCharacter *= rotationYaw;

        // Local Rotation
        Quaternion localRotation = playerCamera.localRotation;

        if (useSmoothRotation)
        {
            // Interpolate local rotation
            localRotation = Quaternion.Slerp(localRotation, rotationCamera, Time.deltaTime * interpolationSpeed);
            // Interpolate character rotation
            characterController.transform.localRotation = Quaternion.Slerp(characterController.transform.localRotation, rotationCharacter, Time.deltaTime * interpolationSpeed);
        }
        else
        {
            // Rotate local
            localRotation *= rotationPitch;
            // Clamp
            localRotation = Clamp(localRotation);

            // Rotate character
            characterController.transform.localRotation *= rotationYaw;
        }

        // Set local rotation
        playerCamera.localRotation = localRotation;
    }
    #endregion

    #region FUNCTIONS
    // Clamps the pitch of a quaternion according to clamps.
    private Quaternion Clamp(Quaternion rotation)
    {
        rotation.x /= rotation.w;
        rotation.y /= rotation.w;
        rotation.z /= rotation.w;
        rotation.w = 1.0f;

        // Pitch
        float pitch = 2.0f * Mathf.Rad2Deg * Mathf.Atan(rotation.x);

        // Clamp
        pitch = Mathf.Clamp(pitch, yClamp.x, yClamp.y);
        rotation.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * pitch);

        return rotation;
    }

    // Sets the cursor state to locked or unlocked.
    private void SetCursorState(bool locked)
    {
        isCursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public void SetSensitivity(Vector2 newSensitivity)
    {
        sensitivity = newSensitivity;
    }
    #endregion
}
