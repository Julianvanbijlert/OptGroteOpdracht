namespace rommelrouterakkers;
//alkdahskjfh

public class Week
{
    //index from 1 to 5, not 0 to 4
    public Dag[] dagen = new Dag[6];

    public Week()
    {

    }

    public string ToString()
    {
        string s = "";
        for(int i = 1; i <= 5 ; i++)
        {
            s += dagen[i].ToString(i.ToString());
        }
        return s;
    }

    
}