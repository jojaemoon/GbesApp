using DocumentFormat.OpenXml.Bibliography;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GBES.Pages.PhysicalKing
{
    public partial class PhysicalPersonResult
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

        public string partyName { get; set; }
        public string loginID { get; set; }
        public string passWord { get; set; }
        public string newPassword { get; set; }
        public bool memberPass { get; set; } = false;

        Z_PhysicalKingRecord? memberRecord = new Z_PhysicalKingRecord();

        protected override void OnInitialized()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;

            //JSRuntimeInjector.InvokeVoidAsync("alert", "마감되었습니다.");
            //NavigationManagerInjector.NavigateTo("/");

            using var context = _contextFactory.CreateDbContext();
            Z_PartyName? pName = context.Z_PartyNames
                        .Where(it => it.partyName.Contains("체력")
                                    && (it.etc == "사용" || it.etc == "마감" || it.etc == "열람") || it.etc == "진행")
                        .FirstOrDefault();
            partyName = pName.partyName;
        }

        private async Task ChangePass()
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            bool confirmed = await JSRuntimeInjector.InvokeAsync<bool>("confirm", "비밀번호를 '" + newPassword + "' 로 바꿀까요?");

            if (confirmed)
            {
                memberRecord.passWord = newPassword;
                context.Update(memberRecord);
                context.SaveChanges();
                await JSRuntimeInjector.InvokeVoidAsync("alert", "비밀번호를 변경하였습니다");
                passWord = newPassword;
                newPassword = string.Empty;
            }
        }

        private async Task ConfirmMember()
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                memberRecord = await context.Z_PhysicalKingRecords
                               .Where(it => it.partyName == partyName
                                            && it.numbering.Trim() == loginID
                                            && it.passWord.Trim() == passWord)
                                .FirstOrDefaultAsync();
                if (memberRecord != null)
                {
                    memberPass = true;
                    await JSRuntimeInjector.InvokeVoidAsync("alert", memberRecord.name + "님 반가워요!");
                }
                else
                {
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "배번 혹은 비밀번호가 틀립니다");
                }
            }
            catch (Exception e)
            {
                await JSRuntimeInjector.InvokeVoidAsync("alert", e.Message);
            }
        }

        private string TimeToHangul(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "분 초";
            }
            string[] arr = str.Split('.');
            if (arr.Length == 1)
            {
                str += "초";
            }
            else if (arr.Length == 2)
            {
                str = arr[0] + "분" + arr[1] + "초";
            }
            else
            {
                str = "";
            }
            return str;
        }

    }
}
