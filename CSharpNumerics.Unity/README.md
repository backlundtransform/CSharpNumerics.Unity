# CSharpNumerics.Unity — Physics Demo

A minimal Unity 6 demo that uses [CSharpNumerics](https://www.nuget.org/packages/CSharpNumerics/) `PhysicsWorld` to simulate rigid body physics and renders the results with Unity GameObjects.

## What it demonstrates

- **`PhysicsWorld`** — the main simulation orchestrator from `CSharpNumerics.Physics.Applied`
- **`RigidBody`** — solid spheres with automatic inertia tensors
- **Collision detection & response** — sphere-sphere and sphere-plane impulse-based resolution
- **Gravity, restitution, friction** — configurable in the Inspector

## How it works

1. On `Start()`, a `PhysicsWorld` is created with gravity, a static floor body, and several dynamic sphere bodies.
2. On `FixedUpdate()`, `PhysicsWorld.Step(dt)` advances the simulation.
3. On `Update()`, Unity `Transform` positions are synced from the CSharpNumerics body state.

CSharpNumerics uses **Z-up** coordinates; the demo converts to Unity's **Y-up** convention automatically.

## Setup

1. Open the project in **Unity 6** (6000.x).
2. Open `Assets/Scenes/SampleScene`.
3. Go to **CSharpNumerics → Setup Demo Scene** in the menu bar.
4. Press **Play**.

## NuGet package

The `CSharpNumerics.dll` is included in `Assets/Plugins/`. It was downloaded from:
- https://www.nuget.org/packages/CSharpNumerics/

Source code: https://github.com/backlundtransform/CSharpNumerics

## Links

- 📦 [NuGet](https://www.nuget.org/packages/CSharpNumerics/)
- 📂 [CSharpNumerics GitHub](https://github.com/backlundtransform/CSharpNumerics)
- 📘 [Documentation](https://csnumerics.com/docs/Charpnumerics/)
- ⚛️ [Physics docs](https://csnumerics.com/docs/Charpnumerics/Physics/)
- 🎮 [Game Engine docs](https://csnumerics.com/docs/Charpnumerics/Physics/Applied/Game%20Engine)
