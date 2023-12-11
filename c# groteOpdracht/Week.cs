namespace rommelrouterakkers;

using System;
using System.Collections.Generic;

public class Week 
{
    //index from 1 to 5, not 0 to 4
    public Dag[] dagen = new Dag[6];
    public int kosten = 0;

    public Week()
    {
        for (int i = 1; i <= 5; i++)
            dagen[i] = new Dag();
    }

    public void LeesKostenIn(List<Bedrijf> bedrijven)
    {
        foreach (Bedrijf bedrijf in bedrijven)
        {
            if (!bedrijf.wordtBezocht)
            {
                kosten += 3 * bedrijf.frequentie * bedrijf.ledigingsDuur;
            }
        }

        for (int i = 1; i <= 5; i++)
        {
            kosten += dagen[i].bussen[0].tijd;
            kosten += dagen[i].bussen[1].tijd;
        }
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
        b.wordtBezocht = false;
        
    }

    public void Bezocht(Bedrijf b, Random r)
    {
        if (b.wordtBezocht == true)
        {
            Delete(b);
        }
        else
        {
            Insert(b, r);


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
            case 3: AddDag3(b, r); break;
            case 4: AddDag4(b, r); break;
            default: break;
        }
        b.wordtBezocht = true;
    }
    public void AddDag1(Bedrijf b, Random r)
    {
        int dag = r.Next(1, 6);
        dagen[dag].Insert(b.Locaties[0], r);
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
        dagen[dag1].Insert(b.Locaties[0], r);
        dagen[dag2].Insert(b.Locaties[1], r);
    }
    public void AddDag3(Bedrijf b, Random r)
    {
        dagen[1].Insert(b.Locaties[0], r);
        dagen[3].Insert(b.Locaties[1], r);
        dagen[5].Insert(b.Locaties[2], r);

    }
    public void AddDag4(Bedrijf b, Random r)
    {
        int dag = r.Next(1, 6);
        int j;
        for (int i = 1; i < 6; i++)
        {
            if (dag != i)
            {
                j = dag > i ? i - 2 : i - 1;
                dagen[i].Insert(b.Locaties[j], r);
            }
        }
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


    public void BFS()
    {
        for(int i = 1; i<= 5; i++) 
        {
            dagen[i].BFS();
        }
    }

    
    public void Load(int dag, int bus, Bedrijf b)
    {
       dagen[dag].bussen[bus].Load(b);
    }
    
}