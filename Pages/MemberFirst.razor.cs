using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Security.Claims;
using DocumentFormat.OpenXml.InkML;

namespace GBES.Pages
{
    public partial class MemberFirst
    {
        // 인정 가져오기 [1]
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        private ClaimsPrincipal AuthenticationStateProviderUser { get; set; }

        public Z_Member? model = new Z_Member();   // 로그인 한 멤버

        public Z_Member? editModel = new Z_Member();   // 회원 수정을 위한 멤버

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

        List<Z_Member> listMember = new List<Z_Member>();

        string? memberName;

        string? cName = "";

        string? searchText = "";

        bool isMember = false;

        bool isEdit = false;

        List<string?> cNameList = new List<string>();
        List<string?> schoolNameList = new List<string>();
        public string year { get; set; } = DateTime.Now.Year.ToString();

        protected override async Task OnInitializedAsync()
        {
            // 인정 가져오기 [2]
            var authState = await authenticationStateTask;
            var user = authState.User;
            memberName = user.Identity.Name;
            memberName = appState.userName;

            if (memberName == "")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "로그인을 하여야 합니다.");
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            model = await context.Z_Members
                    .Where(it => it.memberName == memberName)
                    .SingleOrDefaultAsync();

            if (model is not null)
            {
                isMember = true;

                StateHasChanged();

                if (string.IsNullOrEmpty(model.name))
                {
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "담당자명을 입력하세요 학교는 지도교사 이름 입력.");
                }
            }
            cNameList = appState.GetCities();
        }



        protected async Task Save_Click()
        {
            string? memPart = "";
            using var context = _contextFactory.CreateDbContext();
            context.Z_Members.Update(model);
            await context.SaveChangesAsync();
            if (model.memberName.Contains("학교")) memPart = "학교";
            else if (model.memberName.Contains("지원청")) memPart = "교육지원청";
            else if (model.memberName.Contains("경북교육청")) memPart = "경북교육청";
            else if (model.memberName.Contains("관리자")) memPart = "경북교육청";
            else if (model.division == "경기단체") memPart = "경기단체";
            appState.SetLogin(model.memberName, memPart, model.city, model.name);
        }

        protected async Task Cancel_Click()
        {
            using var context = _contextFactory.CreateDbContext();
            model = await context.Z_Members
                    .Where(it => it.memberName == memberName)
                    .SingleOrDefaultAsync();
            StateHasChanged();
        }

        // 리스트  시군 선택시
        public void SelectCity(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            cName = e.Value.ToString();
           
            using var context = _contextFactory.CreateDbContext();
            listMember = context.Z_Members
                .Where(it => it.city == cName)
                .ToList();

            schoolNameList = context.Z_Members
                .Where(it => it.city == cName)
                .Select(it=>it.memberName)
                .ToList();
            StateHasChanged();
        }

        // 리스트  학교 선택시
        public void SelectSchoolName(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            string schoolName = e.Value.ToString();

            using var context = _contextFactory.CreateDbContext();

            if (schoolName != "학교선택")
            {
                listMember = context.Z_Members
                    .Where(it => it.memberName == schoolName)
                    .ToList();
            }
            else
            {
                listMember = context.Z_Members
                   .Where(it => it.city == cName)
                   .ToList();
            }
            StateHasChanged();
        }

        // 리스트 회원 수정 클릭
        public void MemberEdit(Z_Member editmember)
        {
            editModel = editmember;

            isEdit = true;
        }

        // 리스트 회원 수정 저장
        public void MemberEditSave(Z_Member editmember)
        {
            using var context = _contextFactory.CreateDbContext();

            context.Z_Members.Update(editmember);
            context.SaveChanges();

            // 리스트에 수정 후 새로고침
            foreach (var item in listMember.Where(w => w.Id == editmember.Id))
            {
                item.division= editmember.division;
                item.city = editmember.city;
                item.memberName= editmember.memberName;
                item.logID = editmember.logID;
                item.logPW = editmember.logPW;
                item.phone = editmember.phone;
                item.etc = editmember.etc;
            }

            Cancel();

            StateHasChanged();
        }

        // 리스트 회원 수정 취소
        public void Cancel()
        {
            editModel = new Z_Member();

            isEdit = false;
        }

        private async Task FindMembger()
        {
            if (searchText == "")
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "찾는 학교명 혹은 단체면을 입력하세요.");
            }
            else
            {
                using var context = _contextFactory.CreateDbContext();
                listMember = context.Z_Members
                    .Where(it => it.memberName.Contains(searchText))
                    .ToList();
            }
        }

        private bool isUse;
        private bool isEnd;
        private string? saveResult;

        private void Save()
        {
            // 체크 여부 확인
            saveResult = $"옵션 1: {(isUse ? "체크됨" : "체크 안 됨")}, 옵션 2: {(isEnd ? "체크됨" : "체크 안 됨")}";
            // 실제 저장 로직(예: DB 저장 등)은 여기에 추가


        }

        private void OnUseChanged(ChangeEventArgs e)
        {
            bool checkedValue = (bool)e.Value;
            isUse = checkedValue;
            if (checkedValue)
            {
                isEnd = false;
            }
        }

        private void OnEndChanged(ChangeEventArgs e)
        {
            bool checkedValue = (bool)e.Value;
            isEnd = checkedValue;
            if (checkedValue)
            {
                isUse = false;
            }
        }

    }
}
