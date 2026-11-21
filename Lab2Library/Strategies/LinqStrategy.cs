using System.Xml.Linq;
using Lab2Library.Models;

namespace Lab2Library.Strategies;

/// <summary>LINQ to XML: динамічна побудова запиту за введеними критеріями.</summary>
public class LinqStrategy : ISearchStrategy
{
    public string Name => "LINQ to XML";

    public List<BookRecord> Search(string filePath, SearchCriteria criteria)
    {
        XDocument doc = XDocument.Load(filePath);
        IEnumerable<XElement> query = doc.Root!.Elements("book");

        // динамічно додаємо умову Where для кожного заданого атрибута
        foreach (var kv in criteria.Values)
        {
            string name = kv.Key, wanted = kv.Value;
            query = query.Where(b =>
                SearchCriteria.Match((string?)b.Attribute(name), wanted) ||
                b.Elements("reader").Any(r => SearchCriteria.Match((string?)r.Attribute(name), wanted)));
        }

        return query.Select(b =>
        {
            var rec = new BookRecord();
            foreach (var a in b.Attributes()) rec.Attributes[a.Name.LocalName] = a.Value;
            foreach (var r in b.Elements("reader"))
                rec.Readers.Add(r.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value));
            return rec;
        }).ToList();
    }
}
