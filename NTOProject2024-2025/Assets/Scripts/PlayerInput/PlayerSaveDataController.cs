using System;
using UnityEngine;

public class PlayerSaveDataController : MonoBehaviour
{
    private void Start()
    {
        JSONSerializeManager.streamingDataSaveEvent += SavePlayerPositionEndGame;
    }
    private void OnDestroy()
    {
        JSONSerializeManager.streamingDataSaveEvent -= SavePlayerPositionEndGame;
    }

    #region Methods

    /// <summary>
    /// Сохранение расположения игрока после выхода при сохранении JSON
    /// </summary>
    public void SavePlayerPositionEndGame()
    {
        PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        TransformData transformData = new TransformData(this.gameObject.transform);

        playerSaveData.transformPlayer = transformData;
        
        Debug.Log($"<color=green> Сохранено расположение игрока\nКоординаты: x:{transformData.position.x}, y:{transformData.position.y}, z:{transformData.position.z}");
    }

    #endregion
}
