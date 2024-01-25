namespace rommelrouterakkers;

using System;
using System.Xml.Linq;

public class Week
{
    //index from 1 to 5, not 0 to 4
    public Dag[] dagen = new Dag[6];
    public int kosten = 0;
    public int tijd = 0;
    public EigenArray<Bedrijf> bedrijvenWel = new EigenArray<Bedrijf>();
    public EigenArray<Bedrijf> bedrijvenNiet = new EigenArray<Bedrijf>();
    public int totaalStrafVolume = 0; // hoeveelheid volume waarmee het volume-constraint in totaal wordt geschonden (som van alle rijmomenten)

    public Week()
    {
        for (int i = 1; i <= 5; i++)
            dagen[i] = new Dag(this, i);

        foreach (Bedrijf bedrijf in Setup.bedrijven) // reset alle bedrijven, voeg strafkosten toe
        {
            if (bedrijf.orderNummer != 8942) // zet het bedrijf met negatieve leegtijd niet in de bedrijvenNiet lijst,
                                             // zodat het bij simulated annealing niet geinsert kan worden
                bedrijvenNiet.Add(bedrijf);
            kosten += bedrijf.strafkosten;
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

    public int InsertStrafVolumeBerekenen(Bedrijf bedrijf, Node[] nodes) // Bereken het extra volume waarmee het volume-constraint geschonden wordt door deze actie
    {
        int extraStrafVolume = 0;
        foreach (Node volgende in nodes) // voor elke node waar we het bedrijf vóór willen inserten
        {
            if (volgende.rijmoment.volume + bedrijf.volume > 100_000)
                if (volgende.rijmoment.volume > 100_000)
                    extraStrafVolume += bedrijf.volume;
                else
                    extraStrafVolume += volgende.rijmoment.volume + bedrijf.volume - 100_000;
        }
        return extraStrafVolume;
    }

    public void Insert(Bedrijf b, int index, int[] extratijd, int extraStrafVolume, int overschrijdingsKosten, Node[] nodes) // insert de nodes van het bedrijf voor de gegeven nodes
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
        kosten -= b.strafkosten;
        bedrijvenWel.Add(b);
        bedrijvenNiet.RemoveAt(index);

        totaalStrafVolume += extraStrafVolume;
        kosten += overschrijdingsKosten;
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

    public int DeleteStrafVolumeBerekenen(Bedrijf bedrijf) // bereken het extra volume waarmee het volume-constraint wordt geschonden door deze actie
    {
        int extraStrafVolume = 0;
        foreach (Node n in bedrijf.Locaties)
        {
            if (n.rijmoment.volume > 100_000)
                if (n.rijmoment.volume - bedrijf.volume > 100_000)
                    extraStrafVolume -= bedrijf.volume;
                else
                    extraStrafVolume -= n.rijmoment.volume - 100_000;
        }
        return extraStrafVolume;
    }

    public void Delete(Bedrijf b, int index, int[] extratijd, int extraStrafVolume, int overschrijdingsKosten) // Verwijder het gegeven bedrijf uit de oplossing
    {
        Node n;
        for (int i = 0; i < b.Locaties.Count; i++)
        {
            n = b.Locaties[i];
            n.Verwijder(extratijd[i]);
        }

        b.wordtBezocht = false;
        kosten += b.strafkosten;
        bedrijvenNiet.Add(b);
        bedrijvenWel.RemoveAt(index);

        totaalStrafVolume += extraStrafVolume;
        kosten += overschrijdingsKosten;
    }

    public (bool, int, int) VerplaatsCheck(Node mover, Node hierVoor) // Controleer of node mover naar vóór node hierVoor verplaatst mag worden,
                                                                      // en geef dan de incrementele kosten
    {
        bool legaal;
        int extratijd1;
        int extratijd2;

        if (mover.rijmoment == hierVoor.rijmoment) // Als de nodes in hetzelfde rijmoment zitten
        {
            if (mover.Next == hierVoor) // als mover al vóór hierVoor staat, hoeft het niet natuurlijk
                return (false, 0, 0);
            
            extratijd1 = mover.ExtraTijdskostenBijVerwijderen(); 
            extratijd2 = hierVoor.rijmoment.ExtraTijdskostenBijToevoegen(mover.bedrijf, hierVoor.Previous, hierVoor);

            if (mover.rijmoment.bus.tijd + extratijd1 + extratijd2 > 43200 * 1000) return (false, 0, 0); // als het qua tijd niet past
            return (true, extratijd1, extratijd2);
        }

        // als het programma hier komt zitten ze in een ander rijmoment

        extratijd1 = mover.ExtraTijdskostenBijVerwijderen();
        extratijd2 = hierVoor.rijmoment.ExtraTijdskostenBijToevoegen(mover.bedrijf, hierVoor.Previous, hierVoor);
        // extratijd1 is de incrementele kosten in het rijmoment van mover
        // extratijd2 is de incrementele kosten in het rijmoment van hierVoor

        if (hierVoor.rijmoment.bus == mover.rijmoment.bus) // als de bus hetzelfde is maar een ander rijmoment
            legaal = hierVoor.rijmoment.bus.tijd + extratijd1 + extratijd2 <= 43200 * 1000; //Controleer of het mag
        else if (hierVoor.rijmoment.bus.dag == mover.rijmoment.bus.dag) // als de bus anders is maar dezelfde dag
            legaal = Dag.InterBusVerplaatsCheck(mover, hierVoor, extratijd1, extratijd2); //Controleer of het mag
        else legaal = InterDagVerplaatsCheck(mover, hierVoor, extratijd1, extratijd2); // als de dag anders is, controleer of het mag

        return (legaal, extratijd1, extratijd2); 
    }

    public bool InterDagVerplaatsCheck(Node mover, Node hierVoor, int extratijd1, int extratijd2) // Controleer of mover, die in een andere dag zit als hierVoor,
                                                                                                  // gegeven de incrementele kosten
                                                                                                  // naar vóór hierVoor verplaatst mag worden
    {
        // Controleer of het mag ivm de frequentie-eisen van de bedrijven van mover en hierVoor
        bool legaal = VerplaatsFreqCheck(mover, hierVoor.rijmoment.bus.dag);
        if (!legaal) return false;

        // als het vorige mag, doe alsof ze binnen een dag worden geswapt. in principe zit daar dan geen verschil in
        return Dag.InterBusVerplaatsCheck(mover, hierVoor, extratijd1, extratijd2);
    }

    public bool VerplaatsFreqCheck(Node node, Dag nieuweDag) // Controleer of de gegeven node naar nieuweDag verplaatst mag worden,
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

    public int VerplaatsStrafVolumeBerekenen(Node mover, Node hierVoor) //bereken het extra volume waarmee het volume-constraint geschonden gaat worden door deze actie
    {
        int extraStrafVolume = 0;
        if (mover.rijmoment != hierVoor.rijmoment)
        {
            if (mover.rijmoment.volume > 100_000) 
                if (mover.rijmoment.volume - mover.bedrijf.volume > 100_000)
                    extraStrafVolume -= mover.bedrijf.volume;
                else
                    extraStrafVolume -= mover.rijmoment.volume - 100_000;
            if (hierVoor.rijmoment.volume + mover.bedrijf.volume > 100_000)
                if (hierVoor.rijmoment.volume > 100_000)
                    extraStrafVolume += mover.bedrijf.volume;
                else
                    extraStrafVolume += hierVoor.rijmoment.volume + mover.bedrijf.volume - 100_000;
        }
        return extraStrafVolume;
    }

    public void Verplaats(Node mover, Node hierVoor, int extratijd1, int extratijd2, int extraStrafVolume, int overschrijdingsKosten) // Verplaats node mover naar vóór node hierVoor
    {
        mover.Verwijder(extratijd1);
        hierVoor.rijmoment.ToevoegenVoor(mover, hierVoor, extratijd2);

        kosten += overschrijdingsKosten;
        totaalStrafVolume += extraStrafVolume;
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

    public int Kosten
    {
        get { return kosten; }
    }
}