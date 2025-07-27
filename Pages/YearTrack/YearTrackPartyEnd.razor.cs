using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.JSInterop;

namespace GBES.Pages.YearTrack
{
    public partial class YearTrackPartyEnd
    {
        [Inject]
        public AppState appState { get; set; }
        [Inject]
        public NavigationManager? NavigationManagerInjector { get; set; }

        [Inject]
        public IJSRuntime? JSRuntimeInjector { get; set; }

        [Inject]
        public IDbContextFactory<GBESportsAppDbContext>? _contextFactory { get; set; }

        public string year { get; set; } = DateTime.Now.Year.ToString();

        string? memberName;
        string? memberPart;

        bool isUse = false;

        bool isEnb = false;

        private string? saveResult;
        private string gameEnd = "마감"; // 또는 "사용"

        protected async override Task OnInitializedAsync()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;

            using var context = _contextFactory.CreateDbContext();

            string? gameEnd = context.Z_Events
                             .Where(it => it.year == year && it.specialTwo.Contains("육상경기대회") && it.gName == "육상")
                             .Select(it => it.specialThree)
                             .FirstOrDefault();
            if (gameEnd == "마감")
            {
                isUse = false;
                isEnb = true;
            }
            else if (gameEnd == "사용")
            {
                isUse = true;
                isEnb = false;
            }
        }

        private void OnUseChanged(ChangeEventArgs e)
        {
            bool checkedValue = (bool)e.Value;
            isUse = checkedValue;
            if (checkedValue)
            {
                isEnb = false;
            }
        }

        private void OnEndChanged(ChangeEventArgs e)
        {
            bool checkedValue = (bool)e.Value;
            isEnb = checkedValue;
            if (checkedValue)
            {
                isUse = false;
            }
        }

        private void Save()
        {
            //if (isUse)
            //    gameEnd = "사용";
            //else if (isEnb)
            //    gameEnd = "마감";
            //else
            //    gameEnd = "미설정";

            //saveResult = $"현재 상태: {gameEnd}";
            // 실제 저장 로직 (예: DB 저장 등)은 여기에 추가

            using var context = _contextFactory.CreateDbContext();
            Z_Event? gameEnd = context.Z_Events
                             .Where(it => it.year == year && it.specialTwo.Contains("육상경기대회") && it.gName == "육상")
                             .FirstOrDefault();

            if (gameEnd != null)
            {
                if (isUse) gameEnd.specialThree = "사용";
                if (isEnb) gameEnd.specialThree = "마감";

                context.SaveChanges();
            }

        }
    }
}
