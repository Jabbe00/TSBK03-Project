using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Grid
{
    //storlek på varje cell i gridet och en hashmap för att lagra cellerna och deras partiklar
    public float cellSize;
    private Dictionary<Vector3Int, List<ParticleData>> cells;
    private Dictionary<Vector3Int, List<int>> cells2;

    //konstruktor som initierar cellstorleken och cellordboken
    public Grid(float cellSize)
    {
        this.cellSize = cellSize;
        cells = new Dictionary<Vector3Int, List<ParticleData>>();
        cells2 = new Dictionary<Vector3Int, List<int>>();
    }

    //ger vilken cell baserat på partikelns position
    public Vector3Int GetParticleCell(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / cellSize);
        int y = Mathf.FloorToInt(pos.y / cellSize);
        int z = Mathf.FloorToInt(pos.z / cellSize);
        return new Vector3Int(x, y, z);
    }

    //töm griden
    public void Clear()
    {
        cells.Clear();
        cells2.Clear();
    }

    //lägg till en partikel i rätt cell och om cellen inte finns gör den
    public void AddParticle(ParticleData particle)
    {
        Vector3Int cell = GetParticleCell(particle.position);
        if (!cells.ContainsKey(cell))
        {
            cells[cell] = new List<ParticleData>();
        }
        cells[cell].Add(particle);
    }

    //kolla grannceller för att hitta närliggande partiklar
    //Gammal, används inte längre
    public List<ParticleData> GetNeighboringParticles(ParticleData particle)
    {
        List<ParticleData> neighbours = new List<ParticleData>();
        Vector3Int cell = GetParticleCell(particle.position);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int neighborCell = new Vector3Int(cell.x + x, cell.y + y, cell.z + z);
                    if (cells.ContainsKey(neighborCell))
                    {

                        neighbours.AddRange(cells[neighborCell]);
                        /*foreach (var p in cells[neighborCell])
                        {
                            // Prevent including itself as a neighbor
                            if (p != particle)
                            {
                                neighbours.Add(p);
                            }
                        }*/
                    }
                }
            }
        }
        return neighbours;
    }
    
    public List<int> OLDGetNeighboringIndex(Vector3 position)
    {
        List<int> index = new List<int>();
        
        Vector3Int cell = GetParticleCell(position);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    List<ParticleData> neighbours = new List<ParticleData>();
                    Vector3Int neighborCell = new Vector3Int(cell.x + x, cell.y + y, cell.z + z);
                    if (cells.ContainsKey(neighborCell))
                    {
                        foreach (var p in cells[neighborCell])
                        {
                            index.Add(p.index);
                        }
                        /*foreach (var p in cells[neighborCell])
                        {
                            // Prevent including itself as a neighbor
                            if (p != particle)
                            {
                                neighbours.Add(p);
                            }
                        }*/
                    }
                }
            }
        }
        if (index.Count == 0)
        {
            Debug.Log("COULDNT FIND ANY NEIGHBOUR");
        }
        
        return index;
    }

    public void AddParticle(int index, Vector3 pos)
    {
        Vector3Int cell = GetParticleCell(pos);
        if (!cells2.TryGetValue(cell, out List<int> list))
        {
            list = new List<int>();
            cells2[cell] = list;
        }
        list.Add(index);
    }

    //Returnerar en lista av index för närliggande partiklar

    public List<int> GetNeighboringIndex(Vector3 pos)
    {
        List<int> idx = new List<int>();
        Vector3Int cell = GetParticleCell(pos);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int neighborCell = new Vector3Int(cell.x + x, cell.y + y, cell.z + z);
                    if (cells2.TryGetValue(neighborCell, out List<int> list))
                    {
                        idx.AddRange(list);
                    }
                }
            }
        }

        if (idx.Count == 0)
        {
            Debug.LogWarning($"Grid: no neighbors found for position {pos}");
        }

        return idx;
    }


}
