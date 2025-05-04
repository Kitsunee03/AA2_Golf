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
    [SerializeField, Range(0, 90f)]
    private float launchAngle = 45f;            // Ángulo de elevación en grados

    [Header("Prediction Settings")]
    [SerializeField] private int predictionSteps = 30;
    [SerializeField] private float predictionStepTime = 0.1f;

    private Vector3 dragStart;
    private bool dragging;

    private void Awake()
    {
        phys = GetComponent<PhysicsObject>();
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = predictionSteps;
        lr.enabled = false;
    }

    private void Start() => cam = Camera.main;

    private void Update()
    {
        // Si la bola está en movimiento, ocultar la previsión
        if (phys.Velocity.magnitude > 0.01f)
        {
            lr.enabled = false;
            return;
        }

        if (Input.GetMouseButtonDown(0)) StartDrag();
        if (dragging)
        {
            if (Input.GetMouseButton(0)) UpdateDrag();
            if (Input.GetMouseButtonUp(0)) EndDrag();
            return; 
        }
    }

    private void DrawPrediction(Vector3 drag)
    {
        // 1) Fuerza “plana” (x,z) limitada 
        Vector3 flatForce = Vector3.ClampMagnitude(drag, maxDragDistance) * forceMultiplier;

        // 2) Ángulo en radianes
        float theta = launchAngle * Mathf.Deg2Rad;

        // 3) Velocidad inicial total (v0 = F/m)
        Vector3 dirFlat = flatForce.normalized;
        float flatMag = flatForce.magnitude;
        float v0 = flatMag / phys.Mass;
        Vector3 v0Vec = dirFlat * (v0 * Mathf.Cos(theta))
                      + Vector3.up * (v0 * Mathf.Sin(theta));

        // 4) Tiempo de vuelo hasta volver al mismo nivel Y0
        float g = Physics.gravity.magnitude;
        float tFlight = (2f * v0Vec.y) / g;
        if (tFlight <= 0f) { lr.enabled = false; return; }

        // 5) Dividimos en pasos y ajustamos el LineRenderer
        int steps = Mathf.CeilToInt(tFlight / predictionStepTime);
        lr.positionCount = steps;
        lr.enabled = true;

        // 6) Para cada instante t, cae la parábola exacta
        for (int i = 0; i < steps; i++)
        {
            float t = predictionStepTime * i;
            // x(t) = x0 + vx*t
            // y(t) = y0 + vy*t - ½ g t²
            // z(t) = z0 + vz*t
            Vector3 p = transform.position + new Vector3(
                v0Vec.x * t,
                v0Vec.y * t - 0.5f * g * t * t,
                v0Vec.z * t
            );
            lr.SetPosition(i, p);
        }
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
        Vector3 drag = dragStart - current;
        DrawPrediction(drag);
    }

    private void EndDrag()
    {
        Vector3 drag = dragStart - GetMouseWorldPos();
        Vector3 flatForce = Vector3.ClampMagnitude(drag, maxDragDistance) * forceMultiplier;

        // Convertimos flatForce (x,z) + módulo en y según el ángulo
        float rad = launchAngle * Mathf.Deg2Rad;
        float flatMag = flatForce.magnitude;
        Vector3 dirFlat = flatForce.normalized;
        Vector3 applied = dirFlat * flatMag * Mathf.Cos(rad)      // componente horizontal
                          + Vector3.up * (flatMag * Mathf.Sin(rad)); // componente vertical

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

    public void ResetBall()
    {
        phys.Velocity = Vector3.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        lr.enabled = false;
    }
}
