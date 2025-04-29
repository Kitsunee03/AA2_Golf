using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhysicsObject))]
[RequireComponent(typeof(LineRenderer))]
public class BallController : MonoBehaviour
{
    private PhysicsObject physObj;
    private Camera mainCamera;
    private LineRenderer lineRenderer;

    [Header("Ball parameters")]
    [SerializeField] private float forceMultiplier = 15f;
    [SerializeField] private float maxForceMagnitude = 20f;
    [SerializeField] private int simulationSteps = 100;
    [SerializeField] private float simulationDelta = 0.02f;

    private Vector3 dragStartPos;
    private Vector3 dragEndPos;
    private bool isDragging = false;
    private bool ballInMotion => physObj.velocity.magnitude > 0.05f;

    private void Awake()
    {
        physObj = GetComponent<PhysicsObject>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (ballInMotion)
        {
            lineRenderer.enabled = false;
            return;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        // start dragging
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

        // update dragging
        if (Input.GetMouseButton(0) && isDragging)
        {
            dragEndPos = GetMouseWorldPosition();
            UpdateTrajectoryLine();
        }

        // end dragging
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ApplyDragForce();
            isDragging = false;
            lineRenderer.enabled = false;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Plane plane = new(Vector3.up, transform.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter)) { return ray.GetPoint(enter); }

        return transform.position;
    }

    private void ApplyDragForce()
    {
        Vector3 forceDirection = dragStartPos - dragEndPos;
        Vector3 finalForce = Vector3.ClampMagnitude(forceDirection, maxForceMagnitude);
        PhysicsManager.Instance.ApplyForce(physObj, finalForce * forceMultiplier);
    }

    private void UpdateTrajectoryLine()
    {
        Vector3 forceDirection = dragStartPos - dragEndPos;
        Vector3 finalForce = Vector3.ClampMagnitude(forceDirection, maxForceMagnitude) * forceMultiplier;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, finalForce);
    }

    public void ResetBall()
    {
        lineRenderer.enabled = false;
        physObj.Velocity = Vector3.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}