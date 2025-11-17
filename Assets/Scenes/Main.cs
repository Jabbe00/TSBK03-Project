using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Main : MonoBehaviour
{
    public bool gravity = true;
    public GameObject particlePrefab;
    public List<GameObject> particleList;

    //public SmoothingKernels kernels;

    public Grid grid;
    //public ParticleSpawner spawner;

    public float smoothingRadius = 3;

    public float stiffness = 100f;
    public float restDensity = 100f;
    public float viscosity = 3.5f;


    public Vector3 boxMin = new Vector3(-10f, 0f, -1f);
    public Vector3 boxMax = new Vector3(10f, 10f, 1f);
    public float boundaryDamping = -0.5f;

    private Vector3[] forces;

    private int numParticles;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ParticleSpawner.Instance.SpawnParticles();
        particleList = ParticleSpawner.Instance.particleList;
        numParticles = particleList.Count;
        //data = particlePrefab.GetComponent<ParticleData>();
        //data.position = setData;

        grid = new Grid(smoothingRadius);
        for (int i = 0; i < particleList.Count; i++)
        {
            grid.AddParticle(particleList[i].GetComponent<ParticleData>());
        }

        forces = new Vector3[particleList.Count];
        //kernels = new SmoothingKernels();
        SmoothingKernels.SetRadius(smoothingRadius);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dt = Time.deltaTime;

        //Build grid
        grid.Clear();
        for (int i = 0; i < particleList.Count; i++)
        {
            grid.AddParticle(particleList[i].GetComponent<ParticleData>());
        }


        //Calculate Densisties
        CalculateDensities();

        //Calculate forces
        CalculateForces();

        //Perform Integration
        for (int i = 0; i < particleList.Count; i++)
        {
            ParticleData particle_data = particleList[i].GetComponent<ParticleData>();
            Vector3 acceleration = forces[i] / particle_data.density;
            particle_data.velocity += acceleration * dt;

            particle_data.position += particle_data.velocity * dt;
            //ResolveCollision(ref particle_data.position, ref particle_data.velocity);

            Vector3 p = particle_data.position;
            Vector3 v = particle_data.velocity;

            if (p.x < boxMin.x)
            {
                p.x = boxMin.x + 0.001f;
                v.x *= boundaryDamping;
            }
            if (p.x > boxMax.x)
            {
                p.x = boxMax.x - 0.001f;
                v.x *= boundaryDamping;
            }
            if (p.y < boxMin.y)
            {
                p.y = boxMin.y + 0.001f;
                v.y *= boundaryDamping;
            }
            if (p.y > boxMax.y)
            {
                p.y = boxMax.y - 0.001f;
                v.y *= boundaryDamping;
            }
            if (p.z < boxMin.z)
            {
                p.z = boxMin.z + 0.001f;
                v.z *= boundaryDamping;
            }
            if (p.z > boxMax.z)
            {
                p.z = boxMax.z - 0.001f;
                v.z *= boundaryDamping;
            }
            particle_data.position = p;
            particle_data.velocity = v;
        }

    }

    private void CalculateDensities()
    {
        
        for (int i = 0; i < particleList.Count; i++)
        {
            float density = 0f;
            ParticleData particle_data = particleList[i].GetComponent<ParticleData>();
            List<ParticleData> neighbours = grid.GetNeighboringParticles(particle_data);
            //Debug.Log(neighbours.Count);
            for (int j = 0; j < neighbours.Count; j++)
            {
                //Debug.Log((particle_data.position - neighbours[j].position).sqrMagnitude);
                if ((particle_data.position - neighbours[j].position).sqrMagnitude < smoothingRadius* smoothingRadius)
                {
                    density += neighbours[j].mass *
                        SmoothingKernels.W_poly6(particle_data.position - neighbours[j].position);
                }

            }
            //float k = 200f;
            //float density_0 = 0.03f;
            Debug.Log(density);
            particle_data.density = density;
            particle_data.pressure = stiffness * Mathf.Max(0, (particle_data.density - restDensity));

        }

        //CURRENTLY DOES NOT WORK, NEED TO TAKE ALL GetComponent OUT OF Parallel
        /*
        Parallel.For(0, numParticles, i =>
        {
            float density = 0f;
            ParticleData particle_data = particleList[i].GetComponent<ParticleData>();
            List<ParticleData> neighbours = grid.GetNeighboringParticles(particle_data);
            //Debug.Log(neighbours.Count);
            for (int j = 0; j < neighbours.Count; j++)
            {
                //Debug.Log((particle_data.position - neighbours[j].position).sqrMagnitude);
                if ((particle_data.position - neighbours[j].position).sqrMagnitude < smoothingRadius * smoothingRadius)
                {
                    density += neighbours[j].mass *
                        SmoothingKernels.W_poly6(particle_data.position - neighbours[j].position);
                }

            }
            //float k = 200f;
            //float density_0 = 0.03f;
            //Debug.Log(density);
            particle_data.density = density;
            particle_data.pressure = stiffness * Mathf.Max(0, (particle_data.density - restDensity));
        });*/


    }

    private void CalculateForces()
    {
        
        for (int i = 0; i < particleList.Count; i++)
        {
            Vector3 totalForce = Vector3.zero;
            Vector3 pressureForce = Vector3.zero;
            Vector3 viscosityForce = Vector3.zero;
            ParticleData particle_data = particleList[i].GetComponent<ParticleData>();
            List<ParticleData> neighbours = grid.GetNeighboringParticles(particle_data);


            for (int j = 0; j < neighbours.Count; j++)
            {
                if (particle_data == neighbours[j]) continue;
                //Debug.Log((particle_data.position - neighbours[j].position).magnitude);
                if ((particle_data.position - neighbours[j].position).magnitude < smoothingRadius)
                {
                    //PUCH APART
                    pressureForce += -neighbours[j].mass *
                        (particle_data.pressure + neighbours[j].pressure) / (2 * neighbours[j].density) *
                        SmoothingKernels.gradientW_spiky(particle_data.position - neighbours[j].position);

                    viscosityForce += viscosity * neighbours[j].mass * (neighbours[j].velocity - particle_data.velocity)
                        * SmoothingKernels.laplacianW_viscosity(particle_data.position - neighbours[j].position);
                }
                else continue;
            }
            if (pressureForce != Vector3.zero) {Debug.Log(pressureForce); }
            
            totalForce += pressureForce;
            totalForce += viscosityForce;
            totalForce += GetExternalForces(particle_data);

            forces[i] = totalForce;



        }
    }

    private Vector3 GetExternalForces(ParticleData particle_data)
    {
        Vector3 gravityForce = Vector3.zero;
        if (gravity)
        {
            gravityForce = new Vector3(0,-particle_data.density * 10f,0);
        }
        return gravityForce;
    }

    private void ResolveCollision(ref Vector3 pos, ref Vector3 velocity)
    {
        float floor = 0.5f;

        float roof = 11f;
        float right = 11f;
        float left = -1f;
        float damping = -1f;

        if (pos.y < floor)
        {
            pos.y = floor + 0.0001f;

           
            velocity.y *=damping;
        }
        if (pos.y > roof)
        {
            pos.y = roof - 0.0001f;

           
            velocity.y *= damping;
        }
        
        if (pos.x > right)
        {
            pos.x = right - 0.0001f;

            
            velocity.x *= damping;
        }
        if (pos.x < left)
        {
            pos.x = left + 0.0001f;


            velocity.x *= damping;
        }

    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = (boxMin + boxMax) * 0.5f;
        Vector3 size = boxMax - boxMin;
        Gizmos.DrawWireCube(center, size);
    }

}
