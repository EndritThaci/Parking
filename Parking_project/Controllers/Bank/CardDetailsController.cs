using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
using Parking_project.Helper;
using Parking_project.Models;
using Parking_project.Models.DTO;
using System.Security.Claims;

namespace Parking_project.Controllers.Bank
{
    [ApiController]
    [Route("api/cardDetails")]
    public class CardDetailsController : Controller
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;
        public CardDetailsController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CardDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CardDetails>>> GetById(int id)
        {
            try
            {
                var cardDetails = await _db.CardDetails.Where(cd => cd.Id == id).Include(u => u.User).Include(b => b.BankAccount).ThenInclude(b => b.Bank).FirstOrDefaultAsync();
                if (cardDetails == null)
                {
                    return NotFound(ApiResponse<CardDetails>.NotFound("No card details found for the user."));
                }
                cardDetails.CardNumber = EncryptionHelper.Decrypt(cardDetails.CardNumber);
                return Ok(ApiResponse<CardDetails>.Ok(cardDetails, "Sukses"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }

        [HttpGet("ByUser")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CardDetails>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CardDetails>>>> GetByUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var cardDetails = await _db.CardDetails.Where(cd => cd.UserId == userId).Include(u => u.User).Include(b => b.BankAccount).ThenInclude(b => b.Bank).ToListAsync();
                if (cardDetails == null || !cardDetails.Any())
                {
                    return NotFound(ApiResponse<CardDetails>.NotFound("No card details found for the user."));
                }
                cardDetails.ForEach(cd => cd.CardNumber = EncryptionHelper.Decrypt(cd.CardNumber));
                return Ok(ApiResponse<IEnumerable<CardDetails>>.Ok(cardDetails, "Sukses"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CardDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CardDetails>>> Create([FromBody] CardDetailsCreateDTO cardDetailsDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var existingCard = await _db.CardDetails.FirstOrDefaultAsync(c => c.CardNumber == cardDetailsDto.CardNumber);
                if (existingCard != null)
                {
                    return Conflict(ApiResponse<CardDetails>.Conflict("An account with the same card number already exists."));
                }
                var bankAccount = await _db.BankAccount.FirstOrDefaultAsync(b => b.Id == cardDetailsDto.BankAcountId && b.UserId == userId);
                if (bankAccount == null)
                {
                    return NotFound(ApiResponse<CardDetails>.NotFound("Bank account not found for the user."));
                }
                var cardDetails = _mapper.Map<CardDetails>(cardDetailsDto);
                cardDetails.UserId = userId;
                cardDetails.CardNumber = EncryptionHelper.Encrypt(cardDetailsDto.CardNumber);
                _db.CardDetails.Add(cardDetails);
                await _db.SaveChangesAsync();
                return Ok(ApiResponse<CardDetails>.Ok(cardDetails, "Regjistrimi u be me sukses"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }

        [HttpPost("Account")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CardDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CardDetails>>> CreateAccount([FromBody] CardAcountCreateDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var bank = await _db.Bank.Where(i => i.Id == dto.BankId).FirstOrDefaultAsync();
                if (bank == null)
                {
                    return NotFound(ApiResponse<CardDetails>.NotFound("Bank not found."));
                }
                var existingAccount = await _db.BankAccount.FirstOrDefaultAsync(a => a.AccountNumber == dto.AccountNumber);
                if (existingAccount != null)
                {
                    return Conflict(ApiResponse<CardDetails>.Conflict("An account with the same account number already exists."));
                }
                var existingCard = await _db.CardDetails.FirstOrDefaultAsync(c => c.CardNumber == dto.CardNumber);
                if (existingCard != null)
                {
                    return Conflict(ApiResponse<CardDetails>.Conflict("An account with the same card number already exists."));
                }
                if (dto.CardNumber.Length != 16)
                {
                    return BadRequest(ApiResponse<CardDetails>.BadRequest("Card Number must be 16 digit long"));
                }

                var bankAccount = new BankAccount();
                bankAccount.BankId = dto.BankId;
                bankAccount.AccountNumber = dto.AccountNumber;
                bankAccount.Amount = dto.Amount;
                bankAccount.UserId = userId;
                _db.BankAccount.Add(bankAccount);
                await _db.SaveChangesAsync();


                var cardDetails = new CardDetails();
                cardDetails.ExpirationDate = dto.ExpirationDate;
                cardDetails.CardNumber = EncryptionHelper.Encrypt(dto.CardNumber);
                cardDetails.BankAcountId = bankAccount.Id;
                cardDetails.UserId = userId;
                _db.CardDetails.Add(cardDetails);
                await _db.SaveChangesAsync();
                return Ok(ApiResponse<CardDetails>.Ok(cardDetails, "Regjistrimi u be me sukses"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }

        [HttpPut("Pay/{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CardDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CardDetails>>> Pay(int id, [FromBody] decimal amount)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var cardDetails = await _db.CardDetails.Where(cd => cd.Id == id).FirstOrDefaultAsync();
                if (cardDetails == null || userId != cardDetails.UserId)
                {
                    return NotFound(ApiResponse<CardDetails>.NotFound("No card found"));
                }
                if (cardDetails.ExpirationDate < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    return BadRequest(ApiResponse<CardDetails>.BadRequest("Card has expired"));
                }

                var bankAcc = await _db.BankAccount.Where(b => b.Id == cardDetails.BankAcountId).FirstOrDefaultAsync();
                if (bankAcc == null)
                {
                    return NotFound(ApiResponse<CardDetails>.NotFound("No bank account found for the card details."));
                }

                if (bankAcc.Amount < amount)
                {
                    return BadRequest(ApiResponse<CardDetails>.BadRequest("Insufficient funds in the bank account."));
                }
                else
                {
                    bankAcc.Amount -= amount;
                    _db.BankAccount.Update(bankAcc);
                    await _db.SaveChangesAsync();
                }

                return Ok(ApiResponse<CardDetails>.Ok(cardDetails, "Payment Successfull"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }
    }
}