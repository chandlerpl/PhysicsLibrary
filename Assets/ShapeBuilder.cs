using CP.Common.Maths;
using CPWS.WorldGenerator.ConvexHull;
using CPWS.WorldGenerator.PoissonDisc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class ShapeBuilder
{
    public enum Direction { Up, Down, Left, Right, Forward, Back }

    private readonly static Dictionary<Vector3, Direction> directionMapping = new Dictionary<Vector3, Direction>()
    {
        { Vector3.up, Direction.Up },
        { Vector3.down, Direction.Down },
        { Vector3.left, Direction.Left },
        { Vector3.right, Direction.Right },
        { Vector3.forward, Direction.Forward },
        { Vector3.back, Direction.Back }
    };

    private readonly static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    public enum Shape {  Plane, Cube, Sphere, Random }

    public Shape shape = Shape.Sphere;
    public int resolution = 2;
    public double discRadius = 0.25;
    public uint discSeed = 4354758;

    public ModelData Build()
    {
        ModelData data = null;

        switch (shape)
        {
            case Shape.Sphere:
                BuildSphere(ref data);
                break;
            case Shape.Cube:
                BuildCube(ref data);
                break;
            case Shape.Plane:
                BuildPlane(ref data);
                break;
            case Shape.Random:
                BuildRandom(ref data);
                break;
        }

        return data;
    }

    private void BuildCube(ref ModelData data)
    {
        data = new ModelData();
        data.SubModels = new SubModelData[6];

        for (int side = 0; side < 6; ++side)
        {
            data.SubModels[side] = new SubModelData(data)
            {
                Direction = directionMapping[directions[side]],
                Vertices = new Vector3[resolution * resolution],
                Triangles = new int[(resolution - 1) * (resolution - 1) * 6]
            };

            Vector3 localUp = directions[side] / 2;
            BuildPlane(ref data.SubModels[side], localUp, new Vector3(localUp.y, localUp.z, localUp.x));
        }
    }

    private void BuildPlane(ref ModelData data)
    {
        data = new ModelData();
        data.SubModels = new SubModelData[1];
        data.SubModels[0] = new SubModelData(data)
        {
            Direction = directionMapping[directions[0]]
        };

        BuildPlane(ref data.SubModels[0], Vector3.up / 2, new Vector3(Vector3.up.y, Vector3.up.z, Vector3.up.x));
    }

    private void BuildPlane(ref SubModelData data, Vector3 localUp, Vector3 axisA)
    {
        if (data.Vertices == null)
        {
            data.Vertices = new Vector3[resolution * resolution];
            data.Triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        }
        int triangle = 0;

        Vector3 axisB = Vector3.Cross(localUp == Vector3.zero ? Vector3.up : localUp * 2, axisA);
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = (x + y * resolution);
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;

                data.Vertices[i] = pointOnUnitCube;

                if (x != resolution - 1 && y != resolution - 1)
                {
                    data.Triangles[triangle++] = i;
                    data.Triangles[triangle++] = i + resolution + 1;
                    data.Triangles[triangle++] = i + resolution;

                    data.Triangles[triangle++] = i;
                    data.Triangles[triangle++] = i + 1;
                    data.Triangles[triangle++] = i + resolution + 1;
                }
            }
        }
    }

    private void BuildSphere(ref ModelData data)
    {
        BuildCube(ref data);

        for (int side = 0; side < 6; ++side)
        {
            for (int i = 0; i < data.SubModels[side].Vertices.Length; ++i)
            {
                data.SubModels[side].Vertices[i] = data.SubModels[side].Vertices[i].normalized / 2;
            }
        }
    }

    private void BuildRandom(ref ModelData data)
    {
        data = new ModelData();
        data.SubModels = new SubModelData[1];

        data.SubModels[0] = new SubModelData(data)
        {
            Direction = directionMapping[directions[0]],
            Vertices = new Vector3[resolution * resolution],
            Triangles = new int[(resolution - 1) * (resolution - 1) * 6]
        };

        PoissonDiscSampling sampling = new PoissonDiscSampling(discRadius, discSeed);

        List<Vector3D> points = new List<Vector3D>();
        List<PoissonDisc> tPoints = sampling.Sample3D(new Vector3D(1, 1, 1), true);

        foreach (PoissonDisc disc in tPoints)
        {
            points.Add(new Vector3D(disc.position.X, disc.position.Y, disc.position.Z));
        }

        ConvexHull3D hull = new ConvexHull3D();
        points = hull.ConstructHull(points);

        Vector3 midPoint = new Vector3();
        for (int i = 0; i < points.Count; ++i)
        {
            Vector3D pos1D = points[i];
            midPoint += new Vector3((float)pos1D.X, (float)pos1D.Y, (float)pos1D.Z);
        }
        midPoint /= points.Count;

        List<Vector3> points2 = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int i = 0; i < points.Count; i += 3)
        {
            Vector3D pos1D = points[i];
            Vector3D pos2D = points[i + 1];
            Vector3D pos3D = points[i + 2];
            Vector3 pos1 = new Vector3((float)pos1D.X, (float)pos1D.Y, (float)pos1D.Z) - midPoint;
            Vector3 pos2 = new Vector3((float)pos2D.X, (float)pos2D.Y, (float)pos2D.Z) - midPoint;
            Vector3 pos3 = new Vector3((float)pos3D.X, (float)pos3D.Y, (float)pos3D.Z) - midPoint;

            if(!points2.Contains(pos1))
            {
                points2.Add(pos1);
            }
            triangles.Add(points2.IndexOf(pos1));

            if (!points2.Contains(pos2))
            {
                points2.Add(pos2);
            }
            triangles.Add(points2.IndexOf(pos2));

            if (!points2.Contains(pos3))
            {
                points2.Add(pos3);
            }
            triangles.Add(points2.IndexOf(pos3));
        }

        data.SubModels[0].Vertices = points2.ToArray();
        data.SubModels[0].Triangles = triangles.ToArray();

        data = Remesh(data);
    }
    private static VectorComparer comparer = new VectorComparer();

    private ModelData Remesh(ModelData data)
    {
        ModelData sphere = null;
        BuildSphere(ref sphere);
        sphere = MergeModel(sphere);

        foreach(SubModelData subData in sphere.SubModels)
        {
            for(int i = 0; i < subData.Vertices.Length; ++i)
            {
                Vector3 pos = subData.Vertices[i] * 5;
                CPRay ray = new CPRay(pos, -subData.Vertices[i], 10);
                float tNear = float.MaxValue;
                bool intersected = false;

                for(int j = 0; j < data.SubModels.Length; ++j)
                {
                    for(int k = 0; k < data.SubModels[j].Triangles.Length; k += 3)
                    {
                        Vector3 pos0 = data.SubModels[j].Vertices[data.SubModels[j].Triangles[k]];
                        Vector3 pos1 = data.SubModels[j].Vertices[data.SubModels[j].Triangles[k + 1]];
                        Vector3 pos2 = data.SubModels[j].Vertices[data.SubModels[j].Triangles[k + 2]];

                        if (CPCollider.RayTriangleIntersection(ray, pos0, pos1, pos2, out float t) && t < tNear)
                        {
                            tNear = t;
                            intersected = true;
                        }
                    }
                }

                if(intersected)
                {
                    subData.Vertices[i] = pos + (ray.Direction * tNear);
                }
            }
        }

        return sphere;
    }

    public ModelData MergeModel(ModelData data)
    {
        ModelData newData = new ModelData();
        newData.SubModels = new SubModelData[1];
        newData.SubModels[0] = new SubModelData(newData);

        List<Vector3> points2 = new List<Vector3>();
        List<int> triangles = new List<int>();
        foreach (SubModelData mData in data.SubModels)
        {
            for (int k = 0; k < mData.Triangles.Length; k += 3)
            {
                Vector3 pos0 = mData.Vertices[mData.Triangles[k]];
                Vector3 pos1 = mData.Vertices[mData.Triangles[k + 1]];
                Vector3 pos2 = mData.Vertices[mData.Triangles[k + 2]];

                if (!points2.Contains(pos0, comparer))
                {
                    points2.Add(pos0);
                }
                triangles.Add(points2.FindIndex(v =>
                {
                    return pos0.ApproxEquals(v, 0.001f);
                }));

                if (!points2.Contains(pos1, comparer))
                {
                    points2.Add(pos1);
                }
                triangles.Add(points2.FindIndex(v =>
                {
                    return pos1.ApproxEquals(v, 0.001f);
                }));

                if (!points2.Contains(pos2, comparer))
                {
                    points2.Add(pos2);
                }
                triangles.Add(points2.FindIndex(v =>
                {
                    return pos2.ApproxEquals(v, 0.001f);
                }));
            }
        }

        newData.SubModels[0].Vertices = points2.ToArray();
        newData.SubModels[0].Triangles = triangles.ToArray();
        return newData;
    }
}

public static class Vector3Utils
{
    public static bool ApproxEquals(this Vector3 v, Vector3 other, float offset = float.Epsilon)
    {
        float x = v.x - other.x;
        float y = v.y - other.y;
        float z = v.z - other.z;

        if (x > offset || x < -offset)
            return false;
        if (z > offset || z < -offset)
            return false;
        if (y > offset || y < -offset)
            return false;

        return true;
    }
}

public class VectorComparer : IEqualityComparer<Vector3>
{
    public bool Equals(Vector3 test, Vector3 other)
    {
            return test.ApproxEquals(other, 0.001f);
    }

    public int GetHashCode(Vector3 obj)
    {
        return obj.GetHashCode();
    }
}
