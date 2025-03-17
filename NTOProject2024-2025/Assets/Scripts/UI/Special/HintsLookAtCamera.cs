using System;
using EntityActions.WorkersScripts;
using UnityEngine;

public class HintsLookAtCamera : MonoBehaviour
{
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = GeneralWorkersControl.MainCamera.transform;
    }

    private void Update()
    {
        this.transform.LookAt(cameraTransform);
    }
}
