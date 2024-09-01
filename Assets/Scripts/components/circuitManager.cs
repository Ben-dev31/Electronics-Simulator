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

        void Dfss(Wire startcable, CircuitComponent startComponent, List<CircuitComponent> currentLoop, List<Wire> cap)
        {
            if (startComponent == null) return;

            if (currentLoop.Contains(startComponent))
            {
                loopCounter++;
                Loop lp = new Loop(new List<CircuitComponent>(currentLoop));
                lp.loopCable = cap;
                AllLoops.Add(lp); 
                return;
                
            }

            currentLoop.Add(startComponent);

            cap.Add(startcable);

            List<Wire> cables = startComponent.GetAllCable();
 
            if(cables.Count > 2)
            {
                foreach (Wire item in cables)
                { 
                    if(item != startcable)
                        Dfss(item,startComponent,currentLoop,cap);
                }
            }
            else
            {
                int idx = cables.IndexOf(startcable);
                Wire cable = idx == 1 ? cables[0] : cables[1];
                CircuitComponent newStartComponent = cable.GetOtherComponent(startComponent);

                Dfss(cable,newStartComponent,currentLoop,cap);
            }
        }

        CircuitComponent startComponent = Components[0];
        Wire startcable = startComponent.GetAllCable()[0];

        Dfss(startcable, startComponent, new List<CircuitComponent>(), new List<Wire>());

        // print($"all : {AllLoops.Count}");

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

    private void IdentifiLoops(List<Loop> loops)
    {
        List<Loop> AllLp = loops;

        Loop main = DetectMainLoop(loops);
        main.LoopId = 0;
        AllLp.Remove(main);

        int i = 1;
        foreach (Loop lp in AllLp)
        {
            lp.LoopId = i;
            i++;
        }
        

    }
    // calcul des grandeurs 
    public void CalculateCurrentsAndVoltages()
    {
        InitializeCircuit();
        // DetectNodes();
        // var nodeEquations = GenerateNodeEquations();
        
        List<Loop> loops = DetectLoops();

        // print($"loops : {loops.Count}");

        IdentifiLoops(new List<Loop>(loops));

        var loopEquations = GenerateLoopEquations(new List<Loop>(loops));

        // var allEquations = new List<Equation>(nodeEquations);
        // allEquations.AddRange(loopEquations);
        // print(allEquations.Count);
        UpdateComponentCurrentValues(new List<Loop>(loops));
        
        var solutions = SolveVoltageEquations(loopEquations);
        UpdateComponentVoltage(solutions);

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
        Borne[] Bornecomponents = FindObjectsOfType<Borne>();

        foreach (Borne item in Bornecomponents)
        {
            item.ResetRange();
        }

        CircuitComponent main = MainLoop.GetMainComponent();

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

        print($"{main.gameObject.name}");

        gss(main, main.bp);

        foreach (Loop loop in AllLoops)
        {
            CircuitComponent mainC = loop.GetMainComponent();
            gss(mainC, mainC.bp);
        }

    }

}


public class Loop 
{
    public List<CircuitComponent> loopElements;

    public List<Wire> loopCable = new List<Wire>();
    private int _loopId;
    public int LoopId
    {
        get{
            return _loopId;
        }
        set{
            _loopId = value;
            IdentifiElements();
        }
    }

    public int Count
    {
        get{
            return loopElements.Count;
        }
        private set{}
    }

    public Loop( List<CircuitComponent> loop = null)
    {
        this.loopElements = loop;
    }

    // Association des ids de loop aux cable et aux composant de la boucle
    private void IdentifiElements()
    {
        foreach (Wire cb in loopCable)
        {
            cb.loopId = LoopId;
        }
        foreach (CircuitComponent cp in loopElements)
        {
            cp.loopIdList.Add(LoopId);
        }
    }

    public bool IsCorrect()
    {
        if(loopCable.Count != loopElements.Count)
            return false;
        
        return true;
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
            Debug.LogWarning($"Loop {_loopId} already contains {cable.gameObject.name}");
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


    public void CurrentDirection()
    {
        CircuitComponent M = this.GetMainComponent();

        Borne Bn = M.bn;

        void guu(CircuitComponent startCp, Borne startBn)
        {
            
        }
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
