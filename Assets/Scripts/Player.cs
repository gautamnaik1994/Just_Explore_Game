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

    private Transform playerTransform;
    private Collider playerCollider;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = GetComponent<Transform>();
        playerCollider = GetComponent<Collider>();
    }

    bool CheckIfGrounded()
    {

        return playerCollider.bounds.min.y <= terrainCollider.bounds.max.y + 0.1f;


    }

    public void Move(Vector3 input)
    {
        if (rb.linearVelocity.magnitude < 10.0f)
        {
            rb.AddForce(new Vector3(input.x * MoveForce, 0, input.z * MoveForce), ForceMode.Force);
            Debug.Log("Speed: " + rb.linearVelocity.magnitude);
        }

    }

    public void Custom()
    {
        Debug.Log("Custom");
    }


    public void Jump()
    {
        if (!CheckIfGrounded())
            return;

        rb.AddForce(Vector3.up * JumpForce, ForceMode.Force);
    }
}
