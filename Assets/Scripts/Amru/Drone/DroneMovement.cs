using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    Rigidbody ourDrone;
    private AudioSource droneSound;

    public Transform[] motors; 
    public float baseRotationSpeed = 500.0f; 
    public float rotationMultiplier = 1.2f;
    private float currentMotorSpeed = 0.0f;

    void Awake()
    {
        ourDrone = GetComponent<Rigidbody>();
        droneSound = gameObject.transform.Find("DroneSound").GetComponent<AudioSource>();
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
  
    void FixedUpdate()
    {
        MovementUpDown();
        MovementForward();
        Rotation();
        ClampingSpeedValues();
        Swerve();
        DroneSound();
        RotateMotors();

        ourDrone.AddRelativeForce(Vector3.up * upForce);
        ourDrone.rotation = Quaternion.Euler(
            new Vector3(tiltAmountForward, currentYRotation, tiltAmountSideways)
        );
    }

    public float upForce;
    void MovementUpDown()
    {
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f) 
        {
        if (Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.K)) {
            ourDrone.velocity = ourDrone.velocity;
        }
        if (Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.K) && Input.GetKey(KeyCode.J) && Input.GetKey(KeyCode.L)) {
            ourDrone.velocity = new Vector3(ourDrone.velocity.x, Mathf.Lerp(ourDrone.velocity.y, 0, Time.deltaTime * 5), ourDrone.velocity.z);
            upForce = 281;
        }
        if (Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.K) && Input.GetKey(KeyCode.J) && Input.GetKey(KeyCode.L)) {
            ourDrone.velocity = new Vector3(ourDrone.velocity.x, Mathf.Lerp(ourDrone.velocity.y, 0, Time.deltaTime * 5), ourDrone.velocity.z);
            upForce = 110;
        }
        if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.L)) {
            upForce = 410;
        }
        }

        if (Mathf.Abs(Input.GetAxis("Vertical")) < 0.2f && Mathf.Abs(Input.GetAxis("Horizontal")) < 0.2f)
        {
            upForce = 135;
        }

        if (Input.GetKey(KeyCode.Space)) // Upward movement
        {
            upForce = 450;
            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f)
            {
                upForce = 500;
            }
        }
        else if (Input.GetKey(KeyCode.LeftShift)) // Downward movement
        {
            upForce = -200;
        }
        else if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift) && (Mathf.Abs(Input.GetAxis("Vertical")) < 0.2f && Mathf.Abs(Input.GetAxis("Horizontal")) < 0.2f))
        {
            upForce = 98.1f;
        }
    }

    private float movementForwardSpeed = 500.0f;
    private float tiltAmountForward = 0;
    private float tiltVelocityForward;

    void MovementForward()
    {
        if (Input.GetAxis("Vertical") != 0)
        {
            float forwardForce = Input.GetAxis("Vertical") * movementForwardSpeed;
            ourDrone.AddRelativeForce(Vector3.forward * forwardForce);

            // Maintain altitude by counteracting the downward force caused by tilt
            float altitudeCompensation = ourDrone.velocity.y;
            ourDrone.AddRelativeForce(Vector3.up * -altitudeCompensation, ForceMode.VelocityChange);

            // Apply tilt for visual effect
            tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward, 20 * Input.GetAxis("Vertical"), ref tiltVelocityForward, 0.1f);
        }
        else
        {
            // Apply drag when no input is detected
            float dragForce = -ourDrone.velocity.z * 5f;
            ourDrone.AddRelativeForce(Vector3.forward * dragForce);

            // Smoothly reduce tilt amount to zero
            tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward, 0, ref tiltVelocityForward, 0.1f);

            // Gradually reduce the velocity in the forward direction
            ourDrone.velocity = new Vector3(ourDrone.velocity.x, ourDrone.velocity.y, Mathf.Lerp(ourDrone.velocity.z, 0, Time.deltaTime * 2f));
        }
    }



    private float wantedYRotation;
    public float currentYRotation;
    private float rotateAmountByKeys = 2.5f;
    private float rotationYVelocity;

    void Rotation()
    {
        // Mouse rotation
        float mouseX = Input.GetAxis("Mouse X");
        wantedYRotation += mouseX * rotateAmountByKeys;

        if (Input.GetKey(KeyCode.J))
        {
            wantedYRotation -= rotateAmountByKeys;
        }
        if (Input.GetKey(KeyCode.L))
        {
            wantedYRotation += rotateAmountByKeys;
        }

        currentYRotation = Mathf.SmoothDamp(currentYRotation, wantedYRotation, ref rotationYVelocity, 0.25f);
    }

    private Vector3 velocityToSmoothDampToZero;

    void ClampingSpeedValues()
    {
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f)
        {
            ourDrone.velocity = Vector3.Lerp(ourDrone.velocity, Vector3.ClampMagnitude(ourDrone.velocity, 10.0f), Time.deltaTime * 5f);
        }

        if (Mathf.Abs(Input.GetAxis("Vertical")) < 0.2f && Mathf.Abs(Input.GetAxis("Horizontal")) < 0.2f)
        {
            ourDrone.velocity = Vector3.Lerp(ourDrone.velocity, Vector3.ClampMagnitude(ourDrone.velocity, 5.0f), Time.deltaTime * 5f);
        }

        if (Mathf.Abs(Input.GetAxis("Vertical")) < 0.2f && Mathf.Abs(Input.GetAxis("Horizontal")) < 0.2f)
        {
            ourDrone.velocity = Vector3.SmoothDamp(ourDrone.velocity, Vector3.zero, ref velocityToSmoothDampToZero, 0.95f);
        }
    }

    private float sideMovementAmount = 200.0f;
    private float tiltAmountSideways = 0;
    private float tiltAmountVelocity;

    void Swerve()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f)
        {
            ourDrone.AddRelativeForce(Vector3.right * Input.GetAxis("Horizontal") * sideMovementAmount);
            tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways, -1 * Input.GetAxis("Horizontal"), ref tiltAmountVelocity, 0.1f);
        }
        else
        {
            tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways, 0, ref tiltAmountVelocity, 0.1f);
        }
    }

    void DroneSound()
    {
        droneSound.pitch = 1 + (ourDrone.velocity.magnitude / 100);
    }

    // In DroneMovement script
public void SetInitialRotation(Vector3 direction)
{
    wantedYRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    currentYRotation = wantedYRotation;
    transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
}


    void RotateMotors()
    {
        // Start with base rotation speed
        currentMotorSpeed = baseRotationSpeed;

        // Increase motor speed if moving up, forward, or sideways
        if (Input.GetKey(KeyCode.Space) || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
        {
            currentMotorSpeed *= rotationMultiplier; // Increase rotation speed
        }

        // Decrease motor speed if moving down
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentMotorSpeed /= rotationMultiplier; // Decrease rotation speed
        }

        // Apply rotation to each motor on the Z-axis
        foreach (var motor in motors)
        {
            motor.Rotate(Vector3.forward, currentMotorSpeed * Time.deltaTime, Space.Self);
        }
    }
}
