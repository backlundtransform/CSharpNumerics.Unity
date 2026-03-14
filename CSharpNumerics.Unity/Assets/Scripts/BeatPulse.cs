using UnityEngine;
using CSharpNumerics.Engines.Audio;
using System.Collections.Generic;

/// <summary>
/// Beat floor — generates a rhythmic loop, detects onsets with BeatDetector,
/// and pulses the floor plane (scale + emission flash) on each beat.
/// Also displays BPM and detected pitch.
/// </summary>
public class BeatPulse : MonoBehaviour
{
    [Header("Loop")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private float loopDuration = 4f;

    [Header("Detection")]
    [SerializeField] private int frameSize = 1024;
    [SerializeField] private float beatThreshold = 1.5f;

    [Header("Visual Pulse")]
    [SerializeField] private float pulseScale = 1.15f;
    [SerializeField] private float pulseDecay = 8f;
    [SerializeField] private Color baseColor = new Color(0.05f, 0.02f, 0.1f);
    [SerializeField] private Color pulseColor = new Color(0.5f, 0f, 1f);

    [Header("HUD")]
    public string DetectedBPM = "";
    public string DetectedPitch = "";

    // Exposed for SpectrumVisualizer
    public AudioBuffer LoopBuffer { get; private set; }

    private AudioSource _source;
    private Renderer _floorRenderer;
    private Material _floorMat;
    private Vector3 _baseScale;
    private float _pulseAmount;
    private List<double> _onsets;
    private int _nextOnsetIndex;
    private float _loopStartTime;

    void Start()
    {
        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 0f;

        _floorRenderer = GetComponent<Renderer>();
        if (_floorRenderer != null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _floorMat = new Material(shader);
            if (_floorMat.HasProperty("_BaseColor")) _floorMat.SetColor("_BaseColor", baseColor);
            if (_floorMat.HasProperty("_Color"))     _floorMat.SetColor("_Color", baseColor);
            if (_floorMat.HasProperty("_EmissionColor"))
            {
                _floorMat.EnableKeyword("_EMISSION");
                _floorMat.SetColor("_EmissionColor", baseColor);
            }
            _floorRenderer.material = _floorMat;
        }

        _baseScale = transform.localScale;
        GenerateAndAnalyzeLoop();
    }

    private void GenerateAndAnalyzeLoop()
    {
        // Build a rhythmic loop: kick-like hits at regular intervals
        float beatInterval = 60f / bpm;
        var loopBuf = new AudioBuffer(sampleRate, 1, loopDuration);

        int beatCount = Mathf.FloorToInt(loopDuration / beatInterval);
        for (int b = 0; b < beatCount; b++)
        {
            float t = b * beatInterval;
            // Generate a short 80Hz kick with fast decay
            float kickFreq = 80f + (b % 2 == 1 ? 40f : 0f); // alternate slightly
            var kick = SignalGenerator.Generate(SignalGenerator.Waveform.Sine, kickFreq, 0.9, 0.15, sampleRate: sampleRate);
            // Apply quick envelope
            var env = new Envelope(0.005, 0.02, 0.3, 0.08);
            env.Apply(kick, noteOffTime: 0.08);

            // Mix into loop at correct position
            int offsetSample = (int)(t * sampleRate);
            for (int i = 0; i < kick.Samples.Length && offsetSample + i < loopBuf.Samples.Length; i++)
                loopBuf.Samples[offsetSample + i] += kick.Samples[i];
        }

        loopBuf.Normalize();
        LoopBuffer = loopBuf;

        // Beat detection
        var detector = new BeatDetector(frameSize) { Threshold = beatThreshold };
        _onsets = detector.Detect(loopBuf);
        double estimatedBpm = detector.EstimateTempo(loopBuf);
        DetectedBPM = $"{estimatedBpm:F0} BPM";

        // Pitch detection
        var pitchDetector = new PitchDetector(4096);
        double f0 = pitchDetector.Detect(loopBuf, PitchDetector.Method.Autocorrelation);
        if (f0 > 0)
            DetectedPitch = $"{NoteFromFreq(f0)} ({f0:F1} Hz)";
        else
            DetectedPitch = "—";

        // Play the loop
        AudioBridge.PlayLoop(_source, loopBuf, "beat_loop");
        _loopStartTime = Time.time;
        _nextOnsetIndex = 0;

        Debug.Log($"BeatPulse: {_onsets.Count} onsets detected, est. {estimatedBpm:F0} BPM, pitch {DetectedPitch}");
    }

    void Update()
    {
        // Check if we've hit the next onset
        if (_onsets != null && _onsets.Count > 0)
        {
            float loopTime = (Time.time - _loopStartTime) % loopDuration;

            // Reset onset index on loop wrap
            if (_nextOnsetIndex >= _onsets.Count)
                _nextOnsetIndex = 0;

            if (_nextOnsetIndex < _onsets.Count && loopTime >= _onsets[_nextOnsetIndex])
            {
                _pulseAmount = 1f;
                _nextOnsetIndex++;
            }
        }

        // Animate pulse decay
        _pulseAmount = Mathf.Max(0, _pulseAmount - pulseDecay * Time.deltaTime);
        float s = 1f + _pulseAmount * (pulseScale - 1f);
        transform.localScale = new Vector3(_baseScale.x * s, _baseScale.y, _baseScale.z * s);

        // Emission pulse
        if (_floorMat != null && _floorMat.HasProperty("_EmissionColor"))
        {
            Color emission = Color.Lerp(baseColor * 0.5f, pulseColor * 4f, _pulseAmount);
            _floorMat.SetColor("_EmissionColor", emission);
        }

        // Regenerate loop on Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bpm += 20f;
            if (bpm > 200f) bpm = 80f;
            _source.Stop();
            GenerateAndAnalyzeLoop();
        }
    }

    private static string NoteFromFreq(double freq)
    {
        if (freq <= 0) return "—";
        string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        double semitones = 12.0 * System.Math.Log(freq / 440.0) / System.Math.Log(2.0) + 69;
        int midi = (int)System.Math.Round(semitones);
        int note = ((midi % 12) + 12) % 12;
        int octave = midi / 12 - 1;
        return $"{notes[note]}{octave}";
    }
}
