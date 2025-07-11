using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority)
                bestIndex = i;
        }

        var bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public bool Contains(T item)
    {
        return elements.Any(e => EqualityComparer<T>.Default.Equals(e.item, item));
    }
}
