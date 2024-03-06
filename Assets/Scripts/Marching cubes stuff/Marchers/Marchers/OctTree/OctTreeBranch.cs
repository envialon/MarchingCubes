
public interface IOctTreeNode
{

}

public class OctTreeBranch : IOctTreeNode 
    
{    
    public IOctTreeNode[] children;

    public OctTreeBranch()
    {
        children = new IOctTreeNode[8];
    }


    public IOctTreeNode this[int key]
    {
        get => children[key];
        set => children[key] = value;
    }

}

public class OctTreeLeaf : IOctTreeNode
{
    public float[] values;
    public OctTreeLeaf()
    {
        values = new float[8];
    }
    public float this[int key]
    {
        get => values[key];
        set => values[key] = value;
    }

}

