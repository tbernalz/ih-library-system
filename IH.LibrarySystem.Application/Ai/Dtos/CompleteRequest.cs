using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Ai.Dtos;

public record CompleteRequest([Required, StringLength(200)] string Prompt);
