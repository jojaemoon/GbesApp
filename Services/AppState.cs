using DocumentFormat.OpenXml.Bibliography;
using GBES.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GBES.Services
{
    public class AppState
    {
        private readonly GBESportsAppDbContext _context;

        public AppState(GBESportsAppDbContext context)
        {
            this._context = context;
        }

        //[Inject]
        //public IDbContextFactory<GBESportsAppDbContext>? _contextFactory { get; set; }

        public string? userName { get; set; } = "";
        public string? userPart { get; set; } = "";
        public string? userCity { get; set; } = "";
        public string? userManager { get; set; } = "";

        public string? GetYear { get; set; } = DateTime.Now.Year.ToString();

        public event Action OnChange;

        public void SetLogin(string membername, string partname, string usercity, string usermanager)
        {
            userName = membername;
            userPart = partname;
            userCity = usercity;
            userManager = usermanager;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }


        #region  데이타 처리
        // 전체 지원교육청명 돌려 주기
        public List<string> GetCities()
        {
            //using var context = _contextFactory.CreateDbContext();
            List<string> list = _context.Z_Members
                                 .Where(it => it.division == "교육지원청")
                                 .Select(it => it.memberName.Substring(0, 2))
                                 .ToList();
            return list;
        }

        // 지원교육청명 소속 학교명 돌려 주기  초중고
        public List<string> GetSchoolNames(string cname)
        {
            //using var context = _contextFactory.CreateDbContext();
            List<string?> list = _context.Z_Members
                                 .Where(it => it.city == cname.Trim().Substring(0, 2))
                                 .OrderBy(it => it.Id)
                                 .Select(it => it.memberName)
                                 .ToList();
            return list;
        }

        // 지원교육청명 소속 학교명 돌려 주기  초중
        public List<string> GetSchoolNamesPrimaryMiddle(string cname)
        {
            //using var context = _contextFactory.CreateDbContext();
            List<string?> list = _context.Z_Members
                                 .Where(it => it.city == cname.Trim().Substring(0, 2) && !it.memberName.Contains("고등"))
                                 .OrderBy(it => it.Id)
                                 .Select(it => it.memberName)
                                 .ToList();
            return list;
        }

        // 전체 종목명 돌려 주기
        public List<string> GetGameNames(string partyName)
        {
            //using var context = _contextFactory.CreateDbContext();
            List<string?> list = _context.Z_Events
                                 .Where(it => (it.specialTwo == partyName
                                                || it.specialThree == partyName)
                                                && it.year == GetYear)
                                 .OrderBy(it => it.gName)
                                 .Select(it => it.gName)
                                 .ToList();
            return list;
        }

        // 선택 종목의 전체 종별 돌려 주기
        public List<string> GetSectionNames(string? partyName, string? gname)
        {
            List<string?> list = _context.Z_Sections
                                 .Where(it => it.partyName.Contains(partyName)
                                            && it.year == GetYear
                                            && it.gName == gname)
                                 .Select(it => it.sName)
                                 .ToList();
            return list;
        }

        // 선택 종목, 종별의 전체 세부종목 돌려 주기
        public List<string> GetDetailNames(string? partyName, string? gname, string? sname)
        {
            List<string?> list = _context.Z_Details
                                 .Where(it => it.partyName.Contains(partyName)
                                                && it.year == GetYear
                                                && it.gName == gname
                                                && it.sName == sname)
                                 .OrderBy(it => it.code.Length).ThenBy(it => it.code)
                                 .Select(it => it.dName)
                                 .ToList();
            return list;
        }

        // 세부종목 참가신청한 목록 보내기
        internal List<Z_PartyEntry> GetPlayerListByGameDetail(string? partyName, string? gName, string? sName, string? dName)
        {
            List<Z_PartyEntry> list = _context.Z_PartyEntries
                                 .Where(it => it.partyName == partyName
                                                && it.gName == gName
                                                && it.sName == sName
                                                && (it.dNameOne == dName || it.dNameTwo == dName
                                                        || it.dNameThree == dName || it.dNameFour == dName))
                                 .ToList();
            //학년별 육상경기대회 1학년 상위 2학년 참가  
            if(partyName.Contains("학년별 육상경기대회") && sName.Contains("중학")  && dName== "3,000m 2학년")
            {
                string ss = "3,000m 1학년";
                List<Z_PartyEntry> listFirst = _context.Z_PartyEntries
                                .Where(it => it.partyName == partyName
                                               && it.gName == gName
                                               && it.sName == sName
                                               && (it.dNameOne == ss || it.dNameTwo == ss
                                                       || it.dNameThree == ss || it.dNameFour == ss))
                                .ToList();
                list.AddRange(listFirst);
            }
            else if (partyName.Contains("학년별 육상경기대회") && sName.Contains("중학") && dName == "110mH 2학년")
            {
                string ss = "110mH 1학년";
                List<Z_PartyEntry> listFirst = _context.Z_PartyEntries
                                .Where(it => it.partyName == partyName
                                               && it.gName == gName
                                               && it.sName == sName
                                               && (it.dNameOne == ss || it.dNameTwo == ss
                                                       || it.dNameThree == ss || it.dNameFour == ss))
                                .ToList();
                list.AddRange(listFirst);
            }
            else if (partyName.Contains("학년별 육상경기대회") && sName.Contains("중학") && dName == "100mH 2학년")
            {
                string ss = "100mH 1학년";
                List<Z_PartyEntry> listFirst = _context.Z_PartyEntries
                                .Where(it => it.partyName == partyName
                                               && it.gName == gName
                                               && it.sName == sName
                                               && (it.dNameOne == ss || it.dNameTwo == ss
                                                       || it.dNameThree == ss || it.dNameFour == ss))
                                .ToList();
                list.AddRange(listFirst);
            }
            else if (partyName.Contains("학년별 육상경기대회") && sName.Contains("중학") && dName == "3,000mW 2학년")
            {
                string ss = "3,000mW 1학년";
                List<Z_PartyEntry> listFirst = _context.Z_PartyEntries
                                .Where(it => it.partyName == partyName
                                               && it.gName == gName
                                               && it.sName == sName
                                               && (it.dNameOne == ss || it.dNameTwo == ss
                                                       || it.dNameThree == ss || it.dNameFour == ss))
                                .ToList();
                list.AddRange(listFirst);
            }
            else if (partyName.Contains("학년별 육상경기대회") && sName.Contains("중학") && dName == "세단뛰기 2학년")
            {
                string ss = "세단뛰기 1학년";
                List<Z_PartyEntry> listFirst = _context.Z_PartyEntries
                                .Where(it => it.partyName == partyName
                                               && it.gName == gName
                                               && it.sName == sName
                                               && (it.dNameOne == ss || it.dNameTwo == ss
                                                       || it.dNameThree == ss || it.dNameFour == ss))
                                .ToList();
                list.AddRange(listFirst);
            }





            if (dName == "계주")
            {
                list = _context.Z_PartyEntries
                                .Where(it => it.partyName == partyName
                                               && it.gName == gName
                                               && it.sName == sName)
                                .ToList();
            }
            else if (gName == "양궁" || gName == "자전거" || gName == "체조" || gName == "소프트테니스")
            {
                list = _context.Z_PartyEntries
                                .Where(it => it.partyName == partyName
                                               && it.gName == gName
                                               && it.sName == sName)
                                .ToList();
            }
            else if (gName == "수영" && (dName.Contains("스프링") || dName.Contains("플랫폼")))
            {
                list = _context.Z_PartyEntries
                                .Where(it => it.partyName == partyName
                                               && it.gName == gName
                                               && it.sName == sName
                                               && (it.dNameOne.Contains("스프링") || it.dNameOne.Contains("플랫폼")
                                                        || it.dNameTwo.Contains("스프링") || it.dNameTwo.Contains("플랫폼")
                                                        || it.dNameThree.Contains("스프링") || it.dNameThree.Contains("플랫폼")
                                                        || it.dNameFour.Contains("스프링") || it.dNameFour.Contains("플랫폼")))
                                .ToList();
            }

            return list;
        }

        // 학교의 소속 시군 찾기
        public string GetCityBySchoolName(string schoolName)
        {
            string? cname = _context.Z_Members
                            .Where(it => it.memberName == schoolName)
                            .Select(it => it.city)
                            .FirstOrDefault();
            if (cname == null) return "포항";
            return cname;
        }
        #endregion

        #region  마라톤
        public List<string> GetDetailsByMarathon(string sname)
        {
            List<string> list = new List<string>();
            if (sname == "초등부")
            {
                list = new List<string> { "제1구간 남초", "제2구간 남초(4,5학년)", "제3구간 여초", "제4구간 남초", "제5구간 여초(4,5학년)", "제6구간 여초", "제7구간 남초", "후보 남초6학년", "후보 여초6학년", "후보 남초4,5학년", "후보 여초4,5학년" };

            }
            else if (sname == "중학부")
            {
                list = new List<string> { "제1구간 여중", "제2구간 남중", "제3구간 여중", "제4구간 남중", "제5구간 여중", "제6구간 남중", "후보 남중3학년", "후보 여중3학년", "후보 남중1,2학년", "후보 여중1,2학년" };

            }

            return list;
        }
        public List<string> GetSchoolNamesBySection(string cname, string sname)
        {
            //using var context = _contextFactory.CreateDbContext();
            List<string?> list = new List<string?>();
            if (sname == null)
            {
                list = _context.Z_Members
                   .Where(it => it.city == cname.Trim().Substring(0, 2))
                   .OrderBy(it => it.Id)
                   .Select(it => it.memberName)
                   .ToList();
            }
            else if (sname.Contains("초등"))
            {
                list = _context.Z_Members
                   .Where(it => it.city == cname.Trim().Substring(0, 2)
                              && !it.memberName.Contains("중학")
                              && !it.memberName.Contains("고등"))
                   .OrderBy(it => it.Id)
                   .Select(it => it.memberName)
                   .ToList();
            }
            else if (sname.Contains("중학"))
            {
                list = _context.Z_Members
                   .Where(it => it.city == cname.Trim().Substring(0, 2)
                              && !it.memberName.Contains("초등")
                              && !it.memberName.Contains("고등"))
                   .OrderBy(it => it.Id)
                   .Select(it => it.memberName)
                   .ToList();
            }
            else if (sname.Contains("고등"))
            {
                list = _context.Z_Members
                   .Where(it => it.city == cname.Trim().Substring(0, 2)
                              && !it.memberName.Contains("초등")
                              && !it.memberName.Contains("중학"))
                   .OrderBy(it => it.Id)
                   .Select(it => it.memberName)
                   .ToList();
            }

            return list;
        }

        #endregion
    }
}
