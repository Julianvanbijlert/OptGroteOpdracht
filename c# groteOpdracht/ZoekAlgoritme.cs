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
    public ZoekAlgoritme(Week w)
    {
        week = w;
        oplossingen = new List<(int, string)>();
        timer = new Stopwatch();
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

        while (oplossing >= justASmallScore)
        {

            //dostuff

            //checkscore
            if (oplossing < bestOplossing)
            {
                bestOplossing = oplossing;
                iteratiesSindsVeranderd = 0;
                continue;
                //sla op in bestOplossing / naar file
            }

            totIteraties++;
            iteratiesSindsVeranderd++;
            if (iteratiesSindsVeranderd >= maxAmountOfItt)
            {
                //randomwalk();
                iteratiesSindsVeranderd = 0;
            }

            PrintVoortgang(iteratiesSindsVeranderd, totIteraties, oplossing);
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
