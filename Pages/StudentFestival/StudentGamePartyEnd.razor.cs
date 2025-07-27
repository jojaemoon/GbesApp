using DocumentFormat.OpenXml.InkML;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace GBES.Pages.StudentFestival
{
    public partial class StudentGamePartyEnd
    {
        #region 변수 등 선언
        // 인정 가져오기 [1]
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        private ClaimsPrincipal AuthenticationStateProviderUser { get; set; }

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
        string? memberPart;
        string? memberCity;
        string? manager;

        bool isMember = true;

        bool isDisabledCity = true;
        bool isDisabledSchool = true;

        bool isEdit = false;

        List<string>? cNameList = new List<string>();
        List<string>? schoolNameList = new List<string>();
        List<string>? gNameList = new List<string>();
        List<string>? sNameList = new List<string>();
        List<string>? dNameList = new List<string>();

        public Z_PartyEntry model = new Z_PartyEntry();

        public string? cName { get; set; }
        public string? gName { get; set; }
        public string? sName { get; set; }
        public string? partyName { get; set; }

        public string? showOrHideClass { get; set; } = "hideCss";
        public bool isSelectOne = true;
        public bool isSelectTwo = false;
        public bool isSelectThree = false;
        public bool isSelectFour = false;

        string btnStyle = "btn-success";

        List<Z_Event> events = new List<Z_Event>();
        public string year { get; set; } = DateTime.Now.Year.ToString();

        #endregion

        protected override void OnInitialized()
        {
            // 인정 가져오기 [2]
            //var authState = await authenticationStateTask;
            //var user = authState.User;
            //memberName = user.Identity.Name;

            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;
            manager = appState.userManager;

            using var context = _contextFactory.CreateDbContext();
            events = context.Z_Events
                     .Where(it => it.year == year && it.specialTwo.Contains("학생체육"))
                     .OrderBy(it => it.gName)
                     .ToList();
        }

        private async Task BtnClick(Z_Event game)
        {
            if (game.specialThree == "사용")
            {
                game.specialThree = "마감";
            }
            else if (game.specialThree == "마감")
            {
                game.specialThree = "사용";
            }

            using var context = _contextFactory.CreateDbContext();
            context.Z_Events.Update(game);
            await context.SaveChangesAsync();

            StateHasChanged();
        }
    }
}
