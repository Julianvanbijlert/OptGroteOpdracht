namespace rommelrouterakkers;
public class EigenArray<T> // onze eigen array
{
    public int Count;
    public T[] elems;

    public EigenArray()
    {
        Count = 0;
        elems = new T[1177];
    }

    public void Add(T elem)
    {
        elems[Count] = elem;
        Count++;
    }

    public void RemoveAt(int i) // hierdoor is RemoveAt O(1)
    {
        elems[i] = elems[--Count];
    }

    public T this[int i]
    {
        get { return elems[i]; }
    }
}

