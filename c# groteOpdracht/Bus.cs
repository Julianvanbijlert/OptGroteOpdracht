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
        tijd += 1800;
        return nieuw;
    }

    public void VerwijderLeegRijmoment(Rijmoment rijmoment) // kijken hoe we dit gaan doen, hoe access je het rijmoment en bus als je alleen de nodes hebt?
    {
        tijd -= 1800;
        rijmomenten.Remove(rijmoment);
    }
}