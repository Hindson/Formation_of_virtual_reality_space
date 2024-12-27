using UnityEngine;

public class MeshUpdater : MonoBehaviour
{
    private TerrainGenerator terrainGenerator;
    private WaterGenerator waterGenerator;

    void Start()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        waterGenerator = GetComponent<WaterGenerator>();
    }

    public void RefreshMesh()
    {
        terrainGenerator.UpdateTerrain();
        waterGenerator.UpdateWaterMesh();
    }
}
