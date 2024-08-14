using UnityEngine;

public class Sneak : MonoBehaviour
{
    public float visionAngle = 60f;

    private bool isSneaking = false;

    private void Update()
    {
        // Toggle sneak mode when the 'Q' key is pressed
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleSneakMode();
        }
    }

    private void ToggleSneakMode()
    {
        isSneaking = !isSneaking;
        if (isSneaking)
        {
            Debug.Log("Entered sneak mode");
        }
        else
        {
            Debug.Log("Exited sneak mode");
        }
    }

    public bool IsSneaking()
    {
        return isSneaking;
    }

    public float GetVisionAngle()
    {
        return visionAngle;
    }
}