// CollisionPlaneComponent.cs
using UnityEngine;

public enum SurfaceType { Grass, Ice, Sand }

[RequireComponent(typeof(Transform))]
public class CollisionPlaneComponent : MonoBehaviour
{
    private Vector3 localNormal = Vector3.up;

    [Header("Plane parameters")]
    [Tooltip("Coeficiente de restitución: 0.2 = inelástico, 0.8 = elástico")]
    [SerializeField] private float restitution = 0.2f;
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Grass;

    private void Start()
    {
        PhysicsManager.Instance.RegisterCollisionPlane(this);
    }

    public Vector3 WorldNormal => transform.TransformDirection(localNormal.normalized);
    public Vector3 PointOnPlane => transform.position;
    public float Restitution { get => restitution; set => restitution = Mathf.Clamp01(value); }
    public SurfaceType Surface { get => surfaceType; set => surfaceType = value; }

    public bool IsPointInsideBounds(Vector3 worldPoint)
    {
        // Comprueba que el punto esté dentro de los límites del plano (BoxCollider o Renderer)
        Transform t = transform;
        Vector3 localPoint = t.InverseTransformPoint(worldPoint);
        Vector3 halfSize = Vector3.one;
        if (t.TryGetComponent<BoxCollider>(out var box))
            halfSize = box.size * 0.5f;
        else if (t.TryGetComponent<Renderer>(out var rend))
            halfSize = rend.bounds.size * 0.5f;

        return Mathf.Abs(localPoint.x) <= halfSize.x && Mathf.Abs(localPoint.z) <= halfSize.z;
    }
}