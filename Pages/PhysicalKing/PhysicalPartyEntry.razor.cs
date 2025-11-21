using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Security.Claims;

namespace GBES.Pages.PhysicalKing
{
    public partial class PhysicalPartyEntry
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
        List<string>? sNameList = new List<string>() { "", "초등부", "중학부"};
        List<string>? schoolNameList = new List<string>();

        public Z_PartyEntry model = new Z_PartyEntry();

        public string? cName { get; set; }
        public string? sName { get; set; }
        public string? partyName { get; set; }

        public string? showOrHideClass { get; set; } = "hideCss";


        List<Z_PartyEntry> listPartyEntry = new List<Z_PartyEntry>();
        //List<LSGameOffice> listOffices = new List<LSGameOffice>();
        public string year { get; set; } = DateTime.Now.Year.ToString();

        #endregion

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

            if (memberName != "")
            {
                using var context = _contextFactory.CreateDbContext();
                Z_PartyName? pName = context.Z_PartyNames
                         .Where(it => it.partyName.Contains("경상북도 체력인증제")
                                     && it.etc == "체력사용")
                         .FirstOrDefault();
                partyName = pName.partyName;
                if (pName.etc == "사용" || memberName == "경북교육청" || memberName == "관리자" || memberName.Contains("입력"))
                {
                    showOrHideClass = "showCSS";
                }
                isMember = true;

                // 교육지원청 
                cNameList = appState.GetCities();
                if (memberPart == "경북교육청" || memberName == "관리자" || memberName.Contains("입력"))
                {
                    isDisabledCity = false;
                    isDisabledSchool = false;

                    listPartyEntry = context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName)
                                    .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
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
                //else if (memberPart == "학교")
                //{
                //    cName = memberCity;
                //    isDisabledCity = true;
                //    isDisabledSchool = true;

                //    listPartyEntry = context.Z_PartyEntries
                //                    .Where(it => it.partyName == partyName
                //                            && it.schoolName == memberName)
                //                    .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                //                    .ToList();
                //}

                if (pName.etc == "열람")
                {
                    isDisabledCity = false;
                }
            }
        }

        // 리스트  시군 선택시
        public async Task SelectCity(ChangeEventArgs e)
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
                listPartyEntry = await context.Z_PartyEntries
                                 .Where(it => it.partyName == partyName
                                            && it.city == cName)
                                 .OrderBy(it => it.gName).ThenBy(it => it.sName)
                                 .ToListAsync();
            }
            else
            {
                listPartyEntry = await context.Z_PartyEntries
                                    .Where(it => it.partyName == partyName)
                                    .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                                    .ToListAsync();
            }
            StateHasChanged();
        }

        // 리스트 종별명 선택시
        public async Task SelectSectionName(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            sName = e.Value.ToString();
            using var context = _contextFactory.CreateDbContext();
            listPartyEntry = await context.Z_PartyEntries
                             .Where(it => it.partyName == partyName
                                        && it.city == cName
                                        && it.sName == sName)
                             .OrderBy(it => it.gName).ThenBy(it => it.sName)
                             .ToListAsync();
        }

        // 선수 추가 모달 열기
        public async Task ModalOpenNewPlayer()
        {
            schoolNameList = appState.GetSchoolNames(cName);
            model = new Z_PartyEntry();
            model.city = cName;
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
                //schoolNameList.Add("경북체육중학교");
                //schoolNameList.Add("경북체육고등학교");
                model.city = cName;
                modalSelectCity = cName;
                model.schoolName = modalSelectSchool;
                isDisabledCity = true;
                isDisabledSchool = false;
            }
            else if (memberPart == "경북교육청" || memberName == "관리자" || memberName.Contains("입력"))
            {
                //schoolNameList.Add("경북체육중학교");
                //schoolNameList.Add("경북체육고등학교");
                isDisabledCity = false;
                isDisabledSchool = false;
            }

            isEdit = false;
            await JSRuntimeInjector.InvokeVoidAsync("openModal", "modal-custom");
        }

        // 수정 선택시 모달 열기
        public async Task ModalEdit(Z_PartyEntry editModel)
        {
            model = editModel;

            modalSelectCity = cName;
            schoolNameList = appState.GetSchoolNames(cName);
            modalSelectSchool = model.schoolName;

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
            else if (memberPart == "경북교육청" || memberName == "관리자" || memberName.Contains("입력"))
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
            if (string.IsNullOrEmpty(model.city)) noInputMSG = "시군 미선택 ";
            if (string.IsNullOrEmpty(model.schoolName)) noInputMSG += "학교 미선택 ";
            if (string.IsNullOrEmpty(model.schoolYear)) noInputMSG += "학년 미선택 ";
            if (string.IsNullOrEmpty(model.code)) noInputMSG += "남여 미입력 ";
            if (string.IsNullOrEmpty(model.name)) noInputMSG += "이름 미입력 ";
            if (string.IsNullOrEmpty(model.special)) noInputMSG += "지도교사 미선택 ";

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
                foreach (var item in listPartyEntry.Where(w => w.Id == model.Id))
                {
                    item.city = model.city;
                    item.schoolName = model.schoolName;
                    item.name = model.name;
                    item.code = model.code;
                    item.schoolYear = model.schoolYear;
                    item.schoolName = model.schoolName;
                    if (model.schoolName!.Contains("초등") && model.code == "남") item.sName = "남자초등부";
                    else if (model.schoolName.Contains("초등") && model.code == "여") item.sName = "여자초등부";
                    else if (model.schoolName.Contains("중학") && model.code == "남") item.sName = "남자중학부";
                    else if (model.schoolName.Contains("중학") && model.code == "여") item.sName = "여자중학부";
                    else if (model.schoolName.Contains("고등") && model.code == "남") item.sName = "남자고등부";
                    else if (model.schoolName.Contains("고등") && model.code == "여") item.sName = "여자고등부";
                    item.sName = model.sName;
                    item.special = model.special;    // 지도교사
                    item.etc = model.etc;
                }
                StateHasChanged();
                await JSRuntimeInjector.InvokeVoidAsync("alert", "수정하였습니다.");
            }
            else
            {
                // 추가
                model.city = modalSelectCity;
                model.schoolName = modalSelectSchool;
                model.partyName = partyName;
                model.year = year;

                if (modalSelectSchool!.Contains("초등") && model.code == "남") model.sName = "남자초등부";
                else if (modalSelectSchool.Contains("초등") && model.code == "여") model.sName = "여자초등부";
                else if (modalSelectSchool.Contains("중학") && model.code == "남") model.sName = "남자중학부";
                else if (modalSelectSchool.Contains("중학") && model.code == "여") model.sName = "여자중학부";
                else if (modalSelectSchool.Contains("고등") && model.code == "남") model.sName = "남자고등부";
                else if (modalSelectSchool.Contains("고등") && model.code == "여") model.sName = "여자고등부";

                // 현장 추가 시  Z_PhysicalKingRecords 에 넣기
                if(memberName!.Contains("입력"))
                {
                    Z_PhysicalKingRecord kingRecord = new Z_PhysicalKingRecord();
                    kingRecord = new Z_PhysicalKingRecord();
                    kingRecord.city = model.city;
                    kingRecord.code = model.code;
                    kingRecord.name = model.name;
                    kingRecord.schoolName = model.schoolName;
                    kingRecord.schoolYear = model.schoolYear;
                    kingRecord.passWord = "1234";
                    kingRecord.sName = model.sName;
                    kingRecord.partyName = model.partyName;
                    kingRecord.year = model.year;
                    kingRecord.numbering = model.etc;
                    context.Z_PhysicalKingRecords.Add(kingRecord);
                }

                context.Z_PartyEntries.Add(model);
                context.SaveChanges();

                // 리스트에  추가 후 새로고침
                listPartyEntry.Add(model);
                StateHasChanged();
                await JSRuntimeInjector.InvokeVoidAsync("alert", "추가하였습니다.");
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

                await JSRuntimeInjector.InvokeVoidAsync("closeModal", "modal-custom");
                await JSRuntimeInjector.InvokeVoidAsync("alert", "삭제하였습니다.");
            }
        }
        public async Task CancelModel()
        {
            model = new Z_PartyEntry();
            //selectGame = "";
            //selectSection = "";
           
            await JSRuntimeInjector.InvokeVoidAsync("closeModal", "modal-custom");
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
                schoolNameList = appState.GetSchoolNames(value);
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

        // 참가신청 다운로드
        public async Task DownLoadPartyEntry()
        {
            string excelFileName = "";

            using var context = _contextFactory.CreateDbContext();

            List<Z_PartyEntry> excelPartyEntry = new List<Z_PartyEntry>();

            if (memberPart == "교육지원청")
            {
                excelPartyEntry = await context.Z_PartyEntries
                            .Where(it => it.partyName == partyName
                                       && it.city == cName)
                            .OrderBy(it => it.gName).ThenBy(it => it.sName)
                            .ToListAsync();
                excelFileName = cName;
            }
            else if (memberPart == "경북교육청" || memberName == "관리자" || memberName.Contains("입력"))
            {
                excelPartyEntry = await context.Z_PartyEntries
                            .Where(it => it.partyName == partyName)
                            .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                            .ToListAsync();
                excelFileName = "체력인증제";
            }
            else if (memberPart == "학교")
            {
                excelPartyEntry = await context.Z_PartyEntries
                            .Where(it => it.partyName == partyName
                                            && it.schoolName == memberName)
                            .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                            .ToListAsync();
                excelFileName = memberName;
            }

            // 소트하기
            List<Z_PartyEntry> imsiList = new List<Z_PartyEntry>();
            imsiList.AddRange(
                excelPartyEntry.Where(it=>it.schoolName.Contains("초등") && it.code=="남").OrderBy(it=>it.schoolName)
                );
            imsiList.AddRange(
                excelPartyEntry.Where(it => it.schoolName.Contains("초등") && it.code == "여").OrderBy(it => it.schoolName)
                );
            imsiList.AddRange(
                excelPartyEntry.Where(it => it.schoolName.Contains("중학") && it.code == "남").OrderBy(it => it.schoolName)
                );
            imsiList.AddRange(
                excelPartyEntry.Where(it => it.schoolName.Contains("중학") && it.code == "여").OrderBy(it => it.schoolName)
                );
            imsiList.AddRange(
                excelPartyEntry.Where(it => it.schoolName.Contains("고등") && it.code == "남").OrderBy(it => it.schoolName)
                );
            imsiList.AddRange(
                excelPartyEntry.Where(it => it.schoolName.Contains("고등") && it.code == "여").OrderBy(it => it.schoolName)
                );

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("참가신청리스트");
                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 25;
                worksheet.Column(7).Width = 10;
                worksheet.Column(8).Width = 20;
                worksheet.Column(9).Width = 20;

                // 제목 시작
                worksheet.Cells[1, 1, 1, 13].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Value = DateTime.Now.Year.ToString() + "체력인증제 참가신청서";
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

                var tableBody = worksheet.Cells["A3:A3"].LoadFromCollection(
                    (from m in imsiList
                     select new
                     {
                         시군 = m.city,
                         종목 = m.gName,
                         이름 = m.name,
                         성별 = m.code,
                         학교명 = m.schoolName,
                         학년 = m.schoolYear,
                         지도교사 = m.special,
                         비고 = m.etc
                     })
                     , true);

                int cellsNumber = excelPartyEntry.Count() + 5;
                worksheet.Cells[cellsNumber, 1, cellsNumber, 9].Merge = true;
                worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[cellsNumber, 1].Value = "위와 같이 경상북도 체력인증제 참가신청 합니다.";
                cellsNumber++;
                worksheet.Cells[cellsNumber, 1, cellsNumber, 9].Merge = true;
                worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[cellsNumber, 1].Value = DateTime.Now.Year + "년 " + DateTime.Now.Month + "월 " + DateTime.Now.Day + "일";
                cellsNumber++;
                worksheet.Cells[cellsNumber, 1].Value = "";
                cellsNumber++;
                worksheet.Cells[cellsNumber, 1, cellsNumber, 7].Merge = true;
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

        private string SchoolClass(string schoolname)
        {
            if (schoolname.Contains("초등")) return "초";
            if (schoolname.Contains("중학")) return "중";
            if (schoolname.Contains("고등")) return "고";
            return "";
        }
    }
}
