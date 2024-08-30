using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Borne: MonoBehaviour
{
    public int connectionCount = 0;
    public GameObject Parent;
    public int nodeId;
    public int Polarisation = 0; // {get; set;}

    private List<Wire> _cables = new List<Wire>();

    private int getindex = 0;

    public Wire cable
    {
        get{
            if(_cables.Count -1 < getindex ) getindex = 0; 
            Wire cc = _cables[getindex];
            if(_cables.Count -1 >= getindex ) getindex += 1;
            return cc;
        }
        set{
            if(!_cables.Contains(value)) _cables.Add(value);
        }
    }


    void Start()
    {
        Parent = transform.parent.gameObject;
    }

    public void ResetRange()
    {
        getindex = 0;
    }
}