using Microsoft.EntityFrameworkCore;

namespace GBES.Models
{
    public class GBESportsRepositoty : IGBESportsRepository
    {
        private readonly GBESportsAppDbContext _context;
        private readonly ILogger _logger;

        public GBESportsRepositoty(GBESportsAppDbContext context, ILoggerFactory loggerFactory)
        {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(nameof(GBESportsAppDbContext));
        }

        public async Task<Z_Member> GetMember(string schoolName)
        {
            return await _context.Z_Members.Where(p => p.division == schoolName).SingleAsync();
        }
        
    }
}
