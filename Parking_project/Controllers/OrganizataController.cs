using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Parking_project.Services;

namespace Parking_project.Controllers
{
    [ApiController]
    [Route("api/organizata")]
    public class OrganizataController : ControllerBase
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        public OrganizataController(AplicationDbContext db, IMapper mapper, IAuthService authService)
        {
            _db = db;
            _mapper = mapper;
            _authService = authService;
        }


        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Organizata>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Organizata>>),StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Organizata>>>> GetOrganizata()
        {
            try
            {
                var organizata = await _db.Organizata.ToListAsync();

                var response = ApiResponse<IEnumerable<Organizata>>.Ok(organizata, "Organizatat retrieved successfully");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<Organizata>>.Error(500, "An Error Occurred while retrieving Org", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("Pagination")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OrgPage>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<OrgPage>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrgPage>>> GetOrganizataPagination(string? search, int? userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Page number and page size must be greater than zero."));
                }

                var query = _db.Organizata.AsNoTracking();

                if(search != null)
                {
                    var val = search.ToLower();
                    query = query.Where(x=> 
                            x.EmriBiznesit.ToLower().Contains(val) ||
                            x.Adresa.ToLower().Contains(val) ||
                            x.NumriBiznesit.ToLower().Contains(val) ||
                            x.NumriFiskal.ToLower().Contains(val) ||
                            x.NumriUnikIdentifikues.ToLower().Contains(val) ||
                            x.Komuna.ToLower().Contains(val)); 
                }

                if (userId != null) query = query.Where(o => _db.UserOrg.Any(uo => uo.BiznesId == o.BiznesId && uo.UserId == userId));

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var organizata = await query
                    .OrderByDescending(t => t.BiznesId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = _mapper.Map<List<OrgDTO>>(organizata);

                var page = new OrgPage
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    Data = result
                };

                var response = ApiResponse<OrgPage>.Ok(page, "Organizatat retrieved successfully");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<OrgPage>.Error(500, "An Error Occurred while retrieving Org", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
        
        [HttpGet("ForCustomers")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OrgPage>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<OrgPage>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrgPage>>> GetOrganizataForCustomers(string? search, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Page number and page size must be greater than zero."));
                }

                var query = _db.Organizata.AsNoTracking().Where(o => o.AllowCustomers);
                if (search != null)
                {
                    var val = search.ToLower();
                    query = query.Where(x =>
                            x.EmriBiznesit.ToLower().Contains(val) ||
                            x.Adresa.ToLower().Contains(val) ||
                            x.NumriBiznesit.ToLower().Contains(val) ||
                            x.NumriFiskal.ToLower().Contains(val) ||
                            x.NumriUnikIdentifikues.ToLower().Contains(val) ||
                            x.Komuna.ToLower().Contains(val));
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize!);

                var organizata = await query
                    .OrderByDescending(t => t.BiznesId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = _mapper.Map<List<OrgDTO>>(organizata);

                var page = new OrgPage
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    Data = result
                };

                var response = ApiResponse<OrgPage>.Ok(page, "Organizatat retrieved successfully");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<OrgPage>.Error(500, "An Error Occurred while retrieving Org", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Organizata>>> GetOrganizataByID(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<Organizata>.NotFound("Invalid ID supplied."));
                }

                var organizata = await _db.Organizata.FirstOrDefaultAsync(o => o.BiznesId == id);
                if (organizata == null)
                {
                    return NotFound(ApiResponse<Organizata>.NotFound($"Organizata with ID {id} not found."));
                }
                return Ok(ApiResponse<Organizata>.Ok(organizata, "Organizata retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Organizata>.Error(500, "An Error Occurred while retrieving Org", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Organizata>>> CreateOrganizata(OrgCreateDTO organizataDTO)
        {
            try
            {
                if (organizataDTO == null)
                {
                    return BadRequest(ApiResponse<Organizata>.BadRequest("Data is required"));
                }

                var duplicateName = await _db.Organizata
                    .FirstOrDefaultAsync(o => o.EmriBiznesit.ToLower() == organizataDTO.EmriBiznesit.ToLower());
                if (duplicateName != null)
                {
                    return Conflict(ApiResponse<Organizata>.Conflict($"An Organizata with the name '{organizataDTO.EmriBiznesit}' already exists."));
                }
                var duplicateNrIdentifikues = await _db.Organizata
                    .FirstOrDefaultAsync(o => o.NumriUnikIdentifikues == organizataDTO.NumriUnikIdentifikues);
                if (duplicateNrIdentifikues != null)
                {
                    return Conflict(ApiResponse<Organizata>.Conflict($"An Organizata with the unique identification number '{organizataDTO.NumriUnikIdentifikues}' already exists."));
                }
                var duplicateNrBiznesit = await _db.Organizata
                    .FirstOrDefaultAsync(o => o.NumriBiznesit == organizataDTO.NumriBiznesit);
                if (duplicateNrBiznesit != null)
                {
                    return Conflict(ApiResponse<Organizata>.Conflict($"An Organizata with the business number '{organizataDTO.NumriBiznesit}' already exists."));
                }
                var duplicateNrFiskal = await _db.Organizata
                    .FirstOrDefaultAsync(o => o.NumriFiskal == organizataDTO.NumriFiskal);
                if (duplicateNrFiskal != null)
                {
                    return Conflict(ApiResponse<Organizata>.Conflict($"An Organizata with the fiscal number '{organizataDTO.NumriFiskal}' already exists."));
                }
                
                if (await _authService.IsEmailExistsAsync(organizataDTO.Email))
                {
                    return Conflict(ApiResponse<UserReadDTO>.Conflict("Email already exists"));
                }


                Organizata organizata = _mapper.Map<Organizata>(organizataDTO);
                organizata.DataRegjistrimit = DateTime.UtcNow;

                
                await _db.Organizata.AddAsync(organizata);
                await _db.SaveChangesAsync();


                var defaultNjesia = new NjesiOrg
                {
                    Emri = organizata.EmriBiznesit,
                    BiznesId = organizata.BiznesId,
                    Kodi = organizata.NumriUnikIdentifikues,
                    Adresa = organizata.Adresa,
                };

                await _db.NjesiOrg.AddAsync(defaultNjesia);


                var defaultSherbim1 = new Sherbimi
                {
                    Emri = "Parking",
                    Cmimi = 0,
                    Organizata = organizata
                };

                await _db.Sherbimi.AddAsync(defaultSherbim1);
                await _db.SaveChangesAsync();

                var defaultCilsimi = new CilsimetParkimit
                {
                    Emri = "Sezona e Pare",
                    NjesiOrg = defaultNjesia,
                    Sherbimi = defaultSherbim1,
                    Selected = true
                };

                await _db.CilsimetParkimit.AddAsync(defaultCilsimi);
                await _db.SaveChangesAsync();


                if (organizataDTO.Admin != null)
                {
                    organizataDTO.Admin.UserOrgs =
                    [
                        new UserOrgCreateDto
                        {
                            BiznesId = organizata.BiznesId
                        },
                    ];

                    var user = await _authService.RegisterAsync(organizataDTO.Admin, "Admin");
                    if (user == null)
                    {
                        return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Registration of Admin failed"));
                    }
                }

                return CreatedAtAction(nameof(GetOrganizataByID),new {id = organizata.BiznesId}, ApiResponse<Organizata>.CreatedAt(organizata,"Organizata u krijua me sukses"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Organizata>.Error(500, "An Error Occurred while creating Org", ex.Message); 
                return StatusCode(500, errorResponse);
            }
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles = "Super Admin")]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Organizata>>> UpdateOrganizata(int id, OrgUpdateDTO organizataDTO)
        {
            try
            {
                if (organizataDTO == null)
                {
                    return BadRequest(ApiResponse<Organizata>.BadRequest("Data is required"));
                }

                if (id != organizataDTO.BiznesId)
                {
                    return BadRequest(ApiResponse<Organizata>.BadRequest("ID mismatch"));
                }

                var existingOrganizata = await _db.Organizata.FirstOrDefaultAsync(u => u.BiznesId == id);

                if (existingOrganizata == null)
                {
                    return NotFound(ApiResponse<Organizata>.NotFound($"Organizata with ID {id} not found."));
                }

                var duplicateName = await _db.Organizata
                    .FirstOrDefaultAsync(o => o.EmriBiznesit.ToLower() == organizataDTO.EmriBiznesit.ToLower() && o.BiznesId != id);
                if (duplicateName != null)
                {
                    return Conflict(ApiResponse<Organizata>.Conflict($"An Organizata with the name '{organizataDTO.EmriBiznesit}' already exists."));
                }
                var duplicateNrIdentifikues = await _db.Organizata
                    .FirstOrDefaultAsync(o => o.NumriUnikIdentifikues == organizataDTO.NumriUnikIdentifikues && o.BiznesId != id);
                if (duplicateNrIdentifikues != null)
                {
                    return Conflict(ApiResponse<Organizata>.Conflict($"An Organizata with the unique identification number '{organizataDTO.NumriUnikIdentifikues}' already exists."));
                }
                var duplicateNrBiznesit = await _db.Organizata
                    .FirstOrDefaultAsync(o => o.NumriBiznesit == organizataDTO.NumriBiznesit && o.BiznesId != id);
                if (duplicateNrBiznesit != null)
                {
                    return Conflict(ApiResponse<Organizata>.Conflict($"An Organizata with the business number '{organizataDTO.NumriBiznesit}' already exists."));
                }
                var duplicateNrFiskal = await _db.Organizata
                    .FirstOrDefaultAsync(o => o.NumriFiskal == organizataDTO.NumriFiskal && o.BiznesId != id);
                if (duplicateNrFiskal != null)
                {
                    return Conflict(ApiResponse<Organizata>.Conflict($"An Organizata with the fiscal number '{organizataDTO.NumriFiskal}' already exists."));
                }

                _mapper.Map(organizataDTO, existingOrganizata);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<OrgUpdateDTO>.Ok(organizataDTO,"Organizata created successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Organizata>.Error(500, "An Error Occurred while creating Org", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Super Admin")]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Organizata>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Organizata>>> DeleteOrganizata(int id)
        {
            try
            {
                var existingOrganizata = await _db.Organizata.FirstOrDefaultAsync(u => u.BiznesId == id);

                if (existingOrganizata == null)
                {
                    return NotFound(ApiResponse<Organizata>.NotFound($"Organizata with ID {id} not found."));
                }

                _db.Organizata.Remove(existingOrganizata);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<Organizata>.NoContent("Organizata deletet successfully"));

            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                var errorResponse = ApiResponse<Organizata>.Error(500, "An Error Occurred while deleting Org", innerMessage);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
