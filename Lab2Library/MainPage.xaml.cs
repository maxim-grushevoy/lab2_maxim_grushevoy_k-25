using Lab2Library.Models;
using Lab2Library.Services;
using Lab2Library.Strategies;

namespace Lab2Library;

public partial class MainPage : ContentPage
{
    private readonly Dictionary<string, ISearchStrategy> _strategies = new()
    {
        ["SAX API"] = new SaxStrategy(),
        ["DOM API"] = new DomStrategy(),
        ["LINQ to XML"] = new LinqStrategy(),
    };
    private readonly SearchContext _context;
    private readonly Dictionary<string, (CheckBox Check, Entry Value)> _criteriaControls = new();
    private string? _filePath;
    private string? _lastHtml;

    public MainPage()
    {
        InitializeComponent();
        if (Application.Current != null) Application.Current.UserAppTheme = AppTheme.Light;
        StrategyPicker.ItemsSource = _strategies.Keys.ToList();
        StrategyPicker.SelectedIndex = 0;
        _context = new SearchContext(_strategies["SAX API"]);
    }

    private void OnStrategyChanged(object? sender, EventArgs e)
    {
        if (StrategyPicker.SelectedItem is string name && _context != null)
            _context.SetStrategy(_strategies[name]);
    }

    // ---------- вибір файлу та "підгрузка даних" ----------
    private async void OnPickFileClicked(object? sender, EventArgs e)
    {
        try
        {
            var types = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".xml" } },
                { DevicePlatform.Android, new[] { "text/xml", "application/xml" } },
                { DevicePlatform.iOS, new[] { "public.xml" } },
                { DevicePlatform.MacCatalyst, new[] { "public.xml" } },
            });
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть XML-файл",
                FileTypes = types
            });
            if (file == null) return;

            var attrs = AttributeScanner.Scan(file.FullPath);   // які атрибути присутні у файлі
            _filePath = file.FullPath;
            FileLabel.Text = file.FileName;
            BuildCriteriaPanel(attrs);
            OutputEditor.Text = string.Empty;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", "Не вдалося прочитати файл: " + ex.Message, "OK");
        }
    }

    private void BuildCriteriaPanel(IEnumerable<string> attributes)
    {
        CriteriaPanel.Children.Clear();
        _criteriaControls.Clear();
        foreach (var name in attributes)
        {
            var check = new CheckBox { VerticalOptions = LayoutOptions.Center };
            var entry = new Entry { Placeholder = "значення", VerticalOptions = LayoutOptions.Center };
            var label = new Label
            {
                Text = AttributeLabels.Get(name),
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap
            };
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(120)),
                new ColumnDefinition(GridLength.Star)),
                ColumnSpacing = 6
            };
            row.Add(check, 0, 0);
            row.Add(label, 1, 0);
            row.Add(entry, 2, 0);
            CriteriaPanel.Children.Add(row);
            _criteriaControls[name] = (check, entry);
        }
    }

    // ---------- пошук ----------
    private async void OnSearchClicked(object? sender, EventArgs e)
    {
        if (_filePath == null)
        {
            await DisplayAlertAsync("Увага", "Спочатку оберіть XML-файл.", "OK");
            return;
        }

        var criteria = new SearchCriteria();
        foreach (var (name, ctl) in _criteriaControls)
            if (ctl.Check.IsChecked && !string.IsNullOrWhiteSpace(ctl.Value.Text))
                criteria.Values[name] = ctl.Value.Text.Trim();

        if (criteria.IsEmpty)
        {
            await DisplayAlertAsync("Увага", "Позначте хоча б один атрибут і введіть значення.", "OK");
            return;
        }

        try
        {
            var records = _context.Execute(_filePath, criteria);
            OutputEditor.Text = $"Метод: {_context.StrategyName}\n\n" + ResultFormatter.ToText(records);

            // трансформація результату в HTML за допомогою XSL
            string html = await HtmlTransformer.TransformAsync(ResultFormatter.ToXml(records));
            _lastHtml = html;
            SaveButton.IsEnabled = true;
            HtmlView.Source = new HtmlWebViewSource { Html = html };
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", ex.Message, "OK");
        }
    }

    // ---------- Clear ----------
    private void OnClearClicked(object? sender, EventArgs e)
    {
        OutputEditor.Text = string.Empty;
        HtmlView.Source = new HtmlWebViewSource { Html = "<html><body></body></html>" };
        foreach (var (_, ctl) in _criteriaControls)
        {
            ctl.Check.IsChecked = false;
            ctl.Value.Text = string.Empty;
        }
        _lastHtml = null;
        SaveButton.IsEnabled = false;
    }

    // ---------- Вихід з підтвердженням ----------
    private async void OnExitClicked(object? sender, EventArgs e)
    {
        bool yes = await DisplayAlertAsync("Вихід",
            "Чи дійсно Ви хочете завершити роботу з програмою?", "Так", "Ні");
        if (yes) Application.Current?.Quit();
    }
    private async void OnSaveHtmlClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_lastHtml))
        {
            await DisplayAlertAsync("Увага", "Спочатку виконайте пошук.", "OK");
            return;
        }
        try
        {
    #if WINDOWS
            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedFileName = "result",
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeChoices.Add("HTML-файл", new List<string> { ".html" });

            var window = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView
                         as Microsoft.UI.Xaml.Window;
            WinRT.Interop.InitializeWithWindow.Initialize(picker,
                WinRT.Interop.WindowNative.GetWindowHandle(window));

            var file = await picker.PickSaveFileAsync();
            if (file == null) return;               // користувач натиснув «Скасувати»
            await Windows.Storage.FileIO.WriteTextAsync(file, _lastHtml);
            await DisplayAlertAsync("Готово", "Файл збережено:\n" + file.Path, "OK");
    #else
        string path = Path.Combine(FileSystem.AppDataDirectory, "result.html");
        await File.WriteAllTextAsync(path, _lastHtml);
        await DisplayAlertAsync("Готово", "Файл збережено:\n" + path, "OK");
    #endif
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", "Не вдалося зберегти файл: " + ex.Message, "OK");
        }
    }
}
