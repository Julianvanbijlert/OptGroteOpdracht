using System.IO;

namespace rommelrouterakkers;
using System.Collections.Generic;
using System;
public class Dag
{
    public List<Rijmoment> rijmomenten;
    public Queue<Bedrijf> todoLijst; // dus niet nodig

    public int bus1Tijd = 0;
    public int bus2tijd = 0;

    public Dag()
    {
        rijmomenten = new List<Rijmoment>();
        todoLijst = new Queue<Bedrijf>();
    }

    //maakt van de lijst twee bussen zijn 
    public void splitsDag()
    {

    }

    // manier vinden hoe je rijmoment verwijdert wanneer een rijmoment leeg is

    public void RijmomentToevoegen()
    {
        rijmomenten.Add(new Rijmoment());
        // kijken of dat past qua tijd en bij welke bus hij dan moet horen
    }

    public void Insert(Node nieuw, Random r)
    {
        int welkMoment = r.Next(0, rijmomenten.Count);
        rijmomenten[welkMoment].ToevoegenVoor(nieuw, rijmomenten[welkMoment].eindnode);
    }

    //string is welke dag het is
    public string ToString(string i)
    {
        string dag = i.ToString();
        string s = "";
        int bus = 1;    //fillercode moet nog veranderd worden
        int countbus = 1;
        for (int j = 0; j < rijmomenten.Count;)
        {
            //LETOP hier moet nog iets gebeuren met welke bus er gebeurd
            s += rijmomenten[j].ToString(dag + ";" + bus, countbus );
        }
        return s;

        //iets om aan te geven dat de dag voorbij is
    }
}
