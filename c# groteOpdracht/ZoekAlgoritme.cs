using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    private double stopCriteria = 0.01;
    private int totItt = 0;
    private int totIttTemp = 0;
    private int besteScoreTemp;

    private int sweeps = 1;

    public ZoekAlgoritme(Week w, List<Bedrijf> b)
    {
        week = w;
        timer = new Stopwatch();
        r = new Random();
        bestOplossing = w.Eval;
        besteScoreTemp = w.Eval;

        timer2 = new System.Timers.Timer();
        timer2.Interval = 500;
        timer2.Elapsed += OnTimedEvent;
        timer2.AutoReset = true;
        timer2.Enabled = true;
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
    {
        week.BFS();
    }

    public void ChangeBest(Week w, int t)
    {
        if (w.Eval < 5800)
            IO.CreateBest(w);
        bestOplossing = w.Eval;
    }
    public void PrintVoortgang()
    {
        Console.Clear();
        Console.WriteLine($"Beste oplossingsscore is:  {bestOplossing}           \n" +
                          $"Huidige Score:             {week.Eval}               \n" +
                          $"Totale iteraties :         {totItt}                  \n" +
                          $"Iteraties per seconde:     {2* (totItt -totIttTemp)} \n" +
                          $"Amount of sweeps           {sweeps}                  \n" +
                          $"Time elapsed :             {timer.Elapsed}");

    }

    public void ILSinf()
    {

        timer.Start();


        //reset t
        IlSitt(); //automatically resets t

        sweeps++;
        //if it goes out of the ilsitt that means that there have been a lot of itterations, so something has to change


        //random walk
        if (sweeps % 10 == 0)
        {
            
            //sweeps / 10 zorgt dat hij steeds meer random walked zodat hij verder uit het minimum kan komen
            RandomWalk(sweeps / 10);
        }
        //delete and add
        if (sweeps % 100 == 0)
        {
            //do delete or add
        }
        //random reset
        if (sweeps % 1000 == 0)
        {
            //load old file
            week = IO.LoadSolutionAuto(true, r);
            sweeps = 0;
        }

        //ff 0 gemaakt door verkeerde berekening score, dus gaat eeuwig door
        if (bestOplossing >= 0)
        {
            ILSinf();
        }
        timer.Stop();

    }

    public void IlSitt()
    {
        double T = 20_000; //temperatuur
        int fy;
        int geenVerbetering = 0;
        int bestHuidig = int.MaxValue;
        int welk;

        //gets hit after 917 tempverkleinings
        while (geenVerbetering < 100_000_000) //(T >= stopCriteria)
        {
            //fy = PickAction2(week, r, T);

            if (r.Next(0, 2) == 0)
                Swap(T);
            else
                Verplaats(T);

            fy = week.Eval;

            if (fy < bestOplossing)
            {
                ChangeBest(week, totItt);
                geenVerbetering = 0;
            }
            else if (fy < bestHuidig)
            {
                geenVerbetering = 0;
                bestHuidig = fy;
            }
            else
            {
                geenVerbetering++;
            }


            totItt++;

            if (totItt % 1_000_000 == 0)
            {
                T *= tempVerkleining;
            }

            //if (geenVerbetering == 10_000_000)
            //{
            //    welk = r.Next(0, 3);
            //    if (welk < 2)
            //        Insert(T);
            //    else if (welk == 2)
            //        Delete(T);
            //}

            // kijken wat we willen met insert en delete
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
            bedrijf = week.bedrijvenNiet[r.Next(0, week.bedrijvenNiet.Count)];
            nodes = new Node[bedrijf.frequentie];

            for (int i = 0; i < bedrijf.frequentie; i++)
            {
                bIndex = r.Next(0, week.bedrijvenWel.Count + 20);
                if (bIndex >= week.bedrijvenWel.Count)
                {
                    bIndex -= week.bedrijvenWel.Count;
                    dag = bIndex % 5 + 1;
                    bus = bIndex / 5 < 2 ? 0 : 1;
                    rijmoment = bIndex % 2;
                    nodes[i] = week.dagen[dag].bussen[bus].rijmomenten[rijmoment].eindnode;
                }
                else
                {
                    nodes[i] = GetBedrijfNode(week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)]);
                }
            }

            (bo, extratijd) = week.InsertCheck(bedrijf, nodes);
            if (bo)
                break;  
        }

        int extraTijd = extratijd.Sum();

        if (AcceptatieKans(extraTijd, T))
            week.Insert(bedrijf, extratijd, nodes);
    }

    public void Delete(double T)
    {
        if (week.bedrijvenWel.Count == 0) return;

        int[] extratijd;
        Bedrijf bedrijf;
        bool bo;

        while(true)
        {
            bedrijf = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
            (bo, extratijd) = week.DeleteCheck(bedrijf);
            if (bo)
                break;
        }

        int extraTijd = extratijd.Sum();

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

            while ((b2 = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)]) == b) ;

            n1 = GetBedrijfNode(b);
            n2 = GetBedrijfNode(b2);

            (bo, i, j) = week.SwapCheck(n1, n2);
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

    public void RandomWalk(int i) // maakt het programma heel sloom naarmate het aantal iteraties groter wordt
    {
        for (int j = 0; j <= i; j++) // moet niet afhankelijk zijn van i denk ik
        {
            Swap(10000000000);
        } 
    }

}