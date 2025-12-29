//////////////////////////////////
//Authors:                      //
//  Jesper Larsson - jesla966   //
//  Simon Jonsson - simjo788    //
//////////////////////////////////
using UnityEngine;

public class ParticleData : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
    public float mass;
    public float density;
    public float pressure;
    public int index;
    void Start()
    {
        position = transform.position;
    }

    private void Awake()
    {
        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //gameObject.transform.position = position;
    }
}
