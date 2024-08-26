using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapSweep : MonoBehaviour
{
    private Transform sweepLine;
    private float rotationSpeed;
    // Start is called before the first frame update
    private void Awake()
    {
        sweepLine = transform.Find("SweepLine");
        rotationSpeed = 180.0f;
    }

    // Update is called once per frame
    void Update()
    {
        sweepLine.eulerAngles -= new Vector3(0, 0, rotationSpeed * Time.deltaTime);
    }
}
