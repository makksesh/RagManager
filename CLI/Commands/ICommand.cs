namespace CLI.Commands;

public interface ICommand
{
    Task<int> ExecuteAsync(CancellationToken ct = default);
}