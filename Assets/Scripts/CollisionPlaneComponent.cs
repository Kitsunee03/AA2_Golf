using UnityEngine;

public enum SurfaceType { Grass, Ice, Sand }

[RequireComponent(typeof(Transform))]
public class CollisionPlaneComponent : MonoBehaviour
{
    private Vector3 localNormal = Vector3.up;
    
    [Header("Plane parameters")]
    [Tooltip("Coeficiente de restituci�n: 0.2 = inel�stico, 0.8 = el�stico")]
    [SerializeField] private float restitution = 0.2f;
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Grass;

    public Vector3 WorldNormal => transform.TransformDirection(localNormal.normalized);
    public Vector3 PointOnPlane => transform.position;
    public float Restitution { get => restitution; set => restitution = Mathf.Clamp01(value); }
    public SurfaceType Surface { get => surfaceType; set => surfaceType = value; }
}