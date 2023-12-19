namespace rommelrouterakkers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

public class Setup
{
    private static int aantalOrders = 1177;
    private static int matrixIds = 1099;
    //private static List<Rijmoment>[] dagen = new List<Rijmoment>[6]; //BELANGRIJK: dagindexen zijn 1-5, niet 0-4 (hebben we al in week)
 
    //bestsolutionvariable
    public static Bedrijf stort = new Bedrijf(0, 0, 0, 0, 287, 0);
    public static AfstandMatrix aMatrix;
    public Week werkWeek = new Week();
    public static List<Bedrijf> bedrijven = new List<Bedrijf>();
    public static Dictionary<int, Bedrijf> bedrijvenDict = new Dictionary<int, Bedrijf>();
    // 2 dictionaries bijhouden, 1 met welke bedrijven er wel in zitten en 1 met welke bedrijven er niet in zitten, qua ordernummer
    // dan kan je redelijk makkelijk verwijderen/toevoegen/bedrijf kiezen


    public Setup()
    {
        aMatrix = new AfstandMatrix(vulMatrix(IO.matrixFileNaam)); //afstanden niet in
        vulBedrijven(IO.orderbestandFileNaam);
        vulDict();

        //werkWeek = new Week();
        //werkWeek = IO.loadSolution("../../../../Scores.txt", bedrijven); // dat is voor nu de beginoplossing
        //StelBeginoplossingIn(bedrijven, werkWeek); 


        werkWeek = IO.LoadSolution(IO._scoreFile , bedrijven);
        //werkWeek = IO.LoadSolutionAuto(true, new Random());
        ZoekAlgoritme za = new ZoekAlgoritme(werkWeek, bedrijven);

        // ik zou hem van tevoren ook ff bfs'en voor de zekerheid, kost niet veel tijd

        //za.BFS();
        za.ILSinf();


        //IO.PrintSolution(werkWeek);
        //IO.PrintSolutionToFile(werkWeek);
        //IO.MakeNewBestFile(werkWeek); 
    }
    static void vulDict()
    {
        foreach (Bedrijf bedrijf in bedrijven)
        {
            bedrijvenDict.Add(bedrijf.orderNummer, bedrijf);
        }
    }

    static void vulBedrijven(string fileNaam) // heb het naar een list verandert zodat we kunnen verwijderen voor sorteren
    {

        StreamReader sr = new StreamReader(fileNaam);
        string regel = sr.ReadLine();
        // int count = -1; // is -1 so you can do ++count in the function

        while ((regel = sr.ReadLine()) != null)
        {
            Bedrijf b = Bedrijf.parseBedrijf(regel);
            bedrijven.Add(b);
        }

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
            matrix[i.a, i.b] = i.c * 1000;
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

    public static List<Bedrijf>[] VulBedrijvenPerFreq(List<Bedrijf> bedrijven)
    {
        List<Bedrijf>[] bedrijvenPerFreq = new List<Bedrijf>[5];

        for (int i = 1; i <= 4; i++)
            bedrijvenPerFreq[i] = new List<Bedrijf>();

        foreach (Bedrijf bed in bedrijven)
            if (bed.orderNummer != 8942)    
                bedrijvenPerFreq[bed.frequentie].Add(bed);
    
        return bedrijvenPerFreq;
    }

    static void StelBeginoplossingIn(List<Bedrijf> bedrijven, Week werkWeek)
    {
        List<Bedrijf>[] bedrijvenPerFreq = VulBedrijvenPerFreq(bedrijven);
        Bedrijf bedrijf;

        int extratijd;
        bedrijvenPerFreq[2] = SorteerBedrijven(bedrijvenPerFreq[2]);

        Rijmoment[] huidigen = new Rijmoment[6];
        for (int i = 1; i <= 5; i++)
            huidigen[i] = werkWeek.dagen[i].bussen[0].rijmomenten[0];

        int k = 1;
        foreach (Bedrijf bedr in bedrijvenPerFreq[2])
        {
            if (huidigen[k].volume + bedr.volume > 100000)
                k++;
            extratijd = huidigen[k].ExtraTijdskostenBijToevoegen(bedr, huidigen[k].eindnode.Previous, huidigen[k].eindnode);
            huidigen[k].LaatstToevoegen(bedr.Locaties[0], extratijd);
            huidigen[k + 3].LaatstToevoegen(bedr.Locaties[1], extratijd);
            bedr.wordtBezocht = true;
            werkWeek.kosten -= 3 * 2 * bedr.ledigingsDuur;
            werkWeek.bedrijvenNiet.Remove(bedr.orderNummer);
            werkWeek.bedrijvenWel.Add(bedr.orderNummer, bedr);
        }

        bedrijvenPerFreq[3] = SorteerBedrijven(bedrijvenPerFreq[3]);
        foreach (Bedrijf bedr in bedrijvenPerFreq[3])
        {
            for (int i = 0; i <= 2; i++)
            {
                extratijd = huidigen[2 * i + 1].ExtraTijdskostenBijToevoegen(bedr, huidigen[2 * i + 1].eindnode.Previous, huidigen[2 * i + 1].eindnode);
                huidigen[2 * i + 1].LaatstToevoegen(bedr.Locaties[i], extratijd);
            }
            bedr.wordtBezocht = true;
            werkWeek.kosten -= 3 * 3 * bedr.ledigingsDuur;
            werkWeek.bedrijvenNiet.Remove(bedr.orderNummer);
            werkWeek.bedrijvenWel.Add(bedr.orderNummer, bedr);
        }

        Bedrijf bedr4 = bedrijvenPerFreq[4][0];
        for (int i = 1; i <= 4; i++)
        {
            extratijd = huidigen[i].ExtraTijdskostenBijToevoegen(bedr4, huidigen[i].eindnode.Previous, huidigen[i].eindnode);
            huidigen[i].LaatstToevoegen(bedr4.Locaties[i - 1], extratijd);
        }
        bedr4.wordtBezocht = true;
        werkWeek.kosten -= 3 * 4 * bedr4.ledigingsDuur;
        werkWeek.bedrijvenNiet.Remove(bedr4.orderNummer);
        werkWeek.bedrijvenWel.Add(bedr4.orderNummer, bedr4);

        Rijmoment huidig;
        Bus bus;
        Dag dag;
        bool andereBus;
        int p;

        for (int i = 1; i <= 5; i++)
        {
            huidig = huidigen[i];
            dag = werkWeek.dagen[i];
            for (int j = 0; j <= 1; j++)
            {
                bus = dag.bussen[j];
                andereBus = false;
                p = 0;

                while (!andereBus && bus.tijd + 1800 * 1000 <= 39100 * 1000)
                {    
                    huidig = bus.rijmomenten[p];
                    bedrijvenPerFreq[1] = SorteerBedrijven(bedrijvenPerFreq[1]);
                    while (true)
                    {
                        if (bedrijvenPerFreq[1].Count == 0) return;
                        bedrijf = bedrijvenPerFreq[1][0];
                        extratijd = huidig.ExtraTijdskostenBijToevoegen(bedrijf, huidig.eindnode.Previous, huidig.eindnode);
                        if (bus.tijd + extratijd > 39100 * 1000)
                        {
                            andereBus = true;
                            break;
                        }
                        if (huidig.volume + bedrijf.volume > 100000) //wellicht op 80000 ofzo zetten om het programma speling te geven
                        {
                            break; 
                        }
                        huidig.LaatstToevoegen(bedrijf.Locaties[0], extratijd);
                        werkWeek.bedrijvenNiet.Remove(bedrijf.orderNummer);
                        werkWeek.bedrijvenWel.Add(bedrijf.orderNummer, bedrijf);
                        werkWeek.kosten -= 3 * bedrijf.frequentie * bedrijf.ledigingsDuur;
                        bedrijf.wordtBezocht = true;
                        bedrijvenPerFreq[1].RemoveAt(0);
                    }
                    p = 1;
                }
            }
        }
    }

    public static Bedrijf VindBedrijf(int ord)
    {
        return bedrijvenDict[ord];
    }

    public static void ResetBedrijven()
    {
        foreach (Bedrijf b in bedrijven)
        {
            b.wordtBezocht = false;
            b.ResetNodes();
        }
    }
}

