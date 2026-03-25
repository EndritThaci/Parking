using AutoMapper;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Parking_project.Controllers
{
    [ApiController]
    [Route("api/sherbimet")]
    public class SherbimetController : ControllerBase
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;

        public SherbimetController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sherbimi>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sherbimi>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Sherbimi>>>> GetSherbimi()
        {
            try
            {
                var sherbimet = await _db.Sherbimi.Where(a => a.active).Include(o => o.Organizata).ToListAsync();
                return Ok(ApiResponse<IEnumerable<Sherbimi>>.Ok(sherbimet, "Lokacionet retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Sherbimi>>.Error(500, "An Error Occurred while retrieving sherimet", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("ByOrg")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sherbimi>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sherbimi>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Sherbimi>>>> GetSherbimiByOrg()
        {
            try
            {
                int orgId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var sherbimet = await _db.Sherbimi.Where(a => a.active).Include(o => o.Organizata).Where(s => s.BiznesId == orgId).ToListAsync();
                return Ok(ApiResponse<IEnumerable<Sherbimi>>.Ok(sherbimet, "Lokacionet retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Sherbimi>>.Error(500, "An Error Occurred while retrieving sherimet", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Sherbimi>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Sherbimi>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Sherbimi>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<Sherbimi>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<Sherbimi>>> GetSherbimiById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<Sherbimi>.BadRequest("Id must be grater than 0"));
                }
                var sherbimet = await _db.Sherbimi.Where(a => a.active).Include(o => o.Organizata).FirstOrDefaultAsync(s => s.SherbimiId == id);
                if (sherbimet == null)
                {
                    return NotFound(ApiResponse<Sherbimi>.NotFound("Sherbimi nuk ekziston"));
                }

                return Ok(ApiResponse<Sherbimi>.Ok(sherbimet, "Sherbimet retrievet siccessfully"));
            }
            catch (Exception ex)
            {
                var response = ApiResponse<Sherbimi>.Error(500, "An Error Occurred while retrieving Sherbimet", ex.Message);
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<SherbimiCreateDTO>>> CreateSherbimin(SherbimiCreateDTO sherbimiDto)
        {
            try
            {
                if (sherbimiDto == null)
                    return BadRequest(ApiResponse<SherbimiCreateDTO>.BadRequest("Invalid Data"));

                var organizataExists = await _db.Organizata.AnyAsync(n => n.BiznesId == sherbimiDto.BiznesId);
                if (!organizataExists)
                    return NotFound(ApiResponse<SherbimiCreateDTO>.NotFound($"Organizata with id {sherbimiDto.BiznesId} doesn't exist"));

                var sherbimi = _mapper.Map<Sherbimi>(sherbimiDto);

                await _db.Sherbimi.AddAsync(sherbimi);
                await _db.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetSherbimiById),
                    new { id = sherbimi.SherbimiId },
                    ApiResponse<SherbimiCreateDTO>.CreatedAt(sherbimiDto, "Sherbimi created successfully")
                );
            }
            catch (Exception ex)
            {
                var response = ApiResponse<SherbimiCreateDTO>.Error(500, "An Error Occurred while creating Sherbimet", ex.Message);
                return StatusCode(500, response);
            }

        }

        [HttpPut("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<SherbimiUpdateDTO>>> UpdateSherbimi(int id, SherbimiUpdateDTO sherbimiDto)
        {
            try
            {
                if (sherbimiDto == null || id != sherbimiDto.SherbimiId || sherbimiDto.Cmimi < 0)
                    return BadRequest(ApiResponse<SherbimiUpdateDTO>.BadRequest("Invalid Data"));

                var existing = await _db.Sherbimi.Where(a => a.active).FirstOrDefaultAsync(s => s.SherbimiId == sherbimiDto.SherbimiId);

                if (existing == null)
                    return NotFound(ApiResponse<SherbimiUpdateDTO>.NotFound($"Sherbimi with ID {id} not found"));

                _mapper.Map(sherbimiDto, existing);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<SherbimiUpdateDTO>.Ok(sherbimiDto, "Sherbimi updated successfully"));
            }
            catch (Exception ex)
            {
                var response = ApiResponse<SherbimiUpdateDTO>.Error(500, "An Error Occurred while updating Lokacioni", ex.Message);
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<SherbimiCreateDTO>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Sherbimi>>> DeleteSherbimi(int id)
        {
            try
            {
                var sherbimi = await _db.Sherbimi.FirstOrDefaultAsync(s => s.SherbimiId == id);

                if (sherbimi == null)
                    return NotFound(ApiResponse<Sherbimi>.NotFound("Sherbimi not found"));

                var getCilsimi = await _db.CilsimetParkimit.Where(n => n.SherbimiId == sherbimi.SherbimiId).ToListAsync();
                if (getCilsimi != null)
                {
                    foreach (var x in getCilsimi) x.active = false;
                }
                var getDetajet = await _db.Detajet.Where(n => n.CilsimetParkimit.SherbimiId == sherbimi.SherbimiId).ToListAsync();
                if (getDetajet != null)
                {
                    foreach (var x in getDetajet) x.active = false;
                }

                sherbimi.active = false;
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<Sherbimi>.NoContent("Sherbimi deleted successfully"));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                var response = ApiResponse<Sherbimi>.Error(500, "An Error Occurred while deleting Lokacioni", innerMessage);
                return StatusCode(500, response);
            }

        }

    }
}