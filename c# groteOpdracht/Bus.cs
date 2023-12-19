namespace rommelrouterakkers;
using System.Collections.Generic;
using System;

public class Bus
{
    public List<Rijmoment> rijmomenten = new List<Rijmoment>();
    public int tijd = 0;
    public Week week;
    public Dag dag;

    public Bus(Week werkWeek, Dag werkdag)
    {
        rijmomenten.Add(new Rijmoment(this));
        rijmomenten.Add(new Rijmoment(this));
        week = werkWeek;
        dag = werkdag;
    }

    public int Insert(Node nieuw, Random r)
    {
        if (rijmomenten.Count == 0) return int.MaxValue; // zorgt dat de hele actie gecanceld wordt als deze bus geen rijmoment heeft
        int welkMoment = r.Next(0, rijmomenten.Count);
        Rijmoment huidig = rijmomenten[welkMoment];
        if (huidig.volume + nieuw.bedrijf.volume > 100000) return int.MaxValue; // zorgt dat de hele actie gecanceld wordt als het niet past qua volume
        int extratijd = huidig.ExtraTijdskostenBijToevoegen(nieuw.bedrijf, huidig.eindnode.Previous, huidig.eindnode);
        
        huidig.ToevoegenVoor(nieuw, huidig.eindnode, extratijd);
        return tijd;
    }

    public bool InterRijmomentSwapCheck(int extratijd)
    {
        if (tijd + extratijd > 43200 * 1000) 
            return false;
        return true;
    }

    public void Load(Bedrijf b, bool stortIngelezen)
    {
        //als het stort is maak je gwn een nieuwe aan en stop je daarna 
        //ik heb het iets aangepast, hij moet pas een nieuwe aanmaken als er ook werkelijk een nieuw bedrijf wordt toegevoegd

        if (stortIngelezen)
        {
            Rijmoment temp = rijmomenten[0];
            rijmomenten[0] = rijmomenten[1];
            rijmomenten[1] = temp;
        }
        

        //als dat niet zo is pak het laatste rijmoment en voeg hem daar aan toe
        rijmomenten[0].Load(b);

    }

    public Rijmoment VoegRijmomentToe() 
    {
        Rijmoment nieuw = new Rijmoment(this);
        rijmomenten.Add(nieuw);
        return nieuw;
    }

    public void VerwijderLeegRijmoment(Rijmoment rijmoment) // kijken hoe we dit gaan doen, hoe access je het rijmoment en bus als je alleen de nodes hebt?
    {
        rijmomenten.Remove(rijmoment);
    } 

    public void BFS()
    {
        foreach (Rijmoment rijmoment in rijmomenten)
        { 
            rijmoment.RijBFS();
        }
    }

    public int Evaluate()
    {
        int k = 0;
        foreach(Rijmoment r in rijmomenten)
        {
           k += r.Evaluate();
        }
        return k;
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