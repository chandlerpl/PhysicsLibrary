using System;
using System.Collections.Generic;
using UnityEngine;

public class ModelData
{
    private SubModelData[] subModels;
    private bool isSoftEdges = false;

    public SubModelData[] SubModels { get { return subModels; } set { subModels = value; } }
    public bool IsSoftEdgees { get { return isSoftEdges; } set { isSoftEdges = value; SetNormals(null); } }

    public void StitchSubModels()
    {
        SubModelData newSubModel = Stitch();
        subModels = new SubModelData[1];
        subModels[0] = newSubModel;
    }

    private SubModelData Stitch()
    {
        SubModelData newSubModel = new SubModelData(this)
        {
            Direction = ShapeBuilder.Direction.Up,
        };

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach(SubModelData data in subModels)
        {
            for (int i = 0; i < data.Triangles.Length; i += 3)
            {
                Vector3 one = data.Vertices[data.Triangles[i]];
                Vector3 two = data.Vertices[data.Triangles[i + 1]];
                Vector3 three = data.Vertices[data.Triangles[i + 2]];

                int index = vertices.FindIndex(v => { return VectorPredicate(v, one); });
                if (index == -1)
                {
                    vertices.Add(one);
                    index = vertices.Count - 1;
                }
                triangles.Add(index);
                index = vertices.FindIndex(v => { return VectorPredicate(v, two); });
                if (index == -1)
                {
                    vertices.Add(two);
                    index = vertices.Count - 1;
                }
                triangles.Add(index);
                index = vertices.FindIndex(v => { return VectorPredicate(v, three); });
                if (index == -1)
                {
                    vertices.Add(three);
                    index = vertices.Count - 1;
                }
                triangles.Add(index);
            }
        }

        newSubModel.Vertices = vertices.ToArray();
        newSubModel.Triangles = triangles.ToArray();

        return newSubModel;
    }

    private bool VectorPredicate(Vector3 v, Vector3 w)
    {
        return Math.Abs(v.x - w.x) < 0.001f && Math.Abs(v.y - w.y) < 0.001f && Math.Abs(v.z - w.z) < 0.001f;
    }

    internal Vector3[] CalculateNormals(Vector3[] vertices, int[] triangles)
    {
        if(isSoftEdges)
        {
            SubModelData stitchedVersion = Stitch();
            Vector3[] normals = new Vector3[vertices.Length];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 one = vertices[triangles[i]];
                Vector3 two = vertices[triangles[i + 1]];
                Vector3 three = vertices[triangles[i + 2]];

                Vector3 n1 = Vector3.Cross(two - one, three - one).normalized;

                normals[triangles[i]] += n1;
                normals[triangles[i + 1]] += n1;
                normals[triangles[i + 2]] += n1;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].normalized;
            }

            return normals;
        } else
        {
            Vector3[] normals = new Vector3[vertices.Length];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 one = vertices[triangles[i]];
                Vector3 two = vertices[triangles[i + 1]];
                Vector3 three = vertices[triangles[i + 2]];

                Vector3 n1 = Vector3.Cross(two - one, three - one).normalized;

                normals[triangles[i]] += n1;
                normals[triangles[i + 1]] += n1;
                normals[triangles[i + 2]] += n1;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].normalized;
            }

            return normals;
        }
    }

    internal void SetNormals(Vector3[] normals)
    {
        foreach(SubModelData subModel in subModels)
        {
            subModel.Normals = normals;
        }
    }
}

public class SubModelData
{
    private ModelData parentData;

    private Vector3[] vertices;
    private int[] triangles;

    private Vector3[] normals;

    public SubModelData(ModelData parentData)
    {
        this.parentData = parentData;
    }

    public ShapeBuilder.Direction Direction { get; set; }

    public Vector3[] Vertices { get => vertices; set { vertices = value; normals = null; } }
    public int[] Triangles { get => triangles; set { triangles = value; normals = null; } }
    public Vector3[] Normals { get { if (normals == null) CalculateNormals(); return normals; } internal set => normals = value; }

    private void CalculateNormals()
    {
        if(parentData != null)
        {
            normals = parentData.CalculateNormals(vertices, triangles);
            return;
        }

        normals = new Vector3[vertices.Length];

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 one = vertices[triangles[i]];
            Vector3 two = vertices[triangles[i + 1]];
            Vector3 three = vertices[triangles[i + 2]];

            Vector3 n1 = Vector3.Cross(two - one, three - one).normalized;

            normals[triangles[i]] += n1;
            normals[triangles[i + 1]] += n1;
            normals[triangles[i + 2]] += n1;
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = normals[i].normalized;
        }
    }
}

