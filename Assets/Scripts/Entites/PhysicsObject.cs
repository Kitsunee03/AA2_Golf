// PhysicsObject.cs
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    [SerializeField, Tooltip("Masa en kg")] private float mass = 1f;
    [SerializeField, Tooltip("Radio en metros")] private float radius = 1f;

    [HideInInspector] public Vector3 Velocity;
    [HideInInspector] public SurfaceType CurrentSurface;
    [HideInInspector] public Vector3 CurrentPlaneNormal = Vector3.up;
    [HideInInspector] public float CurrentRestitution;

    private Vector3 physicsPosition;
    private Vector3 lastPhysicsPosition;          
    private Quaternion currentRotation = Quaternion.identity;

    private void Start()
    {
        physicsPosition = transform.position;
        lastPhysicsPosition = physicsPosition;
        ApplyTransform();

        PhysicsManager.Instance.RegisterPhysicsObject(this);
    }

    public Vector3 Position
    {
        get => physicsPosition;
        set => physicsPosition = value;
    }

    public void OnCollision(CollisionPlaneComponent plane)
    {
        CurrentSurface = plane.Surface;
        CurrentPlaneNormal = plane.WorldNormal;
        CurrentRestitution = plane.Restitution;
    }

    public void ApplyTransform()
    {
        // 1) Actualizamos la posición visual (añadiendo el offset del radio)
        transform.position = physicsPosition + Vector3.up * radius;

        // 2) Calculamos desplazamiento desde el frame anterior
        Vector3 delta = physicsPosition - lastPhysicsPosition;
        lastPhysicsPosition = physicsPosition;

        // 3) Sólo nos interesa la componente horizontal para el rodamiento
        Vector3 deltaH = new Vector3(delta.x, 0f, delta.z);
        float distance = deltaH.magnitude;
        if (distance > Mathf.Epsilon)
        {
            Vector3 axis = Vector3.Cross(Vector3.up, deltaH.normalized);
            float angle = distance / radius * Mathf.Rad2Deg;
            currentRotation = Quaternion.AngleAxis(angle, axis) * currentRotation;
            transform.rotation = currentRotation;
        }
    }

    public float Mass => mass;
    public float Radius => radius;
    public float Inertia => 0.4f * mass * radius * radius;
}
