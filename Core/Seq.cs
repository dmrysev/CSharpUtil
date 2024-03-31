namespace CSharpUtil.Core;

class Seq {

public static string joinSeqComma<T> (IEnumerable<T> seq) {
    return string.Join(", ", seq);
}

public static string joinSeqNewLine<T> (IEnumerable<T> seq) {
    return string.Join("\n", seq);
}

}