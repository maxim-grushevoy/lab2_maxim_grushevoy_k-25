namespace Lab2Library.Models;

/// <summary>Запис про видання: атрибути вузла book + читачі (вузли reader).</summary>
public class BookRecord
{
    public Dictionary<string, string> Attributes { get; } = new();
    public List<Dictionary<string, string>> Readers { get; } = new();
}

/// <summary>Параметри пошуку: ім'я атрибута -> шукане значення (хоча б один).</summary>
public class SearchCriteria
{
    public Dictionary<string, string> Values { get; } = new();
    public bool IsEmpty => Values.Count == 0;

    public static bool Match(string? actual, string wanted) =>
        actual != null &&
        actual.Contains(wanted, StringComparison.CurrentCultureIgnoreCase);
}
