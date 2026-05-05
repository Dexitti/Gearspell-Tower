using UnityEngine;

public static class IsometricExtension
{
    public const float isoRatio = 0.5f;

    /// <summary>
    /// Преобразует мировые координаты в "изометрически-нормализованные"
    /// </summary>
    public static Vector3 ToIso(Vector3 worldPos, float ratio = isoRatio)
    {
        return new Vector3(worldPos.x, worldPos.y / ratio, worldPos.z);
    }

    /// <summary>
    /// Изометрическая дистанция (учитывает сжатие Y)
    /// </summary>
    public static float IsoDistance(Vector3 a, Vector3 b, float ratio = isoRatio)
    {
        Vector3 diff = a - b;
        diff.y /= ratio;
        return diff.magnitude;
    }

    /// <summary>
    /// Изометрическое направление (нормализованное)
    /// </summary>
    public static Vector3 IsoDirection(Vector3 from, Vector3 to, float ratio = isoRatio)
    {
        Vector3 dir = to - from;
        dir.y /= ratio;
        return dir.normalized;
    }
    /// <summary>
    /// Возвращает изометрический вектор перемещения по углу (в градусах) и расстоянию.
    /// </summary>
    public static Vector3 IsoVector(float angleDeg, float distance, float ratio = isoRatio)
    {
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float x = Mathf.Cos(angleRad) * distance;
        float y = Mathf.Sin(angleRad) * distance * ratio;
        return new Vector3(x, y, 0);
    }

    /// <summary>
    /// Изометрический Movement (уже с учётом сжатия Y)
    /// </summary>
    public static Vector3 IsoMovement(Vector3 direction, float speed, float ratio = isoRatio)
    {
        Vector3 movement = direction * speed * Time.deltaTime;
        movement.y *= ratio;
        return movement;
    }
}