using System;
using System.Collections.Generic;
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
    //private double tempVerkleining = 0.99999999;
    private int totItt = 0;
    private int totIttTemp = 0; // om iteraties van 0.5 sec terug bij te houden voor it/sec berekening
    private int besteScoreTemp;
    private int sweeps = 0;

    public ZoekAlgoritme()
    {
        r = new Random();
        week = new Week();
        FillRandomWeek();

        timer = new Stopwatch(); // voor de elapsed time
        
        bestOplossing = week.Kosten;
        besteScoreTemp = week.Kosten;

        //timer 2 is voor het berekenen van de iteraties per seconde
        timer2 = new Timer();
        timer2.Interval = 500;
        timer2.Elapsed += OnTimedEvent;
        timer2.AutoReset = true;

        
    }
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

    public Week Week { get { return week; } }

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
        bestOplossing = week.Kosten;
        sweeps = 0; // resetten zodat ie bijv niet randomwalks gaat doen terwijl we steeds beste oplossingen aan het vinden zijn
    }
    public void PrintVoortgang()
    {
        Console.Clear();
        Console.WriteLine($"Beste oplossingsscore:     {(float)bestOplossing / 60000}       \n" +
                          $"Huidige score:             {(float)week.Kosten / 60000}         \n" +
                          $"Totale iteraties:          {totItt:n0}                  \n" +
                          $"Iteraties per seconde:     {2* (totItt-totIttTemp):n0}  \n" +
                          $"Sweeps since new best:     {sweeps}                     \n" +
                          $"Time elapsed:              {timer.Elapsed}");

    }

    public void ILS() //iterated local search
    {

        timer.Start();
        timer2.Enabled = true;
        double T = bestOplossing / 5800;
        //reset t, start simulated annealing
        SimAnn(T);

        //if it goes out of the simulated annealing that means that there have been a lot of iterations, so something has to change.
        //that is what sweeps is for.
        sweeps++;

        //random reset
        if (sweeps % 50 == 0)
        {
            //load de beste file tot nu toe, we kunnen later ook met gewoon lege week doen maar dit is goed voor nu
            //FillRandomWeek()
            week = IO.LoadSolutionAuto(false, r);
            bestOplossing = week.kosten;
            sweeps = 0;
        }

        //random walk
        else if (sweeps % 20 == 0)
        {
            RandomWalk();
        }

        CheckAddDelete(T / 100);

        //infinite loop
        ILS();

        timer.Stop();
        timer2.Enabled = false;
    }

    public void SimAnn(double t) // simulated annealing
    {
        double T = t; //t; //temperatuur = bestscore / 7000
        double tempVerkleining = 0.9999999; //0.999 999 99;
                                            //double maxAantalIteraties = 10_000_000 - T * 100; //Je wil aan het begin zo veel mogelijk resets en later iets minder
                                            // int sindsLastChange = 0; // aantal iteraties sinds de laatste keer dat de beste oplossing is veranderd
        while (T >= 60)//T >= Modulo) 
        {
            PickAction(T); // doe een actie

            if (week.Kosten < bestOplossing) // als een betere oplossing is gevonden
            {
                ChangeBest();
                //sindsLastChange = 0;
            }

            totItt++;
            //sindsLastChange += 1;

            T *= tempVerkleining; // verlaag de temperatuur
        }
    }

    public void PickAction(double T)
    {
        
        int welk = r.Next(0, 8); // 2/8, 1/8, 3/8, 2/8 is dus de verdeling
        if (welk <= 1)
            Insert(T);
        else if (welk <= 2)
            Delete(T);
       // else if (welk <= 5)
         //  Swap(T);
        else
        
        Verplaats(T);
        
        //Opt3(10000000);
    }

    public void CheckAddDelete(double T)
    {
        Insert(T);
        Delete(T);
       
    }

    public void Insert(double T)
    {
        // als alle bedrijven al bezocht worden, return
        if (week.bedrijvenNiet.Count == 0) return;

        Bedrijf bedrijf;
        bool bo;
        int[] extratijd;
        Node[] nodes;
        int b1Index = 0 ,b2Index = 0, dag, bus, rijmoment, iteraties = 0;

        while(true) // blijf doorgaan met inserts vinden totdat je een legale insert vindt
        {
            //kies een random bedrijf uit de niet lijst
            b1Index = r.Next(0, week.bedrijvenNiet.Count);
            bedrijf = week.bedrijvenNiet.Get(b1Index); //flag

            //maak array aan van nodes
            nodes = new Node[bedrijf.frequentie];

            for (int i = 0; i < bedrijf.frequentie; i++) // vind 4 nodes waar de nodes van dit bedrijf vóór worden geinsert
            {
                //random bedrijf
                b2Index = r.Next(0, week.bedrijvenWel.Count + 20);

                //dit is zodat er een kans is dat je een node
                //aan het eind van 1 van de 20 rijmomenten toevoegt
                //in plaats van vóór een node van een ander bedrijf
                if (b2Index >= week.bedrijvenWel.Count) // als het 1 van de 20 rijmoment-eindes is
                {
                    //bereken welk rijmoment
                    b2Index -= week.bedrijvenWel.Count;
                    dag = b2Index % 5 + 1;
                    bus = b2Index / 5 < 2 ? 0 : 1;
                    rijmoment = b2Index % 2;
                    nodes[i] = week.dagen[dag].bussen[bus].rijmomenten[rijmoment].eindnode;
                }
                else
                {
                    //anders pak een node van het bedrijf met die index in de lijst
                    nodes[i] = GetBedrijfNode(week.bedrijvenWel.GetBedrijf(b2Index)); //flag
                }
            }

            (bo, extratijd) = week.InsertCheck(bedrijf, nodes);

            //als het een legale Insert is, stop met zoeken naar een mogelijke insert
            if (bo)
                break;

            //als het geen legale insert is, voeg het bedrijf weer toe aan de niet bezochte bedrijven
            week.bedrijvenNiet.Add(bedrijf);
            
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
            week.Insert(bedrijf, b1Index, extratijd, nodes);

        //als hij toch niet geaccepteerd wordt, voeg hem weer toe aan de niet bezochte bedrijven
        else
            week.bedrijvenNiet.Add(bedrijf);
    }

    public void FillRandomWeek()
    {
        int lengte = Setup.bedrijven.Count;
        //kies aantal bedrijven tussen de laatste 50
        for (int i = r.Next(lengte - 50, lengte); i >= 0; i--)
        {
            Insert(1000000000);
        }
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
            bedrijf = week.bedrijvenWel.Get(r.Next(0, week.bedrijvenWel.Count));

            (bo, extratijd) = week.DeleteCheck(bedrijf);

            //als het een legale delete is, stop met zoeken naar nieuwe nodes
            if (bo)
                break;

            week.bedrijvenWel.Add(bedrijf);
        }

        // bereken de incrementele kosten
        int extraTijd = extratijd.Sum() + bedrijf.strafkosten;

        // als hij wordt geaccepteerd, delete hem
        if (AcceptatieKans(extraTijd, T))
            week.Delete(bedrijf, extratijd);
        
        else
            week.bedrijvenWel.Add(bedrijf);
        
        
    

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
            b = week.bedrijvenWel.GetBedrijf(r.Next(0, week.bedrijvenWel.Count));
            n1 = GetBedrijfNode(b); // vindt een random node van een random bedrijf, deze wordt verplaatst

            b2Index = r.Next(0, week.bedrijvenWel.Count + 19);

            //Dit is zodat er een kans is dat je het verplaatst naar het einde van 1 van de 20 rijmoment in plaats van 
            //naar vóór een node van een bedrijf. 19 ipv 20 omdat je hem natuurlijk niet kan verplaatsen naar een node van hetzelfde bedrijf,
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
                while ((b2 = week.bedrijvenWel.GetBedrijf(r.Next(0, week.bedrijvenWel.Count))) == b) ;
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
            b = week.bedrijvenWel.GetBedrijf(r.Next(0, week.bedrijvenWel.Count));
            
            //zorgen dat het niet dezelfde bedrijven zijn, dan heeft het geen nut
            while ((b2 = week.bedrijvenWel.GetBedrijf(r.Next(0, week.bedrijvenWel.Count))) == b) ;

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

    public void Opt3(double T)
    {
        // als er minder dan 3 bedrijven worden bezocht heeft 3opt geen zin, return
        if (week.bedrijvenWel.Count < 3) return;
        Rijmoment rij = GetRandomRijmoment();
        if (rij.nodeList.Count < 3) return;

        Node n1 , n2, n3;
        while ((n1 = rij.GetRandomNode(r)) == (n2 = rij.GetRandomNode(r)) || n1 == (n3 = rij.GetRandomNode(r)) || n2 == n3) ;
        bool bo;
        int i;
        int j;
        int k;

        var combinaties = GetCombinaties(n1, n2, n3);
        var bestCominatie = GetBestCombinatie(combinaties);

        while (true) // blijf doorgaan met 3optmogelijkheden zoeken totdat je een legale hebt gevonden
        {
            

            (bo, i, j, k) = week.SwapCheck(n1, n2, n3);

            //als het een legale 3opt is, stop met nieuwe zoeken
            if (bo)
                break;
        }

        // als hij geaccepteerd wordt, doe de 3opt
        if (AcceptatieKans(i + j + k, T))
        {
            //week.Swap(bestCominatie);
        }
    }

    public List<(Node, Node, int)> GetCombinaties(Node n1, Node n2, Node n3)
    {
        return
        new List<(Node, Node, int)>{
            (n1, n2, Setup.aMatrix.lookup(n1, n2)),
            (n2, n3, Setup.aMatrix.lookup(n2, n3)),
            (n3, n1, Setup.aMatrix.lookup(n3, n1)),
            (n1, n3, Setup.aMatrix.lookup(n1, n3)),
            (n3, n2, Setup.aMatrix.lookup(n3, n2)),
            (n2, n1, Setup.aMatrix.lookup(n2, n1))
        };
    }

    public (Node, Node, int) GetBestCombinatie(List<(Node, Node, int)> combinaties)
    {
        int best = combinaties[0].Item3;
        (Node, Node, int) bestCombinatie = combinaties[0];
        foreach (var combinatie in combinaties)
        {
            if (combinatie.Item3 < best)
            {
                best = combinatie.Item3;
                bestCombinatie = combinatie;
            }
        }
        return bestCombinatie;
    }

    public Rijmoment GetRandomRijmoment()
    {
        return week.dagen[r.Next(1, 6)].bussen[r.Next(0, 2)].rijmomenten[r.Next(0, 2)];
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
        for (int j = 0; j <= 250; j++) // voert 500 acties uit
        {
            PickAction(2_000_000); //kiest gewoon een actie met een hoge T, zodat hij gegaranadeerd wordt geaccepteerd
        }
    }

    public void ScreenShot()
    {
        IO.ScreenShot(week);
    }
}