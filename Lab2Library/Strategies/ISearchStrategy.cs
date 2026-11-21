using Lab2Library.Models;

namespace Lab2Library.Strategies;

/// <summary>Шаблон «Стратегія»: спільний інтерфейс алгоритмів аналізу XML.</summary>
public interface ISearchStrategy
{
    string Name { get; }
    List<BookRecord> Search(string filePath, SearchCriteria criteria);
}

/// <summary>Контекст стратегії.</summary>
public class SearchContext
{
    private ISearchStrategy _strategy;
    public SearchContext(ISearchStrategy strategy) => _strategy = strategy;
    public void SetStrategy(ISearchStrategy strategy) => _strategy = strategy;
    public string StrategyName => _strategy.Name;
    public List<BookRecord> Execute(string path, SearchCriteria c) => _strategy.Search(path, c);
}
