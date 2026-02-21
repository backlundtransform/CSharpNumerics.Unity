using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor menu item that sets up the demo scene automatically.
/// Menu: CSharpNumerics → Setup Demo Scene
/// </summary>
public static class SetupDemoScene
{
    [MenuItem("CSharpNumerics/Setup Demo Scene")]
    public static void Setup()
    {
        // ── Find or create the main camera ──
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }

        cam.transform.position = new Vector3(0, 10, -20);
        cam.transform.LookAt(new Vector3(0, 5, 0));
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

        if (cam.GetComponent<OrbitCamera>() == null)
            cam.gameObject.AddComponent<OrbitCamera>();

        // ── Create PhysicsDemo manager ──
        var existing = Object.FindAnyObjectByType<PhysicsDemo>();
        if (existing != null)
        {
            Debug.Log("PhysicsDemo already exists in scene.");
            return;
        }

        var manager = new GameObject("PhysicsDemo");
        manager.AddComponent<PhysicsDemo>();

        // ── Add directional light if missing ──
        if (Object.FindAnyObjectByType<Light>() == null)
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        Debug.Log("CSharpNumerics demo scene setup complete! Press Play to run.");
    }
}
