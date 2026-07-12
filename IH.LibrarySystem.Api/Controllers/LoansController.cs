using IH.LibrarySystem.Application.Common.Security;
using IH.LibrarySystem.Application.Loans;
using IH.LibrarySystem.Application.Loans.Commands;
using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Application.Loans.Queries;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController(IMediator mediator, ILoanService loanService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetLoans(
        [FromQuery] LoanSearchFilter filter
    )
    {
        var result = await mediator.Send(new GetLoansQuery(filter));
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
    public async Task<ActionResult<LoanDto>> GetLoan(Guid id)
    {
        var loan = await loanService.GetLoanByIdAsync(id);
        return Ok(loan);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<LoanDto>> CheckoutBook(CheckoutBookRequest request)
    {
        var loan = await mediator.Send(new CheckoutBookCommand(request.BookId, request.MemberId));
        return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
    }

    [HttpPost("return/{loanId}")]
    [Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
    public async Task<ActionResult<LoanDto>> ReturnBook(Guid loanId, ReturnBookRequest request)
    {
        var loan = await loanService.ReturnBookAsync(loanId, request);
        return Ok(loan);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> DeleteLoan(Guid id)
    {
        await loanService.DeleteLoanAsync(id);
        return NoContent();
    }
}
