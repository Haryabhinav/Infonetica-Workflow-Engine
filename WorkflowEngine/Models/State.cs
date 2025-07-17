namespace WorkflowEngine.Models;

public class State
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsInitial { get; set; }    // Indicates whether this state is the initial state of the workflow
    public bool IsFinal { get; set; }      // Indicates whether this state is a final state of the workflow
    public bool Enabled { get; set; }      // Indicates whether this state can be transitioned to or from
    public string Description { get; set; } = string.Empty;  // Optional description of the state
}