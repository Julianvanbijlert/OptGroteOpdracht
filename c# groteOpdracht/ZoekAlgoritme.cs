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
            RandomWalk(sweeps / 10, r);
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
        while (geenVerbetering < 20_000_000) //(T >= stopCriteria)
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
        }
    }

    public void Insert(double T)
    {
        if (week.bedrijvenNiet.Count == 0) return;
        int i = 0;
        int kostenTemp = week.Eval;
        Bedrijf bedrijf = Setup.stort; // tijdelijk, waarom kan je hem niet zonder assignment gebruiken
        
        while (i < 20 && !week.Insert(bedrijf = week.bedrijvenNiet[r.Next(0, week.bedrijvenNiet.Count)], r))
            i++;

        if (week.Eval - kostenTemp < 0)
        {
            week.bedrijvenWel.Add(bedrijf);
            week.bedrijvenNiet.Remove(bedrijf);
        }
        else if (i == 20)
            return;
        else if (!AcceptatieKans(week.Eval - kostenTemp, T, r))
        {
            week.Delete(bedrijf);
        }
        else
        {
            week.bedrijvenWel.Add(bedrijf);
            week.bedrijvenNiet.Remove(bedrijf);
        }
    }

    public void Delete(double T)
    {
        if (week.bedrijvenWel.Count == 0) return;
        int i = 0;
        int[] extratijd = new int[0];
        Bedrijf bedrijf = Setup.stort;

        while (i < 20 && !((_, extratijd) = week.DeleteCheck(bedrijf = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)])).Item1)
            i++;

        if (i == 20) return;
        int extraTijd = extratijd.Sum();

        if (extraTijd < 0 || AcceptatieKans(extraTijd, T, r))
        {
            week.bedrijvenNiet.Add(bedrijf);
            week.bedrijvenWel.Remove(bedrijf);
            week.Delete(bedrijf, extratijd);
        }
    }

    public void Verplaats(double T)
    {
        if (week.bedrijvenWel.Count == 0) return;

        Bedrijf b = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
        Node n1 = GetBedrijfNode(b, r);

        Node n2;
        int b2Index = r.Next(0, week.bedrijvenWel.Count + 19);
        if (b2Index >= week.bedrijvenWel.Count - 1) 
        {          
            b2Index -= week.bedrijvenWel.Count - 1;
            int dag = b2Index % 5 + 1;
            int bus = b2Index / 5 < 2 ? 0 : 1;
            int rijmoment = b2Index % 2;
            n2 = week.dagen[dag].bussen[bus].rijmomenten[rijmoment].eindnode;
        }
        else
        {
            Bedrijf b2;
            while ((b2 = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)]) == b);
            n2 = GetBedrijfNode(b2, r);
        }

        (bool bo, int i, int j) = week.VerplaatsCheck(n1, n2);
        if (bo && AcceptatieKans(i + j, T, r))
        {
            week.Verplaats(n1, n2, i, j);
        }
    }
    public void Swap(double T)
    {
        if (week.bedrijvenWel.Count < 2) return;

        Bedrijf b = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)];
        Bedrijf b2;

        while ((b2 = week.bedrijvenWel[r.Next(0, week.bedrijvenWel.Count)]) == b);

        Node n1 = GetBedrijfNode(b, r);
        Node n2 = GetBedrijfNode(b2, r);

        (bool bo, int i, int j) = week.SwapCheck(n1, n2);
        if (bo && AcceptatieKans(i + j, T, r))
        {
            week.Swap(n1, n2, i, j);
        }
    }

    public bool AcceptatieKans(int i, double T, Random r)
    {

        double acceptKans = double.Exp(-(i) / T);
        return i <= 0 || acceptKans > r.NextDouble();

    }

    public Node GetBedrijfNode(Bedrijf b, Random r)
    {
        return b.Locaties[r.Next(0, b.Locaties.Count)];
    }

    public void RandomWalk(int i, Random r) // maakt het programma heel sloom naarmate het aantal iteraties groter wordt
    {
        for (int j = 0; j <= i; j++) // moet niet afhankelijk zijn van i denk ik
        {
            Swap(10000000000);
        } 
    }

}