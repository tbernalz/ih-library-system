namespace IH.LibrarySystem.Application.Ai;

public interface IAiService
{
    Task<string> CompleteAsync(string prompt);
}
