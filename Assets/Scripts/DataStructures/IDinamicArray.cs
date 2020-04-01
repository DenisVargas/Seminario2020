public interface IDinamicArray<T>
{
    void Add(T element);
    void Insert(T element, int index);

    int IndexOf(T element);
    bool Contains(T element);

    void Remove(T element);
    void RemoveAt(int index);
    void Clear();
}