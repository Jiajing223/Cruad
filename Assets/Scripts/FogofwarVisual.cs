using System.Collections.Generic;
using UnityEngine;

public class FogofwarVisual : MonoBehaviour
{
    public static FogofwarVisual Instance { get; private set; }
    public bool IsReady { get; private set; }
    [SerializeField] private Material fogMaterial;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private Color hiddenColor = new Color(0f, 0f, 0f, 0.85f);
    [SerializeField] private Color visibleColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private float obstacleCheckRadius = 0.3f;
    [SerializeField] private float meshYOffset = 0.0001f;

    private List<Mesh> floorMeshes = new List<Mesh>();
    private List<MeshRenderer> floorRenderers = new List<MeshRenderer>();
    private List<int[]> vertexStartIndices = new List<int[]>();
    private List<bool[]> obstacleMaps = new List<bool[]>();

    private int width, height, floorCount;
    private float cellSize;

    private void Awake() => Instance = this;

    private void Start()
    {
        width = LevelGrid.Instance.GetWidth();
        height = LevelGrid.Instance.GetHeight();
        cellSize = LevelGrid.Instance.GetCellSize();
        floorCount = LevelGrid.Instance.GetFloorCount();

        BuildMeshes();
        IsReady = true;
    }

    private void Update()
    {   
        // Ensure only one fog floor is rendered based on camera height, so that the fog will not block the view of units on other floors
        float cameraHeight = CameraController.Instance.GetCameraHeight();
        float floorHeightOffset = 1f;

        int activeFloor = -1;
        for (int f = floorCount - 1; f >= 0; f--)
        {
            if (cameraHeight > LevelGrid.FLOOR_HEIGHT * f + floorHeightOffset)
            {
                activeFloor = f;
                break;
            }
        }

        for (int f = 0; f < floorRenderers.Count; f++)
            floorRenderers[f].enabled = f == activeFloor;
    }

    private void BuildMeshes()
    {   
        // Build a separate mesh for each floor, so that we can control the rendering of each floor separately based on camera height
        for (int floor = 0; floor < floorCount; floor++)
        {
            GameObject floorGO = new GameObject($"FogFloor_{floor}");
            floorGO.transform.SetParent(transform, false);

            float floorWorldY = floor * LevelGrid.FLOOR_HEIGHT + meshYOffset;
            floorGO.transform.position = new Vector3(0f, floorWorldY, 0f);

            Material mat = new Material(fogMaterial);
            MeshFilter mf = floorGO.AddComponent<MeshFilter>();
            MeshRenderer mr = floorGO.AddComponent<MeshRenderer>();

            mat.renderQueue = 2000 + floor * 10;
            mr.material = mat;
            floorRenderers.Add(mr);

            bool[] obstacleMap;
            Mesh mesh = BuildSingleFloorMesh(floor, floorWorldY, out obstacleMap);
            obstacleMaps.Add(obstacleMap);
            mf.mesh = mesh;
            floorMeshes.Add(mesh);
        }
    }

    private Mesh BuildSingleFloorMesh(int floor, float floorWorldY, out bool[] obstacleMap)
    {
        // Build a box mesh for each grid cell
        int tileCount = width * height;
        obstacleMap = new bool[tileCount];

        // Each box has 20 vertices (4 for each of the 5 faces) assuming the bottom face is not rendered since it will never be visible
        var vertices = new List<Vector3>(tileCount * 20);
        // Each box has 30 tris (2 for each of the 5 faces)
        var tris = new List<int>(tileCount * 30);
        // Each box has 20 vertices, each vertex has a color to control the visibility of the fog
        var colors = new List<Color>(tileCount * 20);
        int[] vertexStartIndex = new int[tileCount];

        // Compute the vertex positions and triangle indices for each cell
        float half = cellSize * 0.5f;
        float boxTop = LevelGrid.FLOOR_HEIGHT * 0.1f;
        float boxDepth = LevelGrid.FLOOR_HEIGHT * 1f;

        Vector3 checkHalf = new Vector3(obstacleCheckRadius,
                                        LevelGrid.FLOOR_HEIGHT * 0.5f,
                                        obstacleCheckRadius);

        // What this does is creates a quad on top of each cell, and then extrudes it downwards to create a box, while rendering each face
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int cellIndex = x * height + z;
                Vector3 worldCenter = new Vector3(x * cellSize, floorWorldY, z * cellSize);
                bool isObstacle = Physics.CheckBox(worldCenter, checkHalf,
                                                    Quaternion.identity, obstacleLayerMask);

                obstacleMap[cellIndex] = isObstacle;
                vertexStartIndex[cellIndex] = vertices.Count;

                int vi = vertices.Count;

                // Top face corners
                Vector3 tfl = new Vector3((x - 0.5f) * cellSize, boxTop, (z + 0.5f) * cellSize);
                Vector3 tfr = new Vector3((x + 0.5f) * cellSize, boxTop, (z + 0.5f) * cellSize);
                Vector3 tbr = new Vector3((x + 0.5f) * cellSize, boxTop, (z - 0.5f) * cellSize);
                Vector3 tbl = new Vector3((x - 0.5f) * cellSize, boxTop, (z - 0.5f) * cellSize);
                // Bottom face corners (extruded down by full floor height)
                Vector3 bfl = tfl - Vector3.up * boxDepth;
                Vector3 bfr = tfr - Vector3.up * boxDepth;
                Vector3 bbr = tbr - Vector3.up * boxDepth;
                Vector3 bbl = tbl - Vector3.up * boxDepth;

                // Top face
                vertices.Add(tfl); vertices.Add(tfr);
                vertices.Add(tbr); vertices.Add(tbl);
                // Front side  (+Z)
                vertices.Add(tfl); vertices.Add(tfr);
                vertices.Add(bfr); vertices.Add(bfl);
                // Right side  (+X)
                vertices.Add(tfr); vertices.Add(tbr);
                vertices.Add(bbr); vertices.Add(bfr);
                // Back side   (-Z)
                vertices.Add(tbr); vertices.Add(tbl);
                vertices.Add(bbl); vertices.Add(bbr);
                // Left side   (-X)
                vertices.Add(tbl); vertices.Add(tfl);
                vertices.Add(bfl); vertices.Add(bbl);

                // 5 faces × 2 tris each
                for (int face = 0; face < 5; face++)
                {
                    int b = vi + face * 4;
                    tris.Add(b);   tris.Add(b+1); tris.Add(b+2);
                    tris.Add(b);   tris.Add(b+2); tris.Add(b+3);
                }

                for (int v = 0; v < 20; v++)
                    colors.Add(hiddenColor);
            }
        }

        vertexStartIndices.Add(vertexStartIndex);

        Mesh mesh = new Mesh { name = $"FogMesh_Floor{floor}" };
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(tris, 0);
        mesh.SetColors(colors);
        mesh.RecalculateNormals();
        return mesh;
    }

    // Update the fog visual based on the visible and explored positions
    public void UpdateFogVisual(HashSet<GridPosition> visible, HashSet<GridPosition> explored)
    {
        for (int f = 0; f < floorCount; f++)
        {
            Color[] colors = floorMeshes[f].colors;
            int[] vertexStartIndex = vertexStartIndices[f];
            bool[] obstacleMap = obstacleMaps[f];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    int cellIndex = x * height + z;
                    int vi = vertexStartIndex[cellIndex];

                    GridPosition gp = new GridPosition(x, z, f);
                    Color c;

                    // Fog changes by changing color of the grids
                    if (obstacleMap[cellIndex])
                        c = visible.Contains(gp) ? visibleColor : hiddenColor;
                    else if (visible.Contains(gp)) c = visibleColor;
                    else c = hiddenColor;

                    for (int v = 0; v < 20; v++)
                        colors[vi + v] = c;
                }
            }
            floorMeshes[f].colors = colors;
        }
    }
}