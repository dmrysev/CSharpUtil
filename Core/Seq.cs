namespace CSharpUtil.Core;

public static class IEnumerable {

public static string JoinByComma<T> (this IEnumerable<T> seq) => string.Join(", ", seq);
public static string JoinByNewLine<T> (this IEnumerable<T> seq) => string.Join("\n", seq);
public static int CountItem<T>  (this IEnumerable<T> seq, T item) where T : IEquatable<T> =>
    seq
    .Aggregate(0, (total, next) => next.Equals(item) ? total + 1 : total);

}