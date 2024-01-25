namespace rommelrouterakkers;
using System.Collections.Generic;

public class Bus
{
    public List<Rijmoment> rijmomenten = new List<Rijmoment>();
    public int tijd = 0;
    public Dag dag;
    public Week week;
    public int welkRijmomentVoorLoaden = 0;

    public Bus(Week werkWeek, Dag werkdag)
    {
        rijmomenten.Add(new Rijmoment(this)); // voeg vast 2 rijmomenten toe
        rijmomenten.Add(new Rijmoment(this));
        week = werkWeek;
        dag = werkdag;
    }

    public void Load(Bedrijf b, bool stortIngelezen) // lees het bedrijf in
    {
        if (stortIngelezen && rijmomenten[0].nodeLijst.Count != 0) //als stort is ingelezen, switch naar het tweede rijmoment, tenminste,
                                                                   //als het eerste rijmoment al gevuld was en het dus geen true van de vorige bus was
            welkRijmomentVoorLoaden = 1;
      
        rijmomenten[welkRijmomentVoorLoaden].Load(b);
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
            if (rijmomenten[j].beginnode.Next == rijmomenten[j].eindnode)
                continue;
            (count, s2) = rijmomenten[j].ToString(i, count);
            s += s2;
        }
        return s;
    }
}