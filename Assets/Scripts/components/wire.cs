using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    CableCreator creator;
    
    public GameObject Componant1;
    public GameObject Componant2;

    private GameObject[] cbornes;

    private float ElectronRadius = 0.07f;

    public int currentDirection = 1 ;

    public void Initialize(GameObject[] bornes, List<GameObject> objs)
    {
        Componant1 = objs[0];
        Componant2 = objs[1];

        cbornes = bornes;

        creator = GetComponent<CableCreator>();

        Rigidbody rd = gameObject.AddComponent<Rigidbody>();
        
        rd.isKinematic = true;
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

    private void CreateElectron()
    {
        List<Transform> controllPoints = creator.GetControllPoints();

        GameObject ElectronObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ElectronObject.name = "electron";
        ElectronObject.transform.SetParent(transform);
        ElectronObject.transform.position = controllPoints[0].position;
        ElectronObject.transform.localScale = Vector3.one * ElectronRadius;

        Renderer Rd = ElectronObject.GetComponent<Renderer>();
        Rd.material.color = Color.red;

        ElectronBezierMovement ElBMovement = ElectronObject.AddComponent<ElectronBezierMovement>();
        ElBMovement.controlPoints = controllPoints;
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.LeftControl))
        {
            CreateElectron();
        }
    }

    public void ChangeOrientation(GameObject PComp)
    {
        if(PComp == cbornes[0])
        {
            currentDirection = 1;
        }
        else
        {
            currentDirection = -1;
        }
    }

    
}


public class ElectronBezierMovement : MonoBehaviour
{
    public List<Transform> controlPoints; // Points de contrôle de la courbe de Bézier
    public float speed = 0.5f; // Vitesse de déplacement des électrons
    private float t = 0.0f; // Paramètre pour parcourir la courbe (de 0 à 1)

    void Update()
    {
        // Avance l'électron le long de la courbe de Bézier
        t += speed * Time.deltaTime;

        // Si l'électron atteint la fin de la courbe, recommence au début
        if (t > 1.0f)
        {
            t = 0.0f;
        }

        // Calcule la nouvelle position sur la courbe de Bézier
        Vector3 newPosition = GetBezierPoint(t, controlPoints);

        Vector3 tangent = GetBezierTangent(t, controlPoints);

        // Déplace l'électron à la nouvelle position
        transform.position = newPosition;

        // Oriente l'électron le long de la tangente
        // transform.rotation = Quaternion.LookRotation(tangent);
    }

    // Méthode pour calculer un point sur une courbe de Bézier quadratique
    Vector3 GetBezierPoint(float t, List<Transform> points)
    {
        if (points.Count == 3)
        {
            // Bézier quadratique (3 points de contrôle)
            return Mathf.Pow(1 - t, 2) * points[0].position +
                    2 * (1 - t) * t * points[1].position +
                    Mathf.Pow(t, 2) * points[2].position; 
        
        }
        else if (points.Count == 4)
        {
            // Bézier cubique (4 points de contrôle)
            return Mathf.Pow(1 - t, 3) * points[0].position +
                   3 * Mathf.Pow(1 - t, 2) * t * points[1].position +
                   3 * (1 - t) * Mathf.Pow(t, 2) * points[2].position +
                   Mathf.Pow(t, 3) * points[3].position;
        }
        else
        {
            // Par défaut, retourne le premier point si le nombre de points de contrôle est incorrect
            return points[0].position;
        }
    }
    Vector3 GetBezierTangent(float t, List<Transform>  points)
    {
        if (points.Count == 3)
        {
            // Tangente pour Bézier quadratique
            return 2 * (1 - t) * (points[1].position - points[0].position) +
                   2 * t * (points[2].position - points[1].position);
        }
        else if (points.Count == 4)
        {
            // Tangente pour Bézier cubique
            return 3 * Mathf.Pow(1 - t, 2) * (points[1].position - points[0].position) +
                   6 * (1 - t) * t * (points[2].position - points[1].position) +
                   3 * Mathf.Pow(t, 2) * (points[3].position - points[2].position);
        }
        else
        {
            // Par défaut, retourne un vecteur zéro si le nombre de points de contrôle est incorrect
            return Vector3.zero;
        }
    }

    Vector3 GetBezierPoint3(float t, List<Transform> points)
    {
        Vector3 p0 = new Vector3();
        Vector3 p1 = new Vector3();
        Vector3 p2 = new Vector3();
        Vector3 p3 = new Vector3();

        if (points.Count == 3)
        {
            p0 = points[0].position;
            p1 = points[1].position;
            p2 = points[2].position;
            p3 = points[2].position;
            // Bézier quadratique (3 points de contrôle)
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f*((2f * p1) +
                        (-p0 + p2) * t +
                        (2f * p0 - 5f * p1 + 4f * p2 ) * t2 +
                        (-p0 + 3f * p1 - 3f * p2 ) * t3 );
        }
        
        else
        {
            // Par défaut, retourne le premier point si le nombre de points de contrôle est incorrect
            return points[0].position;
        }
    }
    Vector3 GetBezierPoint2(float t, List<Transform> points)
    {
        Vector3 point;
        if (points.Count == 3)
        {
            point = Mathf.Pow(1 - t, 2) * points[0].position +
                    2 * (1 - t) * t * points[1].position +
                    Mathf.Pow(t, 2) * points[2].position;
        }
        else if (points.Count == 4)
        {
            point = Mathf.Pow(1 - t, 3) * points[0].position +
                    3 * Mathf.Pow(1 - t, 2) * t * points[1].position +
                    3 * (1 - t) * Mathf.Pow(t, 2) * points[2].position +
                    Mathf.Pow(t, 3) * points[3].position;
        }
        else
        {
            point = points[0].position;
        }

        // Ajoute une légère oscillation aléatoire autour de la courbe
        float oscillationMagnitude = 0.05f; // Ajuste la magnitude de l'oscillation
        Vector3 oscillation = new Vector3(
            Random.Range(-oscillationMagnitude, oscillationMagnitude),
            Random.Range(-oscillationMagnitude, oscillationMagnitude),
            Random.Range(-oscillationMagnitude, oscillationMagnitude)
        );

        return point + oscillation;
    }

}

