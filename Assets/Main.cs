//////////////////////////////////
//Authors:                      //
//  Jesper Larsson - jesla966   //
//  Simon Jonsson - simjo788    //
//////////////////////////////////
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


    public float k_stiffness = 100f;
    public float restDensity = 100f;
    public float viscosity = 3.5f;
    public float jitters = 0.005f;


    public Vector3 boxMin = new Vector3(-10f, 0f, -1f);
    public Vector3 boxMax = new Vector3(10f, 10f, 1f);
    public float boundaryDamping = -0.5f;


    private Vector3[] positions;
    private Vector3[] velocities;
    private Vector3[] forces;
    private float[] density;
    private float[] pressure;
    private float[] mass;

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
        positions = new Vector3[particleList.Count];
        velocities = new Vector3[particleList.Count];
        pressure = new float[particleList.Count];
        density = new float[particleList.Count];
        mass = new float[particleList.Count];

        getParticleValues();
        //kernels = new SmoothingKernels();
        SmoothingKernels.SetRadius(smoothingRadius);
    }

    private void getParticleValues()
    {
        for (int i = 0; i < particleList.Count; i++)
        {
            ParticleData particle_data = particleList[i].GetComponent<ParticleData>();
            positions[i] = particle_data.position;
            velocities[i] = particle_data.velocity;
            //pressure[i] = particle_data.pressure;
            //density[i] = particle_data.density;
            pressure[i] = 0f;
            density[i] = restDensity;
            mass[i] = particle_data.mass;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dt = Time.deltaTime;

        //Build grid
        grid.Clear();
        for (int i = 0; i < particleList.Count; i++)
        {
            grid.AddParticle(i,positions[i]);
        }

        //Calculate Densisties
        NewCalculateDensisites();

        //Calculate forces
        NewCalculateForces();

        
        
        //Adding some jitter forces to make sure particles dont get stuck on each other
        for (int i = 0; i < particleList.Count; i++)
        {
            Vector3 jitter = Random.insideUnitSphere * jitters;
            forces[i] += jitter;
        }
        
        //Parallel integration calculations
        Parallel.For(0, numParticles, i =>
        {
            Vector3 acceleration = Vector3.zero;
            acceleration = forces[i] / density[i];
            velocities[i] += acceleration * dt;
            positions[i] += velocities[i] * dt;


            //Collision with walls calculation
            Vector3 p = positions[i];
            Vector3 v = velocities[i];

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
            positions[i] = p;
            velocities[i] = v;
        });
        
        //Update particle position
        for (int i = 0; i < particleList.Count; i++)
        {
            particleList[i].transform.position = positions[i];

            //Update particle data for debugging
            
            ParticleData data = particleList[i].GetComponent<ParticleData>();
            data.position = positions[i];
            data.velocity = velocities[i];
            data.density = density[i];
            data.pressure = pressure[i];  
            
        }
    }

    //Old density calculation that does not work in parallell
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
            //Debug.Log(density);
            particle_data.density = density;
            particle_data.pressure = k_stiffness * Mathf.Max(0, (particle_data.density - restDensity));

        }
    }

    //New density calculation that works in paralell
    private void NewCalculateDensisites()
    {
        //Parallel function runs code in parallel on cpu
        Parallel.For(0, numParticles, i =>
        {
            float pdensity = 0f;
            //Gets all neighboring particles indexes
            List<int> neighboursIndex = grid.GetNeighboringIndex(positions[i]);
            for (int j = 0; j < neighboursIndex.Count; j++)
            {
                int n = neighboursIndex[j];
                if ((positions[i] - positions[n]).sqrMagnitude < smoothingRadius * smoothingRadius)
                {
                    //Perform the density calculation using smoothing kernel
                    pdensity += mass[n] *
                        SmoothingKernels.W_poly6(positions[i] - positions[n]);
                }
            }
            density[i] = pdensity;
            //Perform the pressure calculation using the k stiffnes term
            pressure[i] = k_stiffness * Mathf.Max(0, (density[i] - restDensity));
        });  

    }

    //Old force calculation that does not work in parallel
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

    private void NewCalculateForces()
    {
        //Parallel function runs code in parallel on cpu
        Parallel.For(0, numParticles, i =>
        {
            forces[i] = Vector3.zero;
            //Gets all neighboring particles indexes
            List<int> neighboursIndex = grid.GetNeighboringIndex(positions[i]);
            for (int j = 0; j < neighboursIndex.Count; j++)
            {
                int n = neighboursIndex[j];
                //Skip if neighbour is itself
                if (i == n) continue;
                if ((positions[i] - positions[n]).magnitude < smoothingRadius)
                {
                    //Pressure force calculation with smoothing kernel
                    forces[i] += -mass[n] *
                        (pressure[i] + pressure[n]) / (2 * density[n]) *
                        SmoothingKernels.gradientW_spiky(positions[i] - positions[n]);

                    //Viscosity Force calculation with smoothing kernel
                    forces[i] += viscosity * mass[n] * (velocities[n] - velocities[i])
                        * SmoothingKernels.laplacianW_viscosity(positions[i] - positions[n]);
                }
                else continue;
            }
            if (gravity) //Gravity force calculation
            {
                forces[i] += new Vector3(0, -density[i] * 10f, 0);
            }
        });
    }

    //Old not used anymore
    private Vector3 GetExternalForces(ParticleData particle_data)
    {
        Vector3 gravityForce = Vector3.zero;
        if (gravity)
        {
            gravityForce = new Vector3(0,-particle_data.density * 10f,0);
        }
        return gravityForce;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = (boxMin + boxMax) * 0.5f;
        Vector3 size = boxMax - boxMin;
        Gizmos.DrawWireCube(center, size);
    }

}
