using IH.LibrarySystem.Application.Members.Dtos;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Application.Members;

public class MemberService(IMemberRepository repository, IUnitOfWork unitOfWork) : IMemberService
{
    public async Task<MemberDto> GetMemberByIdAsync(Guid memberId)
    {
        var member =
            await repository.GetByIdAsync(memberId)
            ?? throw new KeyNotFoundException($"Member with ID {memberId} not found.");
        return MapToDto(member);
    }

    public async Task<MemberDto> RegisterMemberAsync(RegisterMemberRequest request)
    {
        var existing = await repository.GetByEmailAsync(request.Email);

        if (existing is not null)
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        var member = Member.Create(Guid.NewGuid(), request.Name, request.Email);

        await repository.AddAsync(member);
        await unitOfWork.SaveChangesAsync();
        return MapToDto(member);
    }

    public async Task<MemberDto> UpdateMemberAsync(Guid memberId, UpdateMemberRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var member =
            await repository.GetByIdAsync(memberId)
            ?? throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        if (request.Name == member.Name && request.Email == member.Email)
            return MapToDto(member);

        if (!string.Equals(member.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await repository.GetByEmailAsync(request.Email);
            if (existing is not null)
                throw new InvalidOperationException($"Email '{request.Email}' is already taken.");
        }

        member.Update(request.Name, request.Email);
        repository.Update(member);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(member);
    }

    public async Task DeleteMemberAsync(Guid memberId)
    {
        var member =
            await repository.GetByIdAsync(memberId)
            ?? throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        // we can't delete a member who has active loans

        repository.Delete(member);
        await unitOfWork.SaveChangesAsync();
    }

    private static MemberDto MapToDto(Member member) =>
        new()
        {
            Id = member.Id,
            Name = member.Name,
            Email = member.Email,
            JoinDate = member.JoinDate,
            Status = member.Status,
        };
}
