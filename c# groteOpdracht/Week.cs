namespace rommelrouterakkers;

public class Week
{
    //index from 1 to 5, not 0 to 4
    public Dag[] dagen = new Dag[6];

    public Week()
    {

    }

    public void Insert(Bedrijf b)
    {
        b.wordtBezocht = true;
        int i = b.frequentie;

        DoeInDagen(i, b);

    }

    public void DoeInDagen(int i, Bedrijf b)
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