using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIcontroller : MonoBehaviour
{
    public VisualElement bntModif;
    public VisualElement bntCon;
    public VisualElement bntDel;
    public VisualElement elec;

    private int ActiveState = 1;

    private bool electronV = false;

    void Start()
    {
        bntModif = GetComponent<UIDocument>().rootVisualElement.Q<Button>("modif");
        bntModif.RegisterCallback<ClickEvent>(OnClickModif);

        bntCon = GetComponent<UIDocument>().rootVisualElement.Q<Button>("connect");
        bntCon.RegisterCallback<ClickEvent>(OnClickCon);

        bntDel = GetComponent<UIDocument>().rootVisualElement.Q<Button>("del");
        bntDel.RegisterCallback<ClickEvent>(OnClickDel);

        elec = GetComponent<UIDocument>().rootVisualElement.Q<Button>("elec");
        elec.RegisterCallback<ClickEvent>(OnClickElec);
        
    }

    // Update is called once per frame
    private void OnClickCon(ClickEvent evt)
    {
        GetComponent<CableCreator>().Connect();
        GetComponent<Circuit>().InitializeCircuit();
        ActiveState = 0;
        // GetComponent<Circuit>().CalculateCurrentsAndVoltages();
    }

    private void OnClickModif(ClickEvent evt)
    {
        GameObject[] selectObjects = GameObject.FindGameObjectsWithTag("Selected");
        GameObject component = selectObjects[0];
        CableCreator cc = component.GetComponent<CableCreator>();
        if(cc == null) return;

        if(ActiveState == 1) // controlle activer
        {
            cc.DeActiveControlle();
            ActiveState = 0;
           

        }
        else
        {
            cc.ActiveControlle();
            ActiveState = 1;
           
        }
    }

    private void OnClickDel(ClickEvent evt)
    {
        GameObject[] selectObjects = GameObject.FindGameObjectsWithTag("Selected");
        foreach (GameObject item in selectObjects)
        {
            Wire w = item.GetComponent<Wire>();
            if(w != null)
            {
                w.Desconnect();
                Destroy(item);
            }
            // GetComponent<Circuit>().CalculateCurrentsAndVoltages();
            
        }
        
    }

    private void OnClickElec(ClickEvent evt)
    {
        
        if(!electronV)
        {
            ViewElectrons();
            electronV = true;
        }
        else
        {
            RemoveElectrons();
            electronV = false; 
        }
    }
    private void ViewElectrons()
    {
        Wire[] Cables = FindObjectsOfType<Wire>();

        if(Cables.Length > 0)
        {
            foreach (Wire cable in Cables)
            {
                cable.CreateElectron();
            }
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
