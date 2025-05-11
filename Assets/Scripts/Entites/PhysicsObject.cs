using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    [SerializeField, Tooltip("Masa en kg")] private float mass = 1f;
    [SerializeField, Tooltip("Radio en metros")] private float radius = 1f;

    private Vector3 velocity;
    private SurfaceType currentSurface;
    private Vector3 currentPlaneNormal = Vector3.up;
    private bool isGrounded;
    private float angularVelocity = 0f;

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

    public float AngularVelocity
    {
        get => angularVelocity;
        set => angularVelocity = value;
    }

    public void OnCollision(SurfaceType p_surface, Vector3 p_normal)
    {
        currentSurface = p_surface;
        currentPlaneNormal = p_normal;
        isGrounded = true;
    }

    public void ApplyTransform()
    {
        // 1) Use the surface normal to correctly position the ball
        transform.position = physicsPosition + currentPlaneNormal.normalized * radius;

        // 2) Calculate offset from previous frame
        Vector3 delta = physicsPosition - lastPhysicsPosition;
        lastPhysicsPosition = physicsPosition;

        // 3) Rolling on the inclined plane
        Vector3 deltaPlane = Vector3.ProjectOnPlane(delta, currentPlaneNormal);
        float distance = deltaPlane.magnitude;

        if (distance > Mathf.Epsilon)
        {
            // Axis perpendicular to the plane of movement: cross (normal, forward direction)
            Vector3 axis = Vector3.Cross(currentPlaneNormal, deltaPlane.normalized);

            float angle = angularVelocity * Time.fixedDeltaTime * Mathf.Rad2Deg;

            currentRotation = Quaternion.AngleAxis(angle, axis) * currentRotation;
            transform.rotation = currentRotation;
        }
    }

    #region Accessors
    public float Mass => mass;
    public float Radius => radius;
    public float Inertia => 0.4f * mass * radius * radius;

    public Vector3 Position { get { return physicsPosition; } set { physicsPosition = value; } }
    public Vector3 Velocity { get { return velocity; } set { velocity = value; } }
    public SurfaceType CurrentSurface { get { return currentSurface; } set { currentSurface = value; } }
    public Vector3 CurrentPlaneNormal { get { return currentPlaneNormal; } set { currentPlaneNormal = value; } }

    public bool ObjectIsInMotion => velocity.magnitude > 0.1f;
    public bool IsGrounded { get { return isGrounded; } set { isGrounded = value; } }
    #endregion
}