using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace rommelrouterakkers;

public class ZoekAlgoritme
{
    private Week week;
    private int bestOplossing;
    private Stopwatch timer;
    private Timer timer2;
    public Random r;
    private double tempVerkleining = 0.99;
    private int totItt = 0;
    private int totIttTemp = 0; // om iteraties van 0.5 sec terug bij te houden voor it/sec berekening
    private int besteScoreTemp;
    private int sweeps = 0;
    private double Temp;
    private int strafkostenCoefficient;
    private int startTemp = 20000;

    public ZoekAlgoritme(Week w)
    {
        week = w;
        timer = new Stopwatch(); // voor de elapsed time
        r = new Random();
        bestOplossing = w.Kosten;
        besteScoreTemp = w.Kosten;

        //timer 2 is voor het berekenen van de iteraties per seconde
        timer2 = new Timer();
        timer2.Interval = 500;
        timer2.Elapsed += OnTimedEvent;
        timer2.AutoReset = true;
    }

    public void OnTimedEvent(object o, ElapsedEventArgs eea)
    {
        if (bestOplossing < besteScoreTemp)
            Console.ForegroundColor = ConsoleColor.Green;
        PrintVoortgang();
        Console.ResetColor();
        totIttTemp = totItt; 
        besteScoreTemp = bestOplossing;
    }

    public void BFS()
    { //bfs staat voor best first search en zorgt ervoor dat elk rijmoment optimaal wordt voor hoe het nu ingedeeld is
        week.BFS();
    }

    public void ChangeBest()
    {
        if (week.Kosten / 60000 < 5600)
            IO.CreateFile(week, IO._scoreMap);
        bestOplossing = week.Kosten;
        sweeps = 0; // resetten zodat ie bijv niet randomwalks gaat doen terwijl we steeds beste oplossingen aan het vinden zijn
    }
    public void PrintVoortgang()
    {
        Console.Clear();
        string s =        $"Beste oplossingsscore:     {(float)bestOplossing / 60000}       \n" +
                          $"Huidige score:             {(float)week.Kosten / 60000}         \n" +
                          $"Totale iteraties:          {totItt:n0}                          \n" +
                          $"Iteraties per seconde:     {2 * (totItt - totIttTemp):n0}       \n" +
                          $"Sweeps sinds beste score:  {sweeps}                             \n" +
                          $"Totale tijd:               {timer.Elapsed}                      \n" +
                          $"Huidige temperatuur:       {(int)Temp}                          \n" +
                          $"Totaal strafvolume:        {week.totaalStrafVolume}             ";
        if (week.totaalStrafVolume == 0)
            s += "\nDruk op Q om een screenshot te maken (de huidige oplossing op te slaan)";
        Console.WriteLine(s);
    }

    public void StartILS()
    {
        Task keyHandlerTask = Task.Run(() => HandleKeyEvents());

        timer.Start();
        timer2.Enabled = true;

        ILS();
    }

    public void ILS() //iterated local search
    {
        Temp = startTemp;
        //reset temp, start simulated annealing
        SimAnn();

        //if it goes out of the simulated annealing that means that there have been a lot of iterations, so something has to change.
        //that is what sweeps is for.
        sweeps++;

        //random walk
        if (sweeps % 50 == 0)
        {
            RandomWalk();
        }

        //reset
        else if (sweeps % 10 == 0)
        {
            //load de beste file tot nu toe, we kunnen later ook met gewoon lege week doen maar dit is goed voor nu
            week = IO.LoadSolutionAuto(true, r); 
            bestOplossing = week.kosten;
            besteScoreTemp = week.Kosten;
        }

        //infinite loop
        ILS();
    }

    public void SimAnn() // simulated annealing
    {
        while (Temp >= startTemp * 0.01)//T >= Modulo) 
        {
            PickAction(Temp); // doe een actie

            if (week.Kosten < bestOplossing) // als een betere oplossing is gevonden
            {
                if (week.totaalStrafVolume == 0) // als het een toegelaten oplossing is
                    ChangeBest();
            }

            totItt++;

            if (totItt % 300_000 == 0)
            {
                Temp *= tempVerkleining; // verlaag de temperatuur

                week.kosten -= OverschrijdingsKosten(week.totaalStrafVolume);
                strafkostenCoefficient = Math.Min(10000, (int) (startTemp / Temp * startTemp / Temp) - 1); 
                week.kosten += OverschrijdingsKosten(week.totaalStrafVolume);
            }
        }
    }

    public void PickAction(double T)
    {
        
        int welk = r.Next(0, 10); // 2/8, 1/8, 3/8, 4/8 is dus de verdeling
        if (welk <= 1)
            Insert(T);
        else if (welk <= 2)
            Delete(T);
        if (welk <= 5)
            Verplaats(T, true);
        else
            Verplaats(T, false);
    }

    public void Insert(double T)
    {
        // als alle bedrijven al bezocht worden, return
        if (week.bedrijvenNiet.Count == 0) return;

        Bedrijf bedrijf;
        bool bo;
        int[] extratijd;
        Node[] nodes;
        int b1Index, b2Index, iteraties = 0;

        while(true) // blijf doorgaan met inserts vinden totdat je een legale insert vindt
        {
            //kies een random bedrijf uit de niet lijst
            b1Index = r.Next(0, week.bedrijvenNiet.Count);
            bedrijf = week.bedrijvenNiet[b1Index];

            //maak array aan van nodes
            nodes = new Node[bedrijf.frequentie];

            for (int i = 0; i < bedrijf.frequentie; i++) // vind 4 nodes waar de nodes van dit bedrijf v��r worden geinsert
            {
                //random bedrijf
                b2Index = r.Next(0, week.bedrijvenWel.Count + 15);

                //dit is zodat er een kans is dat je een node
                //aan het eind van 1 van de 15 vulbare rijmomenten toevoegt
                //in plaats van v��r een node van een ander bedrijf
                if (b2Index >= week.bedrijvenWel.Count) // als het 1 van de 15 vulbare rijmoment-eindes is
                    nodes[i] = KiesEindnode(b2Index - week.bedrijvenWel.Count); // bereken welk rijmoment en dus welke eindnode
                else
                {
                    //anders pak een node van het bedrijf met die index in de lijst
                    nodes[i] = GetBedrijfNode(week.bedrijvenWel[b2Index]);
                }
            }

            (bo, extratijd) = week.InsertCheck(bedrijf, nodes);

            //als het een legale Insert is, stop met zoeken naar een mogelijke insert
            if (bo)
                break;
            
            //als hij na 100.000 keer proberen nog geen legale plek heeft gevonden, is er misschien
            //helemaal geen legale plek. return, om uit de infinite loop te komen
            iteraties++;
            if (iteraties == 100_000)
                return;
        }

        // bereken de incrementele kosten
        int extraTijd = extratijd.Sum() - bedrijf.strafkosten;

        int extraStrafVolume = week.InsertStrafVolumeBerekenen(bedrijf, nodes);
        int overschrijdingsKosten = OverschrijdingsKosten(extraStrafVolume);

        // als hij geaccepteerd word, insert hem
        if (AcceptatieKans(extraTijd + overschrijdingsKosten, T))
            week.Insert(bedrijf, b1Index, extratijd, extraStrafVolume, overschrijdingsKosten, nodes);
    }

    public void Delete(double T)
    {
        // als nul bedrijven worden bezocht, return
        if (week.bedrijvenWel.Count == 0) return;

        int[] extratijd;
        Bedrijf bedrijf;
        bool bo;
        int bIndex;

        while(true) // blijf doorgaan met bedrijven zoeken totdat je een legale delete hebt gevonden
        {
            bIndex = r.Next(0, week.bedrijvenWel.Count);
            bedrijf = week.bedrijvenWel[bIndex];

            (bo, extratijd) = week.DeleteCheck(bedrijf);

            //als het een legale delete is, stop met zoeken naar nieuwe nodes
            if (bo)
                break;
        }

        // bereken de incrementele kosten
        int extraTijd = extratijd.Sum() + bedrijf.strafkosten;

        int extraStrafVolume = week.DeleteStrafVolumeBerekenen(bedrijf);
        int overschrijdingsKosten = OverschrijdingsKosten(extraStrafVolume);

        // als hij wordt geaccepteerd, delete hem
        if (AcceptatieKans(extraTijd + overschrijdingsKosten, T))
            week.Delete(bedrijf, bIndex, extratijd, extraStrafVolume, overschrijdingsKosten);
    }

    public void Verplaats(double T, bool binnenRijmoment)
    {
        // als er nul bedrijven worden bezocht, return
        if (week.bedrijvenWel.Count == 0) return;
        
        Bedrijf b, b2;
        Node mover, hierVoor; 
        bool bo;
        int i;
        int j;
        int b2Index;
        int iteraties = 0;

        while(true) // blijf doorgaan met verplaatsmogelijkheden zoeken totdat je een legale hebt gevonden
        {
            b = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
            mover = GetBedrijfNode(b); // vindt een random node van een random bedrijf, deze wordt verplaatst

            if (binnenRijmoment)
            {
                b2Index = r.Next(0, mover.rijmoment.nodeLijst.Count); // eentje minder dan totale aantal nodes inclusief eindnode, kan niet voor zichzelf namelijk
                
                if (b2Index == 0)
                    hierVoor = mover.rijmoment.eindnode;
                else
                    while ((hierVoor = mover.rijmoment.nodeLijst[r.Next(0, mover.rijmoment.nodeLijst.Count)]) == mover);
            }
            else           
            {
                b2Index = r.Next(0, week.bedrijvenWel.Count + 14);

                //Dit is zodat er een kans is dat je het verplaatst naar het einde van 1 van de 15 vulbare rijmoment in plaats van 
                //naar v��r een node van een bedrijf. 14 ipv 15 omdat je hem natuurlijk niet kan verplaatsen naar een node van hetzelfde bedrijf,
                //dan zou dat bedrijf 2 keer op 1 dag worden bezocht.
                if (b2Index >= week.bedrijvenWel.Count - 1) // als het 1 van de 15 vulbare rijmoment-eindes is
                    hierVoor = KiesEindnode(b2Index - (week.bedrijvenWel.Count - 1));     //bereken naar het eind van welk rijmoment hij wordt verplaatst
                else
                {
                    // anders pak een node van een random bedrijf ongelijk zichzelf 
                    while ((b2 = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)]) == b) ;
                    hierVoor = GetBedrijfNode(b2);
                }
            }

            (bo, i, j) = week.VerplaatsCheck(mover, hierVoor);

            //als het een legale swap is, stop met nieuwe zoeken
            if (bo)
                break;

            //als hij na 100.000 keer proberen nog geen legale plek heeft gevonden, is er misschien
            //helemaal geen legale plek. return, om uit de infinite loop te komen
            iteraties++;
            if (iteraties == 100_000)
                return;
        }

        int extraStrafVolume = week.VerplaatsStrafVolumeBerekenen(mover, hierVoor);
        int overschrijdingsKosten = OverschrijdingsKosten(extraStrafVolume);

        // als het geaccepteerd wordt, verplaats hem
        if (AcceptatieKans(i + j + overschrijdingsKosten, T))
            week.Verplaats(mover, hierVoor, i, j, extraStrafVolume, overschrijdingsKosten);
    }

    public Node KiesEindnode(int i)
    {
        int bus;
        return week.
               dagen[i % 5 + 1].
               bussen[bus = i / 5 < 2 ? 0 : 1].
               rijmomenten[bus == 1 ? 0 : i % 2].
               eindnode;
    }

    public bool AcceptatieKans(int extratijd, double T) // bepaal of de actie wel of niet geaccepteerd wordt
    {
        if (extratijd < 0) // als het een verbetering is, return true
            return true;
        double acceptKans = double.Exp(-1 * extratijd / T); // anders, bereken de kans op een acceptatie
        return acceptKans > r.NextDouble(); // bepaal of hij geaccepteerd wordt
    }

    public int OverschrijdingsKosten(int extraStrafVolume)
    {
        return strafkostenCoefficient * extraStrafVolume; // mag alleen homogeen lineair zijn, anders is later niet te achterhalen hoeveel strafkosten een bepaalde toevoeging
                                        // van volume kostte, voor als je het volume weer naar beneden wilt halen
    }

    public Node GetBedrijfNode(Bedrijf b) 
    {
        return b.Locaties[r.Next(0, b.Locaties.Count)];
    }

    public void RandomWalk() 
    {
        for (int j = 0; j <= 100; j++) // voert 100 acties uit
        {
            PickAction(2_000_000); //kiest gewoon een actie met een hoge T, zodat hij gegaranadeerd wordt geaccepteerd
        } 
    }

    void HandleKeyEvents()
    {
        while (true)
        {
            // Read a key without displaying it on the console
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            // Check the pressed key
            if (keyInfo.Key == ConsoleKey.Q)
            {
                IO.ScreenShot(week);
            }
            else
            {
                Console.WriteLine($"You pressed: {keyInfo.KeyChar}");
            }
        }
    }
}