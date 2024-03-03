public class OctTreeNode 
{
    
    private OctTreeNode[] children;
    private CubicVolume volume;
    public int depth;

    public OctTreeNode(CubicVolume volume, int depth)
    {
        this.volume = volume;
        children = new OctTreeNode[8];
        this.depth = depth;
    }

    public CubicVolume GetVolume()
    {
        return volume;
    }

    public OctTreeNode[] GetChildren()
    {
        return children;
    }
  
}
