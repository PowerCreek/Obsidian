

namespace Obsidian.Stripped.BusinessLogic.Actions.Commands;
public record Login : IRequest
{

}

public record LoginHandler(IDictionary<string,string> A) : VoidHandlerBase<Login>
{
    protected override Action<IRequest> HandleAction => _ => A;
}
