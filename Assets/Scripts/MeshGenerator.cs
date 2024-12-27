using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    public Renderer objectRenderer;
    Vector3[] vertices;
    int[] triangles;
    Color[] colors;
    private GameObject waterObject;
    private Mesh waterMesh;
    MeshRenderer waterMeshRenderer;

    public int baseXSize = 20;
    public int baseZSize = 20;
    private int xSegments;
    private int zSegments;

    public float heightMultiplier = 2f;
    public float noiseScale = 0.3f;

    public Gradient gradient;
    private float minTerrainHeight;
    private float maxTerrainHeight;

    public Slider heightSlider;
    public Slider detailSlider;
    public Slider noiseScaleSlider;

    public Material frameMaterial;
    public Material gradientMaterial;
    public Material waterMaterial;
    private bool frameMat = true;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateWaterObject();
        UpdateTerrain();
    }

    public void UpdateTerrain()
    {
        (xSegments, zSegments) = (Mathf.Max(1, Mathf.RoundToInt(baseXSize * detailSlider.value)), Mathf.Max(1, Mathf.RoundToInt(baseZSize * detailSlider.value)));
        heightMultiplier = heightSlider.value;
        noiseScale = noiseScaleSlider.value;

        CreateShape();
        UpdateMesh();
        UpdateWaterMesh();
    }

    void CreateShape()
    {
        GenerateVertices();
        GenerateTriangles();
        GenerateColors();
    }

    void GenerateVertices()
    {
        vertices = new Vector3[(xSegments + 1) * (zSegments + 1)];
        minTerrainHeight = float.MaxValue;
        maxTerrainHeight = float.MinValue;

        int index = 0;
        for (int z = 0; z <= zSegments; z++)
        {
            for (int x = 0; x <= xSegments; x++)
            {
                float xCoord = (float)x / xSegments * baseXSize;
                float zCoord = (float)z / zSegments * baseZSize;

                float y = Mathf.PerlinNoise(xCoord * noiseScale, zCoord * noiseScale) * heightMultiplier;
                vertices[index] = new Vector3(xCoord, y, zCoord);

                // Оновлюємо мінімальні та максимальні значення висоти
                minTerrainHeight = Mathf.Min(minTerrainHeight, y);
                maxTerrainHeight = Mathf.Max(maxTerrainHeight, y);

                index++;
            }
        }
    }

    void GenerateTriangles()
    {
        triangles = new int[xSegments * zSegments * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSegments; z++)
        {
            for (int x = 0; x < xSegments; x++)
            {
                // Перший трикутник
                triangles[tris++] = vert;
                triangles[tris++] = vert + xSegments + 1;
                triangles[tris++] = vert + 1;

                // Другий трикутник
                triangles[tris++] = vert + 1;
                triangles[tris++] = vert + xSegments + 1;
                triangles[tris++] = vert + xSegments + 2;

                vert++;
            }
            vert++;
        }
    }

    void GenerateColors()
    {
        colors = vertices
        .Select(vertex => {
            float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertex.y);
            return gradient.Evaluate(height);
        })
        .ToArray();
    }

    public void ChangeObjectMaterial()
    {
        objectRenderer.material = frameMat ? gradientMaterial : frameMaterial;
        frameMat = !frameMat;

        UpdateTerrain();
    }
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }

    void CreateWaterObject()
    {
        waterObject = new GameObject("WaterMesh");
        waterObject.transform.position = Vector3.zero;
        MeshFilter waterMeshFilter = waterObject.AddComponent<MeshFilter>();
        waterMeshRenderer = waterObject.AddComponent<MeshRenderer>();

        waterMesh = new Mesh();
        waterMeshFilter.mesh = waterMesh;
        waterMeshRenderer.material = waterMaterial;
    }
    void UpdateWaterMesh()
    {
        Vector3[] waterVertices = GenerateWaterVertices();
        int[] waterTriangles = GenerateWaterTriangles(waterVertices);

        waterMesh.Clear();
        waterMesh.vertices = waterVertices;
        waterMesh.triangles = waterTriangles;
        waterMesh.RecalculateNormals();
    }

    Vector3[] GenerateWaterVertices()
    {
        Vector3[] waterVertices = new Vector3[vertices.Length];

        float averageTerrainHeight = vertices.Average(vertex => vertex.y);
        float waterHeight = averageTerrainHeight * 0.7f;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 terrainVertex = vertices[i];
            waterVertices[i] = terrainVertex.y < waterHeight
                ? new Vector3(terrainVertex.x, waterHeight, terrainVertex.z)
                : terrainVertex;
        }

        return waterVertices;
    }

    int[] GenerateWaterTriangles(Vector3[] waterVertices)
    {
        List<int> waterTriangles = new List<int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            if (waterVertices[v1].y == waterVertices[v2].y && waterVertices[v2].y == waterVertices[v3].y)
            {
                waterTriangles.Add(v1);
                waterTriangles.Add(v2);
                waterTriangles.Add(v3);
            }
        }

        return waterTriangles.ToArray();
    }
}