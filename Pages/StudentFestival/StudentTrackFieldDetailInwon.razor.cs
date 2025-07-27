using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.StudentFestival
{
    public partial class StudentTrackFieldDetailInwon
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

        List<string?> cNameList = new List<string>();

        List<DetailInwon> listResult = new List<DetailInwon>();
        List<DetailInwon> finalList = new List<DetailInwon>();

        DetailInwon detailInwon = new DetailInwon();

        List<DetailInwon> listInwon = new List<DetailInwon>();

        string? memberName;
        string? memberPart;

        private async Task Click()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;

            using var context = _contextFactory.CreateDbContext();
            Z_PartyName? pName = await context.Z_PartyNames
                        .Where(it => it.partyName.Contains("학생체육대회")
                                    && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람") || it.etc == "진행")
                        .FirstOrDefaultAsync();

            cNameList = appState.GetCities();

            List<Z_PartyEntry> z_PartyEntries = await context.Z_PartyEntries
                                    .Where(it => it.partyName == pName.partyName
                                                    && it.gName == "육상")
                                    .ToListAsync();
            List<string?> detailNames = await context.Z_Details
                                                .Where(it => it.partyName.Contains("학생체육")
                                                            && it.gName == "육상")
                                                .Select(it => it.dName)
                                                .Distinct()
                                                .ToListAsync();
            foreach (var detail in detailNames)
            {
                detailInwon = new DetailInwon();

                detailInwon.detailName = detail;

                detailInwon.primaryMan = z_PartyEntries
                                        .Where(it=>it.sName.Contains("남자초등")
                                                    && (it.dNameOne==detail 
                                                            || it.dNameTwo == detail
                                                            || it.dNameThree == detail
                                                            || it.dNameFour == detail
                                                            || it.dNameFive == detail)
                                                ).Count();
                detailInwon.primaryWoman = z_PartyEntries
                                        .Where(it => it.sName.Contains("여자초등")
                                                    && (it.dNameOne == detail
                                                            || it.dNameTwo == detail
                                                            || it.dNameThree == detail
                                                            || it.dNameFour == detail
                                                            || it.dNameFive == detail)
                                                ).Count();
                detailInwon.middleMan = z_PartyEntries
                                        .Where(it => it.sName.Contains("남자중학")
                                                    && (it.dNameOne == detail
                                                            || it.dNameTwo == detail
                                                            || it.dNameThree == detail
                                                            || it.dNameFour == detail
                                                            || it.dNameFive == detail)
                                                ).Count();
                detailInwon.middleWoman = z_PartyEntries
                                        .Where(it => it.sName.Contains("여자중학")
                                                    && (it.dNameOne == detail
                                                            || it.dNameTwo == detail
                                                            || it.dNameThree == detail
                                                            || it.dNameFour == detail
                                                            || it.dNameFive == detail)
                                                ).Count();
                detailInwon.highMan = z_PartyEntries
                                        .Where(it => it.sName.Contains("남자고등")
                                                    && (it.dNameOne == detail
                                                            || it.dNameTwo == detail
                                                            || it.dNameThree == detail
                                                            || it.dNameFour == detail
                                                            || it.dNameFive == detail)
                                                ).Count();
                detailInwon.highWoman = z_PartyEntries
                                        .Where(it => it.sName.Contains("여자고등")
                                                    && (it.dNameOne == detail
                                                            || it.dNameTwo == detail
                                                            || it.dNameThree == detail
                                                            || it.dNameFour == detail
                                                            || it.dNameFive == detail)
                                                ).Count();

                detailInwon.hap= z_PartyEntries
                                        .Where(it => (it.dNameOne == detail
                                                            || it.dNameTwo == detail
                                                            || it.dNameThree == detail
                                                            || it.dNameFour == detail
                                                            || it.dNameFive == detail)
                                                ).Count();

                if (detail.Contains("mR"))
                {
                    detailInwon.primaryMan = z_PartyEntries
                                        .Where(it => it.sName.Contains("남자초등")
                                                    && (it.dNameOne == detail
                                                            || it.dNameTwo == detail
                                                            || it.dNameThree == detail
                                                            || it.dNameFour == detail
                                                            || it.dNameFive == detail)
                                                )
                                        .Select(it=>it.city)
                                        .Distinct()
                                        .Count();
                    detailInwon.primaryWoman = z_PartyEntries
                                        .Where(it => it.sName.Contains("여자초등")
                                                    && (it.dNameOne == detail
                                                            || it.dNameTwo == detail
                                                            || it.dNameThree == detail
                                                            || it.dNameFour == detail
                                                            || it.dNameFive == detail)
                                                )
                                        .Select(it => it.city)
                                        .Distinct()
                                        .Count();
                    detailInwon.middleMan = z_PartyEntries
                                            .Where(it => it.sName.Contains("남자중학")
                                                        && (it.dNameOne == detail
                                                                || it.dNameTwo == detail
                                                                || it.dNameThree == detail
                                                                || it.dNameFour == detail
                                                                || it.dNameFive == detail)
                                                    )
                                            .Select(it => it.city)
                                            .Distinct()
                                            .Count();
                    detailInwon.middleWoman = z_PartyEntries
                                            .Where(it => it.sName.Contains("여자중학")
                                                        && (it.dNameOne == detail
                                                                || it.dNameTwo == detail
                                                                || it.dNameThree == detail
                                                                || it.dNameFour == detail
                                                                || it.dNameFive == detail)
                                                    )
                                            .Select(it => it.city)
                                            .Distinct()
                                            .Count();
                    detailInwon.highMan = z_PartyEntries
                                            .Where(it => it.sName.Contains("남자고등")
                                                        && (it.dNameOne == detail
                                                                || it.dNameTwo == detail
                                                                || it.dNameThree == detail
                                                                || it.dNameFour == detail
                                                                || it.dNameFive == detail)
                                                    )
                                            .Select(it => it.city)
                                            .Distinct()
                                            .Count();
                    detailInwon.highWoman = z_PartyEntries
                                            .Where(it => it.sName.Contains("여자고등")
                                                        && (it.dNameOne == detail
                                                                || it.dNameTwo == detail
                                                                || it.dNameThree == detail
                                                                || it.dNameFour == detail
                                                                || it.dNameFive == detail)
                                                    )
                                            .Select(it => it.city)
                                            .Distinct()
                                            .Count();

                    detailInwon.hap = z_PartyEntries
                                            .Where(it => (it.dNameOne == detail
                                                                || it.dNameTwo == detail
                                                                || it.dNameThree == detail
                                                                || it.dNameFour == detail
                                                                || it.dNameFive == detail)
                                                    )
                                            .Select(it => it.city)
                                            .Distinct()
                                            .Count();
                }

                listInwon.Add(detailInwon);
            }

           

            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            Click();
        }

        private string IsZeroBlank(int str)
        {
            if (str == 0)
            {
                return "";
            }

            return str.ToString();
        }

        private string StyleForNumber(int n)
        {
            if (n > 5) return "";
            else if (n > 0) return "color:red; background:lightgreen; font-weight: bold;";
            return "";
        }

    }

    class DetailInwon
    {
        public string detailName { get; set; }
        public int primaryMan { get; set; }
        public int primaryWoman { get; set; }
        public int middleMan { get; set; }
        public int middleWoman { get; set; }
        public int highMan { get; set; }
        public int highWoman { get; set; }
        public int hap { get; set; }
    }
}
