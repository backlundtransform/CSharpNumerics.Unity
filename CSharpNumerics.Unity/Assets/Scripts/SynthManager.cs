using UnityEngine;
using UnityEngine.InputSystem;
using CSharpNumerics.Engines.Audio;

/// <summary>
/// Real-time synthesizer pad. Keys Q-W-E-R-T play C4-D4-E4-F4-G4.
/// Number keys 1-4 switch waveform. ADSR controlled by public fields (UI sliders).
/// Feeds live samples to SpectrumVisualizer and EffectsChain if present.
/// </summary>
public class SynthManager : MonoBehaviour
{
    [Header("Synth")]
    [SerializeField] private int sampleRate = 44100;

    [Header("ADSR Envelope")]
    [Range(0.001f, 0.5f)]  public float attack  = 0.02f;
    [Range(0.01f, 0.5f)]   public float decay   = 0.1f;
    [Range(0f, 1f)]         public float sustain = 0.7f;
    [Range(0.01f, 1f)]      public float release = 0.3f;

    [Header("Visual")]
    [SerializeField] private int waveformResolution = 256;

    // Current waveform — can be changed at runtime
    public SignalGenerator.Waveform CurrentWaveform { get; private set; } = SignalGenerator.Waveform.Sine;

    // Last rendered buffer for spectrum / effects chain consumption
    public AudioBuffer LastBuffer { get; private set; }

    private AudioSource _source;
    private LineRenderer _oscilloscope;

    // Note frequencies: C4 D4 E4 F4 G4 A4 B4 C5
    private static readonly float[] NoteFreqs = { 261.63f, 293.66f, 329.63f, 349.23f, 392.00f, 440f, 493.88f, 523.25f };
    private static readonly Key[] NoteKeys = { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I };
    private static readonly string[] NoteNames = { "C4", "D4", "E4", "F4", "G4", "A4", "B4", "C5" };

    [Header("HUD")]
    public string LastNotePlayed = "";
    public string WaveformName = "Sine";

    void Awake()
    {
        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 0f; // 2D sound

        // Oscilloscope line renderer
        _oscilloscope = gameObject.AddComponent<LineRenderer>();
        _oscilloscope.positionCount = waveformResolution;
        _oscilloscope.startWidth = 0.03f;
        _oscilloscope.endWidth = 0.03f;
        _oscilloscope.material = new Material(Shader.Find("Sprites/Default"));
        _oscilloscope.startColor = Color.cyan;
        _oscilloscope.endColor = Color.cyan;
        _oscilloscope.useWorldSpace = true;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Waveform switching: 1-4
        if (kb.digit1Key.wasPressedThisFrame) { CurrentWaveform = SignalGenerator.Waveform.Sine;     WaveformName = "Sine"; }
        if (kb.digit2Key.wasPressedThisFrame) { CurrentWaveform = SignalGenerator.Waveform.Square;   WaveformName = "Square"; }
        if (kb.digit3Key.wasPressedThisFrame) { CurrentWaveform = SignalGenerator.Waveform.Sawtooth; WaveformName = "Sawtooth"; }
        if (kb.digit4Key.wasPressedThisFrame) { CurrentWaveform = SignalGenerator.Waveform.Triangle; WaveformName = "Triangle"; }

        // Note playback
        for (int i = 0; i < NoteKeys.Length; i++)
        {
            if (kb[NoteKeys[i]].wasPressedThisFrame)
            {
                PlayNote(NoteFreqs[i]);
                LastNotePlayed = NoteNames[i];
            }
        }
    }

    public void PlayNote(float freq)
    {
        var synth = new Synthesizer { SampleRate = sampleRate };
        synth.AddVoice(new AudioOscillator(CurrentWaveform, freq, 1.0));
        synth.AddVoice(new AudioOscillator(CurrentWaveform, freq * 2, 0.3), gain: 0.3);
        synth.Envelope = new Envelope(attack, decay, sustain, release);

        float duration = attack + decay + 0.4f + release;
        float noteOff = attack + decay + 0.4f;
        var buf = synth.Render(duration: duration, noteOffTime: noteOff);

        // Apply effects chain if present
        var fx = FindObjectOfType<EffectsChain>();
        if (fx != null)
            buf = fx.Process(buf);

        LastBuffer = buf;
        AudioBridge.PlayOneShot(_source, buf, $"note_{freq:F0}");
        UpdateOscilloscope(buf);
    }

    private void UpdateOscilloscope(AudioBuffer buf)
    {
        if (buf == null || buf.Samples.Length == 0) return;

        // Show first waveformResolution samples as a line
        int count = Mathf.Min(waveformResolution, buf.FrameCount);
        _oscilloscope.positionCount = count;

        // Position the oscilloscope centered in world
        Vector3 origin = new Vector3(0f, 5f, 2f);
        float width = 8f;

        for (int i = 0; i < count; i++)
        {
            float x = origin.x + (i / (float)count) * width - width * 0.5f;
            float y = origin.y + (float)buf.Samples[i] * 1.5f;
            _oscilloscope.SetPosition(i, new Vector3(x, y, origin.z));
        }
    }
}
