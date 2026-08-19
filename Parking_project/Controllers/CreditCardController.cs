using AutoMapper;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;

[ApiController]
[Route("api/creditCards")]
public class CreditCardController : Controller
{
    private readonly AplicationDbContext _db;
    private readonly IMapper _mapper;

    public CreditCardController(AplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;

        StripeConfiguration.ApiKey = "sk_test_51T6URw2KyY4CWYf6iAMFJVTMwPMxcc8oh8z5M7WD3vdp7MWiofYpQMcVFY1KXw1WEB2UPW59FPOFdyx8ieS78fqB00alsItlD4";
    }


    [HttpGet("ByUser")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreditCardReadDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CreditCardReadDto>>>> GetUserCards()
    {
        try
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var cards = await _db.CreditCards
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<CreditCardReadDto>>(cards);

            return Ok(ApiResponse<IEnumerable<CreditCardReadDto>>.Ok(dto, "Cards retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.Error(500, "Error retrieving cards", ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CreditCardReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreditCardReadDto>>> GetById(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.BadRequest("Invalid ID"));

            var card = await _db.CreditCards.FindAsync(id);

            if (card == null)
                return NotFound(ApiResponse<object>.NotFound($"Card with ID {id} not found"));

            var dto = _mapper.Map<CreditCardReadDto>(card);

            return Ok(ApiResponse<CreditCardReadDto>.Ok(dto, "Card retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.Error(500, "Error retrieving card", ex.Message));
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreditCardReadDto>>> Create(CreditCardCreateDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var existingCard = await _db.CreditCards
                .FirstOrDefaultAsync(c => c.UserId == userId);

            string customerId;

            if (existingCard != null)
            {
                customerId = existingCard.StripeCustomerId;
            }
            else
            {
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = User.FindFirst(ClaimTypes.Email)?.Value
                });

                customerId = customer.Id;
            }

            var paymentMethodService = new PaymentMethodService();

            await paymentMethodService.AttachAsync(dto.StripePaymentMethodId, new PaymentMethodAttachOptions
            {
                Customer = customerId
            });

            var paymentMethod = await paymentMethodService.GetAsync(dto.StripePaymentMethodId);

            var card = new CreditCard
            {
                UserId = userId,
                StripeCustomerId = customerId,
                StripePaymentMethodId = paymentMethod.Id,
                Brand = paymentMethod.Card.Brand,
                Last4 = paymentMethod.Card.Last4,
                ExpMonth = (int)paymentMethod.Card.ExpMonth,
                ExpYear = (int)paymentMethod.Card.ExpYear
            };

            _db.CreditCards.Add(card);
            await _db.SaveChangesAsync();

            var readDto = _mapper.Map<CreditCardReadDto>(card);

            return Ok(ApiResponse<CreditCardReadDto>.Ok(readDto, "Card added successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.Error(500, "Error creating card", ex.Message));
        }
    }

    [Authorize]
    [HttpPost("Pay")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> PayWithCard([FromBody] PayRequestDto request)
    {
        try
        {
            if (request == null || request.CreditCardId <= 0 || request.Amount <= 0)
                return BadRequest(ApiResponse<object>.BadRequest("Invalid request"));

            //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var card = await _db.CreditCards.FirstOrDefaultAsync(c => c.Id == request.CreditCardId); //&& c.UserId == userId);
            if (card == null)
                return NotFound(ApiResponse<object>.NotFound("Card not found"));

            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = "eur",
                Customer = card.StripeCustomerId,
                PaymentMethod = card.StripePaymentMethodId,
                OffSession = true,
                Confirm = true
            });

            return Ok(ApiResponse<CreditCardReadDto>.Ok(_mapper.Map<CreditCardReadDto>(card), "Payment successful"));
        }
        catch (StripeException ex)
        {
            return BadRequest(ApiResponse<object>.Error(400, "Stripe error", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error(500, "Error processing payment", ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.BadRequest("Invalid ID"));

            var card = await _db.CreditCards.FindAsync(id);

            if (card == null)
                return NotFound(ApiResponse<object>.NotFound("Card not found"));

            _db.CreditCards.Remove(card);
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<object>.NoContent($"Card with ID {id} deleted"));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.Error(500, "Error deleting card", ex.Message));
        }
    }
}