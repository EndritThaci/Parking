using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using System.Security.Claims;

namespace Parking_project.Controllers.Bank
{
    [ApiController]
    [Route("api/bankAccount")]
    public class BankAccountController : Controller
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;
        public BankAccountController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet("ByUser")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BankAccount>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BankAccount>>>> GetByUser()
        {
            try
            {
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0");
                var bankAcc = await _db.BankAccount.Where(b => b.UserId == userId).Include(u => u.User).Include(b => b.Bank).ToListAsync();
                if (bankAcc == null)
                {
                    return NotFound(ApiResponse<CardDetails>.NotFound("Bank Account was not found for this User"));
                }
                return Ok(bankAcc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }

        [HttpGet("ById")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BankAccount>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BankAccount>>>> GetById(int id)
        {
            try
            {
                var bankAcc = await _db.BankAccount.Include(u => u.User).Include(b => b.Bank).Where(i => i.Id == id).FirstOrDefaultAsync();
                if (bankAcc == null)
                {
                    return NotFound(ApiResponse<CardDetails>.NotFound("Bank Account was not found"));
                }
                return Ok(bankAcc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BankAccount>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BankAccount>>> Create([FromBody] BankAccountCreateDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var bankAccountExists = await _db.BankAccount.AnyAsync(b => b.AccountNumber == dto.AccountNumber);
                if (bankAccountExists)
                {
                    return Conflict(ApiResponse<BankAccount>.Conflict("A bank account with the same account number already exists."));
                }

                var bankAcc = _mapper.Map<BankAccount>(dto);
                bankAcc.UserId = userId;
                _db.BankAccount.Add(bankAcc);
                await _db.SaveChangesAsync();
                return Ok(ApiResponse<BankAccount>.Ok(bankAcc, "Regjistrimi u be me sukses"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }
    }
}