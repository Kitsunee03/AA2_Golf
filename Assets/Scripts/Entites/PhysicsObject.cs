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
    [HideInInspector] public bool IsGrounded;

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

        IsGrounded = true;
    }

    public void ApplyTransform()
    {
        // 1) Use the surface normal to correctly position the ball
        transform.position = physicsPosition + CurrentPlaneNormal.normalized * radius;

        // 2) Calculate offset from previous frame
        Vector3 delta = physicsPosition - lastPhysicsPosition;
        lastPhysicsPosition = physicsPosition;

        // 3) Rolling on the inclined plane
        Vector3 deltaPlane = Vector3.ProjectOnPlane(delta, CurrentPlaneNormal);
        float distance = deltaPlane.magnitude;

        if (distance > Mathf.Epsilon)
        {
            // Axis perpendicular to the plane of movement: cross (normal, forward direction)
            Vector3 axis = Vector3.Cross(CurrentPlaneNormal, deltaPlane.normalized);

            // Angle according to Δθ = s/r (converted to degrees)
            float angle = distance / radius * Mathf.Rad2Deg;

            // We accumulate the rotation
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