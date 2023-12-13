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

        while (oplossing >= justASmallScore && totIteraties < 10000000) // stop anders stopt ie nooit
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
                
                showIn = 1000000; // heb ff veranderd naar miljoen, anders was het amper leesbaar
            }  
        } 
        PrintVoortgang(iteratiesSindsVeranderd, totIteraties, oplossing);
        //IO.CreateBest(week);
        timer.Stop();
    }

    public void ChangeBest(int b, int t)
    {

        best = week; //klopt dit? helaas niet, dat wordt een reference. je moet dan gwn een file met de beste solution opslaan
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

        int kostenTemp = w.Eval;
        Bedrijf b;
        Bedrijf b2;

        while ((b = GetBedrijf(r)).wordtBezocht == false);
        while ((b2 = GetBedrijf(r)) == b || b2.wordtBezocht == false);
        Node node1 = GetBedrijfNode(b, r);
        Node node2 = GetBedrijfNode(b2, r);

        (bool legaal, int extratijd1, int extratijd2) = w.VerplaatsCheck(node1, node2); //SwapCheck
        if (legaal && extratijd1+extratijd2 < 0)
            w.Verplaats(node1, node2, extratijd1, extratijd2); //Swap

        return w.Eval;      //w.Evaluate(bedrijven);
    }

    public Bedrijf GetBedrijf(Random r)
    {
        return bedrijven[r.Next(0,bedrijven.Count)];
    }

    public Node GetBedrijfNode(Bedrijf b, Random r)
    {
        return b.Locaties[r.Next(0, b.Locaties.Count)];
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


// drie niveaus: eerst zoveel mogelijk swappen. lang niks nieuws? bedrijven toevoegen/ verwidjeren. lang niks nieuws? rijmoment toevoegen/verwijderen

// hashtabel met ordernummers -> bedrijven? dan gaat dat tenminste in O(1)

// toevoegen niet in een rijmoment doen, maar juist na een willekeurige andere node? dan is het tenminste echt willekeurig en kunnen we simulated annealing doen
