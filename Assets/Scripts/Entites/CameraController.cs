using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject m_target;

    [Header("Movement")]
    [SerializeField, Min(0)] private Vector2 distanceLimits = new(1.5f, 50f);
    [SerializeField, Min(0)] private float m_cameraLerp = 20f;
    private float m_targetDistance;
    private float rotationX, rotationY;

    //private UIManager m_ui;

    private float timeScaleMultiplier = 1.0f;

    private void Awake()
    {
        m_target = GameObject.FindGameObjectWithTag("Player");
        m_targetDistance = distanceLimits.y / 2;
        //m_ui = FindObjectOfType<UIManager>();
    }

    private void LateUpdate()
    {
        HandleRotation();
        HandleMovement();

        HandleZoom();
    }

    private void HandleRotation()
    {
        // WASD
        rotationX += Input.GetAxis("Vertical") / 3;
        rotationY -= Input.GetAxis("Horizontal") / 3;

        rotationX = Mathf.Clamp(rotationX, -40, 50f); // -40,50 --> camera angle limits
        transform.eulerAngles = new(rotationX, rotationY, 0);
    }

    private void HandleMovement()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            m_target.transform.position - transform.forward * m_targetDistance,
            m_cameraLerp * Time.deltaTime
        );
    }

    private void HandleZoom()
    {
        m_targetDistance -= Input.mouseScrollDelta.y;

        if (Input.GetKey(KeyCode.DownArrow)) { m_targetDistance += 0.1f; } // v = -zoom
        else if (Input.GetKey(KeyCode.UpArrow)) { m_targetDistance -= 0.1f; } // ^ = +zoom

        m_targetDistance = Mathf.Clamp(m_targetDistance, distanceLimits.x, distanceLimits.y); // zoom limits
    }
}