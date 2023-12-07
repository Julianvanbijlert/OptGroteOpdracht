using System.Security.Cryptography;

namespace rommelrouterakkers;

using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;


//Manier om data in te lezen van
//Orderbestand  
//AfstandMatrix

//Manier om solution te vinden

//Manier om solutions op te slaan

//Manier om solutions te representeren




public class Program
{
    private static int aantalOrders = 1177;
    private static int matrixIds = 1099;
    private static string filepath = "../../../../";
    private static string matrixFileNaam =  filepath + "AfstandenMatrix.txt";
    private static string orderbestandFileNaam = filepath + "Orderbestand.txt";
    //private static List<Rijmoment>[] dagen = new List<Rijmoment>[6]; //BELANGRIJK: dagindexen zijn 1-5, niet 0-4 (hebben we al in week)
    private static string scoreFile = filepath + "Scores.txt";
    private static string bestScores = filepath + " ";
    //bestsolutionvariable
    public static Bedrijf stort = new Bedrijf(0, 0, 0, 0, 287, 0);
    public static AfstandMatrix aMatrix;
    public static List<int> freqcount = new List<int>();

    static List<Bedrijf> vulBedrijven(string fileNaam) // heb het naar een list verandert zodat we kunnen verwijderen voor sorteren
    {
        List<Bedrijf> bedrijven = new List<Bedrijf>();

        StreamReader sr = new StreamReader(fileNaam);
        string regel = sr.ReadLine();
        // int count = -1; // is -1 so you can do ++count in the function

        while ((regel = sr.ReadLine()) != null)
        {
            Bedrijf b = Bedrijf.parseBedrijf(regel);
            bedrijven.Add(b);
        }
        return bedrijven;

    }

    static int[,] vulMatrix(string fileNaam)
    {
        int[,] matrix = new int[matrixIds, matrixIds];

        StreamReader sr = new StreamReader(fileNaam);
        string regel = sr.ReadLine();
        int count = -1; // is -1 zodat je ++count in the functie kan doen

        while ((regel = sr.ReadLine()) != null)
        {
            (int a, int b, int c) i = ParseMatrix(regel);
            matrix[i.a, i.b] = i.c;
        }
        return matrix;
    }

    static (int, int, int) ParseMatrix(string s)
    {
        char separator = ';';
        string[] list = s.Split(separator);


        //maybe try catch for parsing, maar is niet nodig omdat we de input weten
        return (int.Parse(list[0]), int.Parse(list[1]), int.Parse(list[3]));
    }

    static List<Bedrijf> SorteerBedrijven(List<Bedrijf> bedrijven)
    {
        List<Bedrijf> bedrijvenSorted = new List<Bedrijf>();

        int minRijtijd = int.MaxValue;
        int rijtijd;
        int besteMatrixId = 287;
        int temp = 287;

        while (bedrijven.Count != 0)
        {
            for (int i = 0; i < bedrijven.Count; i++) // voeg eerst alle bedrijven met de beste matrixid toe
                if (bedrijven[i].matrixId == besteMatrixId)
                {
                    bedrijvenSorted.Add(bedrijven[i]);
                    bedrijven.RemoveAt(i);
                }

            for (int i = 0; i < bedrijven.Count; i++) // vind de nieuwe beste matrixid
            {
                rijtijd = aMatrix[besteMatrixId, bedrijven[i].matrixId];
                if (rijtijd < minRijtijd)
                {
                    minRijtijd = rijtijd;
                    temp = bedrijven[i].matrixId;
                }
            }
            besteMatrixId = temp;
            minRijtijd = int.MaxValue;
        }

        return bedrijvenSorted;
    }

    static void StelBeginoplossingIn(List<Bedrijf> bedrijven, Week werkWeek)
    {
        List<Bedrijf>[] bedrijvenPerFreq = new List<Bedrijf>[5];
        for (int i = 1; i <= 4; i++)
            bedrijvenPerFreq[i] = new List<Bedrijf>();
        foreach (Bedrijf bed in bedrijven)
            bedrijvenPerFreq[bed.frequentie].Add(bed);

        Rijmoment huidig;
        Bus bus;
        Dag dag;
        Bedrijf bedrijf;
        float extratijd;
        bool andereBus;
        for (int i = 1; i <= 5; i++)
        {
            dag = werkWeek.dagen[i];
            for (int j = 0; j <= 1; j++)
            {
                bus = dag.bussen[j];
                andereBus = false;
                while (!andereBus && bus.tijd + 1800 <= 43200)
                {
                    huidig = bus.VoegRijmomentToe();
                    bedrijvenPerFreq[1] = SorteerBedrijven(bedrijvenPerFreq[1]);
                    while (true)
                    {
                        if (bedrijvenPerFreq[1].Count == 0) return;
                        bedrijf = bedrijvenPerFreq[1][0];
                        extratijd = huidig.ExtraTijdskostenBijToevoegen(bedrijf, huidig.eindnode);
                        if (bus.tijd + extratijd > 43200)
                        {
                            andereBus = true;
                            if (huidig.beginnode.Next == huidig.eindnode)
                                bus.rijmomenten.RemoveAt(bus.rijmomenten.Count - 1);
                            break;
                        }
                        if (huidig.volume + bedrijf.volume > 100000)
                        {
                            break;
                        }
                        huidig.ToevoegenVoor(bedrijf.Locaties[0], huidig.eindnode, extratijd);
                        bus.tijd += extratijd;
                        bedrijvenPerFreq[1].RemoveAt(0); 
                    }

                }               
            }
        }
    }
   

    //loads solution from file, should return a "week"
    public Week loadSolution(string fileNaam, List<Bedrijf> bedrijven)
    {
        Week w = new Week(); 
        StreamReader sr = new StreamReader(fileNaam);
        string regel;
        while ((regel = sr.ReadLine()) != null)
        {
            string[] list = regel.Split(';');
            int bus = int.Parse(list[0]);
            int dag = int.Parse(list[1]);
            int seq = int.Parse(list[2]);
            int ord = int.Parse(list[3]);

            Bedrijf b = VindBedrijf(ord, bedrijven );

            w.Load(dag, bus, seq, b);
        }
        sr.Close();

        return w;
    }

    public Bedrijf VindBedrijf(int ord, List<Bedrijf> bedrijven)
    {
        foreach (Bedrijf b in bedrijven)
            if (b.orderNummer == ord)
                return b;
       
        return null;
    }

    static void Main() // is het handiger om, net als bij imperatief, in je main alleen 1 functie aan te roepen, en voor de rest alles in een klasse te zetten?
                       // dan kan je een nieuwe solution makkelijker aanmaken door die klasse gewoon opnieuw aan te roepen (bij inlezen)
    {
        aMatrix = new AfstandMatrix(vulMatrix(matrixFileNaam)); //afstanden niet in
        
        List<Bedrijf> bedrijven = SorteerBedrijven(vulBedrijven(orderbestandFileNaam));

        Week werkWeek = new Week();

        //vulSolution
        Random r = new Random(); // voor alles wat een random nodig heeft
        bool b = true;

        StelBeginoplossingIn(bedrijven, werkWeek);
 
        Output oup = new Output(scoreFile, bestScores);
        oup.printSolution(werkWeek);
        oup.printSolutionToFile(werkWeek);
       // oup.MakeNewBestFile(werkWeek);


    }
}



/*






 */