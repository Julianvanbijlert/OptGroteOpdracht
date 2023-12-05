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


}


