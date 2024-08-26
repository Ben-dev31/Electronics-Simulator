using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    public Color selectedColor = Color.yellow;
    public Color originalColor;
    public bool isSelected = false;

    // Start is called before the first frame update
    void Start()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalColor = renderer.material.color;
        }
    }

    void OnMouseDown()
    {
        if(!isSelected)
        {
            gameObject.transform.tag = "Selected";
            gameObject.GetComponent<Renderer>().material.color = selectedColor;
            isSelected = true;
        }
        else if(!Input.GetKey(KeyCode.LeftControl) || !Input.GetKey(KeyCode.RightControl))
        {
            gameObject.transform.tag = "component";
            gameObject.GetComponent<Renderer>().material.color = originalColor;
            isSelected = false;
        }
        
        
    }
}
