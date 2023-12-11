

namespace rommelrouterakkers;
using System.Collections.Generic;
using System;

public class Bus
{
    public List<Rijmoment> rijmomenten;
    public double tijd = 0;

    public Bus()
    {
        rijmomenten = new List<Rijmoment>();
    }

    public void Insert(Node nieuw, Random r) // nog nieuw rijmoment aanmaken als hij vol is
    {
        int welkMoment = r.Next(0, rijmomenten.Count);
        Rijmoment huidig = rijmomenten[welkMoment];
        double extratijd = huidig.ExtraTijdskostenBijToevoegen(nieuw.bedrijf, huidig.eindnode.Previous, huidig.eindnode);
        tijd += extratijd;
        huidig.ToevoegenVoor(nieuw, huidig.eindnode, extratijd);
    }

    public void Load(Bedrijf b)
    {
        //als het stort is maak je gwn een nieuwe aan en stop je daarna 
        if (b == Setup.stort)
        {
            VoegRijmomentToe();
            return;
        }

        //als dat niet zo is pak het laatste rijmoment en voeg hem daar aan toe
        int laatsteRijmoment = rijmomenten.Count;
        Rijmoment huidig = rijmomenten[laatsteRijmoment];

        //bereken de tijd die daarvoor wordt toegevoegd
        double extratijd = huidig.ExtraTijdskostenBijToevoegen(b, huidig.eindnode.Previous, huidig.eindnode);
        tijd += extratijd;

        huidig.LaatstToevoegen(b.FindUnusedNode(), extratijd);
    }

    public Rijmoment VoegRijmomentToe() 
    {
        Rijmoment nieuw = new Rijmoment(this);
        rijmomenten.Add(nieuw);
        tijd += 1800;
        return nieuw;
    }

    public void VerwijderLeegRijmoment(Rijmoment rijmoment) // kijken hoe we dit gaan doen, hoe access je het rijmoment en bus als je alleen de nodes hebt?
    {
        tijd -= 1800;
        rijmomenten.Remove(rijmoment);
    }

    public void BFS()
    {
        for (int i = 0; i < rijmomenten.Count; i++)
        {
            rijmomenten[i] = rijmomenten[i].RijBFS(); //voelt onnodig als je gwn kan veranderen is handiger
        }
    }

    public string ToString(string i)
    { 
        string s = "";
        int count = 1;
        for (int j = 0; j < rijmomenten.Count; j++)
        {
          //LETOP hier moet nog iets gebeuren met welke bus er gebeurd
            (int c, string s2) = rijmomenten[j].ToString(i, count);
            count = c;
            s += s2;
        }
         return s;
    }
}