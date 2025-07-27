using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.StudentFestival
{
    public partial class StudentJonghapResult
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
            "남초부트랙", "남초부필드", "여초부트랙", "여초부필드"
        };
        List<string> middleList = new List<string>(){
            "남중부트랙", "남중부필드", "여중부트랙", "여중부필드"
        };
        List<string> highList = new List<string>(){
           "남고부트랙", "남고부필드", "여고부트랙", "여고부필드"
        };

        List<string> sectionList = new List<string>();

        List<string?> playerList = new List<string>();

        List<string?> detailList = new List<string>();

        public Z_GameResult model = new Z_GameResult();

        public string? cName { get; set; }
        public string? gName { get; set; }
        public string? sName { get; set; }
        public string? dName { get; set; }
        public string? partyName { get; set; }

        public string? part { get; set; }

        public bool isSelectTwo = false;
        public bool isSelectThree = false;
        public bool isSelectFour = false;
        public bool isShowMember = false;
        public bool isShowLoading = false;

        List<Z_GameResult> listGameResult = new List<Z_GameResult>();
        List<Z_PartyEntry> listPartyEnty = new List<Z_PartyEntry>();
        public string year { get; set; } = DateTime.Now.Year.ToString();

        // 성적처리 관련 변수
        string[] cityArr = new string[22];              // 시군명           세로
        string?[] detailArr = new string[40];           // 세부종목명        가로
        string[,] rankArr = new string[22, 40];         // 세부종목 순위
        double[,] jumsuArr = new double[22, 40];        // 세부종목 득점
        double[] sectionHapArr = new double[22];        // 시군 세부종목합계
        string[] sectionRankArr = new string[22];       // 시군 종별순위
        double[] sectionJumsukArr = new double[22];     // 시군 종별득점       부별종합 및 전체종합
        string[] sectionPartyArr = new string[22];      // 시군 종별 참가여부
        int[] detailRankSuArr = new int[40];            // 세부종목 입상자 수  세로
        double[] detailJumsuHapArr = new double[40];    // 세부종목 득점 합  세로
        int sectionRankSu;                              // 종별 순위 부여한 시군 수  (sectionRankArr !"" count)
        double sectionJumsuHap;                         // 종별 득점의 합           (sectionJumsukArr sum)
        int sectionpartySu;

        #endregion

        protected async override Task OnInitializedAsync()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;

            using var context = _contextFactory.CreateDbContext();
            Z_PartyName? pName = context.Z_PartyNames
                        .Where(it => it.partyName.Contains("학생체육대회")
                                    && it.etc == "사용" || it.etc == "마감" || it.etc == "열람" || it.etc == "진행")
                        .FirstOrDefault();
            partyName = pName.partyName;

            if (memberName == "경북교육청" || memberName == "관리자")
            {
                if (pName.etc == "마감" || pName.etc == "진행")
                {
                    isShowMember = true;
                }
            }
            else if (memberPart == "경기단체")
            {
                gName = memberName;
                sName = "종별선택";
                if (pName.etc == "열람" || pName.etc == "마감" || pName.etc == "진행")
                {
                    isShowMember = true;
                }
            }

            gName = "육상";

            // 시군 명 가져오기
            cityArr = appState.GetCities().ToArray();
        }

        private async Task SelectPart(ChangeEventArgs e)
        {
            part = e.Value.ToString();
            if (part == "육상종합")
            {
                await TrackFieldJonghap();
            }
            else
            {
                if (part == "초등부") sectionList = primaryList;
                else if (part == "중학부") sectionList = middleList;
                else if (part == "고등부") sectionList = highList;

                sName = "종별";
            }
        }

        private async Task TrackFieldJonghap()
        {
            isShowLoading = true;

            // 배열 재 선언
            int detailsu = 12;
            detailArr = new string[] {
                "남초부트랙", "남초부필드", "여초부트랙", "여초부필드",
                "남중부트랙", "남중부필드", "여중부트랙", "여중부필드",
                "남고부트랙", "남고부필드", "여고부트랙", "여고부필드"
            };
            rankArr = new string[22, detailsu];
            jumsuArr = new double[22, detailsu];
            detailRankSuArr = new int[detailsu];
            detailJumsuHapArr = new double[detailsu];
            sectionHapArr = new double[22];
            sectionRankArr = new string[22];
            sectionJumsukArr = new double[22];
            sectionPartyArr = new string[22];
            sectionRankSu = 0;
            sectionJumsuHap = 0;

            sName = "육상종합";

            using var context = _contextFactory.CreateDbContext();

            Z_JonghapResult? jongResult = new Z_JonghapResult();

            for (int y = 0; y < cityArr.Length; y++)
            {
                for (int x = 0; x < detailArr.Length; x++)
                {
                    jongResult = await context.Z_JonghapResults
                                 .Where(it => it.partyName==partyName
                                            && it.city == cityArr[y]
                                            && it.sName == detailArr[x])
                                 .FirstOrDefaultAsync();
                    rankArr[y, x] = jongResult==null ? "": jongResult.rank;
                    jumsuArr[y, x] = jongResult == null ? 0 : double.Parse(jongResult.jumsu);
                    sectionHapArr[y] += jumsuArr[y, x];
                    
                    detailJumsuHapArr[x] += jumsuArr[y, x];
                }
            }

            // 시군 득점 합에 의한 순위 생성
            if (sectionHapArr.Sum() > 0)
            {
                for (int i = 0; i < 22; i++)
                {
                    int L = 1;
                    for (int j = 0; j < 22; j++)
                    {
                        if (sectionHapArr[i] < sectionHapArr[j])
                        {
                            L++;
                        }
                        else if (sectionHapArr[i] == sectionHapArr[j])
                        {
                            int iOne = 0; int iTwo = 0; int iThree = 0;
                            int iFour = 0; int iFive = 0; int iSix = 0;
                            int iSeven = 0; int iEight = 0; int iNine = 0;
                            int iTen = 0; int iEleven = 0; int iTwelve = 0;
                            int jOne = 0; int jTwo = 0; int jThree = 0;
                            int jFour = 0; int jFive = 0; int jSix = 0;
                            int jSeven = 0; int jEight = 0; int jNine = 0;
                            int jTen = 0; int jEleven = 0; int jTwelve = 0;
                            for (int k = 0; k < detailsu; k++)
                            {
                                if (!string.IsNullOrEmpty(rankArr[i, k]))
                                {
                                    if (rankArr[i, k] == "1") iOne++;
                                    else if (rankArr[i, k] == "2") iTwo++;
                                    else if (rankArr[i, k] == "3") iThree++;
                                    else if (rankArr[i, k] == "4") iFour++;
                                    else if (rankArr[i, k] == "5") iFive++;
                                    else if (rankArr[i, k] == "6") iSix++;
                                    else if (rankArr[i, k] == "7") iSeven++;
                                    else if (rankArr[i, k] == "8") iEight++;
                                    else if (rankArr[i, k] == "9") iNine++;
                                    else if (rankArr[i, k] == "10") iTen++;
                                    else if (rankArr[i, k] == "11") iEleven++;
                                    else if (rankArr[i, k] == "12") iTwelve++;
                                }
                                if (!string.IsNullOrEmpty(rankArr[j, k]))
                                {
                                    if (rankArr[j, k] == "1") jOne++;
                                    else if (rankArr[j, k] == "2") jTwo++;
                                    else if (rankArr[j, k] == "3") jThree++;
                                    else if (rankArr[j, k] == "4") jFour++;
                                    else if (rankArr[j, k] == "5") jFive++;
                                    else if (rankArr[j, k] == "6") jSix++;
                                    else if (rankArr[j, k] == "7") jSeven++;
                                    else if (rankArr[j, k] == "8") jEight++;
                                    else if (rankArr[j, k] == "9") jNine++;
                                    else if (rankArr[j, k] == "10") jTen++;
                                    else if (rankArr[j, k] == "11") jEleven++;
                                    else if (rankArr[j, k] == "12") jTwelve++;
                                }
                                if (iOne < jOne)
                                {
                                    L++;
                                }
                                else if (iOne == jOne)
                                {
                                    if (iTwo < jTwo)
                                    {
                                        L++;
                                    }
                                    else if (iTwo == jTwo)
                                    {
                                        if (iThree < jThree)
                                        {
                                            L++;
                                        }
                                        else if (iFour == jFour)
                                        {
                                            if (iFour < jFour)
                                            {
                                                L++;
                                            }
                                            else if (iFive == jFive)
                                            {
                                                if (iSix < jSix)
                                                {
                                                    L++;
                                                }
                                                else if (iSix == jSix)
                                                {
                                                    if (iSeven < jSeven)
                                                    {
                                                        L++;
                                                    }
                                                    else if (iSeven == jSeven)
                                                    {
                                                        if (iEight < jEight)
                                                        {
                                                            L++;
                                                        }
                                                    }
                                                }
                                            }

                                        }

                                    }

                                }
                            }

                        }
                        //else if (sectionHapArr[i] == sectionHapArr[j] && i != j)
                        //{
                        //    await JSRuntimeInjector.InvokeVoidAsync("alert", "합계가 같습니다. " + cityArr[i] + ", " + cityArr[j]);
                        //}
                    }
                    sectionRankArr[i] = L.ToString();
                }
            }
            isShowLoading = false;
            StateHasChanged();
        }

        private void SelectSection(ChangeEventArgs e)
        {
            cNameList = appState.GetCities();

            sName = e.Value.ToString();

            switch (sName)
            {
                case "남초부트랙":
                    SectionJonghap("트랙", "남자초등부");
                    break;
                case "여초부트랙":
                    SectionJonghap("트랙", "여자초등부");
                    break;
                case "남중부트랙":
                    SectionJonghap("트랙", "남자중학부");
                    break;
                case "여중부트랙":
                    SectionJonghap("트랙", "여자중학부");
                    break;
                case "남고부트랙":
                    SectionJonghap("트랙", "남자고등부");
                    break;
                case "여고부트랙":
                    SectionJonghap("트랙", "여자고등부");
                    break;
                case "남초부필드":
                    SectionJonghap("필드", "남자초등부");
                    break;
                case "여초부필드":
                    SectionJonghap("필드", "여자초등부");
                    break;
                case "남중부필드":
                    SectionJonghap("필드", "남자중학부");
                    break;
                case "여중부필드":
                    SectionJonghap("필드", "여자중학부");
                    break;
                case "남고부필드":
                    SectionJonghap("필드", "남자고등부");
                    break;
                case "여고부필드":
                    SectionJonghap("필드", "여자고등부");
                    break;
            }
        }

        //// 남자초등부 트랙 종별성적
        //private void PrymaryManTrack()
        //{
        //    string eventname = "트랙";   // 수정
        //    string sectionname = "남자초등부";   // 수정

        //    SectionJonghap(eventname, sectionname);
        //}

        // 종별 성적처리
        private void SectionJonghap(string eventname, string sectionname)
        {
            isShowLoading = true;

            using var context = _contextFactory.CreateDbContext();
            // 세부종목 가져오기
            List<string?> listDname = new List<string>();
            listDname = context.Z_Details
                         .Where(it => it.partyName == "학생체육대회"
                                     && it.gName == gName
                                     && it.eventName == eventname
                                     && it.sName == sectionname)
                         .OrderBy(it => it.code.Length).ThenBy(it => it.code)
                         .Select(it => it.dName)
                         .ToList();

            int detailsu = listDname.Count();
            sectionpartySu = 22;

            // 배열 재 선언
            detailArr = new string[detailsu];
            rankArr = new string[22, detailsu];
            jumsuArr = new double[22, detailsu];
            detailRankSuArr = new int[detailsu];
            detailJumsuHapArr = new double[detailsu];
            sectionHapArr = new double[22];
            sectionRankArr = new string[sectionpartySu];
            sectionJumsukArr = new double[sectionpartySu];
            sectionPartyArr = new string[sectionpartySu];
            sectionRankSu = 0;
            sectionJumsuHap = 0;

            // 시군 참가 여부 가져오기
            if (eventname == "트랙")
            {
                foreach (var city in cityArr)
                {
                    int sParty = context.Z_PartyEntries
                                 .Where(it => it.partyName == partyName
                                            && it.gName == "육상"
                                            && it.sName == sectionname
                                            && (it.dNameOne.Contains("m") || it.dNameTwo.Contains("m")
                                                || it.dNameThree.Contains("m") || it.dNameFour.Contains("m"))
                                            && it.city == city
                                            && !string.IsNullOrEmpty(it.name))
                                .Count();
                    if (sParty == 0)
                    {
                        sectionHapArr[Array.IndexOf(cityArr, city)] = 0;
                        sectionPartyArr[Array.IndexOf(cityArr, city)] = "불참";
                        sectionpartySu--;
                    }
                }
            }
            else if (eventname == "필드")
            {
                foreach (var city in cityArr)
                {
                    int sParty = context.Z_PartyEntries
                                 .Where(it => it.partyName == partyName
                                            && it.gName == "육상"
                                            && it.sName == sectionname
                                            && (it.dNameOne.Contains("던지기") || it.dNameOne.Contains("뛰기")
                                                || it.dNameTwo.Contains("던지기") || it.dNameTwo.Contains("뛰기")
                                                || it.dNameThree.Contains("던지기") || it.dNameThree.Contains("뛰기")
                                                || it.dNameFour.Contains("던지기") || it.dNameFour.Contains("뛰기"))
                                            && it.city == city
                                            && !string.IsNullOrEmpty(it.name))
                                .Count();
                    if (sParty == 0)
                    {
                        sectionHapArr[Array.IndexOf(cityArr, city)] = 0;
                        sectionPartyArr[Array.IndexOf(cityArr, city)] = "불참";
                        sectionpartySu--;
                    }
                }
            }



            detailArr = listDname.ToArray();

            // 해당종별 결과 가져오기
            List<Z_GameResult> resultList = new List<Z_GameResult>();
            if (eventname == "트랙")
            {
                resultList = context.Z_GameResults
                                            .Where(it => it.partyName == partyName
                                                        && it.gName == gName
                                                        && it.sName == sectionname
                                                        && it.dName.Contains("m")
                                                        && (it.rank == "1" || it.rank == "2" || it.rank == "3"
                                                             || it.rank == "4" || it.rank == "5" || it.rank == "6"))
                                            .ToList();
            }
            else if (eventname == "필드")
            {
                resultList = context.Z_GameResults
                                            .Where(it => it.partyName == partyName
                                                        && it.gName == gName
                                                        && it.sName == sectionname
                                                        && (it.dName.Contains("뛰기") || it.dName.Contains("던지기"))
                                                        && (it.rank == "1" || it.rank == "2" || it.rank == "3"
                                                             || it.rank == "4" || it.rank == "5" || it.rank == "6"))
                                            .ToList();
            }

            // 자료가 없으면 빠져나간다
            if (resultList.Count == 0)
            {
                isShowLoading = false;
                return;
            }

            // 세부종목에 의한 성적 넣기
            for (int i = 0; i < 22; i++)
            {
                sectionHapArr[i] = 0;
            }
            int n = 0;
            int cityY;
            int detailX;
            foreach (var detailResult in resultList)
            {
                n++;
                cityY = Array.IndexOf(cityArr, detailResult.city);
                detailX = Array.IndexOf(detailArr, detailResult.dName);
                if (string.IsNullOrEmpty(rankArr[cityY, detailX]))
                {
                    rankArr[cityY, detailX] = detailResult.rank + ")";
                }
                else if (rankArr[cityY, detailX].Contains(")"))
                {
                    rankArr[cityY, detailX] = !rankArr[cityY, detailX].Contains(")") ? detailResult.rank + ")" : rankArr[cityY, detailX] + detailResult.rank + ")";
                }

                if (!string.IsNullOrEmpty(detailResult.specialOne))
                {
                    if (jumsuArr[cityY, detailX] < double.Parse(detailResult.specialOne))
                    {
                        jumsuArr[cityY, detailX] = double.Parse(detailResult.specialOne);
                        sectionHapArr[cityY] += jumsuArr[cityY, detailX];                   //시군 세부종목합계
                        detailRankSuArr[detailX]++;                                         // 세부종목별 입상자 수
                        detailJumsuHapArr[detailX] += jumsuArr[cityY, detailX];                                       // 세부종목별 득점합계
                    }
                }

            }

            // 시군 득점 합에 의한 순위 생성
            if (sectionHapArr.Sum() > 0)
            {
                for (int i = 0; i < 22; i++)
                {
                    if (sectionPartyArr[i] != "불참")
                    {
                        int L = 1;
                        for (int j = 0; j < 22; j++)
                        {
                            if (sectionPartyArr[j] != "불참")
                            {
                                if (sectionHapArr[i] < sectionHapArr[j])
                                {
                                    L++;
                                }
                                else if (sectionHapArr[i] == sectionHapArr[j])
                                {
                                    int iOne = 0; int iTwo = 0; int iThree = 0;
                                    int iFour = 0; int iFive = 0; int iSix = 0;
                                    int jOne = 0; int jTwo = 0; int jThree = 0;
                                    int jFour = 0; int jFive = 0; int jSix = 0;
                                    for (int k = 0; k < detailsu; k++)
                                    {
                                        if (!string.IsNullOrEmpty(rankArr[i, k]))
                                        {
                                            if (rankArr[i, k].Contains("1)")) iOne++;
                                            if (rankArr[i, k].Contains("2)")) iTwo++;
                                            if (rankArr[i, k].Contains("3)")) iThree++;
                                            if (rankArr[i, k].Contains("4)")) iFour++;
                                            if (rankArr[i, k].Contains("5)")) iFive++;
                                            if (rankArr[i, k].Contains("6)")) iSix++;

                                            if (!string.IsNullOrEmpty(rankArr[j, k]))
                                            {
                                                if (rankArr[j, k].Contains("1)")) jOne++;
                                                if (rankArr[j, k].Contains("2)")) jTwo++;
                                                if (rankArr[j, k].Contains("3)")) jThree++;
                                                if (rankArr[j, k].Contains("4)")) jFour++;
                                                if (rankArr[j, k].Contains("5)")) jFive++;
                                                if (rankArr[j, k].Contains("6)")) jSix++;
                                            }
                                        }
                                    }

                                    if (iOne < jOne)
                                    {
                                        L++;
                                    }
                                    else if (iOne == jOne)
                                    {
                                        if (iTwo < jTwo)
                                        {
                                            L++;
                                        }
                                        else if (iTwo == jTwo)
                                        {
                                            if (iThree < jThree)
                                            {
                                                L++;
                                            }
                                            else if (iFour == jFour)
                                            {
                                                if (iFour < jFour)
                                                {
                                                    L++;
                                                }
                                                else if (iFive == jFive)
                                                {
                                                    if (iSix < jSix)
                                                    {
                                                        L++;
                                                    }
                                                }

                                            }

                                        }

                                    }
                                }
                            }
                        }
                        sectionRankArr[i] = L.ToString();
                    }
                }
            }


            // 시군 순의에 의한 종별점수 생성
            int partyCitySu = sectionpartySu;
            for (int i = 0; i < 22; i++)
            {
                if (sectionPartyArr[i] != "불참")
                {
                    int sameRankSu = (from p in sectionRankArr
                                      where p == sectionRankArr[i]
                                      select p).Count();
                    if (sameRankSu == 1)
                    {
                        sectionJumsukArr[i] = partyCitySu - int.Parse(sectionRankArr[i]) + 1;
                    }
                    else if (sameRankSu > 1)
                    {
                        int nhap = 0;
                        int firstJumsu = partyCitySu - int.Parse(sectionRankArr[i]) + 1;
                        for (int j = firstJumsu; j > (firstJumsu - sameRankSu); j--)
                        {
                            nhap += j;
                        }
                        sectionJumsukArr[i] = ((double)nhap / (double)sameRankSu);
                    }
                }
            }

            // 종별 순위 부여 시군 수 및 종별 득점 합
            sectionRankSu = partyCitySu;
            sectionJumsuHap = sectionJumsukArr.Sum();

            if (sectionHapArr.Sum() != detailJumsuHapArr.Sum())
            {
                JSRuntimeInjector.InvokeVoidAsync("alert", "점수의 합이 틀립니다.");
            }
            else
            {
                //SaveJonghap();
            }

            isShowLoading = false;
        }

        // 세부종목명 줄 바꿈
        private string DivideString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";

            string rStr = str;
            string[] arr = str.Split("(");
            if (arr.Length == 2)
            {
                rStr = arr[0] + "<br />" + arr[1].Replace(")", "");
                //var html = new MarkupString(rStr);
                //return html;
            }

            return rStr;
        }

        // 0을 공백으로
        private string ZeroIsBlank(double su)
        {
            if (su == 0) return "";
            return su.ToString();
        }

        private void SaveJonghap()
        {
            using var context = _contextFactory.CreateDbContext();

            // 자료 지우기
            List<Z_JonghapResult> delResult = context.Z_JonghapResults
                                              .Where(it => it.partyName == partyName
                                                        && it.sName == sName)
                                              .ToList();
            context.Z_JonghapResults.RemoveRange(delResult);
            context.SaveChanges();

            // 저장하기
            List<Z_JonghapResult> saveResult = new List<Z_JonghapResult>();
            Z_JonghapResult jResult = new Z_JonghapResult();
            for (int i = 0; i < cityArr.Count(); i++)
            {
                jResult = new Z_JonghapResult();
                jResult.gName = "육상";
                jResult.sName = sName;
                jResult.city = cityArr[i];
                jResult.dukjum = sectionHapArr[i].ToString();
                jResult.rank = sectionRankArr[i];
                jResult.jumsu = sectionJumsukArr[i].ToString();
                jResult.partyName = partyName;
                jResult.year = DateTime.Now.Year.ToString();
                jResult.etc = sectionPartyArr[i];
                saveResult.Add(jResult);
            }
            context.AddRange(saveResult);
            context.SaveChanges();

            JSRuntimeInjector.InvokeVoidAsync("alert", "종합점수에 저장하였습니다.");
        }

        private void DwonLoadJonghap()
        {

        }

    }

}
