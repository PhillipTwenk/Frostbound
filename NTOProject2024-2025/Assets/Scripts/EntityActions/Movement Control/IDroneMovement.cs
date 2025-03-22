using System.Collections;
using EntityActions.WorkersScripts;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Интерфейс с методамии ивентами, общими для дронов
/// </summary>
public interface IDroneMovement
{
    void StartTakeoff();
    IEnumerator TakeoffCoroutine();
    void StartLanding();
    IEnumerator LandingCoroutine(Vector3 targetGroundPosition);
    Vector3 FindNearestLandingPosition();
    bool CheckForNonGroundObjects();
    UnityEvent OnStartTakeOff { get; }
    UnityEvent OnShutdown { get; }
}
