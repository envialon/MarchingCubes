using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMeshComponent : MonoBehaviour
{
    public enum MarchingMethod
    {
        MarchingCubes,
        MarchingSelectiveCubes,
        MarchingGPU
    }

    #region meshStuff
    protected MeshCollider meshCollider;
    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    protected Mesh mesh;
    public Material material;
    #endregion

    private Marcher marcher;

    #region enums
    public MarchingMethod marchingMethod;
    public Marcher.InterpolationMethod interpolationMethod;
    #endregion

    public int boundSize;
    public float resolution;
    public float threshold;
    public float opacity;


    private void UpdateMarcherAttributes()
    {
        marcher.boundSize = boundSize;
        marcher.resolution = resolution;
        marcher.threshold = threshold;
        marcher.interpolationMethod = interpolationMethod;
    }

    private void InitializeMarcher()
    {
        switch (marchingMethod)
        {
            case MarchingMethod.MarchingCubes:
                marcher = new MarchingCubes(boundSize, resolution,  threshold, interpolationMethod);
                break;
            case MarchingMethod.MarchingSelectiveCubes:
                marcher = new MarchingSelectiveCubes(boundSize, resolution, threshold, interpolationMethod);
                break;
            case MarchingMethod.MarchingGPU:
                marcher = new MarchingCubesGPU(boundSize, resolution, threshold, interpolationMethod);  
                break;
        }
    }

    private void Initialize()
    {
        ClickOnScene.OnClickOnScene += ReactToClick;

        meshCollider = this.gameObject.AddComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.material = material;

        InitializeMarcher();
    }

    private void Start()
    {
        Initialize();
    }
    private void ReactToClick(object sender, EventArgs e)
    {
        mesh.Clear();
        UpdateMarcherAttributes();

        ClickEventArgs eArgs = (ClickEventArgs)e;
        if (eArgs.clickType == ClickEventArgs.ClickType.RightClick)
        {
            marcher.RemoveSelectedVertex(eArgs.pos, opacity);
        }
        else if (eArgs.clickType == ClickEventArgs.ClickType.LeftClick)
        {
            marcher.AddSelectedVertex(eArgs.pos, opacity);
        }
        Marcher.ProceduralMeshInfo meshInfo = marcher.March();
        mesh.vertices = meshInfo.meshVertices;
        mesh.triangles = meshInfo.meshTriangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        if (mesh.vertexCount >= 3)
        {
            meshCollider.sharedMesh = mesh;
        }
    }
}
