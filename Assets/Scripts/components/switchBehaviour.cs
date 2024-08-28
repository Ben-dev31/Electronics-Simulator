using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchBehaviour : MonoBehaviour
{
    
    [SerializeField] private bool isOn = false;
    public CircuitComponent cc;
    private Circuit circuitComponent;
    // public Animator Anim;

    void Start()
    {
        cc = GetComponent<CircuitComponent>();
        circuitComponent = FindObjectsOfType<Circuit>()[0];
        // Anim = GetComponent<Animator>();

        cc.Type = "Switch";
        cc.Current = 0f;
        cc.Voltage = 0f;
        cc.Resistance = 0f;
    }


    void OnMouseDown()
    {
        if(isOn)
        {
            isOn = false;
            cc.Current = 0f;
            cc.Voltage = 0f;
            circuitComponent.componentValue = 0;
            CircuitComponent[] components = FindObjectsOfType<CircuitComponent>();
            foreach (CircuitComponent component in components)
            {
                if(component.Id != 0)
                {
                    component.Current = 0f;
                    component.Voltage = 0f;
                }
                
            }
            // Anim.SetTrigger("TrunOff");
        }
        else
        {
            isOn = true;
            circuitComponent.componentValue = 1;
            circuitComponent.CalculateCurrentsAndVoltages();
            // Anim.SetTrigger("TrunOn");
        }
    }
}
