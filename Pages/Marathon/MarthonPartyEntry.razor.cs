using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OfficeOpenXml;
using System.Security.Claims;
using GBES.Pages.StudentFestival;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Bibliography;

namespace GBES.Pages.Marathon
{
    public partial class MarthonPartyEntry
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

        List<string>? cNameList = new List<string>();
        List<string>? schoolNameList = new List<string>();
        List<string>? sNameList = new List<string>() { "초등부", "중학부" };
        List<string>? dNameList = new List<string>();

        public Z_PartyEntry model = new Z_PartyEntry();

        public string? cName { get; set; }
        public string? gName { get; set; } = "구간마라톤";
        public string? sName { get; set; }
        public string? partyName { get; set; }

        public string? showOrHideClass { get; set; } = "hideCss";
        public bool isSelectTwo = false;
        public bool isSelectThree = false;
        public bool isSelectFour = false;


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
            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;
            manager = appState.userManager;

            string year=DateTime.Now.Year.ToString();

            if (memberName != "")
            {
                using var context = _contextFactory.CreateDbContext();
                Z_PartyName? pName = context.Z_PartyNames
                            .Where(it => it.partyName.Contains("구간마라톤")
                                        && it.year==year
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
                    isDisabledCity = false;
                    isDisabledSchool = false;

                    listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName)
                                    .OrderBy(it => it.city).ThenBy(it => it.gName).ThenByDescending(it => it.sName).ThenBy(it => it.dNameOne)
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
                                    .OrderBy(it => it.gName).ThenByDescending(it => it.sName).ThenBy(it => it.dNameOne)
                                    .ToList();
                    schoolNameList = appState.GetSchoolNamesBySection(cName, sName);
                }
                else if (memberPart == "경기단체")
                {
                    isDisabledCity = false;

                    listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName
                                            && it.gName == memberName)
                                    .OrderByDescending(it => it.sName).ThenBy(it => it.city).ThenBy(it => it.dNameOne)
                                    .ToList();
                    isEdit = false;
                }
                else if (memberPart == "학교")
                {
                    cName = memberCity;
                    isDisabledCity = true;
                    isDisabledSchool = true;

                    listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName
                                            && it.schoolName == memberName)
                                    .OrderBy(it => it.city).ThenBy(it => it.gName).ThenByDescending(it => it.sName).ThenBy(it => it.dNameOne)
                                    .ToList();
                    isEdit = false;
                }
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
                                 .OrderBy(it => it.gName).ThenByDescending(it => it.sName).ThenBy(it => it.dNameOne)
                                 .ToList();
                schoolNameList = appState.GetSchoolNamesBySection(cName, sName);
            }
            else
            {
                listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName)
                                    .OrderBy(it => it.city).ThenBy(it => it.gName).ThenByDescending(it => it.sName).ThenBy(it => it.dNameOne)
                                    .ToList();
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
            listPartyEntry = context.Z_PartyEntries
                             .Where(it => it.partyName == partyName
                                        && it.city == cName
                                        && it.gName == gName
                                        && it.sName == sName)
                             .OrderBy(it => it.dNameOne)
                             .ToList();
        }

        // 선수 추가 모달 열기
        public async Task ModalOpenNewPlayer()
        {
            if (string.IsNullOrEmpty(sName))
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "종별 선택 후 추가하세요.");
                return;
            }
            schoolNameList = appState.GetSchoolNamesBySection(cName, sName);

            model = new Z_PartyEntry();
            model.city = cName;
            model.gName = "구간마라톤";
            model.sName = sName;
            dNameList = appState.GetDetailsByMarathon(model.sName);

            modalSelectCity = cName;
            if (memberPart == "학교")
            {
                model.schoolName = memberName;
                modalSelectSchool = memberName;
                model.special = manager;

                isDisabledCity = true;
                isDisabledSchool = true;

            }
            else if (memberPart == "교육지원청")
            {
                schoolNameList.Add("경북체육중학교");
                schoolNameList.Add("경북체육고등학교");
                model.city = cName;
                modalSelectCity = cName;
                isDisabledCity = true;
                isDisabledSchool = false;
            }
            else if (memberPart == "경북교육청" || memberPart == "관리자")
            {
                schoolNameList.Add("경북체육중학교");
                schoolNameList.Add("경북체육고등학교");
                isDisabledCity = false;
                isDisabledSchool = false;
            }
            //modalSelectGame = gName;
            //sNameList = appState.GetSectionNames("학생체육대회", gName);
            //modalSelectSection = sName;
            //dNameList = appState.GetDetailNames("학생체육대회", gName, sName);

            ddlDname = "구간선택";
            modalSelectSchool = "학교선택";

            isEdit = false;
            await JSRuntimeInjector.InvokeVoidAsync("openModal", "modal-custom");
        }

        // 수정 선택시 모달 열기
        public async Task ModalEdit(Z_PartyEntry editModel)
        {
            model = editModel;

            dNameList = appState.GetDetailsByMarathon(model.sName);

            modalSelectCity = cName;
            schoolNameList = appState.GetSchoolNamesBySection(cName, sName);
            modalSelectSchool = model.schoolName;
            modalSelectGame = model.gName;
            modalSelectSection = model.sName;
            selectDetailOne = model.dNameOne;
            selectDetailTwo = model.dNameTwo;
            selectDetailThree = model.dNameThree;
            selectDetailFour = model.dNameFour;

            ddlDname = model.dNameOne + " " + model.dNameFive;

            isEdit = true;

            if (memberPart == "학교")
            {
                isDisabledCity = true;
                isDisabledSchool = true;
            }
            else if (memberPart == "교육지원청")
            {
                isDisabledCity = true;
                isDisabledSchool = false;
            }
            else if (memberPart == "경북교육청" || memberPart == "관리자")
            {
                isDisabledCity = false;
                isDisabledSchool = false;
            }

            StateHasChanged();

            await JSRuntimeInjector.InvokeVoidAsync("openModal", "modal-custom");
        }

        // 모달 model 저장하기
        public async Task SaveModel()
        {
            string noInputMSG = "";
            if (model.city.Length < 2) noInputMSG = "시군 미선택 ";
            if (model.schoolName.Length < 2) noInputMSG += "학교 미선택 ";
           // if (string.IsNullOrEmpty(model.gName)) noInputMSG += "종목 미선택 ";
            if (string.IsNullOrEmpty(model.sName)) noInputMSG += "종별 미선택 ";
            if (string.IsNullOrEmpty(model.name)) noInputMSG += "이름 미입력 ";
            if (string.IsNullOrEmpty(model.part)) noInputMSG += "남여 미입력 ";
            if (string.IsNullOrEmpty(model.dNameOne)) noInputMSG += "구간 미입력 ";

            if (noInputMSG != "")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", noInputMSG);
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            if (model.Id > 0)
            {
                // 수정
                context.Z_PartyEntries.Update(model);
                context.SaveChanges();

                // 리스트에 수정 후 새로고침
                foreach (var item in listPartyEntry.OrderBy(it=>it.dNameOne))
                {
                    item.city = model.city;
                    item.schoolName = model.schoolName;
                    item.gName = model.gName;
                    item.sName = model.sName;
                    item.name = model.name;
                    item.part = model.part;
                    item.schoolYear = model.schoolYear;
                    item.schoolName = model.schoolName;
                    item.dNameOne = model.dNameOne;
                    item.dNameFive = model.dNameFive;
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
                model.year = year;

                context.Z_PartyEntries.Add(model);
                context.SaveChanges();

                // 리스트에  추가 후 새로고침
                listPartyEntry.Add(model);
                StateHasChanged();
            }
            CancelModel();
        }
        public async Task DeleteModel()
        {
            bool confirmed = await JSRuntimeInjector.InvokeAsync<bool>("confirm", model.name + "삭제 할까요?");
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
                if (!string.IsNullOrEmpty(editModel.dNameTwo))
                {
                    dnames += ", " + editModel.dNameTwo;
                    if (!string.IsNullOrEmpty(editModel.dNameThree))
                    {
                        dnames += ", " + editModel.dNameThree;
                        if (!string.IsNullOrEmpty(editModel.dNameFour))
                        {
                            dnames += ", " + editModel.dNameFour;
                            if (!string.IsNullOrEmpty(editModel.dNameFive))
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
                schoolNameList = appState.GetSchoolNamesBySection(cName, sName);

                if (memberPart == "교육지원청" || memberPart == "경북교육청" || memberPart == "관리자")
                {
                    schoolNameList.Add("경북체육중학교");
                    schoolNameList.Add("경북체육고등학교");
                }
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
                sNameList = appState.GetSectionNames("학생체육대회", value);
                sName = "종별선택";
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

                dNameList = appState.GetDetailNames("학생체육대회", modalSelectGame, value);

                //모달 종별 선택시 세부종목 Select 보이고 감추기
                SetModalDetailSelect();
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
        private void SetModalDetailSelect()
        {
            isSelectTwo = false;
            isSelectThree = false;
            isSelectFour = false;
            if (dNameList.Count == 1)
            {
                model.dNameOne = dNameList[0];

            }
            else if (dNameList.Count > 1)
            {
                switch (modalSelectGame)
                {
                    case "육상":
                        isSelectTwo = true;
                        isSelectThree = true;
                        break;
                    case "수영":
                    case "양궁":
                        isSelectTwo = true;
                        isSelectThree = true;
                        isSelectFour = true;
                        break;
                    case "레슬링":
                    case "펜싱":
                    case "카누":
                        isSelectTwo = true;
                        break;
                    case "유도":
                        if (sName.Contains("고등"))
                        {
                            isSelectTwo = true;
                        }
                        break;
                    case "롤러":
                        isSelectTwo = true;
                        isSelectThree = true;
                        break;
                    case "에어로빅":
                        break;

                }
            }

        }

        // 참가신청 다운로드
        public void DownLoadPartyEntry()
        {
            string excelFileName = "구간마라톤";

            using var context = _contextFactory.CreateDbContext();

            List<Z_PartyEntry> excelPartyEntry = new List<Z_PartyEntry>();

            if (memberPart == "교육지원청")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName
                                       && it.city == cName)
                            .OrderByDescending(it => it.sName).ThenBy(it => it.dNameOne)
                            .ToList();
            }
            else if (memberName == "육상")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName
                                       && it.gName == "구간마라톤")
                            .OrderByDescending(it => it.sName).ThenBy(it => it.city).ThenBy(it => it.dNameOne)
                            .ToList();
            }
            else if (memberPart == "경북교육청" || memberPart == "관리자")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName)
                            .OrderByDescending(it => it.sName).ThenBy(it => it.city).ThenBy(it => it.dNameOne)
                            .ToList();
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("참가신청리스트");
                worksheet.Column(1).Width = 20;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 20;
                worksheet.Column(8).Width = 20;
                worksheet.Cells[1, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                var tableBody = worksheet.Cells["A1:A1"].LoadFromCollection(
                    (from m in excelPartyEntry
                     select new
                     {
                         시군 = m.city,
                         종별 = m.sName,
                         구간 = m.dNameOne,
                         학교명 = m.schoolName,
                         성명 = m.name,
                         학년 = m.schoolYear,
                         성별 = m.part,
                         비고 = m.dNameFive
                     })
                     , true);

                FileUtil.SaveAs(JSRuntimeInjector, excelFileName + " 참가현황.xlsx", package.GetAsByteArray());
            }
        }

        private void Edit_Clicked()
        {
            isEdit = true;
        }
        private void Save_Clicked(Z_PartyEntry model)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Z_PartyEntries.Update(model);
            context.SaveChanges();

            isEdit = false;
        }

        private async Task Delete_Clicked(Z_PartyEntry model)
        {
            //model.name = string.Empty;
            //model.schoolName = string.Empty;
            //model.schoolYear = string.Empty;

            //using var context = _contextFactory.CreateDbContext();
            //context.Z_PartyEntries.Update(model);
            //context.SaveChanges();

            bool confirmed = await JSRuntimeInjector.InvokeAsync<bool>("confirm", model.name + "삭제 할까요?");
            if (confirmed)
            {
                // DB에서 삭제
                using var context = _contextFactory.CreateDbContext();
                context.Z_PartyEntries.Remove(model);
                await context.SaveChangesAsync();

                // 리스트에 삭제 후 새로고침
                isEdit = false;
                bool deleteList = listPartyEntry.Remove(model);
                if (deleteList)
                {
                    StateHasChanged();
                }
            }

            
        }

        private void Cancel_Clicked()
        {
            isEdit = false;
        }

        // 드롭다운 구간명
        private string ddlDname;
        public string DdlDname
        {
            get => ddlDname;
            set
            {
                ddlDname = value;
                string[] arr = ddlDname.Split(' ');
                model.dNameOne = arr[0];
                model.dNameFive = arr[1];
                model.part = arr[1].Substring(0, 1);
            }
        }
    }
}
