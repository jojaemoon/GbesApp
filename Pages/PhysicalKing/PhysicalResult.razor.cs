using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MoreLinq;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace GBES.Pages.PhysicalKing
{
    public partial class PhysicalResult
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
        List<string?> sNameList = new List<string>();
        List<string?> dNameList = new List<string> { "악력", "제자리멀리뛰기", "윗몸앞으로굽히기", "순환도전종목" };
        List<string?> playerList = new List<string>();

        public Z_GameResult model = new Z_GameResult();

        public string? cName { get; set; }
        public string? sName { get; set; }
        public string? dName { get; set; }
        public string? partyName { get; set; }

        public int jumsuHap { get; set; }

        public bool isShowMember = false;

        string? initializePassword = "";

        List<Z_PhysicalKingRecord> listGameResult = new List<Z_PhysicalKingRecord>();
        List<Z_PartyEntry> listPartyEnty = new List<Z_PartyEntry>();
        public string year { get; set; } = DateTime.Now.Year.ToString();

        #endregion

        protected override void OnInitialized()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;

            using var context = _contextFactory.CreateDbContext();
            Z_PartyName? pName = context.Z_PartyNames
                        .Where(it => it.partyName.Contains("경상북도 체력인증제")
                                    && it.etc == "체력사용")
                        .FirstOrDefault();
            partyName = pName.partyName;

            if (memberName == "경북교육청" || memberName == "관리자" || memberName.Contains("입력"))
            {
                if (pName.etc == "마감" || pName.etc == "진행" || pName.etc == "사용")
                {
                    isShowMember = true;
                }
            }
            else
            {
                //JSRuntimeInjector.InvokeVoidAsync("alert", "마감되었습니다.");
                //NavigationManagerInjector.NavigateTo("/");
            }

            cNameList = appState.GetCities();

        }

        // 악력 점수 주기
        private async Task GripInput(ChangeEventArgs args, Z_PhysicalKingRecord? record)
        {
            if (record == null)
            {
                return;
            }
            record.gripRecord = args.Value.ToString();

            using var context = _contextFactory.CreateDbContext();

            string[] arr = record.gripRecord.Split('.');

            string? jum = await context.Z_PhysicalScoreCards
                                .Where(it => it.partyName == partyName
                                            && it.dName == "악력"
                                            && it.record == arr[0])
                                .Select(it => it.jumsu)
                                .SingleOrDefaultAsync();

            record.gripJumsu = jum == null ? 0 : int.Parse(jum);
            record.jumsuHap = record.gripJumsu + record.jumpJumsu + record.bendJumsu + record.run50mJumsu + record.movementJumsu;
        }

        // 제자리멀리뛰기 점수 주기
        private async Task JumpInput(ChangeEventArgs args, Z_PhysicalKingRecord? record)
        {
            if (record == null)
            {
                return;
            }
            record.jumpRecord = args.Value.ToString();

            string[] arr = record.jumpRecord.Split('.');

            using var context = _contextFactory.CreateDbContext();
            string? jum = await context.Z_PhysicalScoreCards
                                .Where(it => it.partyName == partyName
                                            && it.dName == "제자리멀리뛰기"
                                            && it.record == arr[0])
                                .Select(it => it.jumsu)
                                .SingleOrDefaultAsync();

            record.jumpJumsu = jum == null ? 0 : int.Parse(jum);
            record.jumsuHap = record.gripJumsu + record.jumpJumsu + record.bendJumsu + record.run50mJumsu + record.movementJumsu;
        }

        // 앉아윗몸앞으로굽히기 점수 주기
        private async Task BendInput(ChangeEventArgs args, Z_PhysicalKingRecord? record)
        {
            if (record == null)
            {
                return;
            }

            record.bendRecord = args.Value.ToString();

            string[] arr = record.bendRecord.Split('.');

            using var context = _contextFactory.CreateDbContext();
            string? jum = await context.Z_PhysicalScoreCards
                                .Where(it => it.partyName == partyName
                                            && it.dName == "앉아윗몸앞으로굽히기"
                                            && it.record == arr[0])
                                .Select(it => it.jumsu)
                                .SingleOrDefaultAsync();

            record.bendJumsu = jum == null ? 0 : int.Parse(jum);
            record.jumsuHap = record.gripJumsu + record.jumpJumsu + record.bendJumsu + record.run50mJumsu + record.movementJumsu;
        }

        // 50m달리기 점수 주기   2025  40m달리기
        private async Task Run50mInput(ChangeEventArgs args, Z_PhysicalKingRecord? record)
        {
            if (record == null)
            {
                return;
            }

            record.run50mRecord = args.Value.ToString();

            using var context = _contextFactory.CreateDbContext();

            string? jum = await context.Z_PhysicalScoreCards
                                .Where(it => it.partyName == partyName
                                            && it.dName == "40m 달리기"
                                            && it.record == record.run50mRecord)
                                .Select(it => it.jumsu)
                                .SingleOrDefaultAsync();

            record.run50mJumsu = jum == null ? 0 : int.Parse(jum);
            record.jumsuHap = record.gripJumsu + record.jumpJumsu + record.bendJumsu + record.run50mJumsu + record.movementJumsu;
        }

        // 순환도전 점수 주기
        private async Task MovementInput(ChangeEventArgs args, Z_PhysicalKingRecord? record)
        {
            if (record == null)
            {
                return;
            }

            record.movementRecord = args.Value.ToString();

            using var context = _contextFactory.CreateDbContext();
            string? jum = await context.Z_PhysicalScoreCards
                                .Where(it => it.partyName == partyName
                                            && it.dName == "순환도전"
                                            && it.record == record.movementRecord)
                                .Select(it => it.jumsu)
                                .SingleOrDefaultAsync();

            record.movementJumsu = jum == null ? 0 : int.Parse(jum);

            //// 득점 합 계산하기
            //record.jumsuHap = record.gripJumsu + record.jumpJumsu + record.bendJumsu + record.movementJumsu;
            //context.Z_PhysicalKingRecords.Update(record);
            //await context.SaveChangesAsync();

            //// 종별 학년별 종합 순위
            //List<Z_PhysicalKingRecord> sortRecordList = await context.Z_PhysicalKingRecords
            //                                            .Where(it => it.partyName == partyName
            //                                                        && it.sName == record.sName
            //                                                        && it.schoolYear == record.schoolYear)
            //                                            .OrderByDescending(it => it.jumsuHap)
            //                                            .ToListAsync();
            //for (int i = 0; i < sortRecordList.Count(); i++)
            //{
            //    if (string.IsNullOrEmpty(sortRecordList[i].gripRecord))
            //    {
            //        sortRecordList[i].gripJumsu = 0;
            //        sortRecordList[i].gripRank = 0;
            //    }
            //    if (string.IsNullOrEmpty(sortRecordList[i].jumpRecord))
            //    {
            //        sortRecordList[i].jumpJumsu = 0;
            //        sortRecordList[i].jumpRank = 0;
            //    }
            //    if (string.IsNullOrEmpty(sortRecordList[i].bendRecord))
            //    {
            //        sortRecordList[i].bendJumsu = 0;
            //        sortRecordList[i].bendRank = 0;
            //    }
            //    if (string.IsNullOrEmpty(sortRecordList[i].run50mRecord))
            //    {
            //        sortRecordList[i].run50mJumsu = 0;
            //        sortRecordList[i].run50mRank = 0;
            //    }
            //    if (string.IsNullOrEmpty(sortRecordList[i].movementRecord))
            //    {
            //        sortRecordList[i].movementJumsu = 0;
            //        sortRecordList[i].movementRank = 0;
            //    }
            //    sortRecordList[i].jumsuHap = sortRecordList[i].gripJumsu + sortRecordList[i].jumpJumsu
            //        + sortRecordList[i].bendJumsu + sortRecordList[i].movementJumsu;

            //    if (sortRecordList[i].jumsuHap == 0)
            //    {
            //        sortRecordList[i].rank = 0;
            //    }
            //}

            //for (int i = 0; i < sortRecordList.Count(); i++)
            //{
            //    if (sortRecordList[i].jumsuHap == 0)
            //    {
            //        sortRecordList[i].rank = 0;
            //    }
            //    else
            //    {
            //        int L = 1;
            //        for (int j = 0; j < sortRecordList.Count(); j++)
            //        {
            //            if (sortRecordList[i].jumsuHap < sortRecordList[j].jumsuHap)
            //            {
            //                L++;
            //            }
            //            sortRecordList[i].rank = L;
            //        }
            //    }
            //}

            //context.UpdateRange(sortRecordList);
            //await context.SaveChangesAsync();

            //listGameResult = context.Z_PhysicalKingRecords
            //               .Where(it => it.partyName == partyName
            //                                && it.city == cName
            //                                && it.sName == sName)
            //               .OrderBy(it => it.Id)
            //               .ToList();

            // StateHasChanged();
        }

        private async Task Save(Z_PhysicalKingRecord personRecord)
        {
            if (personRecord == null)
            {
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            // 득점 합 계산하기
            personRecord.jumsuHap = personRecord.gripJumsu + personRecord.jumpJumsu + personRecord.bendJumsu + personRecord.run50mJumsu + personRecord.movementJumsu;
            personRecord.etc = memberName;
            context.Z_PhysicalKingRecords.Update(personRecord);
            await context.SaveChangesAsync();


            // 종별 학년별 종합 순위
            List<Z_PhysicalKingRecord> sortRecordList = await context.Z_PhysicalKingRecords
                                                        .Where(it => it.partyName == partyName
                                                                    && it.sName == personRecord.sName
                                                                    && it.schoolYear == personRecord.schoolYear)
                                                        .OrderByDescending(it => it.jumsuHap)
                                                        .ToListAsync();
            //for (int i = 0; i < sortRecordList.Count(); i++)
            //{
            //    if (string.IsNullOrEmpty(sortRecordList[i].gripRecord))
            //    {
            //        sortRecordList[i].gripJumsu = 0;
            //        sortRecordList[i].gripRank = 0;
            //    }
            //    if (string.IsNullOrEmpty(sortRecordList[i].jumpRecord))
            //    {
            //        sortRecordList[i].jumpJumsu = 0;
            //        sortRecordList[i].jumpRank = 0;
            //    }
            //    if (string.IsNullOrEmpty(sortRecordList[i].bendRecord))
            //    {
            //        sortRecordList[i].bendJumsu = 0;
            //        sortRecordList[i].bendRank = 0;
            //    }
            //    if (string.IsNullOrEmpty(sortRecordList[i].run50mRecord))
            //    {
            //        sortRecordList[i].run50mJumsu = 0;
            //        sortRecordList[i].run50mRank = 0;
            //    }
            //    if (string.IsNullOrEmpty(sortRecordList[i].movementRecord))
            //    {
            //        sortRecordList[i].movementJumsu = 0;
            //        sortRecordList[i].movementRank = 0;
            //    }
            //    sortRecordList[i].jumsuHap = sortRecordList[i].gripJumsu + sortRecordList[i].jumpJumsu
            //        + sortRecordList[i].bendJumsu + sortRecordList[i].movementJumsu;

            //    if (sortRecordList[i].jumsuHap == 0)
            //    {
            //        sortRecordList[i].rank = 0;
            //    }
            //}

            //for (int i = 0; i < sortRecordList.Count(); i++)
            //{
            //    if (sortRecordList[i].jumsuHap == 0)
            //    {
            //        sortRecordList[i].rank = 0;
            //    }
            //    else
            //    {
            //        int L = 1;
            //        for (int j = 0; j < sortRecordList.Count(); j++)
            //        {
            //            if (sortRecordList[i].jumsuHap < sortRecordList[j].jumsuHap)
            //            {
            //                L++;
            //            }
            //        }
            //        sortRecordList[i].rank = L;
            //    }
            //}

            int L = 1;
            int preJumsu = 0;
            for (int i = 0; i < sortRecordList.Count(); i++)
            {
                if (sortRecordList[i].jumsuHap == 0)
                {
                    sortRecordList[i].rank = 0;
                }
                else
                {
                    if (i == 0)
                    {
                        sortRecordList[i].rank = L;
                        preJumsu = sortRecordList[i].jumsuHap;
                    }
                    else
                    {
                        if (sortRecordList[i].jumsuHap == preJumsu)
                        {
                            sortRecordList[i].rank = L;
                        }
                        else if (sortRecordList[i].jumsuHap < preJumsu)
                        {
                            L++;
                            sortRecordList[i].rank = L;
                            preJumsu = sortRecordList[i].jumsuHap;
                        }
                    }
                }
            }

            context.UpdateRange(sortRecordList);
            await context.SaveChangesAsync();

            SectionClick(personRecord.sName);

            //listGameResult = context.Z_PhysicalKingRecords
            //               .Where(it => it.partyName == partyName
            //                                && it.city == cName
            //                                && it.sName == sName)
            //               .OrderBy(it => it.name)
            //               .ToList();
        }

        // 리스트 종목명 선택시
        public void SelectCityName(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            cName = e.Value.ToString();

            //SectionClick(sName);

            //using var context = _contextFactory.CreateDbContext();

            //if (cName != "전체")
            //{
            //    listGameResult = context.Z_PhysicalKingRecords
            //               .Where(it => it.partyName == partyName
            //                                && it.city == cName)
            //               .OrderBy(it => it.Id)
            //               .ToList();
            //}
            //else
            //{
            //    listGameResult = context.Z_PhysicalKingRecords
            //              .Where(it => it.partyName == partyName)
            //              .OrderBy(it => it.Id)
            //              .ToList();
            //}
            StateHasChanged();
        }

        // 종별 선택시
        private async Task SectionClick(string? sname)
        {
            sName = sname;
            using var context = _contextFactory.CreateDbContext();
            if (cName == "전체")
            {
                listGameResult = context.Z_PhysicalKingRecords
                    .Where(it => it.partyName == partyName
                                    && it.sName == sname)
                    .OrderByDescending(it => it.schoolYear).ThenByDescending(it => it.jumsuHap)
                    .ToList();
            }
            else
            {
                if (memberName == "경북교육청" || memberName == "관리자" || memberName.Contains("입력"))
                {
                    // 입력
                    listGameResult = context.Z_PhysicalKingRecords
                      .Where(it => it.partyName == partyName
                                        && it.city == cName
                                        && it.sName == sname)
                      .OrderBy(it => it.jumsuHap).ThenBy(it => it.numbering!.Length).ThenBy(it => it.numbering).ThenBy(it => it.name).ToList();
                }
                else
                {
                    // 공개
                    listGameResult = context.Z_PhysicalKingRecords
                    .Where(it => it.partyName == partyName
                                    && it.city == cName
                                    && it.sName == sname)
                    .OrderByDescending(it => it.schoolYear).ThenByDescending(it => it.jumsuHap)
                    .ToList();
                }
            }

            //else
            //{
            //    if (cName == "전체")
            //    {
            //        listGameResult = context.Z_PhysicalKingRecords
            //          .Where(it => it.partyName == partyName
            //                            && it.sName == sname)
            //          .OrderBy(it => it.schoolYear).ThenByDescending(it => it.jumsuHap)
            //          .ToList();
            //    }
            //    else
            //    {
            //        listGameResult = context.Z_PhysicalKingRecords
            //          .Where(it => it.partyName == partyName
            //                            && it.city == cName
            //                            && it.sName == sname
            //                            && it.rank != 0)
            //          .OrderBy(it => it.schoolYear).ThenByDescending(it => it.jumsuHap)
            //          .ToList();
            //        List<Z_PhysicalKingRecord> imsiResult = context.Z_PhysicalKingRecords
            //          .Where(it => it.partyName == partyName
            //                            && it.city == cName
            //                            && it.sName == sname
            //                            && it.rank == 0)
            //          .OrderBy(it => it.schoolYear).ThenByDescending(it => it.jumsuHap)
            //          .ToList();
            //        listGameResult.AddRange(imsiResult);
            //    }
            //}

            StateHasChanged();

            //if (cName == "전체")
            //{
            //    if (memberName == "경북교육청" || memberName == "관리자")
            //    {
            //        listGameResult = context.Z_PhysicalKingRecords
            //              .Where(it => it.partyName == partyName
            //                                && it.sName == sname)
            //              .OrderBy(it => it.schoolYear).ThenBy(it => it.rank)
            //              .ToList();
            //    }
            //    else
            //    {
            //        listGameResult = context.Z_PhysicalKingRecords
            //              .Where(it => it.partyName == partyName
            //                                && it.sName == sname)
            //              .OrderBy(it => it.schoolYear).ThenBy(it => it.rank)
            //              .ToList();
            //    }
            //}
            //else
            //{

            //}

            //if (cName != "전체")
            //{
            //    listGameResult = context.Z_PhysicalKingRecords
            //               .Where(it => it.partyName == partyName
            //                                && it.city == cName
            //                                && it.sName == sname)
            //               .OrderBy(it => it.name)
            //               .ToList();
            //}
            //else
            //{
            //    listGameResult = context.Z_PhysicalKingRecords
            //              .Where(it => it.partyName == partyName
            //                                && it.sName == sname)
            //              .OrderBy(it => it.name)
            //              .ToList();
            //}

            //if (memberName != "경북교육청" && memberName != "관리자")
            //{
            //    listGameResult = context.Z_PhysicalKingRecords
            //              .Where(it => it.partyName == partyName
            //                                && it.sName == sname
            //                                && it.rank != 0)
            //              .OrderBy(it => it.schoolYear).ThenBy(it => it.rank)
            //              .ToList();
            //}

        }

        // 0 이면 공백
        private string IsZeroToBlank(int s)
        {
            if (s == 0) return "";
            return s.ToString();
        }

        private string ReplaceMinSec(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            string[] arr = s.Split('.');
            if (arr.Length == 2)
            {
                s = arr[0] + "분" + arr[1] + "초";
            }
            else if (arr.Length == 1)
            {
                s = arr[0] + "초";
            }
            return s;
        }

        private string ReplaceSec(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s + "초";
        }

        //private string FirstChar(string str)
        //{
        //    str = str.Substring(0, 1))+"**";
        //}

        #region  기록 초기화 하기 
        private async Task MakeResultInitialize_OLD()
        {
            if (initializePassword != "1qaz")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "패스워드가 틀립니다.");
            }
            else
            {
                bool confirmed = await JSRuntimeInjector.InvokeAsync<bool>("confirm", "기존 자료가 모두 지워집니다. 진행할까요?");

                if (confirmed)
                {
                    using var context = _contextFactory.CreateDbContext();

                    List<Z_PhysicalKingRecord> listKingRecord = new List<Z_PhysicalKingRecord>();

                    listKingRecord = await context.Z_PhysicalKingRecords
                                     .Where(it => it.partyName == partyName)
                                      .ToListAsync();
                    context.Z_PhysicalKingRecords.RemoveRange(listKingRecord);
                    // await context.SaveChangesAsync();

                    Z_PhysicalKingRecord kingRecord = new Z_PhysicalKingRecord();

                    List<Z_PartyEntry> listEntryAll = await context.Z_PartyEntries
                                                   .Where(it => it.partyName == partyName)
                                                   .OrderBy(it => it.city)
                                                   .ToListAsync();




                    // 남자초등부
                    List<Z_PartyEntry> listEntry = await context.Z_PartyEntries
                                                   .Where(it => it.partyName == partyName
                                                            && it.schoolName.Contains("초등")
                                                            && it.code == "남")
                                                   .OrderBy(it => it.city)
                                                   .ToListAsync();
                    foreach (var item in listEntryAll)
                    {
                        kingRecord = new Z_PhysicalKingRecord();
                        kingRecord.city = item.city;
                        kingRecord.code = item.code;
                        kingRecord.name = item.name;
                        kingRecord.schoolName = item.schoolName;
                        kingRecord.schoolYear = item.schoolYear;
                        if (string.IsNullOrEmpty(item.sName))
                        {
                            kingRecord.sName = "남자초등부";
                            item.sName = "남자초등부";
                        }
                        else
                        {
                            kingRecord.sName = item.sName;
                        }
                        kingRecord.partyName = item.partyName;
                        kingRecord.year = item.year;

                        listKingRecord.Add(kingRecord);
                    }
                    context.Z_PartyEntries.UpdateRange(listEntry);
                    context.Z_PhysicalKingRecords.UpdateRange(listKingRecord);

                    //await context.SaveChangesAsync();

                    // 여자초등부
                    listKingRecord = new List<Z_PhysicalKingRecord>();
                    listEntry = await context.Z_PartyEntries
                                                   .Where(it => it.partyName == partyName
                                                            && it.schoolName.Contains("초등")
                                                            && it.code == "여")
                                                   .OrderBy(it => it.city)
                                                   .ToListAsync();
                    foreach (var item in listEntry)
                    {
                        kingRecord = new Z_PhysicalKingRecord();
                        kingRecord.city = item.city;
                        kingRecord.code = item.code;
                        kingRecord.name = item.name;
                        kingRecord.schoolName = item.schoolName;
                        kingRecord.schoolYear = item.schoolYear;
                        if (string.IsNullOrEmpty(item.sName))
                        {
                            kingRecord.sName = "여자초등부";
                            item.sName = "여자초등부";
                        }
                        else
                        {
                            kingRecord.sName = item.sName;
                        }
                        kingRecord.partyName = item.partyName;
                        kingRecord.year = item.year;

                        listKingRecord.Add(kingRecord);
                    }
                    context.Z_PartyEntries.UpdateRange(listEntry);
                    context.Z_PhysicalKingRecords.UpdateRange(listKingRecord);

                    // await context.SaveChangesAsync();

                    // 남자중학부
                    listEntry = await context.Z_PartyEntries
                                                   .Where(it => it.partyName == partyName
                                                            && it.schoolName.Contains("중학")
                                                            && it.code == "남")
                                                   .OrderBy(it => it.city)
                                                   .ToListAsync();
                    foreach (var item in listEntry)
                    {
                        kingRecord = new Z_PhysicalKingRecord();
                        kingRecord.city = item.city;
                        kingRecord.code = item.code;
                        kingRecord.name = item.name;
                        kingRecord.schoolName = item.schoolName;
                        kingRecord.schoolYear = item.schoolYear;
                        if (string.IsNullOrEmpty(item.sName))
                        {
                            kingRecord.sName = "남자중학부";
                            item.sName = "남자중학부";
                        }
                        else
                        {
                            kingRecord.sName = item.sName;
                        }
                        kingRecord.partyName = item.partyName;
                        kingRecord.year = item.year;

                        listKingRecord.Add(kingRecord);
                    }
                    context.Z_PartyEntries.UpdateRange(listEntry);
                    context.Z_PhysicalKingRecords.UpdateRange(listKingRecord);

                    //await context.SaveChangesAsync();

                    // 여자중학부
                    listKingRecord = new List<Z_PhysicalKingRecord>();
                    listEntry = await context.Z_PartyEntries
                                                   .Where(it => it.partyName == partyName
                                                            && it.schoolName.Contains("중학")
                                                            && it.code == "여")
                                                   .OrderBy(it => it.city)
                                                   .ToListAsync();
                    foreach (var item in listEntry)
                    {
                        kingRecord = new Z_PhysicalKingRecord();
                        kingRecord.city = item.city;
                        kingRecord.code = item.code;
                        kingRecord.name = item.name;
                        kingRecord.schoolName = item.schoolName;
                        kingRecord.schoolYear = item.schoolYear;
                        if (string.IsNullOrEmpty(item.sName))
                        {
                            kingRecord.sName = "여자중학부";
                            item.sName = "여자중학부";
                        }
                        else
                        {
                            kingRecord.sName = item.sName;
                        }
                        kingRecord.partyName = item.partyName;
                        kingRecord.year = item.year;

                        listKingRecord.Add(kingRecord);
                    }
                    context.Z_PartyEntries.UpdateRange(listEntry);
                    context.Z_PhysicalKingRecords.UpdateRange(listKingRecord);

                    //await context.SaveChangesAsync();

                    // 남자고등부
                    listEntry = await context.Z_PartyEntries
                                                   .Where(it => it.partyName == partyName
                                                            && it.schoolName.Contains("고등")
                                                            && it.code == "남")
                                                   .OrderBy(it => it.city)
                                                   .ToListAsync();
                    foreach (var item in listEntry)
                    {
                        kingRecord = new Z_PhysicalKingRecord();
                        kingRecord.city = item.city;
                        kingRecord.code = item.code;
                        kingRecord.name = item.name;
                        kingRecord.schoolName = item.schoolName;
                        kingRecord.schoolYear = item.schoolYear;
                        if (string.IsNullOrEmpty(item.sName))
                        {
                            kingRecord.sName = "남자고등부";
                            item.sName = "남자고등부";
                        }
                        else
                        {
                            kingRecord.sName = item.sName;
                        }
                        kingRecord.partyName = item.partyName;
                        kingRecord.year = item.year;

                        listKingRecord.Add(kingRecord);
                    }
                    context.Z_PartyEntries.UpdateRange(listEntry);
                    context.Z_PhysicalKingRecords.UpdateRange(listKingRecord);

                    //await context.SaveChangesAsync();

                    // 여자중학부
                    listKingRecord = new List<Z_PhysicalKingRecord>();
                    listEntry = await context.Z_PartyEntries
                                                   .Where(it => it.partyName == partyName
                                                            && it.schoolName.Contains("고등")
                                                            && it.code == "여")
                                                   .OrderBy(it => it.city)
                                                   .ToListAsync();
                    foreach (var item in listEntry)
                    {
                        kingRecord = new Z_PhysicalKingRecord();
                        kingRecord.city = item.city;
                        kingRecord.code = item.code;
                        kingRecord.name = item.name;
                        kingRecord.schoolName = item.schoolName;
                        kingRecord.schoolYear = item.schoolYear;
                        if (string.IsNullOrEmpty(item.sName))
                        {
                            kingRecord.sName = "여자고등부";
                            item.sName = "여자고등부";
                        }
                        else
                        {
                            kingRecord.sName = item.sName;
                        }
                        kingRecord.partyName = item.partyName;
                        kingRecord.year = item.year;

                        listKingRecord.Add(kingRecord);
                    }
                    context.Z_PartyEntries.UpdateRange(listEntry);
                    context.Z_PhysicalKingRecords.UpdateRange(listKingRecord);

                    await context.SaveChangesAsync();

                    await JSRuntimeInjector.InvokeVoidAsync("alert", " 결과를 초기화 하였습니다.");
                }

            }

        }


        private async Task MakeResultInitialize()
        {
            if (initializePassword != "1qaz")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "패스워드가 틀립니다.");
                return;
            }

            bool confirmed = await JSRuntimeInjector.InvokeAsync<bool>(
                "confirm", "기존 자료가 모두 지워집니다. 진행할까요?");

            if (!confirmed) return;

            await using var context = _contextFactory.CreateDbContext();

            // 1) 기존 결과 삭제
            var oldRecords = await context.Z_PhysicalKingRecords
                .Where(it => it.partyName == partyName)
                .ToListAsync();

            context.Z_PhysicalKingRecords.RemoveRange(oldRecords);
            await context.SaveChangesAsync();

            // 공통 함수: 파티엔트리 → 결과 레코드 생성
            async Task CreateGroupAsync(string schoolPart, string genderCode, string defaultSName)
            {
                var entries = await context.Z_PartyEntries
                    .Where(it => it.partyName == partyName
                              && it.schoolName.Contains(schoolPart)
                              && it.code == genderCode)
                    .OrderBy(it => it.city)
                    .ToListAsync();

                foreach (var e in entries)
                {
                    // sName 비어 있으면 채우기
                    if (string.IsNullOrEmpty(e.sName))
                    {
                        e.sName = defaultSName;
                    }

                    var rec = new Z_PhysicalKingRecord
                    {
                        city = e.city,
                        code = e.code,
                        name = e.name,
                        schoolName = e.schoolName,
                        schoolYear = e.schoolYear,
                        sName = e.sName,
                        partyName = e.partyName,
                        year = e.year
                    };

                    // ★ 새 엔터티는 Add (Update 아님)
                    context.Z_PhysicalKingRecords.Add(rec);
                }

                // ★ entries 는 이미 트래킹 중이라 UpdateRange 불필요
                // context.Z_PartyEntries.UpdateRange(entries); // 필요 없음
            }

            // 2) 각 종별별 생성
            await CreateGroupAsync("초등", "남", "남자초등부");
            await CreateGroupAsync("초등", "여", "여자초등부");
            await CreateGroupAsync("중학", "남", "남자중학부");
            await CreateGroupAsync("중학", "여", "여자중학부");
            await CreateGroupAsync("고등", "남", "남자고등부");
            await CreateGroupAsync("고등", "여", "여자고등부");

            // 3) 한 번에 저장
            await context.SaveChangesAsync();

            await JSRuntimeInjector.InvokeVoidAsync("alert", "결과를 초기화 하였습니다.");
        }


        private async Task NumberingInput()
        {
            if (initializePassword != "1qaz")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "패스워드가 틀립니다.");
                return;
            }
            // 넘버링을 주자
            var cityOrder = new Dictionary<string, int>
            {
                ["경산"] = 1,
                ["영천"] = 2,
                ["청도"] = 3,
                ["고령"] = 4,
                ["성주"] = 5,
                ["칠곡"] = 6,
                ["경주"] = 7,
                ["포항"] = 8,
                ["울릉"] = 9,
                ["구미"] = 10,
                ["김천"] = 11,
                ["의성"] = 12,
                ["영양"] = 13,
                ["울진"] = 14,
                ["봉화"] = 15,
                ["영주"] = 16,
                ["청송"] = 17,
                ["영덕"] = 18,
                ["안동"] = 19,
                ["예천"] = 20,
                ["문경"] = 21,
                ["상주"] = 22
            };

            var schoolOrder = new Dictionary<string, int>
            {
                ["초등"] = 1,
                ["중학"] = 2,
                ["고등"] = 3
            };

            var snameOrder = new Dictionary<string, int>
            {
                ["남자초등부"] = 1,
                ["여자초등부"] = 2,
                ["남자중학부"] = 3,
                ["여자중학부"] = 4,
                ["남자고등부"] = 5,
                ["여자고등부"] = 6
            };

            string genderOrder(string sname)
            {
                if (string.IsNullOrEmpty(sname)) return "여"; // 기본값
                return sname.StartsWith("남") ? "남" : "여";
            }

            var genderRank = new Dictionary<string, int>
            {
                ["남"] = 1,
                ["여"] = 2
            };

            using var context = _contextFactory.CreateDbContext();

            var listRecord = await context.Z_PhysicalKingRecords
                        .Where(it => it.partyName == partyName)
                        .ToListAsync();    // AsNoTracking() 빼기

            // 정렬은 그대로
            var sorted = listRecord
            // 1) 시군 순서
            .OrderBy(it =>
            {
                var city = it.city ?? "";
                return cityOrder.TryGetValue(city, out var ord) ? ord : 999;
            })
            // 2) 학교급: 초등 → 중학 → 고등
            .ThenBy(it =>
            {
                var schoolName = it.schoolName ?? "";

                string level =
                    schoolName.Contains("초등") ? "초등" :
                    schoolName.Contains("중학") ? "중학" :
                    schoolName.Contains("고등") ? "고등" :
                    "기타";

                return schoolOrder.TryGetValue(level, out var ord) ? ord : 999;
            })
            .ThenBy(it=>it.schoolName)
            // 3) 종별: 남자 → 여자 (sName 기준)
            .ThenBy(it =>
            {
                var sname = it.sName ?? "";
                string g = genderOrder(sname);  // "남" 또는 "여"
                return genderRank.TryGetValue(g, out var ord) ? ord : 999;
            })
            // 4) 학년 (숫자로 정렬)
            .ThenBy(it =>
            {
                if (int.TryParse(it.schoolYear, out var sy))
                    return sy;
                return 0;
            })
            // 5) 이름
            .ThenBy(it => it.name ?? "")
            .ToList();

            int numbering = 0;

            foreach (var item in sorted)
            {
                numbering++;

                // 이미 context가 추적 중인 엔터티라서 그냥 수정만 하면 됨
                item.numbering = numbering.ToString();
                item.passWord = "1234";
            }

            await context.SaveChangesAsync();

            await JSRuntimeInjector.InvokeVoidAsync("alert", "넘버링을 부여하였습니다.");
        }
        #endregion


        // 참가신청 다운로드
        public void DownLoadPartyEntry()
        {
            string excelFileName = "";
            string title = "";

            using var context = _contextFactory.CreateDbContext();

            List<Z_PhysicalKingRecord> excelPartyEntry = new List<Z_PhysicalKingRecord>();

            excelPartyEntry = context.Z_PhysicalKingRecords
                             .Where(it => it.partyName == partyName)
                             .OrderBy(it => it.numbering!.Length).ThenBy(it => it.numbering)
                             .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("참가신청리스트");
                worksheet.Column(1).Width = 6;
                worksheet.Column(2).Width = 6;
                worksheet.Column(3).Width = 6;
                worksheet.Column(4).Width = 6;
                worksheet.Column(5).Width = 10;
                worksheet.Column(6).Width = 6;
                worksheet.Column(7).Width = 8;
                worksheet.Column(8).Width = 6;
                worksheet.Column(9).Width = 6;
                worksheet.Column(10).Width = 6;
                worksheet.Column(11).Width = 6;
                worksheet.Column(12).Width = 6;
                worksheet.Column(13).Width = 6;
                worksheet.Column(14).Width = 6;
                worksheet.Column(15).Width = 6;
                worksheet.Column(16).Width = 6;
                worksheet.Column(17).Width = 6;
                worksheet.Column(18).Width = 6;
                worksheet.Column(19).Width = 6;
                worksheet.Column(20).Width = 6;
                worksheet.Column(21).Width = 6;
                worksheet.Column(22).Width = 6;
                worksheet.Column(23).Width = 6;
                worksheet.Column(24).Width = 6;
                worksheet.Column(25).Width = 6;
                worksheet.Column(26).Width = 6;

                // 제목 시작
                worksheet.Cells[1, 1, 1, 13].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Value = partyName;
                // 제목 끝

                for (int i = 1; i < 24; i++)
                {
                    worksheet.Cells[3, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[3, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                var tableBody = worksheet.Cells["A3:A3"].LoadFromCollection(
                    (from m in excelPartyEntry
                     select new
                     {
                         시군 = m.city,
                         배번 = m.numbering,
                         비밀번호 = m.sName,
                         이름 = m.name,
                         학교명 = m.schoolName,
                         학년 = m.schoolYear,
                         종별 = m.sName,
                         달리기40m기록 = m.run50mRecord,
                         달리기40m점수 = m.run50mJumsu,
                         달리기40m순위 = m.run50mRank,
                         악력기록 = m.gripRecord,
                         악력점수 = m.gripJumsu,
                         악력순위 = m.gripRank,
                         제자리멀리뛰기기록 = m.jumpRecord,
                         제자리멀리뛰기점수 = m.jumpJumsu,
                         제자리멀리뛰기순위 = m.jumpRank,
                         윗몸앞으로굽히기기록 = m.bendRecord,
                         윗몸앞으로굽히기점수 = m.bendJumsu,
                         윗몸앞으로굽히기순위 = m.bendRank,
                         순환운동기록 = m.movementRecord,
                         순환운동점수 = m.movementJumsu,
                         순환운동순위 = m.movementRank,
                         합계 = m.jumsuHap,
                         순위 = m.rank,
                         비고 = m.etc
                     })
                     , true);

                //int cellsNumber = excelPartyEntry.Count() + 5;
                //worksheet.Cells[cellsNumber, 1, cellsNumber, 13].Merge = true;
                //worksheet.Cells[cellsNumber, 1].Style.Font.Size = 16;
                //worksheet.Cells[cellsNumber, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //worksheet.Cells[cellsNumber, 1].Value = "위와 같이 경북학생체육대회에 참가신청 합니다.";
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

                FileUtil.SaveAs(JSRuntimeInjector, excelFileName + " 참가현황.xlsx", package.GetAsByteArray());
            }
        }
    }
}
