using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dragable : MonoBehaviour
{
    
    private Vector3 offset;

    private float zcoord;

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zcoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDown()
    {
        zcoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        offset = gameObject.transform.position - GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        gameObject.transform.position = GetMouseWorldPosition() + offset;
    }
 
    
}
