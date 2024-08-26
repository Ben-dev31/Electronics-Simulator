using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Borne: MonoBehaviour
{
    public int connectionCount = 0;
    public GameObject Parent;
    public int nodeId;

    private List<Wire> cables = new List<Wire>();

    private int getindex = 0;

    public Wire cable
    {
        get{
            Wire cc = cables[getindex];
            if(connectionCount > 1 ) getindex += 1;
            return cc;
        }
        set{
            cables.Add(value);
        }
    }

    void Start()
    {
        Parent = transform.parent.gameObject;
    }
}