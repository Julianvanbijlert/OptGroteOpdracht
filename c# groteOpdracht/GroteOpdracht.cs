namespace rommelrouterakkers;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

//Manier om data in te lezen van
    //Orderbestand  
    //AfstandMatrix

//Manier om solution te vinden

//Manier om solutions op te slaan

//Manier om solutions te representeren




public class Dag
{
    List<Rijmoment> rijmomenten = new List<Rijmoment>();

    //comment

    public Dag()
    {

    }

    public override string ToString()
    {
        return base.ToString();
    }
}



public class Program
{
    private static int aantalOrders = 1177;
    private static int matrixIds = 1099;
    private static string filepath = "../../../../";
    private static string matrixFileNaam =  filepath + "AfstandenMatrix.txt";
    private static string orderbestandFileNaam = filepath + "Orderbestand.txt";
    private static List<Rijmoment>[] dagen = new List<Rijmoment>[6]; //BELANGRIJK: dagindexen zijn 1-5, niet 0-4
    private static string scoreFile = filepath + "Scores.txt";
    //bestsolutionvariable
    public static Bedrijf stort = new Bedrijf(0, 0, 0, 0, 287, 0);


    static Bedrijf[] vulBedrijven(string fileNaam)
    {
        Bedrijf[] bedrijven = new Bedrijf[aantalOrders];

        StreamReader sr = new StreamReader(fileNaam);
        string regel = sr.ReadLine();
        int count = -1; // is -1 so you can do ++count in the function

        while ((regel = sr.ReadLine()) != null)
        {
            Bedrijf b = Bedrijf.parseBedrijf(regel);
            bedrijven[++count] = b;
        }
        return bedrijven;
    }

    static int[,] vulMatrix(string fileNaam)
    {
        int[,] matrix = new int[matrixIds, matrixIds];

        StreamReader sr = new StreamReader(fileNaam);
        string regel = sr.ReadLine();
        int count = -1; // is -1 zodat je ++count in the functie kan doen

        while ((regel = sr.ReadLine()) != null)
        {
            (int a, int b, int c) i = ParseMatrix(regel);
            matrix[i.a, i.b] = i.c;
        }
        return matrix;
    }

    static (int, int, int) ParseMatrix(string s)
    {
        char separator = ';';
        string[] list = s.Split(separator);


        //maybe try catch for parsing, maar is niet nodig omdat we de input weten
        return (int.Parse(list[0]), int.Parse(list[1]), int.Parse(list[3]));
    }

   
    static void Main()
    {
        
        Bedrijf[] bedrijven = vulBedrijven(orderbestandFileNaam);
        AfstandMatrix aMatrix = new AfstandMatrix(vulMatrix(matrixFileNaam)); //afstanden niet in

        Week werkWeek = new Week();

        //vulSolution

        bool b = true;
    }
}


/*






 */