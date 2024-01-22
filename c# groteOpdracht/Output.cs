namespace rommelrouterakkers;
using System;
using System.Collections.Generic;
using System.IO;

//Eigenlijk zou dit een static class moeten zijn
public static class IO
{
    private static readonly string filepath = "../../../../";

    public static readonly string matrixFileNaam = filepath + "AfstandenMatrix.txt";
    public static readonly string orderbestandFileNaam = filepath + "Orderbestand.txt";

    private static readonly string _scoreMap = "../../../scorefiles/";
    public static readonly string _scoreFile = filepath + "Scores.txt";
    private static readonly string _screenMap = "../../../screenshots/";


    //VrachtwagenNummer ; Dagnummer ; hoeveelste adres ; id van dat adres (odernummer?) afstorten is 0


    //loads solution from file, should return a "week"

    /*
     * NOTE: Dit werkt alleen als er een geldige solution in de text file staat.
     * Er is geen error handling, en als hij niet langs 0 gaat dan zijn we de lul.
     */
    public static Week LoadSolutionAuto(bool best, Random r)
    {
        Week w = new Week();
        Setup.ResetBedrijven();
        try
        {
            string[] files = Directory.GetFiles(_scoreMap);

            if (files.Length == 0)
            {
                Console.WriteLine("No files found in the specified directory.");
                return null; // or throw an exception or handle the case as appropriate
            }

            string firstFilePath;
            if (best)
                firstFilePath = files[0];
            else
            {
                
                firstFilePath = files[r.Next(0, files.Length - 1)];
            }
            w = LoadSolution(firstFilePath, Setup.bedrijven);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading solution: {ex.Message}");
            return null; // or throw an exception or handle the case as appropriate
        }

        return w;
    }

    public static Week LoadPickSolution()
    {
        Week w = new Week();

        try
        {
            Console.WriteLine("Enter the path of the solution file:");
            string selectedFilePath = Console.ReadLine();

            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                w = LoadSolution(selectedFilePath, Setup.bedrijven);
            }
            else
            {
                Console.WriteLine("Invalid file path. Please provide a valid path.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading solution: {ex.Message}");
            return null;
        }

        return w;
    }

    public static Week LoadSolution(string fileNaam, List<Bedrijf> bedrijven)
    {
        Week w = new Week();

        try
        {
            using (StreamReader sr = new StreamReader(fileNaam))
            {
                string regel;
                string[] list;
                int bus;
                int dag;
                int ord;
                Bedrijf b;
                bool stortIngelezen = true;

                while ((regel = sr.ReadLine()) != null)
                {
                    try
                    {
                        list = regel.Split(';');
                        bus = int.Parse(list[0]) - 1;
                        dag = int.Parse(list[1]);
                        ord = int.Parse(list[3]);
                    }
                    catch 
                    {
                        Console.WriteLine("SkippedLine due to error, check file");
                        break;
                    }

                    if (ord == 0)
                    {
                        stortIngelezen = true;
                    }
                    
                    else if (ord != 8942)
                    {
                        
                        b = Setup.VindBedrijf(ord);
                        if (!b.wordtBezocht)
                        {
                            b.wordtBezocht = true;
                            w.kosten -= 3 * b.ledigingsDuur * b.frequentie;
                            w.bedrijvenNiet.Remove(b);
                            w.bedrijvenWel.Add(b);
                        }

                        w.Load(dag, bus, b, stortIngelezen);
                        stortIngelezen = false;
                    }
                }
            }

            //foreach (Bedrijf bedrijf in bedrijven)
            //{
            //    if (!bedrijf.wordtBezocht)
            //    {
            //        w.kosten += 3 * bedrijf.frequentie * bedrijf.ledigingsDuur;
                      
            //        w.bedrijvenNiet.Add(bedrijf.orderNummer, bedrijf);
            //    }
            //}

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading solution: {ex.Message}");
            return null; // or throw an exception or handle the case as appropriate
        }

        return w;
    }




    public static void PrintSolution(Week w)
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

        Console.WriteLine("score: " + w.Eval);
        Console.WriteLine(
            "Ik heb alle ledigingsduren naar boven afgerond. hierdoor valt de score ongeveer +/- 5 hoger uit \n" +
            "dan zou moeten, maar daardoor bouwen we geen afrondfouten op, wat vervelend is bij controleren \n" +
            "of tijden groter of kleiner zijn dan 0. een iets hoger uitvallende score is opzich geen enorme ramp");
    }

    public static void PrintSolutionToFile(Week w)
    {
        //pak de beste solution variabele
        //schrijf die naar de file met pad scoreFile
        StreamWriter wr = new StreamWriter(_scoreFile);
        wr.WriteLine(w.ToString());
        wr.Close();

    }

    public static void CreateBest(Week w)
    {
        string s = w.ToString();
        DateTime currentDateTime = DateTime.Now;
        string dateTimeString = currentDateTime.ToString("MM-dd_HH-mm-ss"); // Using underscores instead of colons
        try
        {
            // Combine the location and the filename (using the integer as the filename)
            string filePath = Path.Combine(_scoreMap, $"{(float)w.Kosten / 60000}________{dateTimeString}.txt");

            // Write the string content to the file
            File.WriteAllText(filePath, s);

            Console.WriteLine($"Printed score {(float)w.Kosten / 60000} successfully");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating the file: {ex.Message}");
        }
    }

    public static void OpenPrevFile()
    {
        
    }

    public static void ScreenShot(Week w)
    {
        string s = w.ToString();
        DateTime currentDateTime = DateTime.Now;
        string dateTimeString = currentDateTime.ToString("MM-dd_HH-mm-ss"); // Using underscores instead of colons
        try
        {
            // Combine the location and the filename (using the integer as the filename)
            string filePath = Path.Combine(_screenMap, $"{dateTimeString}________{w.Eval}.txt");

            // Write the string content to the file
            File.WriteAllText(filePath, s);

            Console.WriteLine($"Printed score {w.Eval} successfully");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating the file: {ex.Message}");
        }
    }
}