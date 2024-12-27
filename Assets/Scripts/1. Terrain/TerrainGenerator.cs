using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{
    // Компоненти та матеріали
    Mesh mesh;
    public Renderer objectRenderer; // Рендер об'єкта для зміни матеріалу
    public Gradient gradient; // Градієнт для генерації кольорів
    
    // Слайдери для налаштування параметрів
    public Slider heightSlider; 
    public Slider detailSlider;
    public Slider noiseScaleSlider;

    // Матеріали для перемикання
    public Material frameMaterial;
    public Material gradientMaterial;

    // Параметри генерації
    public int baseXSize = 20;
    public int baseZSize = 20;
    private readonly int maxSegments = 1000;

    // Кількість сегментів по осях
    private int xSegments;
    private int zSegments;

    // Висота та масштаб шуму
    private float heightMultiplier = 2f;
    private float noiseScale = 0.3f;

    // Дані для меша
    public Vector3[] vertices; // Вершини
    public int[] triangles; // Трикутники
    private Color[] colors; // Кольори

    // Мінімальна та максимальна висота терейну
    public float minTerrainHeight;
    public float maxTerrainHeight;

    private bool frameMat = true; // Поточний матеріал

    public event Action OnTerrainGenerated; // Подія після генерації терейну

    void Start()
    {
        if (frameMaterial != null)
        {
            frameMaterial.color = Color.white;
        }

        // Ініціалізація меша та виклик першої генерації
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        UpdateTerrain();
    }

    public void UpdateTerrain()
    {
        // Розраховуємо кількість сегментів та параметри терейну
        (xSegments, zSegments) = (Mathf.Clamp(Mathf.RoundToInt(baseXSize * detailSlider.value), 1, maxSegments), Mathf.Clamp(Mathf.RoundToInt(baseZSize * detailSlider.value), 1, maxSegments));
        (heightMultiplier, noiseScale) = (heightSlider.value, noiseScaleSlider.value);

        // Генерація форми та оновлення меша
        CreateShape();
        UpdateMesh();

    }

    void CreateShape()
    {
        // Ініціалізація масивів для вершин та трикутників
        vertices = new Vector3[(xSegments + 1) * (zSegments + 1)];
        triangles = new int[xSegments * zSegments * 6];
        minTerrainHeight = float.MaxValue;
        maxTerrainHeight = float.MinValue;

        object lockObj = new(); // Об'єкт для синхронізації потоків

        // Паралельна генерація вершин
        Parallel.For(0, zSegments + 1, z =>
        {
            GenerateVertices(z, lockObj);
        });

        // Паралельна генерація трикутників
        Parallel.For(0, zSegments, z =>
        {
            GenerateTriangles(z);
        });

        // Генерація кольорів та виклик події
        GenerateColors();
        OnTerrainGenerated?.Invoke();
    }

    void GenerateVertices(int z, object lockObj)
    {
        // Генерація вершин для поточного рядка
        for (int x = 0; x <= xSegments; x++)
        {
            int i = z * (xSegments + 1) + x;
            float xCoord = (float)x / xSegments * baseXSize;
            float zCoord = (float)z / zSegments * baseZSize;
            float y = Mathf.PerlinNoise(xCoord * noiseScale, zCoord * noiseScale) * heightMultiplier;

            vertices[i] = new Vector3(xCoord, y, zCoord);

            // Оновлення мінімальної та максимальної висоти (з блокуванням)
            lock (lockObj)
            {
                minTerrainHeight = Mathf.Min(minTerrainHeight, y);
                maxTerrainHeight = Mathf.Max(maxTerrainHeight, y);
            }
        }
    }

    void GenerateTriangles(int z)
    {
        // Генерація трикутників для поточного рядка
        int localVert = z * (xSegments + 1), localTris = z * (xSegments * 6);

        for (int x = 0; x < xSegments; x++)
        {
            triangles[localTris + 0] = localVert + 0; triangles[localTris + 1] = localVert + xSegments + 1;
            triangles[localTris + 2] = localVert + 1; triangles[localTris + 3] = localVert + 1;
            triangles[localTris + 4] = localVert + xSegments + 1; triangles[localTris + 5] = localVert + xSegments + 2;

            localVert++;
            localTris += 6;
        }
    }

    void GenerateColors()
    {
        // Генерація кольорів вершин на основі висоти та градієнта
        colors = vertices.Select(v => Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, v.y))
                 .Select(height => gradient.Evaluate(height))
                 .ToArray();
    }

    void UpdateMesh()
    {
        // Оновлення даних меша
        mesh.Clear();
        (mesh.vertices, mesh.triangles, mesh.colors) = (vertices, triangles, colors);
        mesh.RecalculateNormals(); // Перерахунок нормалей
    }

    public void ChangeObjectMaterial()
    {
        // Зміна матеріалу об'єкта та оновлення терейну
        objectRenderer.material = frameMat ? gradientMaterial : frameMaterial;
        frameMat = !frameMat;

        UpdateTerrain();
    }
}