using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OfficeOpenXml;
using System.Security.Claims;

namespace GBES.Pages.Marathon
{
    public partial class MarathonGameResult
    {

        [Inject]
        public NavigationManager? NavigationManagerInjector { get; set; }

        List<string> sNameList = new List<string> { "", "초등부", "중학부", "종합" };

        public bool noDisplay { get; set; } = true;
        public bool primary { get; set; } = false;
        public bool middle { get; set; } = false;
        public bool jonghap { get; set; } = false;

        protected override void OnInitialized()
        {
            NavigationManagerInjector.NavigateTo("/");
        }
        private void SelectSectionName(ChangeEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            string sName = e.Value.ToString();
            if(sName == "")
            {
                InitializeShow();
                noDisplay = true;
            }
            else if (sName == "초등부")
            {
                InitializeShow();
                primary = true;
            }
            else if (sName == "중학부")
            {
                InitializeShow();
                middle= true;
            }
            else if (sName == "종합")
            {
                InitializeShow();
                jonghap = true;
            }
        }

        private void InitializeShow()
        {
            noDisplay = true;
            primary = false;
            middle = false;
            jonghap= false;
        }
    }
}
