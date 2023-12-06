using System.IO;

namespace rommelrouterakkers;
using System.Collections.Generic;
public class Dag
{
    public List<Rijmoment> rijmomenten;
    public Queue<Bedrijf> todoLijst;
    


    public Dag()
    {
        rijmomenten = new List<Rijmoment>();
        todoLijst = new Queue<Bedrijf>();
    }

    //maakt van de lijst twee bussen zijn 
    public void splitsDag()
    {

    }

    public void Insert(Bedrijf b)
    {

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
