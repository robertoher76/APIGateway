using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibroAPI.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace LibroAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly LibroContext _context;
        private readonly IConnectionMultiplexer _redis;

        public LibrosController(LibroContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        // GET: api/Libros
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibros()
        {
            var db = _redis.GetDatabase();
            string cacheKey = "libroList";
            var librosCache = await db.StringGetAsync(cacheKey);
            if (!librosCache.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<List<Libro>>(librosCache);
            }
            var libros = await _context.Libros.ToListAsync();
            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(libros), TimeSpan.FromMinutes(10));
            return libros;
        }

        // GET: api/Libros/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Libro>> GetLibro(int id)
        {
            var db = _redis.GetDatabase();
            string cacheKey = "libro_" + id.ToString();
            var libroCache = await db.StringGetAsync(cacheKey);

            if (!libroCache.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<Libro>(libroCache);
            }
            var libro = await _context.Libros.FindAsync(id);
            if (libro == null)
            {
                return NotFound();
            }
            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(libro), TimeSpan.FromSeconds(10));
            return libro;
        }

        // PUT: api/Libros/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLibro(int id, Libro libro)
        {
            if (id != libro.Id)
            {
                return BadRequest();
            }

            _context.Entry(libro).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                var db = _redis.GetDatabase();
                string cacheKeyLibro = "libro_" + id.ToString();
                string cacheKeyList = "libroList";
                await db.KeyDeleteAsync(cacheKeyLibro);
                await db.KeyDeleteAsync(cacheKeyList);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LibroExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Libros
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Libro>> PostLibro(Libro libro)
        {
            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();
            var db = _redis.GetDatabase();
            string cacheKeyList = "libroList";
            await db.KeyDeleteAsync(cacheKeyList);
            return CreatedAtAction("GetLibro", new { id = libro.Id }, libro);
        }

        // DELETE: api/Libros/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLibro(int id)
        {
            var libro = await _context.Libros.FindAsync(id);
            if (libro == null)
            {
                return NotFound();
            }

            _context.Libros.Remove(libro);
            await _context.SaveChangesAsync();
            var db = _redis.GetDatabase();
            string cacheKeyLibro = "libro_" + id.ToString();
            string cacheKeyList = "libroList";
            await db.KeyDeleteAsync(cacheKeyLibro);
            await db.KeyDeleteAsync(cacheKeyList);
            return NoContent();
        }

        private bool LibroExists(int id)
        {
            return _context.Libros.Any(e => e.Id == id);
        }
    }
}
