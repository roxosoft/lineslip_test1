using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo1.LimitRequests;
using Demo1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Demo1.Controllers
{
    [ApiController]
    [Route("api")]
    public class DemoController : ControllerBase
    {
        private readonly ILogger<DemoController> _logger;
        private DemoContext _db;

        public DemoController(ILogger<DemoController> logger, DemoContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost("CreateNote")]
        public void CreateNote([FromBody]string note)
        {
            _db.Notes.Add(new Notes() { Note = note, Date = DateTime.UtcNow });
            _db.SaveChanges();
        }

        [HttpGet("Notes")]
        public IEnumerable<object> Notes(DateTime? startDate, DateTime? endDate)
        {
            var query = _db.Notes.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(p => p.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(p => p.Date < endDate.Value.AddDays(1));

            return query.OrderByDescending(p => p.Date).ToList();
        }

        [HttpGet("Ips")]
        public IEnumerable<object> Ips()
        {
            return _db.BlockedIps.OrderByDescending(p => p.Date).ToList();
        }

        //4 testing
        //[HttpGet("IP")]
        //public string IP()
        //{
        //    string ip = HttpContext.Connection.RemoteIpAddress.ToString();

        //    Limiter limiter = new Limiter(this._cache, this._db);

        //    return _limiter.CheckLimit(ip).ToString();
        //}
    }
}
