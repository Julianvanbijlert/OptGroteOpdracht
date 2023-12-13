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
        int oplossing = Int32.MaxValue;
        int iteratiesSindsVeranderd = 0; //hoe lang geleden het is sinds de laatste verandering
        int totIteraties = 0;
        int justASmallScore = 5300;
        const int maxAmountOfItt = 1000;
        int showIn = 0; //wanneer het print

        while (oplossing >= justASmallScore)
        {
            totIteraties++;
            oplossing = PickAction(week, r);

            //checkscore
            if (oplossing < bestOplossing)
            {
                bestOplossing = oplossing;
                iteratiesSindsVeranderd = 0;
                continue;
                //sla op in bestOplossing / naar file
            }

            

            if (++iteratiesSindsVeranderd >= maxAmountOfItt)
            {
                //randomwalk();
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
        timer.Stop();
    }

    public void PrintVoortgang(int i,int t, int s)
    {
        Console.Clear();
        Console.WriteLine($"Beste oplossingsscore is:  {bestOplossing} \n" +
                          $"Huidige Score:             {s}             \n" +
                          $"Aantal iteraties :         {i}             \n" +
                          $"Totale iteraties :         {t}             \n" +
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
            return w.Evaluate();
        }

        //w.Swap(b, r);

        return w.Evaluate();
    }

    public Bedrijf GetBedrijf(Random r)
    {
        return bedrijven[r.Next(0,bedrijven.Count)];
    }

    public void RandomWalk(int i, Random r)
    {

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
