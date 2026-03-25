using AutoMapper;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Parking_project.Controllers
{
    [ApiController]
    [Route("api/lokacioni")]
    public class LokacioniController : ControllerBase
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;

        public LokacioniController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Lokacioni>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Lokacioni>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Lokacioni>>>> GetLokacionet()
        {
            try
            {
                var lokacionet = await _db.Lokacioni.Where(a => a.active).Include(n => n.NjesiOrg).ThenInclude(o => o.Organizata).ToListAsync();
                return Ok(ApiResponse<IEnumerable<Lokacioni>>.Ok(lokacionet, "Lokacionet retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Lokacioni>>.Error(500, "An Error Occurred while retrieving Lokacioni", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("ByOrg")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Lokacioni>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Lokacioni>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Lokacioni>>>> GetLokacionetByOrg()
        {
            try
            {
                int orgId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var lokacionet = await _db.Lokacioni.Where(a => a.active).Include(n => n.NjesiOrg).ThenInclude(o => o.Organizata).Where(l => l.NjesiOrg.BiznesId == orgId).ToListAsync();
                return Ok(ApiResponse<IEnumerable<Lokacioni>>.Ok(lokacionet, "Lokacionet retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Lokacioni>>.Error(500, "An Error Occurred while retrieving Lokacioni", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{njesiId:int}/ByNjesi")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Lokacioni>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Lokacioni>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Lokacioni>>>> GetLokacionetByNjesi(int njesiId)
        {
            try
            {
                var lokacionet = await _db.Lokacioni.Where(a => a.active).Include(n => n.NjesiOrg).ThenInclude(o => o.Organizata).Where(l => l.NjesiteId == njesiId).ToListAsync();
                return Ok(ApiResponse<IEnumerable<Lokacioni>>.Ok(lokacionet, "Lokacionet retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Lokacioni>>.Error(500, "An Error Occurred while retrieving Lokacioni", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Lokacioni>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Lokacioni>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Lokacioni>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Lokacioni>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Lokacioni>>> GetLokacioniById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse<Lokacioni>.BadRequest("Invalid ID supplied"));

                var lokacioni = await _db.Lokacioni.Where(a => a.active).Include(n => n.NjesiOrg).ThenInclude(o => o.Organizata).FirstOrDefaultAsync(l => l.LokacioniId == id);

                if (lokacioni == null)
                    return NotFound(ApiResponse<Lokacioni>.NotFound($"Lokacioni with ID {id} not found"));

                return Ok(ApiResponse<Lokacioni>.Ok(lokacioni, "Lokacioni retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Lokacioni>.Error(500, "An Error Occurred while retrieving Lokacioni", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LokacioniCreateDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<LokacioniCreateDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<LokacioniCreateDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<LokacioniCreateDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<LokacioniCreateDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LokacioniCreateDTO>>> CreateLokacioni(LokacioniCreateDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(ApiResponse<LokacioniCreateDTO>.BadRequest("Data is required"));

                var njesiaExists = await _db.NjesiOrg.Where(a => a.active).AnyAsync(n => n.NjesiteId == dto.NjesiteId);
                if (!njesiaExists)
                    return NotFound(ApiResponse<LokacioniCreateDTO>.NotFound("Njesia does not exist"));

                var lokacioniExists = await _db.Lokacioni.Where(a => a.active).AnyAsync(l => l.Kati == dto.Kati && l.NjesiteId == dto.NjesiteId);
                if (lokacioniExists)
                    return Conflict(ApiResponse<VendiCreateDTO>.Conflict("Lokacioni exists"));

                var lokacioni = _mapper.Map<Lokacioni>(dto);
                await _db.Lokacioni.AddAsync(lokacioni);
                await _db.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetLokacioniById),
                    new { id = lokacioni.LokacioniId },
                    ApiResponse<LokacioniCreateDTO>.CreatedAt(dto, "Lokacioni created successfully")
                );
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<LokacioniCreateDTO>.Error(500, "An Error Occurred while creating Lokacioni", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LokacioniUpdateDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LokacioniUpdateDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<LokacioniUpdateDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<LokacioniUpdateDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<LokacioniUpdateDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LokacioniUpdateDTO>>> UpdateLokacioni(int id, LokacioniUpdateDTO dto)
        {
            try
            {
                if (dto == null || id != dto.LokacioniId)
                    return BadRequest(ApiResponse<LokacioniUpdateDTO>.BadRequest("ID mismatch"));

                var existing = await _db.Lokacioni.Where(a => a.active).FirstOrDefaultAsync(l => l.LokacioniId == id);
                if (existing == null)
                    return NotFound(ApiResponse<LokacioniUpdateDTO>.NotFound($"Lokacioni with ID {id} not found"));

                var existingNjesi = await _db.NjesiOrg.Where(a => a.active).FirstOrDefaultAsync(n => n.NjesiteId == dto.NjesiteId);
                if (existingNjesi == null)
                    return NotFound(ApiResponse<LokacioniUpdateDTO>.NotFound($"Njesia with ID {dto.NjesiteId} not found"));

                var lokacioniExists = await _db.Lokacioni.Where(a => a.active).AnyAsync(l => l.Kati == dto.Kati && l.NjesiteId == dto.NjesiteId && l.LokacioniId != dto.LokacioniId);
                if (lokacioniExists)
                    return Conflict(ApiResponse<VendiCreateDTO>.Conflict("Lokacioni exists"));

                _mapper.Map(dto, existing);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<LokacioniUpdateDTO>.Ok(dto, "Lokacioni updated successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<LokacioniUpdateDTO>.Error(500, "An Error Occurred while updating Lokacioni", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Lokacioni>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Lokacioni>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Lokacioni>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Lokacioni>>> DeleteLokacioni(int id)
        {
            try
            {
                var lokacioni = await _db.Lokacioni.Where(a => a.active).FirstOrDefaultAsync(l => l.LokacioniId == id);

                if (lokacioni == null)
                    return NotFound(ApiResponse<Lokacioni>.NotFound($"Lokacioni with ID {id} not found"));

                var getVendi = await _db.Vendi.Where(n => n.LokacioniId == lokacioni.LokacioniId).ToListAsync();
                if (getVendi != null)
                {
                    foreach (var x in getVendi) x.active = false;
                }

                lokacioni.active = false;
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<Lokacioni>.NoContent("Lokacioni deleted successfully"));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                var errorResponse = ApiResponse<Lokacioni>.Error(500, "An Error Occurred while deleting Lokacioni", innerMessage);
                return StatusCode(500, errorResponse);
            }
        }
    }
}