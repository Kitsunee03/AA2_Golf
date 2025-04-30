// PhysicsObject.cs
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    [SerializeField, Tooltip("Masa en kg")] private float mass = 1f;
    [SerializeField, Tooltip("Radio en metros")] private float radius = 1f;

    [HideInInspector] public Vector3 Velocity;
    [HideInInspector] public SurfaceType CurrentSurface;

    private int collisionCount;
    public int CollisionCount => collisionCount;

    private void Start()
    {
        PhysicsManager.Instance.RegisterPhysicsObject(this);
    }

    public float Mass { get => mass; set => mass = value; }
    public float Radius { get => radius; set => radius = value; }

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    public void OnCollision(CollisionPlaneComponent plane)
    {
        collisionCount++;
        CurrentSurface = plane.Surface;
    }

    public void ApplyTransform()
    {
        transform.position = Position;
    }
}
