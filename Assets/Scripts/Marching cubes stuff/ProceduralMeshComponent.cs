using System;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ProceduralMeshComponent : MonoBehaviour
{
    #region meshStuff
    protected MeshCollider meshCollider;
    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    protected Mesh mesh;
    public Material material;
    #endregion

    #region enums
    
    //duplicate enum for the inspector (I don't like this) :c
    public enum MarcherType
    {
        MarchingCubes,
        MarchingCubesGPU
    }

    public MarcherType type;
    public Marcher.InterpolationMethod interpolationMethod;
    #endregion

    [SerializeField]
    public MarcherStrategy marcherStrategy;

    public int boundSize;
    public float resolution;
    public float threshold;
    public float opacity;



    private void InitializeMarcher()
    {

        marcherStrategy = new MarcherStrategy(boundSize, resolution, threshold, (MarcherStrategy.MarcherType)type);
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
        InitializeMarcher();
    }

    private void Start()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        mesh.Clear();
        marcherStrategy.UpdateAttributes(boundSize, resolution, threshold, (MarcherStrategy.MarcherType)type);
        ProceduralMeshInfo meshInfo = marcherStrategy.March();
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
            marcherStrategy.RemoveSelectedVertex(eArgs.pos, opacity);
        }
        else if (eArgs.clickType == ClickEventArgs.ClickType.LeftClick)
        {
            marcherStrategy.AddSelectedVertex(eArgs.pos, opacity);
        }
        GenerateMesh();
    }

    public void RegenerateValues()
    {
        marcherStrategy.InitializeValues();
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