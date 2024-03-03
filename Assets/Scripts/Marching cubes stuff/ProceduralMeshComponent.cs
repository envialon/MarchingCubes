using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
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

        if (resolution <= 0)
        {
            throw new Exception("Resolution can't be <= 0");
        }
    }

    private void InitializeMarcher()
    {
        if (marcher is null)
        {
            marcher = new MarchingCubes(boundSize, resolution, threshold, interpolationMethod);
        }
        else
        {
            switch (marchingMethod)
            {
                case MarchingMethod.MarchingCubes:
                    marcher = new MarchingCubes(marcher);
                    break;
                //case MarchingMethod.MarchingSelectiveCubes:
                //    marcher = new MarchingSelectiveCubes(marcher);
                //    break;
                case MarchingMethod.MarchingGPU:
                    marcher = new MarchingCubesGPU(marcher);
                    break;
            }
        }
    }

    private void Initialize()
    {
        ClickOnScene.OnClickOnScene += ReactToClick;

        meshCollider = GetComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();

        InitializeMarcher();
    }

    private void OnEnable()
    {
        Initialize();
    }
    private void OnDisable()
    {
        mesh.Clear();
    }

    private void OnValidate()
    {
        Initialize();
    }

    private void Start()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        mesh.Clear();
        UpdateMarcherAttributes();
        Marcher.ProceduralMeshInfo meshInfo = marcher.March();
        mesh.vertices = meshInfo.meshVertices;
        mesh.triangles = meshInfo.meshTriangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        meshRenderer.material = material;
        if (mesh.vertexCount >= 3)
        {
            meshCollider.sharedMesh = mesh;
        }
    }

    private void ReactToClick(object sender, EventArgs e)
    {

        ClickEventArgs eArgs = (ClickEventArgs)e;
        if (eArgs.clickType == ClickEventArgs.ClickType.RightClick)
        {
            marcher.RemoveSelectedVertex(eArgs.pos, opacity);
        }
        else if (eArgs.clickType == ClickEventArgs.ClickType.LeftClick)
        {
            marcher.AddSelectedVertex(eArgs.pos, opacity);
        }
        GenerateMesh();
    }

    public void RegenerateValues()
    {
        marcher.InitializeValues();
        GenerateMesh();
    }
}

[CustomEditor(typeof(ProceduralMeshComponent))]
class ProcMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Regenerate values"))
        {
            ProceduralMeshComponent t = (ProceduralMeshComponent)target;
            t.RegenerateValues();
        }
    }
}