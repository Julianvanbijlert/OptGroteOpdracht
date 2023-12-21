namespace rommelrouterakkers;
using System.Collections.Generic;

public class Bus
{
    public List<Rijmoment> rijmomenten = new List<Rijmoment>();
    public int tijd = 0;
    public Dag dag;
    public Week week;

    public Bus(Week werkWeek, Dag werkdag)
    {
        rijmomenten.Add(new Rijmoment(this)); // voeg vast 2 rijmomenten toe
        rijmomenten.Add(new Rijmoment(this));
        week = werkWeek;
        dag = werkdag;
    }

    public void Load(Bedrijf b, bool stortIngelezen) // lees het bedrijf in
    {
        if (stortIngelezen) //als stort is ingelezen, switch de rijmomenten, zodat rijmomenten[0] nu het tweede rijmoment is
        {
            Rijmoment temp = rijmomenten[0];
            rijmomenten[0] = rijmomenten[1];
            rijmomenten[1] = temp;
        }
      
        //als dat niet zo is pak het eerste rijmoment en voeg hem daar aan toe
        rijmomenten[0].Load(b);
    }

    public void BFS()
    {
        foreach (Rijmoment rijmoment in rijmomenten)
        { 
            rijmoment.RijBFS();
        }
    }

    public string ToString(string i) // maak een string van alle (2) rijmomenten
    { 
        string s = "";
        string s2;
        int count = 1; // count zorgt ervoor dat het hoeveelste bedrijf dat een bus op een dag bezoekt niet reset bij het ToStringen van een volgend rijmoment
        for (int j = 0; j < rijmomenten.Count; j++)
        {
            if (rijmomenten[j].Count == 0)
                continue;
            (count, s2) = rijmomenten[j].ToString(i, count);
            s += s2;
        }
        return s;
    }
}