using System;
using System.Collections;
using System.Collections.Generic;

public class Node
{
    public int Id { get; private set; }
    public int currentIndex = 0;
    
    public List<CircuitComponent> ConnectedComponents { get; private set; }

    public Node(int id)
    {
        Id = id;
        ConnectedComponents = new List<CircuitComponent>();
    }
}

