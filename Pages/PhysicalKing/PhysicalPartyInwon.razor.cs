using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.PhysicalKing
{
    public partial class PhysicalPartyInwon
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

        string? partyName = "";

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
                        .Where(it => it.partyName.Contains("경상북도 체력인증제")
                                    && it.etc == "체력사용")
                        .FirstOrDefault();
            partyName = pName.partyName;
            //if (memberPart == "학교")
            //{
            //    listPartyEntry = context.Z_PartyEntries
            //                     .Where(it => it.partyName == pName.partyName
            //                                && it.schoolName == memberName)
            //                     .ToList();
            //}
            //else if (memberPart == "교육지원청")
            //{
            //    listPartyEntry = context.Z_PartyEntries
            //                     .Where(it => it.partyName == pName.partyName
            //                                && it.city == memberCity)
            //                     .ToList();
            //}
            //else
            //{
            //    listPartyEntry = context.Z_PartyEntries
            //                     .Where(it => it.partyName == pName.partyName)
            //                     .ToList();
            //}

            listPartyEntry = context.Z_PartyEntries
                                .Where(it => it.partyName == partyName)
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
                inwon.primaryMan = listPartyEntry.Where(it => it.city == cname && (it.schoolName.Contains("초등") && it.code=="남") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryWoman = listPartyEntry.Where(it => it.city == cname && (it.schoolName.Contains("초등") && it.code == "여") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryHap = inwon.primaryMan + inwon.primaryWoman;
                inwon.middleMan = listPartyEntry.Where(it => it.city == cname && (it.schoolName.Contains("중학") && it.code == "남") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleWoman = listPartyEntry.Where(it => it.city == cname && (it.schoolName.Contains("중학") && it.code == "여") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleHap = inwon.middleMan + inwon.middleWoman;
                inwon.highMan = listPartyEntry.Where(it => it.city == cname && (it.schoolName.Contains("고등") && it.code == "남") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.highWoman = listPartyEntry.Where(it => it.city == cname && (it.schoolName.Contains("고등") && it.code == "여") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.highHap = inwon.highMan + inwon.highWoman;
                inwon.playerHap = inwon.primaryHap + inwon.middleHap + inwon.highHap;
                inwon.officer = listPartyEntry.Where(it => it.city == cname && !string.IsNullOrEmpty(it.special)).Select(it => it.special).Distinct().Count();
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

