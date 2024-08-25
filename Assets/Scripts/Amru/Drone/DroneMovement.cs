using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    // Drone components
    private Rigidbody droneRigidbody;
    private AudioSource droneAudioSource;

    // Motor settings
    [SerializeField] private Transform[] droneMotors; 
    [SerializeField] private float motorBaseRotationSpeed = 500.0f; 
    [SerializeField] private float motorRotationMultiplier = 1.2f;
    private float currentMotorRotationSpeed = 0.0f;

    // Movement settings
    [SerializeField] private float verticalForce;
    [SerializeField] private float forwardMovementSpeed = 500.0f;
    private float forwardTiltAmount = 0;
    private float forwardTiltVelocity;

    // Up and down force settings
    [SerializeField] private float upwardForce = 450f;  // Adjustable upward force
    [SerializeField] private float downwardForce = -200f;  // Adjustable downward force
    [SerializeField] private float hoverForce = 98.1f;  // Adjustable hover force

    // Rotation settings
    private float targetYRotation;
    private float currentYRotation;
    [SerializeField] private float rotationSensitivity = 2.5f;
    private float rotationSmoothVelocity;

    // Velocity control
    private Vector3 smoothedVelocity;

    // Side movement settings
    [SerializeField] private float sidewaysMovementSpeed = 200.0f;
    private float sidewaysTiltAmount = 0;
    private float sidewaysTiltVelocity;

    void Awake()
    {
        // Initialize drone components
        droneRigidbody = GetComponent<Rigidbody>();
        droneAudioSource = gameObject.transform.Find("DroneSound").GetComponent<AudioSource>();

        // Lock the cursor
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void FixedUpdate()
    {
        // Handle drone movements and rotations
        HandleVerticalMovement();
        HandleForwardMovement();
        HandleRotation();
        ClampVelocityValues();
        HandleSidewaysMovement();
        UpdateDroneSound();
        RotateDroneMotors();

        // Apply movement and rotation
        droneRigidbody.AddRelativeForce(Vector3.up * verticalForce);
        droneRigidbody.rotation = Quaternion.Euler(
            new Vector3(forwardTiltAmount, currentYRotation, sidewaysTiltAmount)
        );
    }

    // Handles vertical movement (up and down)
    void HandleVerticalMovement()
    {
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f) 
        {
            if (Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.K))
            {
                droneRigidbody.velocity = droneRigidbody.velocity;
            }
            if (Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.K) && Input.GetKey(KeyCode.J) && Input.GetKey(KeyCode.L))
            {
                droneRigidbody.velocity = new Vector3(droneRigidbody.velocity.x, Mathf.Lerp(droneRigidbody.velocity.y, 0, Time.deltaTime * 5), droneRigidbody.velocity.z);
                verticalForce = 281;
            }
            if (Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.K) && Input.GetKey(KeyCode.J) && Input.GetKey(KeyCode.L))
            {
                droneRigidbody.velocity = new Vector3(droneRigidbody.velocity.x, Mathf.Lerp(droneRigidbody.velocity.y, 0, Time.deltaTime * 5), droneRigidbody.velocity.z);
                verticalForce = 110;
            }
            if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.L))
            {
                verticalForce = 410;
            }
        }

        if (Mathf.Abs(Input.GetAxis("Vertical")) < 0.2f && Mathf.Abs(Input.GetAxis("Horizontal")) < 0.2f)
        {
            verticalForce = hoverForce;  // Adjustable hover force
        }

        if (Input.GetKey(KeyCode.Space)) // Upward movement
        {
            verticalForce = upwardForce;  // Use adjustable upward force
            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f)
            {
                verticalForce = upwardForce + 50f;  // Increase if moving horizontally
            }
        }
        else if (Input.GetKey(KeyCode.LeftShift)) // Downward movement
        {
            verticalForce = downwardForce;  // Use adjustable downward force
        }
        else if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift) && (Mathf.Abs(Input.GetAxis("Vertical")) < 0.2f && Mathf.Abs(Input.GetAxis("Horizontal")) < 0.2f))
        {
            verticalForce = hoverForce;  // Use adjustable hover force
        }
    }

    // Handles forward and backward movement
    void HandleForwardMovement()
    {
        if (Input.GetAxis("Vertical") != 0)
        {
            float forwardForce = Input.GetAxis("Vertical") * forwardMovementSpeed;
            droneRigidbody.AddRelativeForce(Vector3.forward * forwardForce);

            // Maintain altitude by counteracting the downward force caused by tilt
            float altitudeCompensation = droneRigidbody.velocity.y;
            droneRigidbody.AddRelativeForce(Vector3.up * -altitudeCompensation, ForceMode.VelocityChange);

            // Apply tilt for visual effect
            forwardTiltAmount = Mathf.SmoothDamp(forwardTiltAmount, 20 * Input.GetAxis("Vertical"), ref forwardTiltVelocity, 0.1f);
        }
        else
        {
            // Apply drag when no input is detected
            float dragForce = -droneRigidbody.velocity.z * 5f;
            droneRigidbody.AddRelativeForce(Vector3.forward * dragForce);

            // Smoothly reduce tilt amount to zero
            forwardTiltAmount = Mathf.SmoothDamp(forwardTiltAmount, 0, ref forwardTiltVelocity, 0.1f);

            // Gradually reduce the velocity in the forward direction
            droneRigidbody.velocity = new Vector3(droneRigidbody.velocity.x, droneRigidbody.velocity.y, Mathf.Lerp(droneRigidbody.velocity.z, 0, Time.deltaTime * 2f));
        }
    }

    // Handles drone rotation based on mouse and keys
    void HandleRotation()
    {
        // Mouse rotation
        float mouseX = Input.GetAxis("Mouse X");
        targetYRotation += mouseX * rotationSensitivity;

        if (Input.GetKey(KeyCode.J))
        {
            targetYRotation -= rotationSensitivity;
        }
        if (Input.GetKey(KeyCode.L))
        {
            targetYRotation += rotationSensitivity;
        }

        currentYRotation = Mathf.SmoothDamp(currentYRotation, targetYRotation, ref rotationSmoothVelocity, 0.25f);
    }

    // Clamps velocity values to avoid excessive speeds
    void ClampVelocityValues()
    {
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f)
        {
            droneRigidbody.velocity = Vector3.Lerp(droneRigidbody.velocity, Vector3.ClampMagnitude(droneRigidbody.velocity, 10.0f), Time.deltaTime * 5f);
        }

        if (Mathf.Abs(Input.GetAxis("Vertical")) < 0.2f && Mathf.Abs(Input.GetAxis("Horizontal")) < 0.2f)
        {
            droneRigidbody.velocity = Vector3.Lerp(droneRigidbody.velocity, Vector3.ClampMagnitude(droneRigidbody.velocity, 5.0f), Time.deltaTime * 5f);
        }

        if (Mathf.Abs(Input.GetAxis("Vertical")) < 0.2f && Mathf.Abs(Input.GetAxis("Horizontal")) < 0.2f)
        {
            droneRigidbody.velocity = Vector3.SmoothDamp(droneRigidbody.velocity, Vector3.zero, ref smoothedVelocity, 0.95f);
        }
    }

    // Handles sideways (lateral) movement
    void HandleSidewaysMovement()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f)
        {
            droneRigidbody.AddRelativeForce(Vector3.right * Input.GetAxis("Horizontal") * sidewaysMovementSpeed);
            sidewaysTiltAmount = Mathf.SmoothDamp(sidewaysTiltAmount, -1 * Input.GetAxis("Horizontal"), ref sidewaysTiltVelocity, 0.1f);
        }
        else
        {
            sidewaysTiltAmount = Mathf.SmoothDamp(sidewaysTiltAmount, 0, ref sidewaysTiltVelocity, 0.1f);
        }
    }

    // Updates the drone sound based on velocity
    void UpdateDroneSound()
    {
        droneAudioSource.pitch = 1 + (droneRigidbody.velocity.magnitude / 100);
    }

    // Sets the initial rotation of the drone
    public void SetInitialRotation(Vector3 direction)
    {
        targetYRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        currentYRotation = targetYRotation;
        transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
    }

    // Rotates the drone motors based on current movement
    void RotateDroneMotors()
    {
        // Start with base rotation speed
        currentMotorRotationSpeed = motorBaseRotationSpeed;

        // Increase motor speed if moving up, forward, or sideways
        if (Input.GetKey(KeyCode.Space) || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
        {
            currentMotorRotationSpeed *= motorRotationMultiplier; // Increase rotation speed
        }

        // Decrease motor speed if moving down
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentMotorRotationSpeed /= motorRotationMultiplier; // Decrease rotation speed
        }

        // Apply rotation to each motor on the Z-axis
        foreach (var motor in droneMotors)
        {
            motor.Rotate(Vector3.forward, currentMotorRotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    // Sets the drone's sensitivity (used in conjunction with SaveManager)
    public void SetSensitivity(float sensitivity)
    {
        rotationSensitivity = sensitivity;
    }
}
