using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject), typeof(LineRenderer))]
public class BallController : MonoBehaviour
{
    private PhysicsObject phys;
    private Camera cam;
    private LineRenderer lr;

    [Header("Force Settings")]
    [SerializeField] private float maxDragDistance = 5f;
    [SerializeField] private float maxLaunchSpeed = 70f;
    [SerializeField, Range(0, 90f)]
    private float launchAngle = 45f;         // Ángulo de elevación en grados

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
        if (Input.GetMouseButtonDown(0)) { StartDrag(); }
        if (dragging && Input.GetMouseButton(0)) { UpdateDrag(); }
        if (dragging && Input.GetMouseButtonUp(0)) { EndDrag(); }
    }

    private void DrawPrediction(Vector3 p_drag)
    {
        // 1) Calcula v0Vec tal como lo tienes…
        float mag = Mathf.Clamp(p_drag.magnitude, 0f, maxDragDistance);
        float tNorm = mag / maxDragDistance;
        float v0 = tNorm * maxLaunchSpeed;
        float theta = launchAngle * Mathf.Deg2Rad;
        Vector3 dir = p_drag.normalized;
        Vector3 v0Vec = dir * (v0 * Mathf.Cos(theta))
                      + Vector3.up * (v0 * Mathf.Sin(theta));

        // 2) Calcula tiempo de vuelo (desde y0 hasta y0)
        float y0 = transform.position.y;
        float vy = v0Vec.y;
        float g = Physics.gravity.magnitude;
        float underS = vy * vy + 2f * g * y0;
        if (underS < 0f)
        {
            lr.enabled = false;
            return;
        }

        float tFlight = (vy + Mathf.Sqrt(underS)) / g;
        if (tFlight <= 0f)
        {
            lr.enabled = false;
            return;
        }

        // 3) Decide cuántos puntos: al menos 2
        int steps = Mathf.Max(2, Mathf.CeilToInt(tFlight / predictionStepTime));
        lr.positionCount = steps;
        lr.enabled = true;

        // 4) Muestrea la parábola en t∈[0, tFlight]
        for (int i = 0; i < steps; i++)
        {
            float t = (tFlight * i) / (steps - 1); // así llegas exactamente a tFlight
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
        Vector3 delta = dragStart - GetMouseWorldPos();
        // 1) Magnitud de arrastre [0…maxDragDistance]
        float mag = Mathf.Clamp(delta.magnitude, 0f, maxDragDistance);
        // 2) Interpolador t [0…1]
        float t = mag / maxDragDistance;
        // 3) Velocidad inicial v0 [0…maxLaunchSpeed]
        float v0 = t * maxLaunchSpeed;

        // Dirección plana
        Vector3 dirFlat = delta.normalized;
        float rad = launchAngle * Mathf.Deg2Rad;
        // Componente horizontal y vertical según ángulo
        Vector3 v0Vec = dirFlat * (v0 * Mathf.Cos(rad))
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
}
