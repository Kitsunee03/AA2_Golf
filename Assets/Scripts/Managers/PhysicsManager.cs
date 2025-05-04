// PhysicsManager.cs
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

    private List<PhysicsObject> bodies = new List<PhysicsObject>();
    private List<CollisionPlaneComponent> planes = new List<CollisionPlaneComponent>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPhysicsObject(PhysicsObject body)
    {
        if (!bodies.Contains(body)) bodies.Add(body);
    }

    public void RegisterCollisionPlane(CollisionPlaneComponent plane)
    {
        if (!planes.Contains(plane)) planes.Add(plane);
    }

    public void ApplyForce(PhysicsObject body, Vector3 force)
    {
        body.Velocity += force / body.Mass;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;   
        foreach (var body in bodies)
        {
            IntegratePhysics(body, dt);
            HandleCollisions(body);
            body.ApplyTransform();
        }
    }

    private void IntegratePhysics(PhysicsObject body, float dt)
    {
        // 1) Gravedad
        body.Velocity += Physics.gravity * dt;

        // 2) Resistencia del aire (si y>1m), sin invertir la velocidad
        if (body.Position.y > 1f)
        {
            float area = Mathf.PI * body.Radius * body.Radius;
            float speed = body.Velocity.magnitude;

            if (speed > 0.01f)
            {
                float dragAccMag = 0.5f
                    * airDensity
                    * dragCoefficient
                    * area
                    * speed * speed
                    / body.Mass;
                float dv = Mathf.Min(dragAccMag * dt, speed);
                body.Velocity = body.Velocity.normalized * (speed - dv);
            }
        }

        // 3) Fricción de rodadura sólo en horizontal
        float groundThreshold = body.Radius + 0.01f;
        if (body.Position.y <= groundThreshold)
        {
            int idx = (int)body.CurrentSurface;
            float mu = rollingFriction[idx];
            float decel = mu * Physics.gravity.magnitude;

            Vector3 vH = new Vector3(body.Velocity.x, 0f, body.Velocity.z);
            float speedH = vH.magnitude;
            if (speedH > 0f)
            {
                float newH = Mathf.Max(speedH - decel * dt, 0f);
                vH = vH.normalized * newH;
            }

            body.Velocity = new Vector3(vH.x, body.Velocity.y, vH.z);
        }

        // 4) Integración de posición
        body.Position += body.Velocity * dt;
    }

    private void HandleCollisions(PhysicsObject body)
    {
        foreach (var plane in planes)
        {
            Vector3 n = plane.WorldNormal;
            float dist = Vector3.Dot(body.Position - plane.PointOnPlane, n);
            if (dist < body.Radius && plane.IsPointInsideBounds(body.Position))
            {
                // Corrección de penetración
                body.Position += n * (body.Radius - dist);

                // Rebote avanzado: sólo en la componente normal
                float vN = Vector3.Dot(body.Velocity, n);
                if (vN < 0f)
                {
                    Vector3 vNormal = vN * n;
                    Vector3 vTangent = body.Velocity - vNormal;
                    Vector3 vNormalOut = -vNormal * plane.Restitution;
                    body.Velocity = vTangent + vNormalOut;
                }


                // Registrar colisión
                body.OnCollision(plane);
            }
        }
    }

    public float[] RollingFriction => rollingFriction;
    public IReadOnlyList<CollisionPlaneComponent> Planes => planes;
}