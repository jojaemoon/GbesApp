using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.YearTrack
{
    public partial class YearTrackCityInwonTotal
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

        public string year { get; set; } = DateTime.Now.Year.ToString();

        string? memberName;
        string? memberPart;
        string? memberCity;
        string? manager;

        List<Z_PartyEntry> listPartyEntry = new List<Z_PartyEntry>();
        List<string> listGname = new List<string>();
        List<Inwon> listInwon = new List<Inwon>();

        int memberID = 0;

        protected async override Task OnInitializedAsync()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;
            manager = appState.userManager;

            using var context = _contextFactory.CreateDbContext();

            memberID = context.Z_Members
                       .Where(it => it.memberName == memberName)
                       .Select(it => it.Id)
                       .FirstOrDefault();

            Z_PartyName? pName = context.Z_PartyNames
                            .Where(it => it.partyName.Contains("학년별 육상경기대회")
                                        && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람"))
                            .FirstOrDefault();

            if (memberPart == "학교")
            {
                listPartyEntry = context.Z_PartyEntries
                                 .Where(it => it.partyName == pName.partyName
                                            && it.schoolName == memberName)
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

            listGname = appState.GetGameNames("학년별 육상경기대회");

            if (memberPart == "학교" || memberPart == "교육지원청")
            {
                CalculateInwonCity();
            }
            else
            {
                CalculateInwon();
            }

        }


        public string ZeroIsBlank(string str)
        {
            if (str == "0") return "";
            return str;
        }

        public async Task PrintTable()
        {
            string url = $"/YearTrackCityInwonTotal_Print?id={memberID}";
            await JSRuntimeInjector.InvokeVoidAsync("open", new object[] { url, "_blank" });
        }

        // Print.js 에서 인쇄  =>  웹에서 에러
        private async Task PrintComponent()
        {
            await JSRuntimeInjector.InvokeVoidAsync("printComponent", "#printId");
        }

        class Inwon
        {
            public int no { get; set; }
            public string name { get; set; }
            public int pMan_3 { get; set; }
            public int pMan_4 { get; set; }
            public int pMan_5 { get; set; }
            public int pMan_6 { get; set; }
            public int primaryManHap { get; set; }
            public int pWoman_3 { get; set; }
            public int pWoman_4 { get; set; }
            public int pWoman_5 { get; set; }
            public int pWoman_6 { get; set; }
            public int primaryWomanHap { get; set; }
            public int primaryHap { get; set; }
            public int mMan_1 { get; set; }
            public int mMan_2 { get; set; }
            public int mMan_3 { get; set; }
            public int middleManHap { get; set; }
            public int mWoman_1 { get; set; }
            public int mWoman_2 { get; set; }
            public int mWoman_3 { get; set; }
            public int middleWomanHap { get; set; }
            public int middleHap { get; set; }

            public int playerManHap { get; set; }
            public int playerWomanHap { get; set; }
            public int playerHap { get; set; }

            public int officer { get; set; }
            public int totalHap { get; set; }

        }

        // 총 집계
        private void CalculateInwon()
        {
            List<string> list = appState.GetCities();
            Inwon inwon = new Inwon();
            int i = 0;
            foreach (var cname in list)
            {
                i++;
                inwon = new Inwon();
                inwon.no = i;
                inwon.name = cname;
                inwon.pMan_3 = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자초등부" && it.schoolYear == "3") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pMan_4 = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자초등부" && it.schoolYear == "4") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pMan_5 = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자초등부" && it.schoolYear == "5") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pMan_6 = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자초등부" && it.schoolYear == "6") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryManHap = inwon.pMan_3 + inwon.pMan_4 + inwon.pMan_5 + inwon.pMan_6;

                inwon.pWoman_3 = listPartyEntry.Where(it => it.city == cname && (it.sName == "여자초등부" && it.schoolYear == "3") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pWoman_4 = listPartyEntry.Where(it => it.city == cname && (it.sName == "여자초등부" && it.schoolYear == "4") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pWoman_5 = listPartyEntry.Where(it => it.city == cname && (it.sName == "여자초등부" && it.schoolYear == "5") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pWoman_6 = listPartyEntry.Where(it => it.city == cname && (it.sName == "여자초등부" && it.schoolYear == "6") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryWomanHap = inwon.pWoman_3 + inwon.pWoman_4 + inwon.pWoman_5 + inwon.pWoman_6;

                inwon.primaryHap = inwon.primaryManHap + inwon.primaryWomanHap;

                inwon.mMan_1 = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자중학부" && it.schoolYear == "1") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.mMan_2 = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자중학부" && it.schoolYear == "2") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.mMan_3 = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자중학부" && it.schoolYear == "3") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleManHap = inwon.mMan_1 + inwon.mMan_2 + inwon.mMan_3;

                inwon.mWoman_1 = listPartyEntry.Where(it => it.city == cname && (it.sName == "여자중학부" && it.schoolYear == "1") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.mWoman_2 = listPartyEntry.Where(it => it.city == cname && (it.sName == "여자중학부" && it.schoolYear == "2") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.mWoman_3 = listPartyEntry.Where(it => it.city == cname && (it.sName == "여자중학부" && it.schoolYear == "3") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleWomanHap = inwon.mWoman_1 + inwon.mWoman_2 + inwon.mWoman_3;

                inwon.middleHap = inwon.middleManHap + inwon.middleWomanHap;

                inwon.playerManHap = inwon.primaryManHap + inwon.middleManHap;
                inwon.playerWomanHap = inwon.primaryWomanHap + inwon.middleWomanHap;
                inwon.playerHap = inwon.primaryHap + inwon.middleHap;

                inwon.officer = listPartyEntry.Where(it => it.city == cname && !string.IsNullOrEmpty(it.special)).Select(it => it.special).Distinct().Count();
                inwon.totalHap = inwon.playerHap + inwon.officer;

                listInwon.Add(inwon);
            }
        }

        int GetSchoolTypeOrder(string schoolName)
        {
            if (schoolName.Contains("초등")) return 0; // 초등학교 우선
            if (schoolName.Contains("중")) return 1;   // 중학교 그 다음
            return 2;                                 // 그 외(고등 등)
        }

        // 교육청 집계
        private void CalculateInwonCity()
        {
            List<string> schoolnames = listPartyEntry.Select(it => it.schoolName).Where(s => !string.IsNullOrEmpty(s)).OrderBy(GetSchoolTypeOrder).Distinct().ToList();
            Inwon inwon = new Inwon();
            int i = 0;
            foreach (var schoolname in schoolnames)
            {
                i++;
                inwon = new Inwon();
                inwon.no = i;
                inwon.name = schoolname;
                inwon.pMan_3 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "남자초등부" && it.schoolYear == "3") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pMan_4 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "남자초등부" && it.schoolYear == "4") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pMan_5 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "남자초등부" && it.schoolYear == "5") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pMan_6 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "남자초등부" && it.schoolYear == "6") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryManHap = inwon.pMan_3 + inwon.pMan_4 + inwon.pMan_5 + inwon.pMan_6;

                inwon.pWoman_3 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "여자초등부" && it.schoolYear == "3") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pWoman_4 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "여자초등부" && it.schoolYear == "4") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pWoman_5 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "여자초등부" && it.schoolYear == "5") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.pWoman_6 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "여자초등부" && it.schoolYear == "6") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryWomanHap = inwon.pWoman_3 + inwon.pWoman_4 + inwon.pWoman_5 + inwon.pWoman_6;

                inwon.primaryHap = inwon.primaryManHap + inwon.primaryWomanHap;

                inwon.mMan_1 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "남자중학부" && it.schoolYear == "1") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.mMan_2 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "남자중학부" && it.schoolYear == "2") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.mMan_3 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "남자중학부" && it.schoolYear == "3") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleManHap = inwon.mMan_1 + inwon.mMan_2 + inwon.mMan_3;

                inwon.mWoman_1 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "여자중학부" && it.schoolYear == "1") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.mWoman_2 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "여자중학부" && it.schoolYear == "2") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.mWoman_3 = listPartyEntry.Where(it => it.schoolName == schoolname && (it.sName == "여자중학부" && it.schoolYear == "3") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleWomanHap = inwon.mWoman_1 + inwon.mWoman_2 + inwon.mWoman_3;

                inwon.middleHap = inwon.middleManHap + inwon.middleWomanHap;

                inwon.playerManHap = inwon.primaryManHap + inwon.middleManHap;
                inwon.playerWomanHap = inwon.primaryWomanHap + inwon.middleWomanHap;
                inwon.playerHap = inwon.playerManHap + inwon.playerWomanHap;

                inwon.officer = listPartyEntry.Where(it => it.schoolName == schoolname && !string.IsNullOrEmpty(it.special)).Select(it => it.special).Distinct().Count();
                inwon.totalHap = inwon.playerHap + inwon.officer;

                listInwon.Add(inwon);
            }

        }

        private string NameSplit(string schoolname)
        {
            string name = schoolname;

            name = name.Replace("중학교", "중");
            name = name.Replace("초등학교", "초");


            return name;
        }
    }
}