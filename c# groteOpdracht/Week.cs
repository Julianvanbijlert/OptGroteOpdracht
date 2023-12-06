namespace rommelrouterakkers;

using System;
using System.Collections.Generic;

public class Week
{
    //index from 1 to 5, not 0 to 4
    public Dag[] dagen = new Dag[6];

    public Week()
    {
        for (int i = 1; i <= 5; i++)
            dagen[i] = new Dag();
    }

    public void Pick(Bedrijf b, Random r)
    {
        if (b.wordtBezocht)
        {
            Delete(b);
        }
        else
        {
            Insert(b, r);
        }

        b.wordtBezocht = !b.wordtBezocht;
    }

    public void Delete(Bedrijf b)
    {
        foreach (Node n in b.Locaties)
        {
            n.Verwijder();
        }
        
    }

    public void Insert(Bedrijf b, Random r)
    {
        DoeInDagen(b, r);

    }
    public void DoeInDagen(Bedrijf b, Random r)
    {
        switch (b.frequentie)
        {
            case 1: AddDag1(b, r);break;
            case 2: AddDag2(b,r); break;
            case 3: AddDag3(b); break;
            case 4: AddDag4(b, r); break;
            default: break;
        }      
    }
    public void AddDag1(Bedrijf b, Random r)
    {
        int dag = r.Next(1, 6);
        dagen[dag].Insert(b);
    }
    public void AddDag2(Bedrijf b, Random r)
    {
        int dag1, dag2;
        int welke = r.Next(0, 2);

        if (welke == 0)
        {
            dag1 = 1;
            dag2 = 4;
        }
        else
        {
            dag1 = 2; 
            dag2 = 5;
        }
        dagen[dag1].Insert(b);
        dagen[dag2].Insert(b);
    }
    public void AddDag3(Bedrijf b)
    {
        dagen[1].Insert(b);
        dagen[3].Insert(b);
        dagen[5].Insert(b);

    }
    public void AddDag4(Bedrijf b, Random r)
    {
        int dag = r.Next(1, 6);

        for (int i = 1; i < 6; i++)
        {
            if (dag != i)
            {
                dagen[i].Insert(b);
            }
        }
    }
    public string ToString()
    {
        string s = "";
        for(int i = 1; i <= 5 ; i++)
        {
            s += dagen[i].ToString(i.ToString());
            s += "\n";
        }
        return s;
    }
    
}