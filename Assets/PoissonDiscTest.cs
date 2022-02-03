using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDiscTest : MonoBehaviour
{
    [Tooltip("The size of the poisson disc field")]
    public Vector3 size = new Vector3(1, 0, 1);
    [Tooltip("The seed used for the Random initialisation")]
    public int seed = 43534;
    [Min(0.01f)]
    [Tooltip("The distance between each position")]
    public float radius = 1.0f;
    [Tooltip("The number of attempts to make per iteration to find a valid candidate")]
    public int k = 4;

    [Space]
    [Tooltip("Debugging tool to check distance spacing is working")]
    public bool showRadius = true;

    private void OnDrawGizmos()
    {
        List<Vector3> sample = Sample2D(size, seed, radius, k);

        foreach(Vector3 pos in sample)
        {
            Gizmos.DrawWireSphere(transform.position + pos, 0.1f);
            if (showRadius)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position + pos, radius / 2);
                Gizmos.color = Color.white;
            }
        }
    }

    public static List<Vector3> Sample2D(Vector3 regionSize, int seed, float radius, int k)
    {
        if (radius < float.Epsilon) return new List<Vector3>();

        List<Vector3> points = new List<Vector3>();

        System.Random hash = new System.Random(seed);
        float cellSize = radius / 1.414213f;

        int[,] grid = new int[(int)Mathf.Ceil(regionSize.x / cellSize), (int)Mathf.Ceil(regionSize.z / cellSize)];
        List<Vector3> spawnPoints = new List<Vector3>
            {
                Vector3.zero
            };
        points.Add(spawnPoints[0]);
        grid[(int)(spawnPoints[0].x / cellSize), (int)(spawnPoints[0].z / cellSize)] = points.Count;

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = hash.Next(0, spawnPoints.Count);
            Vector3 spawnCentre = spawnPoints[spawnIndex];

            bool candidateAccepted = false;
            float val = (float)hash.NextDouble();
            float r = radius + 0.00001f;
            float pi = 2 * Mathf.PI;

            for (int i = 0; i < k; i++)
            {
                float theta = pi * (val + 1.0f * i / k);

                float x = spawnCentre.x + r * Mathf.Cos(theta);
                float y = spawnCentre.z + r * Mathf.Sin(theta);

                Vector3 candidate = new Vector3(x, 0, y);
                if (IsValid2D(candidate, regionSize, cellSize, grid, points, radius))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.z / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
                spawnPoints.RemoveAt(spawnIndex);
        }

        return points;
    }
    private static bool IsValid2D(Vector3 candidate, Vector3 sampleRegionSize, double cellSize, int[,] grid, List<Vector3> points, float radius)
    {
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.z >= 0 && candidate.z < sampleRegionSize.z)
        {
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.z / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        Vector3 disc = points[pointIndex];
                        double sqrDst = (candidate - disc).sqrMagnitude;
                        if (sqrDst < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

}
