namespace PersonalAssistant.Api.Infrastructure.Summaries;

public class SummaryOptions
{
    public const string SectionName = "SummarySettings";

    public int TriggerThreshold { get; set; } = 14;
    public int SlidingWindowSize { get; set; } = 6;
    public int MinBatchToSummarize { get; set; } = 8;
    public int MaxRecentHistoryCeiling { get; set; } = 20;
}
