using UnityEngine;

[System.Serializable]
public enum WorkersType
{
    Beekeeper,
    Constructor,
    MainDrone
}

[System.Serializable]
public class WorkerData : MonoBehaviour
{
    public WorkersType workerType;
    public bool IsWorkerAtWork;
    public int SaveListIndex;

    private void Start()
    {
        JSONSerializeManager.streamingDataSaveEvent += SaveWorkerPositionEndGame;
    }
    private void OnDestroy()
    {
        JSONSerializeManager.streamingDataSaveEvent -= SaveWorkerPositionEndGame;
    }

    #region Methods

    /// <summary>
    /// Сохранение расположения рабочего после выхода при сохранении JSON
    /// </summary>
    public void SaveWorkerPositionEndGame()
    {
        PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        TransformData transformData = new TransformData(this.gameObject.transform);
        WorkersDataSaveData workersDataSaveData = new WorkersDataSaveData(this.gameObject.GetComponent<WorkerData>());

        playerSaveData.workersTransform[SaveListIndex] = transformData;
        playerSaveData.workerDatas[SaveListIndex] = workersDataSaveData;
        
        Debug.Log($"<color=green> Сохранено расположение рабочего\nНомер в списке: {SaveListIndex}\nКоординаты: x:{transformData.position.x}, y:{transformData.position.y}, z:{transformData.position.z}");
    }

    #endregion
}


