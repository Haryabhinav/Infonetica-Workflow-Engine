namespace WorkflowEngine.Models;

public class WorkflowDef
{
    public string Id { get; set; } = Guid.NewGuid().ToString();    // Unique identifier for the workflow definition
    public List<State> States { get; set; } = new List<State>();    
    public List<Action> Actions { get; set; } = new List<Action>();

    public bool Validate(out string error)
    {
        error = string.Empty;
        if (!States.Any(s => s.IsInitial))         // Ensure there is exactly one initial state
        {
            error = "Workflow must have exactly one initial state.";
            return false;
        }
        if (States.Count(s => s.IsInitial) > 1)     // Check for multiple initial states
        {
            error = "Workflow can have only one initial state.";
            return false;
        }
        if (States.Any(s => string.IsNullOrEmpty(s.Id)))     // ID should not be empty
        {
            error = "All states must have a non-empty ID.";
            return false;
        }
        if (States.GroupBy(s => s.Id).Any(g => g.Count() > 1))     // Check for duplicate IDs of states
        {
            error = "State IDs must be unique.";
            return false;
        }
        if (Actions.Any(a => string.IsNullOrEmpty(a.Id)))        // ID should not be empty
        {
            error = "All actions must have a non-empty ID.";
            return false;
        }
        if (Actions.GroupBy(a => a.Id).Any(g => g.Count() > 1))       // Check for duplicate IDs of actions
        {
            error = "Action IDs must be unique.";
            return false;
        }
        if (Actions.Any(a => string.IsNullOrEmpty(a.ToState) || !States.Any(s => s.Id == a.ToState)))    // Check if the action can 
        {                                                                                                // transition to a valid state
            error = "All actions must have a valid ToState.";
            return false;
        }
        if (Actions.Any(a => a.FromStates.Any(fs => string.IsNullOrEmpty(fs) || !States.Any(s => s.Id == fs))))    // Check if all
        {                                                                                                          // FromStates in actions
            error = "All FromStates in actions must be valid.";                                                    // must be valid states
            return false;
        }
        return true;
    }
}