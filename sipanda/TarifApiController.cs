using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;

namespace Sipanda.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TarifApiController : ControllerBase
    {
        private readonly SipandaDbContext _context;

        public TarifApiController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: api/TarifApi
        [HttpGet]
        public async Task<IActionResult> GetTarif(
            [FromQuery] string? keyword,
            [FromQuery] int? faskesId,
            [FromQuery] int? kategoriId,
            [FromQuery] int limit = 50)
        {
            var query = _context.VTarifLengkap.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(t => 
                    (t.NamaKomponen != null && t.NamaKomponen.ToLower().Contains(kw)) ||
                    (t.NamaKategori != null && t.NamaKategori.ToLower().Contains(kw)));
            }

            if (faskesId.HasValue)
            {
                query = query.Where(t => t.FaskesId == faskesId.Value);
            }

            if (kategoriId.HasValue)
            {
                query = query.Where(t => t.KategoriId == kategoriId.Value);
            }

            var items = await query.Take(Math.Min(limit, 200)).ToListAsync();
            return Ok(new { total = items.Count, data = items });
        }

        // GET: api/TarifApi/faskes
        [HttpGet("faskes")]
        public async Task<IActionResult> GetFaskes()
        {
            var list = await _context.MasterFaskes
                .Include(f => f.Rekening)
                .Select(f => new
                {
                    f.Id,
                    f.KodeFaskes,
                    f.NamaFaskes,
                    KodeRekening = f.Rekening!.KodeRekening,
                    NamaRekening = f.Rekening!.NamaRekening
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/TarifApi/kategori
        [HttpGet("kategori")]
        public async Task<IActionResult> GetKategori([FromQuery] int? parentId)
        {
            var query = _context.MasterKategori.AsQueryable();
            if (parentId.HasValue)
            {
                query = query.Where(k => k.ParentId == parentId.Value);
            }

            var list = await query
                .Select(k => new
                {
                    k.Id,
                    k.NamaKategori,
                    k.ParentId
                })
                .OrderBy(k => k.NamaKategori)
                .ToListAsync();

            return Ok(list);
        }
    }
}
