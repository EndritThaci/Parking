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
    [Route("api/njesiOrg")]
    public class NjesiOrgController : Controller
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;
        public NjesiOrgController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NjesiReadDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<NjesiReadDto>>>> GetNjesiOrg()
        {
            try
            {
                var njesiOrgList = await _db.NjesiOrg.Where(a => a.active).Include(o => o.Organizata).ToListAsync();
                var dtoResponseNjesi = _mapper.Map<List<NjesiReadDto>>(njesiOrgList);
                var response = ApiResponse<IEnumerable<NjesiReadDto>>.Ok(dtoResponseNjesi, "Records retrieved successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred while creating NjesiOrg: ", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet]
        [Route("ByOrg")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NjesiReadDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<NjesiReadDto>>>> GetNjesiByOrg()
        {
            try
            {
                int biznesId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var njesiOrgList = await _db.NjesiOrg.Where(a => a.active).Include(o => o.Organizata).Where(n => n.BiznesId == biznesId).ToListAsync();
                var dtoResponseNjesi = _mapper.Map<List<NjesiReadDto>>(njesiOrgList);
                var response = ApiResponse<IEnumerable<NjesiReadDto>>.Ok(dtoResponseNjesi, "Records retrieved successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred while creating NjesiOrg: ", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<NjesiReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<NjesiReadDto>>> GetNjesiOrgById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Njesia Id is invalid"));

                }
                var njesiOrg = await _db.NjesiOrg.Where(a => a.active).FirstOrDefaultAsync(n => n.NjesiteId == id);
                if (njesiOrg == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"NjesiOrg with ID {id} not found."));
                }

                var dtoResponseNjesi = _mapper.Map<NjesiReadDto>(njesiOrg);
                var response = ApiResponse<NjesiReadDto>.Ok(dtoResponseNjesi, "Records retrieved successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred while creating NjesiOrg: ", ex.Message);
                return StatusCode(500, errorResponse);
            }

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<NjesiReadDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<NjesiReadDto>>> CreateNjesiOrg(NjesiOrgDto njesiOrgDto)
        {
            try
            {
                if (njesiOrgDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Njesia is required"));
                }
                if (njesiOrgDto.BiznesId <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Id doesnt match"));
                }

                var getBiznes = await _db.Organizata.FindAsync(njesiOrgDto.BiznesId);
                if (getBiznes == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Organizata with ID {njesiOrgDto.BiznesId} not found."));
                }

                var duplicateName = await _db.NjesiOrg.Where(a => a.active).FirstOrDefaultAsync(n => n.Emri == njesiOrgDto.Emri);
                if (duplicateName != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Njesia with the name '{njesiOrgDto.Emri}' already exists."));
                }
                var duplicateCode = await _db.NjesiOrg.Where(a => a.active).FirstOrDefaultAsync(n => n.Kodi == njesiOrgDto.Kodi);
                if (duplicateCode != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Njesia with the code '{njesiOrgDto.Kodi}' already exists."));
                }

                NjesiOrg njesiOrg = _mapper.Map<NjesiOrg>(njesiOrgDto);
                await _db.NjesiOrg.AddAsync(njesiOrg);
                await _db.SaveChangesAsync();

                var response = ApiResponse<NjesiReadDto>.CreatedAt(_mapper.Map<NjesiReadDto>(njesiOrg), "Njesia created Successfully");
                return CreatedAtAction(nameof(GetNjesiOrgById), new { id = njesiOrg.NjesiteId }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred while creating NjesiOrg: ", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<NjesiReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<NjesiUpdateDto>>> UpdateNjesiOrg(int id, NjesiUpdateDto njesiUpdateDto)
        {
            try
            {
                if (njesiUpdateDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Data is required"));
                }
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid ID supplied."));
                }
                if (id != njesiUpdateDto.NjesiteId)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("ID mismatch"));
                }

                var getNjesiOrg = await _db.NjesiOrg.FindAsync(id);
                if (getNjesiOrg == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"NjesiOrg with ID {id} not found."));
                }

                var duplicateName = await _db.NjesiOrg.Where(a => a.active).FirstOrDefaultAsync(n => n.Emri == njesiUpdateDto.Emri && n.NjesiteId != id);
                if (duplicateName != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"An Organizata with the name '{njesiUpdateDto.Emri}' already exists."));
                }

                _mapper.Map(njesiUpdateDto, getNjesiOrg);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<NjesiUpdateDto>.Ok(njesiUpdateDto, "The Njesi has been Updated"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred while creating NjesiOrg: ", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteNjesiOrg(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid ID supplied."));
                }
                var getNjesiOrg = await _db.NjesiOrg.FindAsync(id);
                if (getNjesiOrg == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"NjesiOrg with ID {id} not found."));
                }
                var getLokacionet = await _db.Lokacioni.Where(n => n.NjesiteId == getNjesiOrg.NjesiteId).ToListAsync();
                if (getLokacionet != null)
                {
                    foreach (var loc in getLokacionet) loc.active = false;
                }
                var getVendi = await _db.Vendi.Where(n => n.Lokacioni.NjesiteId == getNjesiOrg.NjesiteId).ToListAsync();
                if (getVendi != null)
                {
                    foreach (var x in getVendi) x.active = false;
                }
                var getCilsimi = await _db.CilsimetParkimit.Where(n => n.NjesiteId == getNjesiOrg.NjesiteId).ToListAsync();
                if (getCilsimi != null)
                {
                    foreach (var x in getCilsimi) x.active = false;
                }
                var getDetajet = await _db.Detajet.Where(n => n.CilsimetParkimit.NjesiteId == getNjesiOrg.NjesiteId).ToListAsync();
                if (getDetajet != null)
                {
                    foreach (var x in getDetajet) x.active = false;
                }
                getNjesiOrg.active = false;
                await _db.SaveChangesAsync();
                var response = ApiResponse<object>.NoContent($"NjesiOrg with ID {id} has been deleted.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred while creating NjesiOrg: ", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}