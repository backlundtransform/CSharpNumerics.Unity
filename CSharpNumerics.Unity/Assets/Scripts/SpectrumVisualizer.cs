using UnityEngine;
using CSharpNumerics.Engines.Audio;

/// <summary>
/// FFT spectrum wall — vertical bars colored cyan→magenta that react to audio.
/// Reads samples from SynthManager.LastBuffer or from BeatPulse loop.
/// </summary>
public class SpectrumVisualizer : MonoBehaviour
{
    [Header("FFT")]
    [SerializeField] private int fftSize = 2048;
    [SerializeField] private int barCount = 64;
    [SerializeField] private float maxBarHeight = 8f;
    [SerializeField] private float magnitudeScale = 200f;

    [Header("Layout")]
    [SerializeField] private float wallWidth = 12f;
    [SerializeField] private float barDepth = 0.15f;

    private Transform[] _bars;
    private Renderer[] _barRenderers;
    private SpectrumAnalyzer _analyzer;
    private Material[] _barMats;

    void Start()
    {
        _analyzer = new SpectrumAnalyzer(fftSize, SpectrumAnalyzer.WindowType.Hann);

        _bars = new Transform[barCount];
        _barRenderers = new Renderer[barCount];
        _barMats = new Material[barCount];

        float barWidth = wallWidth / barCount;
        float startX = -wallWidth * 0.5f;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        for (int i = 0; i < barCount; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"SpectrumBar_{i}";
            go.transform.SetParent(transform);
            Destroy(go.GetComponent<Collider>());

            float t = i / (float)(barCount - 1);
            float x = startX + i * barWidth + barWidth * 0.5f;
            go.transform.localPosition = new Vector3(x, 0, 0);
            go.transform.localScale = new Vector3(barWidth * 0.85f, 0.1f, barDepth);

            // Cyan → Magenta gradient material
            var mat = new Material(shader);
            Color barColor = Color.Lerp(Color.cyan, Color.magenta, t);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", barColor);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color", barColor);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", barColor * 2f);
            }
            go.GetComponent<Renderer>().material = mat;

            _bars[i] = go.transform;
            _barRenderers[i] = go.GetComponent<Renderer>();
            _barMats[i] = mat;
        }
    }

    void Update()
    {
        if (_analyzer == null || _bars == null) return;

        // Get samples from whatever is playing
        var synth = FindObjectOfType<SynthManager>();
        AudioBuffer buf = synth != null ? synth.LastBuffer : null;

        // Fallback: try BeatPulse loop buffer
        if (buf == null)
        {
            var beat = FindObjectOfType<BeatPulse>();
            if (beat != null) buf = beat.LoopBuffer;
        }

        if (buf == null || buf.Samples.Length < fftSize) return;

        // Take the first fftSize samples
        double[] samples = new double[fftSize];
        int copyLen = Mathf.Min(fftSize, buf.Samples.Length);
        System.Array.Copy(buf.Samples, samples, copyLen);

        var spectrum = _analyzer.Analyze(samples, sampleRate: buf.SampleRate);
        if (spectrum == null || spectrum.Count == 0) return;

        // Map spectrum bins to bars using logarithmic frequency scale
        // so bass doesn't eat all bars and treble gets representation
        int specLen = spectrum.Count;
        double minFreq = 20.0;
        double maxFreq = buf.SampleRate * 0.5; // Nyquist
        double logMin = System.Math.Log(minFreq);
        double logMax = System.Math.Log(maxFreq);

        for (int i = 0; i < barCount; i++)
        {
            // Logarithmic bin range for this bar
            double logLo = logMin + (logMax - logMin) * i / barCount;
            double logHi = logMin + (logMax - logMin) * (i + 1) / barCount;
            double freqLo = System.Math.Exp(logLo);
            double freqHi = System.Math.Exp(logHi);

            // Convert frequencies to bin indices
            double binPerHz = specLen / maxFreq;
            int binStart = Mathf.Clamp((int)(freqLo * binPerHz), 0, specLen - 1);
            int binEnd   = Mathf.Clamp((int)(freqHi * binPerHz), binStart + 1, specLen);

            // Average magnitude for this frequency range
            double sum = 0;
            for (int b = binStart; b < binEnd; b++)
                sum += spectrum[b].Value;
            double avgMag = sum / Mathf.Max(1, binEnd - binStart);

            float height = Mathf.Clamp((float)avgMag * magnitudeScale, 0.05f, maxBarHeight);

            // Animate bar height
            var scale = _bars[i].localScale;
            scale.y = Mathf.Lerp(scale.y, height, Time.deltaTime * 15f);
            _bars[i].localScale = scale;

            // Move bar up so it grows from floor
            var pos = _bars[i].localPosition;
            pos.y = scale.y * 0.5f;
            _bars[i].localPosition = pos;

            // Intensity-based emission
            float t = i / (float)(barCount - 1);
            Color baseColor = Color.Lerp(Color.cyan, Color.magenta, t);
            float intensity = height / maxBarHeight;
            if (_barMats[i].HasProperty("_EmissionColor"))
                _barMats[i].SetColor("_EmissionColor", baseColor * (1f + intensity * 4f));
        }
    }
}
