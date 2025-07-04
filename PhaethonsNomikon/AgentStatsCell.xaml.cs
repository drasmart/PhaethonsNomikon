﻿using System.Windows;

namespace PhaethonsNomikon;

public partial class AgentStatsCell : AgentCellBase
{
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(int), typeof(AgentStatsCell), new PropertyMetadata(OnColumnsPropertyChanged));

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }
    
    private static void OnColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AgentStatsCell cell)
        {
            cell.UpdateStats(cell.Agent, (e.NewValue as int?) ?? 1);
        }
    }
    
    protected override void OnAgentChanged() => UpdateStats(Agent, Columns);

    public IEnumerable<IEnumerable<StatRow>> Stats { get; private set; } =
    [
        [
            new StatRow("(ATK)", StatPreference.Preferred, 1),
            new StatRow("(DEF)", StatPreference.NotWanted, 1),
        ],
        [
            new StatRow("(HP)", StatPreference.Fallback, 1),
        ],
    ];

    private void UpdateStats(AgentData? agent, int columns)
    {
        if (agent is null)
        {
            Stats = [];
            return;
        }
        if (columns <= 0)
        {
            columns = 1;
        }
        var props = (agent.AgentStats ?? []);
        var stats = Enumerable.Range(0, columns)
            .Select(_ => new List<StatRow>())
            .ToList();
        var perColumn = (int)Math.Ceiling(props.Count / (float)columns);
        for (int i = 0; i < props.Count; i++)
        {
            StatRow newRow = new(
                props[i].ToString(),
                agent.PreferredStats?.Any(x => x.FullName == props[i].Name) == true
                ? StatPreference.Preferred : StatPreference.NotWanted,
                1);
            stats[i / perColumn].Add(newRow);
        }
        Stats = stats;
    }

    public AgentStatsCell()
    {
        InitializeComponent();
        UpdateStats(Agent, Columns);
    }
}