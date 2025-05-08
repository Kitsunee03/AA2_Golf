using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject), typeof(LineRenderer))]
public class BallController : MonoBehaviour
{
    private PhysicsObject phys;
    private Camera cam;
    private LineRenderer lr;

    [SerializeField] private float heightLimit = -10f; // height limit for the ball to reset

    [Header("Force Settings")]
    [SerializeField] private float maxDragDistance = 5f;
    [SerializeField] private float maxLaunchSpeed = 70f;
    [SerializeField, Tooltip("Maximum launch angle (degrees)")]
    private float maxLaunchAngle = 15f;

    [Header("Prediction Settings")]
    [SerializeField] private float predictionStepTime = 0.1f;

    private Vector3 dragStart;
    private bool dragging;

    private void Awake()
    {
        phys = GetComponent<PhysicsObject>();
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.enabled = false;
    }

    private void Start() { cam = Camera.main; }

    private void Update()
    {
        if(transform.position.y < heightLimit) { ResetBall(); }

        if (phys.ObjectIsInMotion) { return; }

        if (Input.GetMouseButtonDown(0)) { StartDrag(); }
        if (dragging && Input.GetMouseButton(0)) { UpdateDrag(); }
        if (dragging && Input.GetMouseButtonUp(0)) { EndDrag(); }
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
        Vector3 delta = dragStart - current;

        // --- Same initial velocity calculation as in EndDrag() ---
        float mag = Mathf.Clamp(delta.magnitude, 0f, maxDragDistance);
        float tNorm = mag / maxDragDistance;
        float v0 = tNorm * maxLaunchSpeed;
        float angleRad = (tNorm * maxLaunchAngle) * Mathf.Deg2Rad;
        Vector3 dir = delta.normalized;
        Vector3 initialVel = dir * (v0 * Mathf.Cos(angleRad))
                           + Vector3.up * (v0 * Mathf.Sin(angleRad));

        // Draw the prediction
        DrawPrediction(dragStart, initialVel);
    }

    private void EndDrag()
    {
        Vector3 delta = dragStart - GetMouseWorldPos();
        // 1) Drag magnitude [0…maxDragDistance]
        float mag = Mathf.Clamp(delta.magnitude, 0f, maxDragDistance);
        // 2) Interpolator t [0…1]
        float t = mag / maxDragDistance;
        // 3) Initial velocity v0 [0…maxLaunchSpeed]
        float v0 = t * maxLaunchSpeed;

        // Angular direction
        float angle = t * maxLaunchAngle;   
        float rad = angle * Mathf.Deg2Rad;
        Vector3 dir = delta.normalized;
        // Horizontal and vertical components according to angle
        Vector3 v0Vec = dir * (v0 * Mathf.Cos(rad))
                      + Vector3.up * (v0 * Mathf.Sin(rad));

        PhysicsManager.Instance.ApplyForce(phys, v0Vec * phys.Mass);
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
        phys.Position = Vector3.zero;
        phys.ApplyTransform();
        transform.rotation = Quaternion.identity;
        lr.enabled = false;
    }

    private void DrawPrediction(Vector3 startPos, Vector3 initialVelocity)
    {
        // We calculate the flight time until returning to the same ground level:
        float tFlight = 2f * initialVelocity.y / -Physics.gravity.y;
        int steps = Mathf.CeilToInt(tFlight / predictionStepTime) + 1;

        Vector3[] points = new Vector3[steps];
        for (int i = 0; i < steps; i++)
        {
            float t = i * predictionStepTime;
            // s = s0 + v0*t + ½·g·t²
            points[i] = startPos
                        + initialVelocity * t
                        + 0.5f * Physics.gravity * t * t;
        }

        lr.positionCount = steps;
        lr.SetPositions(points);
    }
}
