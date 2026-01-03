namespace IH.LibrarySystem.Domain.SharedKernel;

public class Audit
{
    public DateTime? CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    protected void SetUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
