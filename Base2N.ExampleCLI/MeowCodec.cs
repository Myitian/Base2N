using System.Collections.Frozen;

namespace Base2N.ExampleCLI;

static class MeowCodec
{
    public const char TildeMark = '～';
    public const char PeriodMark = '。';
    public const char CommaMark = '，';
    public const char SemicolonMark = '；';
    public const char PauseMark = '、';
    public const char ExclamationMark = '！';
    public const char QuestionMark = '？';

    public const char Miao = '喵';
    public const string Meow = "Meow";
    public const string Miau = "Miau";
    public const string Nya = "Nya";

    internal static string[] PhraseSuffixList { get; } = [
    /*
     * 16
     * (
     *   | none         | meow        |nya         | miau       |
     *   | meow+meow    | meow+nya    |nya+meow    | nya+nya    |
     *   | meow+meow+喵 | meow+nya+喵 |nya+meow+喵 | nya+nya+喵 |
     *   | meow+喵      | meow+喵喵   |nya+喵      | nya+喵喵   |
     * )TildeMark
     */
        $"{TildeMark}", $"{Meow}{TildeMark}", $"{Nya}{TildeMark}", $"{Miau}{TildeMark}",
        $"{Meow}{Meow}{TildeMark}", $"{Meow}{Nya}{TildeMark}", $"{Nya}{Meow}{TildeMark}", $"{Nya}{Nya}{TildeMark}",
        $"{Meow}{Meow}{Miao}{TildeMark}", $"{Meow}{Nya}{Miao}{TildeMark}", $"{Nya}{Meow}{Miao}{TildeMark}", $"{Nya}{Nya}{Miao}{TildeMark}",
        $"{Meow}{Miao}{TildeMark}", $"{Meow}{Miao}{Miao}{TildeMark}", $"{Nya}{Miao}{TildeMark}", $"{Nya}{Miao}{Miao}{TildeMark}",
    /*
     * 16
     * (
     *   | none         | meow        |nya         | miau       |
     *   | meow+meow    | meow+nya    |nya+meow    | nya+nya    |
     *   | meow+meow+喵 | meow+nya+喵 |nya+meow+喵 | nya+nya+喵 |
     *   | meow+喵      | meow+喵喵   |nya+喵      | nya+喵喵   |
     * )CommaMark
     */
        $"{CommaMark}", $"{Meow}{CommaMark}", $"{Nya}{CommaMark}", $"{Miau}{CommaMark}",
        $"{Meow}{Meow}{CommaMark}", $"{Meow}{Nya}{CommaMark}", $"{Nya}{Meow}{CommaMark}", $"{Nya}{Nya}{CommaMark}",
        $"{Meow}{Meow}{Miao}{CommaMark}", $"{Meow}{Nya}{Miao}{CommaMark}", $"{Nya}{Meow}{Miao}{CommaMark}", $"{Nya}{Nya}{Miao}{CommaMark}",
        $"{Meow}{Miao}{CommaMark}", $"{Meow}{Miao}{Miao}{CommaMark}", $"{Nya}{Miao}{CommaMark}", $"{Nya}{Miao}{Miao}{CommaMark}",
    /*
     * 8
     * (
     *   | none    | meow      |nya    | nya+nya  |
     *   | meow+喵 | meow+喵喵 |nya+喵 | nya+喵喵 |
     * )PauseMark
     */
        $"{PauseMark}", $"{Meow}{PauseMark}", $"{Nya}{PauseMark}", $"{Nya}{Nya}{PauseMark}",
        $"{Meow}{Miao}{PauseMark}", $"{Meow}{Miao}{Miao}{PauseMark}", $"{Nya}{Miao}{PauseMark}", $"{Nya}{Miao}{Miao}{PauseMark}",
    /*
     * 8
     * (
     *   | none    | meow      |nya    | nya+nya  |
     *   | meow+喵 | meow+喵喵 |nya+喵 | nya+喵喵 |
     * )PeriodMark
     */
        $"{PeriodMark}", $"{Meow}{PeriodMark}", $"{Nya}{PeriodMark}", $"{Nya}{Nya}{PeriodMark}",
        $"{Meow}{Miao}{PeriodMark}", $"{Meow}{Miao}{Miao}{PeriodMark}", $"{Nya}{Miao}{PeriodMark}", $"{Nya}{Miao}{Miao}{PeriodMark}",
    /*
     * 8
     * (
     *   | none    | meow        |nya    | nya+nya  |
     *   | miau    | meow+nya+喵 |nya+喵 | nya+喵喵 |
     * )ExclamationMark
     */
        $"{ExclamationMark}", $"{Meow}{ExclamationMark}", $"{Nya}{ExclamationMark}", $"{Nya}{Nya}{ExclamationMark}",
        $"{Miau}{ExclamationMark}", $"{Meow}{Nya}{Miao}{ExclamationMark}", $"{Nya}{Miao}{ExclamationMark}", $"{Nya}{Miao}{Miao}{ExclamationMark}",
    /*
     * 4
     * (
     *   | none | meow |nya | nya+喵 |
     * )SemicolonMark
     */
        $"{SemicolonMark}", $"{Meow}{SemicolonMark}", $"{Nya}{SemicolonMark}", $"{Nya}{Miao}{SemicolonMark}",
    /*
     * 4
     * (
     *   | none | meow |nya | nya+喵 |
     * )QuestionMark
     */
        $"{QuestionMark}", $"{Meow}{QuestionMark}", $"{Nya}{QuestionMark}", $"{Nya}{Miao}{QuestionMark}"
        ];
    public static FrozenDictionary<string, int> PhraseSuffixMap { get; } = PhraseSuffixList
        .Select(MakeKeyValuePair)
        .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    public static int MaxSuffixLength { get; } = PhraseSuffixList.Max(s => s.Length);
    private static KeyValuePair<TKey, TValue> MakeKeyValuePair<TKey, TValue>(TKey key, TValue value)
        => new(key, value);
}