using GBES.Managers;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace GBES.Pages.BoyFestival
{
    public partial class BoyIntro
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

        [Inject]
        public IFileStorageManager FileStorageManagerReference { get; set; }     // 파일 업로드 다운로드 관련
        #endregion

        string? memberName;
        string? memberPart;
        string? memberCity;

        bool isRuleShow = false;

        public string year { get; set; } = DateTime.Now.Year.ToString();

        #endregion



        #region  파일 업로드 관련
        private CancellationTokenSource cancelation;
        private bool displayProgress;
        private EditContext editContext;
        private Person person;
        private int progressPercent;


        protected override void OnInitialized()
        {
            memberName = appState.userName;
            memberPart = appState.userPart;
            memberCity = appState.userCity;

            cancelation = new CancellationTokenSource();
            person = new Person();
            editContext = new EditContext(person);
        }

        [Inject]
        private IWebHostEnvironment env { get; set; }

        private IList<string> imageDataUrls = new List<string>();
        private int Total;
        private async Task OnChange(InputFileChangeEventArgs e)
        {
            person.Picture = e.GetMultipleFiles().ToArray();

            var format = "image/png";
            Total = e.GetMultipleFiles().Count();
            foreach (var imageFile in e.GetMultipleFiles())
            {
                var resizedImageFile = await imageFile.RequestImageFileAsync(format, 100, 100);
                var buffer = new byte[resizedImageFile.Size];
                await resizedImageFile.OpenReadStream().ReadAsync(buffer);
                var imageDataUrl = $"data:{format};base64,{Convert.ToBase64String(buffer)}";
                imageDataUrls.Add(imageDataUrl);
            }
            editContext.NotifyFieldChanged(FieldIdentifier.Create(() => person.Picture));
        }

        private async Task OnSubmit()
        {
            for (int i = 0; i < Total; i++)
            {
                //string  filename = year + "_" + person.Picture[i].Name;
                string[] arr = person.Picture[i].Name.Split('.');
                string filename = year + "_소년체육대회요강." + arr[1];
                var path = $"{env.WebRootPath}\\Upload\\{filename}";
                using var file = File.OpenWrite(path);
                using var stream = person.Picture[i].OpenReadStream(968435456);

                var buffer = new byte[4 * 1096];
                int bytesRead;
                double totalRead = 0;

                displayProgress = true;

                while ((bytesRead = await stream.ReadAsync(buffer, cancelation.Token)) != 0)
                {
                    totalRead += bytesRead;
                    await file.WriteAsync(buffer, cancelation.Token);

                    progressPercent = (int)((totalRead / person.Picture[i].Size) * 100);
                    StateHasChanged();
                }

                displayProgress = false;
            }

            await JSRuntimeInjector.InvokeVoidAsync("alert", "파일을 저장하였습니다");
        }

        public void Dispose()
        {
            cancelation.Cancel();
        }

        public class Person
        {
            public string Name { get; set; }

            public IBrowserFile[] Picture { get; set; }
        }
        #endregion

        #region   파일 다운로드
        // 참가신청요강 다운로드
        public async Task DownLoad()
        {
            var fileName = year + "년 경북소년체육대회 참가요강.pdf";

            if (!string.IsNullOrEmpty(fileName))
            {
                byte[] fileBytes = await FileStorageManagerReference.DownloadAsync(fileName, "");
                if (fileBytes != null)
                {
                    await FileUtil.SaveAs(JSRuntimeInjector, fileName, fileBytes);
                }
                else
                {
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "자료가 없습니다");
                }
            }
        }
        #endregion

        private void ShowRule()
        {
            isRuleShow = !isRuleShow;
        }
    }
}
