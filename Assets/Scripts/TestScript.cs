using UnityEngine;

public class TestScript : MonoBehaviour
{
    // automatically add Rigidbody component to target


    public GameObject target;
    public int force = 10;

    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = target.GetComponent<Rigidbody>();
    
    }

    // Update is called once per frame
    void Update()
    {
        // rb.AddForce(Vector3.forward * force);
        
    }
}
