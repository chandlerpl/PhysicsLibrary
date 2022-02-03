using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CPOBBCollider : CPCollider
{
    public Vector3 prevRot = new Vector3();
    public Vector3[] matrix = new Vector3[3];
    public Vector3 size = new Vector3(.5f, .5f, .5f);

    public override bool CheckCollision(CPCollider other)
    {
        return other.CheckCollision(this);
    }

    public override bool CheckCollision(CPAABBCollider other)
    {
        throw new System.NotImplementedException();
    }

    public override bool CheckCollision(CPSphereCollider other)
    {
        throw new System.NotImplementedException();
    }

    public override CPRay.CPRayHit CheckCollision(CPRay ray)
    {
        throw new System.NotImplementedException();
    }

    public override bool CheckCollision(CPPlaneCollider other)
    {
        return false;
    }

    public override bool CheckCollision(CPMeshCollider other)
    {
        return false;
    }

    public override bool CheckCollision(CPOBBCollider other)
    {
        float ra, rb;
        Vector3[] R = new Vector3[3];
        Vector3[] AbsR = new Vector3[3];

        Vector3 size = GetSize();
        Vector3 otherSize = other.GetSize();

        for (int i = 0; i < 3; ++i)
            for (int j = 0; j < 3; ++j)
            {
                R[i][j] = Vector3.Dot(GetMatrix()[i], other.GetMatrix()[j]);
                AbsR[i][j] = Mathf.Abs(R[i][j]) + Mathf.Epsilon;
            }

        Vector3 t = other.GetPosition() - GetPosition();
        t = new Vector3(Vector3.Dot(t, GetMatrix()[0]), Vector3.Dot(t, GetMatrix()[1]), Vector3.Dot(t, GetMatrix()[2]));

        // Test axes L = A0, L = A1, L = A2
        for(int i = 0; i < 3; ++i)
        {
            ra = size[i];
            rb = otherSize[0] * AbsR[i][0] + otherSize[1] * AbsR[i][1] + otherSize[2] * AbsR[i][2];

            if (Mathf.Abs(t[i]) > ra + rb)
            {
                Debug.Log("1");
                return false;
            }
        }

        // Test axes L = B0, L = B1, L = B2
        for (int i = 0; i < 3; ++i)
        {
            ra = size[0] * AbsR[0][i] + size[1] * AbsR[1][i] + size[2] * AbsR[2][i];
            rb = otherSize[i];

            if (Mathf.Abs(t[0] * R[0][i] + t[1] * R[1][i] + t[2] * R[2][i]) > ra + rb)
            {
                Debug.Log("2 " + i);
                return false;
            }
        }

        // Test axes L = A0 x B0
        ra = size[1] * AbsR[2][0] + size[2] * AbsR[1][0];
        rb = otherSize[1] * AbsR[0][2] + otherSize[2] * AbsR[0][1];
        if (Mathf.Abs(t[2] * R[1][0] - t[1] * R[2][0]) > ra + rb)
        {
            Debug.Log("3");
            return false;
        }

        // Test axes L = A0 x B1
        ra = size[1] * AbsR[2][1] + size[2] * AbsR[1][1];
        rb = otherSize[0] * AbsR[0][2] + otherSize[2] * AbsR[0][0];
        if (Mathf.Abs(t[2] * R[1][1] - t[1] * R[2][1]) > ra + rb)
        {
            Debug.Log("4");
            return false;
        }

        // Test axes L = A0 x B2
        ra = size[1] * AbsR[2][2] + size[2] * AbsR[1][2];
        rb = otherSize[0] * AbsR[0][1] + otherSize[1] * AbsR[0][0];
        if (Mathf.Abs(t[2] * R[1][2] - t[1] * R[2][2]) > ra + rb)
        {
            Debug.Log("5");
            return false;
        }

        // Test axes L = A1 x B0
        ra = size[0] * AbsR[2][0] + size[2] * AbsR[0][0];
        rb = otherSize[1] * AbsR[1][2] + otherSize[2] * AbsR[1][1];
        if (Mathf.Abs(t[0] * R[2][0] - t[2] * R[0][0]) > ra + rb)
        {
            Debug.Log("6");
            return false;
        }

        // Test axes L = A1 x B1
        ra = size[0] * AbsR[2][1] + size[2] * AbsR[0][1];
        rb = otherSize[0] * AbsR[1][2] + otherSize[2] * AbsR[1][0];
        if (Mathf.Abs(t[0] * R[2][1] - t[2] * R[0][1]) > ra + rb)
        {
            Debug.Log("7");
            return false;
        }

        // Test axes L = A1 x B2
        ra = size[0] * AbsR[2][2] + size[2] * AbsR[0][2];
        rb = otherSize[0] * AbsR[1][1] + otherSize[1] * AbsR[1][0];
        if (Mathf.Abs(t[0] * R[2][2] - t[2] * R[0][2]) > ra + rb)
        {
            Debug.Log("8");
            return false;
        }

        // Test axes L = A2 x B0
        ra = size[0] * AbsR[1][0] + size[1] * AbsR[0][0];
        rb = otherSize[1] * AbsR[2][2] + otherSize[2] * AbsR[2][1];
        if (Mathf.Abs(t[1] * R[0][0] - t[0] * R[1][0]) > ra + rb)
        {
            Debug.Log("9");
            return false;
        }

        // Test axes L = A2 x B1
        ra = size[0] * AbsR[1][1] + size[1] * AbsR[0][1];
        rb = otherSize[0] * AbsR[2][2] + otherSize[2] * AbsR[2][0];
        if (Mathf.Abs(t[1] * R[0][1] - t[0] * R[1][1]) > ra + rb)
        {
            Debug.Log("10");
            return false;
        }

        // Test axes L = A2 x B2
        ra = size[0] * AbsR[1][2] + size[1] * AbsR[0][2];
        rb = otherSize[0] * AbsR[2][1] + otherSize[1] * AbsR[2][0];
        if (Mathf.Abs(t[1] * R[0][2] - t[0] * R[1][2]) > ra + rb)
        {
            Debug.Log("11");
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(GetPosition(), transform.rotation, GetSize());
        Gizmos.matrix = rotationMatrix;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2);

        GetMatrix();
        rotationMatrix = Matrix4x4.TRS(GetPosition(), Quaternion.Euler(GetEuler()), GetSize());
        Gizmos.matrix = rotationMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2);
    }

    public Vector3 GetSize()
    {
        return new Vector3(transform.localScale.x * (size.x / 2), transform.localScale.y * (size.y / 2), transform.localScale.z * (size.z / 2));
    }

    public Vector3[] GetMatrix()
    {
        Vector3 euler = transform.rotation.eulerAngles;

        if(prevRot.x != euler.x || prevRot.y != euler.y || prevRot.z != euler.z)
        {
            float attitude = euler.z;
            float bank = euler.x;
            float heading = euler.y;

            float ca = Mathf.Cos(Mathf.Deg2Rad * attitude);
            float sa = Mathf.Sin(Mathf.Deg2Rad * attitude);

            float cb = Mathf.Cos(Mathf.Deg2Rad * bank);
            float sb = Mathf.Sin(Mathf.Deg2Rad * bank);

            float ch = Mathf.Cos(Mathf.Deg2Rad * heading);
            float sh = Mathf.Sin(Mathf.Deg2Rad * heading);

            Vector3[] retMat = new Vector3[3];

            retMat[0][0] = ch * ca;
            retMat[0][1] = sh * sb - ch * sa * cb;
            retMat[0][2] = ch * sa * sb + sh * cb;

            retMat[1][0] = sa;
            retMat[1][1] = ca * cb;
            retMat[1][2] = -ca * sb;

            retMat[2][0] = -sh * ca;
            retMat[2][1] = sh * sa * cb + ch * sb;
            retMat[2][2] = -sh * sa * sb + ch * cb;

            matrix = retMat;
            prevRot = euler;
        }

        return matrix;
    }

    public Vector3 GetEuler()
    {
        Vector3 euler = new Vector3();

        if (matrix[1][0] > 0.9998)
        {
            euler.x = 0 * Mathf.Rad2Deg;
            euler.y = Mathf.Atan2(matrix[0][2], matrix[2][2]) * Mathf.Rad2Deg;
            euler.z = (Mathf.PI / 2) * Mathf.Rad2Deg;
            return euler;
        }
        else if(matrix[1][0] < -0.9998)
        {
            euler.x = 0 * Mathf.Rad2Deg;
            euler.y = Mathf.Atan2(matrix[0][2], matrix[2][2]) * Mathf.Rad2Deg;
            euler.z = (-Mathf.PI / 2) * Mathf.Rad2Deg;
            return euler;
        }

        euler.y = Mathf.Atan2(-matrix[2][0], matrix[0][0]) * Mathf.Rad2Deg;
        
        euler.x = Mathf.Atan2(-matrix[1][2], matrix[1][1]) * Mathf.Rad2Deg;
        if (matrix[1][1] < 0 || matrix[2][2] < 0)
            euler.x = -euler.x;
        euler.z = Mathf.Asin(matrix[1][0]) * Mathf.Rad2Deg;
        if(matrix[0][0] < 0 || matrix[1][1] < 0)
            euler.z = -euler.z;

        return euler;
    }
}
