namespace rommelrouterakkers;

using System.Collections.Generic;
using System;

public class Bedrijf
{
    //Order;Plaats;Frequentie;AantContainers;VolumePerContainer;LedigingsDuurMinuten;MatrixID;XCoordinaat;YCoordinaat
    public int orderNummer;
    public int frequentie;
    public int volume; //totale volume niet volume per container
    public int matrixId;
    public int ledigingsDuur;
    public bool wordtBezocht;
    public int strafkosten;
    public List<Node> Locaties;

    public Bedrijf(int ord, int f, int v, int aantalBakken, int mId, double ledD)
    {
        orderNummer = ord;
        frequentie = f;
        volume = v * aantalBakken;
        matrixId = mId;
        ledigingsDuur = (int) Math.Ceiling(ledD*60*1000); // kan het liever iets te hoog dan iets te laag inschatten ivm tijdsconstraints
        wordtBezocht = false;
        strafkosten = 3 * frequentie * ledigingsDuur;
        FillLocaties(); // Maak vast nodes aan
    }

    public static Bedrijf parseBedrijf(string s)
    {
        char separator = ';';
        string[] list = s.Split(separator); 

        int ord = int.Parse(list[0]);
        int f = int.Parse(list[2].Substring(0, 1));
        int aantalBakken = int.Parse(list[3]);
        int v = int.Parse(list[4]);
        double ledD = double.Parse(list[5]);
        int mId = int.Parse(list[6]);

        return new Bedrijf(ord, f, v, aantalBakken, mId, ledD);
    }

    public void FillLocaties() // Maak nodes aan
    {
        Locaties = new List<Node>();
        for (int j = 0; j < frequentie; j++)
        {
            Locaties.Add(new Node(this));
        }
    }

    public Node FindUnusedNode() // vind een node die niet in een rijmoment zit
    {
        foreach (Node node in Locaties)
        {
            if (node.rijmoment == null) 
            {
                return node;
            }
        }
        return null;
    }

    public void ResetNodes() // reset de nodes, zodat duidelijk is dat ze niet in een rijmoment zitten
    {
        foreach (Node n in Locaties)
        {
            n.rijmoment = null;
        }
    }

}
