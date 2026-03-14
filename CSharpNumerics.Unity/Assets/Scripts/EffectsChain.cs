using UnityEngine;
using CSharpNumerics.Engines.Audio;

/// <summary>
/// Live effects rack with toggle/slider control.
/// Processes audio buffers through Reverb → Delay → AudioFilter → Compressor.
/// Keys: F5=Reverb, F6=Delay, F7=Filter, F8=Compressor
/// </summary>
public class EffectsChain : MonoBehaviour
{
    [Header("Reverb")]
    public bool reverbOn = false;
    [Range(0.1f, 1f)] public float roomSize = 0.7f;
    [Range(0f, 1f)]   public float damping = 0.4f;
    [Range(0f, 1f)]   public float reverbWet = 0.3f;

    [Header("Delay")]
    public bool delayOn = false;
    [Range(0.05f, 1f)] public float delayTime = 0.25f;
    [Range(0f, 0.95f)] public float feedback = 0.6f;
    [Range(0f, 1f)]    public float delayWet = 0.4f;

    [Header("Filter")]
    public bool filterOn = false;
    public AudioFilter.FilterType filterType = AudioFilter.FilterType.LowPass;
    [Range(20f, 20000f)] public float cutoffLow = 1000f;
    [Range(20f, 20000f)] public float cutoffHigh = 5000f;

    [Header("Compressor")]
    public bool compressorOn = false;
    [Range(0.01f, 1f)] public float threshold = 0.5f;
    [Range(1f, 20f)]   public float ratio = 4f;
    [Range(0.001f, 0.1f)] public float compAttack = 0.01f;
    [Range(0.01f, 0.5f)]  public float compRelease = 0.1f;

    void Update()
    {
        // Toggle effects with F-keys
        if (Input.GetKeyDown(KeyCode.F5)) reverbOn = !reverbOn;
        if (Input.GetKeyDown(KeyCode.F6)) delayOn = !delayOn;
        if (Input.GetKeyDown(KeyCode.F7)) filterOn = !filterOn;
        if (Input.GetKeyDown(KeyCode.F8)) compressorOn = !compressorOn;

        // Adjust filter cutoff with up/down arrows when filter is on
        if (filterOn)
        {
            if (Input.GetKey(KeyCode.UpArrow))   cutoffLow = Mathf.Min(cutoffLow + 200f * Time.deltaTime, 20000f);
            if (Input.GetKey(KeyCode.DownArrow)) cutoffLow = Mathf.Max(cutoffLow - 200f * Time.deltaTime, 20f);
        }
    }

    /// <summary>
    /// Process an AudioBuffer through the active effects chain.
    /// </summary>
    public AudioBuffer Process(AudioBuffer dry)
    {
        var buf = dry;

        if (reverbOn)
            buf = new Reverb(roomSize, damping, reverbWet).Process(buf);

        if (delayOn)
            buf = new Delay(delayTime, feedback, delayWet).Process(buf);

        if (filterOn)
            buf = AudioFilter.Apply(buf, filterType, cutoffLow: cutoffLow, cutoffHigh: cutoffHigh);

        if (compressorOn)
            buf = new Compressor(threshold, ratio, compAttack, compRelease).Process(buf);

        return buf;
    }
}
