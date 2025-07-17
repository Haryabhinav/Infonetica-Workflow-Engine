namespace WorkflowEngine.Services;

using System.Text.Json;
using WorkflowEngine.Models;

public class WorkflowService
{
    private readonly string _filePath = "workflows.json";
    private readonly List<WorkflowDef> _definitions = new();
    private readonly List<WorkflowInstance> _instances = new();
    private readonly Dictionary<string, DateTime> _stateEntryTimes = new(); // Track state entry times for validation

    public WorkflowService()
    {
        // Load existing data from JSON file on startup
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<WorkflowData>(json) ?? new WorkflowData();
            _definitions = data.Definitions ?? new List<WorkflowDef>();
            _instances = data.Instances ?? new List<WorkflowInstance>();
            foreach (var instance in _instances)
            {
                _stateEntryTimes[instance.Id] = DateTime.UtcNow; // Initialize entry times (approximation)
            }
        }
    }

    public WorkflowDef CreateDefinition(WorkflowDef definition)
    {
        if (!definition.Validate(out string error)) // Validate definition before adding
            throw new ArgumentException(error);
        _definitions.Add(definition);
        SaveToFile(); // Persist to JSON file
        return definition;
    }

    public WorkflowDef? GetDefinition(string id)
    {
        return _definitions.FirstOrDefault(d => d.Id == id); // Retrieve definition by ID
    }

    public WorkflowInstance StartInstance(string definitionId)
    {
        var definition = _definitions.FirstOrDefault(d => d.Id == definitionId);
        if (definition == null)
            throw new ArgumentException("Workflow definition not found.");
        if (!definition.Validate(out string error))
            throw new ArgumentException(error);

        var initialState = definition.States.First(s => s.IsInitial);
        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = definitionId,
            CurrentStateId = initialState.Id
        };
        _instances.Add(instance); // Add new instance to store
        _stateEntryTimes[instance.Id] = DateTime.UtcNow; // Record entry time
        SaveToFile(); // Persist to JSON file
        return instance;
    }

    public WorkflowInstance ExecuteAction(string instanceId, string actionId)
    {
        var instance = _instances.FirstOrDefault(i => i.Id == instanceId);
        if (instance == null)
            throw new ArgumentException("Workflow instance not found.");

        var definition = _definitions.FirstOrDefault(d => d.Id == instance.WorkflowDefinitionId);
        if (definition == null)
            throw new ArgumentException("Workflow definition not found.");

        var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
        if (currentState == null || !currentState.Enabled)
            throw new ArgumentException("Current state is invalid or disabled.");

        if (currentState.IsFinal)
            throw new ArgumentException("Cannot execute actions on a final state.");

        var action = definition.Actions.FirstOrDefault(a => a.Id == actionId);
        if (action == null || !action.Enabled)
            throw new ArgumentException("Action not found or disabled.");

        if (!action.FromStates.Contains(instance.CurrentStateId))
            throw new ArgumentException("Action not valid from current state.");

        // Validate minimum time in current state (e.g., 5 seconds)
        var entryTime = _stateEntryTimes[instance.Id];
        if ((DateTime.UtcNow - entryTime).TotalSeconds < action.MinTimeInStateSeconds)
            throw new ArgumentException($"Must remain in {currentState.Name} for at least {action.MinTimeInStateSeconds} seconds.");

        instance.CurrentStateId = action.ToState;
        _stateEntryTimes[instance.Id] = DateTime.UtcNow;                                                  // Update entry time for new state
        instance.History.Add(new HistoryEntry { ActionId = actionId, Timestamp = DateTime.UtcNow });      // Record action history
        SaveToFile();                                                                                     // Persist changes to JSON file
        return instance;
    }

    public WorkflowInstance? GetInstance(string id)
    {
        return _instances.FirstOrDefault(i => i.Id == id);                                                  // Retrieve instance by ID
    }

    public List<WorkflowDef> ListDefinitions()
    {
        return _definitions;                                                                                // Return all definitions
    }

    public List<WorkflowInstance> ListInstances()
    {
        return _instances;                                                                                 // Return all instances
    }

    private void SaveToFile()
    {
        // Serialize and save definitions and instances to JSON file
        var data = new WorkflowData
        {
            Definitions = _definitions,
            Instances = _instances
        };
        File.WriteAllText(_filePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }
}

// Helper class to deserialize JSON data
public class WorkflowData
{
    public List<WorkflowDef>? Definitions { get; set; }
    public List<WorkflowInstance>? Instances { get; set; }
}


// TODO: Implement rollback mechanism (planned but not implemented due to time constraints)
        // Description: Add a rollback feature to revert an instance to its previous state if an action fails or is canceled.
        // Approach:
        // 1. Add a Stack<string> PreviousStateIds to WorkflowInstance to track state history.
        // 2. Before updating CurrentStateId, push the current state to PreviousStateIds.
        // 3. Create a new method Rollback(string instanceId) that:
        //    - Checks if PreviousStateIds is not empty.
        //    - Pops the last state and sets it as CurrentStateId.
        //    - Updates _stateEntryTimes with the original entry time (if stored).
        //    - Removes the last HistoryEntry if rollback is due to failure.
        // 4. Call Rollback in a catch block or as a separate endpoint (e.g., POST /instances/{instanceId}/rollback).
        // Benefit: Enhances error recovery and reliability.
        // Note: Would require updating SaveToFile to persist PreviousStateIds.