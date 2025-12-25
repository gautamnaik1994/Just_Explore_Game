using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Transform))]
public class Player : MonoBehaviour, IInputReceiver
{
    private Rigidbody rb;

    public Collider terrainCollider;

    public int JumpForce = 10;
    public float MoveForce = 25.0f;

    public float moveForce = 20f;
    public float turnSpeed = 120f; // degrees per second
    public float maxSpeed = 10f;


    private Transform playerTransform;
    private Collider playerCollider;

    float targetYaw;
    float yawVelocity;
    bool rebasingYaw;





    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        playerTransform = GetComponent<Transform>();
        playerCollider = GetComponent<Collider>();

        // targetYaw = rb.rotation.eulerAngles.y;
    }

    bool CheckIfGrounded()
    {

        return playerCollider.bounds.min.y <= terrainCollider.bounds.max.y + 0.1f;


    }

    // public void Move(Vector3 input)
    // {
    //     // if (rb.linearVelocity.magnitude < 10.0f)
    //     // {
    //     //     rb.AddForce(new Vector3(input.x * MoveForce, 0, input.z * MoveForce), ForceMode.Force);
    //     // }

    //     // --------------------
    //     // 1. FORWARD / BACKWARD MOVEMENT
    //     // --------------------
    //     float forwardTilt = input.z;   // device tilt forward/back

    //     if (rb.linearVelocity.magnitude < maxSpeed)
    //     {
    //         Vector3 force = playerTransform.forward * forwardTilt * moveForce;
    //         rb.AddForce(force, ForceMode.Force);
    //     }

    //     // --------------------
    //     // 2. SIDEWAYS TILT → ROTATION
    //     // --------------------
    //     float sidewaysTilt = input.x;  // device tilt left/right

    //     float rotationAmount = sidewaysTilt * turnSpeed * Time.deltaTime;
    //     Quaternion turn = Quaternion.Euler(0f, rotationAmount, 0f);
    //     rb.MoveRotation(rb.rotation * turn);

    // }


    public void Jump()
    {
        if (!CheckIfGrounded())
            return;

        rb.AddForce(Vector3.up * JumpForce, ForceMode.Force);
    }


    Vector3 cachedInput;

    public void Move(Vector3 input)
    {
        cachedInput = input;
        // Debug.Log("Cached Input: " + cachedInput);
    }

    void FixedUpdate()
    {
        ApplyMovement(cachedInput);
    }

    void ApplyMovement(Vector3 input)
    {
        // Forward / backward
        float forwardTilt = input.z;

        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(playerTransform.forward * forwardTilt * moveForce, ForceMode.Force);
        }

        // Rotation
        float sidewaysTilt = input.x;

        if (Mathf.Abs(sidewaysTilt) < 0.01f)
        {
            // stop rotation completely if tilt is very small
            sidewaysTilt = 0f;
            // return;

        }

        float rotationAmount = sidewaysTilt * turnSpeed * Time.fixedDeltaTime;
        Debug.Log("Rotation Amount: " + rotationAmount + " from Sideways Tilt: " + sidewaysTilt);
        // targetYaw += sidewaysTilt * turnSpeed * Time.fixedDeltaTime;

        Quaternion turn = Quaternion.Euler(0f, rotationAmount, 0f);
        rb.MoveRotation(rb.rotation * turn);


    }
    IEnumerator RebaseYaw()
    {
        Debug.Log("Rebasing Yaw");
        rebasingYaw = true;

        float startYaw = targetYaw;
        float physicsYaw = rb.rotation.eulerAngles.y;

        float t = 0f;
        const float rebaseTime = 0.2f; // tweak (0.15–0.3 feels good)

        while (t < 1f)
        {
            t += Time.fixedDeltaTime / rebaseTime;

            targetYaw = Mathf.SmoothDampAngle(
                targetYaw,
                physicsYaw,
                ref yawVelocity,
                0.05f
            );

            yield return new WaitForFixedUpdate();
        }

        targetYaw = physicsYaw;
        rebasingYaw = false;
        Debug.Log("Finished Rebasing Yaw");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with " + collision.gameObject.name);
        StartCoroutine(RebaseYaw());
    }

}
