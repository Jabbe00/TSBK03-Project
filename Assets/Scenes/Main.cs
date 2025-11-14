using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Main : MonoBehaviour
{
    public bool gravity = true;
    public GameObject particlePrefab;
    public List<GameObject> particleList;

    public SmoothingKernels kernels;

    public Grid grid;
    //public ParticleSpawner spawner;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ParticleSpawner.Instance.SpawnParticles();
        particleList = ParticleSpawner.Instance.particleList;
        //data = particlePrefab.GetComponent<ParticleData>();
        //data.position = setData;

        grid = new Grid(1f);
        for (int i = 0; i < particleList.Count; i++)
        {
            grid.AddParticle(particleList[i].GetComponent<ParticleData>());
        }
        kernels = new SmoothingKernels();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 totalForce = Vector3.zero;
        Vector3 pressureForce = Vector3.zero;
        float dt = Time.deltaTime;
        grid.Clear();
        for (int i = 0; i < particleList.Count; i++)
        {
            grid.AddParticle(particleList[i].GetComponent<ParticleData>());
        }
        kernels.SetRadius(1f);
        for (int i = 0; i < particleList.Count; i++)
        {
            float density = 0;
            ParticleData particle_data = particleList[i].GetComponent<ParticleData>();
            List<ParticleData> neighbours = grid.GetNeighboringParticles(particle_data);
            Debug.Log(neighbours.Count);
            for (int j = 0; j < neighbours.Count; j++)
            {
                density += neighbours[j].mass * 
                    kernels.W_ploy6(particle_data.position - neighbours[j].position);
            }
            float k = 8.3f;
            float density_0 = 1;
            particle_data.density = density;
            particle_data.pressure = k * (density - density_0);

        }

        for (int i = 0; i < particleList.Count; i++) 
        {
            totalForce = Vector3.zero;
            ParticleData particle_data = particleList[i].GetComponent<ParticleData>();

            List<ParticleData> neighbours = grid.GetNeighboringParticles(particle_data);

            if(neighbours.Count > 0) { 
            for (int j = 0; j < neighbours.Count; j++)
            {
                if (Mathf.Abs(particle_data.position.magnitude - neighbours[j].position.magnitude) < 1f)
                {
                    //PUCH APART
                    pressureForce += neighbours[j].mass *
                        (particle_data.pressure + neighbours[j].pressure) / (2 * neighbours[j].density) *
                        kernels.gradientW_spiky(particle_data.position - neighbours[j].position);
                }
            }}
            Debug.Log(pressureForce);
            totalForce += -pressureForce;



            totalForce += GetExternalForces(particle_data) * 0.7f;

            Vector3 acceleration = totalForce / particle_data.mass;
            particle_data.velocity += acceleration * dt;

            
        }
        for (int i = 0; i < particleList.Count; i++)
        {
            ParticleData particle_data = particleList[i].GetComponent<ParticleData>();
            particle_data.position = particle_data.position + particle_data.velocity * dt;
            ResolveCollision(ref particle_data.position, ref particle_data.velocity);
        }

    }

    private Vector3 GetExternalForces(ParticleData particle_data)
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

        if (pos.y < floor)
        {
            pos.y = floor + 0.0001f;

            if (velocity.y < 0f)
                velocity.y = -velocity.y;
        }
    }

}
