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



public class Bedrijf
{
    //Order;Plaats;Frequentie;AantContainers;VolumePerContainer;LedigingsDuurMinuten;MatrixID;XCoordinaat;YCoordinaat
    public int orderNummer;
    public int frequentie;
    public int volume; //totale volume niet volume per container
    public int matrixId;
    public float ledigingsDuur;


    //gwn om ff snel dingen aan te maken voor schrijven van code
    public Bedrijf()
    {
    }

    public Bedrijf(int ord, int f, int v, int aantalBakken, int mId, float ledD)
    {
        orderNummer = ord;
        frequentie = f;
        volume = v * aantalBakken;
        matrixId = mId;
        ledigingsDuur = ledD;
    }

    //int ord, int f, int v, int aantalBakken, int mId, float ledD
    //Order;Plaats;Frequentie;AantContainers;VolumePerContainer;LedigingsDuurMinuten;MatrixID;XCoordinaat;YCoordinaat
    public static Bedrijf parseBedrijf(string s)
    {
        char separator = ';';
        string[] list = s.Split(separator);
        
        int ord = int.Parse(list[0]);
       
        int f = int.Parse(list[2].Substring(0, 1));
        int aantalBakken = int.Parse(list[3]);
        int v = int.Parse(list[4]);
        float ledD = float.Parse(list[5]);
        int mId = int.Parse(list[6]);

        return new Bedrijf(ord, f, v, aantalBakken, mId, ledD);
    }
}

public class AfstandMatrix
{
    //we chose to ignore distance as this does not matter for calculating time
    public int[,] matrix;

    public AfstandMatrix(int[,] matrix1)
    {
        matrix = matrix1;
    }

    public int lookup(Bedrijf b1, Bedrijf b2)
    {
        return matrix[b1.matrixId, b2.matrixId];
    }


}

public class Program
{
    private static int aantalOrders = 1177;
    private static int matrixIds = 1099;
    private static string filepath = "../../../../";
    private static string matrixFileNaam =  filepath + "AfstandenMatrix.txt";
    private static string orderbestandFileNaam = filepath + "Orderbestand.txt";

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
        AfstandMatrix aMatrix = new AfstandMatrix(vulMatrix(matrixFileNaam));

        bool b = true;
    }
}