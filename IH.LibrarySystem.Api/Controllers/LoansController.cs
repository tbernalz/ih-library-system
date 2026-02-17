using IH.LibrarySystem.Application.Loans;
using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController(ILoanService loanService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetLoans(
        [FromQuery] LoanSearchFilter filter
    )
    {
        var result = await loanService.GetLoansAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LoanDto>> GetLoan(Guid id)
    {
        var loan = await loanService.GetLoanByIdAsync(id);
        return Ok(loan);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<LoanDto>> CheckoutBook(CheckoutBookRequest request)
    {
        var loan = await loanService.CheckoutBookAsync(request);
        return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
    }

    [HttpPost("return/{loanId}")]
    public async Task<ActionResult<LoanDto>> ReturnBook(Guid loanId, ReturnBookRequest request)
    {
        var loan = await loanService.ReturnBookAsync(loanId, request);
        return Ok(loan);
    }
}
