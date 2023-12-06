using System.Threading;

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
        eindnode  = new Node(Program.stort);
        beginnode.Next = eindnode;
        eindnode.Previous = beginnode;
    }

    public void ToevoegenVoor(Node nieuw, Node volgende)
    {
        nieuw.Previous = volgende.Previous;
        nieuw.Next = volgende;
        volgende.Previous.Next = nieuw;
        volgende.Previous = nieuw;

        volume += nieuw.bedrijf.volume;
        tijd += ExtraTijdskostenBijToevoegen(nieuw);
    }

    public void Verwijderen(Node weg)
    {
        volume -= weg.bedrijf.volume;
        tijd -= ExtraTijdskostenBijToevoegen(weg);

        weg.Previous.Next = weg.Next;
        weg.Next.Previous = weg.Previous;
    }

    public int ExtraTijdskostenBijToevoegen(Node node)
    {
        int extra = 0;
        extra += Program.aMatrix.lookup(node.Previous.bedrijf, node.bedrijf);
        extra += Program.aMatrix.lookup(node.bedrijf, node.Next.bedrijf);
        extra -= Program.aMatrix.lookup(node.Previous.bedrijf, node.Next.bedrijf);
        return extra;
    }


    // methode voor tegengestelde richting doorlopen? als buuroplossing

    public void Wisselen(Node node, Node node2)
    {
        if (node.Next == node2)
            WisselNaastElkaar(node, node2);
        else if (node2.Next == node)
            WisselNaastElkaar(node2, node);
        else
        {
            tijd -= ExtraTijdskostenBijToevoegen(node);
            tijd -= ExtraTijdskostenBijToevoegen(node2);

            Node nieuwn = node2.Next;
            Node nieuwp = node2.Previous;

            node2.Next = node.Next;
            node2.Previous = node.Previous;

            node.Next = nieuwn;
            node.Previous = nieuwp;

            tijd += ExtraTijdskostenBijToevoegen(node);
            tijd += ExtraTijdskostenBijToevoegen(node2);
        }
    }

    public void WisselNaastElkaar(Node node, Node node2)
    {
        tijd -= ExtraTijdskostenBijToevoegen(node);
        Verwijderen(node);
        ToevoegenVoor(node, node2.Next);
        tijd += ExtraTijdskostenBijToevoegen(node);
    }

    public string ToString(string str, int c)
    {
        string s = "";
        Node current = beginnode;
        int count = c;
        while (current.Next != eindnode)
        {
            count++;
            s += current.ToString(str + ";" + count.ToString());

        }
        return ";";
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

    public void Verwijder()
    {
        this.Previous.Next = this.Next;
        this.Next.Previous = this.Previous;
    }

    public string ToString(string str)
    {
        return str + ";" + bedrijf.orderNummer.ToString() + "\n";
    }
    
}

// bij aanmaken van bedrijf meteen nodes aanmaken die nog geen pointers hebben.
// als je hem toevoegt, maak je pointers. als je hem verwijdert, haal je de pointers weg
    // is gedaan
