using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    CableCreator creator;
    
    public GameObject Componant1;
    public GameObject Componant2;

    private GameObject[] cbornes;

    public void Initialize(GameObject[] bornes, List<GameObject> objs)
    {
        Componant1 = objs[0];
        Componant2 = objs[1];

        cbornes = bornes;

        creator = GetComponent<CableCreator>();

        Rigidbody rd = gameObject.AddComponent<Rigidbody>();
        
        rd.isKinematic = true;

        // CapsuleCollider cp = gameObject.GetComponent<CapsuleCollider>();
        // if(cp == null)
        // {
        //     gameObject.AddComponent<CapsuleCollider>();
        // }
        // cp.center = Vector3.zero;
        // cp.direction = 2;
        // cp.radius = 0.015f;
    
        // cp.transform.rotation = transform.rotation;

    }

    public Borne GetOtherBorne(Borne borne)
    {
        return borne == cbornes[0].GetComponent<Borne>() ? cbornes[1].GetComponent<Borne>() : cbornes[0].GetComponent<Borne>();
    }

    public void Desconnect()
    {
        foreach (GameObject item in cbornes)
        {
            Borne bn = item.GetComponent<Borne>();
            bn.connectionCount -= 1;
        }
        Componant1.GetComponent<CircuitComponent>().cables.Remove(gameObject);
        Componant2.GetComponent<CircuitComponent>().cables.Remove(gameObject);
    }

    
}
