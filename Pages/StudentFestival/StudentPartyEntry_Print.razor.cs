using DocumentFormat.OpenXml.Bibliography;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.StudentFestival
{
    public partial class StudentPartyEntry_Print
    {
        [Parameter]
        public int Id { get; set; }

        [Parameter]
        public string? gName { get; set; }
        [Parameter]
        public string? sName { get; set; }
        [Parameter]
        public string? partyName { get; set; }
        [Parameter]
        public string? city { get; set; }

        [Inject]
        public AppState? appState { get; set; }

        [Inject]
        public NavigationManager? NavigationManagerInjector { get; set; }
        [Inject]
        public IJSRuntime? JSRuntimeInjector { get; set; }
        [Inject]
        public IDbContextFactory<GBESportsAppDbContext>? _contextFactory { get; set; }



        public async Task PrintTable()
        {
            await JSRuntimeInjector.InvokeVoidAsync("open", new object[] { $"/GWPartyEntryNew_Print/{partyName}/{city}/{gName}/{sName}", "_blank" });
        }
    }
}
