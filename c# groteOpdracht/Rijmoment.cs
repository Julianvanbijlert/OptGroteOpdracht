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
        tijd = 1800; // omgerekend naar seconden
        volume = 0;

        beginnode = new Node(Program.stort);
        eindnode  = new Node(Program.stort);
        beginnode.Next = eindnode;
        eindnode.Previous = beginnode;
    }

    public void ToevoegenVoor(Node nieuw, Node volgende)
    {
        tijd += ExtraTijdskostenBijToevoegen(nieuw.bedrijf, volgende);
        volume += nieuw.bedrijf.volume;

        nieuw.Previous = volgende.Previous;
        nieuw.Next = volgende;
        volgende.Previous.Next = nieuw;
        volgende.Previous = nieuw;
    }

    public void LaatstToevoegen(Node nieuw)
    {
        ToevoegenVoor(nieuw, eindnode);
    }

    public void Verwijderen(Node weg)
    {
        volume -= weg.bedrijf.volume;
        tijd += ExtraTijdskostenBijVerwijderen(weg);

        weg.Previous.Next = weg.Next;
        weg.Next.Previous = weg.Previous;
    }

    public int ExtraTijdskostenBijToevoegen(Bedrijf bedrijf, Node volgende)
    {
        int extra = 0;
        extra += Program.aMatrix.lookup(volgende.Previous.bedrijf, bedrijf);
        extra += Program.aMatrix.lookup(bedrijf, volgende.bedrijf);
        extra -= Program.aMatrix.lookup(volgende.Previous.bedrijf, volgende.bedrijf);
        return extra;
    }

    public int ExtraTijdskostenBijVerwijderen(Node node)
    {
        int extra = 0;
        extra -= Program.aMatrix.lookup(node.Previous.bedrijf, node.bedrijf);
        extra -= Program.aMatrix.lookup(node.bedrijf, node.Next.bedrijf);
        extra += Program.aMatrix.lookup(node.Previous.bedrijf, node.Next.bedrijf);
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
            Node next1 = node.Next;
            Node next2 = node2.Next;

            Verwijderen(node);
            Verwijderen(node2);

            ToevoegenVoor(node, next2);
            ToevoegenVoor(node2, next1);
        }
    }

    public void WisselNaastElkaar(Node node, Node node2)
    {
        tijd += ExtraTijdskostenBijVerwijderen(node);
        Verwijderen(node);
        tijd += ExtraTijdskostenBijToevoegen(node.bedrijf, node2.Next);
        ToevoegenVoor(node, node2.Next);
    }

    public (int, string) ToString(string str, int c)
    {
        string s = "";
        Node current = beginnode;
        int count = c;
        while (current.Next != eindnode)
        {
            s += current.ToString(str + count.ToString());
            current = current.Next;
            count++;

        }
        return (count,s);
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
