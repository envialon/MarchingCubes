using Unity.Collections;

public abstract class Marcher
{
    public enum InterpolationMethod
    {
        HalfPoint,
        Linear,
        Smoothstep
    }

    public struct MarcherParams
    {
        public int boundSize;
        public int step;
        public float isoLevel;
        public InterpolationMethod interpolationMethod;
    }

    public abstract ProceduralMeshInfo March(in NativeArray<float> values, MarcherParams parameters);
}
