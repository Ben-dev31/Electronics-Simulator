using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Borne: MonoBehaviour
{
    public int connectionCount = 0;
    public GameObject Parent;
    public int nodeId;

    // private int _polarisation = 0;
    public int Polarisation = 0;

    public List<Wire> _cables = new List<Wire>();

    private int getindex = 0;

    private List<int> visitedCableindex = new List<int>();
    public List<Wire> visitedCable = new List<Wire>();

    public Wire cable
    {
        get{
            if(_cables.Count -1 < getindex ) getindex = 0;  // à modiffier pour plus de précision sur le nombre de loop
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

    public List<Wire> GetAllCable()
    {
        return _cables;
    }

    public void RemoveCable(Wire cab)
    {
        if(_cables.Contains(cab))
        {
            _cables.RemoveAt(_cables.IndexOf(cab));
        }
    }
    
    public Wire GetOtherCable(Wire cb = null)
    {
        Wire w = null;

        if(cb == null && visitedCableindex.Count == 0)
        {
            visitedCableindex.Add(getindex);
            return _cables[getindex]; 
        }
        else if(cb == null && !(visitedCableindex.Count == 0))
        {
            getindex++;

            if(getindex > _cables.Count -1)
                return w;

            visitedCableindex.Add(getindex);
            return _cables[getindex]; 
        }

        if(_cables.Count -1 > 0)
        {
            int cabIndesx = _cables.IndexOf(cb);
            visitedCableindex.Add(cabIndesx);
            for(int i = 0; i<_cables.Count; i++)
            {
                if(i != cabIndesx && !visitedCableindex.Contains(i))
                {
                    visitedCableindex.Add(i);
                    return _cables[i];
                }
            }
        }
        return w;
    }

    public void ResetRange()
    {
        getindex = 0;
        visitedCableindex.Clear();
    }

    public int GetPolarisation(int loopid)
    {
        int pl = 0;

        foreach (Wire cb in _cables)
        {
            if(cb.loopId == -1) continue;
            if(cb.loopId == loopid) pl = cb.currentDirection;
        }
        return pl;
    }
}