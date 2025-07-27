using GBES.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace GBES.Pages
{
    public partial class Index111
    {
        List<Z_Event> _events;

        [Inject]
        public IDbContextFactory<GBESportsAppDbContext>? _contextFactory { get; set; }

       
    }
}
