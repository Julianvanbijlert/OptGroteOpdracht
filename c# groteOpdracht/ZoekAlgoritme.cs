using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace rommelrouterakkers;

public class ZoekAlgoritme
{
    private List<(int, string)> oplossingen;
    private Week week;
    private int bestOplossing;
    private Stopwatch timer;
    public Random r;
    private List<Bedrijf> bedrijven;


    public ZoekAlgoritme(Week w, List<Bedrijf> b)
    {
        week = w;
        oplossingen = new List<(int, string)>();
        timer = new Stopwatch();
        r = new Random();
        bedrijven = b;
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
        const int maxAmountOfItt = 1000;
        int showIn = 0; //wanneer het print

        while (oplossing >= justASmallScore)
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
                
                showIn = 10000;
            }  
        } 
        PrintVoortgang(iteratiesSindsVeranderd, totIteraties, oplossing);
        //IO.CreateBest(week);
        timer.Stop();
    }

    public void ChangeBest(int b, int t)
    {

        bestOplossing = b;
        Console.ForegroundColor = ConsoleColor.Green;
        PrintVoortgang(b, 0, t);
        Console.ResetColor();
    }
     public void PrintVoortgang(int i, int t, int s)
    {
        double milToMin = 1.66666667 * Math.Pow(10, -5);
        Console.Clear();
        Console.WriteLine($"Beste oplossingsscore is:  {bestOplossing} \n" +
                          $"Huidige Score:             {s}             \n" +
                          $"Aantal iteraties :         {i}             \n" +
                          $"Totale iteraties :         {t}             \n" +
                          $"Iteraties per minuut:      {t / timer.Elapsed.TotalMilliseconds * milToMin} \n" +
                          $"Time elapsed :             {timer.Elapsed}");
      
    } 


    public int PickAction(Week w, Random r)
    {
        float chanceDelete = 0;
        float chanceInsert = 1;
        float chanceSwap = 1 - chanceDelete - chanceInsert;

        Bedrijf b = GetBedrijf(r);

        if (r.NextDouble() >= chanceSwap)
        {
            w.Pick(b, r); 
            return w.Eval;  // w.Evaluate(bedrijven); 
        }  
        Bedrijf b2;
        while ((b2 = GetBedrijf(r)) == b){}

        //w.Swap(b, b2, r);

        return w.Eval;      //w.Evaluate(bedrijven);
    }

    public Bedrijf GetBedrijf(Random r)
    {
        return bedrijven[r.Next(0,bedrijven.Count)];
    }

    public void RandomWalk(int i, Random r)
    {
        for (int j = 0; j <= i; j++)
        {
            _ = PickAction(week, r);
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

// zorgen dat na acties alle tijden goed worden aangepast (bustijden, totale kosten)

// drie niveaus: eerst zoveel mogelijk swappen. lang niks nieuws? bedrijven toevoegen/ verwidjeren. lang niks nieuws? rijmoment toevoegen/verwijderen

// hashtabel met ordernummers -> bedrijven? dan gaat dat tenminste in O(1)
