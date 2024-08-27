using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulbBehaviour : MonoBehaviour
{
    private float minVoltage = 0.5f;
    private float maxVoltage = 10f;

    [SerializeField] private bool lighting = false;

    private CircuitComponent cc;
    public GameObject lightComponent;
    public MaterialManager MM;

    void Start()
    {
        this.cc = gameObject.GetComponent<CircuitComponent>();
        MM = gameObject.GetComponent<MaterialManager>();

        this.cc.Type = "Bulb";
        this.cc.Current = 0f;
        this.cc.Voltage = 0f;
        this.cc.Resistance = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        if(this.cc.Value != 0 && this.cc.Voltage >= minVoltage && this.cc.Voltage <= maxVoltage ) //
        {
            if(!lighting)
            {
                MM.AddMaterial(lightComponent);
            }
            lighting = true;
        }
        else
        {
            lighting = false;
            MM.RemoveMaterial(lightComponent);
            // this.cc.Current = 0f;
            // this.cc.Voltage = 0f;
        }
    }
}

