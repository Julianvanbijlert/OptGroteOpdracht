using System.Threading;

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

    public void Insert(Node nieuw, Random r) // nog nieuw rijmoment aanmaken als hij vol is
    {
        int welkMoment = r.Next(0, rijmomenten.Count);
        Rijmoment huidig = rijmomenten[welkMoment];
        tijd -= huidig.tijd;
        rijmomenten[welkMoment].ToevoegenVoor(nieuw, rijmomenten[welkMoment].eindnode);
        tijd += huidig.tijd;
    }

    public Rijmoment VoegRijmomentToe() 
    {
        Rijmoment nieuw = new Rijmoment();
        rijmomenten.Add(nieuw);
        tijd += 30;
        return nieuw;
    }

    public void VerwijderLeegRijmoment(Rijmoment rijmoment) // kijken hoe we dit gaan doen, hoe access je het rijmoment en bus als je alleen de nodes hebt?
    {
        tijd -= 30;
        rijmomenten.Remove(rijmoment);
    }

    public string ToString(string i)
    { 
        string s = "";
        int count = 1;
        for (int j = 0; j < rijmomenten.Count; j++)
        {
          //LETOP hier moet nog iets gebeuren met welke bus er gebeurd
            (int c, string s2) = rijmomenten[j].ToString(i, count);
            count += c;
            s += s2;
        }
         return s;
    }
}