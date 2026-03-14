using UnityEngine;
using CSharpNumerics.Engines.Audio;

/// <summary>
/// Floating orb that emits a tone, flies around the listener, and uses
/// CSharpNumerics SpatialAudio to pan/attenuate in real-time.
/// </summary>
public class OrbSpatializer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float orbitRadius = 6f;
    [SerializeField] private float orbitSpeed = 0.5f;
    [SerializeField] private float orbitHeight = 3f;
    [SerializeField] private float verticalBob = 1.5f;

    [Header("Audio")]
    [SerializeField] private float toneFreq = 440f;
    [SerializeField] private float toneDuration = 4f;
    [SerializeField] private int sampleRate = 44100;

    [Header("HUD")]
    public float CurrentDistance;
    public float CurrentPan;

    private AudioSource _sourceL;
    private AudioSource _sourceR;
    private Transform _listener;
    private float _angle;
    private Renderer _renderer;
    private Material _orbMat;

    // Pre-rendered mono tone
    private AudioBuffer _monoTone;
    private float _nextPlayTime;

    void Start()
    {
        // Create emissive orb visual
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _orbMat = new Material(shader);
            Color orbColor = new Color(0.4f, 0.1f, 1f); // purple
            if (_orbMat.HasProperty("_BaseColor")) _orbMat.SetColor("_BaseColor", orbColor);
            if (_orbMat.HasProperty("_Color"))     _orbMat.SetColor("_Color", orbColor);
            if (_orbMat.HasProperty("_EmissionColor"))
            {
                _orbMat.EnableKeyword("_EMISSION");
                _orbMat.SetColor("_EmissionColor", orbColor * 5f);
            }
            _renderer.material = _orbMat;
        }

        // Two AudioSources for stereo spatialization (L/R)
        _sourceL = gameObject.AddComponent<AudioSource>();
        _sourceL.playOnAwake = false;
        _sourceL.spatialBlend = 0f;
        _sourceL.panStereo = -1f;

        _sourceR = gameObject.AddComponent<AudioSource>();
        _sourceR.playOnAwake = false;
        _sourceR.spatialBlend = 0f;
        _sourceR.panStereo = 1f;

        _listener = Camera.main != null ? Camera.main.transform : transform;

        // Generate the mono tone buffer
        var osc = new AudioOscillator(SignalGenerator.Waveform.Sine, toneFreq, 0.6);
        _monoTone = osc.GenerateBuffer(duration: toneDuration, sampleRate: sampleRate);
    }

    void Update()
    {
        // Orbit around center
        _angle += orbitSpeed * Time.deltaTime;
        float x = Mathf.Cos(_angle * Mathf.PI * 2f) * orbitRadius;
        float z = Mathf.Sin(_angle * Mathf.PI * 2f) * orbitRadius;
        float y = orbitHeight + Mathf.Sin(_angle * Mathf.PI * 4f) * verticalBob;
        transform.position = new Vector3(x, y, z);

        // Compute distance and pan values
        CurrentDistance = Vector3.Distance(transform.position, _listener.position);
        float dx = transform.position.x - _listener.position.x;
        float maxDist = orbitRadius * 2f;
        CurrentPan = Mathf.Clamp(dx / maxDist, -1f, 1f);

        // Pulse emission based on distance
        if (_orbMat != null && _orbMat.HasProperty("_EmissionColor"))
        {
            float glow = Mathf.Lerp(8f, 1f, CurrentDistance / (orbitRadius + 2f));
            _orbMat.SetColor("_EmissionColor", new Color(0.4f, 0.1f, 1f) * glow);
        }

        // Play spatialized tone on a loop
        if (Time.time >= _nextPlayTime && _monoTone != null)
        {
            PlaySpatialized();
            _nextPlayTime = Time.time + toneDuration * 0.9f; // slight overlap for seamless loop
        }
    }

    private void PlaySpatialized()
    {
        // Use CSharpNumerics SpatialAudio to spatialize the mono tone
        var spatialized = SpatialAudio.Spatialize(
            _monoTone,
            sourceX: transform.position.x,
            sourceY: transform.position.z,
            listenerX: _listener.position.x,
            listenerY: _listener.position.z);

        // Split stereo buffer into L and R for the two AudioSources
        if (spatialized.Channels == 2)
        {
            int frames = spatialized.FrameCount;
            double[] leftSamples = new double[frames];
            double[] rightSamples = new double[frames];
            for (int i = 0; i < frames; i++)
            {
                leftSamples[i]  = spatialized.Samples[i * 2];
                rightSamples[i] = spatialized.Samples[i * 2 + 1];
            }

            var leftBuf  = new AudioBuffer(leftSamples,  sampleRate, 1);
            var rightBuf = new AudioBuffer(rightSamples, sampleRate, 1);

            _sourceL.PlayOneShot(AudioBridge.ToClip(leftBuf, "orb_L"));
            _sourceR.PlayOneShot(AudioBridge.ToClip(rightBuf, "orb_R"));
        }
        else
        {
            _sourceL.panStereo = CurrentPan;
            AudioBridge.PlayOneShot(_sourceL, spatialized, "orb_spatial");
        }
    }
}
