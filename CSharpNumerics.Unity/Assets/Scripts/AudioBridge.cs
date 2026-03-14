using UnityEngine;
using CSharpNumerics.Engines.Audio;

/// <summary>
/// Converts CSharpNumerics AudioBuffer to Unity AudioClip and provides playback helpers.
/// </summary>
public static class AudioBridge
{
    public static AudioClip ToClip(AudioBuffer buf, string name = "clip")
    {
        float[] samples = new float[buf.Samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            double s = buf.Samples[i];
            samples[i] = (float)(s < -1.0 ? -1.0 : s > 1.0 ? 1.0 : s);
        }

        var clip = AudioClip.Create(name, buf.FrameCount, buf.Channels, buf.SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public static void PlayOneShot(AudioSource source, AudioBuffer buf, string name = "clip")
    {
        source.PlayOneShot(ToClip(buf, name));
    }

    public static void PlayLoop(AudioSource source, AudioBuffer buf, string name = "loop")
    {
        source.clip = ToClip(buf, name);
        source.loop = true;
        source.Play();
    }
}
