using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OfficeOpenXml;
using System.Security.Claims;

namespace GBES.Pages.StudentFestival
{
    public partial class StudentPartyGameInwon
    {

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

        List<Z_PartyEntry> listPartyEntry = new List<Z_PartyEntry>();
        List<string> listGname = new List<string>();
        List<Inwon> listInwon = new List<Inwon>();

        protected async override Task OnInitializedAsync()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;
            manager = appState.userManager;

            using var context = _contextFactory.CreateDbContext();

            Z_PartyName? pName = context.Z_PartyNames
                            .Where(it => it.partyName.Contains("학생체육대회")
                                        && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람"))
                            .FirstOrDefault();

            if (memberPart == "학교")
            {
                listPartyEntry = context.Z_PartyEntries
                                 .Where(it => it.partyName==pName.partyName
                                            && it.schoolName== memberName)
                                 .ToList();
            }
            else if (memberPart == "교육지원청")
            {
                listPartyEntry = context.Z_PartyEntries
                                 .Where(it => it.partyName == pName.partyName
                                            && it.city == memberCity)
                                 .ToList();
            }
            else
            {
                listPartyEntry = context.Z_PartyEntries
                                 .Where(it => it.partyName == pName.partyName)
                                 .ToList();
            }

            listGname = appState.GetGameNames("학생체육대회");

            CalculateInwon();

        }

        private void CalculateInwon()
        {
            Inwon inwon = new Inwon();
            int i = 0;
            foreach (var gname in listGname)
            {
                i++;
                inwon = new Inwon();
                inwon.no = i;
                inwon.gName = gname;
                inwon.primaryMan = listPartyEntry.Where(it=>it.gName == gname && (it.sName=="남자초등부" || it.sName == "초등부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryWoman = listPartyEntry.Where(it => it.gName == gname && it.sName == "여자초등부" && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryHap = inwon.primaryMan + inwon.primaryWoman;
                inwon.middleMan = listPartyEntry.Where(it => it.gName == gname && (it.sName == "남자중학부" || it.sName == "중학부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleWoman = listPartyEntry.Where(it => it.gName == gname && it.sName == "여자중학부" && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleHap = inwon.middleMan + inwon.middleWoman;
                inwon.highMan = listPartyEntry.Where(it => it.gName == gname && (it.sName == "남자고등부" || it.sName == "고등부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.highWoman = listPartyEntry.Where(it => it.gName == gname && it.sName == "여자고등부" && !string.IsNullOrEmpty(it.name)).Count();
                inwon.highHap = inwon.highMan + inwon.highWoman;
                inwon.playerHap = inwon.primaryHap + inwon.middleHap + inwon.highHap;
                inwon.officer = listPartyEntry.Where(it => it.gName == gname && !string.IsNullOrEmpty(it.special)).Select(it=>it.special).Distinct().Count();
                inwon.totalHap = inwon.playerHap + inwon.officer;

                listInwon.Add(inwon);
            }
        }

        public string ZeroIsBlank(string str)
        {
            if (str == "0") return "";
            return str;
        }
        class Inwon
        {
            public int no { get; set; }
            public string gName { get; set; }
            public int primaryMan { get; set; }
            public int primaryWoman { get; set; }
            public int primaryHap { get; set; }
            public int middleMan { get; set; }
            public int middleWoman { get; set; }
            public int middleHap { get; set; }
            public int highMan { get; set; }
            public int highWoman { get; set; }
            public int highHap { get; set; }
            public int playerHap { get; set; }
            public int officer { get; set; }
            public int totalHap { get; set; }
        }
    }
}
