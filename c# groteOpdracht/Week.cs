namespace rommelrouterakkers;

using System;
using System.Collections.Generic;

public class Week
{
    //index from 1 to 5, not 0 to 4
    public Dag[] dagen = new Dag[6];
    public int kosten = 0;
    public int tijd = 0;
    public List<Bedrijf> bedrijvenWel = new List<Bedrijf>();
    public List<Bedrijf> bedrijvenNiet = new List<Bedrijf>();
    public Week()
    {
        for (int i = 1; i <= 5; i++)
            dagen[i] = new Dag(this, i);

        foreach (Bedrijf bedrijf in Setup.bedrijven)
        {
            if (bedrijf.orderNummer != 8942) 
                bedrijvenNiet.Add(bedrijf);
            kosten += 3 * bedrijf.frequentie * bedrijf.ledigingsDuur;
            bedrijf.ResetNodes();
            bedrijf.wordtBezocht = false;
        }
    }

    public (bool, int[]) InsertCheck(Bedrijf b, Node[] nodes)
    {
        for (int i = 0; i < nodes.Length; i++)
            for (int j = i + 1; j < nodes.Length; j++)
                if (nodes[i].rijmoment.bus.dag == nodes[j].rijmoment.bus.dag)
                    return (false, null);

        if (!InsertFreqCheck(nodes))
            return (false, null);

        foreach (Node node in nodes)
            if (node.rijmoment.volume + b.volume > 100000)
                return (false, null);

        int[] extratijd = new int[nodes.Length];
        int extraTijd;
        Node volgende;
        for (int i = 0; i < nodes.Length; i++)
        {
            volgende = nodes[i];
            extraTijd = volgende.rijmoment.ExtraTijdskostenBijToevoegen(b, volgende.Previous, volgende);
            if (volgende.rijmoment.bus.tijd + extraTijd > 43200 * 1000)
                return (false, null);
            extratijd[i] = extraTijd;
        }
        return (true, extratijd);
    }

    public void Insert(Bedrijf b, int[] extratijd, Node[] nodes)
    {
        Node n;
        Node volgende;
        for (int i = 0; i < nodes.Length; i++)
        {
            n = b.Locaties[i];
            volgende = nodes[i];
            volgende.rijmoment.ToevoegenVoor(n, volgende, extratijd[i]);
        }
        b.wordtBezocht = true;
        kosten -= 3 * b.frequentie * b.ledigingsDuur;
        bedrijvenWel.Add(b);
        bedrijvenNiet.Remove(b);
    }

    public (bool, int[]) DeleteCheck(Bedrijf b)
    {
        int[] extratijd = new int[b.Locaties.Count];
        Node n;
        int extraTijd;
        for (int i = 0; i < b.Locaties.Count; i++)
        {
            n = b.Locaties[i];
            extraTijd = n.ExtraTijdskostenBijVerwijderen();
            if (n.rijmoment.bus.tijd + extraTijd > 43200 * 1000)
            {
                return (false, null);
            }
            extratijd[i] = extraTijd;
        }

        return (true, extratijd);
    }

    public void Delete(Bedrijf b, int[] extratijd)
    {
        Node n;
        for (int i = 0; i < b.Locaties.Count; i++)
        {
            n = b.Locaties[i];
            n.Verwijder(extratijd[i]);
        }
        // hier hoeven de nodes niet gereset te worden

        b.wordtBezocht = false;
        kosten += 3 * b.frequentie * b.ledigingsDuur;
        bedrijvenNiet.Add(b);
        bedrijvenWel.Remove(b);
    }

    public (bool, int, int) VerplaatsCheck(Node mover, Node hierVoor) // kan dus niet in leeg rijmoment plaatsen, maar boeie daar zorgt insert wel weer voor
    {
        bool gelukt;
        int extratijd1;
        int extratijd2;
        if (mover.rijmoment == hierVoor.rijmoment)
        {
            extratijd1 = mover.ExtraTijdskostenBijVerwijderen();
            Node hierNa = hierVoor.Previous == mover ? mover.Previous : hierVoor.Previous;
            extratijd2 = hierVoor.rijmoment.ExtraTijdskostenBijToevoegen(mover.bedrijf, hierNa, hierVoor);

            if (mover.rijmoment.bus.tijd + extratijd1 + extratijd2 > 43200 * 1000) return (false, 0, 0);
            return (true, extratijd1, extratijd2);
        }

        if (hierVoor.rijmoment.volume + mover.bedrijf.volume > 100000)
            return (false, 0, 0);

        extratijd1 = mover.ExtraTijdskostenBijVerwijderen();
        extratijd2 = hierVoor.rijmoment.ExtraTijdskostenBijToevoegen(mover.bedrijf, hierVoor.Previous, hierVoor);

        if (hierVoor.rijmoment.bus == mover.rijmoment.bus)
            gelukt = hierVoor.rijmoment.bus.InterRijmomentSwapCheck(extratijd1 + extratijd2);
        else if (hierVoor.rijmoment.bus.dag == mover.rijmoment.bus.dag)
            gelukt = Dag.InterBusSwapCheck(mover, hierVoor, extratijd1, extratijd2);
        else gelukt = InterDagVerplaatsCheck(mover, hierVoor, extratijd1, extratijd2);

        return (gelukt, extratijd1, extratijd2);
    }

    public void Verplaats(Node mover, Node hierVoor, int extratijd1, int extratijd2)
    {
        mover.Verwijder(extratijd1);
        hierVoor.rijmoment.ToevoegenVoor(mover, hierVoor, extratijd2);
    }

    public (bool, int, int) SwapCheck(Node node1, Node node2) 
    {
        bool gelukt;
        int extratijd1;
        int extratijd2;

        if (node1.rijmoment == node2.rijmoment)
        {
            extratijd1 = node1.rijmoment.ExtraTijdsKostenBijWisselen(node1, node2);
            if (node1.rijmoment.bus.tijd + extratijd1 > 43200 * 1000) 
                return (false, 0, 0);
            return (true, extratijd1, 0);
        }

        if (node1.rijmoment.volume - node1.bedrijf.volume + node2.bedrijf.volume > 100000 ||
            node2.rijmoment.volume - node2.bedrijf.volume + node1.bedrijf.volume > 100000)
            return (false, 0, 0);

        extratijd1 = node1.ExtraTijdskostenBijVerwijderen();
        extratijd1 += node1.rijmoment.ExtraTijdskostenBijToevoegen(node2.bedrijf, node1.Previous, node1.Next);

        extratijd2 = node2.ExtraTijdskostenBijVerwijderen();
        extratijd2 += node2.rijmoment.ExtraTijdskostenBijToevoegen(node1.bedrijf, node2.Previous, node2.Next);

        if (node1.rijmoment.bus == node2.rijmoment.bus)
            gelukt = node1.rijmoment.bus.InterRijmomentSwapCheck(extratijd1 + extratijd2);
        else if (node1.rijmoment.bus.dag == node2.rijmoment.bus.dag)
            gelukt = Dag.InterBusSwapCheck(node1, node2, extratijd1, extratijd2);
        else gelukt = InterDagSwapCheck(node1, node2, extratijd1, extratijd2);

        return (gelukt, extratijd1, extratijd2);
    }

    public void Swap(Node node1, Node node2, int extratijd1, int extratijd2)
    {
        if (node1.rijmoment == node2.rijmoment)
            node1.rijmoment.Wisselen(node1, node2, extratijd1);
        else
        {
            Node node1n = node1.Next;
            Node node2n = node2.Next;

            node1.Verwijder(0);
            node2.Verwijder(0);

            node1n.rijmoment.ToevoegenVoor(node2, node1n, extratijd1);
            node2n.rijmoment.ToevoegenVoor(node1, node2n, extratijd2);
        }
    }

    public bool InterDagSwapCheck(Node node1, Node node2, int extratijd1, int extratijd2)
    {
        bool gelukt = MoveFreqCheck(node1, node2.rijmoment.bus.dag) && MoveFreqCheck(node2, node1.rijmoment.bus.dag);
        if (!gelukt) return false;

        return Dag.InterBusSwapCheck(node1, node2, extratijd1, extratijd2);
    }

    public bool InterDagVerplaatsCheck(Node mover, Node hierVoor, int extratijd1, int extratijd2)
    {
        bool gelukt = MoveFreqCheck(mover, hierVoor.rijmoment.bus.dag);
        if (!gelukt) return false;

        return Dag.InterBusSwapCheck(mover, hierVoor, extratijd1, extratijd2);
    }

    public bool InsertFreqCheck(Node[] nodes)
    {
        if (nodes.Length == 1 || nodes.Length == 4)
            return true;
        if (nodes.Length == 3)
        {
            int getal0 = nodes[0].rijmoment.bus.dag.getal;
            int getal1 = nodes[1].rijmoment.bus.dag.getal;
            int getal2 = nodes[2].rijmoment.bus.dag.getal;
            if (getal0 + getal1 + getal2 == 9 &&
                (getal0 == 1 || getal1 == 1 || getal2 == 1))
                return true;
        }
        if (nodes.Length == 2)
        {
            if (Math.Abs(nodes[0].rijmoment.bus.dag.getal -
                        nodes[1].rijmoment.bus.dag.getal) == 3)
                return true;
        }
        return false;
    }

    public bool MoveFreqCheck(Node node, Dag nieuweDag)
    {
        if (node.bedrijf.frequentie == 3)
            return false;
        if (node.bedrijf.frequentie == 1)
            return true;

        foreach (Node n in node.bedrijf.Locaties) // voor freq 2,3,4
            if (n.rijmoment.bus.dag == nieuweDag)
                return false;

        if (node.bedrijf.frequentie == 2)
        {
            Node node2 = node.bedrijf.Locaties[0] == node ? node.bedrijf.Locaties[1] : node.bedrijf.Locaties[0];
            if (Math.Abs(nieuweDag.getal - node2.rijmoment.bus.dag.getal) == 3)
            {
                return true;
            }
            return false;
        }

        return true; // dan is het freq 4 en mag het
    }

    public override string ToString()
    {
        string s = "";
        for (int i = 1; i <= 5; i++)
        {
            s += dagen[i].ToString(i.ToString());
        }
        return s;
    }

    public void BFS()
    {
        for (int i = 1; i <= 5; i++)
        {
            dagen[i].BFS();
        }
    }

    public void Load(int dag, int bus, Bedrijf b, bool stortIngelezen)
    {
        dagen[dag].bussen[bus].Load(b, stortIngelezen);
    }

    public int Eval { get
        {
            return kosten / 60000;  
        } 
    }
} 

