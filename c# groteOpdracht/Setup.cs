namespace rommelrouterakkers;
using System;
using System.Collections.Generic;
using System.IO;

public class Setup
{
    private static int matrixIds = 1099;
    public static Bedrijf stort = new Bedrijf(0, 0, 0, 0, 287, 0);
    public static AfstandMatrix aMatrix;
    public Week werkWeek;
    public static List<Bedrijf> bedrijven = new List<Bedrijf>();
    public static Dictionary<int, Bedrijf> bedrijvenDict = new Dictionary<int, Bedrijf>();

    public Setup()
    {
        aMatrix = new AfstandMatrix(vulMatrix(IO.matrixFileNaam)); 
        vulBedrijven(IO.orderbestandFileNaam);
        vulDict();
 
        //werkWeek = StelBeginoplossingIn();                  //nieuwe beginoplossing maken en loaden
        //werkWeek = IO.LoadSolution(IO._beginoplossing);     //oude beginoplossing loaden
        werkWeek = IO.LoadSolutionAuto(true, new Random());   //load beste oplossing tot nu toe
        ZoekAlgoritme za = new ZoekAlgoritme(werkWeek);

        //za.BFS();                             //huidige oplossing BFS'en, vooral handig na instellen van een nieuwe beginoplossing
        //IO.SaveBeginOplossing(werkWeek);      //huidige oplossing opslaan als beginoplossing

        za.ILSinf();

        //IO.PrintSolution(werkWeek);           //huidige oplossing in de console weergeven
    }
    static void vulDict()
    {
        foreach (Bedrijf bedrijf in bedrijven)
        {
            bedrijvenDict.Add(bedrijf.orderNummer, bedrijf);
        }
    }

    static void vulBedrijven(string fileNaam) 
    {

        StreamReader sr = new StreamReader(fileNaam);
        string regel = sr.ReadLine();

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

        return (int.Parse(list[0]), int.Parse(list[1]), int.Parse(list[3]));
    }

    static List<Bedrijf> SorteerBedrijven(List<Bedrijf> bedrijven) 
        //sorteert de bedrijven, begint bij de stort en dan steeds naar het bedrijf
        // dat het dichts bij het vorige bedrijf ligt
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
        // split de bedrijven in 4 lijsten, 1 per frequentie
    {
        List<Bedrijf>[] bedrijvenPerFreq = new List<Bedrijf>[5];

        for (int i = 1; i <= 4; i++)
            bedrijvenPerFreq[i] = new List<Bedrijf>();

        foreach (Bedrijf bed in bedrijven)
            if (bed.orderNummer != 8942)    
                bedrijvenPerFreq[bed.frequentie].Add(bed);
    
        return bedrijvenPerFreq;
    }

    static Week StelBeginoplossingIn()
    {
        Week werkWeek = new Week();
        
        List<Bedrijf>[] bedrijvenPerFreq = VulBedrijvenPerFreq(bedrijven);
        Bedrijf bedrijf;
        int extratijd;

        Rijmoment[] huidigen = new Rijmoment[6];
        for (int i = 1; i <= 5; i++)
            huidigen[i] = werkWeek.dagen[i].bussen[0].rijmomenten[0];

        bedrijvenPerFreq[2] = SorteerBedrijven(bedrijvenPerFreq[2]); // eerst frequentie 2 toevoegen
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
            werkWeek.bedrijvenNiet.Remove(bedr);
            werkWeek.bedrijvenWel.Add(bedr);
        }

        bedrijvenPerFreq[3] = SorteerBedrijven(bedrijvenPerFreq[3]); // dan frequentie 3
        foreach (Bedrijf bedr in bedrijvenPerFreq[3])
        {
            for (int i = 0; i <= 2; i++)
            {
                extratijd = huidigen[2 * i + 1].ExtraTijdskostenBijToevoegen(bedr, huidigen[2 * i + 1].eindnode.Previous, huidigen[2 * i + 1].eindnode);
                huidigen[2 * i + 1].LaatstToevoegen(bedr.Locaties[i], extratijd);
            }
            bedr.wordtBezocht = true;
            werkWeek.kosten -= 3 * 3 * bedr.ledigingsDuur;
            werkWeek.bedrijvenNiet.Remove(bedr);
            werkWeek.bedrijvenWel.Add(bedr);
        }

        Bedrijf bedr4 = bedrijvenPerFreq[4][0]; // dan frequentie 4
        for (int i = 1; i <= 4; i++)
        {
            extratijd = huidigen[i].ExtraTijdskostenBijToevoegen(bedr4, huidigen[i].eindnode.Previous, huidigen[i].eindnode);
            huidigen[i].LaatstToevoegen(bedr4.Locaties[i - 1], extratijd);
        }
        bedr4.wordtBezocht = true;
        werkWeek.kosten -= 3 * 4 * bedr4.ledigingsDuur;
        werkWeek.bedrijvenNiet.Remove(bedr4);
        werkWeek.bedrijvenWel.Add(bedr4);

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
                        if (bedrijvenPerFreq[1].Count == 0) return werkWeek;
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
                        werkWeek.bedrijvenNiet.Remove(bedrijf);
                        werkWeek.bedrijvenWel.Add(bedrijf);
                        werkWeek.kosten -= 3 * bedrijf.frequentie * bedrijf.ledigingsDuur;
                        bedrijf.wordtBezocht = true;
                        bedrijvenPerFreq[1].RemoveAt(0);
                    }
                    p = 1;
                }
            }
        }

        return werkWeek;
    }

    public static Bedrijf VindBedrijf(int ord)
    {
        return bedrijvenDict[ord];
    }
}

