using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Text;
using Lab2Library.Models;

namespace Lab2Library.Services;

/// <summary>Підгрузка даних: які атрибути реально присутні у файлі.</summary>
public static class AttributeScanner
{
    public static List<string> Scan(string path)
    {
        var names = new SortedSet<string>();
        var s = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, IgnoreComments = true };
        using var xr = XmlReader.Create(path, s);
        while (xr.Read())
        {
            if (xr.NodeType == XmlNodeType.Element && (xr.Name == "book" || xr.Name == "reader") && xr.HasAttributes)
            {
                while (xr.MoveToNextAttribute()) names.Add(xr.Name);
                xr.MoveToElement();
            }
        }
        return names.ToList();
    }
}

public static class ResultFormatter
{
    public static string ToText(List<BookRecord> records)
    {
        if (records.Count == 0) return "Нічого не знайдено.";
        var sb = new StringBuilder($"Знайдено записів: {records.Count}\n\n");
        int i = 1;
        foreach (var b in records)
        {
            sb.AppendLine($"{i++}. " + string.Join("; ", b.Attributes.Select(a => $"{AttributeLabels.Get(a.Key)}: {a.Value}")));
            foreach (var r in b.Readers)
                sb.AppendLine("    ↳ читач: " + string.Join("; ", r.Select(a => $"{AttributeLabels.Get(a.Key)}: {a.Value}")));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public static XDocument ToXml(List<BookRecord> records) =>
        new(new XElement("library",
            records.Select(b => new XElement("book",
                b.Attributes.Select(a => new XAttribute(a.Key, a.Value)),
                b.Readers.Select(r => new XElement("reader", r.Select(a => new XAttribute(a.Key, a.Value))))))));
}

public static class HtmlTransformer
{
    /// <summary>XML (результат пошуку) -> HTML за допомогою Library.xsl.</summary>
    public static async Task<string> TransformAsync(XDocument source)
    {
        using var xslStream = await FileSystem.OpenAppPackageFileAsync("Library.xsl");
        using var xslReader = XmlReader.Create(xslStream);
        var xslt = new XslCompiledTransform();
        xslt.Load(xslReader);

        using var sw = new StringWriter();
        using (var xw = XmlWriter.Create(sw, xslt.OutputSettings))
        using (var xr = source.CreateReader())
            xslt.Transform(xr, xw);
        return sw.ToString();
    }
}
