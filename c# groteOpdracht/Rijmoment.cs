namespace rommelrouterakkers;
using System; 
public class Rijmoment
{
    public int volume;
    public int tijd;
    public Node beginnode;
    public Node eindnode;
    
    public Rijmoment()
    {
        tijd = 30;
        volume = 0;

        beginnode = new Node(Program.stort);
        eindnode = new Node(Program.stort);
        beginnode.Next = eindnode;
        eindnode.Previous = beginnode;
    }

    public void ToevoegenVoor(Bedrijf bedrijf, Node volgende)
    {
        Node nieuw = new Node(bedrijf);
        nieuw.Previous = volgende.Previous;
        nieuw.Next = volgende;
        volgende.Previous.Next = nieuw;
        volgende.Previous = nieuw;
    }

    public void Verwijderen(Node node)
    {
        node.Previous.Next = node.Next;
        node.Next.Previous = node.Previous;
    }

    public void Wisselen(Node bedrijf, Node bedrijf2)
    {
        Node nieuwn = bedrijf2.Next;
        Node nieuwp = bedrijf2.Previous;

        bedrijf2.Next = bedrijf.Next;
        bedrijf2.Previous = bedrijf.Previous;

        bedrijf.Next = nieuwn;
        bedrijf.Previous = nieuwp;

    }
}

public class Node
{
    public Node Next = null;
    public Node Previous = null;
    public Bedrijf bedrijf;

    public Node(Bedrijf bedrijf)
    {
        this.bedrijf = bedrijf;
    }
}