// CameraController.cs
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target;
    [Header("Distance Limits")]
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float lerpSpeed = 20f;

    private float currentDistance;
    private float rotX, rotY;

    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) target = player.transform;
        currentDistance = (minDistance + maxDistance) * 0.5f;
    }

    private void LateUpdate()
    {
        if (!target) return;
        HandleRotation();
        HandleZoom();
        HandlePosition();
    }

    private void HandleRotation()
    {
        rotX = Mathf.Clamp(rotX + Input.GetAxis("Vertical") / 3f, -40f, 50f);
        rotY += -Input.GetAxis("Horizontal") / 3f;
        transform.rotation = Quaternion.Euler(rotX, rotY, 0f);
    }

    private void HandleZoom()
    {
        currentDistance = Mathf.Clamp(currentDistance - Input.mouseScrollDelta.y, minDistance, maxDistance);
    }

    private void HandlePosition()
    {
        Vector3 desired = target.position - transform.forward * currentDistance;
        transform.position = Vector3.Lerp(transform.position, desired, lerpSpeed * Time.deltaTime);
    }
}
