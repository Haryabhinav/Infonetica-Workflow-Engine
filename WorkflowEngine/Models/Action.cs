namespace WorkflowEngine.Models;

public class Action
{
    public string Id { get; set; } = string.Empty;    
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }    // Indicates whether the action can be executed
    public List<string> FromStates { get; set; } = new List<string>();   // List of state IDs from which this action can be executed
    public string ToState { get; set; } = string.Empty;
    public int MinTimeInStateSeconds { get; set; } = 0; // Minimum time required in the current state before this action can be executed
}