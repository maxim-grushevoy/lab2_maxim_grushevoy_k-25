using System.Xml;
using Lab2Library.Models;

namespace Lab2Library.Strategies;

/// <summary>SAX-подібний потоковий аналіз (XmlReader — forward-only, без побудови дерева).</summary>
public class SaxStrategy : ISearchStrategy
{
    public string Name => "SAX API";

    public List<BookRecord> Search(string filePath, SearchCriteria criteria)
    {
        var result = new List<BookRecord>();
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            IgnoreComments = true,
            IgnoreWhitespace = true
        };
        BookRecord? current = null;

        using var xr = XmlReader.Create(filePath, settings);
        while (xr.Read())
        {
            if (xr.NodeType == XmlNodeType.Element)
            {
                if (xr.Name == "book")
                {
                    current = new BookRecord();
                    ReadAttributes(xr, current.Attributes);
                    if (xr.IsEmptyElement)          // <book .../> без дочірніх вузлів
                    {
                        if (IsMatch(current, criteria)) result.Add(current);
                        current = null;
                    }
                }
                else if (xr.Name == "reader" && current != null)
                {
                    var d = new Dictionary<string, string>();
                    ReadAttributes(xr, d);
                    current.Readers.Add(d);
                }
            }
            else if (xr.NodeType == XmlNodeType.EndElement && xr.Name == "book" && current != null)
            {
                if (IsMatch(current, criteria)) result.Add(current);
                current = null;
            }
        }
        return result;
    }

    private static void ReadAttributes(XmlReader xr, Dictionary<string, string> target)
    {
        if (!xr.HasAttributes) return;
        while (xr.MoveToNextAttribute())
            target[xr.Name] = xr.Value;
        xr.MoveToElement();
    }

    private static bool IsMatch(BookRecord b, SearchCriteria c) =>
        c.Values.All(kv =>
            (b.Attributes.TryGetValue(kv.Key, out var v) && SearchCriteria.Match(v, kv.Value)) ||
            b.Readers.Any(r => r.TryGetValue(kv.Key, out var rv) && SearchCriteria.Match(rv, kv.Value)));
}
