using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resistor : MonoBehaviour
{
    public string Type = "Resistor";

    private float resistance = 10f;

    public float Resistance
    {
        private set{
            resistance = value;
        }
        get{
            return resistance;
        }
    }

    private CircuitComponent cc;

    // Start is called before the first frame update

    void Start()
    {
        cc = gameObject.GetComponent<CircuitComponent>();
        cc.Type = Type;

        cc.Resistance = Resistance;
    }

  
}
