using ClosedXML.Report.Utils;
using DocumentFormat.OpenXml.InkML;
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
    public partial class StudentGameOfficials
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
        bool isEdit = false;

        List<string?> cNameList = new List<string>();
        List<string?> schoolNameList = new List<string>();
        List<string?> gNameList = new List<string>();

        public Z_GameOfficer? model = new Z_GameOfficer();

        public string? cName { get; set; }
        public string? gName { get; set; }
        public string? sName { get; set; }
        public string? partyName { get; set; }

        List<Z_GameOfficer> listGameOfficer = new List<Z_GameOfficer>();
        public string year { get; set; } = DateTime.Now.Year.ToString();

        List<string> listSchoolPosition = new List<string>() { 
            "교사", "전임지도자", "행정직", "교감", "교장", "기타"
        };
        List<string> listGamePosition = new List<string>() {
            "교육청","위원장","부위원장","경기임원장","경기부임원장","총무부장","총무부차장","총무부임원","경기부장","경기부차장","경기부임원",
            "심판부장","심판부차장","심판부임원","시설부장","시설부차장","시설부임원","심판배정위원회위원장","심판배정위원회위원",
            "질서대책부장","질서대책차장","질서대책임원","기록부장","기록부차장","기록부임원","영상판독부장","영상판독관","홍보부장","홍보부차장"
        };

        #endregion

        #region 함수
        // 초기 설정
        protected override async Task OnInitializedAsync()
        {
            // 인정 가져오기 [2]
            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;
            manager = appState.userManager;

            if (memberName != "")
            {
                // 종목
                gNameList = appState.GetGameNames("학생체육대회");
                gNameList.Insert(0,"교육청");

                using var context = _contextFactory.CreateDbContext();
                Z_PartyName? pName = context.Z_PartyNames
                            .Where(it => it.partyName.Contains("학생체육대회")
                                        && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람"))
                            .FirstOrDefault();
                partyName = pName.partyName;
                if (pName.etc == "사용" || memberName == "경북교육청" || memberPart == "교육지원청" || memberPart == "경기단체" || memberName == "관리자")
                {
                    isMember = true;
                }
                else
                {
                    isMember = false;
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "교육지원청, 경기단체용 입니다. 로그인 후 사용 가능합니다.");
                    NavigationManagerInjector.NavigateTo("/");
                }

                listGameOfficer = context.Z_GameOfficers
                                    .Where(it => it.partyName == partyName)
                                    .OrderBy(it => it.gName)
                                    .ToList();
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

            using var context = _contextFactory.CreateDbContext();
            if (gName == "종목선택")
            {
                listGameOfficer = context.Z_GameOfficers
                                    .Where(it => it.partyName == partyName)
                                    .OrderBy(it => it.gName)
                                    .ToList();
            }
            else
            {
                listGameOfficer = context.Z_GameOfficers
                                    .Where(it => it.partyName == partyName
                                                && it.gName == gName)
                                    .ToList();
            }
        }

        // 경기임원 추가 모달 열기
        public async Task ModalOpenNewPlayer()
        {
            model = new Z_GameOfficer();

            modalSelectCity = "시군선택";
            modalSelectSchool = "학교선택";
            model.schoolPosition = "직위선택";
            model.gamePosition = "경기직책선택";

            if (memberPart == "경기단체")
            {
                modalSelectGame = memberName;
            }

            if (gName != null)
            {
                modalSelectGame = gName;
            }
            
            cNameList = appState.GetCities();

            await JSRuntimeInjector.InvokeVoidAsync("openModal", "modal-customOfficer");
        }

        // 수정 선택시 모달 열기
        public async Task ModalEdit(Z_GameOfficer editModel)
        {
            model = editModel;

            cNameList = appState.GetCities();
            modalSelectCity = appState.GetCityBySchoolName(model.schoolName);
            schoolNameList = appState.GetSchoolNames(modalSelectCity);
            schoolNameList.Add("경북체육중학교");
            schoolNameList.Add("경북체육고등학교");
            modalSelectGame = model.gName.Trim();
            modalSelectSchool = model.schoolName;
            modalSelectSchoolPosition = model.schoolPosition;
            modalSelectGamePosition = model.gamePosition;

            isEdit = true;

            await JSRuntimeInjector.InvokeVoidAsync("openModal", "modal-customOfficer");
        }

        // 모달 model 저장하기
        public async Task SaveModel()
        {
            string noInputMSG = "";
            if (model.schoolName.Length < 2) noInputMSG += "학교 미선택 ";
            if (string.IsNullOrEmpty(model.gName)) noInputMSG += "종목 미선택 ";
            if (string.IsNullOrEmpty(model.name)) noInputMSG += "이름 미입력 ";

            if (noInputMSG != "")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", noInputMSG);
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            if (model.Id > 0)
            {
                // 수정
                context.Z_GameOfficers.Update(model);
                context.SaveChanges();

                // 리스트에 수정 후 새로고침
                foreach (var item in listGameOfficer.Where(w => w.Id == model.Id))
                {
                    item.gName = model.gName.Trim();
                    item.name = model.name.Trim();
                    item.schoolName = model.schoolName.Trim();
                    item.schoolPosition=model.schoolPosition.Trim();
                    item.gamePosition = model.gamePosition.Trim();
                    item.etc = model.etc;
                }
                StateHasChanged();
            }
            else
            {
                // 추가
                model.gName = modalSelectGame.Trim();
                model.schoolName = modalSelectSchool.Trim();
                model.schoolPosition = modalSelectSchoolPosition.Trim();
                model.gamePosition= modalSelectGamePosition.Trim();
                model.partyName = partyName;

                context.Z_GameOfficers.Add(model);
                context.SaveChanges();

                // 리스트에  추가 후 새로고침
                listGameOfficer.Add(model);
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
                context.Z_GameOfficers.Remove(model);
                await context.SaveChangesAsync();

                // 리스트에 삭제 후 새로고침
                bool deleteList = listGameOfficer.Remove(model);
                if (deleteList)
                {
                    StateHasChanged();
                }

                CancelModel();
            }
        }
        public async Task CancelModel()
        {
            model = new Z_GameOfficer();
            
            await JSRuntimeInjector.InvokeVoidAsync("closeModal", "modal-customOfficer");
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
                // 학교명 가져오기
                schoolNameList = appState.GetSchoolNames(value);
                schoolNameList.Add("경북체육중학교");
                schoolNameList.Add("경북체육고등학교");
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
            }
        }

        // 모달 학교직위 Bind
        private string? _modalSelectSchoolPosition = null;
        private string? modalSelectSchoolPosition
        {
            get
            {
                return _modalSelectSchoolPosition;
            }
            set
            {
                _modalSelectSchoolPosition = value;
                model.schoolPosition = value;
            }
        }

        // 모달 종목직책 Bind
        private string? _modalSelectGamePosition = null;
        private string? modalSelectGamePosition
        {
            get
            {
                return _modalSelectGamePosition;
            }
            set
            {
                _modalSelectGamePosition = value;
                model.gamePosition = value;
            }
        }

        #endregion

        // 참가신청 다운로드
        public void DownLoadGameOfficers()
        {
            string excelFileName = "";

            using var context = _contextFactory.CreateDbContext();

            List<Z_GameOfficer> excelPartyEntry = new List<Z_GameOfficer>();

            if (memberPart == "경기단체")
            {
                excelPartyEntry = context.Z_GameOfficers
                            .Where(it => it.partyName == partyName
                                       && it.gName == memberName)
                            .ToList();
                excelFileName = memberName;
            }
            else if (memberPart == "경북교육청" || memberPart == "관리자")
            {
                excelPartyEntry = context.Z_GameOfficers
                            .Where(it => it.partyName == partyName)
                            .ToList();
                excelFileName = "학생체전";
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("시간할애대상자");
                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 25;
                worksheet.Column(7).Width = 10;
                worksheet.Column(8).Width = 20;
                worksheet.Column(9).Width = 20;
                worksheet.Column(10).Width = 20;
                worksheet.Column(11).Width = 20;
                worksheet.Column(12).Width = 15;
                worksheet.Column(13).Width = 20;
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
                worksheet.Cells[1, 9].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 10].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 11].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 12].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                worksheet.Cells[1, 13].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                var tableBody = worksheet.Cells["A1:A1"].LoadFromCollection(
                    (from m in excelPartyEntry
                     select new
                     {
                         종목 = m.gName,
                         이름 = m.name,
                         학교명 = m.schoolName,
                         직위 = m.schoolPosition,
                         종목직책=m.gamePosition,
                         비고 = m.etc
                     })
                     , true);

                FileUtil.SaveAs(JSRuntimeInjector, excelFileName + " 시간할애대상자.xlsx", package.GetAsByteArray());
            }
        }
    }
}


