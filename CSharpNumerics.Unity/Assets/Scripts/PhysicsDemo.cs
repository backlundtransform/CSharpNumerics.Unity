using UnityEngine;
using CSharpNumerics.Physics;
using CSharpNumerics.Physics.Applied;
using CSharpNumerics.Physics.Applied.Objects;
using CSharpNumerics.Physics.Objects;
using CSVector = CSharpNumerics.Numerics.Objects.Vector;

/// <summary>
/// Demo using CSharpNumerics RigidBody + Velocity Verlet integration
/// with manual floor collision and sphere-sphere collision response.
/// </summary>
public class PhysicsDemo : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int ballCount = 5;
    [SerializeField] private float spawnHeight = 6f;
    [SerializeField] private float spawnSpread = 1.5f;
    [SerializeField] private float ballRadius = 0.7f;
    [SerializeField] private float ballMass = 1f;
    [SerializeField] private float arenaHalf = 4f;

    [Header("Physics Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float restitution = 0.8f;
    [SerializeField] private float frictionCoeff = 0.3f;

    private RigidBody[] _bodies;
    private GameObject[] _ballObjects;
    private double _gravityD;

    void Start()
    {
        _gravityD = gravity;

        // ── Visual floor (matches arena size) ──
        var floorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floorGo.name = "Floor";
        floorGo.transform.position = new Vector3(0, -0.025f, 0);
        floorGo.transform.localScale = new Vector3(arenaHalf * 2, 0.05f, arenaHalf * 2);
        floorGo.GetComponent<Renderer>().material = CreateURPMat(new Color(0.15f, 0.15f, 0.2f));
        Destroy(floorGo.GetComponent<Collider>());

        // ── Spawn balls ──
        _bodies = new RigidBody[ballCount];
        _ballObjects = new GameObject[ballCount];

        for (int i = 0; i < ballCount; i++)
        {
            float x = Random.Range(-spawnSpread, spawnSpread);
            float y = Random.Range(-spawnSpread, spawnSpread);
            float z = spawnHeight + Random.Range(0f, 4f);

            var body = RigidBody.CreateSolidSphere(mass: ballMass, radius: ballRadius);
            body.Position = new CSVector(x, y, z);
            body.Velocity = new CSVector(0, 0, 0); // drop straight down
            _bodies[i] = body;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Ball_{i}";
            go.transform.localScale = Vector3.one * ballRadius * 2f;
            // Bright distinct colors: red, green, blue, yellow, magenta
            Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta,
                               Color.cyan, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f) };
            go.GetComponent<Renderer>().material = CreateURPMat(colors[i % colors.Length]);
            Destroy(go.GetComponent<Collider>());
            _ballObjects[i] = go;
        }

        Debug.Log($"CSharpNumerics PhysicsDemo: {ballCount} balls, gravity={gravity}");
    }

    void FixedUpdate()
    {
        if (_bodies == null) return;

        double dt = Time.fixedDeltaTime;

        // Gravity + Verlet integration using CSharpNumerics RigidBody
        System.Func<RigidBody, (CSVector force, CSVector torque)> forceFunc = b =>
        {
            var gForce = new CSVector(0, 0, _gravityD * b.Mass);
            return (gForce, new CSVector(0, 0, 0));
        };

        for (int i = 0; i < _bodies.Length; i++)
        {
            _bodies[i].IntegrateVelocityVerlet(forceFunc, dt);
        }

        // ── Floor collision (plane at z=0) ──
        for (int i = 0; i < _bodies.Length; i++)
        {
            double r = ballRadius;
            double px = _bodies[i].Position.x;
            double py = _bodies[i].Position.y;
            double pz = _bodies[i].Position.z;
            double vx = _bodies[i].Velocity.x;
            double vy = _bodies[i].Velocity.y;
            double vz = _bodies[i].Velocity.z;

            // Floor bounce
            if (pz < r)
            {
                pz = r;
                if (vz < 0) vz = -vz * restitution;
            }

            // Wall bounces (keep balls inside arena)
            double wall = arenaHalf - r;
            if (px > wall)  { px = wall;  if (vx > 0) vx = -vx * restitution; }
            if (px < -wall) { px = -wall; if (vx < 0) vx = -vx * restitution; }
            if (py > wall)  { py = wall;  if (vy > 0) vy = -vy * restitution; }
            if (py < -wall) { py = -wall; if (vy < 0) vy = -vy * restitution; }

            _bodies[i].Position = new CSVector(px, py, pz);
            _bodies[i].Velocity = new CSVector(vx, vy, vz);
        }

        // ── Sphere-sphere collisions using CSharpNumerics ──
        for (int i = 0; i < _bodies.Length; i++)
        {
            for (int j = i + 1; j < _bodies.Length; j++)
            {
                var sA = new CSharpNumerics.Physics.Applied.Objects.BoundingSphere(_bodies[i].Position, ballRadius);
                var sB = new CSharpNumerics.Physics.Applied.Objects.BoundingSphere(_bodies[j].Position, ballRadius);
                var contact = sA.SphereSphereContact(sB);

                if (contact is CSharpNumerics.Physics.Applied.Objects.ContactPoint c)
                {
                    CollisionResponse.ResolveCollision(
                        ref _bodies[i], ref _bodies[j], c,
                        restitution: restitution,
                        friction: frictionCoeff);

                    CollisionResponse.CorrectPositions(
                        ref _bodies[i], ref _bodies[j], c,
                        correctionFraction: 0.4,
                        slop: 0.01);
                }
            }
        }
    }

    void Update()
    {
        if (_bodies == null || _ballObjects == null) return;

        for (int i = 0; i < _bodies.Length; i++)
        {
            _ballObjects[i].transform.position = CSToUnity(_bodies[i].Position);
        }
    }

    /// <summary>
    /// CSharpNumerics Z-up → Unity Y-up.
    /// </summary>
    private static Vector3 CSToUnity(CSVector v)
    {
        return new Vector3((float)v.x, (float)v.z, (float)v.y);
    }

    /// <summary>
    /// Create a new URP Lit material with the given color.
    /// Falls back to Standard shader if URP Lit not found.
    /// </summary>
    private static Material CreateURPMat(Color color)
    {
        // Try URP Lit first, then fallback to Standard
        var shader = Shader.Find("Universal Render Pipeline/Lit")
                  ?? Shader.Find("Standard");

        var mat = new Material(shader);
        // URP uses _BaseColor, Standard uses _Color
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
        return mat;
    }
}
