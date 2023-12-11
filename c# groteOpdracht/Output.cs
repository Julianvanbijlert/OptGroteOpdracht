namespace rommelrouterakkers;
using System;
using System.Collections.Generic;
using System.IO;
public class Output
{
    private readonly string _scoreFile;
    private readonly string _bestScores;

    public Output(string sf, string bs)
    {
        _scoreFile = sf;
        _bestScores = bs;
    }
    //VrachtwagenNummer ; Dagnummer ; hoeveelste adres ; id van dat adres (odernummer?) afstorten is 0


    //loads solution from file, should return a "week"

    /*
     * NOTE: Dit werkt alleen als er een geldige solution in de text file staat.
     * Er is geen error handling, en als hij niet langs 0 gaat dan zijn we de lul. 
     */
    public Week loadSolution(string fileNaam, List<Bedrijf> bedrijven)
    {
        Week w = new Week();
        StreamReader sr = new StreamReader(fileNaam);
        string regel;
        while ((regel = sr.ReadLine()) != null)
        {
            string[] list = regel.Split(';');
            int bus = int.Parse(list[0]) - 1;
            int dag = int.Parse(list[1]);
            int ord = int.Parse(list[3]);


            Bedrijf b = Setup.VindBedrijf(ord, bedrijven);
            b.wordtBezocht = true;

           w.Load(dag, bus, b);
        }
        sr.Close();

        return w;
    }


    public void PrintSolution(Week w)
    {
        /*
         * 1.      Vrachtautonummer (1 of 2)
           2.      Dagnummer (maandag =1, dinsdag =2, …, vrijdag =5)
           3.      Hoeveelste adres dat het voertuig op die dag aandoet (begin met 1, 2, …)
           4.      Id. van het adres (uit orderbestand.txt); de stort heeft nummer 0.

           1; 1; 1; 10
           1; 1; 2; 20
           1; 1; 3; 0
           1; 1; 4; 30
         */
        Console.WriteLine(w.ToString());
    }

    public void PrintSolutionToFile(Week w)
    {
        //pak de beste solution variabele
        //schrijf die naar de file met pad scoreFile
        StreamWriter wr = new StreamWriter(_scoreFile);
        wr.WriteLine(w.ToString());
        wr.Close();

    }

    public void MakeNewBestFile(Week w)
    {
        FileStream fs = File.Create(_bestScores);
        StreamWriter wr = new StreamWriter(fs);
        wr.WriteLine(w.ToString());
        wr.Close();
    }
}