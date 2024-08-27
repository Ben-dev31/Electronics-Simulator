using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentManager : MonoBehaviour
{
    private CircuitComponent Battery;

    public List<CircuitComponent> CCp;


    void Start()
    {
        Battery = gameObject.GetComponent<CircuitComponent>();
        CircuitComponent[] componentsInScene = FindObjectsOfType<CircuitComponent>();
        CCp = new List<CircuitComponent>(componentsInScene);

        foreach (CircuitComponent item in CCp)
        {
            if(item.gameObject.name == "Battery")
            {
                item.currentDirection = 1;
            }
            else{
                item.currentDirection = -1;
            }
        }
    }

    public void currentDirection()
    {
        foreach (CircuitComponent item in CCp)
        {
            if(item.gameObject.name == "Battery")
            {
                item.currentDirection = 1;
            }
            else{
                item.currentDirection = -1;
            }
        } 
    }
}