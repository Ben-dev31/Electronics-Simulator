using System.Collections.Generic;
using UnityEngine;

public class WireManager : MonoBehaviour
{
    public List<CircuitComponent> LoopComponents = new List<CircuitComponent>();

    public List<Wire> AllWire = new List<Wire>();

    public void DetectWires()
    {
        AllWire =  new List<Wire>(FindObjectsOfType<Wire>());
    }

    public void DetectLoops()
    {

    }
}