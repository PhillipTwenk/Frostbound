using EntityActions.Movement_Control;
using UnityEngine;

public class DroneMovementController : MonoBehaviour, IUnitMovement
{
    public Vector3 GetSelectedMapPosition()
    {
        throw new System.NotImplementedException();
    }

    public void SetUnitDestination(Transform point, bool isAutomatic)
    {
        throw new System.NotImplementedException();
    }

    public void MovementHandler()
    {
        throw new System.NotImplementedException();
    }

    public bool isSelected { get; set; }
    public GameObject OutlineRotate { get; }
    public bool isSelecting { get; set; }
}
