using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generator : MonoBehaviour
{
    public string Type = "Battery";
    public float Value = 3.6f;  // tension

    private float internalResistance = 1.5f;


    void Start()
    {
        CircuitComponent cc = gameObject.GetComponent<CircuitComponent>();

        cc.Voltage = Value;
        cc.Current = Value / internalResistance;
    }

}
