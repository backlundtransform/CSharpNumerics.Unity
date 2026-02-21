using UnityEngine;

/// <summary>
/// Fixed camera positioned to see the bouncing balls clearly.
/// Slowly orbits around the scene center.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private float distance = 18f;
    [SerializeField] private float height = 12f;
    [SerializeField] private float autoRotateSpeed = 10f;

    private float _angle = 0f;

    void LateUpdate()
    {
        _angle += autoRotateSpeed * Time.deltaTime;
        float rad = _angle * Mathf.Deg2Rad;

        // Circle around (0, height, 0) looking at the floor center
        transform.position = new Vector3(
            Mathf.Sin(rad) * distance,
            height,
            Mathf.Cos(rad) * distance);
        transform.LookAt(new Vector3(0, 2f, 0));
    }
}
