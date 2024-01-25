namespace rommelrouterakkers;
public class Node
{
    public Node Previous = null;
    public Node Next = null;
    public Bedrijf bedrijf;
    public Rijmoment rijmoment;
    public int Plaats; // op de hoeveelste plek in de lijst met nodes van het rijmoment staat deze node
                       // op deze manier kunnen we in O(1) tijd vinden waar een node in de lijst staat,
                       // waarna we in O(1) tijd (door onze eigen array) de node kunnen verwijderen

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
        extra -= Setup.aMatrix.lookup(Previous, this); // haal de rijtijd van de vorige naar deze node eraf
        extra -= Setup.aMatrix.lookup(this, Next); // haal de rijtijd van deze naar de volgende eraf
        extra += Setup.aMatrix.lookup(Previous, Next); // tel de rijtijd van de vorige naar de volgende erbij op
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
