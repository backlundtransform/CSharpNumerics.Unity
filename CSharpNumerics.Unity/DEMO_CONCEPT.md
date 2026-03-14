# 🎬 Audio Engine — Unity 3D Demo Concept

**Format:** Video / screencast, 5–10 min  
**Motor:** Unity 3D (2022 LTS+)  
**Mål:** Visa att CSharpNumerics Audio Engine kan driva realtidsljud, visualisering och spatial audio i en 3D-motor.

---

## Scenöversikt

En mörk 3D-scen med neonlysande objekt. Tre zoner i rummet:

```
┌─────────────────────────────────────────────────┐
│                                                 │
│   🎹 SYNTH PAD        📊 SPECTRUM WALL         │
│   (vänster)           (bakvägg)                 │
│                                                 │
│            🔮 ORB                               │
│         (mitten, rör sig)                       │
│                                                 │
│   🎛️ EFFECTS RACK     💓 BEAT FLOOR            │
│   (höger)             (golvet pulsar)           │
│                                                 │
│              🎧 LISTENER (kamera/FPS)           │
└─────────────────────────────────────────────────┘
```

| Element | Unity-objekt | Audio Engine-klass |
|---|---|---|
| Synth Pad | UI-panel med tangenter | `Synthesizer`, `AudioOscillator`, `Envelope` |
| Floating Orb | Sphere + emissive material | `SpatialAudio.Spatialize()` |
| Spectrum Wall | Mesh-bars (eller LineRenderer) | `SpectrumAnalyzer` |
| Effects Rack | UI-sliders | `Reverb`, `Delay`, `AudioFilter`, `Compressor` |
| Beat Floor | Plane + shader | `BeatDetector` |

---

## Flöde & Tidslinje

### 0:00–0:30 — Intro & Setup
- Visa titelskärm: *"CSharpNumerics Audio Engine — Unity Demo"*
- Kort textöverlägg: "Pure C# — inga native plugins — allt beräknas i realtid"
- Kameran flyger in i scenen (cinemachine dolly)

### 0:30–2:00 — Synth Pad: Realtidsyntes (★ Huvuddemo)
**Visa:** Spela toner live genom att trycka på tangenter (Q-W-E-R-T = C-D-E-F-G)

```csharp
// SynthManager.cs — attached to Synth Pad
void PlayNote(float freq)
{
    var synth = new Synthesizer { SampleRate = 44100 };
    synth.AddVoice(new AudioOscillator(Waveform.Sine, freq, 1.0));
    synth.AddVoice(new AudioOscillator(Waveform.Sine, freq * 2, 0.3), gain: 0.3);
    synth.Envelope = new Envelope(0.02, 0.1, 0.7, 0.3);

    var buf = synth.Render(duration: 1.5, noteOffTime: 1.0);
    PlayBuffer(buf); // → AudioClip → AudioSource
}
```

- Visuellt: Oscilloskop-linje (LineRenderer) som visar vågformen i realtid
- Byt vågform live med knapptryck (Sine → Square → Sawtooth) — visa hur ljudet ändras
- Tweak:a ADSR med sliders → visa envelope-kurvan ovanför tangentbordet

### 2:00–3:30 — Spektrumvägg: FFT-visualisering
**Visa:** Spectrum bars som reagerar på ljudet

```csharp
// SpectrumVisualizer.cs
var analyzer = new SpectrumAnalyzer(2048, SpectrumAnalyzer.WindowType.Hann);
var spectrum = analyzer.Analyze(currentSamples, 44100);

for (int i = 0; i < bars.Length; i++)
{
    float height = (float)spectrum[i].Magnitude * scale;
    bars[i].localScale = new Vector3(1, height, 1);
    bars[i].GetComponent<Renderer>().material.color =
        Color.Lerp(Color.cyan, Color.magenta, height / maxHeight);
}
```

- Spela olika vågformer → visa skillnaden i spektrum (Sine = en topp, Square = udda övertoner, Sawtooth = alla)
- Zooma in kameran mot väggen, peka ut frekvensaxeln

### 3:30–5:00 — Floating Orb: Spatial Audio
**Visa:** En lysande sfär som flyger runt i 3D → ljudet panorerar och avtar med avstånd

```csharp
// OrbSpatializer.cs — varje frame
var spatialized = SpatialAudio.Spatialize(
    monoBuffer,
    sourceX: orb.transform.position.x,
    sourceY: orb.transform.position.z,
    listenerX: listener.transform.position.x,
    listenerY: listener.transform.position.z
);
```

- Orben cirklar runt lyssnaren → ljud vandrar L↔R
- Orben flyger långt bort → ljudet avtar (inverse distance)
- Visa avståndsvärde + pan-värde som HUD-overlay
- Kameran följer med i FPS-läge så tittaren "hör" perspektivet

### 5:00–6:30 — Effects Rack: Live-effekter
**Visa:** UI-panel med 4 sliders: Reverb, Delay, Filter, Compressor

```csharp
// EffectsChain.cs
AudioBuffer Process(AudioBuffer dry)
{
    var buf = dry;
    if (reverbOn)
        buf = new Reverb(roomSize, damping, reverbWet).Process(buf);
    if (delayOn)
        buf = new Delay(delayTime, feedback, delayWet).Process(buf);
    if (filterOn)
        buf = AudioFilter.Apply(buf, filterType, cutoffLow, cutoffHigh);
    if (compressorOn)
        buf = new Compressor(threshold, ratio, 0.01, 0.1).Process(buf);
    return buf;
}
```

- Börja med torrt ljud → dra upp Reverb (stor hall-känsla)
- Lägg till Delay (eko) → visa feedback-knapp
- Slå på LowPass filter → dra cutoff ner → "under vatten"-effekt
- Compressor → visa hur dynamiken plattas ut

### 6:30–8:00 — Beat Floor: Onset Detection
**Visa:** Spela en enkel rytmisk loop → golvet pulsar i takt

```csharp
// BeatPulse.cs
var detector = new BeatDetector(1024) { Threshold = 1.5 };
List<double> onsets = detector.Detect(loopBuffer);
double bpm = detector.EstimateTempo(loopBuffer);

// Visa BPM i HUD
bpmText.text = $"{bpm:F0} BPM";

// Trigga golvpuls vid varje onset
foreach (var t in onsets)
    SchedulePulse(t); // scale + emission flash
```

- Golv-shader blinkar/pulserar vid varje detekterad onset
- Visa BPM-siffra i hörnet
- Byt till snabbare loop → BPM uppdateras, pulser tightare
- Visa pitch detection: `PitchDetector` identifierar grundtonen → text "A4 = 440 Hz"

### 8:00–8:30 — Kodöversikt
- Snabb split-screen: Unity-scenen till vänster, koden till höger
- Scrolla genom `SynthManager.cs` — visa hur kort integrationen är
- Poängtera: "Inget native-plugin, ren C#, cross-platform"

### 8:30–9:00 — Outro
- Kameran drar sig bakåt, alla element aktiva samtidigt
- Text overlay: API-sammanfattning

```
✅ Synthesizer — additive multi-voice synth
✅ AudioFilter — LP/HP/BP via FFT
✅ Reverb / Delay / Compressor — studio-grade FX
✅ SpatialAudio — 3D panning & attenuation
✅ SpectrumAnalyzer — real-time FFT visualization
✅ BeatDetector — onset & tempo detection
✅ PitchDetector — autocorrelation & HPS
```

- Länk till repo / NuGet

---

## Unity-glue: AudioBuffer → AudioClip

Nyckelfunktionen som kopplar CSharpNumerics till Unity:

```csharp
public static class AudioBridge
{
    public static AudioClip ToClip(AudioBuffer buf, string name = "clip")
    {
        float[] samples = new float[buf.Samples.Length];
        for (int i = 0; i < samples.Length; i++)
            samples[i] = (float)Math.Clamp(buf.Samples[i], -1.0, 1.0);

        var clip = AudioClip.Create(name, buf.FrameCount, buf.Channels, buf.SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public static void PlayOneShot(AudioSource source, AudioBuffer buf)
    {
        source.PlayOneShot(ToClip(buf));
    }
}
```

---

## Visuell stil

| Element | Stil |
|---|---|
| Bakgrund | Mörk (#0a0a0f), subtle grid |
| Spectrum bars | Cyan → magenta gradient |
| Orb | Emissive sphere, bloom post-processing |
| Beat floor | Emissive plane, pulse = scale + HDR emission |
| UI | Minimal, semi-transparent panels, monospace font |
| Post-processing | Bloom, vignette, chromatic aberration (subtil) |

**Unity packages:** URP/HDRP, Cinemachine, TextMeshPro

---

## Förberedelser / Checklista

- [ ] Nytt Unity-projekt (URP), importera CSharpNumerics DLL
- [ ] `AudioBridge.cs` — buffer → AudioClip helper
- [ ] `SynthManager.cs` — tangentbord → synth
- [ ] `SpectrumVisualizer.cs` — bars mesh + SpectrumAnalyzer
- [ ] `OrbSpatializer.cs` — orb movement + SpatialAudio
- [ ] `EffectsChain.cs` — UI sliders → reverb/delay/filter/compressor
- [ ] `BeatPulse.cs` — BeatDetector → floor shader
- [ ] Post-processing profil (bloom, vignette)
- [ ] Cinemachine-kamera (dolly intro + FPS mode)
- [ ] OBS / Unity Recorder redo
