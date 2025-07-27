using GBES.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GBES.Services;

namespace GBES.Pages
{
    public partial class MemberInfo
    {
        // 인정 가져오기 [1]
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        private ClaimsPrincipal AuthenticationStateProviderUser { get; set; }

        public Z_Member? model = new Z_Member();

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

        string? memberName;

        bool isMember = false;

        //List<LSPartyEntry> listParty = new List<LSPartyEntry>();
        //List<LSGameOffice> listOffices = new List<LSGameOffice>();
        public string year { get; set; } = DateTime.Now.Year.ToString();

        protected override async Task OnInitializedAsync()
        {
            // 인정 가져오기 [2]
            var authState = await authenticationStateTask;
            var user = authState.User;
            memberName = user.Identity.Name;
            memberName = appState.userName;

            using var context = _contextFactory.CreateDbContext();

            model = await context.Z_Members
                    .Where(it => it.memberName == memberName)
                    .SingleOrDefaultAsync();

            if (model is not null)
            {
                isMember = true;

                StateHasChanged();
            }

        }

        

        protected async Task Save_Click()
        {
            using var context = _contextFactory.CreateDbContext();
            context.Z_Members.Update(model);
            await context.SaveChangesAsync();
        }

        protected async Task Cancel_Click()
        {
            using var context = _contextFactory.CreateDbContext();
            model = await context.Z_Members
                    .Where(it => it.memberName == memberName)
                    .SingleOrDefaultAsync();
            StateHasChanged();
        }

    }
}