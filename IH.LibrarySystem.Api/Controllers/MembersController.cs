using IH.LibrarySystem.Application.Members;
using IH.LibrarySystem.Application.Members.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController(IMemberService memberService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetMember(Guid id)
    {
        var member = await memberService.GetMemberByIdAsync(id);
        return Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<MemberDto>> RegisterMember(RegisterMemberRequest request)
    {
        var member = await memberService.RegisterMemberAsync(request);
        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MemberDto>> UpdateMember(Guid id, UpdateMemberRequest request)
    {
        var member = await memberService.UpdateMemberAsync(id, request);
        return Ok(member);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        await memberService.DeleteMemberAsync(id);
        return NoContent();
    }
}
