namespace rommelrouterakkers;


public class AfstandMatrix
{
    //we hebben ervoor gekozen om de afstanden niet op te slaan, alleen de rijtijden.
    //afstanden hebben immers geen invloed op de kosten. het is dus eigenlijk een rijtijdenmatrix
    public int[,] matrix;

    public AfstandMatrix(int[,] matrix1)
    {
        matrix = matrix1;
    }

    public int lookup(Bedrijf b1, Bedrijf b2) // geef de rijtijd gegeven 2 bedrijven
    {
        return matrix[b1.matrixId, b2.matrixId];
    }

    public int lookup(Node n1, Node n2) // geef de rijtijd gegeven 2 nodes
    {
        return lookup(n1.bedrijf, n2.bedrijf);
    }
    
    public int this[int i, int j] // indexer
    {
        get { return matrix[i, j]; }
    }

}


