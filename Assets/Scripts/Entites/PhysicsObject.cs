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

    public void OnCollision(SurfaceType p_surface, Vector3 p_normal, float p_restitution)
    {
        CurrentSurface = p_surface;
        CurrentPlaneNormal = p_normal;
        CurrentRestitution = p_restitution;
    }

    public void ApplyTransform()
    {
        // 1) Usar la normal de la superficie para posicionar correctamente la bola
        transform.position = physicsPosition + CurrentPlaneNormal.normalized * radius;

        // 2) Calcular desplazamiento desde frame anterior
        Vector3 delta = physicsPosition - lastPhysicsPosition;
        lastPhysicsPosition = physicsPosition;

        // 3) Rodamiento solo horizontal
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

    #region Accessors
    public float Mass => mass;
    public float Radius => radius;
    public float Inertia => 0.4f * mass * radius * radius;
    public Vector3 Position { get => physicsPosition; set => physicsPosition = value; }
    #endregion
}