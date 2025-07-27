using GBES.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Security.Claims;
using GBES.Managers;

namespace GBES.Pages
{
    public partial class IndexTwo
    {
        List<Z_Event> _events;

        [Inject]
        public IDbContextFactory<GBESportsAppDbContext>? _contextFactory { get; set; }
        [Inject]
        public IFileStorageManager FileStorageManagerReference { get; set; }     // 파일 업로드 다운로드 관련
        [Inject]
        public IJSRuntime? JSRuntimeInjector { get; set; }

        //protected override void OnInitialized()
        //{
        //    //using var context = _contextFactory.CreateDbContext();

        //    //_events = context.Z_Events.ToList();  

        //    //List<Z_Member> query = context.Z_Members.ToList();
        //    //foreach (var item in query)
        //    //{
        //    //    int m = 0;
        //    //    m++;
        //    //}

        //}

        int imgSu = 1;

        protected override void OnInitialized()
        {
            Random rand = new Random();

            imgSu = rand.Next(1, 4);
        }


        private async Task MarathonResultDown()
        {
            string year = DateTime.Now.Year.ToString();
            var fileName = year + "_구간마라톤결과.xlsx";

            if (!string.IsNullOrEmpty(fileName))
            {
                byte[] fileBytes = await FileStorageManagerReference.DownloadAsync(fileName, "");
                if (fileBytes != null)
                {
                    await FileUtil.SaveAs(JSRuntimeInjector, fileName, fileBytes);
                }
            }
        }
    }
}
