using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WaterGenerator : MonoBehaviour
{
    private GameObject waterObject; // Об'єкт води
    public Mesh waterMesh; // Меш води
    private MeshRenderer waterMeshRenderer; // Рендерер для відображення води

    public Material staticMaterial; // Матеріал для статичної води
    public Material dynamicMaterial; // Матеріал для динамічної води
    private bool staticWater = true; // Прапор статичної чи динамічної води

    private TerrainGenerator terrainGenerator; // Генератор ландшафту

    public float waveHeight = 0.5f;
    public float waveSpeed = 1f;
    public float waveFrequency = 0.2f;
    public int waveCount = 3;

    private Vector3[] originalVertices; // Оригінальні вершини меша
    private Vector3[] displacedVertices; // Зміщені вершини меша

    public float waterDepth = 0.5f; // Глибина води
    public float transparency = 0.5f; // Прозорість води
    public Slider depthSlider; // Слайдер для глибини води
    public Slider transparencySlider; // Слайдер для прозорості води

    void Start()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        terrainGenerator.OnTerrainGenerated += GenerateWater;

        // Прив'язуємо зміни слайдерів до відповідних методів
        depthSlider.onValueChanged.AddListener(OnDepthChanged);
        transparencySlider.onValueChanged.AddListener(OnTransparencyChanged);
    }

    // Генерація води
    void GenerateWater()
    {
        if (waterObject == null)
        {
            CreateWaterObject();
        }
        UpdateWaterMesh(); // Оновлюємо меш води
    }

    // Створення об'єкта води
    void CreateWaterObject()
    {
        waterObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterObject.transform.localScale = new Vector3(terrainGenerator.baseXSize / 10f, 1, terrainGenerator.baseZSize / 10f);

        waterMeshRenderer = waterObject.GetComponent<MeshRenderer>();
        waterMeshRenderer.material = staticMaterial;

        MeshFilter meshFilter = waterObject.GetComponent<MeshFilter>();
        waterMesh = meshFilter.mesh;
        waterMesh.RecalculateNormals();
        meshFilter.mesh = waterMesh;

        originalVertices = waterMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];

        UpdateTransparency();
    }

    void Update()
    {
        if (!staticWater)
        {
            SimulateWaves(); // Симуляція хвиль для динамічної води
        }
    }

    // Оновлення мешу води
    public void UpdateWaterMesh()
    {
        float waterHeight = Mathf.Lerp(terrainGenerator.minTerrainHeight, terrainGenerator.maxTerrainHeight, 0.2f) + waterDepth;
        if (waterObject != null)
        {
            waterObject.transform.position = new Vector3(terrainGenerator.baseXSize / 2f, waterHeight, terrainGenerator.baseZSize / 2f);
        }
    }

    // Зміна матеріалу води
    public void ChangeObjectMaterial()
    {
        waterMeshRenderer.material = staticWater ? dynamicMaterial : staticMaterial;
        staticWater = !staticWater; // Змінюємо тип води (статична/динамічна)

        UpdateWaterMesh();
    }

    // Симуляція хвиль
    void SimulateWaves()
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            float waveY = Enumerable.Range(0, waveCount)
                .Sum(j =>
                {
                    float frequency = waveFrequency + j * 0.2f;
                    float amplitude = waveHeight / (j + 1);

                    float sinWave = Mathf.Sin((originalVertices[i].x + Time.time * waveSpeed) * frequency) * amplitude;
                    float cosWave = Mathf.Cos((originalVertices[i].z + Time.time * waveSpeed) * frequency) * amplitude;

                    return sinWave + cosWave;
                });

            displacedVertices[i] = new Vector3(originalVertices[i].x, originalVertices[i].y + waveY, originalVertices[i].z);
        }

        waterMesh.vertices = displacedVertices;
        waterMesh.RecalculateNormals();

    }

    // Зміна глибини води через слайдер
    void OnDepthChanged(float newDepth)
    {
        waterDepth = newDepth;
        UpdateWaterMesh();
    }

    // Зміна прозорості води через слайдер
    void OnTransparencyChanged(float newTransparency)
    {
        transparency = newTransparency;
        UpdateTransparency();
    }

    // Оновлення прозорості матеріалу води
    void UpdateTransparency()
    {
        waterMeshRenderer.material.color = new Color(
            waterMeshRenderer.material.color.r,
            waterMeshRenderer.material.color.g,
            waterMeshRenderer.material.color.b,
            transparency
        );
    }
}
