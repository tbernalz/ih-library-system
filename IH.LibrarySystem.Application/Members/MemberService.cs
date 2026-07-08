using IH.LibrarySystem.Application.Common.Abstractions;
using IH.LibrarySystem.Application.Common.Exceptions;
using IH.LibrarySystem.Application.Members.Dtos;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Application.Members;

public class MemberService(
    IMemberRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext,
    ILogger<MemberService> logger
) : IMemberService
{
    public async Task<MemberDto> GetMemberByIdAsync(Guid memberId)
    {
        logger.LogDebug("Fetching member with {MemberId}", memberId);

        if (currentUserContext.UserId != memberId && !currentUserContext.IsStaffOrAdmin)
        {
            logger.LogWarning(
                "User {UserId} forbidden from accessing member data for {TargetMemberId}",
                currentUserContext.UserId,
                memberId
            );

            throw new ForbiddenException(
                "You do not have permission to view this member's details."
            );
        }

        var member = await repository.GetByIdAsync(memberId);

        if (member is null)
        {
            logger.LogWarning("Member retrieval failed: ID {MemberId} not found", memberId);
            throw new NotFoundException(nameof(Member), memberId);
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
            var validationException = new ValidationException();
            validationException.AddError(
                "Email",
                $"Email '{request.Email}' is already registered."
            );
            throw validationException;
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
            throw new NotFoundException(nameof(Member), memberId);
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
                var validationException = new ValidationException();
                validationException.AddError("Email", $"Email '{request.Email}' is already taken.");
                throw validationException;
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
            throw new NotFoundException(nameof(Member), memberId);
        }

        // we can't delete a member who has active loans

        repository.Delete(member);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<MemberDto> UpdateStatusAsync(Guid memberId, UpdateStatusRequest request)
    {
        logger.LogInformation(
            "Updating status for member {MemberId} to {Status}",
            memberId,
            request.Status
        );

        var member = await repository.GetByIdAsync(memberId);

        if (member is null)
        {
            logger.LogWarning("Status update failed: Member {MemberId} not found", memberId);
            throw new NotFoundException(nameof(Member), memberId);
        }

        member.UpdateStatus(request.Status);

        await unitOfWork.SaveChangesAsync();

        return MapToDto(member);
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
