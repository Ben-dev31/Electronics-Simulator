using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    CableCreator creator;
    
    public GameObject Componant1;
    public GameObject Componant2;
    // les composants auxquels le cable est rataché

    private GameObject[] cbornes;
    // les bornes auxquels le cable est rataché (taille 2)

    private float ElectronRadius = 0.02f;
    // rayon de la sphère representant un électron

    public int currentDirection = 0 ;
    // direction du courant dans le cable
    // indique le sens de circulation des électrons 

    private int ElectronNumber = 10;
    // le nombre d'électrons à générer 

    public int ElectronLength = 0;

    public bool ElectronCreating = false;

    private float craetTime = 0f;

    public float speed = 0.8f; 

    public int WireId;   
    // indique si le cable appartient à une maille principale ou secondaire 
    // 0        pour maille principal
    // 1,2,3... pour maille secondaire
    // null si le composant n'est pas connecté

    public int loopId = -1;
    // Id de la maille auquelle le cable appartient 

    public Material transparentMat;
    public Material defautMaterial;
    
    public void Initialize(GameObject[] bornes, List<GameObject> objs)
    {
        Componant1 = objs[0];
        Componant2 = objs[1];

        cbornes = bornes;

        creator = GetComponent<CableCreator>();

        Rigidbody rd = gameObject.AddComponent<Rigidbody>();
        
        rd.isKinematic = true;

        transparentMat = Resources.Load<Material>("Materials/cableMaterial");
        defautMaterial = gameObject.GetComponent<Renderer>().materials[0];
    }

    private void CheckPolarisation()
    {
        Borne B1 = cbornes[0].GetComponent<Borne>();
        Borne B2 = cbornes[1].GetComponent<Borne>();

        if(B1.Polarisation == 1 && B2.Polarisation == -1)
        {
            currentDirection = 1;
        }
        else if(B1.Polarisation == -1 && B2.Polarisation == 1)
        {
            currentDirection = 1;
        }
        else if(B1.Polarisation == 1 && B2.Polarisation == 1)
        {
            if(B2.Parent.GetComponent<CircuitComponent>().Type == "Battery")
            {
                print("if 3.1");
                currentDirection = -1;
            }
            else if(B1.Parent.GetComponent<CircuitComponent>().Type == "Battery")
            {
                print("if 3.2");
                currentDirection = 1;
            }
            else{
                print("else");
                currentDirection = 1;
            }
            
        }
        else if(B1.Polarisation == -1 && B2.Polarisation == -1)
        {
            if(B2.Parent.GetComponent<CircuitComponent>().Type == "Battery")
            {
                currentDirection = 1;
            }
            else if(B1.Parent.GetComponent<CircuitComponent>().Type == "Battery")
            {
                currentDirection = -1;
            }
            else{
                currentDirection = 1;
            }
            
        }
        else{
            currentDirection = 1;
        }


    }

    public Borne GetOtherBorne(Borne borne)
    {
        return borne == cbornes[0].GetComponent<Borne>() ? cbornes[1].GetComponent<Borne>() : cbornes[0].GetComponent<Borne>();
    }

    public CircuitComponent GetOtherComponent(CircuitComponent cp)
    {
        return cp != Componant1.GetComponent<CircuitComponent>() ? Componant1.GetComponent<CircuitComponent>() : Componant2.GetComponent<CircuitComponent>();
    }

    public void Desconnect()
    {
        foreach (GameObject item in cbornes)
        {
            Borne bn = item.GetComponent<Borne>();
            bn.RemoveCable(this);

            bn.connectionCount -= 1;
        }
        Componant1.GetComponent<CircuitComponent>().cables.Remove(gameObject);
        Componant2.GetComponent<CircuitComponent>().cables.Remove(gameObject);
    }


    public void CreateElectron()
    {
        if(currentDirection == 0 ) return;
        List<Transform> controllPoints = creator.GetControllPoints();
        // CheckPolarisation();
       
        if(!(cbornes[0].GetComponent<Borne>().Polarisation == 1))
        {
            controllPoints.Reverse();
        }

        GameObject ElectronObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ElectronObject.name = "electron";
        ElectronObject.transform.SetParent(transform);
        ElectronObject.transform.position = controllPoints[0].position;
        ElectronObject.transform.localScale = Vector3.one * ElectronRadius;

        Renderer Rd = ElectronObject.GetComponent<Renderer>();
        Rd.material.color = Color.red;

        ElectronBezierMovement ElBMovement = ElectronObject.AddComponent<ElectronBezierMovement>();
        ElBMovement.controlPoints = controllPoints;

        ElectronLength++;
        
    }

    void Update()
    {
        if(ElectronCreating && ElectronNumber > ElectronLength)
        {
            if(craetTime >= 0.3f)
            {
                CreateElectron();
                craetTime = 0f;
            }
            craetTime += speed * Time.deltaTime;
            print($"{craetTime}");
        }
    }

    public void ChangeOrientation(Borne PComp)
    {
        if(PComp.gameObject == cbornes[0])
        {
            currentDirection = 1;
        }
        else
        {
            currentDirection = -1;
        }
    }

    public void ToggleView(int t)
    {
        Material[] mats = new Material[1];

        if(t == 1)
            mats[0] = transparentMat;
        else
            mats[0] = defautMaterial;

        gameObject.GetComponent<Renderer>().materials = mats;
    }

    
}


public class ElectronBezierMovement : MonoBehaviour
{
    public List<Transform> controlPoints; // Points de contrôle de la courbe de Bézier
    public float speed = 0.8f; // Vitesse de déplacement des électrons
    private float t = 0.0f; // Paramètre pour parcourir la courbe (de 0 à 1)
    private int sp = 0;

    void Update()
    {
        // Avance l'électron le long de la courbe de Bézier
        t += speed * Time.deltaTime;

        // Si l'électron atteint la fin de la courbe, recommence au début
        if (t > 1.0f)
        {
            t = 0.0f;
            sp = sp == 0 ? 1 : 0;
        }

        // Calcule la nouvelle position sur la courbe de Bézier
        Vector3 newPosition = GetBezierPoint3(t, controlPoints)[sp];

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

    List<Vector3> GetBezierPoint3(float t, List<Transform> points)
    {
        List<Vector3> interpolatedPoint = new List<Vector3>();
        // int interpolationCount = 10;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = i > 0 ? points[i - 1].position : points[i].position;
            Vector3 p1 = points[i].position;
            Vector3 p2 = points[i + 1].position;
            Vector3 p3 = i < points.Count - 2 ? points[i + 2].position : points[i + 1].position;

            // for (int j = 0; j < interpolationCount; j++)
            // {
            //     float t = (float)j / interpolationCount;
            Vector3 interpolated = CatmullRom(p0, p1, p2, p3, t);
            interpolatedPoint.Add(interpolated);
            // }
        }
        return interpolatedPoint;
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * ((2f * p1) +
                       (-p0 + p2) * t +
                       (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                       (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
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

