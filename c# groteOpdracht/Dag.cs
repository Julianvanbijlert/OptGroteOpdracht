namespace rommelrouterakkers;
using System;

public class Dag
{
    public Bus[] bussen;
    public int getal;
    public int tijd { get { return bussen[0].tijd + bussen[1].tijd; }  }

    public Dag(Week werkWeek, int i)
    {
        bussen = new Bus[2];
        bussen[0] = new Bus(werkWeek, this);
        bussen[1] = new Bus(werkWeek, this);
        getal = i;
    }

    // manier vinden hoe je rijmoment verwijdert wanneer een rijmoment leeg is

    public Rijmoment RijmomentToevoegen() // geen void, zodat we meteen verder kunnen met rijmoment mocht het nodig zijn
    {
        if (bussen[0].tijd <= bussen[1].tijd) 
            return bussen[0].VoegRijmomentToe();
        else return bussen[1].VoegRijmomentToe();

        //het is niet erg om dit niet random te maken, het is altijd voordeliger voor de optimale oplossing
        //om rijmomenten zoveel mogelijk te spreiden

        //toevoeging moet wel passen qua tijd natuurlijk
    }

    public static bool InterBusSwapCheck(Node node1, Node node2, int extratijd1, int extratijd2)
    {
        if (node1.rijmoment.bus.tijd + extratijd1 > 43200 ||
            node2.rijmoment.bus.tijd + extratijd2 > 43200 || 
            node1.rijmoment.volume - node1.bedrijf.volume + node2.bedrijf.volume > 100000 ||
            node2.rijmoment.volume - node2.bedrijf.volume + node1.bedrijf.volume > 100000)
            return false;
        return true;
    }

    public int Insert(Node nieuw, Random r)
    {  
        int welkeBus = r.Next(0, 2);
        return bussen[welkeBus].Insert(nieuw, r);
    }

    public string ToString(string i)
    {
        string s = "";

        s += bussen[0].ToString("1;" + i + ";");
        s += bussen[1].ToString("2;" + i + ";");


        return s;
    }

    public void BFS()
    {
        foreach (var bus in bussen)
        {
            bus.BFS();
        }
    }

    ////string is welke dag het is
    //public string ToString(string i) // deze methode moet nog wel aangepast worden, door de nieuwe busklasse
    //{
    //   

    //    //iets om aan te geven dat de dag voorbij is
    //}
}
