using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableCreator : MonoBehaviour
{
    public List<Vector3> points;        // The points through which the cylinder should pass
    public float radius = 0.02f;        // Radius of the cylinder
    public int segmentCount = 16;       // Number of segments around the cylinder
    public int interpolationCount = 10; // Number of interpolated points between each original point

    private MeshFilter meshFilter;
    public Mesh mesh;

    public bool isCreatable = true;
    public bool makeModiffication = false;
 
    public GameObject cableObject;
    public GameObject cablePrefab;

    private GameObject[] pointObjects;   // Game objects for each point
    private List<List<Vector3>> lastPoints = new List<List<Vector3>>();

    public GameObject[] ObjectToConnect;
    public MeshCreator meshCreator;
    private int cableCounter = 1;


    void Update()
    {
        if(Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.Return) && isCreatable)
        {
            print($"create {gameObject.name}");
            Connect();
        }

        // if(makeModiffication)
        // {
        //     meshFilter = gameObject.GetComponent<MeshFilter>();
        //     mesh = meshFilter.mesh;

        //     // meshCreator = new MeshCreator(mesh, points, segmentCount);
        //     meshCreator.CreateSmoothCylinderMesh();
        // }
    }

    public void Connect()
    {
        GetPoint();
        if (points == null || points.Count < 2 || lastPoints.Contains(points))
        {
            Debug.LogError("At least two points are required to create a cylinder.");
            return;
        }

        // creation du cable
        cableObject = new GameObject($"cable {cableCounter}");
        // cableObject.name = $"cable {cableCounter}";
        // if(cablePrefab == null)
        // {
        //     cableObject = new GameObject($"cable {cableCounter}");
        // }
        // else
        // {
        //     cableObject = Instantiate(cablePrefab);
        //     cableObject.transform.position = (points[points.Count-1] + points[0])/2f;
        //     cableObject.transform.localScale = new Vector3(
        //         cableObject.transform.localScale.x,
        //         Vector3.Distance(points[points.Count-1],points[0]),
        //         cableObject.transform.localScale.z
        //     );
        // }
        // cableObject.transform.position = points[(int)points.Count/2];

        cableCounter++;

        meshFilter = cableObject.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = cableObject.GetComponent<MeshRenderer>();

        if(meshFilter == null)
        {
            meshFilter = cableObject.AddComponent<MeshFilter>();
            meshRenderer = cableObject.AddComponent<MeshRenderer>();
        }

        

        meshRenderer.material = new Material(Shader.Find("Standard"));

        mesh = new Mesh();
        meshFilter.mesh = mesh;

        meshCreator = new MeshCreator(mesh, points, radius);
        meshCreator.CreateSmoothCylinderMesh();

        CableCreator cc = cableObject.AddComponent<CableCreator>();

        cc.meshCreator = meshCreator;

        cc.points = new List<Vector3>();

        for(int j=0; j<this.points.Count; j++)
        {
            cc.points.Add(this.points[j]);
        }
        
        cc.cableObject = this.cableObject;
        cc.mesh = mesh;
        

        lastPoints.Add(points);
        CreatePointObjects();

       

        cableObject.AddComponent<Selectable>();

        List<GameObject> ls = new List<GameObject>();
        ls.Add(ObjectToConnect[0].transform.parent.gameObject);
        ls.Add(ObjectToConnect[1].transform.parent.gameObject);

        Wire wr = cableObject.AddComponent<Wire>();
        wr.Initialize(ObjectToConnect, ls);
        
        foreach (GameObject item in ObjectToConnect)
        {
            Selectable Sc = item.GetComponent<Selectable>();
            item.transform.tag = "component";
            item.GetComponent<Renderer>().material.color = Sc.originalColor;
            Sc.isSelected = false;

            Borne bn = item.GetComponent<Borne>();
            bn.connectionCount += 1;
            bn.cable = wr;

            CircuitComponent c = bn.Parent.GetComponent<CircuitComponent>();
            c.nodeCounter += 1;
            c.cables.Add(cableObject);
        }

        cableObject.AddComponent<MeshCollider>();

        cc.pointObjects = pointObjects;
      
    }

    

    void GetPoint()
    {
        this.points.Clear();
        GameObject[] selectObjects = GameObject.FindGameObjectsWithTag("Selected");
        if(selectObjects.Length == 2)
        {
            for(int j = 0; j < selectObjects.Length; j++)
            {
                points.Add(selectObjects[j].transform.position);
            }
            ObjectToConnect = selectObjects;
        }
        else
        {
            Debug.LogError($"You must select 2 Objects to connect not {selectObjects.Length}");
            // PointSelector2 point = gameObject.GetComponent<PointSelector2>();

            // if(point.positions.Count > 1)
            // {
            //     points = point.positions;
            // }
            // else{
            //     points.Clear();
            //     return;
            // }
            
        }
        List<Vector3> temp = new List<Vector3>();

        for(int i = 0; i<points.Count -1; i++)
        {
            temp.Add(points[i]);
            Vector3 position = (points[i+1] + points[i])/2;
            temp.Add(position);
            // temp.Add(2*position);
            
        }
        temp.Add(points[points.Count -1]);

        points.Clear();
        points = temp;
    }

    
    private void CreatePointObjects()
    {
        pointObjects = new GameObject[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObject.transform.SetParent(cableObject.transform);
            pointObject.transform.position = points[i];
            pointObject.transform.localScale = Vector3.one * radius + Vector3.one * 0.08f;
            pointObject.GetComponent<Renderer>().enabled = false; // Hide the sphere

            pointObject.AddComponent<PointMover>().Initialize(i); // Add PointMover component

            SphereCollider sp = pointObject.GetComponent<SphereCollider>();
            // sp.radius = 0.0f;
            pointObject.GetComponent<Renderer>().material.color = Color.yellow;
            pointObjects[i] = pointObject;

        }
    }

    public void UpdatePointPosition(int index, Vector3 newPosition)
    {

        // mise à jour du mesh
        if (index >= 0 && index < this.points.Count)
        {
            points[index] = newPosition;
            meshCreator.points = points;
            meshCreator.CreateSmoothCylinderMesh();
        }
    }

    public void ActiveControlle()
    {
        foreach (var item in pointObjects)
        {
            SphereCollider sp = item.GetComponent<SphereCollider>();
            sp.radius = 1f;
            item.GetComponent<Renderer>().enabled = true;
        }
    }

    public void DeActiveControlle()
    {
        foreach (var item in pointObjects)
        {
            SphereCollider sp = item.GetComponent<SphereCollider>();
            sp.radius = 0.1f;
            item.GetComponent<Renderer>().enabled = false;
        }
    }
}

public class MeshCreator
{
    public Mesh mesh;
    public List<Vector3> points;
    public int segmentCount = 16; // Number of segments around the cylinder
    public int interpolationCount = 10; // Number of interpolated points between each original point
    public float radius;

    public MeshCreator(Mesh mesh, List<Vector3> points, float radius)
    {
        this.points = points;
        this.mesh = mesh;
        this.radius = radius;
    }

     
    public void CreateSmoothCylinderMesh()
    {
        List<Vector3> smoothPoints = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = i > 0 ? points[i - 1] : points[i];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = i < points.Count - 2 ? points[i + 2] : points[i + 1];

            for (int j = 0; j < interpolationCount; j++)
            {
                float t = (float)j / interpolationCount;
                Vector3 interpolatedPoint = CatmullRom(p0, p1, p2, p3, t);
                smoothPoints.Add(interpolatedPoint); 
            }
        }

        // Adding the last point
        smoothPoints.Add(points[points.Count - 1]);

        GenerateCylinderMesh(smoothPoints, mesh);
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

    void GenerateCylinderMesh(List<Vector3> points, Mesh mesh)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Add central vertices for the cylinder caps
        Vector3 baseCenter = points[0];
        Vector3 topCenter = points[points.Count - 1];

        vertices.Add(baseCenter); // base center vertex
        vertices.Add(topCenter); // top center vertex

        int baseCenterIndex = 0;
        int topCenterIndex = 1;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 center = points[i];
            Vector3 forward = (i < points.Count - 1) ? (points[i + 1] - points[i]).normalized : (points[i] - points[i - 1]).normalized;
            Vector3 up = Vector3.up;

            // Ensure 'up' is not parallel to 'forward'
            if (Vector3.Dot(forward, up) > 0.99f) up = Vector3.right;

            Vector3 right = Vector3.Cross(forward, up).normalized;
            up = Vector3.Cross(right, forward).normalized;

            for (int j = 0; j < segmentCount; j++)
            {
                float angle = (float)j / segmentCount * 2 * Mathf.PI;
                Vector3 offset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up) * radius;
                vertices.Add(center + offset);
            }
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = 0; j < segmentCount; j++)
            {
                int current = i * segmentCount + j + 2; // Offset by 2 for central vertices
                int next = current + segmentCount;

                int nextJ = (j + 1) % segmentCount;

                // Create two triangles for each segment
                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(next + nextJ - j);

                triangles.Add(current);
                triangles.Add(next + nextJ - j);
                triangles.Add(current + nextJ - j);
            }
        }

        // Create base cap
        for (int j = 0; j < segmentCount; j++)
        {
            int current = j + 2;
            int nextJ = (j + 1) % segmentCount + 2;

            triangles.Add(baseCenterIndex);
            triangles.Add(nextJ);
            triangles.Add(current);
        }

        // Create top cap
        int topOffset = (points.Count - 1) * segmentCount + 2;
        for (int j = 0; j < segmentCount; j++)
        {
            int current = topOffset + j;
            int nextJ = topOffset + (j + 1) % segmentCount;

            triangles.Add(topCenterIndex);
            triangles.Add(current);
            triangles.Add(nextJ);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
 
public class PointMover: MonoBehaviour
{
    public CableCreator cableCreator;
    private int index;

    public void Initialize(int index)
    {
        this.cableCreator =  transform.parent.gameObject.GetComponent<CableCreator>(); //gameObject.GetComponent<CableCreator>();

        this.cableCreator.isCreatable = false;

        this.index = index;
        Rigidbody rdb = gameObject.AddComponent<Rigidbody>();
        rdb.isKinematic = true;
    }

    void OnMouseDown()
    {
        MeshCollider mc = transform.parent.gameObject.GetComponent<MeshCollider>(); 
        
        Destroy(mc);
        
    }

    void OnMouseUp()
    {
        MeshCollider mc = transform.parent.gameObject.AddComponent<MeshCollider>(); 

    }

    void OnMouseDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);

        float distance;
        if (plane.Raycast(ray, out distance))
        {
            Vector3 point = ray.GetPoint(distance);
            transform.position = point;
            // cableCreator = gameObject.GetComponent<CableCreator>();
            cableCreator.UpdatePointPosition(index, point);
        }
    }
}
