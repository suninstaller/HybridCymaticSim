using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaveSolver : MonoBehaviour
{
    [Header("Simulation")]
    public int resolution = 128;               // grid cells per side
    public float baseDepth = 0.05f;            // metres
    public float viscosity = 0.02f;            // 0 = no damping
    public float damping = 0.99f;              // per-frame mul
    public float waveSpeed = 3f;               // m/s
    public float dt = 1f / 60f;                // fixed to 60 Hz

    [Header("Excitation")]
    public KeyCode exciteKey = KeyCode.Space;
    public float exciteRadius = 5;             // grid units
    public float exciteStrength = 0.5f;

    [Header("Real-time excitation")]
    public float exciteFreq = 5f;              // Hz
    public float exciteAmp = 0.2f;             // metres
    public bool useContinuous = true;

    float[,] u, uPrev, uNext;                  // heightfields
    float dx;                                  // world-space cell size
    Mesh mesh;
    float phase;                               // running phase
    bool wasContinuous;                        // detect slider changes

    void OnEnable()
    {
        Init();
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        if (resolution < 2)
            resolution = 2;
        dx = 1f / resolution;
        u = new float[resolution, resolution];
        uPrev = (float[,])u.Clone();
        uNext = (float[,])u.Clone();

        if (mesh == null)
            mesh = new Mesh { name = "CymaticMesh" };
        mesh.MarkDynamic();
        BuildFlatMesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void BuildFlatMesh()
    {
        Vector3[] v = new Vector3[resolution * resolution];
        int[] tri = new int[(resolution - 1) * (resolution - 1) * 6];
        Vector2[] uv = new Vector2[v.Length];

        for (int y = 0, i = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
            {
                v[i] = new Vector3(x * dx, 0, y * dx);
                uv[i] = new Vector2(x / (float)(resolution - 1), y / (float)(resolution - 1));
            }
        }

        int t = 0;
        for (int y = 0; y < resolution - 1; y++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int i = y * resolution + x;
                tri[t++] = i;
                tri[t++] = i + resolution;
                tri[t++] = i + 1;

                tri[t++] = i + 1;
                tri[t++] = i + resolution;
                tri[t++] = i + resolution + 1;
            }
        }

        mesh.Clear();
        mesh.vertices = v;
        mesh.uv = uv;
        mesh.triangles = tri;
        mesh.RecalculateNormals();
    }

    void Update()
    {
        if (useContinuous && exciteAmp > 0)
        {
            phase += 2f * Mathf.PI * exciteFreq * Time.deltaTime;
            float t = Mathf.Sin(phase) * exciteAmp * baseDepth;
            int cx = resolution / 2;
            int cy = resolution / 2;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d <= exciteRadius)
                        u[x, y] += t * Mathf.Cos(d / exciteRadius * Mathf.PI * 0.5f);
                }
            }
        }

        if (IsExcitePressed())
            ExciteCenter();

        // 2-D wave eq:  ∂²u/∂t² = c²(∂²u/∂x² + ∂²u/∂y²) - k∂u/∂t
        float c2 = waveSpeed * waveSpeed;
        float r = c2 * dt * dt / (dx * dx);
        float k = viscosity * dt;

        for (int y = 1; y < resolution - 1; y++)
        {
            for (int x = 1; x < resolution - 1; x++)
            {
                float laplacian = u[x + 1, y] + u[x - 1, y] + u[x, y + 1] + u[x, y - 1] - 4 * u[x, y];
                float vel = (u[x, y] - uPrev[x, y]) / dt;
                uNext[x, y] = 2 * u[x, y] - uPrev[x, y] + r * laplacian - k * vel;
                uNext[x, y] *= damping;
            }
        }

        var tmp = uPrev; uPrev = u; u = uNext; uNext = tmp;
        ApplyToMesh();
    }

    void ExciteCenter()
    {
        int cx = resolution / 2;
        int cy = resolution / 2;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (d <= exciteRadius)
                    u[x, y] += exciteStrength * Mathf.Cos(d / exciteRadius * Mathf.PI * 0.5f);
            }
        }
    }

    void ApplyToMesh()
    {
        Vector3[] v = mesh.vertices;
        for (int y = 0, i = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
                v[i].y = u[x, y] * baseDepth;
        }
        mesh.vertices = v;
        mesh.RecalculateNormals();
    }

    bool IsExcitePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null) return false;
        if (!Enum.TryParse(exciteKey.ToString(), out Key key)) key = Key.Space;
        var k = Keyboard.current[key];
        return k != null && k.wasPressedThisFrame;
#else
        return Input.GetKeyDown(exciteKey);
#endif
    }
}
