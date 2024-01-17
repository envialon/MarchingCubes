using System;
using System.Collections.Generic;
using UnityEngine;

public class ClickEventArgs : EventArgs
{
    public enum ClickType
    {
        LeftClick,
        RightClick,
        other
    }

    public Vector3Int pos;
    public ClickType clickType;
    public ClickEventArgs(Vector3Int pos, ClickType type)
    {
        this.pos = pos;
        this.clickType = type;
    }
}
public class ClickOnScene : MonoBehaviour
{

    static public event EventHandler OnClickOnScene;

    ClickEventArgs.ClickType type;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                type = ClickEventArgs.ClickType.LeftClick;
            }
            else
            {
                type = ClickEventArgs.ClickType.RightClick;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                Vector3Int pos = Vector3Int.RoundToInt(hit.point + new Vector3(0,.4f, 0));
                OnClickOnScene?.Invoke(this, new ClickEventArgs(pos, type));
             //   Debug.Log("Clicked on " + pos);
              
            }
        }
    }
}
