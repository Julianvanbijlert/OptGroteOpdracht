using System.Threading;

namespace rommelrouterakkers;
using System; 
public class Rijmoment
{
    public int volume;
    public float tijd;
    public Node beginnode;
    public Node eindnode;
    
    public Rijmoment(Bus buss)
    {
        tijd = 1800; // omgerekend naar seconden
        volume = 0;

        beginnode = new Node(Setup.stort);
        eindnode  = new Node(Setup.stort);
        beginnode.Next = eindnode;
        eindnode.Previous = beginnode;
    }

    public void ToevoegenVoor(Node nieuw, Node volgende, float extratijd)
    {
        tijd += extratijd;
        volume += nieuw.bedrijf.volume;

        nieuw.Previous = volgende.Previous;
        nieuw.Next = volgende;
        volgende.Previous.Next = nieuw;
        volgende.Previous = nieuw;

        nieuw.bedrijf.wordtBezocht = true;
    }

    public void LaatstToevoegen(Node nieuw, float extratijd)
    {
        ToevoegenVoor(nieuw, eindnode, extratijd);
    }

    public void Verwijderen(Node weg)
    {
        volume -= weg.bedrijf.volume;
        tijd += ExtraTijdskostenBijVerwijderen(weg);

        weg.Previous.Next = weg.Next;
        weg.Next.Previous = weg.Previous;

        weg.bedrijf.wordtBezocht = false;
    }

    public float ExtraTijdskostenBijToevoegen(Bedrijf bedrijf, Node volgende)
    { 
        float extra = 0;
        extra += Setup.aMatrix.lookup(volgende.Previous.bedrijf, bedrijf);
        extra += Setup.aMatrix.lookup(bedrijf, volgende.bedrijf);
        extra -= Setup.aMatrix.lookup(volgende.Previous.bedrijf, volgende.bedrijf);
        extra += bedrijf.ledigingsDuur * 60;
        return extra;
    }

    public float ExtraTijdskostenBijVerwijderen(Node node)
    {
        float extra = 0;
        extra -= Setup.aMatrix.lookup(node.Previous.bedrijf, node.bedrijf);
        extra -= Setup.aMatrix.lookup(node.bedrijf, node.Next.bedrijf);
        extra += Setup.aMatrix.lookup(node.Previous.bedrijf, node.Next.bedrijf);
        extra -= node.bedrijf.ledigingsDuur * 60;
        return extra;
    }

    //Zoekt de beste plek voor alle nodes binnen dat rijmoment op
    public Rijmoment RijBFS()
    {
        Rijmoment best = this;
        Rijmoment Current = this;
        Node beginNode = Current.beginnode;
        Node wisselNode = Current.beginnode.Next;

        while (beginNode.Next != Current.eindnode)
        {
            while (beginNode.Next != Current.eindnode)
            {
                //iets met wissel -> tijden geupdate    
                Current.Wisselen(beginNode, wisselNode);

                if (best.tijd > Current.tijd)
                {
                    best = Current;
                    Current = best;
                    beginNode = Current.beginnode;
                    wisselNode = Current.beginnode.Next;
                }

                wisselNode = wisselNode.Next;

            }
            beginNode = beginnode.Next;
        }

        return best;
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

            ToevoegenVoor(node, next2, ExtraTijdskostenBijToevoegen(node.bedrijf, next2));
            ToevoegenVoor(node2, next1, ExtraTijdskostenBijToevoegen(node2.bedrijf, next1));
        }
    }

    public void WisselNaastElkaar(Node node, Node node2)
    {
        Verwijderen(node);
        ToevoegenVoor(node, node2.Next, ExtraTijdskostenBijToevoegen(node.bedrijf, node2.Next));
    }

    public (int, string) ToString(string str, int c)
    {
        string s = "";
        Node current = beginnode;
        int count = c;
        while (current != eindnode)
        {
            current = current.Next;
            s += current.ToString(str + count.ToString());
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
