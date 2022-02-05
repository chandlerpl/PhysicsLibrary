using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoiseData")]
public class NoiseData : ScriptableObject
{
    public enum NoiseDirections
    {
        Sphere, X, Y, Z, XY, YZ, XZ
    }

    public Dictionary<NoiseDirections, Vector3> directions = new Dictionary<NoiseDirections, Vector3>() { 
        { NoiseDirections.Sphere, new Vector3(1, 1, 1) },
        { NoiseDirections.X, new Vector3(1, 0, 0) },
        { NoiseDirections.Y, new Vector3(0, 1, 0) },
        { NoiseDirections.Z, new Vector3(0, 0, 1) },
        { NoiseDirections.XY, new Vector3(1, 1, 0) },
        { NoiseDirections.YZ, new Vector3(0, 1, 1) },
        { NoiseDirections.XZ, new Vector3(1, 0, 1) },
    };

    public NoiseDirections noiseDirection;

    public List<NoiseSettings> settings = new List<NoiseSettings>();

    public float[] GetNoiseFromPositions(float[][] positions, int resolution)
    {
        float[] noiseVal = new float[positions.Length];
        /*for (int i = 0; i < positions.Length; i++)
        {
            noiseVal[i] = 1;
        }*/

        if (settings.Count > 0)
        {
            foreach (NoiseSettings setting in settings)
            {
                float[] retNoiseVal = setting.GetNoiseValue(positions, resolution);

                for(int i = 0; i < retNoiseVal.Length;i++)
                {
                    noiseVal[i] += retNoiseVal[i];
                }
            }
        }

        return noiseVal;
    }

    public float GetNoiseFromPosition(Vector3 position, int resolution)
    {
        float noiseVal = 1;

        if(settings.Count > 0)
        {

            position.x += 1;
            position.y += 1;
            position.z += 1;
            foreach (NoiseSettings setting in settings)
            {
                noiseVal += (float)setting.GetNoiseValue(position * resolution, resolution);
            }
        }

        if (noiseVal == 0)
        {
            noiseVal = 1;
        }

        return noiseVal;
    }
}
