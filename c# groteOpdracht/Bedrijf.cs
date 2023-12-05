namespace rommelrouterakkers;

public class Bedrijf
{
    //Order;Plaats;Frequentie;AantContainers;VolumePerContainer;LedigingsDuurMinuten;MatrixID;XCoordinaat;YCoordinaat
    public int orderNummer;
    public int frequentie;
    public int volume; //totale volume niet volume per container
    public int matrixId;
    public float ledigingsDuur;

    //gwn om ff snel dingen aan te maken voor schrijven van code
    public Bedrijf()
    {
    }

    public Bedrijf(int ord, int f, int v, int aantalBakken, int mId, float ledD)
    {
        orderNummer = ord;
        frequentie = f;
        volume = v * aantalBakken;
        matrixId = mId;
        ledigingsDuur = ledD;
    }

    //int ord, int f, int v, int aantalBakken, int mId, float ledD
    //Order;Plaats;Frequentie;AantContainers;VolumePerContainer;LedigingsDuurMinuten;MatrixID;XCoordinaat;YCoordinaat
    public static Bedrijf parseBedrijf(string s)
    {
        char separator = ';';
        string[] list = s.Split(separator);

        int ord = int.Parse(list[0]);

        int f = int.Parse(list[2].Substring(0, 1));
        int aantalBakken = int.Parse(list[3]);
        int v = int.Parse(list[4]);
        float ledD = float.Parse(list[5]);
        int mId = int.Parse(list[6]);

        return new Bedrijf(ord, f, v, aantalBakken, mId, ledD);
    }
}
