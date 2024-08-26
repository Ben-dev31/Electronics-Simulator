
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public Material materialToAdd; // Le matériau à ajouter
    public Material defautMaterial;

    void Start()
    {
        Renderer Rd = transform.Find("empoule").transform.Find("Top").GetComponent<Renderer>();
        defautMaterial = Rd.materials[0];
    }

    // Méthode pour ajouter le matériau
    public void AddMaterial(GameObject target)
    {
        Renderer renderer = target.GetComponent<Renderer>();

        if (renderer != null && materialToAdd != null)
        {
            Material[] materials = renderer.materials;
            Material[] newMaterials = new Material[materials.Length + 1];
            newMaterials[0] = materialToAdd;
            // for (int i = 1; i < materials.Length; i++)
            // {
            //     newMaterials[i] = materials[i];
            // }

            renderer.materials = newMaterials;
        }
        else
        {
            Debug.LogError("Renderer ou matériau à ajouter non trouvé");
        }
    }

    // Méthode pour retirer le matériau
    public void RemoveMaterial(GameObject target)
    {
        Renderer renderer = target.GetComponent<Renderer>();

        if (renderer != null && materialToAdd != null)
        {
            Material[] materials = renderer.materials;
            if (materials.Length == 1)
            {
                // Debug.LogWarning("Il ne reste qu'un seul matériau, il ne peut pas être retiré.");
                return;
            }

            Material[] newMaterials = new Material[materials.Length - 1];
            // int index = 1;
            newMaterials[0] = defautMaterial;

            // for (int i = 0; i < materials.Length-1; i++)
            // {
            //     if (materials[i] != materialToAdd)
            //     {
            //         newMaterials[index] = materials[i];
            //         index++;
            //     }
            // }

            renderer.materials = newMaterials;
        }
        else 
        {
            Debug.LogError("Renderer ou matériau à retirer non trouvé");
        }
    }
}