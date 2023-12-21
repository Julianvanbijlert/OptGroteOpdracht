using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Timers;

namespace rommelrouterakkers;

public class ZoekAlgoritme
{
    private Week week;
    private int bestOplossing;
    private Stopwatch timer;
    private System.Timers.Timer timer2;
    public Random r;
    private double tempVerkleining = 0.99;
    private int totItt = 0;
    private int totIttTemp = 0;
    private int besteScoreTemp;
    private int startT = 25000;

    //tracks the amount of actions
    private long[] amountOfActions = new long[4];
    

    private int sweeps = 0;

    public ZoekAlgoritme(Week w)
    {
        week = w;
        timer = new Stopwatch();
        r = new Random();
        bestOplossing = w.Eval;
        besteScoreTemp = w.Eval;

        //timer 2 is voor het berekenen van de iteraties per seconde
        timer2 = new System.Timers.Timer();
        timer2.Interval = 500;
        timer2.Elapsed += OnTimedEvent;
        timer2.AutoReset = true;
    }

    public void OnTimedEvent(object o, ElapsedEventArgs eea)
    {
        if (bestOplossing < besteScoreTemp)
            Console.ForegroundColor = ConsoleColor.Green;
        PrintVoortgang();
        Console.ResetColor();
        totIttTemp = totItt;
        besteScoreTemp = bestOplossing;
    }

    public void BFS()
    { //bfs staat voor best first search en zorgt ervoor dat elk rijmoment optimaal wordt voor hoe het nu ingedeeld is
        week.BFS();
    }

    public void ChangeBest(Week w, int t)
    {
        if (w.Eval < 5600)
            IO.CreateBest(w);
        bestOplossing = w.Eval;
        sweeps = 0; // resetten zodat ie bijv niet randomwalks gaat doen terwijl we steeds beste oplossingen aan het vinden zijn
    }
    public void PrintVoortgang()
    {
        Console.Clear();
        Console.WriteLine($"Beste oplossingsscore:     {bestOplossing}           \n" +
                          $"Huidige score:             {week.Eval}               \n" +
                          $"Totale iteraties:          {totItt:n0}                  \n" +
                          $"Iteraties per seconde:     {2* (totItt-totIttTemp):n0}  \n" +
                          $"Sweeps since new best:     {sweeps}                  \n" +
                          $"Time elapsed:              {timer.Elapsed}");

    }

    public void ILSinf()
    {

        timer.Start();
        timer2.Enabled = true;

        //reset t
        IlSitt();
        startT = 25000;

        //if it goes out of the ilsitt that means that there have been a lot of itterations, so something has to change
        sweeps++;

        //random walk
        if (sweeps % 5 == 0)
        {
            RandomWalk();
            startT = 100000;
        }

        //random reset
        if (sweeps % 1000 == 0) 
        {
            //load de beste file tot nu toe, we kunnen later ook met gewoon lege file doen maar dit is goed voor nu
            week = IO.LoadSolutionAuto(true, r); 
            sweeps = 0;
        } 

        //infinite loop
        ILSinf();

        timer.Stop();

    }

    public void IlSitt()
    {
        double T = startT; //temperatuur
        int fy;

        //gets hit after 917 tempverkleinings
        while (T >= 2000)
        {
            PickAction(T);
            fy = week.Eval;

            if (fy < bestOplossing)
            {
                ChangeBest(week, totItt);
            }

            totItt++;

            if (totItt % 2_000_000 == 0)
            {
                T *= tempVerkleining;
            }
        }
    }

    public void PickAction(double T)
    {
        int welk = r.Next(0, 8); // 2/8, 1/8, 3/8, 2/8 is dus de verdeling
        if (welk <= 1)
        {
            amountOfActions[0]++;
            Insert(T);
        }
        else if (welk <= 2)
        {
            amountOfActions[1]++;
            Delete(T);
        }
        else if (welk <= 5)
        {
            amountOfActions[2]++;
            Swap(T);
        }
        else
        {
            amountOfActions[3]++;
            Verplaats(T);
        }       
    }

    public void Insert(double T)
    {
        if (week.bedrijvenNiet.Count == 0) return;

        Bedrijf bedrijf;
        bool bo;
        int[] extratijd;
        Node[] nodes;
        int bIndex;
        int dag;
        int bus;
        int rijmoment;

        while (true)
        {
            //kies een random bedrijf uit de niet lijst
            bedrijf = week.bedrijvenNiet[r.Next(0, week.bedrijvenNiet.Count)];
            //maak array aan van nodes
            nodes = new Node[bedrijf.frequentie];

            for (int i = 0; i < bedrijf.frequentie; i++)
            {
                //random bedrijf
                bIndex = r.Next(0, week.bedrijvenWel.Count + 20);

                //dit is zodat er een kans is dat je het bedrijf aan het einde toevoegd in plaats van voor een ander bedrijf
                if (bIndex >= week.bedrijvenWel.Count)
                {
                    //bereken de plek
                    bIndex -= week.bedrijvenWel.Count;
                    dag = bIndex % 5 + 1;
                    bus = bIndex / 5 < 2 ? 0 : 1;
                    rijmoment = bIndex % 2;
                    nodes[i] = week.dagen[dag].bussen[bus].rijmomenten[rijmoment].eindnode;
                }
                else
                {
                    //anders pak een node van dat bedrijf
                    nodes[i] = GetBedrijfNode(week.bedrijvenWel[bIndex]);
                }
            }

            (bo, extratijd) = week.InsertCheck(bedrijf, nodes);
            //als het een legale Insert is, stop met zoeken naar nieuwe nodes
            if (bo)
                break;  
        }

        int extraTijd = extratijd.Sum() - bedrijf.strafkosten;     

        if (AcceptatieKans(extraTijd, T))
            week.Insert(bedrijf, extratijd, nodes);
    }

    public void Delete(double T)
    {
        //mag geen lege lijst zijn
        if (week.bedrijvenWel.Count == 0) return;

        int[] extratijd;
        Bedrijf bedrijf;
        bool bo;

        while(true)
        {
            bedrijf = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
            (bo, extratijd) = week.DeleteCheck(bedrijf);
            //als het een legale delete is, stop met zoeken naar nieuwe nodes
            if (bo)
                break;
        }

        int extraTijd = extratijd.Sum() + bedrijf.strafkosten;

        if (AcceptatieKans(extraTijd, T))
            week.Delete(bedrijf, extratijd);
    }

    public void Verplaats(double T)
    {
        if (week.bedrijvenWel.Count == 0) return;
        Bedrijf b, b2;
        Node n1, n2;
        bool bo;
        int i;
        int j;
        int b2Index;
        int dag;
        int bus;
        int rijmoment;

        while(true)
        {
            b = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
            n1 = GetBedrijfNode(b);

            b2Index = r.Next(0, week.bedrijvenWel.Count + 19);

            //Dit is zodat er een kans is dat je het verplaatst naar het einde van een lijst
            if (b2Index >= week.bedrijvenWel.Count - 1)
            {
                b2Index -= week.bedrijvenWel.Count - 1;
                dag = b2Index % 5 + 1;
                bus = b2Index / 5 < 2 ? 0 : 1;
                rijmoment = b2Index % 2;
                n2 = week.dagen[dag].bussen[bus].rijmomenten[rijmoment].eindnode;
            }
            else
            {
                while ((b2 = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)]) == b) ;
                n2 = GetBedrijfNode(b2);
            }

            (bo, i, j) = week.VerplaatsCheck(n1, n2);
            //als het een legale swap is, stop met nieuwe zoeken
            if (bo)
                break;
        }

        if (AcceptatieKans(i + j, T))
        {
            week.Verplaats(n1, n2, i, j);
        }
    }
    public void Swap(double T)
    {
        if (week.bedrijvenWel.Count < 2) return;

        Bedrijf b, b2;
        Node n1, n2;
        bool bo;
        int i;
        int j;

        while(true)
        {
            b = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
            //zorgen dat het niet dezelfde bedrijven zijn
            while ((b2 = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)]) == b) ;

            n1 = GetBedrijfNode(b);
            n2 = GetBedrijfNode(b2);

            (bo, i, j) = week.SwapCheck(n1, n2);

            //als het een legale swap is, stop met nieuwe zoeken
            if (bo)
                break;
        }

        if (AcceptatieKans(i + j, T))
        {
            week.Swap(n1, n2, i, j);
        }
    }

    public bool AcceptatieKans(int i, double T)
    {
        double acceptKans = double.Exp(-1 * i / T);
        return i < 0 || acceptKans > r.NextDouble();
    }

    public Node GetBedrijfNode(Bedrijf b)
    {
        return b.Locaties[r.Next(0, b.Locaties.Count)];
    }

    public void RandomWalk() 
    {
        for (int j = 0; j <= 10_000; j++) 
        {
            PickAction(100_000); //kiest gewoon een actie met een hoge T, zodat hij gegaranadeerd wordt geaccepteerd
        } 
    }

}