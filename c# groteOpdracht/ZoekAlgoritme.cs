using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;

namespace rommelrouterakkers;

public class ZoekAlgoritme
{
    private List<(int, string)> oplossingen;
    private Week week;
    private Week best;
    private int bestOplossing;
    private Stopwatch timer;
    public Random r;
    private List<Bedrijf> bedrijven;
    private double tempVerkleining = 0.99;
    private double stopCriteria = 0.01;
    private int totItt = 0;

    //chances, we start with empty week, so addition is high
    // nee we beginnen bij de beginoplossing? die zit op 6222
    private static float chanceSwap = 0.98f;
    private static float chanceInsert = 0.015f; //f maakt het een float
    private static float chanceDelete = 1 - chanceSwap - chanceInsert;

    private int sweeps = 1;

    public ZoekAlgoritme(Week w, List<Bedrijf> b)
    {
        week = w;
        oplossingen = new List<(int, string)>();
        timer = new Stopwatch();
        r = new Random();
        bedrijven = b;
        bestOplossing = w.Eval;
    }

    public void BFS()
    {
        week.BFS();
    }

    public void ILS()
    {
        timer.Start();
        int oplossing = week.Eval;
        int iteratiesSindsVeranderd = 0; //hoe lang geleden het is sinds de laatste verandering
        int totIteraties = 0;
        int justASmallScore = 5300;
        int amountOfRandomWalks = 5;
        const int maxAmountOfItt = 100000000;
        int showIn = 0; //wanneer het print

        double T = 100; //temperatuur

        while (oplossing >= justASmallScore && totIteraties < 1_000_000_000) // stop anders stopt ie nooit, dat is toch de bedoeling? ja maar voor het testen ff
        {
            totIteraties++;
            oplossing = PickAction(week, r);

            //checkscore
            if (oplossing < bestOplossing)  
            {
                //sla op in bestOplossing / naar file
                ChangeBest(oplossing, totIteraties);
                iteratiesSindsVeranderd = 0;
                continue;
            }

            if (++iteratiesSindsVeranderd >= maxAmountOfItt)
            {
                //misschien ++ vervangen door x / totaleIteraties, zodat je meer doet hoe langer bezig
                RandomWalk(++amountOfRandomWalks, r);

                iteratiesSindsVeranderd = 0;
            }
            
            if (--showIn <= 0)
            {
                /* Don't measure printing to console, we're only interested in
                 * the performance of the local search */
                PrintVoortgang(iteratiesSindsVeranderd, totIteraties, oplossing);
                
                showIn = 1_000_000; // heb ff veranderd naar miljoen, anders was het amper leesbaar
            }  
        } 
        //PrintVoortgang(iteratiesSindsVeranderd, totIteraties, oplossing);
        //IO.CreateBest(week);
        timer.Stop();
    }

    public void ChangeBest(int b, int t) // voor het testen ff wat dingen weggecomment, anders was het niet leesbaar in de console
    {

        if (b < 6100)
        {
            IO.CreateBest(week); //ff weggecomment zodat ie nu nog niet duizenden files maakt
        }
        
        best = week; 
        bestOplossing = b;
        
        Console.ForegroundColor = ConsoleColor.Green;
        PrintVoortgang(b, 0, t);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public void ChangeBest(Week w, int t)
    {
        IO.CreateBest(w);
        best = w;
        bestOplossing = w.Eval;

        Console.ForegroundColor = ConsoleColor.Green;
        PrintVoortgang(w.Eval, 0, t);
        Console.ResetColor();
    }
    public void PrintVoortgang(int i, int t, int s)
    {
        Console.Clear();
        Console.WriteLine($"Beste oplossingsscore is:  {bestOplossing} \n" +
                          $"Huidige Score:             {s}             \n" +
                          $"Aantal iteraties :         {i}             \n" +
                          $"Totale iteraties :         {t}             \n" +
                          $"Iteraties per seconde:     {t / timer.Elapsed.TotalSeconds} \n" +
                          $"Amount of sweeps           {sweeps}        \n" +
                          $"Time elapsed :             {timer.Elapsed}");
      
    }


    Bedrijf b;
    Bedrijf b2;
    private int kostenTemp;
    private Node node1;
    private Node node2;
    public int PickAction(Week w, Random r)
    {

        //Bedrijf b = GetBedrijf(r);

        // we kunnen dit helemaal anders aanpakken. ten eerste, we kunnen er voor zorgen dat altijd maximaal 100 bedrijven
        // tegelijk er niet in zitten, meer eruit halen is niet nodig (variabele bijhouden).
        // dan ga je standaard eerst swappen/verplaatsen met simulated annealing. na een 
        // hoog aantal iteraties daarvan daan we een willekeurig bedrijf toevoegen/verwijderen (dat is een random walk)
        // verder dacht ik: wat nou als we gwn een vast aantal rijmomenten
        // per vrachtauto per dag doen, dan hoeven we er niet meer/ minder te maken. teveel is niet erg, kost 0 punten.
        // af en toe wel de rijmomenten optimaal verwisselen natuurlijk

        //if (r.NextDouble() >= chanceSwap)
        //{
        //    w.Pick(b, r); 
        //    return w.Eval;  // w.Evaluate(bedrijven); 
        //}  

        kostenTemp = w.Eval;  // stuk hieronder is allemaal redelijk tijdelijk,
                                  // (en alleen goed voor lokaal optimum)
                                  // maar was vooral om te testen
        

        while ((b = GetBedrijf(r)).wordtBezocht == false);
        while ((b2 = GetBedrijf(r)) == b || b2.wordtBezocht == false);
        node1 = GetBedrijfNode(b, r);
        node2 = GetBedrijfNode(b2, r);

        (bool legaal, int extratijd1, int extratijd2) = w.VerplaatsCheck(node1, node2); //SwapCheck
        if (legaal && extratijd1 + extratijd2 <= 0)
        {
            w.Verplaats(node1, node2, extratijd1, extratijd2); //Swap
            return w.Eval;
        }

        return w.Eval + extratijd1 + extratijd2;      //w.Evaluate(bedrijven);
    }
    public void ILSinf()
    {
        
        timer.Start();


        //reset t
        IlSitt(); //automatically resets t
        
        sweeps++;
        //if it goes out of the ilsitt that means that there have been a lot of itterations, so something has to change


        //random walk
        if (sweeps % 10 == 0)
        {
            //sweeps / 10 zorgt dat hij steeds meer random walked zodat hij verder uit het minimum kan komen
            RandomWalk(sweeps/10, r); 
        }
        //delete and add
        if (sweeps % 100 == 0)
        {
            //do delete or add
        }
        //random reset
        if (sweeps % 1000 == 0)
        {
            //load old file
            sweeps = 0;
        }

        //ff 0 gemaakt door verkeerde berekening score, dus gaat eeuwig door
        if (bestOplossing >= 0)
        {
            ILSinf();
        }
        timer.Stop();

    }

    public void IlSitt()
    {
        double T = 100; //temperatuur
        int fy;

        //gets hit after 917 tempverkleinings
        while (T >= stopCriteria)
        {
            //fy = PickAction2(week, r, T);
            fy = Swap(T);

            if (fy < bestOplossing)
            {
                ChangeBest(week, totItt);
            }


            totItt++;

            if (totItt % 100_000 == 0)
            {
                PrintVoortgang(totItt, totItt, week.Eval);
                T *= tempVerkleining;
            }
            
        }
    }
    public int PickAction2(Week w, Random r, double T) 
    {

        Bedrijf b = GetBedrijf(r);
        double random = r.NextDouble();

        

        if (chanceDelete >= random && b.wordtBezocht)
            w.Delete(b);
        else if (chanceSwap >= random)  
        {
            Bedrijf b2 = GetBedrijf(r);

            Node n1 = GetBedrijfNode(b, r);
            Node n2 = GetBedrijfNode(b2, r);

            (bool bo, int i, int j) = w.SwapCheck(n1, n2 );
            if (bo && AcceptatieKans(i, j, T, r))
            {
                w.Swap(n1, n2, i, j);
            }
            
        }
        else if(chanceInsert >= random && !b.wordtBezocht)
            w.Insert(b, r);

        return w.Eval;
    }
    public int Swap(double T)
    {
        Bedrijf b = GetBedrijf(r);
        Bedrijf b2 = GetBedrijf(r);
        //double random = r.NextDouble();

        Node n1 = GetBedrijfNode(b, r);
        Node n2 = GetBedrijfNode(b2, r);

        (bool bo, int i, int j) = week.SwapCheck(n1, n2);
        if (bo && AcceptatieKans(i, j, T, r))
        {
            week.Swap(n1, n2, i, j);
        }

        return week.Eval;
    }

    public bool AcceptatieKans(int i, int j, double T, Random r)
    {

        double acceptKans = double.Exp((i + j) / T);
        return i + j <= 0 || acceptKans > r.NextDouble();

    }

    public Bedrijf GetBedrijf(Random r)
    {
        return bedrijven[r.Next(0,bedrijven.Count)];
    }

    public Node GetBedrijfNode(Bedrijf b, Random r)
    {
        return b.Locaties[r.Next(0, b.Locaties.Count)];
    }

    public void RandomWalk(int i, Random r) // maakt het programma heel sloom naarmate het aantal iteraties groter wordt
    {
        for (int j = 0; j <= i; j++)
        {
            _ = Swap(0.00000000000001);
        }

    }

}

public class ILS
{
    private Week week;
    public ILS(Week w)
    {
        week = w;
    }

   


    
}


/*
 action a
-Delete
-Add
-Swap
 
 
 */

// willen we anders eerst op klein niveau optimaliseren (alleen wisselen/toevoegen/verwijderen)
// en dan als dat niks oplevert naar inter rijmoment shit? zie dat variable neighbourhood descent(?) ofzo in powerpoint
// of variable neighbourhood search ofzo
// dan zitten we niet te kutten met verhoudingen


// drie niveaus: eerst zoveel mogelijk swappen. lang niks nieuws? bedrijven toevoegen/ verwidjeren. lang niks nieuws? rijmoment toevoegen/verwijderen

// hashtabel met ordernummers -> bedrijven? dan gaat dat tenminste in O(1)

// toevoegen niet in een rijmoment doen, maar juist na een willekeurige andere node? dan is het tenminste echt willekeurig en kunnen we simulated annealing doen
