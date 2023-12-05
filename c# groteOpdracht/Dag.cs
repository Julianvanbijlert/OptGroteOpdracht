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

    //string is welke dag het is
    public string ToString(string i)
    {

        return " ";

        //iets om aan te gevan dat de dag voorbij is
    }
}
