using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.BoyFestival
{
    public partial class BoySelection
    {
        string? memberName;
        string? memberPart;
        string? memberCity;
        string? manager;

        #region Injectors
        [Inject]
        public AppState appState { get; set; }
        [Inject]
        public NavigationManager? NavigationManagerInjector { get; set; }

        [Inject]
        public IJSRuntime? JSRuntimeInjector { get; set; }

        [Inject]
        public IDbContextFactory<GBESportsAppDbContext>? _contextFactory { get; set; }
        #endregion

        protected override async Task OnInitializedAsync()
        {
            // 인정 가져오기 [2]
            //var authState = await authenticationStateTask;
            //var user = authState.User;
            //memberName = user.Identity.Name;


            //var authState = await authenticationStateTask;
            //var user = authState.User;
            //memberName = user.Identity.Name;
            memberName = appState.userName;

            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;
            manager = appState.userManager;

        }
    }
}
