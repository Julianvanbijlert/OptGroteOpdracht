using System.Security.Cryptography;

namespace rommelrouterakkers;
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
    //bestsolutionvariable
    public static Bedrijf stort = new Bedrijf(0, 0, 0, 0, 287, 0);
    public static AfstandMatrix aMatrix;


    static Bedrijf[] vulBedrijven(string fileNaam)
    {
        Bedrijf[] bedrijven = new Bedrijf[aantalOrders];

        StreamReader sr = new StreamReader(fileNaam);
        string regel = sr.ReadLine();
        int count = -1; // is -1 so you can do ++count in the function

        while ((regel = sr.ReadLine()) != null)
        {
            Bedrijf b = Bedrijf.parseBedrijf(regel);
            bedrijven[++count] = b;
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

    static List<Bedrijf> SorteerBedrijven(Bedrijf[] bedrijven)
    {
        List<Bedrijf> bedrijvenSorted = new List<Bedrijf>();

        int minRijtijd = int.MaxValue;
        int rijtijd;
        int besteMatrixId = 0;

        while (bedrijvenSorted.Count < aantalOrders)
        {
            for (int i = 0; i < aantalOrders; i++) // voeg eerst alle bedrijven met de beste matrixid toe
                if (bedrijven[i] != null && bedrijven[i].matrixId == besteMatrixId)
                {
                    bedrijvenSorted.Add(bedrijven[i]);
                    bedrijven[i] = null;
                }

            for (int i = 0; i < aantalOrders; i++) // vind de nieuwe beste matrixid
            {
                if (bedrijven[i] == null) continue;
                rijtijd = aMatrix[besteMatrixId, bedrijven[i].matrixId];
                if (rijtijd < minRijtijd)
                {
                    minRijtijd = rijtijd;
                    besteMatrixId = bedrijven[i].matrixId;
                }
            }
            minRijtijd = int.MaxValue;
        }

        return bedrijvenSorted;
    }

    static void StelBeginoplossingIn(List<Bedrijf> bedrijven, Week werkWeek)
    {
        List<Bedrijf>[] bedrijvenPerFreq = new List<Bedrijf>[5];
        for (int i = 1; i <= 4; i++)
            bedrijvenPerFreq[i] = new List<Bedrijf>();
        foreach (Bedrijf bedrijf in bedrijven)
            bedrijvenPerFreq[bedrijf.frequentie].Add(bedrijf);

        int dag = 1;
        int tijd;
        Bus huidigeBus = werkWeek.dagen[1].bussen[0];
        Rijmoment huidigMoment = huidigeBus.VoegRijmomentToe();
        foreach (Bedrijf bedrijf in bedrijvenPerFreq[1])
        {
            if (huidigMoment.volume + bedrijf.volume > 100000)
            {
                if (Math.Min(werkWeek.dagen[dag].bussen[0].tijd, 
                             werkWeek.dagen[dag].bussen[1].tijd) + 1800 > 43200) // omgerekend naar seconden
                {
                    if (dag == 5) return;
                    dag += 1;
                }
                (huidigeBus, huidigMoment) = werkWeek.dagen[dag].RijmomentToevoegen();
            }
            tijd = huidigeBus.tijd + huidigMoment.ExtraTijdskostenBijToevoegen(bedrijf, huidigMoment.eindnode);
            if (tijd > 43200)
            {
                if (dag == 5) return;
                dag += 1;
                (huidigeBus, huidigMoment) = werkWeek.dagen[dag].RijmomentToevoegen();
            }
            huidigMoment.LaatstToevoegen(bedrijf.Locaties[0]);             
        }

        //ik zou zeggen: alleen de bedrijven met freq 1 erin zetten. dat is namelijk al 95 procent van de bedrijven
        //en daarmee zullen de dagen al aardig vol zitten. het programma kan de rest doen
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
 
        Output oup = new Output(scoreFile);
        oup.printSolution(werkWeek);
    }
}



/*






 */