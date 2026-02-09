namespace Apps.Taus.Polling;

public class BatchMemory
{
    public DateTime? LastPollingTime { get; set; }

    public bool Triggered { get; set; }
}
