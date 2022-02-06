using CP.Common.Maths;
using CPWS.WorldGenerator.ConvexHull;
using CPWS.WorldGenerator.PoissonDisc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class AsteroidBuilder : MonoBehaviour
{
    public ShapeBuilder shapeBuilder;
    public NoiseData data;

    public Material material;

    private Dictionary<ShapeBuilder.Direction, GameObject> sides = new Dictionary<ShapeBuilder.Direction, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Build()
    {
        ModelData mDatas = shapeBuilder.Build();
        
        foreach (SubModelData mData in mDatas.SubModels)
        {
            mData.Vertices = HCFilter(mData.Vertices, mData.Triangles);
            //mData.Vertices = RelaxVertices(mData.Vertices, mData.Triangles);

            if (!sides.TryGetValue(mData.Direction, out GameObject side) || side == null)
            {
                if (sides.ContainsKey(mData.Direction))
                    sides.Remove(mData.Direction);
                side = new GameObject("Side-" + mData.Direction.ToString());
                sides.Add(mData.Direction, side);
            }

            side.transform.parent = gameObject.transform;

            Mesh m;
            if (!side.TryGetComponent(out MeshFilter filter))
            {
                filter = side.AddComponent<MeshFilter>();
            }
            if (!side.TryGetComponent(out MeshRenderer renderer))
            {
                renderer = side.AddComponent<MeshRenderer>();
            }
            if (filter.sharedMesh != null)
                m = filter.sharedMesh;
            else
                m = new Mesh();
            renderer.sharedMaterial = material;

            SubModelData mData2 = ApplyNoise(mData);
            //SubModelData mData2 = mData;
            m.triangles = null;
            m.normals = null;
            m.vertices = mData2.Vertices;
            m.triangles = mData2.Triangles;
            m.normals = mData2.Normals;
            //m.RecalculateNormals();

            m.name = gameObject.name;
            filter.sharedMesh = m;
        }
    }

    public Vector3[] RelaxVertices(Vector3[] vertices, int[] triangles)
    {
        float offset = 0;
        for(int i = 0; i < triangles.Length; i += 6)
        {
            Vector3 pos0 = vertices[triangles[i]];
            Vector3 pos1 = vertices[triangles[i + 1]];
            Vector3 pos2 = vertices[triangles[i + 2]];

            offset += (pos2 - pos0).magnitude;
            offset += (pos1 - pos2).magnitude;

            Vector3 pos3 = vertices[triangles[i + 3]];
            Vector3 pos4 = vertices[triangles[i + 4]];
            Vector3 pos5 = vertices[triangles[i + 5]];

            offset += (pos4 - pos3).magnitude;
            offset += (pos5 - pos4).magnitude;
        }
        offset /= triangles.Length;

        for (int i = 0; i < triangles.Length; i += 6)
        {
            Vector3 pos0 = vertices[triangles[i]];
            Vector3 pos1 = vertices[triangles[i + 1]];
            Vector3 pos2 = vertices[triangles[i + 2]];
            Vector3 pos4 = vertices[triangles[i + 4]];

            Vector3 pos01 = (pos0 + pos1) / 2;

            vertices[triangles[i]] -= ((pos1 - pos0).normalized * offset / 2) * 0.1f;
            vertices[triangles[i + 1]] += ((pos1 - pos0).normalized * offset / 2) * 0.1f;

            Vector3 pos02 = (pos0 + pos2) / 2;
            vertices[triangles[i]] -= ((pos2 - pos0).normalized * offset / 2) * 0.1f;
            vertices[triangles[i + 2]] += ((pos2 - pos0).normalized * offset / 2) * 0.1f;

            Vector3 pos24 = (pos2 + pos4) / 2;
            vertices[triangles[i]] -= ((pos4 - pos2).normalized * offset / 2) * 0.1f;
            vertices[triangles[i + 2]] += ((pos4 - pos2).normalized * offset / 2) * 0.1f;

            Vector3 pos14 = (pos1 + pos4) / 2;
            vertices[triangles[i]] -= ((pos4 - pos1).normalized * offset / 2) * 0.1f;
            vertices[triangles[i + 2]] += ((pos4 - pos1).normalized * offset / 2) * 0.1f;
        }

        return vertices;
    }

    public static Mesh LaplacianFilter(Mesh mesh, int times = 1)
    {
        mesh.vertices = LaplacianFilter(mesh.vertices, mesh.triangles, times);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Vector3[] LaplacianFilter(Vector3[] vertices, int[] triangles, int times)
    {
        var network = VertexConnection.BuildNetwork(triangles);
        for (int i = 0; i < times; i++)
        {
            vertices = LaplacianFilter(network, vertices, triangles);
        }
        return vertices;
    }

    static Vector3[] LaplacianFilter(Dictionary<int, VertexConnection> network, Vector3[] origin, int[] triangles)
    {
        Vector3[] vertices = new Vector3[origin.Length];
        for (int i = 0, n = origin.Length; i < n; i++)
        {
            var connection = network[i].Connection;
            var v = Vector3.zero;
            foreach (int adj in connection)
            {
                v += origin[adj];
            }
            vertices[i] = v / connection.Count;
        }
        return vertices;
    }

    public static Mesh HCFilter(Mesh mesh, int times = 5, float alpha = 0.5f, float beta = 0.75f)
    {
        mesh.vertices = HCFilter(mesh.vertices, mesh.triangles, times, alpha, beta);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    static Vector3[] HCFilter(Vector3[] vertices, int[] triangles, int times = 5, float alpha = 0.5f, float beta = 0.75f)
    {
        alpha = Mathf.Clamp01(alpha);
        beta = Mathf.Clamp01(beta);

        var network = VertexConnection.BuildNetwork(triangles);

        Vector3[] origin = new Vector3[vertices.Length];
        Array.Copy(vertices, origin, vertices.Length);
        for (int i = 0; i < times; i++)
        {
            vertices = HCFilter(network, origin, vertices, triangles, alpha, beta);
        }
        return vertices;
    }

    public static Vector3[] HCFilter(Dictionary<int, VertexConnection> network, Vector3[] o, Vector3[] q, int[] triangles, float alpha, float beta)
    {
        Vector3[] p = LaplacianFilter(network, q, triangles);
        Vector3[] b = new Vector3[o.Length];

        for (int i = 0; i < p.Length; i++)
        {
            b[i] = p[i] - (alpha * o[i] + (1f - alpha) * q[i]);
        }

        for (int i = 0; i < p.Length; i++)
        {
            var adjacents = network[i].Connection;
            var bs = Vector3.zero;
            foreach (int adj in adjacents)
            {
                bs += b[adj];
            }
            p[i] = p[i] - (beta * b[i] + (1 - beta) / adjacents.Count * bs);
        }

        return p;
    }


    public SubModelData ApplyNoise(SubModelData mData)
    {
        float[][] verts = new float[mData.Vertices.Length][];

        for (int i = 0; i < mData.Vertices.Length; i++)
        {
            Vector3 vert = mData.Vertices[i];
            verts[i] = new float[] { vert.x, vert.y, vert.z };
        }

        float[] noise = data.GetNoiseFromPositions(verts, shapeBuilder.resolution);

        for (int i = 0; i < mData.Vertices.Length; ++i)
        {
            mData.Vertices[i] += mData.Vertices[i].normalized * noise[i];
        }

        return mData;
    }
}

public class VertexConnection
{

    public HashSet<int> Connection { get { return connection; } }

    HashSet<int> connection;

    public VertexConnection()
    {
        this.connection = new HashSet<int>();
    }

    public void Connect(int to)
    {
        connection.Add(to);
    }

    public static Dictionary<int, VertexConnection> BuildNetwork(int[] triangles)
    {
        var table = new Dictionary<int, VertexConnection>();

        for (int i = 0, n = triangles.Length; i < n; i += 3)
        {
            int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
            if (!table.ContainsKey(a))
            {
                table.Add(a, new VertexConnection());
            }
            if (!table.ContainsKey(b))
            {
                table.Add(b, new VertexConnection());
            }
            if (!table.ContainsKey(c))
            {
                table.Add(c, new VertexConnection());
            }
            table[a].Connect(b); table[a].Connect(c);
            table[b].Connect(a); table[b].Connect(c);
            table[c].Connect(a); table[c].Connect(b);
        }

        return table;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(AsteroidBuilder))]
public class AsteroidBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AsteroidBuilder pg = target as AsteroidBuilder;
        base.OnInspectorGUI();

        if (GUILayout.Button("Build"))
        {
            pg.Build();
        }
    }
}
#endif