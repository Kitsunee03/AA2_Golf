using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager Instance;

    [Header("Parámetros físicos globales")]
    [SerializeField] private float airDensity = 1.225f;
    [SerializeField] private float dragCoefficient = 0.47f;

    [Tooltip("Fricción para Grass, Ice, Sand (orden según enum)")]
    public float[] frictionCoefficients = { 0.4f, 0.1f, 0.6f };

    private List<PhysicsObject> physicsObjects = new();
    private List<CollisionPlaneComponent> collisionPlanes = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            CollisionPlaneComponent[] planes = FindObjectsOfType<CollisionPlaneComponent>();
            foreach (CollisionPlaneComponent plane in planes) { collisionPlanes.Add(plane); }

            return;
        }

        Destroy(gameObject);
    }

    public void RegisterPhysicsObject(PhysicsObject p_obj)
    {
        if (!physicsObjects.Contains(p_obj)) { physicsObjects.Add(p_obj); }
    }

    public void ApplyForce(PhysicsObject p_obj, Vector3 p_force)
    {
        p_obj.velocity += p_force / p_obj.Mass;
    }

    private void Update()
    {
        foreach (PhysicsObject obj in physicsObjects)
        {
            ApplyPhysics(obj);
            MoveObject(obj);
        }
    }

    private void ApplyPhysics(PhysicsObject obj)
    {
        Vector3 gravity = Physics.gravity;

        // Gravedad constante
        obj.velocity += gravity * Time.deltaTime;

        // Resistencia del aire si está elevado
        if (obj.transform.position.y > 1f)
        {
            float area = Mathf.PI * obj.Radius * obj.Radius;
            float speed = obj.velocity.magnitude;

            if (speed > 0.01f)
            {
                Vector3 airResistance = -0.5f * airDensity * dragCoefficient * area * speed * speed * obj.velocity.normalized;
                obj.velocity += (airResistance / obj.Mass) * Time.deltaTime;
            }
        }

        // Frenado si casi no se mueve
        if (obj.velocity.magnitude < 0.05f) { obj.velocity = Vector3.zero; }
    }

    private void MoveObject(PhysicsObject obj)
    {
        Vector3 nextPos = obj.transform.position + obj.velocity * Time.deltaTime;
        Vector3 gravity = Physics.gravity;

        foreach (var plane in collisionPlanes)
        {
            Vector3 normal = plane.WorldNormal;
            Vector3 point = plane.PointOnPlane;

            // Distancia del centro de la esfera al plano
            float distanceToPlane = Vector3.Dot(nextPos - point, normal);

            if (distanceToPlane < obj.Radius && IsPointInsidePlaneBounds(plane, nextPos))
            {
                // Corrección de penetración
                float penetration = obj.Radius - distanceToPlane;
                nextPos += normal * penetration;

                // Rebote si se dirige contra el plano
                float vDotN = Vector3.Dot(obj.velocity, normal);
                if (vDotN < 0)
                {
                    obj.velocity = Vector3.Reflect(obj.velocity, normal) * plane.Restitution;
                    obj.velocity *= 0.95f; // fricción adicional de impacto
                }

                // Fricción del plano
                SurfaceType surface = plane.Surface;
                float mu = frictionCoefficients[(int)surface];

                if (obj.velocity.sqrMagnitude > 0.0001f)
                {
                    Vector3 friction = -mu * obj.Mass * gravity.magnitude * obj.velocity.normalized;
                    obj.velocity += (friction / obj.Mass) * Time.deltaTime;
                }

                // Detener si es demasiado lento
                if (obj.velocity.magnitude < 0.05f)
                    obj.velocity = Vector3.zero;
            }
        }

        obj.transform.position = nextPos;

        // Rotación visual de la bola
        Vector3 horizontalVel = new Vector3(obj.velocity.x, 0f, obj.velocity.z);
        if (horizontalVel != Vector3.zero)
        {
            obj.transform.Rotate(
                Vector3.Cross(Vector3.up, horizontalVel.normalized),
                horizontalVel.magnitude / obj.Radius * Mathf.Rad2Deg * Time.deltaTime
            );
        }
    }

    private bool IsPointInsidePlaneBounds(CollisionPlaneComponent plane, Vector3 point)
    {
        Transform t = plane.transform;
        Vector3 localPoint = t.InverseTransformPoint(point);

        Vector3 size = Vector3.one;
        if (t.TryGetComponent<BoxCollider>(out var box))
            size = box.size * 0.5f;
        else if (t.TryGetComponent<Renderer>(out var rend))
            size = rend.bounds.size * 0.5f;

        return Mathf.Abs(localPoint.x) <= size.x && Mathf.Abs(localPoint.z) <= size.z;
    }
}