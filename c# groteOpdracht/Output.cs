namespace rommelrouterakkers;

public class Output
{
    //VrachtwagenNummer ; Dagnummer ; hoeveelste adres ; id van dat adres (odernummer?) afstorten is 0

    //loads solution from file, should return a "week"
    public static void loadSolution(string fileNaam)
    {

    }

    public void printSolution()
    {
        /*
         * 1.      Vrachtautonummer (1 of 2)
           2.      Dagnummer (maandag =1, dinsdag =2, …, vrijdag =5)
           3.      Hoeveelste adres dat het voertuig op die dag aandoet (begin met 1, 2, …)
           4.      Id. van het adres (uit orderbestand.txt); de stort heeft nummer 0.

           1; 1; 1; 10
           1; 1; 2; 20
           1; 1; 3; 0
           1; 1; 4; 30
         */
    }

    public void printSolutionToFile()
    {
        //pak de beste solution variabele
        //schrijf die naar de file met pad scoreFile
    }
}