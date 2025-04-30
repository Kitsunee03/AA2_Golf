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

    private void Update()
    {
        float dt = Time.deltaTime;
        foreach (var body in bodies)
        {
            IntegratePhysics(body, dt);
            HandleCollisions(body);
            body.ApplyTransform();
        }
    }

    private void IntegratePhysics(PhysicsObject body, float dt)
    {
        // 1) Gravedad (aceleración constante, independiente de la masa)
        body.Velocity += Physics.gravity * dt;

        // 2) Resistencia del aire si y > 1m
        if (body.transform.position.y > 1f)
        {
            float area = Mathf.PI * body.Radius * body.Radius;
            float speed = body.Velocity.magnitude;
            if (speed > 0.01f)
            {
                Vector3 drag = -0.5f * airDensity * dragCoefficient * area * speed * speed * body.Velocity.normalized;
                body.Velocity += (drag / body.Mass) * dt;
            }
        }

        // 3) Fricción de rodadura lineal progresiva
        int idx = (int)body.CurrentSurface;
        float mu = rollingFriction[idx];
        // deceleración constante: a = μ·g
        float decel = mu * Physics.gravity.magnitude;
        float velMag = body.Velocity.magnitude;
        if (velMag > 0f)
        {
            float newMag = velMag - decel * dt;
            if (newMag > 0f)
                body.Velocity = body.Velocity.normalized * newMag;
            else
                body.Velocity = Vector3.zero;
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

                // Rebote
                float vN = Vector3.Dot(body.Velocity, n);
                if (vN < 0)
                {
                    body.Velocity = Vector3.Reflect(body.Velocity, n) * plane.Restitution;
                }

                // Registrar colisión
                body.OnCollision(plane);
            }
        }
    }
}