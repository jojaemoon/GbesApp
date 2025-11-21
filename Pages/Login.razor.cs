using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages
{
    public partial class Login
    {
        public string Username { get; set; }
        public string Password { get; set; }

        [Inject]
        public AppState appState { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IDbContextFactory<GBESportsAppDbContext>? _contextFactory { get; set; }

        [Inject]
        public IJSRuntime? JSRuntimeInjector { get; set; }
        public async Task Login_Clicked()
        {
            using var context = _contextFactory.CreateDbContext();
            Z_Member? member=context.Z_Members
                             .Where(it => it.logID == Username)
                             .SingleOrDefault();
            if (member == null)
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "학교명(소속명)이 틀립니다.");
                // NavigationManager.NavigateTo("/Login");
            }
            else
            {
                if (member.logPW != Password)
                {
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "비밀번호가 틀립니다.");
                }
                else
                {
                    string memPart = "";
                    if (member.memberName.Contains("학교")) memPart = "학교";
                    else if (member.memberName.Contains("지원청")) memPart = "교육지원청";
                    else if (member.memberName.Contains("경북교육청")) memPart = "경북교육청";
                    else if (member.memberName.Contains("관리자")) memPart = "경북교육청";
                    else if(member.memberName.Contains("입력")) memPart = "입력";
                    else if (member.division == "경기단체") memPart = "경기단체";

                    appState.SetLogin(member.memberName, memPart, member.city, member.name);

                    await JSRuntimeInjector.InvokeVoidAsync("loginPass", member.memberName, memPart);

                    if(memPart == "입력")
                    {
                        NavigationManager.NavigateTo("/PhysicalResult");
                    }
                    else
                        NavigationManager.NavigateTo("/MemberFirst");
                }
            }
        }

        private void cancel()
        {
            NavigationManager.NavigateTo("/");
        }
    }
}
