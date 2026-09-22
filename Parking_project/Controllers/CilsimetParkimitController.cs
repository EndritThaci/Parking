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
        [Authorize]
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
        [Authorize]
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
                    .Where(a => a.active && a.CilsimetiId == id)
                    .Select(c => new CilsimetReadDto
                    {
                        CilsimetiId = c.CilsimetiId,
                        Emri = c.Emri,
                        NjesiteId = c.NjesiteId,
                        SherbimiId = c.SherbimiId,
                        Selected = c.Selected,

                        NjesiOrg = c.NjesiOrg,
                        Sherbimi = c.Sherbimi,

                        Detajet = _db.Detajet
                            .Where(d => d.CilsimetiId == c.CilsimetiId)
                            .Select(d => new DetajetReadDto
                            {
                                DetajetId = d.DetajetId,
                                FromHour = d.FromHour,
                                ToHour = d.ToHour,
                                Cmimi = d.Cmimi,
                                CilsimetiId = d.CilsimetiId
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();
                if (cilsimetParking == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Cilsimi with ID {id} not found."));
                }

                var response = ApiResponse<CilsimetReadDto>.Ok(cilsimetParking, "Records retrieved successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred retrieving Cilsimin ", ex.Message);
                return StatusCode(500, errorResponse);
            }

        }

        [HttpPost]
        [Authorize]
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
        
        [HttpPost("Detail")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CilsimetReadDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CilsimetReadDto>>> CreateCilsimeWithDetails(CilsimetWithDetailsCreateDTO cilsimetDto)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();    
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

                CilsimetParkimit cilsimetParkimit = _mapper.Map<CilsimetParkimit>(cilsimetDto);
                cilsimetParkimit.Selected = false;

                await _db.CilsimetParkimit.AddAsync(cilsimetParkimit);
                await _db.SaveChangesAsync();

                var detajet = cilsimetDto.Detajet ?? new List<DetajetCreateDto>();
                for (int i = 0; i < detajet.Count; i++)
                {
                    for (int j = i + 1; j < detajet.Count; j++)
                    {
                        var first = detajet[i];
                        var second = detajet[j];

                        bool conflict =
                            (first.FromHour == second.FromHour)
                            || (first.FromHour <= second.FromHour && first.ToHour > second.FromHour)
                            || (first.FromHour < second.ToHour && first.ToHour >= second.ToHour)
                            || (first.FromHour <= second.ToHour && first.ToHour == null)
                            || (first.FromHour <= second.FromHour && first.ToHour == null)
                            || (first.ToHour <= second.ToHour && first.FromHour >= second.FromHour)
                            || (first.ToHour > second.FromHour && second.ToHour == null);

                        if (conflict)
                        {
                            await transaction.RollbackAsync();
                            return Conflict( ApiResponse<object>.Conflict( $"Time conflict between Detail {i + 1} and Detail {j + 1}."));
                        }
                    }
                }

                var detajetToAdd = new List<Detajet>();
                foreach (var detajetDto in detajet)
                {
                    var detajetModel = _mapper.Map<Detajet>(detajetDto);
                    detajetModel.CilsimetiId = cilsimetParkimit.CilsimetiId;

                    detajetToAdd.Add(detajetModel);
                }
                await _db.AddRangeAsync(detajetToAdd);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = ApiResponse<CilsimetReadDto>.CreatedAt(_mapper.Map<CilsimetReadDto>(cilsimetParkimit), "Cilsimi has been added Successfully");
                return CreatedAtAction(nameof(GetCilsimetById), new { id = cilsimetParkimit.CilsimetiId }, response); ;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<IEnumerable<CilsimetReadDto>>.Error(500, "An error occurred while processing your request.", ex.Message));
            }

        }

        [HttpPut("{id:int}")]
        [Authorize]
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

        [HttpPut("Detail/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CilsimetReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CilsimetReadDto>>> UpdateCilsimeWithDetails(int id, CilsimetWithDetailsUpdateDTO cilsimetDto)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (cilsimetDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Data is invalid."));
                }

                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"The Cilsimet Id {id} is invalid."));
                }

                if (id != cilsimetDto.CilsimetiId)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"Id mismatch."));
                }

                if (cilsimetDto.NjesiteId <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"The Njesi Id {cilsimetDto.NjesiteId} is invalid."));
                }

                if (cilsimetDto.SherbimiId <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"The Sherbimi Id {cilsimetDto.SherbimiId} is invalid."));
                }

                var getNjesi = await _db.NjesiOrg.AsNoTracking().FirstOrDefaultAsync(n => n.NjesiteId == cilsimetDto.NjesiteId && n.active);
                if (getNjesi == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"The Njesi Id {cilsimetDto.NjesiteId} is not found."));
                }

                var getSherbim = await _db.Sherbimi.AsNoTracking().FirstOrDefaultAsync(s => s.SherbimiId == cilsimetDto.SherbimiId && s.active);
                if (getSherbim == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"The Sherbimi Id {cilsimetDto.SherbimiId} is not found."));
                }

                if (getSherbim.BiznesId != getNjesi.BiznesId)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Sherbimi and Njesia are not in the same Organization."));
                }

                var cilsimet = await _db.CilsimetParkimit.FirstOrDefaultAsync(c => c.CilsimetiId == id && c.active);
                if (cilsimet == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Cilsimet with Id {id} was not found."));
                }

                cilsimet.Emri = cilsimetDto.Emri;
                cilsimet.NjesiteId = cilsimetDto.NjesiteId;
                cilsimet.SherbimiId = cilsimetDto.SherbimiId;

                var detajet = await _db.Detajet.Where(d => d.CilsimetiId == id).ToListAsync();

                var incomingDetails = cilsimetDto.Detajet ?? new List<DetajetUpdateDto>();
                var incomingDetailIds = incomingDetails
                    .Where(d => d.DetajetId > 0)
                    .Select(d => d.DetajetId)
                    .Distinct()
                    .ToHashSet();

                var detailsToRemove = detajet
                    .Where(d => !incomingDetailIds.Contains(d.DetajetId))
                    .ToList();

                for (int i = 0; i < incomingDetails.Count; i++)
                {
                    for (int j = i + 1; j < incomingDetails.Count; j++)
                    {
                        var first = incomingDetails[i];
                        var second = incomingDetails[j];

                        bool conflict =
                            (first.FromHour == second.FromHour)
                            || (first.FromHour <= second.FromHour && first.ToHour > second.FromHour)
                            || (first.FromHour < second.ToHour && first.ToHour >= second.ToHour)
                            || (first.FromHour <= second.ToHour && first.ToHour == null)
                            || (first.FromHour <= second.FromHour && first.ToHour == null)
                            || (first.ToHour <= second.ToHour && first.FromHour >= second.FromHour)
                            || (first.ToHour > second.FromHour && second.ToHour == null);

                        if (conflict)
                        {
                            await transaction.RollbackAsync();
                            return Conflict(ApiResponse<object>.Conflict($"Time conflict between Detail {i + 1} and Detail {j + 1}."));
                        }
                    }
                }

                if (detailsToRemove.Count > 0)
                {
                    _db.Detajet.RemoveRange(detailsToRemove);
                }
                
                var incomingDetajet = detajet.Where(d => incomingDetailIds.Contains(d.DetajetId)).ToList();
                var DetajetDict = incomingDetajet.ToDictionary(d=> d.DetajetId);

                var DetailsToAdd = new List<Detajet>();
                foreach (var detailDto in incomingDetails)
                {
                    if (detailDto.DetajetId > 0)
                    {
                        if (!DetajetDict.TryGetValue(detailDto.DetajetId.Value, out var existingDetail))
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(ApiResponse<object>.BadRequest( $"Detajet with Id {detailDto.DetajetId} does not belong to Cilsimet {id}."));
                        }

                        existingDetail.FromHour = detailDto.FromHour;
                        existingDetail.ToHour = detailDto.ToHour;
                        existingDetail.Cmimi = detailDto.Cmimi;
                    }
                    else
                    {
                        var newDetail = _mapper.Map<Detajet>(detailDto);
                        newDetail.CilsimetiId = cilsimet.CilsimetiId;
                        DetailsToAdd.Add(newDetail);
                    }
                }
                if (DetailsToAdd.Count > 0)
                {
                    await _db.Detajet.AddRangeAsync(DetailsToAdd);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = ApiResponse<CilsimetReadDto>.Ok(_mapper.Map<CilsimetReadDto>(cilsimet), "The Cilsimet and its Details have been updated successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing your request.", ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
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
                var getTransaksion = await _db.TransaksionParkimi.Where(c => c.CilsimiId == id).FirstOrDefaultAsync();
                if (getTransaksion == null)
                {
                    _db.CilsimetParkimit.Remove(getCilsimin);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var getDetajet = await _db.Detajet.Where(n => n.CilsimetiId == getCilsimin.CilsimetiId).ToListAsync();
                    if (getDetajet != null)
                    {
                        _db.Detajet.RemoveRange(getDetajet);
                        await _db.SaveChangesAsync();
                    }

                    getCilsimin.active = false;
                    await _db.SaveChangesAsync();
                }

                var response = ApiResponse<object>.NoContent($"Cilsimi with ID {id} has been deleted.");
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CilsimetReadDto>>.Error(500, "An error occurred deleting the data.", ex.Message));
            }
        }

        [HttpPut("{id:int}/Activate")]
        [Authorize]
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