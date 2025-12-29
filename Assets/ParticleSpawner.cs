//////////////////////////////////
//Authors:                      //
//  Jesper Larsson - jesla966   //
//  Simon Jonsson - simjo788    //
//////////////////////////////////
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{

    public float width = 1f;
    public float height = 1f;
    public float depth = 0f;

    public int amount_width = 10;
    public int amount_height = 10;
    public int amount_depth = 1;

    public GameObject particlePrefab;

    public List<GameObject> particleList;

    public static ParticleSpawner Instance;


    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Spawns particles in a 3D grid
    public void SpawnParticles()
    {
        Vector3 pos = Vector3.zero;
        for (int x = 0; x < amount_width; x++)
        {
            for (int y = 0; y < amount_height; y++)
            {
                for (int z = 0; z < amount_depth; z++)
                {
                    pos = CalculatePosition(x, y, z);
                    GameObject particle = Instantiate(particlePrefab, pos, Quaternion.identity);
                    particle.GetComponent<ParticleData>().index = particleList.Count;
                    particleList.Add(particle);
                }
            }
        }
    }

    //Calculates position for new particle
    private Vector3 CalculatePosition(int x, int y, int z)
    {
        float offset_width = width/amount_width * x;
        float offset_height = height/amount_height * y;
        float offset_depth = depth / amount_depth * z;

        Vector3 pos = new Vector3(offset_width, 1 + offset_height, offset_depth);
        return pos;
    }
}
