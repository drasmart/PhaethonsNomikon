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
        new("(ATK)", true, 1),
        new("(DEF)", false, 1),
        new("(HP)", false, 1),
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
        Stats = item.AllProperties.Select(p => new StatRow(
            p.ToString(),
            (agent.PreferredStats?.Any(x => x.FullName == p.Name) == true)
            || PreferredOverrides.Contains(p.Name),
            p.Level is {} level and > 1 ? level : 1));
        if (item.Star is null)
        {
            int evaluation = Stats.Skip(1).Aggregate(0, (i, row) => i + (row.Preferred ? row.Level : 0));
            ApplyEvaluation(evaluation);
        }
        else
        {
            EvaluationLetter = string.Empty;
        }
    }

    private void ApplyEvaluation(int evaluation)
    {
        (string, Brush, double) settings = evaluation switch
        {
            0 => ("F", Brushes.Magenta, 80),
            1 => ("E", Brushes.OrangeRed, 80),
            2 => ("D", Brushes.DeepSkyBlue, 80),
            3 => ("C", Brushes.ForestGreen, 80),
            4 => ("B", Brushes.LimeGreen, 80),
            5 => ("A", Brushes.Yellow, 80),
            6 => ("S", Brushes.Orange, 80),
            7 => ("SS", Brushes.Orange, 56),
            8 => ("SSS", Brushes.Orange, 36),
            9 => ("GOD", Brushes.Gold, 36),
            _ => ("GOD+", Brushes.Gold, 28),
        };
        EvaluationLetter = settings.Item1;
        EvaluationColor = settings.Item2;
        EvaluationFontSize = settings.Item3;
    }

    public ItemCell()
    {
        InitializeComponent();
        UpdateStats(Agent, Item);
    }
}