namespace rommelrouterakkers;
public class Node
{
    public Node Previous = null;
    public Node Next = null;
    public Bedrijf bedrijf;
    public Rijmoment rijmoment;

    public Node(Bedrijf bedrijf)
    {
        this.bedrijf = bedrijf;
    }

    public void Verwijder(int extratijd)
    {
        rijmoment.Verwijderen(this, extratijd);
    }

    public int ExtraTijdskostenBijVerwijderen() // bereken de incrementele tijdskosten die ontstaan bij verwijderen van deze node
    {
        int extra = 0;
        extra -= Setup.aMatrix.lookup(Previous, this);
        extra -= Setup.aMatrix.lookup(this, Next);
        extra += Setup.aMatrix.lookup(Previous, Next);
        extra -= bedrijf.ledigingsDuur;

        if (Previous == rijmoment.beginnode && Next == rijmoment.eindnode) // als het rijmoment nu leeg is, haal de stortkosten ervanaf
        {
            extra -= 1800 * 1000;
        }

        return extra;
    }

    public string ToString(string str)
    {
        return str + ";" + bedrijf.orderNummer.ToString() + "\n";
    }

}
