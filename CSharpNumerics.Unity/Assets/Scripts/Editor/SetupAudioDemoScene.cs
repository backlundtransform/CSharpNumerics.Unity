using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor menu item: CSharpNumerics → Setup Audio Demo Scene
/// Creates the dark neon scene with all audio demo zones.
/// </summary>
public static class SetupAudioDemoScene
{
    [MenuItem("CSharpNumerics/Setup Audio Demo Scene")]
    public static void Setup()
    {
        // ── Camera ──
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }
        cam.transform.position = new Vector3(0, 6, -12);
        cam.transform.LookAt(new Vector3(0, 2, 4));
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.04f); // very dark

        // Audio listener on camera
        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();

        // Remove orbit camera if present (static cam for audio demo)
        var orbit = cam.GetComponent<OrbitCamera>();
        if (orbit != null) Object.DestroyImmediate(orbit);

        // Remove PhysicsDemo if present (not part of audio demo)
        var physics = Object.FindObjectOfType<PhysicsDemo>();
        if (physics != null) Object.DestroyImmediate(physics.gameObject);

        // HUD
        if (cam.GetComponent<AudioDemoHUD>() == null)
            cam.gameObject.AddComponent<AudioDemoHUD>();

        // ── Directional Light (dim, moody) ──
        if (Object.FindObjectOfType<Light>() == null)
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.3f;
            light.color = new Color(0.6f, 0.6f, 0.8f);
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        // ── Beat Floor (center/floor) ──
        if (Object.FindObjectOfType<BeatPulse>() == null)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "BeatFloor";
            floor.transform.position = new Vector3(0, -0.05f, 0);
            floor.transform.localScale = new Vector3(20f, 0.1f, 20f);
            Object.DestroyImmediate(floor.GetComponent<Collider>());
            floor.AddComponent<BeatPulse>();
        }

        // ── Synth Pad (center-front, facing camera) ──
        if (Object.FindObjectOfType<SynthManager>() == null)
        {
            var synthGo = new GameObject("SynthPad");
            synthGo.transform.position = new Vector3(0f, 2f, 0f);
            synthGo.AddComponent<SynthManager>();
            synthGo.AddComponent<EffectsChain>();
        }

        // ── Spectrum Wall (centered behind synth) ──
        if (Object.FindObjectOfType<SpectrumVisualizer>() == null)
        {
            var wallGo = new GameObject("SpectrumWall");
            wallGo.transform.position = new Vector3(0, 0, 6f);
            wallGo.AddComponent<SpectrumVisualizer>();
        }

        // Remove orb if present from previous setup
        var oldOrb = Object.FindObjectOfType<OrbSpatializer>();
        if (oldOrb != null) Object.DestroyImmediate(oldOrb.gameObject);

        // ── Ambient grid floor lines (subtle visual) ──
        CreateGridLines();

        Debug.Log("CSharpNumerics Audio Demo scene setup complete! Press Play to run.\n" +
                  "Controls: Q-I = notes, 1-4 = waveform, F5-F8 = effects, Space = change BPM");
    }

    private static void CreateGridLines()
    {
        var existing = GameObject.Find("GridLines");
        if (existing != null) return;

        var root = new GameObject("GridLines");
        float size = 10f;
        int lines = 21;
        var mat = new Material(Shader.Find("Sprites/Default"));
        Color gridColor = new Color(0.1f, 0.1f, 0.2f, 0.4f);

        for (int i = 0; i < lines; i++)
        {
            float pos = -size + i * (size * 2f / (lines - 1));

            // X-axis line
            var lx = new GameObject($"GridX_{i}");
            lx.transform.SetParent(root.transform);
            var lrx = lx.AddComponent<LineRenderer>();
            lrx.material = mat;
            lrx.startColor = gridColor; lrx.endColor = gridColor;
            lrx.startWidth = 0.02f; lrx.endWidth = 0.02f;
            lrx.positionCount = 2;
            lrx.SetPosition(0, new Vector3(-size, 0.01f, pos));
            lrx.SetPosition(1, new Vector3(size, 0.01f, pos));

            // Z-axis line
            var lz = new GameObject($"GridZ_{i}");
            lz.transform.SetParent(root.transform);
            var lrz = lz.AddComponent<LineRenderer>();
            lrz.material = mat;
            lrz.startColor = gridColor; lrz.endColor = gridColor;
            lrz.startWidth = 0.02f; lrz.endWidth = 0.02f;
            lrz.positionCount = 2;
            lrz.SetPosition(0, new Vector3(pos, 0.01f, -size));
            lrz.SetPosition(1, new Vector3(pos, 0.01f, size));
        }
    }
}
