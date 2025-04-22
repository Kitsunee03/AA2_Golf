using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhysicsObject))]
[RequireComponent(typeof(LineRenderer))]
public class BallController : MonoBehaviour
{
    private PhysicsObject physObj;
    private Camera mainCamera;
    private LineRenderer lineRenderer;

    [SerializeField] private float forceMultiplier = 15f;
    [SerializeField] private float maxForceMagnitude = 20f;
    [SerializeField] private int simulationSteps = 100;
    [SerializeField] private float simulationDelta = 0.02f;

    private Vector3 dragStartPos;
    private Vector3 dragEndPos;
    private bool isDragging = false;
    private bool ballInMotion => physObj.velocity.magnitude > 0.05f;

    void Start()
    {
        physObj = GetComponent<PhysicsObject>();
        lineRenderer = GetComponent<LineRenderer>();
        mainCamera = Camera.main;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (ballInMotion)
        {
            lineRenderer.enabled = false;
            return;
        }

        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                    dragStartPos = GetMouseWorldPosition();
                    lineRenderer.enabled = true;
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            dragEndPos = GetMouseWorldPosition();
            UpdateTrajectoryLine();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ApplyDragForce();
            isDragging = false;
            lineRenderer.enabled = false;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return transform.position;
    }

    void ApplyDragForce()
    {
        Vector3 forceDirection = dragStartPos - dragEndPos;
        Vector3 finalForce = Vector3.ClampMagnitude(forceDirection, maxForceMagnitude);
        PhysicsManager.Instance.ApplyForce(physObj, finalForce * forceMultiplier);
    }

    void UpdateTrajectoryLine()
    {
        Vector3 forceDirection = dragStartPos - dragEndPos;
        Vector3 finalForce = Vector3.ClampMagnitude(forceDirection, maxForceMagnitude) * forceMultiplier;

        Vector3 predictedEnd = PhysicsManager.Instance.PredictEndPoint(
            physObj,
            finalForce,
            simulationSteps,
            simulationDelta
        );

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, predictedEnd);
    }

}