using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogofwarVisual : MonoBehaviour
{
    public static FogofwarVisual Instance { get; private set; }
    [SerializeField] private Material fogMaterial;

    // tile colour values
    [SerializeField] private Color hiddenColor   = new Color(0f, 0f, 0f, 1f);   // full black
    [SerializeField] private Color exploredColor = new Color(0f, 0f, 0f, 0.5f); // semi-transparent
    [SerializeField] private Color visibleColor  = new Color(0f, 0f, 0f, 0f);   // fully clear
    public bool IsReady { get; private set; }
    private Mesh fogMesh;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    private int width, height;
    private float cellSize;
    private void Awake()
    {
        Instance = this;

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter   = gameObject.AddComponent<MeshFilter>();
        meshRenderer.material = fogMaterial;
    }
    private void Start()
    {
        width    = LevelGrid.Instance.GetWidth();
        height   = LevelGrid.Instance.GetHeight();
        cellSize = LevelGrid.Instance.GetCellSize();
        BuildMesh();
        IsReady = true;
    } 
    private void BuildMesh()
    {
        fogMesh = new Mesh();
        fogMesh.name = "FogOfWarMesh";
        

        int tileCount = width * height;
        Vector3[] vertices = new Vector3[tileCount * 4];
        int[]     tris     = new int[tileCount * 6];
        Color[]   colors   = new Color[tileCount * 4];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int i    = (x * height + z);
                int vi   = i * 4;
                int ti   = i * 6;

                Vector3 center = new Vector3(x * cellSize, 0.05f, z * cellSize);
                float   half   = cellSize * 0.5f;

                vertices[vi + 0] = center + new Vector3(-half, 0,  half);
                vertices[vi + 1] = center + new Vector3( half, 0,  half);
                vertices[vi + 2] = center + new Vector3( half, 0, -half);
                vertices[vi + 3] = center + new Vector3(-half, 0, -half);

                tris[ti + 0] = vi; tris[ti + 1] = vi + 1; tris[ti + 2] = vi + 2;
                tris[ti + 3] = vi; tris[ti + 4] = vi + 2; tris[ti + 5] = vi + 3;

                // Start everything hidden
                colors[vi] = colors[vi+1] = colors[vi+2] = colors[vi+3] = hiddenColor;
            }
        }

        fogMesh.vertices  = vertices;
        fogMesh.triangles = tris;
        fogMesh.colors    = colors;
        fogMesh.RecalculateNormals();

        meshFilter.mesh = fogMesh;
    }

    public void UpdateFogVisual(HashSet<GridPosition> visible, HashSet<GridPosition> explored)
    {
        Color[] colors = fogMesh.colors;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gp = new GridPosition(x, z, 0);
                int i  = (x * height + z) * 4;

                Color c;
                if      (visible.Contains(gp))  c = visibleColor;
                else if (explored.Contains(gp)) c = exploredColor;
                else                            c = hiddenColor;

                colors[i] = colors[i+1] = colors[i+2] = colors[i+3] = c;
            }
        }

        fogMesh.colors = colors;
    }
}
