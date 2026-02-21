using UnityEngine;
using CSharpNumerics.Physics.Applied;
using CSharpNumerics.Physics.Objects;
using CSVector = CSharpNumerics.Vector;

/// <summary>
/// Simple demo that uses CSharpNumerics PhysicsWorld to simulate
/// bouncing spheres on a floor, rendered by Unity GameObjects.
/// </summary>
public class PhysicsDemo : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int ballCount = 5;
    [SerializeField] private float spawnHeight = 10f;
    [SerializeField] private float spawnSpread = 4f;
    [SerializeField] private float ballRadius = 0.5f;
    [SerializeField] private float ballMass = 1f;

    [Header("Physics Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float restitution = 0.7f;
    [SerializeField] private float friction = 0.3f;
    [SerializeField] private int solverIterations = 10;

    private PhysicsWorld _world;
    private GameObject[] _ballObjects;
    private int[] _bodyIndices;

    void Start()
    {
        // ── Create CSharpNumerics physics world ──
        // CSharpNumerics uses Z-up; we map Z→Y for Unity later
        _world = new PhysicsWorld
        {
            Gravity = new CSVector(0, 0, gravity),
            DefaultRestitution = restitution,
            DefaultFriction = friction,
            SolverIterations = solverIterations,
            FixedTimeStep = Time.fixedDeltaTime,
        };

        // ── Static floor at z=0 ──
        var floor = RigidBody.CreateStatic(new CSVector(0, 0, 0));
        _world.AddBody(floor, boundingRadius: 100);

        // Visual floor
        var floorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floorGo.name = "Floor";
        floorGo.transform.position = new Vector3(0, -0.5f, 0);
        floorGo.transform.localScale = new Vector3(20, 1, 20);
        floorGo.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
        // Remove Unity's collider — we use CSharpNumerics for physics
        Destroy(floorGo.GetComponent<Collider>());

        // ── Spawn bouncing balls ──
        _ballObjects = new GameObject[ballCount];
        _bodyIndices = new int[ballCount];

        for (int i = 0; i < ballCount; i++)
        {
            // Random start position (CSharpNumerics Z = up)
            float x = Random.Range(-spawnSpread, spawnSpread);
            float y = Random.Range(-spawnSpread, spawnSpread);
            float z = spawnHeight + Random.Range(0f, 5f);

            var body = RigidBody.CreateSolidSphere(mass: ballMass, radius: ballRadius);
            body.Position = new CSVector(x, y, z);
            body.Velocity = new CSVector(
                Random.Range(-2f, 2f),
                Random.Range(-2f, 2f),
                0);

            _bodyIndices[i] = _world.AddBody(body, boundingRadius: ballRadius);

            // Visual sphere
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Ball_{i}";
            go.transform.localScale = Vector3.one * ballRadius * 2f;
            go.GetComponent<Renderer>().material.color = Color.HSVToRGB(
                (float)i / ballCount, 0.8f, 1f);
            Destroy(go.GetComponent<Collider>());

            _ballObjects[i] = go;
        }
    }

    void FixedUpdate()
    {
        // Step the CSharpNumerics physics
        _world.Step(dt: Time.fixedDeltaTime);
    }

    void Update()
    {
        // Sync Unity transforms from CSharpNumerics state
        // CSharpNumerics: X right, Y forward, Z up
        // Unity:          X right, Y up,      Z forward
        for (int i = 0; i < ballCount; i++)
        {
            ref var body = ref _world.Body(_bodyIndices[i]);
            _ballObjects[i].transform.position = CSToUnity(body.Position);
        }
    }

    /// <summary>
    /// Convert CSharpNumerics Vector (Z-up) → Unity Vector3 (Y-up).
    /// </summary>
    private static Vector3 CSToUnity(CSVector v)
    {
        return new Vector3((float)v[0], (float)v[2], (float)v[1]);
    }
}
