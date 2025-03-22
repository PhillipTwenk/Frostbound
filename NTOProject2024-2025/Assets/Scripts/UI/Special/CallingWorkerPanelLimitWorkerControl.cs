using System;
using TMPro;
using UnityEngine;

public class CallingWorkerPanelLimitWorkerControl : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        textMesh.text = String.Empty;
    }
}
