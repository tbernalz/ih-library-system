using IH.LibrarySystem.Application.Ai.Dtos;

namespace IH.LibrarySystem.Application.Ai;

public interface IAiService
{
    Task<string> CompleteAsync(CompleteRequest request);
}
