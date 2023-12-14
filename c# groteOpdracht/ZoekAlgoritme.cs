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
        const int maxAmountOfItt = 1000;
        int showIn = 0; //wanneer het print

        double T = 100; //temperatuur

        while (oplossing >= justASmallScore && totIteraties < 10000000) // stop anders stopt ie nooit, dat is toch de bedoeling? ja maar voor het testen ff
        {
            totIteraties++;
            oplossing = PickAction(week, r);

            //checkscore
            if (oplossing <= bestOplossing)  
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
        IO.CreateBest(week); 
        timer.Stop();
    }

    public void ChangeBest(int b, int t) // voor het testen ff wat dingen weggecomment, anders was het niet leesbaar in de console
    {
        //IO.CreateBest(week); ff weggecomment zodat ie nu nog niet duizenden files maakt
        best = week; 
        bestOplossing = b;
        
        //Console.ForegroundColor = ConsoleColor.Green;
        //PrintVoortgang(b, 0, t);
        //Console.ForegroundColor = ConsoleColor.White;
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
                          $"Iteraties per seconde:      {t / timer.Elapsed.TotalSeconds} \n" +
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

        int kostenTemp = w.Eval;  // stuk hieronder is allemaal redelijk tijdelijk,
                                  // (en alleen goed voor lokaal optimum)
                                  // maar was vooral om te testen
        Bedrijf b;
        Bedrijf b2;

        while ((b = GetBedrijf(r)).wordtBezocht == false);
        while ((b2 = GetBedrijf(r)) == b || b2.wordtBezocht == false);
        Node node1 = GetBedrijfNode(b, r);
        Node node2 = GetBedrijfNode(b2, r);

        (bool legaal, int extratijd1, int extratijd2) = w.VerplaatsCheck(node1, node2); //SwapCheck
        if (legaal && extratijd1 + extratijd2 <= 0)
        {
            w.Verplaats(node1, node2, extratijd1, extratijd2); //Swap
            return w.Eval;
        }

        return w.Eval + extratijd1 + extratijd2;      //w.Evaluate(bedrijven);
    }


    public void ILS2()
    {
        Week w = new Week();

        double T = 100; //temperatuur
        double acceptKans;

        Week fx = w;
        Week fy;

        int totItt = 0;

        while (T >= stopCriteria)
        {
            fy = PickAction2(w, r);

            if (fy.Eval < bestOplossing)
            {
                ChangeBest(fy, totItt);
            }

            acceptKans = double.Exp((fx.Eval - fy.Eval) / T);
            if (fy.Eval <= fx.Eval || acceptKans > r.NextDouble())
            {
                fx = fy;
            }

            
            
            T *= tempVerkleining;


            totItt++;
        }
    }
    public Week PickAction2(Week w, Random r) 
        // dit is denk ik alsnog veel te veel insert en delete. we gaan verplaats/swap
        // de standaard acties moeten maken, en insert/delete als we niks beters kunnen vinden.
        // en zorgen dat insert/delete ook met score afhankelijke kansen gaat, zodat er niet teveel 
        // bedrijven worden gedelete
    {
        Week w1 = (Week)w.Clone();
        //chances, we start with empty week, so addition is high
        // nee we beginnen bij de beginoplossing? die zit op 6222
        float chanceSwap = 0.19f;
        float chanceInsert = 0.8f; //f maakt het een float
        float chanceDelete = 1 - chanceSwap - chanceInsert; 

        Bedrijf b = GetBedrijf(r);
        double random = r.NextDouble();

        if (chanceDelete >= random)
            w1.Delete(b);
        else if (chanceSwap >= random)  
        {
            Bedrijf b2 = GetBedrijf(r);

            Node n1 = GetBedrijfNode(b, r);
            Node n2 = GetBedrijfNode(b2, r);

            (bool bo, int i, int j) = w1.SwapCheck(n1, n2 );

            w1.Swap(n1, n2, i, j);
        }
        else if(chanceInsert >= random)
            w1.Insert(b, r);

        return w1;
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
