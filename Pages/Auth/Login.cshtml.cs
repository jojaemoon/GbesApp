using GBES.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace GBES.Pages.Auth
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly GBESportsAppDbContext _context;

        public LoginModel(GBESportsAppDbContext context)
        {
            this._context = context;
        }

        #region Injectors
        [Inject]
        public NavigationManager? NavigationManagerInjector { get; set; }
        [Inject]
        public IJSRuntime? JSRuntimeInjector { get; set; }
        [Inject]
        public IDbContextFactory<GBESportsAppDbContext>? _contextFactory { get; set; }
        #endregion

        public async Task<IActionResult> OnGetAsync(string paramUsername, string paramPassword)
        {
            string returnUrl = "/MemberInfo";

            try
            {
                // Clear the existing external cookie
                await HttpContext
                    .SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch { }

            //var member = await (from p in _context.Z_Schools
            //                    where p.name == paramUsername
            //                    select p).SingleOrDefaultAsync();


            //List<Z_Event>? _events= _context.Z_Events.ToList();
            // List<Z_Member>? _events = _context.Z_Members.ToList();

            Z_Member? member = await _context.Z_Members
                             .Where(it => it.logID == paramUsername)
                             .SingleOrDefaultAsync();

            if (member == null) {
                await JSRuntimeInjector.InvokeVoidAsync("alert", "학교명(소속명)이 틀립니다.");
                return BadRequest("아이디가 틀림");
                //return View("/Auth/loginControl");
            }
            else
            {
                if (member.logPW != paramPassword)
                {
                    await JSRuntimeInjector.InvokeVoidAsync("alert", "비밀번호가 틀립니다.");
                    return BadRequest("비밀번호가 틀림");
                }
            }

            string memPart = "";
            if (member.memberName.Contains("학교")) memPart = "학교";
            else if (member.memberName.Contains("지원청")) memPart = "교육지원청";
            else if (member.memberName.Contains("경북교욱청")) memPart = "경북교육청";
            else if (member.memberName.Contains("관리자")) memPart = "경북교육청";
            else if (member.division == "경기단체") memPart = "경기단체";

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, member.memberName),
                new(ClaimTypes.Role, memPart),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = this.Request.Host.Value
            };

            try
            {
                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                await JSRuntimeInjector.InvokeAsync<object>("alert", error);
            }

            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
            //    new ClaimsPrincipal(claimsIdentity), authProperties);

            return LocalRedirect(Url.Content("/MemberInfo"));
        }
    }
}
