using System.Collections.Generic;
using System;

namespace rommelrouterakkers;
//comment
public class Bedrijf
{
    //Order;Plaats;Frequentie;AantContainers;VolumePerContainer;LedigingsDuurMinuten;MatrixID;XCoordinaat;YCoordinaat
    public int orderNummer;
    public int frequentie;
    public int volume; //totale volume niet volume per container
    public int matrixId;
    public int ledigingsDuur;
    public bool wordtBezocht;

    public List<Node> Locaties;
    //gwn om ff snel dingen aan te maken voor schrijven van code
    public Bedrijf()
    {
    } 

    public Bedrijf(int ord, int f, int v, int aantalBakken, int mId, double ledD)
    {
        orderNummer = ord;
        frequentie = f;
        volume = v * aantalBakken;
        matrixId = mId;
        ledigingsDuur = (int) Math.Ceiling(ledD*60);
        wordtBezocht = false;
        FillNodesLocatie(frequentie);
    }

    //int ord, int f, int v, int aantalBakken, int mId, double ledD
    //Order;Plaats;Frequentie;AantContainers;VolumePerContainer;LedigingsDuurMinuten;MatrixID;XCoordinaat;YCoordinaat
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

    public void FillNodesLocatie(int f)
    {
        Locaties = new List<Node>();
        for (int j = 0; j < f; j++)
        {
            Locaties.Add(new Node(this));
        }
    }

    public bool checkBezocht()
    {
        return wordtBezocht;
    }

    public Node FindUnusedNode()
    {
        foreach (Node node in Locaties)
        {
            if (node.Next == null)
            {
                return node;
            }
        }
        return null;
    }
}

