using IH.LibrarySystem.Application.Loans;
using IH.LibrarySystem.Application.Loans.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController(ILoanService loanService) : ControllerBase
{
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
}
