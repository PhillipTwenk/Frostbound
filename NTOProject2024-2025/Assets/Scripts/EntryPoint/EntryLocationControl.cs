using System;
using TMPro;
using UnityEngine;

public class EntryLocationControl : MonoBehaviour
{
    [SerializeField] private GameEvent UpdateResourcesEvent;
    [SerializeField] private GameEvent BaseSongStartEvent;

    public void InitizilizePLayer()
    {
        UpdateResourcesEvent.TriggerEvent();

        PlayerSaveData pLayerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        pLayerSaveData.InitializeData();
        BaseSongStartEvent.TriggerEvent();

        LoadingCanvasController.Instance.LoadingCanvasNotTransparent.SetActive(false);
    }
}
