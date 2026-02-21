using UnityEngine;

/// <summary>
/// Simple orbit camera — rotate around origin with mouse, scroll to zoom.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private float distance = 20f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private Vector3 target = new Vector3(0, 5, 0);

    private float _yaw = 30f;
    private float _pitch = 20f;

    void Update()
    {
        if (Input.GetMouseButton(1)) // right-click drag
        {
            _yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            _pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);
        }

        distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        distance = Mathf.Clamp(distance, 2f, 50f);

        var rotation = Quaternion.Euler(_pitch, _yaw, 0);
        transform.position = target + rotation * (Vector3.back * distance);
        transform.LookAt(target);
    }
}
