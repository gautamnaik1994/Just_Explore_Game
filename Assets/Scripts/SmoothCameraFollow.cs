using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    // This controls where on the screen the target appears
    public Vector3 lookOffset = new Vector3(0f, 1.5f, 0f);

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Look slightly above the target
        transform.LookAt(target.position);
    }
}