using Akka.Actor;

namespace Akka.Pattern.Common;

/// <summary>
/// Indicates that this message is a command, which means the caller likely wants a response back
/// </summary>
public interface IDomainCommand
{
    
}

public static class CommandExtensions{
    
    public static bool IsSuccess(this ICommandResponse response)
    {
        // treat "no-op" as a success here, i.e. no failure needs to be propagated back to client
        return response.Result != CommandResult.Failure;
    }

    public static Task<TResponse> ExecuteCommandAsync<TResponse>(this ICanTell actor, IDomainCommand cmd,
        CancellationToken ct = default) where TResponse : ICommandResponse
    {
        return actor.Ask<TResponse>(cmd, cancellationToken: ct);
    }
}