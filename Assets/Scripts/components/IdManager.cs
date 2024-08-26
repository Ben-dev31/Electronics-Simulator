
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdManager : MonoBehaviour
{
    private int id = 1;

    void Start()
    {
        CircuitComponent[] componentsInScene = FindObjectsOfType<CircuitComponent>();

        foreach (CircuitComponent component in componentsInScene)
        {
            if(component.gameObject.name == "Battery")
            {
                component.Id = 0;
            }
            else{
                component.Id = id;
                id++;
            }
        }
    }
}