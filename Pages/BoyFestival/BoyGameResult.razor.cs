using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace GBES.Pages.BoyFestival
{
    public partial class BoyGameResult
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
        List<string?> gNameList = new List<string>();
        List<string?> sNameList = new List<string>();
        List<string?> dNameList = new List<string>();
        List<string?> playerList = new List<string>();

        public Z_GameResult model = new Z_GameResult();

        public string? cName { get; set; }
        public string? gName { get; set; }
        public string? sName { get; set; }
        public string? dName { get; set; }
        public string? partyName { get; set; }

        public int detailPartyInwon { get; set; }

        public double jumsuHap { get; set; }

        public bool isSelectTwo = false;
        public bool isSelectThree = false;
        public bool isSelectFour = false;
        public bool isShowMember = false;


        List<Z_GameResult> listGameResult = new List<Z_GameResult>();
        List<Z_PartyEntry> listPartyEnty = new List<Z_PartyEntry>();
        public string year { get; set; } = DateTime.Now.Year.ToString();

        #endregion

        protected async override Task OnInitializedAsync()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;

            // 종목
            gNameList = appState.GetGameNames("소년체육대회");

            using var context = _contextFactory.CreateDbContext();
            Z_PartyName? pName = context.Z_PartyNames
                        .Where(it => it.partyName.Contains("소년체육대회")
                                    && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람") || it.etc == "진행")
                        .FirstOrDefault();
            partyName = pName.partyName;

            if (memberName == "경북교육청" || memberName == "관리자")
            {
                if (pName.etc == "마감" || pName.etc == "열람" || pName.etc == "사용" || pName.etc == "진행")
                {
                    isShowMember = true;
                }

                listGameResult = context.Z_GameResults
                                 .Where(it => it.partyName == partyName)
                                 .OrderBy(it => it.gName).ThenBy(it => it.sName).ThenBy(it => it.dName)
                                 .ThenBy(it => it.rank.Length).ThenBy(it => it.rank)
                                 .ToList();
            }
            else if (memberPart == "경기단체")
            {
                gName = memberName;
                sNameList = appState.GetSectionNames("소년체육대회", gName);
                sName = "종별선택";
                if (pName.etc == "마감" || pName.etc == "열람" || pName.etc == "사용" || pName.etc == "진행")
                {
                    isShowMember = true;
                }
            }
        }

        // 리스트 종목명 선택시
        public void SelectGameName(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            gName = e.Value.ToString();
            if (memberPart == "경기단체" && gName == memberName)
            {
                isShowMember = true;
            }
            else if (memberName == "경북교육청" || memberName == "관리자")
            {
                isShowMember = true;
                listGameResult = listGameResult.Where(it => it.gName == gName)
                                 .OrderBy(it => it.sName)
                                 .ThenBy(it => it.rank.Length).ThenBy(it => it.rank)
                                 .ToList();
            }

            sNameList = appState.GetSectionNames("소년체육대회", gName);
            sName = "종별선택";

            StateHasChanged();
        }

        // 리스트 종별명 선택시
        public void SelectSectionName(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            sName = e.Value.ToString();
            dNameList = appState.GetDetailNames("소년체육대회", gName, sName);
            dName = "세부종목선택";

            if (memberName == "경북교육청" || memberName == "관리자")
            {
                isShowMember = true;
                using var context = _contextFactory.CreateDbContext();
                listGameResult = context.Z_GameResults
                                 .Where(it => it.partyName == partyName
                                            && it.gName == gName
                                            && it.sName == sName)
                                 .OrderBy(it => it.dName)
                                 .ThenBy(it => it.rank.Length).ThenBy(it => it.rank)
                                 .ToList();
            }
        }

        // 리스트 세부종목명 선택시
        public void SelectDetailName(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            dName = e.Value.ToString();
            if (dName.Contains("선택"))
            {
                addDisabled = true;
            }
            else
            {
                using var context = _contextFactory.CreateDbContext();
                // 리스트 불러오기
                ListGameResultFromDB();

                addDisabled = false;

                // 참가 인원 가져오기
                if (dName.Contains("mR"))
                {
                    detailPartyInwon = context.Z_PartyEntries
                                        .Where(it => it.partyName == partyName
                                                && it.gName == gName
                                                && it.sName == sName
                                                && (it.dNameOne.Contains("mR")
                                                     || it.dNameTwo.Contains("mR")
                                                     || it.dNameThree.Contains("mR")
                                                     || it.dNameFour.Contains("mR")))
                                        .Select(it => it.city).Distinct().Count();
                }
                else
                {
                    detailPartyInwon = context.Z_PartyEntries
                                        .Where(it => it.partyName == partyName
                                                && it.gName == gName
                                                && it.sName == sName
                                                && (it.dNameOne == dName
                                                     || it.dNameTwo == dName
                                                     || it.dNameThree == dName
                                                     || it.dNameFour == dName))
                                        .Select(it => it.city).Count();
                    if (gName == "양궁" || gName == "자전거" || gName == "체조" || gName == "소프트테니스" || gName == "검도")
                    {
                        detailPartyInwon = context.Z_PartyEntries
                                        .Where(it => it.partyName == partyName
                                                && it.gName == gName
                                                && it.sName == sName)
                                        .Select(it => it.city).Count();
                    }
                }
            }
        }

        // DB의 결과 불러오기
        private void ListGameResultFromDB()
        {
            using var context = _contextFactory.CreateDbContext();
            listGameResult = context.Z_GameResults
                               .Where(it => it.partyName == partyName
                                               && it.gName == gName
                                               && it.sName == sName
                                               && it.dName == dName)
                               .OrderBy(it => it.rank.Length).ThenBy(it => it.rank)
                               .ToList();
            jumsuHap = listGameResult.Sum(it => Convert.ToDouble(it.specialOne));
        }

        // 추가
        public async Task RankPlayerAdd()
        {
            onePlayer = ""; twoPlayer = ""; threePlayer = ""; fourPlayer = "";
            if (gName.Contains("선택") || sName.Contains("선택") || dName.Contains("선택"))
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "종목, 종별, 세부종목을 선택하세요");
                return;
            }

            model = new Z_GameResult();

            // 세부종목에 참가한 선수들의 리스트
            listPartyEnty = appState.GetPlayerListByGameDetail(partyName, gName, sName, dName);
            cNameList = listPartyEnty.OrderBy(it => it.city).Select(it => it.city).Distinct().ToList();
            

            // 참가자 가져오기
            using var context = _contextFactory.CreateDbContext();

            isEdit = true;

            detailInwon = 1;
            if (dName.Contains("mR") || dName.Contains("계영") || dName.Contains("계주") || dName.Contains("K-4")
                || (gName == "탁구" && dName.Contains("단체"))
                || ((gName == "사격" || gName == "양궁" || gName == "자전거") && dName.Contains("단체")))
            {
                detailInwon = 4;
                List<Z_PartyEntry> qList = context.Z_PartyEntries
                             .Where(it => it.partyName == partyName
                                        && it.city == cName
                                        && it.gName == gName
                                        && it.sName == sName)
                             .ToList();
                playerList = qList.Select(it => it.name).ToList();
            }
            else if (dName.Contains("3인"))
            {
                detailInwon = 3;
            }
            else if (dName.Contains("복식") || dName.Contains("-2"))
            {
                detailInwon = 2;
            }
            else
            {
                playerList = context.Z_PartyEntries
                        .Where(it => it.partyName == partyName
                                   && it.gName == gName
                                   && it.sName == sName
                                   && (it.dNameOne == dName
                                         || it.dNameTwo == dName
                                         || it.dNameThree == dName
                                         || it.dNameFour == dName
                                         || it.dNameFive == dName))
                        .Select(it => it.name).ToList();
                if (gName == "양궁" || gName == "자전거" || gName == "체조" || gName == "소프트테니스" || gName == "검도")
                {
                    playerList = context.Z_PartyEntries
                        .Where(it => it.partyName == partyName
                                   && it.gName == gName
                                   && it.sName == sName)
                        .Select(it => it.name).ToList();
                }
            }
        }

        // 수정
        public void RecordEdit(Z_GameResult gameresult)
        {
            model = gameresult;
            detailInwon = 1;
            using var context = _contextFactory.CreateDbContext();
            if (dName.Contains("mR") || dName.Contains("계영") || dName.Contains("계주") || dName.Contains("K-4")
                || (gName == "탁구" && dName.Contains("단체")) || ((gName == "사격" || gName == "양궁") && dName.Contains("단체")))
            {
                detailInwon = 4;
                List<Z_PartyEntry> qList = context.Z_PartyEntries
                             .Where(it => it.partyName == partyName
                                        && it.gName == gName
                                        && it.sName == sName)
                             .ToList();
                playerList = qList.Select(it => it.name).ToList();
            }
            else if (dName.Contains("3인"))
            {
                detailInwon = 3;
            }
            else if (dName.Contains("복식") || dName.Contains("-2"))
            {
                detailInwon = 2;
            }
            else
            {
                playerList = context.Z_PartyEntries
                        .Where(it => it.partyName == partyName
                                   && it.city == model.city
                                   && it.gName == gName
                                   && it.sName == sName
                                   && (it.dNameOne == dName
                                         || it.dNameTwo == dName
                                         || it.dNameThree == dName
                                         || it.dNameFour == dName
                                         || it.dNameFive == dName))
                        .Select(it => it.name).ToList();
                if (gName == "양궁" || gName == "자전거" || gName == "체조" || gName == "소프트테니스" || gName == "검도")
                {
                    playerList = context.Z_PartyEntries
                        .Where(it => it.partyName == partyName
                                   && it.city == model.city
                                   && it.gName == gName
                                   && it.sName == sName)
                        .Select(it => it.name).ToList();
                }
            }

            listPartyEnty = appState.GetPlayerListByGameDetail(partyName, gName, sName, dName);
            if (gName == "양궁" || gName == "자전거" || gName == "체조" || gName == "소프트테니스" || gName == "검도")
            {
                listPartyEnty = appState.GetPlayerListByGameDetail(partyName, gName, sName, "계주");
            }
            cNameList = listPartyEnty.OrderBy(it => it.city).Select(it => it.city).Distinct().ToList();
            tfootSelectCity = model.city;

            isEdit = true;

            string[] app = new string[4];

            if (dName.Contains("mR") || dName.Contains("계영") || dName.Contains("계주") || dName.Contains("K-4")
                || (gName == "탁구" && dName.Contains("단체")) || ((gName == "사격" || gName == "양궁") && dName.Contains("단체")))
            {
                detailInwon = 4;
                app = model.name.Split(",");
                if (app.Count() > 3)
                {
                    onePlayer = app[0];
                    twoPlayer = app[1];
                    threePlayer = app[2];
                    fourPlayer = app[3];
                }
                else if (app.Count() > 2)
                {
                    onePlayer = app[0];
                    twoPlayer = app[1];
                    threePlayer = app[2];
                }
                else if (app.Count() > 1)
                {
                    onePlayer = app[0];
                    twoPlayer = app[1];
                }
                else if (app.Count() > 0)
                {
                    onePlayer = app[0];
                }
            }
            else if (dName.Contains("3인"))
            {
                detailInwon = 3;
                app = model.name.Split(",");
                onePlayer = app[0];
                twoPlayer = app[1];
                threePlayer = app[2];

            }
            else if (dName.Contains("복식") || dName.Contains("-2"))
            {
                detailInwon = 2;
                app = model.name.Split(",");
                onePlayer = app[0];
                twoPlayer = app[1];
            }
            else
            {
                detailInwon = 1;
                onePlayer = model.name;
            }

        }

        // 저장 - 추가, 수정
        public async Task SaveModel()
        {
            using var context = _contextFactory.CreateDbContext();

            if (model.Id > 0)
            {
                // 수정
                if (string.IsNullOrEmpty(model.rank))
                {
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "순위를 입력하세요.");
                    return;
                }
                if (string.IsNullOrEmpty(model.city))
                {
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "시군을 선택하세요.");
                    return;
                }
                //if (string.IsNullOrEmpty(model.name))
                //{
                //    await JSRuntimeInjector.InvokeVoidAsync("alert", "선수를 선택하세요.");
                //    return;
                //}

                context.Z_GameResults.Update(model);
                context.SaveChanges();

                // 점수 주기
                PutJumsuByRank();

                StateHasChanged();
            }
            else
            {
                // 추가
                if (string.IsNullOrEmpty(model.rank))
                {
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "순위를 입력하세요.");
                    return;
                }
                if (string.IsNullOrEmpty(model.city))
                {
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "시군을 선택하세요.");
                    return;
                }
                //if (string.IsNullOrEmpty(model.name))
                //{
                //    await JSRuntimeInjector.InvokeVoidAsync("alert", "선수를 선택하세요.");
                //    return;
                //}
                model.gName = gName;
                model.sName = sName;
                model.dName = dName;
                model.partyName = partyName;
                model.year = year;

                // 점수 주기

                context.Z_GameResults.Add(model);
                context.SaveChanges();

                // 점수 주기
                PutJumsuByRank();
            }

            // 리스트 불러오기
            ListGameResultFromDB();

            CancelModel();
        }

        // 순위에 의한 득점 주기
        private void PutJumsuByRank()
        {
            using var context = _contextFactory.CreateDbContext();

            // 리스트 불러오기
            ListGameResultFromDB();

            foreach (var item in listGameResult)
            {
                int sameRankCount = listGameResult.Where(it => it.rank == item.rank).Count();
                int iRank = int.Parse(item.rank);
                int oneJumsu = 6;
                if (detailPartyInwon < 6) oneJumsu = detailPartyInwon;
                if (sameRankCount == 1)
                {
                    item.specialOne = (oneJumsu - iRank + 1).ToString();
                }
                else if (sameRankCount > 1)
                {
                    // 동순위자 점수 나누기
                    int nhap = 0;
                    int firstJumsu = oneJumsu - iRank + 1;
                    for (int i = firstJumsu; i > (firstJumsu - sameRankCount); i--)
                    {
                        nhap += i;
                    }
                    item.specialOne = ((double)nhap / (double)sameRankCount).ToString();
                }
                else
                {
                    item.specialOne = "";
                }

                // 신기록 가산점
                if (item.specialOne != "" && !string.IsNullOrEmpty(item.etc))
                {
                    item.specialOne = NewRecordAddJumsu(item.specialOne, item.etc);
                }
            }
            context.Z_GameResults.UpdateRange(listGameResult);
            context.SaveChanges();
        }

        // 신기록 가산점
        private string NewRecordAddJumsu(string specialOne, string? etc)
        {
            double newJumsu = double.Parse(specialOne);
            if (etc.Contains("경북신")) newJumsu *= 2.5;
            if (etc.Contains("경북타이")) newJumsu *= 2;
            if (etc.Contains("경북부별신")) newJumsu *= 1.8;
            if (etc.Contains("경북부별타이")) newJumsu *= 1.6;
            if (etc.Contains("대회신")) newJumsu *= 1.5;
            if (etc.Contains("대회타이")) newJumsu *= 1.2;

            return newJumsu.ToString();
        }

        // 삭제
        private async Task DeleteModel(Z_GameResult gameresult)
        {
            bool isDelete = await JSRuntimeInjector.InvokeAsync<bool>("confirm", gameresult.name + " 삭제 할까요?");
            if (isDelete)
            {
                using var context = _contextFactory.CreateDbContext();
                context.Z_GameResults.Remove(gameresult);
                context.SaveChanges();

                // 리스트 불러오기
                ListGameResultFromDB();
            }
        }
        // 취소
        private void CancelModel()
        {
            model = new Z_GameResult();
            tfootSelectCity = "";
            // tfootSelectPlayer = "";
            tfootSelectPlayerTwo = "";
            tfootSelectPlayerThree = "";
            tfootSelectPlayerFour = "";

            // 리스트 불러오기
            ListGameResultFromDB();

            isEdit = false;
        }

        string onePlayer = "";
        string twoPlayer = "";
        string threePlayer = "";
        string fourPlayer = "";
        private void OnePlayer_Changed(ChangeEventArgs e)
        {
            if (string.IsNullOrEmpty(model.city))
            {
                onePlayer = "";
                return;
            }
            onePlayer = e.Value.ToString();

            if (!string.IsNullOrEmpty(model.city) && model.city != "시/군")
            {
                model.name = onePlayer;

                // 세부종목 참가 학교명 가져오기
                using var context = _contextFactory.CreateDbContext();
                if (detailInwon < 4 && onePlayer != "")
                {
                    Z_PartyEntry? entry = context.Z_PartyEntries
                             .Where(it => it.partyName == partyName
                                        && it.gName == gName
                                        && it.sName == sName
                                        && (it.dNameOne == dName
                                              || it.dNameTwo == dName
                                              || it.dNameThree == dName
                                              || it.dNameFour == dName
                                              || it.dNameFive == dName)
                                        && it.name == onePlayer.Trim())
                             .FirstOrDefault();
                    if (gName == "양궁" || gName == "자전거" || gName == "체조" || gName == "소프트테니스" || gName == "검도")
                    {
                        entry = context.Z_PartyEntries
                             .Where(it => it.partyName == partyName
                                        && it.gName == gName
                                        && it.sName == sName
                                        && it.name == onePlayer)
                             .FirstOrDefault();
                    }
                    model.school = entry.schoolName;
                    model.schoolYear = entry.schoolYear;
                    model.teacher = entry.special;
                }
                else
                {
                    model.school = "선발";
                }
            }
        }

        private void TwoPlayer_Changed(ChangeEventArgs e)
        {
            twoPlayer = e.Value.ToString();

            if (!string.IsNullOrEmpty(model.city) && model.city != "시/군")
            {
                model.name += "," + twoPlayer;
            }
        }
        private void ThreePlayer_Changed(ChangeEventArgs e)
        {
            threePlayer = e.Value.ToString();

            if (!string.IsNullOrEmpty(model.city) && model.city != "시/군")
            {
                model.name += "," + threePlayer;
            }
        }
        private void FourPlayer_Changed(ChangeEventArgs e)
        {
            fourPlayer = e.Value.ToString();

            if (!string.IsNullOrEmpty(model.city) && model.city != "시/군")
            {
                model.name += "," + fourPlayer;
            }
        }

        // tfoot 시군 Bind
        private string? _tfootSelectCity = null;
        private string? tfootSelectCity
        {
            get
            {
                return _tfootSelectCity;
            }
            set
            {
                _tfootSelectCity = value;
                model.city = value;

                // 세부종목 참가 선수명 가져오기
                using var context = _contextFactory.CreateDbContext();

                if (detailInwon != 4)
                {
                    List<Z_PartyEntry> qList = context.Z_PartyEntries
                             .Where(it => it.partyName == partyName
                                        && it.city == value
                                        && it.gName == gName
                                        && it.sName == sName
                                        && (it.dNameOne == dName
                                              || it.dNameTwo == dName
                                              || it.dNameThree == dName
                                              || it.dNameFour == dName
                                              || it.dNameFive == dName))
                             .ToList();
                    if (gName == "양궁" || gName == "자전거" || gName == "체조" || gName == "소프트테니스" || gName == "검도")
                    {
                        qList = context.Z_PartyEntries
                             .Where(it => it.partyName == partyName
                                        && it.city == value
                                        && it.gName == gName
                                        && it.sName == sName)
                             .ToList();
                    }
                    playerList = qList.Select(it => it.name).ToList();

                    //if (qList.Count == 1)
                    //{
                    //    Z_PartyEntry? pStudent = qList.FirstOrDefault();
                    //    onePlayer = pStudent.name;
                    //    model.name = onePlayer;
                    //    model.school = pStudent.schoolName;
                    //    model.schoolYear = pStudent.schoolYear;
                    //    model.teacher = pStudent.special;
                    //}
                }
                else
                {
                    // 계주
                    List<Z_PartyEntry> qList = context.Z_PartyEntries
                             .Where(it => it.partyName == partyName
                                        && it.city == value
                                        && it.gName == gName
                                        && it.sName == sName)
                             .ToList();
                    playerList = qList.Select(it => it.name).ToList();
                }

            }
        }

        // tfoot 선수 Bind
        private string? _tfootSelectPlayer = null;
        private string? tfootSelectPlayer
        {
            get
            {
                return _tfootSelectPlayer;
            }
            set
            {
                if (!string.IsNullOrEmpty(model.city) && model.city != "시/군")
                {
                    _tfootSelectPlayer = value;
                    model.name = value;
                    // 세부종목 참가 학교명 가져오기
                    using var context = _contextFactory.CreateDbContext();
                    if (detailInwon < 4 && value != "")
                    {
                        Z_PartyEntry? entry = context.Z_PartyEntries
                                 .Where(it => it.partyName == partyName
                                            && it.gName == gName
                                            && it.sName == sName
                                            && (it.dNameOne == dName
                                                  || it.dNameTwo == dName
                                                  || it.dNameThree == dName
                                                  || it.dNameFour == dName
                                                  || it.dNameFive == dName)
                                            && it.name == value)
                                 .FirstOrDefault();
                        if (gName == "양궁" || gName == "자전거" || gName == "체조" || gName == "소프트테니스" || gName == "검도")
                        {
                            entry = context.Z_PartyEntries
                                 .Where(it => it.partyName == partyName
                                            && it.gName == gName
                                            && it.sName == sName
                                            && it.name == value)
                                 .FirstOrDefault();
                        }
                        model.school = entry.schoolName;
                        model.schoolYear = entry.schoolYear;
                        model.teacher = entry.special;
                    }
                    else
                    {
                        model.school = "선발";
                    }
                }
            }
        }
        private string? _tfootSelectPlayerTwo = null;
        private string? tfootSelectPlayerTwo
        {
            get
            {
                return _tfootSelectPlayerTwo;
            }
            set
            {
                _tfootSelectPlayerTwo = value;
                model.name += "," + value;
            }
        }
        private string? _tfootSelectPlayerThree = null;
        private string? tfootSelectPlayerThree
        {
            get
            {
                return _tfootSelectPlayerThree;
            }
            set
            {
                _tfootSelectPlayerThree = value;
                model.name += "," + value;
            }
        }
        private string? _tfootSelectPlayerFour = null;
        private string? tfootSelectPlayerFour
        {
            get
            {
                return _tfootSelectPlayerFour;
            }
            set
            {
                _tfootSelectPlayerFour = value;
                model.name += "," + value;
            }
        }

        private string? SchoolYearDisplay(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            str = str.Replace(" ", "");
            str = str.Replace("학년", "");
            return str;
        }

        // 참가신청 다운로드
        public void DwonLoadGameResult()
        {
            string excelFileName = "";
            string title = "";

            using var context = _contextFactory.CreateDbContext();

            List<Z_GameResult> excelPartyEntry = new List<Z_GameResult>();
            List<Z_GameResult> gameResultList = new List<Z_GameResult>();

            gameResultList = context.Z_GameResults
                                .Where(it => it.partyName == partyName
                                        && (it.rank == "1" || it.rank == "2" || it.rank == "3"))
                                .ToList();
            string? birth = "";
            int n = 0;
            foreach (var item in gameResultList)
            {
                try
                {
                    n++;
                    birth = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName
                                    && it.gName == item.gName
                                    && it.sName == item.sName
                                    && (it.dNameOne == item.dName || it.dNameTwo == item.dName
                                         || it.dNameThree == item.dName || it.dNameFour == item.dName)
                                    && it.name == item.name)
                        .Select(it => it.jumin)
                        .FirstOrDefault() == null ? "" :
                        context.Z_PartyEntries
                            .Where(it => it.partyName == partyName
                                    && it.gName == item.gName
                                    && it.sName == item.sName
                                    && (it.dNameOne == item.dName || it.dNameTwo == item.dName
                                         || it.dNameThree == item.dName || it.dNameFour == item.dName)
                                    && it.name == item.name)
                        .Select(it => it.jumin)
                        .FirstOrDefault();
                    item.coach = birth;
                }
                catch (Exception e)
                {
                    continue;
                }

            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("참가신청리스트");
                worksheet.Column(1).Width = 6;
                worksheet.Column(2).Width = 10;
                worksheet.Column(3).Width = 10;
                worksheet.Column(4).Width = 10;
                worksheet.Column(5).Width = 10;
                worksheet.Column(6).Width = 10;
                worksheet.Column(7).Width = 10;
                worksheet.Column(8).Width = 20;
                worksheet.Column(9).Width = 10;
                worksheet.Column(10).Width = 10;
                worksheet.Column(11).Width = 10;
                //worksheet.Column(12).Width = 10;
                //worksheet.Column(13).Width = 6;

                // 제목 시작
                worksheet.Cells[1, 1, 1, 13].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Value = title;
                // 제목 끝

                worksheet.Cells[3, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 9].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 10].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 11].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                //worksheet.Cells[3, 12].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                //worksheet.Cells[3, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                //worksheet.Cells[3, 13].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                //worksheet.Cells[3, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                var tableBody = worksheet.Cells["A3:A3"].LoadFromCollection(
                    (from m in gameResultList
                     select new
                     {
                         시군 = m.city,
                         종목 = m.gName,
                         종별 = m.sName,
                         이름 = m.name,
                         세부종목 = m.dName,
                         기록 = m.record,
                         순위 = m.rank,
                         학교명 = m.school,
                         학년 = m.schoolYear,
                         생년월일 = m.coach,
                         비고 = m.etc
                     })
                     , true);

                //int cellsNumber = excelPartyEntry.Count() + 5;
                //worksheet.Cells[cellsNumber, 1, cellsNumber, 13].Merge = true;
                //worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                //worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //worksheet.Cells[cellsNumber, 1].Value = "위와 같이 경북소년체육대회에 참가신청 합니다.";
                //cellsNumber++;
                //worksheet.Cells[cellsNumber, 1, cellsNumber, 13].Merge = true;
                //worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                //worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //worksheet.Cells[cellsNumber, 1].Value = DateTime.Now.Year + "년 " + DateTime.Now.Month + "월 " + DateTime.Now.Day + "일";
                //cellsNumber++;
                //worksheet.Cells[cellsNumber, 1].Value = "";
                //cellsNumber++;
                //worksheet.Cells[cellsNumber, 1, cellsNumber, 10].Merge = true;
                //worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                //worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //if (memberName.Contains("지원청"))
                //{
                //    worksheet.Cells[cellsNumber, 1].Value = memberName + " 교육장";
                //}
                //else if (memberName.Contains("학교"))
                //{
                //    worksheet.Cells[cellsNumber, 1].Value = memberName + " 장";
                //}

                FileUtil.SaveAs(JSRuntimeInjector, "소년체전경기결과.xlsx", package.GetAsByteArray());
            }
        }
    }
}
