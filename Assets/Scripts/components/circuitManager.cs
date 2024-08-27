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

    public List<List<Node>> DetectLoops()
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

    public List<List<CircuitComponent>> DetectLoops2()
    {
        HashSet<Node> visitedNodes = new HashSet<Node>();  // Pour suivre les nœuds visités
        List<List<CircuitComponent>> loopComponents = new List<List<CircuitComponent>>();
        int loopCounter = 0;

        if (Components.Count < 2) return loopComponents; 

        List<Node> nodes = DetectNodes();
        print($"Nodes: {nodes.Count}");

        CircuitComponent initialComponent = Components[0];
        if (initialComponent == null)
        {
            print("component 0 is null");
            return loopComponents;
        }

        Borne initialBorne = initialComponent.bp;

        void Dfss(Borne startBorne, CircuitComponent startComponent, List<CircuitComponent> currentLoop)
        {
            // print($"{startComponent.gameObject.name}");

            if (currentLoop.Contains(startComponent))
            {
                // Si nous revisitons le même composant, une boucle est détectée
                if (startComponent == currentLoop[0])
                {
                    loopCounter++;
                    loopComponents.Add(new List<CircuitComponent>(currentLoop));  // Ajouter une copie pour éviter les références partagées
                }
                return;
            }

            currentLoop.Add(startComponent);
            // print($"{currentLoop.Count}");

            Borne nextBorne = startBorne.cable.GetOtherBorne(startBorne);
            if (nextBorne == null) return;

            CircuitComponent nextComponent = nextBorne.Parent.GetComponent<CircuitComponent>();
            if (nextComponent == null) return;

            nextBorne = nextComponent.GetOtherBorne(nextBorne);

            Dfss(nextBorne, nextComponent, currentLoop);
            
            if(currentLoop.Count > 0)
            {
                currentLoop.RemoveAt(currentLoop.Count - 1);  // Retirer le composant après la récursion pour éviter les références partagées
            }
        }

        Dfss(initialBorne, initialComponent, new List<CircuitComponent>());

        if (nodes.Count > 0)
        {
            foreach (var node in nodes)
            {
                CircuitComponent nodeComponent = node.ConnectedComponents[0];
                Borne nodeBorne = nodeComponent.bp;
 
                if (!visitedNodes.Contains(node))  // Pour éviter de revisiter les mêmes nœuds
                {
                    visitedNodes.Add(node);
                    Dfss(nodeBorne, nodeComponent, new List<CircuitComponent>());
                }
            }
        }

        // print($"loop:: {loopComponents.Count}");
        // print($"loop counter : {loopCounter}");
        foreach (var item in loopComponents)
        {
            print($"loop el:: {item.Count}");
        }
        // print($"===============");

        return loopComponents;
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

        UpdateComponentCurrentValues();

        var loopEquations = GenerateLoopEquations();

        // var allEquations = new List<Equation>(nodeEquations);
        // allEquations.AddRange(loopEquations);
        // print(allEquations.Count);
        var solutions = SolveEquations(loopEquations);
        UpdateComponentVoltage(solutions);
    }

    void UpdateComponentCurrentValues()
    {
        List<List<CircuitComponent>> loops = DetectLoops2();
       
        CircuitComponent mainComponent = FindObjectsOfType<generator>()[0].GetComponent<CircuitComponent>();

        foreach (var loop in loops)
        {
            foreach (CircuitComponent component in loop)
            {
                if(!(component.Id == 0))
                {
                    component.Value = 1f;
                    component.Current = mainComponent.Current;
                }
            }
        }

    }

    private List<Equation> GenerateNodeEquations()
    {
        List<Node> nodes = DetectNodes();
        print($"Nodes: {nodes.Count}");

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

    private List<Equation> GenerateLoopEquations()
    {
        var equations = new List<Equation>();
        List<List<CircuitComponent>> loops = DetectLoops2();
        
        foreach (var loop in loops)
        {
            var equation = new Equation();
            for (int i = 0; i < loop.Count - 1; i++)
            {
                CircuitComponent component = loop[i];
                float voltage = component.Voltage;
                equation.AddTerm(component.Id, component.Type == "Battery" ? voltage : -component.Current*component.Resistance);
            }
            equation.SetEqualTo(0);
            equations.Add(equation);
        }
        return equations;
    }

    private Dictionary<int, float> SolveEquations(List<Equation> equations)
    {
        // Implémention de la méthode de résolution des équations ici
        // Par exemple, utilisz la méthode de Gauss-Seidel
        var solutions = new Dictionary<int, float>();

        foreach (Equation item in equations)
        {
           

            Dictionary<int, float> solution = new Dictionary<int, float>();
            Dictionary<int, float> terms = item.GetTerms();
            print($"terms : {terms.Count}");

            float equalTo = item.GetEqualTo();

            float initialValtage = 0f;
            int unk = 0;

            foreach(CircuitComponent component in Components.Values)
            {
                if(component.Id == 0)
                {
                    initialValtage = component.Voltage;
                }
                else if(component.Resistance == 0)
                {
                    solution[component.Id] = 0;
                    unk++;
                }
                else
                {
                    solution.Add(component.Id, terms[component.Id]);
                }
            }

            // correction  de la resolution
            float sum = 0;

            for(int j = 0; j < solution.Count; j++)
            {
                sum += solution[j];
            }

            if(sum == equalTo)
            {
                return solution;
            } 
            else
            {
                for(int k = 0; k < solution.Count; k++)
                {
                    if(solution[k] == 0)
                    {
                        solution[k] = (sum - equalTo)/unk;
                    }
                }

            }
            
            for(int key = 0; key < solution.Count; key++)
            {
                solutions[key] = solution[key];
            }
        }
        
        print($"solutions : {solutions.Count}");
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
                // component.Value = componentValue;
            }
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
