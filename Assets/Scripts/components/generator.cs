using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generator : MonoBehaviour
{
    public string Type = "Battery";
    public float Value = 3.6f;  // tension

    private float internalResistance = 1.5f;

    public int oriantation = 1;

    CircuitComponent cc;

    void Start()
    {
        cc = gameObject.GetComponent<CircuitComponent>();

        cc.Voltage = Value;
        cc.Current = Value / internalResistance;

        UpdateOrientation();
    }

    public void UpdateOrientation(int ort = 1)
    {
        oriantation = ort;
        if(oriantation == 1)
        {
            cc.pos = cc.bp;
            cc.neg = cc.bn;
        }
        else
        {
            cc.pos = cc.bn;
            cc.neg = cc.bp;
        }
    }

}
