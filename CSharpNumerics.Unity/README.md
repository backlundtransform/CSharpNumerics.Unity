# CSharpNumerics.Unity — Audio & Physics Demo

Unity 6 demos showcasing [CSharpNumerics](https://www.nuget.org/packages/CSharpNumerics/) **Audio Engine** and **Game Engine** — pure C#, no native plugins.

## 🎧 Audio Engine Demo

A dark neon 3D scene with five interactive zones:

| Element | Script | CSharpNumerics API |
|---|---|---|
| **Synth Pad** | `SynthManager.cs` | `Synthesizer`, `AudioOscillator`, `Envelope` |
| **Spectrum Wall** | `SpectrumVisualizer.cs` | `SpectrumAnalyzer` (FFT) |
| **Floating Orb** | `OrbSpatializer.cs` | `SpatialAudio.Spatialize()` |
| **Effects Rack** | `EffectsChain.cs` | `Reverb`, `Delay`, `AudioFilter`, `Compressor` |
| **Beat Floor** | `BeatPulse.cs` | `BeatDetector`, `PitchDetector` |

### Controls

| Key | Action |
|---|---|
| Q–I | Play notes C4–C5 |
| 1–4 | Switch waveform (Sine/Square/Sawtooth/Triangle) |
| F5–F8 | Toggle Reverb / Delay / Filter / Compressor |
| ↑/↓ | Adjust filter cutoff |
| Space | Change beat loop BPM |

### Setup

1. Open in **Unity 6** (6000.x)
2. **CSharpNumerics → Setup Audio Demo Scene**
3. Press **Play**

## ⚛️ Physics Demo

Bouncing spheres with `RigidBody`, Velocity Verlet integration, sphere-sphere collision response, and wall boundaries.

### Setup

1. **CSharpNumerics → Setup Demo Scene**
2. Press **Play**

## NuGet

CSharpNumerics 2.6.1 (`netstandard2.1`) — `Assets/Plugins/CSharpNumerics.dll`

## Links

- 📦 [NuGet](https://www.nuget.org/packages/CSharpNumerics/)
- 📂 [GitHub](https://github.com/backlundtransform/CSharpNumerics)
- 📘 [Documentation](https://csnumerics.com)
- 🎧 [Audio Engine docs](https://csnumerics.com/docs/Csharpnumerics/Simulation%20Engines/Audio%20Engine/)
- 🎮 [Game Engine docs](https://csnumerics.com/docs/Csharpnumerics/Simulation%20Engines/Game%20Engine/)
