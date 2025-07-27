using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using DocumentFormat.OpenXml.InkML;

namespace GBES.Pages.YearTrack
{
    public partial class YearTrackPartyEntry
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

        bool isMember = false;

        bool isDisabledCity = true;
        bool isDisabledSchool = true;

        bool isEdit = false;

        bool isGameUsing = true;

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

        public bool isMemberSchool = true;           // 학교명 disabled

        List<string> sNameOrder = new List<string> { "남자초등부", "여자초등부", "남자중학부", "여자중학부" };


        List<Z_PartyEntry> listPartyEntry = new List<Z_PartyEntry>();
        //List<LSGameOffice> listOffices = new List<LSGameOffice>();
        public string year { get; set; } = DateTime.Now.Year.ToString();

        #endregion

        #region 함수
        // 초기 설정
        protected override async Task OnInitializedAsync()
        {
            // 인정 가져오기 [2]
            //var authState = await authenticationStateTask;
            //var user = authState.User;
            //memberName = user.Identity.Name;


            var authState = await authenticationStateTask;
            var user = authState.User;
            memberName = user.Identity.Name;
            memberName = appState.userName;

            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;
            manager = appState.userManager;

            using var context = _contextFactory.CreateDbContext();

            if (memberName != "")
            {
                // 종목
                gNameList = appState.GetGameNames("학년별 육상경기대회");

                Z_PartyName? pName = context.Z_PartyNames
                            .Where(it => it.partyName.Contains("학년별 육상경기대회")
                                        && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람"))
                            .FirstOrDefault();

                partyName = pName.partyName;
                if (pName.etc == "사용" || memberName == "경북교육청" || memberName == "관리자")
                {
                    showOrHideClass = "showCSS";
                }
                isMember = true;

                // 교육지원청 
                cNameList = appState.GetCities();
                if (memberPart == "경북교육청" || memberPart == "관리자")
                {
                    cName = "시/군";
                    isDisabledCity = false;
                    isDisabledSchool = false;

                    listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName)
                                    .OrderBy(it => it.city).ThenBy(it => it.sName)
                                    .ToList();
                }
                else if (memberPart == "교육지원청")
                {
                    cName = memberCity;
                    isDisabledCity = true;
                    isDisabledSchool = false;

                    listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName
                                            && it.city == cName)
                                    .OrderBy(it => it.gName).ThenBy(it => it.sName)
                                    .ToList();
                }
                else if (memberPart == "경기단체")
                {
                    isDisabledCity = false;

                    listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName
                                            && it.gName == memberName)
                                    .OrderBy(it => it.sName).ThenBy(it => it.city)
                                    .ToList();
                }
                else if (memberPart == "학교")
                {
                    cName = memberCity;
                    isDisabledCity = true;
                    isDisabledSchool = true;

                    listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName
                                            && it.schoolName == memberName)
                                    .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                                    .ToList();
                }

                if (pName.etc == "열람")
                {
                    isDisabledCity = false;
                }

                gName = "육상";
                //SelectGameName(new ChangeEventArgs { Value = "값" });
                sNameList = new List<string> { "남자초등부", "여자초등부", "남자중학부", "여자중학부" };
                sName = "종별선택";
            }
            string? gameEnd = context.Z_Events
                             .Where(it => it.year == year && it.specialTwo.Contains("육상경기대회") && it.gName == gName)
                             .Select(it => it.specialThree)
                             .FirstOrDefault();
            if (gameEnd == "마감")
            {
                isGameUsing = false;
            }
            else if (gameEnd == "사용")
            {
                isGameUsing = true;
            }

        }

        // 리스트  시군 선택시
        public void SelectCity(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            cName = e.Value.ToString();
            sName = "종별선택";
            using var context = _contextFactory.CreateDbContext();
            if (cName != "시/군")
            {
                listPartyEntry = context.Z_PartyEntries
                                 .Where(it => it.partyName == partyName
                                            && it.city == cName)
                                 .OrderBy(it => it.gName).ThenBy(it => it.sName)
                                 .ToList();
            }
            else
            {
                listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName)
                                    .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                                    .ToList();
            }
            StateHasChanged();
        }

        // 리스트 종목명 선택시
        public void SelectGameName(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            gName = "육상";
            sName = "종별선택";
            dNameList = new List<string>();
            using var context = _contextFactory.CreateDbContext();
            string? gameEnd = context.Z_Events
                             .Where(it => it.year == year && it.specialTwo.Contains("육상경기대회") && it.gName == gName)
                             .Select(it => it.specialThree)
                             .FirstOrDefault();
            if (gameEnd == "마감")
            {
                isGameUsing = false;
            }
            else if (gameEnd == "사용")
            {
                isGameUsing = true;
            }

            if (memberName.Contains("학교"))
            {
                listPartyEntry = context.Z_PartyEntries
                               .Where(it => it.partyName == partyName
                                              && it.schoolName == memberName
                                              && it.gName == gName)
                               .OrderBy(it => it.gName).ThenBy(it => it.sName)
                               .ToList();
            }
            else if (memberName.Contains("교육지원청"))
            {
                listPartyEntry = context.Z_PartyEntries
                               .Where(it => it.partyName == partyName
                                              && it.city == cName
                                              && it.gName == gName)
                               .OrderBy(it => it.gName).ThenBy(it => it.sName)
                               .ToList();
            }
            else
            {
                if (string.IsNullOrEmpty(cName))
                {
                    listPartyEntry = context.Z_PartyEntries
                               .Where(it => it.partyName == partyName
                                              && it.gName == gName)
                               .OrderBy(it => it.gName).ThenBy(it => it.sName)
                               .ToList();
                }
                else
                {
                    listPartyEntry = context.Z_PartyEntries
                                   .Where(it => it.partyName == partyName
                                                  && it.city == cName
                                                  && it.gName == gName)
                                   .OrderBy(it => it.gName).ThenBy(it => it.sName)
                                   .ToList();
                }
            }
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
            using var context = _contextFactory.CreateDbContext();
            if (memberName.Contains("학교"))
            {
                listPartyEntry = context.Z_PartyEntries
                               .Where(it => it.partyName == partyName
                                              && it.schoolName == memberName
                                              && it.gName == gName
                                        && it.sName == sName)
                               .OrderBy(it => it.gName).ThenBy(it => it.sName)
                               .ToList();
            }
            else if (memberName.Contains("교육지원청"))
            {
                if (sName == "종별선택")
                {
                    listPartyEntry = context.Z_PartyEntries
                               .Where(it => it.partyName == partyName
                                              && it.city == cName
                                              && it.gName == gName)
                               .OrderBy(it => it.sName)
                               .ToList();
                }
                else
                {
                    listPartyEntry = context.Z_PartyEntries
                                   .Where(it => it.partyName == partyName
                                                  && it.city == cName
                                                  && it.gName == gName
                                            && it.sName == sName)
                                   .AsEnumerable() // 메모리로 가져온 후
                                   .OrderBy(it => ExtractGrade(it.dNameOne))
                                   .ToList();
                }
            }
            else if (memberPart == "경북교육청" || memberPart == "관리자")
            {
                if (cName == "시/군")
                {
                    if (sName == "종별선택")
                    {
                        listPartyEntry = context.Z_PartyEntries
                               .Where(it => it.partyName == partyName
                                              && it.gName == gName
                                        && it.sName == sName)
                               .AsEnumerable() // 메모리로 가져온 후
                               .OrderBy(it => ExtractGrade(it.dNameOne))
                               .ToList();
                    }
                    else
                    {
                        listPartyEntry = context.Z_PartyEntries
                               .Where(it => it.partyName == partyName
                                              && it.gName == gName
                                        && it.sName == sName)
                               .AsEnumerable() // 메모리로 가져온 후
                               .OrderBy(it => ExtractGrade(it.dNameOne))
                               .ToList();

                    }
                }
                else
                {
                    if (sName == "종별선택")
                    {
                        listPartyEntry = context.Z_PartyEntries
                               .Where(it => it.partyName == partyName
                                            && it.city == cName
                                            && it.gName == gName)
                               .AsEnumerable() // 메모리로 가져온 후
                               .OrderBy(it => ExtractGrade(it.dNameOne))
                               .ToList();
                    }
                    else
                    {
                        listPartyEntry = context.Z_PartyEntries
                               .Where(it => it.partyName == partyName
                                              && it.city == cName
                                              && it.gName == gName
                                        && it.sName == sName)
                               .AsEnumerable() // 메모리로 가져온 후
                               .OrderBy(it => ExtractGrade(it.dNameOne))
                               .ToList();

                    }
                }


            }
        }

        // 선수 추가 모달 열기
        public async Task ModalOpenNewPlayer()
        {
            if (sName == "종별선택")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "종별을 먼저 선택 하세요.");
                return;
            }
            modalSelectSection = sName;
            if (memberPart == "경기단체")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "경기단체에서는 추가를 할 수 없습니다.");
                return;
            }

            if (!isGameUsing)
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", model.gName + " 참가신청이 마감되었습니다.");
                return;
            }

            if (memberPart == "학교")
            {
                isMemberSchool = true;
            }
            else
            {
                isMemberSchool = false;
            }
            modalSelectSchool = "학교선택";

            schoolNameList = appState.GetSchoolNamesPrimaryMiddle(cName);
            model = new Z_PartyEntry();
            model.city = cName;
            modalSelectCity = cName;
            if (memberPart == "학교")
            {
                modalSelectSchool = 
                model.schoolName = memberName;
                modalSelectSchool = memberName;
                model.special = manager;

                isDisabledCity = true;
                isDisabledSchool = true;

            }
            else if (memberPart == "교육지원청")
            {
                model.city = cName;
                modalSelectCity = cName;
                isDisabledCity = true;
                isDisabledSchool = false;
            }
            else if (memberPart == "경북교육청" || memberPart == "관리자")
            {
                isDisabledCity = false;
                isDisabledSchool = false;
            }
            modalSelectGame = gName;
            modalSelectSection = sName;

            if (sName != "종별선택")
            {
                dNameList = appState.GetDetailNames(partyName, model.gName, model.sName);
                dNameList = dNameList.OrderBy(s => ExtractGrade(s)).ToList();
            }
            SetModalDetailSelect(modalSelectSection);

            isEdit = false;
            await JSRuntimeInjector.InvokeVoidAsync("openModal", "modal-custom");
        }

        // 세부종목 정렬
        int ExtractGrade(string s)
        {
            // "100m 3학년"에서 3만 추출
            var match = Regex.Match(s, @"(\d+)학년");
            if (match.Success)
                return int.Parse(match.Groups[1].Value);
            return -1; // 못 찾으면
        }

        // 수정 선택시 모달 열기
        public async Task ModalEdit(Z_PartyEntry editModel)
        {
            if (!isGameUsing)
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", model.gName + " 참가신청이 마감되었습니다.");
                return;
            }

            model = editModel;

            //sNameList = appState.GetSectionNames(partyName, model.gName);
            dNameList = appState.GetDetailNames(partyName, model.gName, model.sName);

            modalSelectCity = model.city;
            schoolNameList = appState.GetSchoolNamesPrimaryMiddle(model.city);
            modalSelectSchool = model.schoolName;
            modalSelectGame = model.gName;
            modalSelectSection = model.sName;
            selectDetailOne = model.dNameOne;
            selectDetailTwo = model.dNameTwo == "선택" ? "" : model.dNameTwo;
            selectDetailThree = model.dNameThree == "선택" ? "" : model.dNameThree;
            selectDetailFour = model.dNameFour == "선택" ? "" : model.dNameFour;

            isEdit = true;

            if (memberPart == "학교")
            {
                isDisabledCity = true;
                isDisabledSchool = true;
                isMemberSchool = true;
            }
            else if (memberPart == "교육지원청")
            {
                isDisabledCity = true;
                isDisabledSchool = false;
                isMemberSchool = false;
            }
            else if (memberPart == "경북교육청" || memberPart == "관리자")
            {
                isDisabledCity = false;
                isDisabledSchool = false;
                isMemberSchool = false;
            }

            SetModalDetailSelect(model.sName);

            StateHasChanged();

            await JSRuntimeInjector.InvokeVoidAsync("openModal", "modal-custom");
        }

        // 모달 model 저장하기
        public async Task SaveModel()
        {
            if (!isGameUsing)
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", model.gName + " 참가신청이 마감되었습니다.");
                return;
            }
            string noInputMSG = "";
            if (model.city.Length < 2) noInputMSG = "시군 미선택 ";
            if (model.schoolName.Length < 2) noInputMSG += "학교 미선택 ";
            if (string.IsNullOrEmpty(model.sName)) noInputMSG += "종별 미선택 ";
            if (string.IsNullOrEmpty(model.name)) noInputMSG += "이름 미입력 ";
            if (string.IsNullOrEmpty(model.jumin)) noInputMSG += "생년월일 미입력 ";
            if (string.IsNullOrEmpty(model.schoolYear)) noInputMSG += "학년 미입력 ";
            if (string.IsNullOrEmpty(model.special)) noInputMSG += "지도교사 미선택 ";
            // 1인 2종목 체크
            noInputMSG += TwoDetailOver();

            if (noInputMSG != "")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", noInputMSG);
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            if (model.Id > 0)
            {
                // 수정
                model.dNameOne = selectDetailOne;
                model.dNameTwo = selectDetailTwo;
                model.dNameThree = selectDetailThree;
                model.dNameFour = selectDetailFour;

                context.Z_PartyEntries.Update(model);
                await context.SaveChangesAsync();

                // 리스트에 수정 후 새로고침
                foreach (var item in listPartyEntry.Where(w => w.Id == model.Id))
                {
                    item.city = model.city;
                    item.schoolName = model.schoolName;
                    item.gName = model.gName;
                    item.sName = model.sName;
                    item.name = model.name;
                    item.jumin = model.jumin;
                    item.schoolYear = model.schoolYear;
                    item.schoolName = model.schoolName;
                    item.dNameOne = selectDetailOne;   // model.dNameOne;
                    item.dNameTwo = selectDetailTwo;   // model.dNameTwo;
                    item.dNameThree = selectDetailThree;    // model.dNameThree;
                    item.dNameFour = selectDetailFour;    // model.dNameFour;
                    item.photo = model.photo;               // 체육 중·고 출신 시군
                    item.special = model.special;
                    item.etc = model.etc;
                }
                StateHasChanged();
            }
            else
            {
                // 추가
                model.city = modalSelectCity;
                model.schoolName = modalSelectSchool;
                model.partyName = partyName;
                model.dNameOne = selectDetailOne;
                model.dNameTwo = selectDetailTwo;
                model.dNameThree = selectDetailThree;
                model.dNameFour = selectDetailFour;
                model.year = year;

                context.Z_PartyEntries.Add(model);
                context.SaveChanges();

                // 리스트에  추가 후 새로고침
                listPartyEntry.Add(model);
                StateHasChanged();
            }
            await CancelModel();
        }

        // 1인 2종목
        private string TwoDetailOver()
        {
            string msg = "";
            var dNames = new[] { model.dNameOne, model.dNameTwo, model.dNameThree, model.dNameFour };

            if (string.IsNullOrEmpty(dNames[0]))
            {
                msg = "세부종목을 선택하세요.";
            }
            else
            {
                // model.schoolYear에서 학년 추출
                //int schoolGrade = ExtractGrade(model.schoolYear);

                // 릴레이(mR) 제외, 학년이 다른 세부종목 체크
                //foreach (var dName in dNames)
                //{
                //    if (!string.IsNullOrEmpty(dName) && !dName.Contains("mR"))
                //    {
                //        int dNameGrade = ExtractGrade(dName);
                //        if (dNameGrade != -1 && dNameGrade != schoolGrade)
                //        {
                //            msg = $"세부종목({dName})의 학년이 참가 학년과 다릅니다.\n";
                //            return msg;
                //        }
                //    }
                //}

                var nonEmptyDNames = dNames.Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (nonEmptyDNames.Count != nonEmptyDNames.Distinct().Count())
                {
                    msg = "같은 세부종목이 중복 선택되었습니다.\n";
                    return msg;
                }

                // 세부종목2부터 선택 체크
                if (string.IsNullOrEmpty(dNames[1]) && (!string.IsNullOrEmpty(dNames[2]) || !string.IsNullOrEmpty(dNames[3])))
                {
                    msg = "세부종목2 부터 선택하세요.";
                }
                // 세부종목3부터 선택 체크
                else if (string.IsNullOrEmpty(dNames[2]) && !string.IsNullOrEmpty(dNames[3]))
                {
                    msg = "세부종목3 부터 선택하세요.";
                }

                if (!string.IsNullOrEmpty(dNames[2]))
                {
                    if(!dNames[0].Contains("mR") && !dNames[1].Contains("mR") && !dNames[2].Contains("mR"))
                    {
                        msg = "릴레이를 제외한 1인 2종목 출전입니다.\n수정 하세요.";
                    }
                }

                if (!string.IsNullOrEmpty(dNames[3]))
                {
                    int n = dNames[0].Contains("mR") ? 1 : 0;
                    n += dNames[1].Contains("mR") ? 1 : 0;
                    n += dNames[2].Contains("mR") ? 1 : 0;
                    n += dNames[3].Contains("mR") ? 1 : 0;

                    if (n<2)
                    {
                        msg = "릴레이를 제외한 1인 2종목 출전입니다.\n수정 하세요.";
                    }
                }



                //else
                //{
                //    // 릴레이를 제외한 1인 2종목 출전 체크
                //    bool isRelay1 = dNames[0]?.Contains("mR") ?? false;
                //    bool isRelay2 = dNames[1]?.Contains("mR") ?? false;
                //    bool isRelay3 = dNames[2]?.Contains("mR") ?? false;
                //    bool isRelay4 = dNames[3]?.Contains("mR") ?? false;

                //    // 세부종목3가 있을 때 릴레이 개수 체크
                //    if (!string.IsNullOrEmpty(dNames[2]))
                //    {
                //        int relayCount = dNames.Count(x => !string.IsNullOrEmpty(x) && x.Contains("mR"));
                //        if (relayCount > 2)
                //        {
                //            msg = "릴레이를 제외한 1인 2종목 출전입니다.\n수정 하세요.";
                //        }
                //    }
                //    // 릴레이가 하나도 없을 때
                //    else if (!isRelay1 && !isRelay2 && !isRelay3)
                //    {
                //        msg = "릴레이를 제외한 1인 2종목 출전입니다.\n수정 하세요.";
                //    }
                //}
            }

            return msg;

            #region 노가다 
            //if(string.IsNullOrEmpty(model.dNameOne))
            //{
            //    noInputMSG += "세부종목을 선택하세요.";
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(model.dNameTwo))
            //    {
            //        if(!string.IsNullOrEmpty(model.dNameThree) || !string.IsNullOrEmpty(model.dNameFour))
            //        {
            //            noInputMSG += "세부종목2 부터 선택하세요.";
            //        }
            //    }
            //    else
            //    {
            //        if (string.IsNullOrEmpty(model.dNameThree))
            //        {
            //            if (!string.IsNullOrEmpty(model.dNameFour))
            //            {
            //                noInputMSG += "세부종목3 부터 선택하세요.";
            //            }
            //        }
            //        else
            //        {
            //            if(!model.dNameOne.Contains("mR") && !model.dNameOne.Contains("mR") && !model.dNameOne.Contains("mR"))
            //            {
            //                noInputMSG += "릴레이를 제외한 1인 2종목 출전입니다.\n 수정 하세요.";
            //            }
            //            if(!string.IsNullOrEmpty(model.dNameFour)) 
            //            {
            //                int n = 0;
            //                if (model.dNameOne.Contains("mR")) n++;
            //                if (model.dNameTwo.Contains("mR")) n++;
            //                if (model.dNameThree.Contains("mR")) n++;
            //                if (model.dNameFour.Contains("mR")) n++;

            //                if (n < 2)
            //                {
            //                    noInputMSG += "릴레이를 제외한 1인 2종목 출전입니다.\n 수정 하세요.";
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion
        }

        public async Task DeleteModel()
        {
            if (!isGameUsing)
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", model.gName + " 참가신청이 마감되었습니다.");
                return;
            }

            bool confirmed = await JSRuntimeInjector.InvokeAsync<bool>("confirm", model.name + " 삭제 할까요?");
            if (confirmed)
            {
                // DB에서 삭제
                using var context = _contextFactory.CreateDbContext();
                context.Z_PartyEntries.Remove(model);
                await context.SaveChangesAsync();

                // 리스트에 삭제 후 새로고침
                bool deleteList = listPartyEntry.Remove(model);
                if (deleteList)
                {
                    StateHasChanged();
                }

                await JSRuntimeInjector.InvokeVoidAsync("closeModal", "");
            }
        }
        public async Task CancelModel()
        {
            model = new Z_PartyEntry();
            //selectGame = "";
            //selectSection = "";
            selectDetailOne = "";
            selectDetailTwo = "";
            selectDetailThree = "";
            selectDetailFour = "";
            await JSRuntimeInjector.InvokeVoidAsync("closeModal", "modal-custom");
        }

        // 리스트에 세부종목 표시하기  (세부1, 세부2, ......)
        public string DetailNames(Z_PartyEntry editModel)
        {
            string dnames = "";
            if (!string.IsNullOrEmpty(editModel.dNameOne))
            {
                dnames = editModel.dNameOne;
                if (!string.IsNullOrEmpty(editModel.dNameTwo) && editModel.dNameTwo != "선택")
                {
                    dnames += ", " + editModel.dNameTwo;
                    if (!string.IsNullOrEmpty(editModel.dNameThree) && editModel.dNameThree != "선택")
                    {
                        dnames += ", " + editModel.dNameThree;
                        if (!string.IsNullOrEmpty(editModel.dNameFour) && editModel.dNameFour != "선택")
                        {
                            dnames += ", " + editModel.dNameFour;
                            if (!string.IsNullOrEmpty(editModel.dNameFive) && editModel.dNameFive != "선택")
                            {
                                dnames += ", " + editModel.dNameFive;
                            }
                        }
                    }
                }
            }

            return dnames;
        }

        // 모달 시군 Bind
        private string? _modalSelectCity = null;
        private string? modalSelectCity
        {
            get
            {
                return _modalSelectCity;
            }
            set
            {
                _modalSelectCity = value;
                model.city = value;
                // 학교명 가져오기
                schoolNameList = appState.GetSchoolNamesPrimaryMiddle(model.city);

                //if (memberPart == "교육지원청" || memberPart == "경북교육청" || memberPart == "관리자")
                //{
                //    schoolNameList.Add("경북체육중학교");
                //}
            }
        }

        // 모달 시군 소속 학교명 Bind
        private string? _modalSelectSchool = null;
        private string? modalSelectSchool
        {
            get
            {
                return _modalSelectSchool;
            }
            set
            {
                _modalSelectSchool = value;
                model.schoolName = value;
            }
        }

        // 모달 종목 Bind
        private string? _modalSelectGame = null;
        private string? modalSelectGame
        {
            get
            {
                return _modalSelectGame;
            }
            set
            {
                _modalSelectGame = value;
                model.gName = value;
                //sNameList = appState.GetSectionNames(partyName, model.gName);

                using var context = _contextFactory.CreateDbContext();
                string? gameEnd = context.Z_Events
                             .Where(it => it.year == year && it.specialTwo.Contains("학년별 육상경기대회") && it.gName == model.gName)
                             .Select(it => it.specialThree)
                             .FirstOrDefault();
                if (gameEnd == "마감")
                {
                    isGameUsing = false;
                    JSRuntimeInjector.InvokeVoidAsync("alert", model.gName + " 참가신청이 마감되었습니다.");
                    modalSelectGame = "종목선택";
                }
                else if (gameEnd == "사용")
                {
                    isGameUsing = true;
                }
            }
        }
        // 모달 종별 Bind
        private string? _modalSelectSection = null;
        private string? modalSelectSection
        {
            get
            {
                return _modalSelectSection;
            }
            set
            {
                _modalSelectSection = value;
                model.sName = value;


                dNameList = appState.GetDetailNames(partyName, model.gName, model.sName);

                //모달 종별 선택시 세부종목 Select 보이고 감추기
                SetModalDetailSelect(model.sName);
            }
        }

        // 모달 세부종목 One  Bind
        private string? _selectDetailOne = null;
        private string? selectDetailOne
        {
            get
            {
                return _selectDetailOne;
            }
            set
            {
                _selectDetailOne = value;

                model.dNameOne = value;
            }
        }

        // 모달 세부종목 Two  Bind
        private string? _selectDetailTwo = null;
        private string? selectDetailTwo
        {
            get
            {
                return _selectDetailTwo;
            }
            set
            {
                _selectDetailTwo = value;

                model.dNameTwo = value;
            }
        }
        // 모달 세부종목 Three  Bind
        private string? _selectDetailThree = null;
        private string? selectDetailThree
        {
            get
            {
                return _selectDetailThree;
            }
            set
            {
                _selectDetailThree = value;

                model.dNameThree = value;
            }
        }
        // 모달 세부종목 Four  Bind
        private string? _selectDetailFour = null;
        private string? selectDetailFour
        {
            get
            {
                return _selectDetailFour;
            }
            set
            {
                _selectDetailFour = value;

                model.dNameFour = value;
            }
        }
        #endregion

        // 모달 종별 선택시 세부종목 Select 보이고 감추기
        private void SetModalDetailSelect(string sectionname)
        {
            if (sectionname.Contains("중학"))
            {
                isSelectFour = true;
            }
        }

        // 생년월일 체크
        string errorMessage = "";
        void ValidateJumin(ChangeEventArgs e)
        {
            var value = e.Value?.ToString() ?? "";
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{6}$"))
            {
                errorMessage = "주민번호 앞자리는 6자리 숫자여야 합니다.";
            }
            else
            {
                errorMessage = "";
            }
        }

        // dNameList에서 선택된 학년(model.schoolYear)에 해당하는 항목만 표시
        IEnumerable<string> GetFilteredDNameList()
        {
            if (string.IsNullOrEmpty(model.schoolYear))
                return Enumerable.Empty<string>();

            // 예: "100m 1학년"에서 "1학년"이 포함된 것만
            string gradeText = $"{model.schoolYear}학년";
            List<string> strings = dNameList.Where(x => x.Contains(gradeText) || x.Contains("mR")).ToList();
            return dNameList.Where(x => x.Contains(gradeText) || x.Contains("mR"));
        }

        // 참가신청 다운로드
        public void DownLoadPartyEntry()
        {
            //if (gName == null || gName == "종목선택")
            //{
            //    JSRuntimeInjector.InvokeVoidAsync("alert", "종목별 출력입니다.\n종목을 먼저 출력하세요.");
            //    return;
            //}
            string excelFileName = "";
            string title = "";

            using var context = _contextFactory.CreateDbContext();

            List<Z_PartyEntry> excelPartyEntry = new List<Z_PartyEntry>();

            if (memberPart == "교육지원청")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName
                                       && it.city == cName)
                            .OrderBy(it => it.gName).ThenBy(it => it.sName)
                            .ToList();
                excelFileName = cName;
                if (gName != "종목선택")
                {
                    title = gName;
                    excelPartyEntry = excelPartyEntry.Where(it => it.gName == gName).ToList();
                    excelFileName += "_" + gName;

                    if (sName != "종별선택")
                    {
                        title += " " + sName + " ";
                        excelPartyEntry = excelPartyEntry.Where(it => it.sName == sName).ToList();
                        excelFileName += "_" + sName;
                    }
                }

                title = title + "참가신청서";
            }
            else if (memberPart == "경기단체")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName
                                       && it.gName == memberName)
                            .OrderBy(it => it.sName).ThenBy(it => it.city)
                            .ToList();
                excelFileName = memberName;

                if (sName != "종별선택" && sName != null)
                {
                    title += " " + sName + " ";
                    excelPartyEntry = excelPartyEntry.Where(it => it.sName == sName)
                                        .OrderBy(it => it.dNameOne).ToList();
                    // excelFileName += "_" + sName;
                }

                title = memberName + "참가신청 현황";
            }
            else if (memberPart == "경북교육청" || memberPart == "관리자")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName)
                            .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                            .ToList();
                excelFileName = "학년별 육상경기대회";
                title = year + " 2025 경상북도교육감배 초·중학년별 육상경기대회 참가신처현황";
            }
            else if (memberPart == "학교")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName
                                            && it.schoolName == memberName)
                            .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                            .ToList();
                excelFileName = memberName;

                if (gName != "종목선택")
                {
                    title = gName;
                    excelPartyEntry = excelPartyEntry.Where(it => it.gName == gName).ToList();

                    if (sName != "종별선택")
                    {
                        title += " " + sName + " ";
                        excelPartyEntry = excelPartyEntry.Where(it => it.sName == sName).ToList();
                    }
                }

                title = title + "참가신청서";
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("참가신청리스트");
                worksheet.Column(1).Width = 6;
                worksheet.Column(2).Width = 10;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 10;
                worksheet.Column(5).Width = 10;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 6;
                worksheet.Column(8).Width = 17;
                worksheet.Column(9).Width = 17;
                worksheet.Column(10).Width = 17;
                worksheet.Column(11).Width = 17;
                worksheet.Column(12).Width = 10;
                worksheet.Column(13).Width = 6;

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
                worksheet.Cells[3, 12].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[3, 13].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                var tableBody = worksheet.Cells["A3:A3"].LoadFromCollection(
                    (from m in excelPartyEntry
                     select new
                     {
                         시군 = m.city,
                         종목 = m.gName,
                         종별 = m.sName,
                         이름 = m.name,
                         생년월일 = m.jumin,
                         학교명 = m.schoolName,
                         학년 = m.schoolYear,
                         세부종목1 = m.dNameOne,
                         세부종목2 = m.dNameTwo,
                         세부종목3 = m.dNameThree,
                         지도교사 = m.special,
                         비고 = m.etc
                     })
                     , true);

                int cellsNumber = excelPartyEntry.Count() + 5;
                worksheet.Cells[cellsNumber, 1, cellsNumber, 13].Merge = true;
                worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[cellsNumber, 1].Value = "위와 같이 " + year + " 경상북도교육감배 초·중학년별 육상경기대회에 참가신청 합니다.";
                cellsNumber++;
                worksheet.Cells[cellsNumber, 1, cellsNumber, 13].Merge = true;
                worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[cellsNumber, 1].Value = DateTime.Now.Year + "년 " + DateTime.Now.Month + "월 " + DateTime.Now.Day + "일";
                cellsNumber++;
                worksheet.Cells[cellsNumber, 1].Value = "";
                cellsNumber++;
                worksheet.Cells[cellsNumber, 1, cellsNumber, 10].Merge = true;
                worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (memberName.Contains("지원청"))
                {
                    worksheet.Cells[cellsNumber, 1].Value = memberName + " 교육장";
                }
                else if (memberName.Contains("학교"))
                {
                    worksheet.Cells[cellsNumber, 1].Value = memberName + " 장";
                }

                FileUtil.SaveAs(JSRuntimeInjector, excelFileName + " 참가현황.xlsx", package.GetAsByteArray());
            }
        }

    }
}

