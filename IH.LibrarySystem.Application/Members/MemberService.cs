using IH.LibrarySystem.Application.Members.Dtos;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Application.Members;

public class MemberService(
    IMemberRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<MemberService> logger
) : IMemberService
{
    public async Task<MemberDto> GetMemberByIdAsync(Guid memberId)
    {
        logger.LogDebug("Fetching member with {MemberId}", memberId);

        var member = await repository.GetByIdAsync(memberId);

        if (member is null)
        {
            logger.LogWarning("Member retrieval failed: ID {MemberId} not found", memberId);
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");
        }

        return MapToDto(member);
    }

    public async Task<MemberDto> RegisterMemberAsync(RegisterMemberRequest request)
    {
        logger.LogInformation("Initiating member registration: request {request}", request);

        var existing = await repository.GetByEmailAsync(request.Email);

        if (existing is not null)
        {
            logger.LogWarning("RegisterMember rejected: Duplicate email: {Email}", request.Email);
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
        }

        var member = Member.Create(Guid.NewGuid(), request.Name, request.Email);

        await repository.AddAsync(member);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(member);
    }

    public async Task<MemberDto> UpdateMemberAsync(Guid memberId, UpdateMemberRequest request)
    {
        logger.LogInformation(
            "Initiating update for member: {MemberId}, request {request}",
            memberId,
            request
        );

        var member = await repository.GetByIdAsync(memberId);

        if (member is null)
        {
            logger.LogWarning("UpdateMember failed: Member {MemberId} not found", memberId);
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");
        }

        if (request.Name == member.Name && request.Email == member.Email)
        {
            logger.LogDebug("No changes detected for member: {MemberId}", memberId);
            return MapToDto(member);
        }

        if (!string.Equals(member.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await repository.GetByEmailAsync(request.Email);
            if (existing is not null)
            {
                logger.LogWarning(
                    "Attempted to update member {MemberId} with duplicate email: {Email}",
                    memberId,
                    request.Email
                );
                throw new InvalidOperationException($"Email '{request.Email}' is already taken.");
            }
        }

        member.Update(request.Name, request.Email);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(member);
    }

    public async Task DeleteMemberAsync(Guid memberId)
    {
        logger.LogInformation("Initiating member deletion: {MemberId}", memberId);

        var member = await repository.GetByIdAsync(memberId);

        if (member is null)
        {
            logger.LogWarning("DeleteMember failed: Member {MemberId} not found", memberId);
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");
        }

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
