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
    [Route("api/vendi")]
    public class VendiController : ControllerBase
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;

        public VendiController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Vendi>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Vendi>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Vendi>>>> GetVendet()
        {
            try
            {
                var vendet = await _db.Vendi
                    .Include(v => v.Lokacioni)
                    .ThenInclude(l => l.NjesiOrg)
                    .ThenInclude(o => o.Organizata)
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<Vendi>>.Ok(vendet, "Vendet retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Vendi>>.Error(500, "An Error Occurred while retrieving Vendi", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("ByOrg")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Vendi>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Vendi>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Vendi>>>> GetVendetByOrg()
        {
            try
            {
                int orgId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var vendet = await _db.Vendi
                    .Include(v => v.Lokacioni)
                    .ThenInclude(l => l.NjesiOrg)
                    .ThenInclude(o => o.Organizata)
                    .Where(v => v.Lokacioni.NjesiOrg.BiznesId == orgId)
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<Vendi>>.Ok(vendet, "Vendet retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Vendi>>.Error(500, "An Error Occurred while retrieving Vendi", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
        
        [HttpGet]
        [Authorize]
        [Route("ByNjesi")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Vendi>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Vendi>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Vendi>>>> GetVendetByNjesi()
        {
            try
            {
                int njeisaId = int.Parse(User.FindFirst("NjesiaId")!.Value);
                var vendet = await _db.Vendi
                    .Include(v => v.Lokacioni)
                    .ThenInclude(l => l.NjesiOrg)
                    .ThenInclude(o => o.Organizata)
                    .Where(v => v.Lokacioni.NjesiteId == njeisaId)
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<Vendi>>.Ok(vendet, "Vendet retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Vendi>>.Error(500, "An Error Occurred while retrieving Vendi", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpGet("{lokacionId:int}/ByLokacion")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Vendi>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Vendi>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Vendi>>>> GetVendetByLokacion(int lokacionId)
        {
            try
            {
                var vendet = await _db.Vendi
                    .Include(v => v.Lokacioni)
                    .ThenInclude(l => l.NjesiOrg)
                    .ThenInclude(o => o.Organizata)
                    .Where(v => v.LokacioniId == lokacionId)
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<Vendi>>.Ok(vendet, "Vendet retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Vendi>>.Error(500, "An Error Occurred while retrieving Vendi", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Vendi>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Vendi>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Vendi>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Vendi>>> GetVendiById(int id)
        {
            try
            {
                if (id <= 0)
                    return NotFound(ApiResponse<Vendi>.NotFound("Invalid ID supplied"));

                var vendi = await _db.Vendi
                    .Include(v => v.Lokacioni)
                    .ThenInclude(l => l.NjesiOrg)
                    .ThenInclude(o => o.Organizata)
                    .FirstOrDefaultAsync(v => v.VendiId == id);

                if (vendi == null)
                    return NotFound(ApiResponse<Vendi>.NotFound($"Vendi with ID {id} not found"));

                return Ok(ApiResponse<Vendi>.Ok(vendi, "Vendi retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Vendi>.Error(500, "An Error Occurred while retrieving Vendi", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<VendiCreateDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<VendiCreateDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<VendiCreateDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<VendiCreateDTO>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<VendiCreateDTO>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<VendiCreateDTO>>> CreateVendi(VendiCreateDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(ApiResponse<VendiCreateDTO>.BadRequest("Data is required"));

                var lokacioniExists = await _db.Lokacioni.AnyAsync(l => l.LokacioniId == dto.LokacioniId);
                if (!lokacioniExists)
                    return NotFound(ApiResponse<VendiCreateDTO>.NotFound("Lokacioni does not exist"));

                var vendiExists = await _db.Vendi.AnyAsync(n => n.VendiEmri.ToLower() == dto.VendiEmri.ToLower() && n.LokacioniId == dto.LokacioniId);
                if (vendiExists)
                {
                    return Conflict(ApiResponse<VendiCreateDTO>.Conflict("Vendi exists"));
                }

                var vendi = _mapper.Map<Vendi>(dto);
                vendi.IsFree = true;
                await _db.Vendi.AddAsync(vendi);
                await _db.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetVendiById),
                    new { id = vendi.VendiId },
                    ApiResponse<VendiCreateDTO>.CreatedAt(dto, "Vendi created successfully")
                );
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<VendiCreateDTO>.Error(500, "An Error Occurred while creating Vendi", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<VendiUpdateDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VendiUpdateDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<VendiUpdateDTO>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<VendiUpdateDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<VendiUpdateDTO>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VendiUpdateDTO>>> UpdateVendi(int id, VendiUpdateDTO dto)
        {
            try
            {
                if (dto == null || id != dto.VendiId)
                    return BadRequest(ApiResponse<VendiUpdateDTO>.BadRequest("ID mismatch"));

                var existing = await _db.Vendi.FirstOrDefaultAsync(v => v.VendiId == id);
                if (existing == null)
                    return NotFound(ApiResponse<VendiUpdateDTO>.NotFound($"Vendi with ID {id} not found"));

                var lokacioniExists = await _db.Lokacioni.AnyAsync(l => l.LokacioniId == dto.LokacioniId);
                if (!lokacioniExists)
                    return NotFound(ApiResponse<VendiUpdateDTO>.NotFound("Lokacioni does not exist"));

                var vendiExists = await _db.Vendi.AnyAsync(n => n.VendiEmri.ToLower() == dto.VendiEmri.ToLower() && n.LokacioniId == dto.LokacioniId && n.VendiId != dto.VendiId);
                if (vendiExists)
                    return Conflict(ApiResponse<VendiCreateDTO>.Conflict("Vendi exists"));

                _mapper.Map(dto, existing);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<VendiUpdateDTO>.Ok(dto, "Vendi updated successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<VendiUpdateDTO>.Error(500, "An Error Occurred while updating Vendi", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Vendi>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Vendi>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<Vendi>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Vendi>>> DeleteVendi(int id)
        {
            try
            {
                var vendi = await _db.Vendi.FirstOrDefaultAsync(v => v.VendiId == id);

                if (vendi == null)
                    return NotFound(ApiResponse<Vendi>.NotFound($"Vendi with ID {id} not found"));

                var transaksionet = await _db.TransaksionParkimi.Where(s => s.VendiParkimitId == id).ToListAsync();

                var transIds = transaksionet.Select(t => t.TransaksioniId).ToList();

                var detajet = await _db.TransaksionDetaj.Where(d => transIds.Contains(d.TransaksionId)).ToListAsync();

                _db.TransaksionDetaj.RemoveRange(detajet);
                await _db.SaveChangesAsync();
                _db.TransaksionParkimi.RemoveRange(transaksionet);
                await _db.SaveChangesAsync();

                _db.Vendi.Remove(vendi);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<Vendi>.NoContent("Vendi deleted successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Vendi>.Error(500, "An Error Occurred while deleting Vendi", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

    }
}
