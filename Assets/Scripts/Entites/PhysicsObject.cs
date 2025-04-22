using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    [SerializeField] private float mass = 1f;           // kilograms
    [SerializeField] private float radius = 1f;         // meters

    [HideInInspector] public Vector3 velocity;

    private void Start()
    {
        PhysicsManager.Instance.RegisterPhysicsObject(this);
    }

    #region Accessors
    public Vector3 Velocity { get => velocity; set => velocity = value; }
    public float Mass { get => mass; set => mass = value; }
    public float Radius { get => radius; set => radius = value; }
    #endregion
}