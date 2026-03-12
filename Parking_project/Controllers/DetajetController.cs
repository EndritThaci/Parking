using AutoMapper;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace Parking_project.Controllers
{
    [ApiController]
    [Route("api/Detajet")]
    public class DetajetController : Controller
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;
        public DetajetController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<DetajetReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DetajetReadDto>>>> GetDetajet()
        {
            try
            {
                var detajetList = await _db.Detajet.Include(c => c.CilsimetParkimit).ThenInclude(n => n.NjesiOrg).ThenInclude(o => o.Organizata).ToListAsync();
                if (detajetList == null)
                {
                    return BadRequest(ApiResponse<object>.NotFound("No Detajet found."));
                }
                var detajetDtoList = _mapper.Map<IEnumerable<DetajetReadDto>>(detajetList);
                var response = ApiResponse<IEnumerable<DetajetReadDto>>.Ok(detajetDtoList, "Detajet retrieved successfuly");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }

        [HttpGet]
        [Authorize]
        [Route("ByOrg")]
        [ProducesResponseType(typeof(ApiResponse<DetajetReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DetajetReadDto>>>> GetDetajetByOrg()
        {
            try
            {
                int orgId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var detajetList = await _db.Detajet.Include(c => c.CilsimetParkimit).ThenInclude(n => n.NjesiOrg).ThenInclude(o => o.Organizata)
                    .Where(o=>o.CilsimetParkimit.NjesiOrg.BiznesId == orgId).ToListAsync();
                if (detajetList == null)
                {
                    return BadRequest(ApiResponse<object>.NotFound("No Detajet found."));
                }
                var detajetDtoList = _mapper.Map<IEnumerable<DetajetReadDto>>(detajetList);
                var response = ApiResponse<IEnumerable<DetajetReadDto>>.Ok(detajetDtoList, "Detajet retrieved successfuly");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }
        
        [HttpGet]
        [Authorize]
        [Route("ByNjesi")]
        [ProducesResponseType(typeof(ApiResponse<DetajetReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DetajetReadDto>>>> GetDetajetByNjesi()
        {
            try
            {
                int njeisaId = int.Parse(User.FindFirst("NjesiaId")!.Value);
                var detajetList = await _db.Detajet.Include(c => c.CilsimetParkimit).ThenInclude(n => n.NjesiOrg).ThenInclude(o => o.Organizata)
                    .Where(o=>o.CilsimetParkimit.NjesiteId == njeisaId).ToListAsync();
                if (detajetList == null)
                {
                    return BadRequest(ApiResponse<object>.NotFound("No Detajet found."));
                }
                var detajetDtoList = _mapper.Map<IEnumerable<DetajetReadDto>>(detajetList);
                var response = ApiResponse<IEnumerable<DetajetReadDto>>.Ok(detajetDtoList, "Detajet retrieved successfuly");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<DetajetReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DetajetReadDto>>> GetDetajetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid id parameter."));
                }
                var detajet = await _db.Detajet.FindAsync(id);
                if (detajet == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Detajet with the given id {id} doesnt exists."));
                }
                var detajetDto = _mapper.Map<DetajetReadDto>(detajet);
                return Ok(ApiResponse<DetajetReadDto>.Ok(detajetDto, "Detajet retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));

            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DetajetReadDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<DetajetReadDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DetajetReadDto>>> CreateDetajet(DetajetCreateDto DetajeCreateDto)
        {
            try
            {
                if (DetajeCreateDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid data has been send."));
                }

                var getCilsimet = await _db.CilsimetParkimit.FindAsync(DetajeCreateDto.CilsimetiId);
                if (getCilsimet == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Cilsimet with the given id {DetajeCreateDto.CilsimetiId} doesnt exists."));
                }

            var getDetajet = await _db.Detajet.Where(c=> c.CilsimetiId == DetajeCreateDto.CilsimetiId)
                    .Where(d => (d.FromHour == DetajeCreateDto.FromHour) 
                    || (d.FromHour <= DetajeCreateDto.FromHour && d.ToHour > DetajeCreateDto.FromHour) 
                    || (d.FromHour < DetajeCreateDto.ToHour && d.ToHour >= DetajeCreateDto.ToHour) 
                    || (d.FromHour <= DetajeCreateDto.ToHour && d.ToHour==null)
                    || (d.FromHour <= DetajeCreateDto.FromHour && d.ToHour == null)
                    || (d.ToHour <= DetajeCreateDto.ToHour && d.FromHour >= DetajeCreateDto.FromHour)
                    || (d.ToHour > DetajeCreateDto.FromHour && DetajeCreateDto.ToHour ==null)).FirstOrDefaultAsync();
            if (getDetajet != null)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Invalid time has been set"));
            }
               
                var detajetModel = _mapper.Map<Detajet>(DetajeCreateDto);
                await _db.Detajet.AddAsync(detajetModel);
                await _db.SaveChangesAsync();

                var response = ApiResponse<DetajetReadDto>.CreatedAt(_mapper.Map<DetajetReadDto>(detajetModel), "Detajet created successfully.");

                return CreatedAtAction(nameof(GetDetajetById), new { id = detajetModel.DetajetId }, response); ;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<DetajetUpdateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DetajetUpdateDto>>> UpdateDetajet(int id, DetajetUpdateDto detajeUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"Invalid id {id} parameter."));
                }
                if (detajeUpdateDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid data has been send."));
                }

                var findDetajet = await _db.Detajet.FindAsync(id);
                if (findDetajet == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Detajet with the given id {id} doesnt exists."));
                }

                //var getDetajet = await _db.Detajet.Where(d=> d.CilsimetiId == findDetajet.CilsimetiId)
                //    .Where(d => (d.FromHour == detajeUpdateDto.FromHour) 
                //    || (d.FromHour <= detajeUpdateDto.FromHour && d.ToHour > detajeUpdateDto.FromHour) 
                //    || (d.FromHour < detajeUpdateDto.ToHour && d.ToHour >= detajeUpdateDto.ToHour) 
                //    || (d.FromHour <= detajeUpdateDto.ToHour && d.ToHour == null)
                //    || (d.FromHour <= detajeUpdateDto.FromHour && d.ToHour == null)
                //    || (d.ToHour <= detajeUpdateDto.ToHour && d.FromHour >= detajeUpdateDto.FromHour)
                //    || (d.ToHour > detajeUpdateDto.FromHour && detajeUpdateDto.ToHour == null)).FirstOrDefaultAsync();

                if(detajeUpdateDto.ToHour > detajeUpdateDto.FromHour)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid Time has been given"));
                }

                var getDetajet = await _db.Detajet.Where(d => d.CilsimetiId == findDetajet.CilsimetiId)
                    .Where(d => (detajeUpdateDto.ToHour == null || d.FromHour < detajeUpdateDto.ToHour)
                    && (d.ToHour == null || d.ToHour > detajeUpdateDto.FromHour)).FirstOrDefaultAsync();

                if (getDetajet != null && getDetajet.DetajetId != id)
                {
                    return Conflict(ApiResponse<object>.Conflict("Time conflict for this Cilsim"));
                }

                _mapper.Map(detajeUpdateDto, findDetajet);
                await _db.SaveChangesAsync();

                var response = ApiResponse<DetajetUpdateDto>.Ok(detajeUpdateDto, "Cilsimeti updated successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<DetajetUpdateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DetajetReadDto>>> DeleteDetajinById(int id)
        {
            try
            {
                if(id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"The id {id} is invalid"));
                }
                var findDetajin = await _db.Detajet.FindAsync(id);
                if (findDetajin == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"The Details with id {id} you trying to delete is not found"));
                }
                _db.Detajet.Remove(findDetajin);
                await _db.SaveChangesAsync();

                var response = ApiResponse<object>.NoContent($"Detaji with ID {id} has been deleted.");
                return Ok(response);

            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }
    }
}