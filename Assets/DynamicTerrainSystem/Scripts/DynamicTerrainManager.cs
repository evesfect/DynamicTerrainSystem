using UnityEngine;

[System.Serializable]
public class TerrainSettings
{
    [Header("Terrain Dimensions")]
    public Vector2 terrainSize = new Vector2(1000f, 1000f);

    [Header("Height Range")]
    public float minHeight = 0f;
    public float maxHeight = 600f;

    [Header("Resolution")]
    public int heightmapResolution = 513;
}

public class DynamicTerrainManager : MonoBehaviour
{
    [Header("Components")]
    public Terrain terrain;
    public Camera scanCamera;
    public RenderTexture renderTexture;

    [Header("Terrain Settings")]
    public TerrainSettings terrainSettings = new TerrainSettings();

    private void Start()
    {
        ValidateComponents();
        UpdateTerrainAndCamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyRenderTextureToTerrain();
        }
    }

    [ContextMenu("Update Terrain and Camera")]
    public void UpdateTerrainAndCamera()
    {
        if (!ValidateComponents()) return;

        UpdateTerrain();
        UpdateCamera();
        UpdateRenderTexture();
    }

    [ContextMenu("Apply Render Texture to Terrain")]
    public void ApplyRenderTextureToTerrain()
    {
        if (!ValidateComponents()) return;

        // Check if TerrainHeightmapUpdater is attached to terrain, if not add it
        TerrainHeightmapUpdater heightmapUpdater = terrain.GetComponent<TerrainHeightmapUpdater>();
        if (heightmapUpdater == null)
        {
            heightmapUpdater = terrain.gameObject.AddComponent<TerrainHeightmapUpdater>();
        }

        // Set the source heightmap and enable auto sync
        heightmapUpdater.sourceHeightmap = renderTexture;
        heightmapUpdater.autoSync = true;

        // Apply the render texture to heightmap
        heightmapUpdater.ApplyRenderTextureToHeightmap();
    }

    private bool ValidateComponents()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain is not assigned to DynamicTerrainManager", this);
            return false;
        }

        if (scanCamera == null)
        {
            Debug.LogError("Scan Camera is not assigned to DynamicTerrainManager", this);
            return false;
        }

        if (renderTexture == null)
        {
            Debug.LogError("Render Texture is not assigned to DynamicTerrainManager", this);
            return false;
        }

        return true;
    }

    private void UpdateTerrain()
    {
        // Position terrain at manager's position
        terrain.transform.position = transform.position;

        // Update terrain data size
        TerrainData terrainData = terrain.terrainData;
        if (terrainData == null)
        {
            Debug.LogError("Terrain has no TerrainData assigned", terrain);
            return;
        }

        // Set terrain size (width, height, length)
        terrainData.size = new Vector3(terrainSettings.terrainSize.x, terrainSettings.maxHeight - terrainSettings.minHeight, terrainSettings.terrainSize.y);

        // Set heightmap resolution if it doesn't match
        if (terrainData.heightmapResolution != terrainSettings.heightmapResolution)
        {
            terrainData.heightmapResolution = terrainSettings.heightmapResolution;
        }
    }

    private void UpdateCamera()
    {
        // Set camera to orthographic for parallel projection
        scanCamera.orthographic = true;
        scanCamera.allowHDR = true;

        // Camera orthographic size matches terrain size (half of the larger dimension)
        scanCamera.orthographicSize = Mathf.Max(terrainSettings.terrainSize.x, terrainSettings.terrainSize.y) * 0.5f;

        // Position camera above the terrain center, looking down
        Vector3 terrainCenter = transform.position + new Vector3(terrainSettings.terrainSize.x * 0.5f, 0f, terrainSettings.terrainSize.y * 0.5f);
        scanCamera.transform.position = terrainCenter + Vector3.up * (terrainSettings.maxHeight + 50f); // Extra height buffer
        scanCamera.transform.rotation = Quaternion.LookRotation(Vector3.down);

        // Set camera clipping planes
        scanCamera.nearClipPlane = 10f;
        scanCamera.farClipPlane = (terrainSettings.maxHeight - terrainSettings.minHeight) + 100f;

        // Assign render texture
        scanCamera.targetTexture = renderTexture;
    }

    private void UpdateRenderTexture()
    {
        // Check if render texture resolution matches heightmap resolution
        if (renderTexture.width != terrainSettings.heightmapResolution ||
            renderTexture.height != terrainSettings.heightmapResolution)
        {
            Debug.LogWarning($"RenderTexture resolution ({renderTexture.width}x{renderTexture.height}) " +
                           $"doesn't match heightmap resolution ({terrainSettings.heightmapResolution}). " +
                           "Consider updating the RenderTexture resolution for optimal results.");
        }
    }
}