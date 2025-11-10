using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Main : MonoBehaviour
{
    public bool gravity = true;
    public GameObject particlePrefab;
    public List<GameObject> particleList;
    //public ParticleSpawner spawner;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ParticleSpawner.Instance.SpawnParticles();
        particleList = ParticleSpawner.Instance.particleList;
        //data = particlePrefab.GetComponent<ParticleData>();
        //data.position = setData;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 totalForce = Vector3.zero;
        float dt = Time.deltaTime;
        for (int i = 0; i < particleList.Count; i++) 
        {
            totalForce = Vector3.zero;
            TEMPPARTICLEDATA particle_data = particleList[i].GetComponent<TEMPPARTICLEDATA>();

            totalForce += GetExternalForces(particle_data);

            Vector3 acceleration = totalForce / particle_data.mass;
            particle_data.velocity += acceleration * dt;

            particle_data.position = particle_data.position + particle_data.velocity * dt;

            ResolveCollision(ref particle_data.position, ref particle_data.velocity);
        }


    }

    private Vector3 GetExternalForces(TEMPPARTICLEDATA particle_data)
    {
        Vector3 gravityForce = Vector3.zero;
        if (gravity)
        {
            gravityForce = new Vector3(0,-particle_data.mass * 10f,0);
        }
        return gravityForce;
    }

    private void ResolveCollision(ref Vector3 pos, ref Vector3 velocity)
    {
        float floor = 0.5f;

        // If particle goes below the floor
        if (pos.y < floor)
        {
            pos.y = floor + 0.0001f; // small offset to prevent re-collision

            // Only reflect if moving downward
            if (velocity.y < 0f)
                velocity.y = -velocity.y;
        }

        // Add other boundaries here
        // e.g. walls or ceiling
    }

}
