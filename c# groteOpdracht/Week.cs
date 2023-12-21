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

        foreach (Bedrijf bedrijf in Setup.bedrijven) // reset alle bedrijven, voeg strafkosten toe
        {
            if (bedrijf.orderNummer != 8942) // zet het bedrijf met negatieve leegtijd niet in de bedrijvenNiet lijst,
                                             // zodat het bij simulated annealing niet geinsert kan worden
                bedrijvenNiet.Add(bedrijf);
            kosten += 3 * bedrijf.frequentie * bedrijf.ledigingsDuur;
            bedrijf.ResetNodes();
            bedrijf.wordtBezocht = false;
        }
    }

    public (bool, int[]) InsertCheck(Bedrijf b, Node[] nodes) // controleer of de nodes van dit bedrijf vóór deze nodes geinsert mogen worden
                                                              // en geef dan de incrementele kosten
    {
        for (int i = 0; i < nodes.Length; i++) // als de dagen van 2 nodes overeen komen mag het niet
            for (int j = i + 1; j < nodes.Length; j++)
                if (nodes[i].rijmoment.bus.dag == nodes[j].rijmoment.bus.dag)
                    return (false, null);

        if (!InsertFreqCheck(nodes)) // als de freqcheck is gefaald mag het ook niet
            return (false, null);

        foreach (Node node in nodes) // als het niet in de bus past mag het ook niet
            if (node.rijmoment.volume + b.volume > 100000)
                return (false, null);

        int[] extratijd = new int[nodes.Length];
        int extraTijd;
        Node volgende;
        for (int i = 0; i < nodes.Length; i++) // als het qua tijd niet in de bus past, mag het ook niet. 
                                               // Anders, voeg de incrementele kosten toe aan de incrementele kosten-array
        {
            volgende = nodes[i];
            extraTijd = volgende.rijmoment.ExtraTijdskostenBijToevoegen(b, volgende.Previous, volgende);
            if (volgende.rijmoment.bus.tijd + extraTijd > 43200 * 1000)
                return (false, null);
            extratijd[i] = extraTijd;
        }
        return (true, extratijd);
    }

    public void Insert(Bedrijf b, int[] extratijd, Node[] nodes) // insert de nodes van het bedrijf voor de gegeven nodes
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

    public (bool, int[]) DeleteCheck(Bedrijf b) // Controleer of een bedrijf verwijderd mag worden uit de oplossing
                                                // en geef dan de incrementele kosten
    {
        int[] extratijd = new int[b.Locaties.Count];
        Node n;
        int extraTijd;
        for (int i = 0; i < b.Locaties.Count; i++) // als het verwijderen van een node qua tijd niet past (de kans is klein, heel klein),
                                                   // mag het niet. Anders, voeg de incrementele kosten toe aan de incrementele kosten-array
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

    public void Delete(Bedrijf b, int[] extratijd) // Verwijder het gegeven bedrijf uit de oplossing
    {
        Node n;
        for (int i = 0; i < b.Locaties.Count; i++)
        {
            n = b.Locaties[i];
            n.Verwijder(extratijd[i]);
        }

        b.wordtBezocht = false;
        kosten += 3 * b.frequentie * b.ledigingsDuur;
        bedrijvenNiet.Add(b);
        bedrijvenWel.Remove(b);
    }

    public (bool, int, int) VerplaatsCheck(Node mover, Node hierVoor) // Controleer of node mover naar vóór node hierVoor verplaatst mag worden,
                                                                      // en geef dan de incrementele kosten
    {
        bool legaal;
        int extratijd1;
        int extratijd2;
        if (mover.rijmoment == hierVoor.rijmoment) // Als de nodes in hetzelfde rijmoment zitten
        {
            extratijd1 = mover.ExtraTijdskostenBijVerwijderen();
            Node hierNa = hierVoor.Previous == mover ? mover.Previous : hierVoor.Previous; // Na welke node moet mover ingevoegd worden?
            extratijd2 = hierVoor.rijmoment.ExtraTijdskostenBijToevoegen(mover.bedrijf, hierNa, hierVoor);

            if (mover.rijmoment.bus.tijd + extratijd1 + extratijd2 > 43200 * 1000) return (false, 0, 0); // als het qua tijd niet past
            return (true, extratijd1, extratijd2);
        }

        // als het programma hier komt zitten ze in een ander rijmoment

        if (hierVoor.rijmoment.volume + mover.bedrijf.volume > 100000) // als het qua volume niet past
            return (false, 0, 0);

        extratijd1 = mover.ExtraTijdskostenBijVerwijderen();
        extratijd2 = hierVoor.rijmoment.ExtraTijdskostenBijToevoegen(mover.bedrijf, hierVoor.Previous, hierVoor);
        // extratijd1 is de incrementele kosten in het rijmoment van mover
        // extratijd2 is de incrementele kosten in het rijmoment van hierVoor

        if (hierVoor.rijmoment.bus == mover.rijmoment.bus) // als de bus hetzelfde is maar een ander rijmoment
            legaal = hierVoor.rijmoment.bus.InterRijmomentSwapCheck(extratijd1 + extratijd2); //Controleer of het mag
        else if (hierVoor.rijmoment.bus.dag == mover.rijmoment.bus.dag) // als de bus anders is maar dezelfde dag
            legaal = Dag.InterBusSwapCheck(mover, hierVoor, extratijd1, extratijd2); //Controleer of het mag
        else legaal = InterDagVerplaatsCheck(mover, hierVoor, extratijd1, extratijd2); // als de dag anders is, controleer of het mag

        return (legaal, extratijd1, extratijd2); 
    }

    public void Verplaats(Node mover, Node hierVoor, int extratijd1, int extratijd2) // Verplaats node mover naar vóór node hierVoor
    {
        mover.Verwijder(extratijd1);
        hierVoor.rijmoment.ToevoegenVoor(mover, hierVoor, extratijd2);
    }

    public (bool, int, int) SwapCheck(Node node1, Node node2) // Controleer of node1 en node2 omgewisseld mogen worden in de oplossing
                                                              // en geef dan de incrementele kosten
    {
        bool legaal;
        int extratijd1;
        int extratijd2;

        if (node1.rijmoment == node2.rijmoment) // als de rijmomenten hetzelfde zijn
        {
            extratijd1 = node1.rijmoment.ExtraTijdsKostenBijWisselen(node1, node2);
            if (node1.rijmoment.bus.tijd + extratijd1 > 43200 * 1000) // als het qua tijd niet past
                return (false, 0, 0);
            return (true, extratijd1, 0);
        }

        // als het programma hier komt zitten ze in een ander rijmoment

        if (node1.rijmoment.volume - node1.bedrijf.volume + node2.bedrijf.volume > 100000 || // als het qua volume niet past
            node2.rijmoment.volume - node2.bedrijf.volume + node1.bedrijf.volume > 100000)
            return (false, 0, 0);

        extratijd1 = node1.ExtraTijdskostenBijVerwijderen();
        extratijd1 += node1.rijmoment.ExtraTijdskostenBijToevoegen(node2.bedrijf, node1.Previous, node1.Next);
        // extratijd1 is de incrementele kosten in het rijmoment van node1

        extratijd2 = node2.ExtraTijdskostenBijVerwijderen();
        extratijd2 += node2.rijmoment.ExtraTijdskostenBijToevoegen(node1.bedrijf, node2.Previous, node2.Next);
        // extratijd2 is de incrementele kosten in het rijmoment van node2

        if (node1.rijmoment.bus == node2.rijmoment.bus) // als de bus hetzelfde is maar een ander rijmoment
            legaal = node1.rijmoment.bus.InterRijmomentSwapCheck(extratijd1 + extratijd2); //Controleer of het mag
        else if (node1.rijmoment.bus.dag == node2.rijmoment.bus.dag) // als de dag hetzelfde is maar een andere bus
            legaal = Dag.InterBusSwapCheck(node1, node2, extratijd1, extratijd2); //Controleer of het mag
        else legaal = InterDagSwapCheck(node1, node2, extratijd1, extratijd2); // als ook de dag anders is, controleer of het mag

        return (legaal, extratijd1, extratijd2);
    }

    public void Swap(Node node1, Node node2, int extratijd1, int extratijd2) // Wissel node1 en node2 in de oplossing
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

    public bool InterDagSwapCheck(Node node1, Node node2, int extratijd1, int extratijd2) // Controleer of de nodes, die in verschillende dagen zitten,
                                                                                          // gegeven de incrementele kosten gewisseld mogen worden
    {
        // Controleer of het mag ivm de frequentie-eisen van de bedrijven van node1 en node2
        bool legaal = MoveFreqCheck(node1, node2.rijmoment.bus.dag) && MoveFreqCheck(node2, node1.rijmoment.bus.dag);
        if (!legaal) return false;

        // als het vorige mag, doe alsof ze binnen een bus worden geswapt. in principe zit daar dan geen verschil in
        return Dag.InterBusSwapCheck(node1, node2, extratijd1, extratijd2);
    }

    public bool InterDagVerplaatsCheck(Node mover, Node hierVoor, int extratijd1, int extratijd2) // Controleer of mover, die in een andere dag zit als hierVoor,
                                                                                                  // gegeven de incrementele kosten
                                                                                                  // naar vóór hierVoor verplaatst mag worden
    {
        // Controleer of het mag ivm de frequentie-eisen van de bedrijven van mover en hierVoor
        bool legaal = MoveFreqCheck(mover, hierVoor.rijmoment.bus.dag);
        if (!legaal) return false;

        // als het vorige mag, doe alsof ze binnen een bus worden geswapt. in principe zit daar dan geen verschil in
        return Dag.InterBusSwapCheck(mover, hierVoor, extratijd1, extratijd2);
    }

    public bool InsertFreqCheck(Node[] nodes) // Controleer of de dagen waarin de gegeven nodes (waar je de nodes van het bedrijf voor wilt voegen)
                                              // zich bevinden overeen komen met de frequentie-eisen van het bedrijf (bijv freq 3 moet op ma-wo-vr) 
    {
        // we nemen aan de de gegeven nodes allemaal in een verschillende dag zitten, dat is immers al gecontroleerd
        
        if (nodes.Length == 1 || nodes.Length == 4) // bij deze frequenties is elke mogelijkheid toegestaan
            return true;
        if (nodes.Length == 3) // bij frequentie 3 moet 1 node op maandag, 1 node op woensdag, 1 node op vrijdag
        {
            int getal0 = nodes[0].rijmoment.bus.dag.getal;
            int getal1 = nodes[1].rijmoment.bus.dag.getal;
            int getal2 = nodes[2].rijmoment.bus.dag.getal;
            if (getal0 + getal1 + getal2 == 9 &&
                (getal0 == 1 || getal1 == 1 || getal2 == 1))
                return true;
        }
        if (nodes.Length == 2) // bij frequentie 2, controleer of er wel drie dagen verschil in zit
        {
            if (Math.Abs(nodes[0].rijmoment.bus.dag.getal -
                        nodes[1].rijmoment.bus.dag.getal) == 3)
                return true;
        }
        return false;
    }

    public bool MoveFreqCheck(Node node, Dag nieuweDag) // Controleer of de gegeven node naar nieuweDag verplaatst mag worden,
                                                        // dus of dat wel of niet in strijd is met de frequentie-eisen van het bedrijf van de node
    {
        // We nemen aan de node zich niet al in nieuweDag bevindt, dat is immers al gecontroleerd
        
        if (node.bedrijf.frequentie == 3) // bij frequentie 3 mag een node nooit naar een andere dag verplaatst worden
            return false;
        if (node.bedrijf.frequentie == 1) // bij frequentie 1 mag het altijd
            return true;

        foreach (Node n in node.bedrijf.Locaties) // voor freq 2,4: als er al een node van het bedrijf in nieuweDag zit, mag het niet
            if (n.rijmoment.bus.dag == nieuweDag) 
                return false;

        if (node.bedrijf.frequentie == 2) // Controleer of er drie dagen verschil zit tussen nieuweDag en de dag van de andere node van het bedrijf
        {
            Node node2 = node.bedrijf.Locaties[0] == node ? node.bedrijf.Locaties[1] : node.bedrijf.Locaties[0];
            if (Math.Abs(nieuweDag.getal - node2.rijmoment.bus.dag.getal) == 3)
            {
                return true;
            }
            return false;
        }

        return true; // dan is het freq 4 en mag het, want er zat nog geen node van het bedrijf in de nieuweDag
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

