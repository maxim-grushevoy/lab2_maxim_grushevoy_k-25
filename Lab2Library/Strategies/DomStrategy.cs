using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Lab2Library.Models;

namespace Lab2Library.Strategies;

/// <summary>DOM API: завантаження дерева XmlDocument і динамічно згенерований XPath-запит.</summary>
public class DomStrategy : ISearchStrategy
{
    public string Name => "DOM API";

    private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ";
    private const string Lower = "abcdefghijklmnopqrstuvwxyzабвгґдеєжзиіїйклмнопрстуфхцчшщьюя";

    public List<BookRecord> Search(string filePath, SearchCriteria criteria)
    {
        var doc = new XmlDocument();
        doc.Load(filePath);

        string xpath = BuildXPath(criteria);
        var result = new List<BookRecord>();
        var nodes = doc.SelectNodes(xpath);
        if (nodes == null) return result;

        foreach (XmlNode node in nodes)
        {
            var rec = new BookRecord();
            foreach (XmlAttribute a in node.Attributes!) rec.Attributes[a.Name] = a.Value;
            foreach (XmlNode r in node.SelectNodes("reader")!)
            {
                var d = new Dictionary<string, string>();
                foreach (XmlAttribute a in r.Attributes!) d[a.Name] = a.Value;
                rec.Readers.Add(d);
            }
            result.Add(rec);
        }
        return result;
    }

    /// <summary>//book[ (contains(@a,'x') or reader[contains(@a,'x')]) and ... ]</summary>
    private static string BuildXPath(SearchCriteria c)
    {
        var sb = new StringBuilder("/library/book");
        var parts = new List<string>();
        foreach (var kv in c.Values)
        {
            if (!Regex.IsMatch(kv.Key, @"^[\w.\-]+$")) continue; // захист від некоректних імен
            string val = Literal(kv.Value.ToLowerInvariant());
            string tr(string path) => $"contains(translate({path},'{Upper}','{Lower}'),{val})";
            parts.Add($"({tr("@" + kv.Key)} or reader[{tr("@" + kv.Key)}])");
        }
        if (parts.Count > 0) sb.Append('[').Append(string.Join(" and ", parts)).Append(']');
        return sb.ToString();
    }

    private static string Literal(string s)
    {
        if (!s.Contains('\'')) return $"'{s}'";
        if (!s.Contains('"')) return $"\"{s}\"";
        return "concat('" + s.Replace("'", "',\"'\",'") + "')";
    }
}
