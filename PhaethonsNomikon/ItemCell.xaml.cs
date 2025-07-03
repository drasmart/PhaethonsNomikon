using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PhaethonsNomikon;

public partial class ItemCell : AgentCellBase
{
    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(AgentData.Item), typeof(ItemCell), new PropertyMetadata(OnItemPropertyChanged));

    public AgentData.Item? Item
    {
        get => (AgentData.Item)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public string EvaluationLetter { get; private set; } = "?";
    public Brush EvaluationColor { get; private set; } = Brushes.Pink;
    public double EvaluationFontSize { get; private set; } = 0;

    public static readonly DependencyProperty IsStripeVisibleProperty =
        DependencyProperty.Register(nameof(IsStripeVisible), typeof(bool), typeof(ItemCell), new PropertyMetadata(OnIsStripeVisibleChanged));

    public bool IsStripeVisible
    {
        get => (bool)GetValue(IsStripeVisibleProperty);
        set => SetValue(IsStripeVisibleProperty, value);
    }

    private static void OnIsStripeVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemCell cell)
        {
            cell.UpdateStats(cell.Agent, cell.Item);
        }
    }
    
    private static void OnItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemCell cell)
        {
            cell.UpdateStats(cell.Agent, e.NewValue as AgentData.Item);
        }
    }
    
    protected override void OnAgentChanged() => UpdateStats(Agent, Item);

    public IEnumerable<StatRow> Stats { get; private set; } =
    [
        new("(ATK)", StatPreference.Preferred, 1),
        new("(DEF)", StatPreference.NotWanted, 1),
        new("(HP)", StatPreference.Fallback, 1),
    ];

    private static readonly IEnumerable<string> PreferredOverrides =
    [
        "ATK",
        "CRIT Rate",
        "CRIT DMG",
    ];

    private void UpdateStats(AgentData? agent, AgentData.Item? item)
    {
        if (agent is null || item is null)
        {
            Stats = [];
            return;
        }
        Stats = item.AllProperties.Select(p =>
        {
            StatPreference preference = p.Name switch
            {
                { } propName when agent.PreferredStats?.Any(x => x.FullName == propName) == true => StatPreference.Preferred,
                { } propName when PreferredOverrides.Contains(propName) => StatPreference.Fallback,
                _ => StatPreference.NotWanted,
            };
            return new StatRow(
                p.ToString(),
                preference,
                GetLevel(p));
        });
        if (item.Star is null)
        {
            var evaluation = item.Rarity == "S"
                ? Stats.Skip(1).Aggregate(0f, (i, row) => i + CalculatePreferenceFactor(row.Preference) * row.Level)
                : -1f;
            ApplyEvaluation((int)evaluation, item is { Rarity: "S", Level: < 15 });
        }
        else
        {
            EvaluationLetter = string.Empty;
        }
    }

    private static int GetLevel(AgentData.ItemProperty itemProperty)
    {
        if (itemProperty.Level is { } level and >= 1)
        {
            return level;
        }
        bool isPercent = (itemProperty.Base ?? "").Contains('%');
        string rawValue = (itemProperty.Base ?? "").Replace("%", "");
        double valueNum = double.Parse(rawValue);
        double? singleRoll = (itemProperty.Name ?? "", isPercent) switch
        {
            ("HP", true) => 3, 
            ("HP", false) => 112,
            ("ATK", true) => 3, 
            ("ATK", false) => 19, 
            ("DEF", true) => 4.8, 
            ("DEF", false) => 15, 
            ("PEN", _) => 9, 
            ("CRIT Rate", _) => 2.4, 
            ("CRIT DMG", _) => 4.8, 
            ("Anomaly Proficiency", _) => 9, 
            _ => null,
        };
        return (singleRoll is { } rollValue) ? (int)Math.Round(valueNum / rollValue) : 1;
    }

    private void ApplyEvaluation(int evaluation, bool canUpgrade)
    {
        (string, Brush) settings = evaluation switch
        {
            -1 => ("XXX", Brushes.Pink),
            0 => ("F", Brushes.Magenta),
            1 => ("E", Brushes.OrangeRed),
            2 => ("D", Brushes.DeepSkyBlue),
            3 => ("C", Brushes.ForestGreen),
            4 => ("B", Brushes.LimeGreen),
            5 => ("A", Brushes.Yellow),
            6 => ("S", Brushes.Orange),
            7 => ("SS", Brushes.Orange),
            8 => ("SSS", Brushes.Orange),
            9 => ("GOD", Brushes.Gold),
            _ => ("?", Brushes.Pink),
        };
        int size = (settings.Item1.Length + (canUpgrade ? 1 : 0)) switch
        {
            1 => 80,
            2 => 56,
            3 => 36,
            _ => 28,
        };
        EvaluationLetter = settings.Item1 + (canUpgrade ? "+" : "");
        EvaluationColor = settings.Item2;
        EvaluationFontSize = size;
    }

    private float CalculatePreferenceFactor(StatPreference preferenceSource)
    {
        return IsStripeVisible
            ? ((float)preferenceSource) / 2.0f
            : preferenceSource == StatPreference.NotWanted ? 0 : 1;
    }

    public ItemCell()
    {
        InitializeComponent();
        UpdateStats(Agent, Item);
    }
}
