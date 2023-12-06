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

    public void ToevoegenVoor(Bedrijf bedrijf, Node volgende)
    {
        Node nieuw = new Node(bedrijf);
        nieuw.Previous = volgende.Previous;
        nieuw.Next = volgende;
        volgende.Previous.Next = nieuw;
        volgende.Previous = nieuw;

        //tijden erbij optellen
    }

    public void Verwijderen(Node node)
    {
        node.Previous.Next = node.Next;
        node.Next.Previous = node.Previous;

        //tijden eraf halen en nieuwe rijtijd optellen
    }

    public void Wisselen(Node bedrijf, Node bedrijf2)
    {
        Node nieuwn = bedrijf2.Next;
        Node nieuwp = bedrijf2.Previous;

        bedrijf2.Next = bedrijf.Next;
        bedrijf2.Previous = bedrijf.Previous;

        bedrijf.Next = nieuwn;
        bedrijf.Previous = nieuwp;

        //oppassen wat er gebeurd met de tijden
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
