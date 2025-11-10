using UnityEngine;

public class TEMPPARTICLEDATA : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Vector3 position;
    public Vector3 velocity;
    public float mass = 1.0f;
    void Start()
    {
        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = position;
    }
}
