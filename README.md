# Workflow Engine API

A minimal backend service for a configurable workflow engine (state-machine API) per the Infonetica take-home exercise.

## Quick-Start

1. **Prerequisites**: Install [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
2. **Clone Repo**: `git clone https://github.com/your-username/Infonetica-Workflow-Engine.git`
3. **Navigate**: `cd WorkflowEngine`
4. **Restore**: `dotnet restore`
5. **Run**: `dotnet run`
6. **Access**: Open `https://localhost:5001/swagger` (port may vary; check terminal output).

## API Endpoints

- `POST /workflows`: Create a workflow definition.
- `GET /workflows/{id}`: Retrieve a definition.
- `GET /workflows`: List definitions.
- `POST /instances`: Start an instance (provide `definitionId`).
- `POST /instances/{instanceId}/actions`: Execute an action (provide `actionId`).
- `GET /instances/{id}`: Get instance state/history.
- `GET /instances`: List instances.

## Assumptions & Limitations

- **Storage**: Persisted to `workflows.json` (no database).
- **Validation**: Enforces unique IDs, single initial state, valid transitions, and minimum time in state.
- **TODO**: Optimize JSON I/O, add unit tests.

## Testing

- Use Swagger UI at `/swagger`.
- Example workflow:
  ```json
  {
    "id": "wf1",
    "states": [{"id": "s1", "name": "Draft", "isInitial": true, "isFinal": false, "enabled": true},
               {"id": "s2", "name": "Approved", "isInitial": false, "isFinal": true, "enabled": true}],
    "actions": [{"id": "a1", "name": "Approve", "enabled": true, "fromStates": ["s1"], "toState": "s2", "minTimeInStateSeconds": 5}]
  }
