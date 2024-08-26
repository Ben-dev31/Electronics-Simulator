
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitComponent : MonoBehaviour
{
    public int Id;
    
    public string Type;         // ex. "Resistance", "Battery"
    public float Value = 1f;     // ex. valeur de la résistance
    public float Current = 0f;
    public float Voltage = 0f;

    public  Node Node1;
    public  Node Node2;

    public int nodeCounter = 0;

    public List<GameObject> cables;

    public Borne bp;
    public Borne bn;

    void Start()
    {
        Node1 = new Node(Id*2);
        Node2 = new Node(Id*2 + 1);

        bp = transform.Find("BorneP").GetComponent<Borne>();
        bp.nodeId = Node1.Id;
        bn = transform.Find("BorneN").GetComponent<Borne>();
        bn.nodeId = Node2.Id;
    }

    public Node GetOtherNode(Node node)
    {
        return node == Node1 ? Node2 : Node1;
    }

    public Borne GetOtherBorne(Borne borne)
    {
        return borne == bp ? bn : bp;
    }

    public bool TrueConnection()
    {
        if(bn.connectionCount < 1 || bp.connectionCount < 1) return false;
        return true;
    } 

}