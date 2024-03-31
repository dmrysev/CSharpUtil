namespace CSharpUtil.Core;

public class Seq {

public static string joinSeqComma<T> (IEnumerable<T> seq) => string.Join(", ", seq);
public static string joinSeqNewLine<T> (IEnumerable<T> seq) => string.Join("\n", seq);

}