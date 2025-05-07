using UnityEngine;

public class WindmillRotor : MonoBehaviour
{
    
    [SerializeField] float rotationSpeed = 90f;
    [SerializeField] Vector3 rotationAxis = Vector3.up;

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
    }
}
