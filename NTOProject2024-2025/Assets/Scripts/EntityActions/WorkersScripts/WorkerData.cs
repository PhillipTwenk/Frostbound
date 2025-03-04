using UnityEngine;

[System.Serializable]
public enum WorkersType
{
    Beekeeper,
    Constructor,
    MainDrone
}

public class WorkerData : MonoBehaviour
{
    public WorkersType workerType;
}


