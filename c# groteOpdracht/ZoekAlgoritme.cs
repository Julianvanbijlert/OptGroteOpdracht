using System;
using System.Diagnostics;
using System.Linq;
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

    public ZoekAlgoritme(Week w)
    {
        week = w;
        timer = new Stopwatch(); // voor de elapsed time
        r = new Random();
        bestOplossing = w.Eval;
        besteScoreTemp = w.Eval;

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
        if (week.Eval < 5600)
            IO.CreateBest(week);
        bestOplossing = week.Eval;
        sweeps = 0; // resetten zodat ie bijv niet randomwalks gaat doen terwijl we steeds beste oplossingen aan het vinden zijn
    }
    public void PrintVoortgang()
    {
        Console.Clear();
        Console.WriteLine($"Beste oplossingsscore:     {bestOplossing}              \n" +
                          $"Huidige score:             {week.Eval}                  \n" +
                          $"Totale iteraties:          {totItt:n0}                  \n" +
                          $"Iteraties per seconde:     {2* (totItt-totIttTemp):n0}  \n" +
                          $"Sweeps since new best:     {sweeps}                     \n" +
                          $"Time elapsed:              {timer.Elapsed}");

    }

    public void ILS() //iterated local search
    {

        timer.Start();
        timer2.Enabled = true;

        //reset t, start simulated annealing
        SimAnn();

        //if it goes out of the simulated annealing that means that there have been a lot of iterations, so something has to change.
        //that is what sweeps is for.
        sweeps++;

        //random reset
        if (sweeps % 20 == 0)
        {
            //load de beste file tot nu toe, we kunnen later ook met gewoon lege week doen maar dit is goed voor nu
            week = IO.LoadSolutionAuto(true, r);
            sweeps = 0;
        }

        //random walk
        else if (sweeps % 5 == 0)
        {
            RandomWalk();
        }

        //infinite loop
        ILS();

        timer.Stop();
        timer2.Enabled = false;
    }

    public void SimAnn() // simulated annealing
    {
        double T = 25000; //temperatuur

        while (T >= 2000) 
        {
            PickAction(T); // doe een actie

            if (week.Eval < bestOplossing) // als een betere oplossing is gevonden
            {
                ChangeBest();
            }

            totItt++;

            if (totItt % 2_000_000 == 0) // na elke 2 mil iteraties, verklein T
            {
                T *= tempVerkleining;
            }
        }
    }

    public void PickAction(double T)
    {
        int welk = r.Next(0, 8); // 2/8, 1/8, 3/8, 2/8 is dus de verdeling
        if (welk <= 1)
            Insert(T);
        else if (welk <= 2)
            Delete(T);
        else if (welk <= 5)
            Swap(T);
        else
            Verplaats(T);
    }

    public void Insert(double T)
    {
        // als alle bedrijven al bezocht worden, return
        if (week.bedrijvenNiet.Count == 0) return;

        Bedrijf bedrijf;
        bool bo;
        int[] extratijd;
        Node[] nodes;
        int bIndex;
        int dag;
        int bus;
        int rijmoment;
        int iteraties = 0;

        while(true) // blijf doorgaan met inserts vinden totdat je een legale insert vindt
        {
            //kies een random bedrijf uit de niet lijst
            bedrijf = week.bedrijvenNiet[r.Next(0, week.bedrijvenNiet.Count)];

            //maak array aan van nodes
            nodes = new Node[bedrijf.frequentie];

            for (int i = 0; i < bedrijf.frequentie; i++) // vind 4 nodes waar de nodes van dit bedrijf v��r worden geinsert
            {
                //random bedrijf
                bIndex = r.Next(0, week.bedrijvenWel.Count + 20);

                //dit is zodat er een kans is dat je een node
                //aan het eind van 1 van de 20 rijmomenten toevoegt
                //in plaats van v��r een node van een ander bedrijf
                if (bIndex >= week.bedrijvenWel.Count) // als het 1 van de 20 rijmoment-eindes is
                {
                    //bereken welk rijmoment
                    bIndex -= week.bedrijvenWel.Count;
                    dag = bIndex % 5 + 1;
                    bus = bIndex / 5 < 2 ? 0 : 1;
                    rijmoment = bIndex % 2;
                    nodes[i] = week.dagen[dag].bussen[bus].rijmomenten[rijmoment].eindnode;
                }
                else
                {
                    //anders pak een node van het bedrijf met die index in de lijst
                    nodes[i] = GetBedrijfNode(week.bedrijvenWel[bIndex]);
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

        // als hij geaccepteerd word, insert hem
        if (AcceptatieKans(extraTijd, T))
            week.Insert(bedrijf, extratijd, nodes);
    }

    public void Delete(double T)
    {
        // als nul bedrijven worden bezocht, return
        if (week.bedrijvenWel.Count == 0) return;

        int[] extratijd;
        Bedrijf bedrijf;
        bool bo;

        while(true) // blijf doorgaan met bedrijven zoeken totdat je een legale delete hebt gevonden
        {
            bedrijf = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
            (bo, extratijd) = week.DeleteCheck(bedrijf);

            //als het een legale delete is, stop met zoeken naar nieuwe nodes
            if (bo)
                break;
        }

        // bereken de incrementele kosten
        int extraTijd = extratijd.Sum() + bedrijf.strafkosten;

        // als hij wordt geaccepteerd, delete hem
        if (AcceptatieKans(extraTijd, T))
            week.Delete(bedrijf, extratijd);
    }

    public void Verplaats(double T)
    {
        // als er nul bedrijven worden bezocht, return
        if (week.bedrijvenWel.Count == 0) return;
        
        Bedrijf b, b2;
        Node n1, n2;
        bool bo;
        int i;
        int j;
        int b2Index;
        int dag;
        int bus;
        int rijmoment;

        while(true) // blijf doorgaan met verplaatsmogelijkheden zoeken totdat je een legale hebt gevonden
        {
            b = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
            n1 = GetBedrijfNode(b); // vindt een random node van een random bedrijf, deze wordt verplaatst

            b2Index = r.Next(0, week.bedrijvenWel.Count + 19);

            //Dit is zodat er een kans is dat je het verplaatst naar het einde van 1 van de 20 rijmoment in plaats van 
            //naar v��r een node van een bedrijf. 19 ipv 20 omdat je hem natuurlijk niet kan verplaatsen naar een node van hetzelfde bedrijf,
            //dan zou dat bedrijf 2 keer op 1 dag worden bezocht.
            if (b2Index >= week.bedrijvenWel.Count - 1) // als het 1 van de 20 rijmoment-eindes is
            {
                b2Index -= week.bedrijvenWel.Count - 1;     //bereken naar het eind van welk rijmoment hij wordt verplaatst
                dag = b2Index % 5 + 1;
                bus = b2Index / 5 < 2 ? 0 : 1;
                rijmoment = b2Index % 2;
                n2 = week.dagen[dag].bussen[bus].rijmomenten[rijmoment].eindnode;
            }
            else
            {
                // anders pak een node van een random bedrijf ongelijk zichzelf 
                while ((b2 = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)]) == b) ;
                n2 = GetBedrijfNode(b2);
            }

            (bo, i, j) = week.VerplaatsCheck(n1, n2);

            //als het een legale swap is, stop met nieuwe zoeken
            if (bo)
                break;
        }

        // als het geaccepteerd wordt, verplaats hem
        if (AcceptatieKans(i + j, T))
        {
            week.Verplaats(n1, n2, i, j);
        }
    }
    public void Swap(double T)
    {
        // als er minder dan 2 bedrijven worden bezocht heeft swap geen zin, return
        if (week.bedrijvenWel.Count < 2) return;

        Bedrijf b, b2;
        Node n1, n2;
        bool bo;
        int i;
        int j;

        while(true) // blijf doorgaan met swapmogelijkheden zoeken totdat je een legale hebt gevonden
        {
            //kies 2 random bedrijven
            b = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
            
            //zorgen dat het niet dezelfde bedrijven zijn, dan heeft het geen nut
            while ((b2 = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)]) == b) ;

            // kies 2 nodes van die bedrijven
            n1 = GetBedrijfNode(b);
            n2 = GetBedrijfNode(b2);

            (bo, i, j) = week.SwapCheck(n1, n2);

            //als het een legale swap is, stop met nieuwe zoeken
            if (bo)
                break;
        }

        // als hij geaccepteerd wordt, doe de swap
        if (AcceptatieKans(i + j, T))
        {
            week.Swap(n1, n2, i, j);
        }
    }

    public bool AcceptatieKans(int extratijd, double T) // bepaal of de actie wel of niet geaccepteerd wordt
    {
        if (extratijd < 0) // als het een verbetering is, return true
            return true;
        double acceptKans = double.Exp(-1 * extratijd / T); // anders, bereken de kans op een acceptatie
        return acceptKans > r.NextDouble(); // bepaal of hij geaccepteerd wordt
    }

    public Node GetBedrijfNode(Bedrijf b) 
    {
        return b.Locaties[r.Next(0, b.Locaties.Count)];
    }

    public void RandomWalk() 
    {
        for (int j = 0; j <= 500; j++) // voert 500 acties uit
        {
            PickAction(2_000_000); //kiest gewoon een actie met een hoge T, zodat hij gegaranadeerd wordt geaccepteerd
        }
    }
}