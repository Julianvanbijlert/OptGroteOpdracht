namespace rommelrouterakkers;


public class AfstandMatrix
{
    //we chose to ignore distance as this does not matter for calculating time
    //so actually it's a driving time matrix
    public int[,] matrix;

    public AfstandMatrix(int[,] matrix1)
    {
        matrix = matrix1;
    }

    public int lookup(Bedrijf b1, Bedrijf b2)
    {
        return matrix[b1.matrixId, b2.matrixId];
    }

    public int lookup(Node n1, Node n2)
    {
        return lookup(n1.bedrijf, n2.bedrijf);
    }
    
    public int this[int i, int j] // indexer
    {
        get { return matrix[i, j]; }
    }

}


