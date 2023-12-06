using System.IO;

namespace rommelrouterakkers;
using System.Collections.Generic;
using System;

public class Bus
{
    public List<Rijmoment> rijmomenten;
    public int tijd = 0; 

    public Bus()
    {
        rijmomenten = new List<Rijmoment>();
    }

    public void Insert(Node nieuw, Random r)
    {
        int welkMoment = r.Next(0, rijmomenten.Count);
        Rijmoment huidig = rijmomenten[welkMoment];
        tijd -= huidig.tijd;
        rijmomenten[welkMoment].ToevoegenVoor(nieuw, rijmomenten[welkMoment].eindnode);
        tijd += huidig.tijd;
    }

    public void VoegRijmomentToe()
    {
        rijmomenten.Add(new Rijmoment());
        tijd += 30;
    }
    
    public void VerwijderLeegRijmoment(Rijmoment rijmoment) // kijken hoe we dit gaan doen, hoe access je het rijmoment en bus als je alleen de nodes hebt?
    {
        tijd -= 30;
        rijmomenten.Remove(rijmoment);
    }
}
public class Dag
{
    public Bus[] bussen;

    public Dag()
    {
        bussen[0] = new Bus();
        bussen[1] = new Bus();
    }

    // manier vinden hoe je rijmoment verwijdert wanneer een rijmoment leeg is

    public void RijmomentToevoegen()
    {
        if (bussen[0].tijd <= bussen[1].tijd) 
            bussen[0].VoegRijmomentToe();
        else bussen[1].VoegRijmomentToe();

        //het is niet erg om dit niet random te maken, het is altijd voordeliger voor de optimale oplossing
        //om rijmomenten zoveel mogelijk te spreiden

        //toevoeging moet wel passen qua tijd natuurlijk
    }

    public void Insert(Node nieuw, Random r)
    {
        int welkeBus = r.Next(0, 2);
        bussen[welkeBus].Insert(nieuw, r);
    }

    ////string is welke dag het is
    //public string ToString(string i) // deze methode moet nog wel aangepast worden, door de nieuwe busklasse
    //{
    //    string dag = i.ToString();
    //    string s = "";
    //    int bus = 1;    //fillercode moet nog veranderd worden
    //    int countbus = 1;
    //    for (int j = 0; j < rijmomenten.Count;)
    //    {
    //        //LETOP hier moet nog iets gebeuren met welke bus er gebeurd
    //        s += rijmomenten[j].ToString(dag + ";" + bus, countbus );
    //    }
    //    return s;

    //    //iets om aan te geven dat de dag voorbij is
    //}
}
