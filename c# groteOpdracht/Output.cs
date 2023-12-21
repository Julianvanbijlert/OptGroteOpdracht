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

    public static Week LoadSolutionAuto(bool best, Random r)
    {
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
                
                firstFilePath = files[r.Next(0, files.Length)]; // die -1 kon weg, random heeft exclusive upper bound
            }
            return LoadSolution(firstFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading solution: {ex.Message}");
            return null; // or throw an exception or handle the case as appropriate
        }
    }

    public static Week LoadSolution(string fileNaam)
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
        Console.WriteLine(w.ToString());
        Console.WriteLine("score: " + w.Eval);
    }

    public static void SaveBeginOplossing(Week w)
    {
        StreamWriter wr = new StreamWriter(_beginoplossing);
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
            string filePath = Path.Combine(_scoreMap, $"{w.Eval}________{dateTimeString}.txt");

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