namespace rommelrouterakkers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

public class Setup
{
    private static int matrixIds = 1099;
    public static Bedrijf stort = new Bedrijf(0, 0, 0, 0, 287, 0);
    public static AfstandMatrix aMatrix;
    public Week week;
    public static List<Bedrijf> bedrijven = new List<Bedrijf>();
    public static Dictionary<int, Bedrijf> bedrijvenDict = new Dictionary<int, Bedrijf>();

    public Setup()
    {
        aMatrix = new AfstandMatrix(vulMatrix(IO.matrixFileNaam)); 
        vulBedrijven(IO.orderbestandFileNaam);
        vulDict();
        //week = new Week();
        
        //week = StelBeginoplossingIn();                  //nieuwe beginoplossing maken en loaden
        //week = IO.LoadSolution(IO._beginoplossing);     //bestaande beginoplossing loaden
        //week = IO.LoadSolutionAuto(true, r: new Random());   //load beste oplossing tot nu toe
       
        
        ZoekAlgoritme za = new ZoekAlgoritme(); 

        //za.BFS();                                       //huidige oplossing BFS'en, vooral handig na instellen van een nieuwe beginoplossing
        //IO.SaveBeginOplossing(werkWeek);                //huidige oplossing opslaan als beginoplossing

        za.ILS();                                         //ga iterated local searchen
        //IO.CreateBest(za.Week);
        //IO.PrintSolution(week);                         //huidige oplossing in de console weergeven
    }
    static void vulDict() // dictionary maken zodat je in O(1) tijd een bedrijf kan vinden aan de hand van zijn ordernummer
    {
        foreach (Bedrijf bedrijf in bedrijven)
        {
            bedrijvenDict.Add(bedrijf.orderNummer, bedrijf);
        }
    }

    static void vulBedrijven(string fileNaam) // vult de lijst met bedrijven uit het orderbestand
    {
        StreamReader sr = new StreamReader(fileNaam);
        string regel = sr.ReadLine();

        while ((regel = sr.ReadLine()) != null)
        {
            Bedrijf b = Bedrijf.parseBedrijf(regel);
            bedrijven.Add(b);
        }
    }
    
    static int[,] vulMatrix(string fileNaam) // vult een tweedimensionale array met alle rijtijden uit het afstandenbestand
    {
        int[,] matrix = new int[matrixIds, matrixIds];

        StreamReader sr = new StreamReader(fileNaam);
        string regel = sr.ReadLine();

        while ((regel = sr.ReadLine()) != null)
        {
            (int a, int b, int c) i = ParseRijtijd(regel);
            matrix[i.a, i.b] = i.c * 1000;
        }
        return matrix;
    }

    static (int, int, int) ParseRijtijd(string s) // zet een regel uit het tekstbestand om naar een tuple met
                                                  // de ordernummers en rijtijd
    {
        char separator = ';';
        string[] list = s.Split(separator);

        return (int.Parse(list[0]), int.Parse(list[1]), int.Parse(list[3]));
    }

    static List<Bedrijf> SorteerBedrijven(List<Bedrijf> bedrijven) //sorteert een lijst met bedrijven, begint bij de stort en dan steeds naar het bedrijf
                                                                   //dat het dichtst bij het vorige bedrijf ligt
    {
        List<Bedrijf> bedrijvenSorted = new List<Bedrijf>();

        int minRijtijd = int.MaxValue;
        int rijtijd;
        int besteMatrixId = 287;
        int temp = 287;

        while (bedrijven.Count != 0)
        {
            for (int i = 0; i < bedrijven.Count; i++) // voeg eerst alle bedrijven met de gevonden matrixid toe
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

    public static List<Bedrijf>[] VulBedrijvenPerFreq(List<Bedrijf> bedrijven) // split de bedrijven in 4 lijsten, 1 per frequentie
    {
        List<Bedrijf>[] bedrijvenPerFreq = new List<Bedrijf>[5];

        for (int i = 1; i <= 4; i++)
            bedrijvenPerFreq[i] = new List<Bedrijf>();

        foreach (Bedrijf bed in bedrijven)
            if (bed.orderNummer != 8942) // bedrijf met negatieve leegtijd negeren, die willen we niet in de beginoplossing, we willen zijn strafkosten
                bedrijvenPerFreq[bed.frequentie].Add(bed);
    
        return bedrijvenPerFreq;
    }

    static Week StelBeginoplossingIn() // Creeert de beginoplossing
    {
        Week werkWeek = new Week();
        
        List<Bedrijf>[] bedrijvenPerFreq = VulBedrijvenPerFreq(bedrijven);

        Rijmoment[] huidigen = new Rijmoment[6]; // maakt een array aan, met voor elke dag het rijmoment waar we op dat moment bezig zijn met invoegen
        for (int i = 1; i <= 5; i++)
            huidigen[i] = werkWeek.dagen[i].bussen[0].rijmomenten[0];

        // note: we voegen bij het maken van de beginoplossing steeds nodes toe *achterin* een rijmoment

        // Voeg voor elke frequentie de bedrijven toe aan de lege week
        BeginOplossingFreq2(SorteerBedrijven(bedrijvenPerFreq[2]), ref werkWeek, huidigen);
        BeginOplossingFreq3(SorteerBedrijven(bedrijvenPerFreq[3]), ref werkWeek, huidigen);
        BeginOplossingFreq4(bedrijvenPerFreq[4], ref werkWeek, huidigen);
        BeginOplossingFreq1(bedrijvenPerFreq[1], ref werkWeek, huidigen);

        return werkWeek; // return de week, die inmiddels een beginoplossing is

        // note: dit is precies de code die we hebben gebruikt om de ingeleverde beginoplossing te bepalen
    }

    public static void BeginOplossingFreq1(List<Bedrijf> bedrijvenFreq1, ref Week werkWeek, Rijmoment[] huidigen)
    {
        Rijmoment huidig;
        Bedrijf bedrijf;
        Bus bus;
        Dag dag;
        int p;
        int extratijd;

        for (int i = 1; i <= 5; i++) //Voor elke dag
        {
            dag = werkWeek.dagen[i];
            for (int j = 0; j <= 1; j++) // voor elke bus
            {
                bus = dag.bussen[j];
                p = 0; // index van rijmoment

                while (p <= 1 && bus.tijd <= 39100 * 1000) // expres de max tijd kleiner dan 43200 * 1000 gemaakt,
                                                           // zodat de bedrijven mooi gesplit worden over de dagen en niet een paar dagen alles krijgen
                {
                    huidig = bus.rijmomenten[p];
                    bedrijvenFreq1 = SorteerBedrijven(bedrijvenFreq1); // Als je een nieuw rijmoment pakt, sorteer de bedrijven opnieuw
                                                                       //zodat het eerste bedrijf weer het dichtst bij de stort ligt
                    while (true)
                    {
                        if (bedrijvenFreq1.Count == 0) // als alle bedrijven er al in zitten
                            return; 
                        bedrijf = bedrijvenFreq1[0]; // kies het dichtstbijzijnde bedrijf
                        extratijd = huidig.ExtraTijdskostenBijToevoegen(bedrijf, huidig.eindnode.Previous, huidig.eindnode); // bereken de incrementele kosten
                        if (bus.tijd + extratijd > 39100 * 1000) // als het qua tijd niet in de bus past
                        {
                            p = 2; // forceer een switch naar de volgende bus
                            break;
                        }
                        if (huidig.volume + bedrijf.volume > 100000) // als het qua volume niet in het rijmoment past
                        {
                            break;
                        }
                        huidig.LaatstToevoegen(bedrijf.Locaties[0], extratijd);
                        werkWeek.bedrijvenNiet.Remove(bedrijf);
                        werkWeek.bedrijvenWel.Add(bedrijf);
                        werkWeek.kosten -= bedrijf.strafkosten;
                        bedrijf.wordtBezocht = true;
                        bedrijvenFreq1.RemoveAt(0);
                    }
                    p++; // switch naar volgende rijmoment
                }
            }
        }
    }
    public static void BeginOplossingFreq2(List<Bedrijf> bedrijvenFreq2, ref Week werkWeek, Rijmoment[] huidigen)
    {
        int extratijd;
        int k = 1;
        foreach (Bedrijf bedr in bedrijvenFreq2)
        {
            if (huidigen[k].volume + bedr.volume > 100000) // als het niet meer op ma/do past ivm volume
                k++; //ma-do wordt di-vr. wat er daarna gebeurt boeit niet, zoveel bedrijven met freq 2 zijn er niet
            extratijd = huidigen[k].ExtraTijdskostenBijToevoegen(bedr, huidigen[k].eindnode.Previous, huidigen[k].eindnode); // bereken de incrementele kosten
            huidigen[k].LaatstToevoegen(bedr.Locaties[0], extratijd);
            huidigen[k + 3].LaatstToevoegen(bedr.Locaties[1], extratijd); // ook toevoegen 3 dagen verder
            bedr.wordtBezocht = true;
            werkWeek.kosten -= bedr.strafkosten;
            werkWeek.bedrijvenNiet.Remove(bedr);
            werkWeek.bedrijvenWel.Add(bedr);
        }
    }
    public static void BeginOplossingFreq3(List<Bedrijf> bedrijvenFreq3, ref Week werkWeek, Rijmoment[] huidigen)
    {
        int extratijd; 
        foreach (Bedrijf bedr in bedrijvenFreq3)
        {
            for (int i = 0; i <= 2; i++)
            {
                extratijd = huidigen[2 * i + 1].ExtraTijdskostenBijToevoegen(bedr, huidigen[2 * i + 1].eindnode.Previous, huidigen[2 * i + 1].eindnode); // bereken de incrementele kosten
                huidigen[2 * i + 1].LaatstToevoegen(bedr.Locaties[i], extratijd); // voeg toe in ma-wo-vr 
            }
            bedr.wordtBezocht = true;
            werkWeek.kosten -= bedr.strafkosten;
            werkWeek.bedrijvenNiet.Remove(bedr);
            werkWeek.bedrijvenWel.Add(bedr);
        }
    }
    public static void BeginOplossingFreq4(List<Bedrijf> bedrijvenFreq4, ref Week werkWeek, Rijmoment[] huidigen)
    {
        int extratijd;
        Bedrijf bedr4 = bedrijvenFreq4[0]; 
        for (int i = 1; i <= 4; i++)
        {
            extratijd = huidigen[i].ExtraTijdskostenBijToevoegen(bedr4, huidigen[i].eindnode.Previous, huidigen[i].eindnode); // bereken de incrementele kosten
            huidigen[i].LaatstToevoegen(bedr4.Locaties[i - 1], extratijd); // voeg toe in ma-di-wo-do 
        }
        bedr4.wordtBezocht = true;
        werkWeek.kosten -= bedr4.strafkosten;
        werkWeek.bedrijvenNiet.Remove(bedr4);
        werkWeek.bedrijvenWel.Add(bedr4);

    }

    public static Bedrijf VindBedrijf(int ord) // manier om in O(1) tijd een bedrijf te vinden aan de hand van zijn ordernummer
    {
        return bedrijvenDict[ord];
    }
}

