using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;

namespace Parking_project.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class BankController : Controller
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;
        public BankController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Banka>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Banka>>>> GetAll()
        {
            try
            {
                var banks = await _db.Bank.ToListAsync();
                if (banks == null || !banks.Any())
                {
                    return NotFound(ApiResponse<object>.NotFound("No banks were found"));
                }
                return Ok(ApiResponse<IEnumerable<Banka>>.Ok(banks, "No banks were found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Banka>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Banka>>> GetById(int id)
        {
            try
            {
                var banks = await _db.Bank.FindAsync(id);
                if (banks == null)
                {
                    return NotFound(ApiResponse<CardDetails>.NotFound("Bank was not found"));
                }
                return Ok(ApiResponse<Banka>.Ok(banks, "No banks were found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Banka>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Banka>>> Create(string name)
        {
            try
            {
                Banka bank = new();
                bank.Name = name;
                _db.Bank.Add(bank);
                await _db.SaveChangesAsync();
                return Ok(ApiResponse<Banka>.Ok(bank, "Regjistrimi u be me sukses"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }
    }
}
