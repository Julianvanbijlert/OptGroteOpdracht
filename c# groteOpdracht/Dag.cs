namespace rommelrouterakkers;

public class Dag
{
    public Bus[] bussen;
    public int getal; // hoeveelste dag in de week is dit

    public Dag(Week werkWeek, int i)
    {
        bussen = new Bus[2];
        bussen[0] = new Bus(werkWeek, this);
        bussen[1] = new Bus(werkWeek, this);
        getal = i;
    }

    public static bool InterBusSwapCheck(Node node1, Node node2, int extratijd1, int extratijd2) // Controleer of de nodes tussen de bussen geswapt mogen worden qua tijd
    {
        if (node1.rijmoment.bus.tijd + extratijd1 > 43200 * 1000 ||
            node2.rijmoment.bus.tijd + extratijd2 > 43200 * 1000)
            return false;
        return true;
    }

    public string ToString(string i)
    {
        string s = "";

        s += bussen[0].ToString("1;" + i + ";");
        s += bussen[1].ToString("2;" + i + ";");

        return s;
    }

    public void BFS()
    {
        foreach (var bus in bussen)
        {
            bus.BFS();
        }
    }
}
