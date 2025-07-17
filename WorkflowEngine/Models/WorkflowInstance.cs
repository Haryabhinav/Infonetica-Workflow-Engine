namespace WorkflowEngine.Models;

public class WorkflowInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkflowDefinitionId { get; set; } = string.Empty;
    public string CurrentStateId { get; set; } = string.Empty;
    public List<HistoryEntry> History { get; set; } = new List<HistoryEntry>();   // History of actions taken in this instance
}

public class HistoryEntry
{
    public string ActionId { get; set; } = string.Empty;   
    public DateTime Timestamp { get; set; }                                        // Timestamp of when the action was executed
}