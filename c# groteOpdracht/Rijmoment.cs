namespace rommelrouterakkers;

public class Rijmoment
{
    public int volume;
    public int tijd;
    public Node beginnode;
    public Node eindnode;
    public Bus bus;
    public EigenArray<Node> nodeLijst;

    public Rijmoment(Bus buss)
    {
        bus = buss;
        volume = 0;

        beginnode = new Node(Setup.stort); // maak de 2 stortnodes
        eindnode  = new Node(Setup.stort);
        beginnode.Next = eindnode;
        eindnode.Previous = beginnode;
        beginnode.rijmoment = this;
        eindnode.rijmoment = this;

        nodeLijst = new EigenArray<Node>();
    }
    public int ExtraTijdskostenBijToevoegen(Bedrijf bedrijf, Node vorige, Node volgende) // bereken de incrementele kosten die ontstaan bij toevoegen
                                                                                         // van dit bedrijf na de node vorige en vóór de node volgende
    {
        int extra = 0;
        extra += Setup.aMatrix.lookup(vorige.bedrijf, bedrijf); // tel de rijtijd van de vorige naar deze node erbijop
        extra += Setup.aMatrix.lookup(bedrijf, volgende.bedrijf); // tel de rijtijd van deze naar de volgende erbijop
        extra -= Setup.aMatrix.lookup(vorige.bedrijf, volgende.bedrijf); // haal de rijtijd van de vorige naar de volgende ervanaf
        extra += bedrijf.ledigingsDuur;

        if (vorige == beginnode && volgende == eindnode) // als het rijmoment leeg was, tel de stortkosten erbijop
        {
            extra += 1800 * 1000;
        }
        return extra;
    }

    public void ToevoegenVoor(Node nieuw, Node volgende, int extratijd) // voegt Node nieuw toe vóór volgende
    {
        nieuw.rijmoment = this;
        
        bus.tijd += extratijd;
        bus.week.kosten += extratijd;
        volume += nieuw.bedrijf.volume;

        nieuw.Previous = volgende.Previous; // deze 4 regels zetten de pointers van de nodes goed
        nieuw.Next = volgende;
        volgende.Previous.Next = nieuw;
        volgende.Previous = nieuw;

        nieuw.Plaats = nodeLijst.Count;
        nodeLijst.Add(nieuw); 
    }
    
    public void LaatstToevoegen(Node nieuw, int extratijd) // dan hoef je niet de hele tijd eindnode als argument mee te geven
    {
        ToevoegenVoor(nieuw, eindnode, extratijd);
    }

    public void Verwijderen(Node weg, int extratijd) // verwijder de node uit het rijmoment
    {
        volume -= weg.bedrijf.volume;
        bus.tijd += extratijd;
        bus.week.kosten += extratijd;

        weg.Previous.Next = weg.Next; // deze twee regels zetten de pointers van de nodes goed
        weg.Next.Previous = weg.Previous;

        weg.rijmoment = null;

        nodeLijst[nodeLijst.Count - 1].Plaats = weg.Plaats;
        nodeLijst.RemoveAt(weg.Plaats); 
    }

    public int ExtraTijdsKostenBijWisselen(Node node, Node node2) // Berekent de incrementele kosten die ontstaan na wisselen van node en node2
    {
        if (node.Next == node2) // aparte casussen voor als ze naast elkaar staan
            return ExtraTijdsKostenBijNaastWisselen(node, node2);
        else if (node2.Next == node)
            return ExtraTijdsKostenBijNaastWisselen(node2, node);
        else
        {
            return node.ExtraTijdskostenBijVerwijderen() +        // deze 4 lines berekenen de incrementele tijdskosten
                   node2.ExtraTijdskostenBijVerwijderen() +
                   ExtraTijdskostenBijToevoegen(node.bedrijf, node2.Previous, node2.Next) +
                   ExtraTijdskostenBijToevoegen(node2.bedrijf, node.Previous, node.Next);
        }
    }

    public int ExtraTijdsKostenBijNaastWisselen(Node node, Node node2) // berekent de incrementele kosten bij wisselen van naastgelegen nodes
    {
        return node.ExtraTijdskostenBijVerwijderen() +
               ExtraTijdskostenBijToevoegen(node.bedrijf, node2, node2.Next);
    }

    public void Wisselen(Node node, Node node2, int extratijd) // wisselt twee nodes
    {
        if (node.Next == node2) // aparte casussen voor als node en node2 naast elkaar staan
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

    public void WisselNaastElkaar(Node node, Node node2) // wisselt twee naastgelegen nodes
    {
        Verwijderen(node, 0);
        ToevoegenVoor(node, node2.Next, 0);
    }

    public void RijBFS() // Vindt binnen een rijmoment heel snel een lokaal optimum,
                         // handig als je bijvoorbeeld een slechte oplossing even heel snel beter wilt maken
    {
        if (beginnode.Next == eindnode || beginnode.Next.Next == eindnode) // als er minder dan 2 nodes in zitten
            return;

        int extratijd;

        Node node1 = beginnode.Next;
        Node node2 = node1.Next;

        while (node1.Next != eindnode) // terwijl node1 nog niet bij het einde is
        {
            while (node2 != eindnode) // terwijl node2 nog niet bij het einde is
            {
                extratijd = ExtraTijdsKostenBijWisselen(node1, node2);
                if (extratijd < 0) // als deze wissel voordelig is
                {
                    Wisselen(node1, node2, extratijd); // doe de wissel
                    node1 = beginnode.Next; // en begin weer helemaal opnieuw met BFS
                    node2 = node1.Next;
                }
                else
                {
                    node2 = node2.Next; // node2 gaat 1 verder
                }
            }
            node1 = node1.Next; // node1 gaat 1 verder
            node2 = node1.Next; // node2 reset weer naar node1.Next
        }
    }

    public void Load(Bedrijf b) // leest een bedrijf in naar dit rijmoment
    {
        //bereken de incrementele kosten die ontstaan door inlezen van dit bedrijf
        int extratijd = ExtraTijdskostenBijToevoegen(b, eindnode.Previous, eindnode);

        //voeg een lege node van het bedrijf toe aan dit rijmoment
        LaatstToevoegen(b.FindUnusedNode(), extratijd);
    }

    public (int, string) ToString(string str, int c) // maak een string van het rijmoment
    {
        string s = "";
        Node current = beginnode;
        int count = c;
        while (current != eindnode) // maak een string van elke node en voeg die toe aan de rijmoment-string 
        {
            current = current.Next;
            s += current.ToString(str + count.ToString());
            count++;
        }
        return (count,s); // door count weet bus.tostring bij welk getal dit rijmoment is gebleven 
                          // (het hoeveelste bedrijf dat door deze bus op deze dag wordt bezocht)
    }
}
