using AutoMapper;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Security.Cryptography;

namespace Parking_project.Controllers
{
    [Route("api/LibriShitjes")]
    [ApiController]
    public class LibriShitjesController : Controller
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;
        public LibriShitjesController(AplicationDbContext db, IMapper map)
        {
            _db = db;
            _mapper = map;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<byte[]>>> GetLibrin([FromQuery] LibriShitjesCreateDTO dto)
        {
            if (dto.id == null && dto.day == null && (dto.month == null || dto.year == null) && dto.year == null && (dto.all == null || dto.all == false))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Please provide the parameters."));
            }

            int org = int.Parse(User.FindFirst("BiznesId")?.Value ?? "0");

            List<TransaksionParkimi> transaksionet = new();

            var query = _db.TransaksionParkimi.Where(i => i.Njesia.BiznesId == org)
                .Include(t => t.Cilsimet)
                    .ThenInclude(c => c.Sherbimi)
                .Include(t => t.Vendi)
                    .ThenInclude(v => v.Lokacioni)
                        .ThenInclude(l => l.NjesiOrg)
                .Include(t => t.User)
                .AsQueryable();

            if (dto.id != null)
                query = query.Where(t => t.TransaksioniId == dto.id);

            if (dto.day != null)
            {
                var targetDate = dto.day.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(t => t.KohaHyrjes.Date == targetDate);
            }

            if (dto.year != null)
            {
                query = query.Where(t => t.KohaHyrjes.Year == dto.year);
                if (dto.month != null)
                    query = query.Where(t => t.KohaHyrjes.Month == dto.month);
            }

            if (dto.njesia != null)
                query = query.Where(t => t.NjesiaId == dto.njesia);

            transaksionet = await query.ToListAsync();

            if (!transaksionet.Any()) return NotFound(ApiResponse<byte[]>.NotFound("No transactions found."));

            var transaksionIds = transaksionet.Select(t => t.TransaksioniId).ToList();

            var getSherbimet = await _db.TransaksionDetaj.Where(c => transaksionIds.Contains(c.TransaksionId)).Include(c => c.Sherbimi).ToListAsync();

            if (transaksionet.Count() == 0)
            {
                return NotFound(ApiResponse<object>.NotFound("No transactions found."));
            }

            var result = transaksionet.Select(t => new TransaksionRead
            {
                TransaksioniId = t.TransaksioniId,
                KohaHyrjes = t.KohaHyrjes,
                KohaDaljes = t.KohaDaljes,
                Cmimi = getSherbimet.Where(i => i.TransaksionId == t.TransaksioniId).Sum(c => c.Cmimi),
                Statusi = t.Statusi,
                Vendi = t.Vendi,
                Cilsimi = t.Cilsimet,
                Useri = t.User,
                Sherbimi = getSherbimet.Where(d => d.TransaksionId == t.TransaksioniId).Select(d => d.Sherbimi).ToList(),
            }).ToList();

            foreach (var rez in result)
            {
                rez.Useri.Passwordi = "";

                if (rez.Statusi == "Pending")
                {
                    rez.Cmimi = null;
                    rez.KohaDaljes = null;
                    rez.Sherbimi = null;
                }

            }

            var file = BookToExcel(result);

            return Ok(new ApiResponse<byte[]>
            {
                Success = true,
                Data = file,
                Message = "Transactions retrieved successfully."
            });

        }

        private byte[] BookToExcel(List<TransaksionRead> transactions)
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Libri i Shitjeve");

            // ===== HEADER =====
            worksheet.Cells[1, 1].Value = "Transaksioni ID";
            worksheet.Cells[1, 2].Value = "Koha Hyrjes";
            worksheet.Cells[1, 3].Value = "Koha Daljes";
            worksheet.Cells[1, 4].Value = "Useri";
            worksheet.Cells[1, 5].Value = "Sherbimet";
            worksheet.Cells[1, 6].Value = "Cmimi";
            worksheet.Cells[1, 7].Value = "TVSH%";
            worksheet.Cells[1, 8].Value = "TVSH";
            worksheet.Cells[1, 9].Value = "Cmimi pa TVSH";

            using (var header = worksheet.Cells[1, 1, 1, 9])
            {
                header.Style.Font.Bold = true;
                header.Style.Fill.PatternType = ExcelFillStyle.Solid;
                header.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
            }

            int row = 2;

            foreach (var transaction in transactions)
            {
                worksheet.Cells[row, 1].Value = transaction.TransaksioniId;

                worksheet.Cells[row, 2].Value = transaction.KohaHyrjes;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "dd.MM.yyyy HH:mm";

                worksheet.Cells[row, 3].Value = transaction.KohaDaljes;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "dd.MM.yyyy HH:mm";

                worksheet.Cells[row, 4].Value = transaction.Useri?.Emri;

                worksheet.Cells[row, 5].Value = string.Join(", ",
                    transaction.Sherbimi?.Select(s => s.Emri) ?? Enumerable.Empty<string>());

                worksheet.Cells[row, 6].Value = Math.Round(transaction.Cmimi ?? 0, 2);
                worksheet.Cells[row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[row, 7].Value = 18;

                worksheet.Cells[row, 8].Formula = $"F{row}*G{row}/100";

                worksheet.Cells[row, 9].Formula = $"F{row}-H{row}";
                worksheet.Cells[row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                row++;
            }

            // ===== TOTAL =====
            if (row > 3)
            {
                worksheet.Cells[row, 5].Value = "Totali:";
                worksheet.Cells[row, 5].Style.Font.Bold = true;
                worksheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);

                worksheet.Cells[row, 6].Formula = $"SUM(F2:F{row - 1})";
                worksheet.Cells[row, 6].Style.Font.Bold = true;
                worksheet.Cells[row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);

                worksheet.Cells[row, 7].Value = 18;
                worksheet.Cells[row, 7].Style.Font.Bold = true;
                worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);

                worksheet.Cells[row, 8].Formula = $"SUM(H2:H{row - 1})";
                worksheet.Cells[row, 8].Style.Font.Bold = true;
                worksheet.Cells[row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);

                worksheet.Cells[row, 9].Formula = $"SUM(I2:I{row - 1})";
                worksheet.Cells[row, 9].Style.Font.Bold = true;
                worksheet.Cells[row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }
    }
}