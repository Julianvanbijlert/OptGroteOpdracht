using System.Security.Cryptography;

namespace rommelrouterakkers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


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
    private static List<Rijmoment>[] dagen = new List<Rijmoment>[6]; //BELANGRIJK: dagindexen zijn 1-5, niet 0-4
    private static string scoreFile = filepath + "Scores.txt";
    //bestsolutionvariable
    public static Bedrijf stort = new Bedrijf(0, 0, 0, 0, 287, 0);
    public static AfstandMatrix aMatrix;
    public static List<Bedrijf> bedrijven;


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
   
    static void Main() // is het handiger om, net als bij imperatief, in je main alleen 1 functie aan te roepen, en voor de rest alles in een klasse te zetten?
                       // dan kan je een nieuwe solution makkelijker aanmaken door die klasse gewoon opnieuw aan te roepen (bij inlezen)
    {
        aMatrix = new AfstandMatrix(vulMatrix(matrixFileNaam)); //afstanden niet in
        bedrijven = SorteerBedrijven(vulBedrijven(orderbestandFileNaam));

        Week werkWeek = new Week();

        //vulSolution
        Random r = new Random();
        bool b = true;

        //// uit lijst halen (wordtbezogd = true) 
        //Week e = new Week();

        //for (int i = 1; i < 7; i++) ;
        ////  while dag i nog niet vol is 
        //// op dagen controleren
        //// while (rijmoment x nogn iet vol zit) 
        //// iets uit de lijst halen toevoegen in rijmoment 
        ////   e.dagen[1].rijmomenten[0].ToevoegenVoor(bedrijven[i] ,  );


        // in de linked list gooien 
        Output oup = new Output(scoreFile);
        oup.printSolution(werkWeek);
    }
}



/*






 */