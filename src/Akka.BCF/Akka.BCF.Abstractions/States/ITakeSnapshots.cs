namespace Akka.BCF.Abstractions.States;

/// <summary>
/// Must able to emit simple, serializable types
/// </summary>
/// <typeparam name="TSnapshot"></typeparam>
public interface ITakeSnapshots<out TSnapshot> where TSnapshot:new()
{
    TSnapshot ToSnapshot();
}