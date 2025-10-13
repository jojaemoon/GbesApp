using DocumentFormat.OpenXml.Bibliography;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace GBES.Pages.StudentFestival
{
    public partial class StudentCityInwon
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

        string partyName = "";
        string year = DateTime.Now.Year.ToString();

        protected override void OnInitialized()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;
            manager = appState.userManager;

            using var context = _contextFactory.CreateDbContext();

            Z_PartyName? pName = context.Z_PartyNames
                            .Where(it => it.partyName.Contains("학생체육대회")
                                        && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람"))
                            .FirstOrDefault();
            partyName = pName.partyName;

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
                inwon.primaryMan = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자초등부" || it.sName == "초등부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryWoman = listPartyEntry.Where(it => it.city == cname && it.sName == "여자초등부" && !string.IsNullOrEmpty(it.name)).Count();
                inwon.primaryHap = inwon.primaryMan + inwon.primaryWoman;
                inwon.middleMan = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자중학부" || it.sName == "중학부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleWoman = listPartyEntry.Where(it => it.city == cname && it.sName == "여자중학부" && !string.IsNullOrEmpty(it.name)).Count();
                inwon.middleHap = inwon.middleMan + inwon.middleWoman;
                inwon.highMan = listPartyEntry.Where(it => it.city == cname && (it.sName == "남자고등부" || it.sName == "고등부") && !string.IsNullOrEmpty(it.name)).Count();
                inwon.highWoman = listPartyEntry.Where(it => it.city == cname && it.sName == "여자고등부" && !string.IsNullOrEmpty(it.name)).Count();
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
        private void PrintPage()
        {
            JSRuntimeInjector.InvokeVoidAsync("window.print");
        }
        private void DownLoadExcel()
        {
            string excelFileName = "";
            string title = "";

            using var context = _contextFactory.CreateDbContext();

            List<Z_PartyEntry> excelPartyEntry = new List<Z_PartyEntry>();

            if (memberPart == "교육지원청")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName && it.year == year
                                       && it.city == memberCity)
                            .OrderBy(it => it.gName).ThenBy(it => it.sName)
                            .ToList();
                excelFileName = partyName + " " + memberCity;
                title = excelFileName + " 시군별 참가인원현황";
            }
            else if (memberPart == "경북교육청" || memberPart == "관리자")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName && it.year == year)
                            .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                            .ToList();
                excelFileName = partyName;
                title = excelFileName + " 시군별 참가인원현황";
            }
            else if (memberPart == "학교")
            {
                excelPartyEntry = context.Z_PartyEntries
                            .Where(it => it.partyName == partyName && it.year == year
                                            && it.schoolName == memberName)
                            .OrderBy(it => it.city).ThenBy(it => it.gName).ThenBy(it => it.sName)
                            .ToList();
                excelFileName = partyName + " " + memberName;
                title = excelFileName + " 시군별 참가인원현황";
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("시군별참가인원현황");
                worksheet.Column(1).Width = 6;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 8;
                worksheet.Column(4).Width = 8;
                worksheet.Column(5).Width = 8;
                worksheet.Column(6).Width = 8;
                worksheet.Column(7).Width = 8;
                worksheet.Column(8).Width = 8;
                worksheet.Column(9).Width = 8;
                worksheet.Column(10).Width = 8;
                worksheet.Column(11).Width = 8;
                worksheet.Column(12).Width = 8;
                worksheet.Column(13).Width = 8;
                worksheet.Column(14).Width = 8;

                // 제목 시작
                int iMerge = 14;
                worksheet.Cells[1, 1, 1, iMerge].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 18;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Value = title;
                // 제목 끝
                for (int i = 1; i <= iMerge; i++)
                {
                    worksheet.Cells[3, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[3, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                var tableBody = worksheet.Cells["A3:A3"].LoadFromCollection(
                    (from m in listInwon
                     select new
                     {
                         순 = m.no.ToString(),
                         시군 = m.cname,
                         남초 = m.primaryMan == 0 ? "" : m.primaryMan.ToString(),
                         여초 = m.primaryWoman == 0 ? "" : m.primaryWoman.ToString(),
                         초등계 = m.primaryHap == 0 ? "" : m.primaryHap.ToString(),
                         남중 = m.middleMan == 0 ? "" : m.middleMan.ToString(),
                         여중 = m.middleWoman == 0 ? "" : m.middleWoman.ToString(),
                         중학계 = m.middleHap == 0 ? "" : m.middleHap.ToString(),
                         남고 = m.highMan == 0 ? "" : m.highMan.ToString(),
                         여고 = m.highWoman == 0 ? "" : m.highWoman.ToString(),
                         고등계 = m.highHap == 0 ? "" : m.highHap.ToString(),
                         선수계 = m.playerHap == 0 ? "" : m.playerHap.ToString(),
                         임원수 = m.officer == 0 ? "" : m.officer.ToString(),
                         합계 = m.totalHap == 0 ? "" : m.totalHap.ToString()
                     })
                     , true);

                int cellsNumber = listInwon.Count() + 5;
                worksheet.Cells[cellsNumber, 1, cellsNumber, 13].Merge = true;
                worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[cellsNumber, 1].Value = "위와 같이 경북학생체육대회에 참가신청 합니다.";
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

                // LoadFromCollection 호출 직후에 추가
                int headerRow = 3;
                int dataStartRow = headerRow + 1;
                int dataEndRow = listInwon.Count() + headerRow;
                int firstNumericCol = 3; // "남초"는 C열(3)부터
                int lastCol = iMerge;

                // 헤더 가운데 정렬 (3행 전체)
                worksheet.Cells[headerRow, 1, headerRow, lastCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[headerRow, 1, headerRow, lastCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // 숫자(데이터) 열 가운데 정렬 (C열~M열)
                if (dataEndRow >= dataStartRow)
                {
                    worksheet.Cells[dataStartRow, firstNumericCol, dataEndRow, lastCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[dataStartRow, firstNumericCol, dataEndRow, lastCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                worksheet.Cells[dataStartRow, 1, dataEndRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // 필요하면 '시군'(B열)도 가운데 정렬하려면 아래처럼 변경
                // worksheet.Cells[dataStartRow, 2, dataEndRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                FileUtil.SaveAs(JSRuntimeInjector, excelFileName + " 시군별 참가현황.xlsx", package.GetAsByteArray());
            }
        }

        private void GoToGameInwon()
        {
            NavigationManagerInjector.NavigateTo("/StudentPartyGameInwon");
        }

        class Inwon
        {
            public int no { get; set; }
            public string? cname { get; set; }
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
