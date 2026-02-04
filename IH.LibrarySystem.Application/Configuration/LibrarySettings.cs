using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Configuration;

public class LibrarySettings
{
    public const string SectionName = "LibrarySettings";

    [Required, Range(0, 100)]
    public decimal DailyFineRate { get; init; }

    [Required, Range(1, 365)]
    public int DefaultLoanDurationDays { get; init; }
}
