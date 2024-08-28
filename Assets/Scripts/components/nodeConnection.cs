public class NodeConnection
{
    public Node Node1 { get; private set; }
    public Node Node2 { get; private set; }
    public CircuitComponent Component { get; private set; }

    public NodeConnection(Node node1, Node node2, CircuitComponent component)
    {
        Node1 = node1;
        Node2 = node2;
        Component = component;
    }
}
