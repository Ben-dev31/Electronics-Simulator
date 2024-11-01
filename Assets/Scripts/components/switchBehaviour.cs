using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchBehaviour : MonoBehaviour
{
    
    public bool isOn = false;
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
        Switcher();
    }
    public void Switcher()
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
            Borne[] Bornecomponents = FindObjectsOfType<Borne>();
            foreach (Borne item in Bornecomponents)
            {
                item.ResetRange();
            }
            Wire[] Wr = FindObjectsOfType<Wire>();
            foreach (Wire w in Wr)
            {
                w.currentDirection = 0; 
            }

            RemoveElectrons();

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

    private void RemoveElectrons()
    {
        ElectronBezierMovement[] Electrons = FindObjectsOfType<ElectronBezierMovement>();
        if(Electrons.Length > 0)
        {
            foreach (ElectronBezierMovement electron in Electrons)
            {
                Destroy(electron.gameObject);
            }
        }
    }
}
