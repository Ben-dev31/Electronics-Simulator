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

    Borne bn;
    Borne bp;

    void Start()
    {
        bp = transform.Find("BorneP").GetComponent<Borne>();
        bn = transform.Find("BorneN").GetComponent<Borne>();
        
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
            
            bp.Polarisation = 1;
            bn.Polarisation = -1;
        }
        else
        {
            bp.Polarisation = -1;
            bn.Polarisation = 1;
        }
    }

}
