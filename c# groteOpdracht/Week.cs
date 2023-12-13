namespace rommelrouterakkers;

using System;
using System.Collections.Generic;
using System.Linq;

public class Week
{
    //index from 1 to 5, not 0 to 4
    public Dag[] dagen = new Dag[6];
    public int kosten = 0;
    public int tijd = 0;

    public Week()
    {
        for (int i = 1; i <= 5; i++)
            dagen[i] = new Dag(this, i);
    }

    public void Pick(Bedrijf b, Random r)
    {
        bool gelukt;
        if (b.wordtBezocht)
        {
            gelukt = Delete(b);
        }
        else
        {
            gelukt = Insert(b, r);
        }

        if (gelukt)
        {
            b.wordtBezocht = !b.wordtBezocht;
        }
        // als een insert of delete niet lukt wordt ie geskipt, er wordt niet gezocht naar een andere mogelijkheid
    }
    public bool Delete(Bedrijf b)

    {
        int[] extratijd = new int[b.Locaties.Count];
        Node n;
        int extraTijd;
        for (int i = 0; i < b.Locaties.Count; i++)
        {
            n = b.Locaties[i];
            extraTijd = n.ExtraTijdskostenBijVerwijderen();
            if (n.rijmoment.bus.tijd + extraTijd > 43200)
                return false;
            extratijd[i] = extraTijd;
        }

        for (int i = 0; i < b.Locaties.Count; i++)
        {
            n = b.Locaties[i];
            n.Verwijder(extratijd[i]);
        }

        b.wordtBezocht = false;
        kosten += 3 * b.frequentie * b.ledigingsDuur;
        return true;
    }


    public bool Swap(Node node1, Node node2)
    {
        if (node1.bedrijf == node2.bedrijf) return false;

        bool gelukt;
        int extratijd1;
        int extratijd2;
        if (node1.rijmoment == node2.rijmoment)
        {
            extratijd1 = node1.rijmoment.ExtraTijdsKostenBijWisselen(node1, node2);
            if (node1.rijmoment.bus.tijd + extratijd1 > 43200) return false;
            node1.rijmoment.Wisselen(node1, node2, extratijd1);
            return true;
        }

        extratijd1 = node1.ExtraTijdskostenBijVerwijderen();
        extratijd1 += node1.rijmoment.ExtraTijdskostenBijToevoegen(node2.bedrijf, node1.Previous, node1.Next);

        extratijd2 = node2.ExtraTijdskostenBijVerwijderen();
        extratijd2 += node2.rijmoment.ExtraTijdskostenBijToevoegen(node1.bedrijf, node2.Previous, node2.Next);

        Node node1n = node1.Next;
        Node node2n = node2.Next;

        if (node1.rijmoment.bus == node2.rijmoment.bus)
            gelukt = node1.rijmoment.bus.InterRijmomentSwapCheck(node1, node2, extratijd1, extratijd2);
        else if (node1.rijmoment.bus.dag == node2.rijmoment.bus.dag)
            gelukt = Dag.InterBusSwapCheck(node1, node2, extratijd1, extratijd2);
        else gelukt = InterDagSwapCheck(node1, node2, extratijd1, extratijd2);

        if (!gelukt) return false;

        node1.Verwijder(0); 
        node2.Verwijder(0); 

        node1n.rijmoment.ToevoegenVoor(node2, node1n, extratijd1);
        node2n.rijmoment.ToevoegenVoor(node1, node2n, extratijd2);

        return true;
    }

    public bool FreqCheck(Node node, Dag nieuweDag)
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
                return true;
            return false;
        }

        return true; // dan is het freq 4
    }

    public bool InterDagSwapCheck(Node node1, Node node2, int extratijd1, int extratijd2)
    {
        bool gelukt = FreqCheck(node1, node2.rijmoment.bus.dag) && FreqCheck(node2, node1.rijmoment.bus.dag);
        if (!gelukt) return false;

        return Dag.InterBusSwapCheck(node1, node2, extratijd1, extratijd2);
    }


    public bool Insert(Bedrijf b, Random r)
    {
        int bustijd;
        switch (b.frequentie)
        {
            case 1: bustijd = AddDag1(b, r); break;
            case 2: bustijd = AddDag2(b, r); break;
            case 3: bustijd = AddDag3(b, r); break;
            case 4: bustijd = AddDag4(b, r); break;
            default: bustijd = 50000; break;
        }

        if (bustijd > 43200)
        {
            foreach (Node node in b.Locaties)
                if (node.Next != null && node.Next.Previous == node) // als ie uberhaupt net is toegevoegd en niet geblocked omdat de bus leeg was qua rijmomenten
                    node.Verwijder(node.ExtraTijdskostenBijVerwijderen());
            return false;
        }
        kosten -= 3 * b.frequentie * b.ledigingsDuur;
        b.wordtBezocht = true;
        return true;
    }
    public int AddDag1(Bedrijf b, Random r)
    {
        int dag = r.Next(1, 6);

        Node node = b.Locaties[0];
        int bustijd = dagen[dag].Insert(node, r);
        return bustijd;

    }
    public int AddDag2(Bedrijf b, Random r)
    {
        int dag1, dag2;
        int welke = r.Next(0, 2);

        if (welke == 0)
        {
            dag1 = 1;
            dag2 = 4;
        }
        else
        {
            dag1 = 2;
            dag2 = 5;
        }
        Node node1 = b.Locaties[0];
        Node node2 = b.Locaties[1];
        int bustijd = dagen[dag1].Insert(node1, r);
        bustijd = Math.Max(bustijd, dagen[dag2].Insert(node2, r));
        return bustijd;
    }
    public int AddDag3(Bedrijf b, Random r)
    {
        int bustijd = dagen[1].Insert(b.Locaties[0], r);
        bustijd = Math.Max(bustijd, dagen[3].Insert(b.Locaties[1], r));
        bustijd = Math.Max(bustijd, dagen[5].Insert(b.Locaties[2], r));
        return bustijd;
    }
    public int AddDag4(Bedrijf b, Random r)
    {
        int dag = r.Next(1, 6);
        int j = 0;
        int[] bustijd = new int[4];
        for (int i = 1; i < 6; i++)
        {
            if (dag != i)
            {
                bustijd[j] = dagen[i].Insert(b.Locaties[j], r);
                j++;
            }
        }
        return bustijd.Max();
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
            return kosten / 60;  // jo kan je ff die error fixen van
        } 
    }
    
    public int Evaluate(List<Bedrijf> b)
    {
        int t = 0;
        for (int i = 1; i <= 5; i++)
        {
            t += dagen[i].tijd;
        } 
        
        t += StrafKosten(b); 
        
        
        tijd = t; 
        return t / 60;  
    }

    public int StrafKosten(List<Bedrijf> bedrijven)
    {
        int k = 0;
        foreach (Bedrijf b in bedrijven)
        {
            if (!b.wordtBezocht)
            {
                k += 3 * b.frequentie * b.ledigingsDuur; 
            }
        }
        return k;
    }
} 

