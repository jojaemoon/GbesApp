using DocumentFormat.OpenXml.Spreadsheet;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages
{
    public partial class Logout
    {
        [Inject]
        public AppState appState { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IDbContextFactory<GBESportsAppDbContext>? _contextFactory { get; set; }

        [Inject]
        public IJSRuntime? JSRuntimeInjector { get; set; }

        protected async override Task OnInitializedAsync()
        {
            appState.SetLogin("", "", "", "");

            await JSRuntimeInjector.InvokeVoidAsync("loginOut");

            NavigationManager.NavigateTo("/");
        }

        //protected async Task override OnInitializedAsync()
        //{
        //    appState.SetLogin("", "", "", "");

        //    await JSRuntimeInjector.InvokeVoidAsync("loginOut");

        //    NavigationManager.NavigateTo("/");
        //}
    }
}
