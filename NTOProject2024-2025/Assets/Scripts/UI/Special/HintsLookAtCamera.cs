using System;
using UnityEngine;

public class HintsLookAtCamera : MonoBehaviour
{
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = WorkersInterBuildingControl.MainCamera.transform;
    }

    private void Update()
    {
        this.transform.LookAt(cameraTransform);
    }
}
