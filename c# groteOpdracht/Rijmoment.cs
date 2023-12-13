//using System.Threading;

namespace rommelrouterakkers;
using System; 
public class Rijmoment
{
    public int volume;
    public int tijd;
    public Node beginnode;
    public Node eindnode;
    public Bus bus;
    public int Count;
    
    public Rijmoment(Bus buss)
    {
        bus = buss;
        volume = 0;
        Count = 0;

        beginnode = new Node(Setup.stort);
        eindnode  = new Node(Setup.stort);
        beginnode.Next = eindnode;
        eindnode.Previous = beginnode;
    }

    public void ToevoegenVoor(Node nieuw, Node volgende, int extratijd)
    {
        nieuw.rijmoment = this;
        
        bus.tijd += extratijd;
        bus.week.kosten += extratijd;
        volume += nieuw.bedrijf.volume;
        Count++;

        nieuw.Previous = volgende.Previous;
        nieuw.Next = volgende;
        volgende.Previous.Next = nieuw;
        volgende.Previous = nieuw;
    }

    
    public void LaatstToevoegen(Node nieuw, int extratijd)
    {
        ToevoegenVoor(nieuw, eindnode, extratijd);
    }

    public void Verwijderen(Node weg, int extratijd)
    {
        volume -= weg.bedrijf.volume;
        bus.tijd += extratijd;
        bus.week.kosten += extratijd;
        Count--;

        weg.Previous.Next = weg.Next;
        weg.Next.Previous = weg.Previous;
    }

    public void Load(Bedrijf b)
    {

        //bereken de tijd die daarvoor wordt toegevoegd
        int extratijd = ExtraTijdskostenBijToevoegen(b, eindnode.Previous, eindnode);

        LaatstToevoegen(b.FindUnusedNode(), extratijd);
        //stop bedrijf in dit rijmoment
        //voeg pointers toe van bedrijf naar deze node?

    }



    public int ExtraTijdskostenBijToevoegen(Bedrijf bedrijf, Node vorige, Node volgende)
    { 
        int extra = 0;
        extra += Setup.aMatrix.lookup(vorige.bedrijf, bedrijf);
        extra += Setup.aMatrix.lookup(bedrijf, volgende.bedrijf);
        extra -= Setup.aMatrix.lookup(vorige.bedrijf, volgende.bedrijf);
        extra += bedrijf.ledigingsDuur;
        return extra;
    }

    public void RijBFS()
    {
        if (Count < 2) return;
        int extratijd;

        Node node1 = beginnode.Next;
        Node node2 = node1.Next;

        while (node1.Next != eindnode)
        {
            while (node2 != eindnode)
            {
                extratijd = ExtraTijdsKostenBijWisselen(node1, node2);
                if (extratijd < 0)
                {
                    Wisselen(node1, node2, extratijd);
                    node1 = beginnode.Next;
                    node2 = node1;
                }
                node2 = node2.Next;
            }
            node1 = node1.Next;
            node2 = node1.Next;
        }
    }

    public void Wisselen(Node node, Node node2, int extratijd)
    {
        if (node.Next == node2)
            WisselNaastElkaar(node, node2);
        else if (node2.Next == node)
            WisselNaastElkaar(node2, node);
        else
        {
            Node next1 = node.Next;
            Node next2 = node2.Next;

            Verwijderen(node, 0);
            Verwijderen(node2, 0);

            ToevoegenVoor(node, next2, 0);
            ToevoegenVoor(node2, next1, 0);
        }
        bus.tijd += extratijd;
        bus.week.kosten += extratijd;
    }

    public int ExtraTijdsKostenBijWisselen(Node node, Node node2)
    {
        if (node.Next == node2)
            return ExtraTijdsKostenBijNaastWisselen(node, node2);
        else if (node2.Next == node)
            return ExtraTijdsKostenBijNaastWisselen(node2, node);
        else
        {
            return node.ExtraTijdskostenBijVerwijderen() +
                   node2.ExtraTijdskostenBijVerwijderen() +
                   ExtraTijdskostenBijToevoegen(node.bedrijf, node2.Previous, node2.Next) +
                   ExtraTijdskostenBijToevoegen(node2.bedrijf, node.Previous, node.Next);
        }
    }

    public int ExtraTijdsKostenBijNaastWisselen(Node node, Node node2)
    {
        return node.ExtraTijdskostenBijVerwijderen() + 
               ExtraTijdskostenBijToevoegen(node.bedrijf, node2, node2.Next);
    }

    public void WisselNaastElkaar(Node node, Node node2)
    {
        Verwijderen(node, 0);
        ToevoegenVoor(node, node2.Next, 0);
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

    public int Evaluate()
    {
        return 1;
    }
}

public class Node
{
    public Node Previous = null;
    public Node Next = null;
    public Bedrijf bedrijf;
    public Rijmoment rijmoment;

    public Node(Bedrijf bedrijf)
    {
        this.bedrijf = bedrijf;
    }

    public void Verwijder(int extratijd)
    {
        rijmoment.Verwijderen(this, extratijd);
    }

    //errorfull function
    public int ExtraTijdskostenBijVerwijderen()
    { 
        int extra = 0; 
        extra -= Setup.aMatrix.lookup(Previous.bedrijf, bedrijf); // da fak, ik zal ff kijken wat er fout kan zijn shit man
        extra -= Setup.aMatrix.lookup(bedrijf, Next.bedrijf); //ik heb nog niet echt iets met kosten gedaan
        extra += Setup.aMatrix.lookup(Previous.bedrijf, Next.bedrijf);
        extra -= bedrijf.ledigingsDuur;
        return extra; 
    }

    public string ToString(string str)
    {
        return str + ";" + bedrijf.orderNummer.ToString() + "\n";
    }
    
}

// bij aanmaken van bedrijf meteen nodes aanmaken die nog geen pointers hebben.
// als je hem toevoegt, maak je pointers. als je hem verwijdert,

    /// haal je de pointers weg
    // is gedaan
