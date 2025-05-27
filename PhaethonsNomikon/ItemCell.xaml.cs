using System.Windows;
using System.Windows.Controls;

namespace PhaethonsNomikon;

public partial class ItemCell : AgentCellBase
{
    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(AgentData.Item), typeof(ItemCell), new PropertyMetadata(OnItemPropertyChanged));

    public AgentData.Item? Item
    {
        get => (AgentData.Item)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
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
        new("(ATK)", true),
        new("(DEF)", false),
        new("(HP)", false),
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
            agent.PreferredStats?.Any(x => x.FullName == p.Name) == true));
    }

    public ItemCell()
    {
        InitializeComponent();
        UpdateStats(Agent, Item);
    }
}