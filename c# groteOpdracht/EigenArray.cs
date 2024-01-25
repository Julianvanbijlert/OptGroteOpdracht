namespace rommelrouterakkers;
public class EigenArray<T>
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

    public void RemoveAt(int i)
    {
        elems[i] = elems[--Count];
    }

    public T this[int i]
    {
        get { return elems[i]; }
    }
}

