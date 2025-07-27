using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.StudentFestival
{
    public partial class StudentCityJonghap
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

        List<DuckJUm> listResult = new List<DuckJUm>();
        List<DuckJUm> finalList = new List<DuckJUm>();

        DuckJUm duckJum = new DuckJUm();

        Z_JonghapResult? sectionTrack = new Z_JonghapResult();
        Z_JonghapResult? sectionField = new Z_JonghapResult();

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

            List<Z_JonghapResult> sectionList = await context.Z_JonghapResults
                                                .Where(it => it.partyName == pName.partyName)
                                                .ToListAsync();
            if (sectionList.Count == 0)
            {
                JSRuntimeInjector.InvokeVoidAsync("alert", "대회결과가 없습니다.");
                return;
            }
            foreach (var cname in cNameList)
            {
                duckJum = new DuckJUm();
                duckJum.city = cname;
                sectionTrack = new Z_JonghapResult(); sectionField = new Z_JonghapResult();
                sectionTrack = sectionList.Where(it => it.sName == "남초부트랙" && it.city == cname).FirstOrDefault();
                sectionField = sectionList.Where(it => it.sName == "남초부필드" && it.city == cname).FirstOrDefault();
                duckJum.primaryMan = double.Parse(sectionTrack.jumsu) + double.Parse(sectionField.jumsu);

                sectionTrack = new Z_JonghapResult(); sectionField = new Z_JonghapResult();
                sectionTrack = sectionList.Where(it => it.sName == "여초부트랙" && it.city == cname).FirstOrDefault();
                sectionField = sectionList.Where(it => it.sName == "여초부필드" && it.city == cname).FirstOrDefault();
                duckJum.primaryWoman = double.Parse(sectionTrack.jumsu) + double.Parse(sectionField.jumsu);
                duckJum.primaryGye = duckJum.primaryMan + duckJum.primaryWoman;

                sectionTrack = new Z_JonghapResult(); sectionField = new Z_JonghapResult();
                sectionTrack = sectionList.Where(it => it.sName == "남중부트랙" && it.city == cname).FirstOrDefault();
                sectionField = sectionList.Where(it => it.sName == "남중부필드" && it.city == cname).FirstOrDefault();
                duckJum.middleMan = double.Parse(sectionTrack.jumsu) + double.Parse(sectionField.jumsu);

                sectionTrack = new Z_JonghapResult(); sectionField = new Z_JonghapResult();
                sectionTrack = sectionList.Where(it => it.sName == "여중부트랙" && it.city == cname).FirstOrDefault();
                sectionField = sectionList.Where(it => it.sName == "여중부필드" && it.city == cname).FirstOrDefault();
                duckJum.middleWoman = double.Parse(sectionTrack.jumsu) + double.Parse(sectionField.jumsu);
                duckJum.middleGye = duckJum.middleMan + duckJum.middleWoman;

                sectionTrack = new Z_JonghapResult(); sectionField = new Z_JonghapResult();
                sectionTrack = sectionList.Where(it => it.sName == "남고부트랙" && it.city == cname).FirstOrDefault();
                sectionField = sectionList.Where(it => it.sName == "남고부필드" && it.city == cname).FirstOrDefault();
                duckJum.highMan = double.Parse(sectionTrack.jumsu) + double.Parse(sectionField.jumsu);

                sectionTrack = new Z_JonghapResult(); sectionField = new Z_JonghapResult();
                sectionTrack = sectionList.Where(it => it.sName == "여고부트랙" && it.city == cname).FirstOrDefault();
                sectionField = sectionList.Where(it => it.sName == "여고부필드" && it.city == cname).FirstOrDefault();
                duckJum.highWoman = double.Parse(sectionTrack.jumsu) + double.Parse(sectionField.jumsu);
                duckJum.highGye = duckJum.highMan + duckJum.highWoman;

                duckJum.hap = duckJum.primaryGye + duckJum.middleGye + duckJum.highGye;

                listResult.Add(duckJum);
            }

            finalList = listResult.OrderByDescending(x => x.hap).ToList();
            int n = 0;
            foreach (var item in finalList)
            {
                n++;
                item.rank = n;
            }
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            Click();
        }
    }

    class DuckJUm
    {
        public int rank { get; set; }
        public string city { get; set; }
        public double primaryMan { get; set; }
        public double primaryWoman { get; set; }
        public double primaryGye { get; set; }
        public double middleMan { get; set; }
        public double middleWoman { get; set; }
        public double middleGye { get; set; }
        public double highMan { get; set; }
        public double highWoman { get; set; }
        public double highGye { get; set; }
        public double hap { get; set; }
    }
}
