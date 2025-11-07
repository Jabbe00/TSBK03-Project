using UnityEngine;

public class Main : MonoBehaviour
{

    public GameObject particlePrefab;
    private ParticleData data;
    public Vector3 setData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        data = particlePrefab.GetComponent<ParticleData>();
        data.position = setData;

    }

    // Update is called once per frame
    void Update()
    {
        data.position = setData;
    }


}
