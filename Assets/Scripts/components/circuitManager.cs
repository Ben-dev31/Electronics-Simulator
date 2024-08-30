using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
    public Dictionary<int, Node> Nodes { get; private set; }
    public Dictionary<int, CircuitComponent> Components { get; private set; }
    public int componentCount = 0;
    public int componentValue = 0;

    private void Start()
    {
        Nodes = new Dictionary<int, Node>();
        Components = new Dictionary<int, CircuitComponent>();

        // Initialiser le circuit avec les composants présents dans la scène
        InitializeCircuit();
    }

    public void InitializeCircuit()
    {
        Components.Clear();
        CircuitComponent[] componentsInScene = FindObjectsOfType<CircuitComponent>();
        foreach (var component in componentsInScene)
        {
            AddComponent(component);
        }
        componentCount = Components.Count;
    }

    public void AddComponent(CircuitComponent component)
    {
        // Vérifier les connexions avant d'ajouter le composant
        if (ValidateConnections(component))
        {
            
            Components[component.Id] = component;

            foreach (var node in new Node[] { component.Node1, component.Node2 })
            {
                if (!Nodes.ContainsKey(node.Id))
                {
                    Nodes[node.Id] = node;
                }
                Nodes[node.Id].ConnectedComponents.Add(component);
            }
        }
        else
        {
            Debug.LogWarning($"Invalid connection for component {component.gameObject.name}");
        }
    }

    public void RemoveComponent(int componentId)
    {
        if (Components.ContainsKey(componentId))
        {
            var component = Components[componentId];
            foreach (var node in new Node[] { component.Node1, component.Node2 })
            {
                node.ConnectedComponents.Remove(component);
            }
            Components.Remove(componentId);
        }
    }

    // public List<Node> DetectNodes()
    // {
        

    //     List<Node> detectedNodes = new List<Node>();
    //     Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();

    //     foreach (var component in Components.Values)
    //     {
    //         foreach (var node in new Node[] { component.Node1, component.Node2 })
    //         {
    //             if (!nodeMap.ContainsKey(node.Id))
    //             {
    //                 Node newNode = new Node(node.Id);
    //                 detectedNodes.Add(newNode);
    //                 nodeMap[node.Id] = newNode;
    //             }
    //             nodeMap[node.Id].ConnectedComponents.Add(component);
    //         }
    //     }

    //     return detectedNodes;
    // }

    public List<Node> DetectNodes()
    {
        List<Node> detectedNodes = new List<Node>();
        Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();

        foreach (var component in Components.Values)
        {
            
            if(component.bp.connectionCount > 1 && !nodeMap.ContainsKey(component.bp.nodeId))
            {
                Node newNode = new Node(component.bp.nodeId);
                detectedNodes.Add(newNode);
                nodeMap[component.bp.nodeId] = newNode;
                nodeMap[component.bp.nodeId].ConnectedComponents.Add(component);
            }
            if(component.bn.connectionCount > 1 && !nodeMap.ContainsKey(component.bn.nodeId))
            {
                Node newNode = new Node(component.bn.nodeId);
                detectedNodes.Add(newNode);
                nodeMap[component.bn.nodeId] = newNode;
                nodeMap[component.bn.nodeId].ConnectedComponents.Add(component);
            }
            
        }

        

        return detectedNodes;
    }

    public List<List<Node>> DetectLoops2()
    {
        HashSet<Node> visited = new HashSet<Node>();
        List<List<Node>> loops = new List<List<Node>>();

        void Dfs(Node node, List<Node> currentPath, Node startNode)
        {
            if (visited.Contains(node)) return;
            visited.Add(node);
            currentPath.Add(node);

            foreach (var component in node.ConnectedComponents)
            {
                Node nextNode = component.GetOtherNode(node);
                if (nextNode == startNode)
                {
                    loops.Add(new List<Node>(currentPath) { startNode });
                }
                else if (!visited.Contains(nextNode))
                {
                    Dfs(nextNode, new List<Node>(currentPath), startNode);
                }
            }
        }

        foreach (var node in Nodes.Values)
        {
            if (!visited.Contains(node))
            {
                Dfs(node, new List<Node>(), node);
            }
        }

        return loops;
    }

    public List<Loop> DetectLoops()
    {
        HashSet<Node> visitedNodes = new HashSet<Node>();
        List<Loop> AllLoops = new List<Loop>();
        int loopCounter = 0;

        if (Components.Count < 2) return AllLoops;

        List<Node> nodes = DetectNodes();

        // Check for null or empty cases before proceeding
        // if (nodes == null || nodes.Count == 0) return AllLoops;

        void Dfss(Borne startBorne, CircuitComponent startComponent, List<CircuitComponent> currentLoop)
        {
            if (startComponent == null || startBorne == null) return;

            if (currentLoop.Contains(startComponent))
            {
                // If we revisit the same component and it's the first one, we have a loop
                if (startComponent == currentLoop[0])
                {
                    loopCounter++;
                    AllLoops.Add(new Loop(new List<CircuitComponent>(currentLoop))); // Use ToList() to avoid reference issues
                    // print($"lp : {AllLoops[AllLoops.Count - 1].Count}");
                    return;
                }
            }

            currentLoop.Add(startComponent);

            Wire cable = startBorne.cable;
            if (cable == null) return;

            Borne nextBorne = cable.GetOtherBorne(startBorne);
            if (nextBorne == null) return;

            CircuitComponent nextComponent = nextBorne.Parent.GetComponent<CircuitComponent>();
            if (nextComponent == null) return;

            nextBorne = nextComponent.GetOtherBorne(nextBorne);

            Dfss(nextBorne, nextComponent, currentLoop);

            // Backtracking: remove the component after recursion
            currentLoop.RemoveAt(currentLoop.Count - 1);
        }

        // Traverse all components and nodes to ensure no loop is missed
        foreach (var component in Components.Values)
        {
            if (component == null || component.bp == null) continue;

            Borne initialBorne = component.bp;

            Dfss(initialBorne, component, new List<CircuitComponent>());
            
        }

        if (nodes.Count > 0)
        {
            foreach (var node in nodes)
            {
                CircuitComponent nodeComponent = node.ConnectedComponents[0];
                Borne nodeBorne = nodeComponent.bp;

                if (!visitedNodes.Contains(node) && nodeBorne != null)  // Avoid revisiting nodes
                {
                    visitedNodes.Add(node);
                    Dfss(nodeBorne, nodeComponent, new List<CircuitComponent>());
                }
            }
        }

        // Prevent access to an empty list 
        // if (AllLoops.Count > 0)
        // {
        //     print($"loops test : {AllLoops[0].loopElements.Count}");
        // }

        return AllLoops;
    }

    

    private Loop DetectMainLoop(List<Loop> loops)
    {
        List<Loop> MainLoop = new List<Loop>();

        foreach (Loop item in loops)
        {
            if(item.IsMainLoop())
            {
                MainLoop.Add(item);
            }
        }
        if(MainLoop.Count == 1) return MainLoop[0];

        Loop main = MainLoop[0];

        foreach (Loop lp in MainLoop)
        {
            if(lp.MultiConnectionCount() > main.MultiConnectionCount())
            {
                main = lp;
            }
        }

        return main;

    }

    private bool ValidateConnections(CircuitComponent component)
    {
        return component.TrueConnection();
    }

    // calcul des grandeurs 
    public void CalculateCurrentsAndVoltages()
    {
        InitializeCircuit();
        // DetectNodes();
        // var nodeEquations = GenerateNodeEquations();

        List<Loop> loops = DetectLoops();
        var loopEquations = GenerateLoopEquations(loops);

        // var allEquations = new List<Equation>(nodeEquations);
        // allEquations.AddRange(loopEquations);
        // print(allEquations.Count);
        var solutions = SolveVoltageEquations(loopEquations);
        UpdateComponentVoltage(solutions);

        UpdateComponentCurrentValues(loops);
    }

    void UpdateComponentCurrentValues(List<Loop> loops)
    {
       
        foreach (var loop in loops)
        {
            foreach (CircuitComponent component in loop.loopElements)
            {
                if(!(component.Id == 0))
                {
                    component.Value = 1f;
                    component.Current = component.Voltage / component.Resistance;
                }
            }
        }
        CurrentDirectionControl(loops);

    }

    private List<Equation> GenerateNodeEquations()
    {
        List<Node> nodes = DetectNodes();
        // print($"Nodes: {nodes.Count}");

        var equations = new List<Equation>();
        foreach (var node in Nodes.Values)
        {
            var equation = new Equation();
            foreach (var component in node.ConnectedComponents)
            {
                var current = component.Id; // Utiliser l'ID du composant comme variable de courant
                if (component.Node1 == node)
                {
                    equation.AddTerm(current, 1);
                }
                else
                {
                    equation.AddTerm(current, -1);
                }
            }
            equation.SetEqualTo(0);
            equations.Add(equation);
        }
        
        return equations;
    }

    private List<Equation> GenerateLoopEquations(List<Loop> loops)
    {
        var equations = new List<Equation>();
 
        foreach (Loop loop in loops)
        {
            
            print($"loop el : {loop.Count}");

            var equation = new Equation();
            foreach(CircuitComponent component in loop.loopElements)
            {
                
                float voltage = component.Voltage;
                equation.AddTerm(component.Id, component.Type == "Battery" ? voltage : -voltage);
            }
            equation.SetEqualTo(0);
            equations.Add(equation);

        }
        return equations;
    }

    private Dictionary<int, float> SolveVoltageEquations(List<Equation> equations)
    {
        // Implémention de la méthode de résolution des équations ici
        // Par exemple, utilisz la méthode de Gauss-Seidel
        var solutions = new Dictionary<int, float>();

        foreach (Equation item in equations)
        {
            Dictionary<int, float> terms = item.GetTerms();
            // print($"terms : {terms.Count}");

            float equalTo = item.GetEqualTo();

            float initialValtage = 0f;
            int unk = 0;
            int initialId = -25;

            float EquivResistance = 0f;

            foreach (int key in terms.Keys)
            {
                EquivResistance += Components[key].Resistance;
            }

            foreach (int key in terms.Keys)
            {
                if(Components[key].Voltage != 0)
                {
                    initialValtage = Components[key].Voltage;
                    initialId = key;
                    // print($"key: {initialId}");
                }
            }
            foreach (int key in terms.Keys)
            {
                if(!(Components[key].Id == initialId ) && !(Components[key].Type == "Switch"))
                {
                    float voltage = initialValtage * Components[key].Resistance/EquivResistance;
                    if(!solutions.ContainsKey(key))
                    {
                        solutions.Add(Components[key].Id, voltage);
                    }
                }
                
            }

        }
        
        // print($"solutions : {solutions.Count}");
        return solutions;
    }

    private void UpdateComponentVoltage(Dictionary<int, float> solutions)
    {
        foreach (var component in Components.Values)
        {
            if (solutions.ContainsKey(component.Id))
            {
                // Mettre à jour les valeurs de courant et de tension du composant
                
                component.Voltage = solutions[component.Id]; // component.Value; // Exemple simplifié
            }
        }
    }

    private void CurrentDirectionControl(List<Loop> loops)
    {
        List<Loop> AllLoops = new List<Loop>(loops);
        Loop MainLoop = DetectMainLoop(AllLoops);

        AllLoops.RemoveAt(AllLoops.IndexOf(MainLoop));

        CircuitComponent main = MainLoop.GetMainComponent();

        Borne[] Bornecomponents = FindObjectsOfType<Borne>();
        foreach (Borne item in Bornecomponents)
        {
            item.ResetRange();
        }

        List<CircuitComponent> visitedCp = new List<CircuitComponent>();

        void gss(CircuitComponent cp, Borne startBorne)
        {
            if(visitedCp.Contains(cp)) return;
           
            Borne nextBorne = startBorne.cable.GetOtherBorne(startBorne);
            CircuitComponent nextComponent = nextBorne.Parent.GetComponent<CircuitComponent>();
            Borne newStartBorne = nextComponent.GetOtherBorne(nextBorne);

            nextBorne.Polarisation = -1;
            newStartBorne.Polarisation = 1;
            cp.CurrentDirectionControl();
            visitedCp.Add(cp);

            gss(nextComponent,newStartBorne);
        }
        // print($"{main.gameObject.name}");
        gss(main, main.bp);

    }

}


public class Loop 
{
    public List<CircuitComponent> loopElements;

    private List<Wire> loopCable = new List<Wire>();

    public int loopId;

    public int Count
    {
        get{
            return loopElements.Count;
        }
        private set{}
    }

    public Loop( List<CircuitComponent> loop = null)
    {
        Debug.Log($"in loop class : {loop.Count}");
        this.loopElements = loop;
    }

    public void SetElements(List<CircuitComponent> loop)
    {
        this.loopElements = loop;
    }

    public void Add(CircuitComponent el)
    {
        loopElements.Add(el);
    }
    
    public int MultiConnectionCount()
    {
        int k = 0;
        foreach (CircuitComponent cp in loopElements)
        {
            k += cp.cables.Count - 2;
        }

        return k;
    }

    public bool IsMainLoop()
    {
        foreach (CircuitComponent cp in loopElements)
        {
            if(cp.Type == "Battery")
            {
                return true;
            }
        }
        
        return false;
    }
    public void AddCable(Wire cable)
    {
        if(!loopCable.Contains(cable))
        {
            loopCable.Add(cable);
        }
        else{
            Debug.LogWarning($"Loop {loopId} already contains {cable.gameObject.name}");
        }
    }
    public CircuitComponent GetMainComponent()
    {
        CircuitComponent Comp = loopElements[0];

        foreach (CircuitComponent cp in loopElements)
        {
            if(cp.Type == "Battery")
            {
                Comp =  cp;
            }
            else if(cp.cables.Count > Comp.cables.Count)
            {
                Comp = cp;
            }
            
        }
       
       return Comp;
    }

}




public class Equation
{
    private Dictionary<int, float> terms = new Dictionary<int, float>();
    private float equalTo;

    public void AddTerm(int variable, float coefficient)
    {
        if (terms.ContainsKey(variable))
        {
            terms[variable] += coefficient;
        }
        else
        {
            terms[variable] = coefficient;
        }
    }

    
    public void SetEqualTo(float value)
    {
        equalTo = value;
    }

    public Dictionary<int, float> GetTerms()
    {
        return terms;
    }

    public float GetEqualTo()
    {
        return equalTo;
    }

   
}
