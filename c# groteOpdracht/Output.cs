namespace rommelrouterakkers;
using System;
using System.IO;

public static class IO
{
    private static readonly string filepath = "../../../../";

    public static readonly string matrixFileNaam = filepath + "AfstandenMatrix.txt";
    public static readonly string orderbestandFileNaam = filepath + "Orderbestand.txt";

    public static readonly string _scoreMap = "../../../scorefiles/";
    public static readonly string _beginoplossing = filepath + "Beginoplossing.txt";
    private static readonly string _screenMap = "../../../screenshots/";

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


    public static Week LoadPickSolution()
    {
        Console.WriteLine("Drag and drop the desired solution file into the console, or enter its path:");
        string selectedFilePath = Console.ReadLine().Replace("\"", "");

        Week w = LoadSolution(selectedFilePath);
        if (w == null)
            return LoadPickSolution();
        return w;
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
                        Console.WriteLine("File is not in correct format");
                        return null;
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
                            w.kosten -= b.strafkosten;
                            for (int i = 0; i < w.bedrijvenNiet.Count; i++)
                                if (w.bedrijvenNiet[i] == b)
                                {
                                    w.bedrijvenNiet.RemoveAt(i);
                                    break;
                                }
                            w.bedrijvenWel.Add(b);
                        }

                        w.Load(dag, bus, b, stortIngelezen); // load een node van het bedrijf
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
        Console.WriteLine("score: " + (float) w.Kosten / 60000);
    }

    public static void SaveBeginOplossing(Week w) // sla de huidige oplossing op in beginoplossing.txt
    {
        File.WriteAllText(_beginoplossing, w.ToString());
    }

    public static void CreateFile(Week w, string mapje) // maak een nieuwe text file aan in scorefiles met de huidige oplossing in string-vorm
    {
        DateTime currentDateTime = DateTime.Now;
        string dateTimeString = currentDateTime.ToString("MM-dd_HH-mm-ss"); // de huidige datum+tijd
        try
        {
            // Voeg de locatie en de naam van het bestand samen, de score en datum vormen de naam van het bestand
            string filePath = Path.Combine(mapje, $"{Math.Round((float)w.Kosten / 60000, 2)}________{dateTimeString}.txt"); // de datum+tijd zorgt ervoor dat een oude file met dezelfde score niet wordt ge-overwrite

            // Schrijf de oplossing in string-vorm naar het bestand
            File.WriteAllText(filePath, w.ToString());

            Console.WriteLine($"Printed score {(float) w.Kosten / 60000} successfully");

        }
        catch (Exception ex) // er is iets fout gegaan met een file createn
        {
            Console.WriteLine($"Error creating the file: {ex.Message}");
        }
    }

    public static void ScreenShot(Week w)
    {
        if (w.totaalStrafVolume != 0)
        {
            Console.WriteLine("Deze oplossing kan niet worden opgeslagen, volumeconstraint wordt overschreden");
            return;
        }

        CreateFile(w, _screenMap);
    }
}