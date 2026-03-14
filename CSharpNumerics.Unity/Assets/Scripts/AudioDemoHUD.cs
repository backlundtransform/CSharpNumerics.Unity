using UnityEngine;

/// <summary>
/// Minimal IMGUI HUD showing waveform, BPM, pitch, distance/pan, active effects, and controls.
/// </summary>
public class AudioDemoHUD : MonoBehaviour
{
    private GUIStyle _boxStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _headerStyle;

    void OnGUI()
    {
        EnsureStyles();

        float w = 280f, pad = 10f;

        // ─── Left panel: Synth + Effects ───
        GUILayout.BeginArea(new Rect(pad, pad, w, Screen.height - pad * 2), _boxStyle);

        GUILayout.Label("🎹 SYNTH PAD", _headerStyle);
        var synth = FindObjectOfType<SynthManager>();
        if (synth != null)
        {
            GUILayout.Label($"Waveform: {synth.WaveformName}  [1-4]", _labelStyle);
            GUILayout.Label($"Last note: {synth.LastNotePlayed}  [Q-I]", _labelStyle);
            GUILayout.Label($"ADSR: {synth.attack:F2}/{synth.decay:F2}/{synth.sustain:F2}/{synth.release:F2}", _labelStyle);
        }

        GUILayout.Space(10);
        GUILayout.Label("🎛️ EFFECTS RACK", _headerStyle);
        var fx = FindObjectOfType<EffectsChain>();
        if (fx != null)
        {
            GUILayout.Label($"[F5] Reverb:     {(fx.reverbOn ? "ON" : "OFF")}", _labelStyle);
            GUILayout.Label($"[F6] Delay:      {(fx.delayOn ? "ON" : "OFF")}", _labelStyle);
            GUILayout.Label($"[F7] Filter:     {(fx.filterOn ? "ON" : "OFF")} ({fx.cutoffLow:F0} Hz)", _labelStyle);
            GUILayout.Label($"[F8] Compressor: {(fx.compressorOn ? "ON" : "OFF")}", _labelStyle);
        }

        GUILayout.EndArea();

        // ─── Right panel: Spatial + Beat ───
        GUILayout.BeginArea(new Rect(Screen.width - w - pad, pad, w, Screen.height - pad * 2), _boxStyle);

        GUILayout.Label("🔮 SPATIAL ORB", _headerStyle);
        var orb = FindObjectOfType<OrbSpatializer>();
        if (orb != null)
        {
            GUILayout.Label($"Distance: {orb.CurrentDistance:F1} m", _labelStyle);
            GUILayout.Label($"Pan: {orb.CurrentPan:F2}", _labelStyle);
        }

        GUILayout.Space(10);
        GUILayout.Label("💓 BEAT FLOOR", _headerStyle);
        var beat = FindObjectOfType<BeatPulse>();
        if (beat != null)
        {
            GUILayout.Label($"{beat.DetectedBPM}", _labelStyle);
            GUILayout.Label($"Pitch: {beat.DetectedPitch}", _labelStyle);
            GUILayout.Label("[Space] Toggle beat loop", _labelStyle);
        }

        GUILayout.Space(10);
        GUILayout.Label("📊 SPECTRUM WALL", _headerStyle);
        GUILayout.Label("Real-time FFT visualization", _labelStyle);

        GUILayout.EndArea();

        // ─── Bottom center: Title ───
        float titleW = 500f;
        GUI.Label(
            new Rect((Screen.width - titleW) * 0.5f, Screen.height - 40f, titleW, 30f),
            "CSharpNumerics Audio Engine — Pure C# — No Native Plugins",
            _headerStyle);
    }

    private void EnsureStyles()
    {
        if (_boxStyle != null) return;

        _boxStyle = new GUIStyle("box");
        _boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.7f));
        _boxStyle.padding = new RectOffset(10, 10, 10, 10);

        _labelStyle = new GUIStyle("label");
        _labelStyle.fontSize = 14;
        _labelStyle.normal.textColor = Color.white;
        _labelStyle.wordWrap = true;

        _headerStyle = new GUIStyle("label");
        _headerStyle.fontSize = 16;
        _headerStyle.fontStyle = FontStyle.Bold;
        _headerStyle.normal.textColor = Color.cyan;
        _headerStyle.alignment = TextAnchor.MiddleCenter;
    }

    private static Texture2D MakeTex(int w, int h, Color col)
    {
        var pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        var tex = new Texture2D(w, h);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
}
