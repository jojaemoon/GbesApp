using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.StudentFestival
{
    public partial class StudentJongHapCityByul
    {
        #region 변수 등 선언

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

        bool isEdit = false;
        bool addDisabled = true;

        int detailInwon = 1;

        List<string?> cNameList = new List<string>();
        List<string?> schoolNameList = new List<string>();
        List<double> hapList = new List<double>();
        List<int> partRankList = new List<int>();
        List<string> partyList = new List<string>();

        List<string> jonghapList = new List<string>() {
            "육상종합", "초등부", "중학부", "고등부"
        };
        List<string> primaryList = new List<string>(){
            "초등부종합", "남초부트랙", "남초부필드", "여초부트랙", "여초부필드"
        };
        List<string> middleList = new List<string>(){
            "중학부종합", "남중부트랙", "남중부필드", "여중부트랙", "여중부필드"
        };
        List<string> highList = new List<string>(){
            "고등부종합", "남고부트랙", "남고부필드", "여고부트랙", "여고부필드"
        };

        List<string> sectionList = new List<string>();

        List<string?> playerList = new List<string>();

        List<string?> detailList = new List<string>();

        public Z_GameResult model = new Z_GameResult();

        public string? cName { get; set; }
        public string? sName { get; set; }
        public string? partyName { get; set; }


        public string year { get; set; } = DateTime.Now.Year.ToString();

        // 성적처리 관련 변수
        string[] cityArr = new string[22];              // 시군명           세로
        List<Z_JonghapResult> listJonghap = new List<Z_JonghapResult>();

        List<string> partList = new List<string>() {
            "남초부트랙", "남초부필드", "여초부트랙", "여초부필드"
            , "남중부트랙", "남중부필드", "여중부트랙", "여중부필드", "남고부트랙", "남고부필드", "여고부트랙", "여고부필드"
        };
        #endregion
        protected async override Task OnInitializedAsync()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;

            using var context = _contextFactory.CreateDbContext();
            Z_PartyName? pName = context.Z_PartyNames
                        .Where(it => it.partyName.Contains("학생체육대회")
                                    && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람") || it.etc == "진행")
                        .FirstOrDefault();
            partyName = pName.partyName;

            // 시군 명 가져오기
            cityArr = appState.GetCities().ToArray();
        }

        private void SelectSection(ChangeEventArgs e)
        {
            sName = e.Value.ToString();
            cName = "시/군";
        }

        private async Task SelectCity(ChangeEventArgs e)
        {
            cName = e.Value.ToString();

            using var context = _contextFactory.CreateDbContext();
            if (sName == "육상종합")
            {
                cName = "시/군";
            }
            else
            {
                string sectionName = sName.Substring(0, 3);
                string partName = sName.Substring(3, 2);
                if (sectionName == "남초부") sectionName = "남자초등부";
                if (sectionName == "남중부") sectionName = "남자중학부";
                if (sectionName == "남고부") sectionName = "남자고등부";
                if (sectionName == "여초부") sectionName = "여자초등부";
                if (sectionName == "여중부") sectionName = "여자중학부";
                if (sectionName == "여고부") sectionName = "여자고등부";

                // 해당 부별 자료 가져오기  
                List<Z_GameResult> listGameResult = new List<Z_GameResult>();
                if (partName == "트랙")
                {
                    listGameResult = await context.Z_GameResults
                                 .Where(it => it.partyName == partyName
                                            && it.city == cName
                                            && it.gName == "육상"
                                            && it.sName == sectionName
                                            && it.dName.Contains("m"))
                                 .ToListAsync();
                }
                else if (partName == "필드")
                {
                    listGameResult = await context.Z_GameResults
                                 .Where(it => it.partyName == partyName
                                            && it.city == cName
                                            && it.gName == "육상"
                                            && it.sName == sectionName
                                            && (it.dName.Contains("던지기")
                                                 || it.dName.Contains("뛰기")))
                                 .ToListAsync();
                }

                listJonghap = new List<Z_JonghapResult>();
                Z_JonghapResult? jonghapResult = new Z_JonghapResult();
                foreach (var item in listGameResult)
                {
                    jonghapResult = new Z_JonghapResult();
                    jonghapResult.gName = item.dName;
                    jonghapResult.sName = sName;
                    jonghapResult.city = item.city;
                    jonghapResult.dukjum = item.specialOne;
                    jonghapResult.jumsu = "";
                    jonghapResult.rank = item.rank;
                    listJonghap.Add(jonghapResult);
                }
                jonghapResult = new Z_JonghapResult();
                jonghapResult = await context.Z_JonghapResults
                                .Where(it => it.partyName == partyName
                                            && it.city == cName
                                            && it.sName == sName)
                                .FirstOrDefaultAsync();
                listJonghap.Add(jonghapResult);
            }
            
        }
    }
}
