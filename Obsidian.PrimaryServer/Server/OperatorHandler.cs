namespace Obsidian.PrimaryServer.Server;

/// <summary>
/// Handles operator data.
/// </summary>
public class OperatorHandler
{
    public OperatorHandler()
    {

    }

    public bool CreateRequest(CreateRequestActionInput input)
    {
        return true;
    }

    public bool ChangeOperatorModeAction(ChangeOperatorModeActionInput input)
    {
        return true;
    }

    public bool ValidateOperateStateAction(QueryOperatorStateActionInput input, bool IsOperator)
    {
        return true;
    }
}

public enum OperatorMode
{
    NONE,
    CREATIVE,
    SURVIVAL
}

public record struct ChangeOperatorModeActionInput
(
    string? Username,
    string? Uuid,
    OperatorMode Mode
);

public record struct QueryOperatorStateActionInput
(
    string? Username,
    string? Uuid
);
public record struct CreateRequestActionInput
(
    string? Username,
    string? Uuid
);
