using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager Instance;

    [Header("Global Physics Parameters")]
    [SerializeField, Tooltip("Air density (kg/m³)")] private float airDensity = 1.225f;
    [SerializeField, Tooltip("Drag coefficient for sphere")] private float dragCoefficient = 0.47f;

    [Header("Rolling Friction Coefficients (0=Grass,1=Ice,2=Sand)")]
    [SerializeField] private float[] rollingFriction = { 0.4f, 0.1f, 0.6f };

    private List<PhysicsObject> bodies = new();
    private List<MeshCollisionComponent> meshes = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    #region Object registration
    public void RegisterPhysicsObject(PhysicsObject p_body)
    {
        if (!bodies.Contains(p_body)) bodies.Add(p_body);
    }   
    public void RegisterMeshCollider(MeshCollisionComponent p_meshColCom)
    {
        if (!meshes.Contains(p_meshColCom)) meshes.Add(p_meshColCom);
    }
    #endregion

    public void ApplyForce(PhysicsObject p_body, Vector3 p_force)
    {
        p_body.Velocity += p_force / p_body.Mass;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        foreach (PhysicsObject body in bodies)
        {
            IntegratePhysics(body, dt);
            HandleCollisions(body);
            body.ApplyTransform();
        }
    }

    private void IntegratePhysics(PhysicsObject p_body, float p_dt)
    {
        // 1) gravity
        p_body.Velocity += Physics.gravity * p_dt;

        // 2) air resistance (if y > 1m)
        if (p_body.Position.y > 1f)
        {
            float area = Mathf.PI * p_body.Radius * p_body.Radius;
            float speed = p_body.Velocity.magnitude;

            if (speed > 0.01f)
            {
                float dragAccMag = 0.5f
                    * airDensity
                    * dragCoefficient
                    * area
                    * speed * speed
                    / p_body.Mass;
                float dv = Mathf.Min(dragAccMag * p_dt, speed);
                p_body.Velocity = p_body.Velocity.normalized * (speed - dv);
            }
        }

        // 3) frictional break (I = 2/5·m·r²)
        float groundThreshold = p_body.Radius + 0.01f;
        if (p_body.Position.y <= groundThreshold)
        {
            int idx = (int)p_body.CurrentSurface;
            float mu = rollingFriction[idx];

            // body inertia: I = 0.4·m·r²
            float I = p_body.Inertia;  
            float denom = 1f + I / (p_body.Mass * p_body.Radius * p_body.Radius);
            
            // a = μ·g / (1 + I/(m·r²))
            float decel = mu * Physics.gravity.magnitude / denom;

            Vector3 vH = new(p_body.Velocity.x, 0f, p_body.Velocity.z);
            float speedH = vH.magnitude;
            if (speedH > 0f)
            {
                float newH = Mathf.Max(speedH - decel * p_dt, 0f);
                vH = vH.normalized * newH;
            }

            p_body.Velocity = new Vector3(vH.x, p_body.Velocity.y, vH.z);
        }

        // 4) new position
        p_body.Position += p_body.Velocity * p_dt;
    }

    private void HandleCollisions(PhysicsObject p_body)
    {
        // mesh collision
        foreach (MeshCollisionComponent mesh in meshes)
        {
            foreach ((Vector3 a, Vector3 b, Vector3 c) in mesh.GetWorldTriangles())
            {
                Vector3 n = Vector3.Cross(b - a, c - a).normalized;
                float dist = Vector3.Dot(p_body.Position - a, n);

                // check if the body is above the plane
                if (Mathf.Abs(dist) > p_body.Radius) { continue; }

                // projected body position
                Vector3 p = p_body.Position - n * dist;

                // inside triangle check
                if (IsPointInTriangle(p, a, b, c))
                {
                    // correct normal direction if the ball comes from the other side
                    if (Vector3.Dot(p_body.Velocity, n) > 0f) { n = -n; }

                    // correct position
                    p_body.Position += n * (p_body.Radius - Mathf.Abs(dist));

                    // normal component bounce
                    float vN = Vector3.Dot(p_body.Velocity, n);
                    if (vN < 0f)
                    {
                        Vector3 vNormal = vN * n;
                        Vector3 vTangent = p_body.Velocity - vNormal;
                        Vector3 vNormalOut = -vNormal * mesh.Restitution;
                        p_body.Velocity = vTangent + vNormalOut;
                    }

                    // register collision
                    p_body.OnCollision(mesh.Surface, n, mesh.Restitution);
                }
            }
        }
    }

    private bool IsPointInTriangle(Vector3 p_point, Vector3 p_a, Vector3 p_b, Vector3 p_c)
    {
        Vector3 v0 = p_c - p_a;
        Vector3 v1 = p_b - p_a;
        Vector3 v2 = p_point - p_a;

        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;
        if (Mathf.Abs(denom) < 1e-6f) { return false; } // invalid triangle check

        float u = (d11 * d20 - d01 * d21) / denom;
        float v = (d00 * d21 - d01 * d20) / denom;

        return (u >= 0) && (v >= 0) && (u + v <= 1);
    }


    public float[] RollingFriction => rollingFriction;
}