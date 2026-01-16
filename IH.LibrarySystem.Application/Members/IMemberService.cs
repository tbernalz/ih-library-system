using IH.LibrarySystem.Application.Members.Dtos;

namespace IH.LibrarySystem.Application.Members;

public interface IMemberService
{
    Task<MemberDto> GetMemberByIdAsync(Guid memberId);

    Task<MemberDto> RegisterMemberAsync(RegisterMemberRequest request);
    Task<MemberDto> UpdateMemberAsync(Guid memberId, UpdateMemberRequest request);
    Task DeleteMemberAsync(Guid memberId);
}
