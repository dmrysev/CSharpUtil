using System.Collections;

namespace CSharpUtil.Core;

public static class IEnumerable {

public static int CountItem<T>  (this IEnumerable<T> seq, T item) where T : IEquatable<T> =>
    seq
    .Aggregate(0, (total, next) => next.Equals(item) ? total + 1 : total);
    
public static bool IsEmpty<T>(this IEnumerable<T> list)
{
    if (list == null)
    {
        throw new ArgumentNullException("list");
    }

    var genericCollection = list as ICollection<T>;
    if (genericCollection != null)
    {
        return genericCollection.Count == 0;
    }

    var nonGenericCollection = list as ICollection;
    if (nonGenericCollection != null)
    {
        return nonGenericCollection.Count == 0;
    }

    return !list.Any();
}

public static IEnumerable<T[]> Windowed<T>(this IEnumerable<T> list, int windowSize)
{
    //Checks elided
    var arr = new T[windowSize];
    int r = windowSize - 1, i = 0;
    using(var e = list.GetEnumerator())
    {
        while(e.MoveNext())
        {
            arr[i] = e.Current;
            i = (i + 1) % windowSize;
            if(r == 0) 
                yield return ArrayInit<T>(windowSize, j => arr[(i + j) % windowSize]);
            else
                r = r - 1;
        }
    }
}
public static T[] ArrayInit<T>(int size, Func<int, T> func)
{
    var output = new T[size];
    for(var i = 0; i < size; i++) output[i] = func(i);
    return output;
}

}