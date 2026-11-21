namespace Lab2Library.Models;

/// <summary>Українські назви атрибутів XML для інтерфейсу.</summary>
public static class AttributeLabels
{
    private static readonly Dictionary<string, string> Map = new()
    {
        ["id"] = "Номер видання",
        ["author"] = "Автор",
        ["title"] = "Назва",
        ["annotation"] = "Анотація",
        ["qualification"] = "Кваліфікаційна ознака",
        ["year"] = "Рік видання",
        ["isbn"] = "ISBN",
        ["fio"] = "П.І.П. читача",
        ["faculty"] = "Факультет",
        ["department"] = "Кафедра",
        ["position"] = "Посада",
    };

    public static string Get(string name) => Map.TryGetValue(name, out var v) ? v : name;
}
