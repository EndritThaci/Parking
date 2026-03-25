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
    [Route("api/cilsimetParkimit")]
    public class CilsimetParkimitController : Controller
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;
        public CilsimetParkimitController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CilsimetReadDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CilsimetReadDto>>>> GetCilsimet()
        {
            try
            {
                var getCilsimet = await _db.CilsimetParkimit
                    .Where(a => a.active)
                    .Include(n => n.NjesiOrg)
                    .ThenInclude(o => o.Organizata)
                    .Include(s => s.Sherbimi)
                    .ToListAsync();
                var cilsimet = _mapper.Map<IEnumerable<CilsimetReadDto>>(getCilsimet);
                var response = ApiResponse<IEnumerable<CilsimetReadDto>>.Ok(cilsimet, "Cilsimet retrieved successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CilsimetReadDto>>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }

        [HttpGet]
        [Route("ByOrg")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CilsimetReadDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CilsimetReadDto>>>> GetCilsimetByOrg()
        {
            try
            {
                int orgId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var getCilsimet = await _db.CilsimetParkimit
                    .Where(a => a.active)
                    .Include(n => n.NjesiOrg)
                    .ThenInclude(o => o.Organizata)
                    .Include(s => s.Sherbimi)
                    .Where(c => c.NjesiOrg.BiznesId == orgId)
                    .ToListAsync();
                var cilsimet = _mapper.Map<IEnumerable<CilsimetReadDto>>(getCilsimet);
                var response = ApiResponse<IEnumerable<CilsimetReadDto>>.Ok(cilsimet, "Cilsimet retrieved successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CilsimetReadDto>>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }

        [HttpGet]
        [Route("{njesiaId:int}/ByNjesi")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CilsimetReadDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CilsimetReadDto>>>> GetCilsimetByNjesi(int njesiaId)
        {
            try
            {
                var getCilsimet = await _db.CilsimetParkimit
                    .Where(a => a.active)
                    .Include(n => n.NjesiOrg)
                    .ThenInclude(o => o.Organizata)
                    .Include(s => s.Sherbimi)
                    .Where(c => c.NjesiteId == njesiaId)
                    .ToListAsync();
                var cilsimet = _mapper.Map<IEnumerable<CilsimetReadDto>>(getCilsimet);
                var response = ApiResponse<IEnumerable<CilsimetReadDto>>.Ok(cilsimet, "Cilsimet retrieved successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CilsimetReadDto>>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CilsimetReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CilsimetReadDto>>> GetCilsimetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Njesia Id is invalid"));

                }
                var cilsimetParking = await _db.CilsimetParkimit
                    .Where(a => a.active)
                    .Include(n => n.NjesiOrg)
                     .ThenInclude(o => o.Organizata)
                    .Include(s => s.Sherbimi).FirstOrDefaultAsync(n => n.CilsimetiId == id);
                if (cilsimetParking == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Cilsimi with ID {id} not found."));
                }

                var dtoResponseCilsimet = _mapper.Map<CilsimetReadDto>(cilsimetParking);
                var response = ApiResponse<CilsimetReadDto>.Ok(dtoResponseCilsimet, "Records retrieved successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred retrieving Cilsimin ", ex.Message);
                return StatusCode(500, errorResponse);
            }

        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CilsimetReadDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CilsimetReadDto>>> CreateCilsime(CilsimetCreateDto cilsimetDto)
        {
            try
            {
                if (cilsimetDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Data is invalid"));
                }
                if (cilsimetDto.NjesiteId <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"The Njesi Id {cilsimetDto.NjesiteId} is invalid"));
                }
                if (cilsimetDto.SherbimiId <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"The Sherbimi Id {cilsimetDto.SherbimiId} is invalid"));
                }

                var getNjesi = await _db.NjesiOrg.Where(a => a.active).FirstOrDefaultAsync(n => n.NjesiteId == cilsimetDto.NjesiteId);
                if (getNjesi == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"The Njesi Id {cilsimetDto.NjesiteId} is not found"));
                }
                var getSherbim = await _db.Sherbimi.Where(a => a.active).FirstOrDefaultAsync(s => s.SherbimiId == cilsimetDto.SherbimiId);
                if (getSherbim == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"The Sherbimi Id {cilsimetDto.SherbimiId} is not found"));
                }
                if (getSherbim.BiznesId != getNjesi.BiznesId)
                    return BadRequest(ApiResponse<object>.BadRequest($"Sherbimi and Njesia are not in the same Organization"));
                if (getSherbim.Emri.ToLower() != "parking")
                    return BadRequest(ApiResponse<object>.BadRequest($"Sherbimi with id {cilsimetDto.SherbimiId} is not a Parking Sherbim."));

                CilsimetParkimit cilsimetParkimit = _mapper.Map<CilsimetParkimit>(cilsimetDto);
                cilsimetParkimit.Selected = false;
                await _db.CilsimetParkimit.AddAsync(cilsimetParkimit);
                await _db.SaveChangesAsync();

                var response = ApiResponse<CilsimetReadDto>.CreatedAt(_mapper.Map<CilsimetReadDto>(cilsimetParkimit), "The Cilsimet has been added Successfully");
                return CreatedAtAction(nameof(GetCilsimetById), new { id = cilsimetParkimit.CilsimetiId }, response); ;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CilsimetReadDto>>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CilsimetUpdateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CilsimetUpdateDto>>> UpdateCilsimet(int id, CilsimetUpdateDto updateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"Cilsimeti Id {id} is invalid"));
                }
                if (updateDto.NjesiteId <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"Njesia Id {updateDto.NjesiteId} is invalid"));
                }
                if (updateDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("The object given is invalid"));
                }
                if (updateDto.SherbimiId <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"The Sherbimi Id {updateDto.SherbimiId} is invalid"));
                }

                var getNjesi = await _db.NjesiOrg.Where(a => a.active).FirstOrDefaultAsync(n => n.NjesiteId == updateDto.NjesiteId);
                if (getNjesi == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"The Njesi Id {updateDto.NjesiteId} is not found"));
                }

                var getSherbim = await _db.Sherbimi.Where(a => a.active).FirstOrDefaultAsync(s => s.SherbimiId == updateDto.SherbimiId);
                if (getSherbim == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"The Sherbimi Id {updateDto.SherbimiId} is not found"));
                }
                if (getSherbim.BiznesId != getNjesi.BiznesId)
                    return BadRequest(ApiResponse<object>.BadRequest($"Sherbimi and Njesia are not in the same Organization"));

                var getCilsimet = await _db.CilsimetParkimit.Where(a => a.active).FirstOrDefaultAsync(i => i.CilsimetiId == id);
                if (getCilsimet == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Cilsimeti with Id {id} is not found"));
                }

                _mapper.Map(updateDto, getCilsimet);
                await _db.SaveChangesAsync();
                var response = ApiResponse<CilsimetUpdateDto>.Ok(updateDto, "Cilsimeti updated successfully");
                return Ok(response);

            }
            catch
            (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred while updating cilsimet: ", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCilsimet(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"The id {id} given is invalid"));
                }
                var getCilsimin = await _db.CilsimetParkimit.FindAsync(id);
                if (getCilsimin == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Cilsimi with id {id} is not found"));
                }
                var getDetajet = await _db.Detajet.Where(n => n.CilsimetiId == getCilsimin.CilsimetiId).ToListAsync();
                if (getDetajet != null)
                {
                    foreach (var x in getDetajet) x.active = false;
                }

                getCilsimin.active = false;
                await _db.SaveChangesAsync();

                var response = ApiResponse<object>.NoContent($"Cilsimi with ID {id} has been deleted.");
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CilsimetReadDto>>.Error(500, "An error occurred deleting the data.", ex.Message));
            }
        }

        [HttpPut("{id:int}/Activate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ActivateCilsimet(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"The id {id} given is invalid"));
                }
                var getCilsimin = await _db.CilsimetParkimit.Where(a => a.active).FirstOrDefaultAsync(i => i.CilsimetiId == id);
                if (getCilsimin == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Cilsimi with id {id} is not found"));
                }
                var getCilsiminActiv = await _db.CilsimetParkimit.Where(a => a.active).Where(c => c.Selected == true && c.NjesiteId == getCilsimin.NjesiteId).ToListAsync();

                foreach (var item in getCilsiminActiv)
                {
                    item.Selected = false;
                }

                getCilsimin.Selected = true;

                await _db.SaveChangesAsync();

                var response = ApiResponse<object>.Ok(null, $"Cilsimi with ID {id} activated successfully.");
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CilsimetReadDto>>.Error(500, "An error occurred deleting the data.", ex.Message));
            }
        }
    }
}