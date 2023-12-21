namespace rommelrouterakkers;
using System;
using System.IO;

public static class IO
{
    private static readonly string filepath = "../../../../";

    public static readonly string matrixFileNaam = filepath + "AfstandenMatrix.txt";
    public static readonly string orderbestandFileNaam = filepath + "Orderbestand.txt";

    private static readonly string _scoreMap = "../../../scorefiles/";
    public static readonly string _beginoplossing = filepath + "Beginoplossing.txt";

    public static Week LoadSolutionAuto(bool best, Random r) // selecteert een oplossing en roept LoadSolution aan
    {
        try
        {
            string[] files = Directory.GetFiles(_scoreMap); // selecteer de map met oplossingen

            if (files.Length == 0)
            {
                Console.WriteLine("No files found in the specified directory.");
                return null; 
            }

            string file;
            if (best) 
                file = files[0]; // kies de beste oplossing
            else
            {              
                file = files[r.Next(0, files.Length)]; // kies een random oplossing
            }
            return LoadSolution(file);
        }
        catch (Exception ex) // er is iets mis met de scorefiles map
        {
            Console.WriteLine($"Error loading solution: {ex.Message}");
            return null; 
        }
    }

    public static Week LoadSolution(string fileNaam) // leest een oplossing van een tekstbestand in en returnt de hele week
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
                    catch // de regel staat niet in de juiste vorm
                    {
                        Console.WriteLine("SkippedLine due to error, check file");
                        break;
                    }

                    if (ord == 0) // als het rijmoment klaar is
                    {
                        stortIngelezen = true; 
                    }
                    
                    else if (ord != 8942) // als het nog een hele oude oplossing is staat het bedrijf met negatieve leegtijd
                                          // er nog in. Lees dat bedrijf nu niet meer in, het is immers altijd winstgevend
                                          // om dat bedrijf te skippen
                    {
                        
                        b = Setup.VindBedrijf(ord);
                        if (!b.wordtBezocht) // als er nog geen node van dit bedrijf is ingelezen
                        {
                            b.wordtBezocht = true;
                            w.kosten -= 3 * b.ledigingsDuur * b.frequentie;
                            w.bedrijvenNiet.Remove(b);
                            w.bedrijvenWel.Add(b);
                        }

                        w.Load(dag, bus, b, stortIngelezen); // load het bedrijf
                        stortIngelezen = false; // het rijmoment is nog niet klaar
                    }
                }
            }
        }
        catch (Exception ex) // het bestand staat niet in de juiste vorm
        {
            Console.WriteLine($"Error loading solution: {ex.Message}");
            return null; 
        }

        return w;
    }

    public static void PrintSolution(Week w) // schrijf de oplossing in de console
    {
        Console.WriteLine(w.ToString());
        Console.WriteLine("score: " + w.Eval);
    }

    public static void SaveBeginOplossing(Week w) // sla de huidige oplossing op in beginoplossing.txt
    {
        File.WriteAllText(_beginoplossing, w.ToString());
    }

    public static void CreateBest(Week w) // maak een nieuwe text file aan in scorefiles met de huidige oplossing in string-vorm
    {
        DateTime currentDateTime = DateTime.Now;
        string dateTimeString = currentDateTime.ToString("MM-dd_HH-mm-ss"); // de huidige datum+tijd
        try
        {
            // Voeg de locatie en de naam van het bestand samen, de score en datum vormen de naam van het bestand
            string filePath = Path.Combine(_scoreMap, $"{w.Eval}________{dateTimeString}.txt"); // de datum+tijd zorgt ervoor dat een oude file met dezelfde score niet wordt ge-overwrite

            // Schrijf de oplossing in string-vorm naar het bestand
            File.WriteAllText(filePath, w.ToString());

            Console.WriteLine($"Printed score {w.Eval} successfully");

        }
        catch (Exception ex) // er is iets fout gegaan met een file createn
        {
            Console.WriteLine($"Error creating the file: {ex.Message}");
        }
    }
}