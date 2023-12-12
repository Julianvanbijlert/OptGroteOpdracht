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
        week.BFS();
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

// willen we anders eerst op klein niveau optimaliseren (alleen wisselen/toevoegen/verwijderen)
// en dan als dat niks oplevert naar inter rijmoment shit? zie dat variable neighbourhood descent(?) ofzo in powerpoint
// of variable neighbourhood search ofzo
// dan zitten we niet te kutten met verhoudingen

// zorgen dat na acties alle tijden goed worden aangepast (bustijden, totale kosten)

// drie niveaus: eerst zoveel mogelijk swappen. lang niks nieuws? bedrijven toevoegen/ verwidjeren. lang niks nieuws? rijmoment toevoegen/verwijderen

// hashtabel met ordernummers -> bedrijven? dan gaat dat tenminste in O(1)
