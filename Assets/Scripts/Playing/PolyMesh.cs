﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolyMesh : SingletonBehaviour<PolyMesh>
{
    public int Count { get; private set; } = 6;
    public float Radius { get; private set; } = 3.5f;

    public const int MINIMUM_COUNT = 3;

    public Vector2[] SelectedTriangleUV => new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
    };
    public Vector2[] NonSelectedTriangleUV => new Vector2[]
    {
        new Vector2(1, 1),
        new Vector2(0, 1),
        new Vector2(1, 0),
    };

    private MeshFilter m_meshFilter;
    public MeshFilter meshFilter {
        get {
            if (m_meshFilter == null)
                return m_meshFilter = GetComponent<MeshFilter>();
            return m_meshFilter;
        }
    }
    private MeshRenderer m_meshRenderer;
    public MeshRenderer meshRenderer {
        get {
            if (m_meshRenderer == null)
                return m_meshRenderer = GetComponent<MeshRenderer>();
            return m_meshRenderer;
        }
    }

    private void Start()
    {
        Skin.CurrentlyLoadedSkin.Apply();
    }

    public void UpdateMesh()
    {
        Generate(Radius, Count);
    }

    public Vector3 PosForVertices(int i, int n, float radius)
    {
        return new Vector3(radius * Mathf.Sin((2 * Mathf.PI * i) / n), radius * Mathf.Cos((2 * Mathf.PI * i) / n), 0f);
    }

    public Mesh mesh;

    public void Generate(float radius, int n)
    {
        if (n < MINIMUM_COUNT)
            throw new Exception("Cannot have less than 3 sides");

        Radius = radius;
        Count = n;

        mesh = new Mesh();
        meshFilter.mesh = mesh;

        //Verticies
        //Double the vertices for texturing individual triangles
        Vector3[] verticies = new Vector3[n*2+1];
        for (int i = 0; i < n; i++)
        {
            verticies[i] = PosForVertices(i, n, radius);
            verticies[i+n] = PosForVertices(i, n, radius);
        }
        verticies[n*2] = new Vector3(0, 0, 0);

        //Triangles
        //For n = 3, (0, 4),(1,5),(2,6),(3,7),(8 is middle)
        int[] triangles = new int[(n) * 3];
        for (int i = 0; i < n; i++)
        {
            triangles[i * 3] = n*2;
            triangles[(i * 3) + 1] = i;
            triangles[(i * 3) + 2] = (i + 1) % n + n;
        }

        //Normals
        Vector3[] normals = new Vector3[verticies.Length];
        for (int i = 0; i < verticies.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        //UVs

#if DEBUG
        Vector2[] uvs = new Vector2[verticies.Length];
        for(int i = 0; i < n; i++)
        {
            uvs[i] = new Vector2(0, 0);
            uvs[i + n] = new Vector2(1, 0);
        }

        int slice = AimController.Instance.SelectedSlice;
        uvs[slice] = new Vector2(1, 0);
        uvs[(slice + 1) % n + n] = new Vector2(1, 1);

        uvs[n*2] = new Vector2(0.5f, 0.5f);
#else
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(1,0),
            new Vector2(0,0),
            new Vector2(0,0),
            new Vector2(0,0),
            new Vector2(0,0),
            new Vector2(0,0),

            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(1,0),
            new Vector2(1,0),
            new Vector2(1,0),
            new Vector2(1,0),

            new Vector2(0.5f, 0.5f),
        };
#endif

        if(uvs.Length != verticies.Length)
        {
            Debug.LogError("uv length is " + uvs.Length + " vertices length is " + verticies.Length);
        }

        //initialise
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.name = n + " sided polygon";
    }
}