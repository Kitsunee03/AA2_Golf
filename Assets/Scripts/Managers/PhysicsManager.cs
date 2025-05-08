using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager Instance;

    [Header("Global Physics Parameters")]
    private readonly float airDensity = 1.225f;
    private readonly float dragCoefficient = 0.47f;
    private readonly float[] rollingFriction = { 1.6f, 0.4f, 2.4f };

    private List<PhysicsObject> bodies = new();
    private List<MeshCollisionComponent> meshes = new();

    private void Awake()
    {
        if (Instance == null) { Instance = this; return; }

        Destroy(gameObject);
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

    private void IntegratePhysics(PhysicsObject body, float dt)
    {
        Vector3 g = Physics.gravity;                          
        Vector3 n = body.CurrentPlaneNormal.normalized;

        if (body.IsGrounded) // ground physics
        {
            // 1) Decompose g into parallel and normal to the plane
            Vector3 gNormal = Vector3.Dot(g, n) * n;    // gNormal = (g·n) n → Fₙ = mg cosθ
            Vector3 gParallel = g - gNormal;            // gParallel = g – gNormal → F∥ = mg sinθ
            body.Velocity += gParallel * dt;

            // 2) Rolling friction with Fn = mg cosθ
            int idx = (int)body.CurrentSurface;
            float mu = rollingFriction[idx]; // coefficient µ according to surface area           

            // calculate cosθ for the normal component
            float cosTheta = Mathf.Abs(Vector3.Dot(g.normalized, n));
            float aRod = mu * g.magnitude * cosTheta; // a_rod = µ·g·cosθ / (1 + I/(m·r²))

            // 3) braking only the component parallel to the plane (rolling)
            Vector3 vPar = Vector3.ProjectOnPlane(body.Velocity, n);
            float speed = vPar.magnitude;
            if (speed > 0f) { vPar = vPar.normalized * Mathf.Max(speed - aRod * dt, 0f); }

            // 4) preserve the normal component intact
            float vN = Vector3.Dot(body.Velocity, n);
            body.Velocity = vPar + n * vN;
        }
        else // in air physics
        {
            // 1) vertical gravity + air resistance if y>1m
            body.Velocity += g * dt;
            if (body.Position.y > 1f)
            {
                float area = Mathf.PI * body.Radius * body.Radius;
                float speedAir = body.Velocity.magnitude;
                if (speedAir > 0.01f)
                {
                    float dragAcc = 0.5f * airDensity * dragCoefficient
                                  * area * speedAir * speedAir
                                  / body.Mass;
                    float dv = Mathf.Min(dragAcc * dt, speedAir);
                    body.Velocity = body.Velocity.normalized * (speedAir - dv);
                }
            }
        }

        // 2) Integrate contact point position
        body.Position += body.Velocity * dt;
    }

    private void HandleCollisions(PhysicsObject p_body)
    {
        bool hasCollided = false;

        foreach (MeshCollisionComponent mesh in meshes)
        {
            if (mesh == null) { continue; } // mesh destroyed check

            // mesh near check
            float meshDistance = Vector3.Distance(p_body.Position, mesh.Center);
            float maxRange = mesh.BoundingRadius + p_body.Radius; // precomputed mesh bounding radius
            if (meshDistance > maxRange) { continue; } // mesh too far, skip

            // mesh triangles iteration
            int triIndex = 0;
            foreach ((Vector3 a, Vector3 b, Vector3 c) in mesh.GetWorldTriangles())
            {
                Vector3 n = Vector3.Cross(b - a, c - a).normalized;
                float dist = Vector3.Dot(p_body.Position - a, n);

                if (Mathf.Abs(dist) > p_body.Radius)
                {
                    triIndex++;
                    continue;
                }

                Vector3 p = p_body.Position - n * dist;
                if (IsPointInTriangle(p, a, b, c))
                {
                    if (Vector3.Dot(p_body.Velocity, n) > 0f) { n = -n; }

                    p_body.Position += n * (p_body.Radius - Mathf.Abs(dist));

                    float vN = Vector3.Dot(p_body.Velocity, n);
                    if (vN < 0f)
                    {
                        Vector3 vNormal = vN * n;
                        Vector3 vTangent = p_body.Velocity - vNormal;
                        Vector3 vNormalOut = -vNormal * mesh.Restitution; // restitution coefficient application
                        p_body.Velocity = vTangent + vNormalOut;
                    }

                    p_body.OnCollision(mesh.Surface, n);

                    // hole triangle check
                    if (mesh.IsHoleTriangle(triIndex) && !p_body.ObjectIsInMotion && p_body.IsGrounded)
                    {
                        Debug.Log("Enter hole");
                        GameManager.Instance.LoadLevel();
                    }

                    hasCollided = true;
                }

                triIndex++;
            }
        }

        p_body.IsGrounded = hasCollided;
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
        if (Mathf.Abs(denom) < 1e-6f) { return false; } // invalid triangle check (prevent Unity doing weird things)

        float u = (d11 * d20 - d01 * d21) / denom;
        float v = (d00 * d21 - d01 * d20) / denom;

        return (u >= 0) && (v >= 0) && (u + v <= 1);
    }
}