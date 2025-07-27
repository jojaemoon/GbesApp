using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.Marathon
{
    public partial class MarthonPartyCityInwon
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
        List<string> listCities = new List<string>();
        List<Inwon> listInwon = new List<Inwon>();

        protected override void OnInitialized()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;
            manager = appState.userManager;

            using var context = _contextFactory.CreateDbContext();

            Z_PartyName? pName = context.Z_PartyNames
                            .Where(it => it.partyName.Contains("구간마라톤대회")
                                        && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람"))
                            .FirstOrDefault();

            listPartyEntry = context.Z_PartyEntries
                                .Where(it => it.partyName == pName.partyName)
                                .ToList();

            listCities = appState.GetCities();

            CalculateInwon();

        }

        private void CalculateInwon()
        {
            Inwon inwon = new Inwon();
            int i = 0;
            foreach (var cname in listCities)
            {
                i++;
                inwon = new Inwon();
                inwon.no = i;
                inwon.cname = cname;
                inwon.primaryMan = listPartyEntry.Where(it => it.city == cname && (it.part == "남" && it.sName == "초등부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryWoman = listPartyEntry.Where(it => it.city == cname && (it.part == "여" && it.sName == "초등부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryHap = inwon.primaryMan + inwon.primaryWoman;
                inwon.middleMan = listPartyEntry.Where(it => it.city == cname && (it.part == "남" && it.sName == "중학부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleWoman = listPartyEntry.Where(it => it.city == cname && (it.part == "여" && it.sName == "중학부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleHap = inwon.middleMan + inwon.middleWoman;
                inwon.playerHap = inwon.primaryHap + inwon.middleHap;
                inwon.officer = 0;
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
            public string cname { get; set; }
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

