// BallController.cs
using UnityEngine;

[RequireComponent(typeof(PhysicsObject), typeof(LineRenderer))]
public class BallController : MonoBehaviour
{
    private PhysicsObject phys;
    private Camera cam;
    private LineRenderer lr;

    [Header("Force Settings")]
    [SerializeField] private float forceMultiplier = 50f;
    [SerializeField] private float maxDragDistance = 5f;
    [SerializeField] private int predictionSteps = 50;

    private Vector3 dragStart;
    private bool dragging;

    private void Awake()
    {
        phys = GetComponent<PhysicsObject>();
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.enabled = false;
    }

    private void Start() => cam = Camera.main;

    private void Update()
    {
        // Si la bola está en movimiento, ocultar la línea
        if (phys.Velocity.magnitude > 0.01f)
        {
            lr.enabled = false;
            return;
        }

        if (Input.GetMouseButtonDown(0)) StartDrag();
        if (dragging && Input.GetMouseButton(0)) UpdateDrag();
        if (dragging && Input.GetMouseButtonUp(0)) EndDrag();
    }

    private void StartDrag()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
        {
            dragging = true;
            dragStart = GetMouseWorldPos();
            lr.enabled = true;
        }
    }

    private void UpdateDrag()
    {
        Vector3 current = GetMouseWorldPos();
        DrawPrediction(dragStart - current);
    }

    private void EndDrag()
    {
        Vector3 drag = dragStart - GetMouseWorldPos();
        Vector3 applied = Vector3.ClampMagnitude(drag, maxDragDistance) * forceMultiplier;
        PhysicsManager.Instance.ApplyForce(phys, applied);
        dragging = false;
        lr.enabled = false;
    }

    private Vector3 GetMouseWorldPos()
    {
        Plane ground = new Plane(Vector3.up, transform.position);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (ground.Raycast(ray, out float enter)) return ray.GetPoint(enter);
        return transform.position;
    }

    private void DrawPrediction(Vector3 drag)
    {
        // Vector de fuerza limitado
        Vector3 force = Vector3.ClampMagnitude(drag, maxDragDistance) * forceMultiplier;
        // Punto final = posición de la bola + dirección de la fuerza
        Vector3 endPoint = transform.position + force;

        // Dibuja línea desde la bola hasta ese punto
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, endPoint);
    }

    public void ResetBall()
    {
        phys.Velocity = Vector3.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        lr.enabled = false;
    }
}