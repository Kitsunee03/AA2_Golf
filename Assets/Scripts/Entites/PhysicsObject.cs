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
    private int collisionCount;
    public int CollisionCount => collisionCount;

    private void Start()
    {
        physicsPosition = transform.position;
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
        collisionCount++;
        CurrentSurface = plane.Surface;
        CurrentPlaneNormal = plane.WorldNormal;
        CurrentRestitution = plane.Restitution;
    }

    public void ApplyTransform()
    {
        transform.position = physicsPosition + Vector3.up * radius;
    }

    public float Mass => mass;
    public float Radius => radius;
    public float Inertia => 0.4f * mass * radius * radius;
}
