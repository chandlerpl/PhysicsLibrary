using CP.Common.Random;
using CP.Procedural.Noise;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public FractalType type;

    public bool enabled = true;

    public bool specificSeed = false;
    [ConditionalHide("specificSeed")]
    public uint seed = 0;
    public uint Seed { get => !specificSeed && seed == 0 ? seed = new RandomHash().GetHash(3) : seed; set { seed = value; noise = null; } }

    public int octaves = 3;
    public float scale = 0.005f;
    public float persistance = 0.5f;

    public float strength = 0.2f;

    public int textureResolution = 1024;

    public Vector2 MinMax = new Vector2(-1, 1);

    private SimplexNoise noise;

    public float[] GetNoiseValue(float[][] positions, int resolution)
    {
        if (!enabled)
        {
            float[] vals = new float[positions.Length];

            return vals;
        }

        if (noise == null) noise = new SimplexNoise(Seed, scale, persistance);
        noise.Scale = scale;
        noise.Persistence = persistance;

        float res = textureResolution;

        for(int i = 0; i < positions.Length;i++)
        {
            positions[i][0] *= res;
            positions[i][1] *= res;
            positions[i][2] *= res;
        }

        return noise.NoiseMap(octaves, type, positions, strength, MinMax.x, MinMax.y).Result;
    }

    public double GetNoiseValue(Vector3 pos, int resolution)
    {
        if (!enabled) return 1;

        if (noise == null) noise = new SimplexNoise(Seed, scale, persistance);
        noise.Scale = scale;
        noise.Persistence = persistance;

        float res = textureResolution / resolution / 2;

        int x = (int)(pos.x * res);
        int y = (int)(pos.y * res);
        int z = (int)(pos.z * res);

        double n = 0;
        switch (type)
        {
            case FractalType.FBM:
                n = noise.FractalFBM(octaves, 3, x, y, z);
                break;
            case FractalType.Billow:
                n = noise.FractalBillow(octaves, 3, x, y, z);
                break;
            case FractalType.Rigid:
                n = noise.FractalRigid(octaves, 3, x, y, z);
                break;
        }
        
        if (n < MinMax.x) n = MinMax.x;
        if (n > MinMax.y) n = MinMax.y;

        return (strength - -strength) / (1 - -1) * (n - 1) + strength;
    }
}
