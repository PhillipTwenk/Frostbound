using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Максимальная скорость движения камеры")]
    public float maxScrollSpeed = 10f;
    [Tooltip("Скорость ускорения камеры при движении")]
    public float acceleration = 5f;
    [Tooltip("Скорость замедления камеры при отсутствии команды")]
    public float deceleration = 5f;
    [Tooltip("Толщина зоны срабатывания по краям экрана (в пикселях)")]
    public float edgeThreshold = 10f;

    [Header("Ограничения карты")]
    [Tooltip("Минимальные границы карты (ось X и ось Z)")]
    public Vector2 minBounds = new Vector2(-50f, -50f);
    [Tooltip("Максимальные границы карты (ось X и ось Z)")]
    public Vector2 maxBounds = new Vector2(50f, 50f);

    private Vector3 currentSpeed = Vector3.zero;

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 targetSpeed = Vector3.zero;

        float horizontalFactor = 0f;
        float verticalFactor = 0f;

        // Горизонтальное движение (влево-вправо)
        if (mousePos.x < edgeThreshold)
            horizontalFactor = (edgeThreshold - mousePos.x) / edgeThreshold; // Движение влево
        else if (mousePos.x > Screen.width - edgeThreshold)
            horizontalFactor = -((mousePos.x - (Screen.width - edgeThreshold)) / edgeThreshold); // Движение вправо

        // Вертикальное движение (вперед-назад)
        if (mousePos.y < edgeThreshold)
            verticalFactor = -(edgeThreshold - mousePos.y) / edgeThreshold; // Движение назад
        else if (mousePos.y > Screen.height - edgeThreshold)
            verticalFactor = (mousePos.y - (Screen.height - edgeThreshold)) / edgeThreshold; // Движение вперед

        // Применяем изометрическую трансформацию
        targetSpeed.x = (horizontalFactor - verticalFactor) * maxScrollSpeed;
        targetSpeed.z = (horizontalFactor + verticalFactor) * maxScrollSpeed;

        // Плавное изменение скорости по каждой оси
        currentSpeed.x = Mathf.Lerp(currentSpeed.x, targetSpeed.x,
            (Mathf.Abs(targetSpeed.x) > 0.01f ? acceleration : deceleration) * Time.deltaTime);
        currentSpeed.z = Mathf.Lerp(currentSpeed.z, targetSpeed.z,
            (Mathf.Abs(targetSpeed.z) > 0.01f ? acceleration : deceleration) * Time.deltaTime);

        // Вычисляем новую позицию камеры с учетом скорости
        Vector3 newPosition = transform.position + currentSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
        newPosition.z = Mathf.Clamp(newPosition.z, minBounds.y, maxBounds.y);
        transform.position = newPosition;
    }

    // Отображение границ карты в Scene View
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, transform.position.y, (minBounds.y + maxBounds.y) * 0.5f);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, 0, maxBounds.y - minBounds.y);
        Gizmos.DrawWireCube(center, size);
    }
}
