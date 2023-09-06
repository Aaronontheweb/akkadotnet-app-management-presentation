namespace Akka.Pattern.Common;

public interface ICommandResponse
{
    CommandResult Result { get; }
    
    string? Message { get; }
}