using System.Runtime.InteropServices;

namespace rommelrouterakkers;

public class ZoekAlgoritme
{
    private Week week;
    public ZoekAlgoritme(Week w)
    {
        week = w;
    }

    public void BFS()
    {
        for (int i = 1; i <= 5; i++)//Dag d in week.dagen)
        {
            Dag d = week.dagen[i];
            for (int j = 0; j <= 1; j++)
            {
                Bus b = d.bussen[j];
                for (int k = 0; k < b.rijmomenten.Count; k++)
                {
                    b.rijmomenten[k].RijBFS();
                }
                //miss moet je de variabele elke keer toekennen aan de plek, maar weet niet
            }
        }
    }
}

public class ILS
{
    private Week week;
    public ILS(Week w)
    {
        week = w;
    }

   


    
}


/*
 action a
-Delete
-Add
-Swap
 
 
 */

