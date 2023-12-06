namespace rommelrouterakkers;


public class AfstandMatrix
{
    //we chose to ignore distance as this does not matter for calculating time
    public int[,] matrix;

    public AfstandMatrix(int[,] matrix1)
    {
        matrix = matrix1;
    }

    public int lookup(Bedrijf b1, Bedrijf b2)
    {
        return matrix[b1.matrixId, b2.matrixId];
    }
    
    public int this[int i, int j] // indexer, indien je geen bedrijven tot je beschikking hebt maar alleen IDs
    {
        get { return matrix[i, j]; }
    }

}


