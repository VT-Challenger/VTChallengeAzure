using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NugetVTChallenge.Models;
using System.Net;
using System.Security.Claims;
using System.Xml.Linq;
using VTChallengeAzure.Services;

namespace VTChallenge.Controllers {
    public class ManagedController : Controller {

        private ServiceVTChallenge service;

        public ManagedController(ServiceVTChallenge service) {
            this.service = service;
        }

        public IActionResult Login() {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password) {
            string? token = await this.service.GetToken(username, password);

            if(token == null) {
                ViewData["MENSAJE"] = "Usuario/Password incorrectos";
                return View();
            } else {
                HttpContext.Session.SetString("TOKEN", token);

                Usuario user = await this.service.GetPerfil();
                ClaimsIdentity identity = new ClaimsIdentity(
                  CookieAuthenticationDefaults.AuthenticationScheme,
                  ClaimTypes.Name,
                  ClaimTypes.Role
               );

                Claim claimName = new Claim(ClaimTypes.Name, username);
                Claim claimId = new Claim(ClaimTypes.NameIdentifier, user.Uid.ToString());
                Claim claimRole = new Claim(ClaimTypes.Role, user.Rol);
                Claim claimRank = new Claim("RANGO", user.Rank);
                Claim claimTag = new Claim("TAG", user.Tag);
                Claim claimEmail = new Claim("EMAIL", user.Email);
                Claim claimImageLarge = new Claim("IMAGELARGE", user.ImageLarge);
                Claim claimImageSmall = new Claim("IMAGESMALL", user.ImageSmall);

                identity.AddClaim(claimName);
                identity.AddClaim(claimId);
                identity.AddClaim(claimRole);
                identity.AddClaim(claimRank);
                identity.AddClaim(claimTag);
                identity.AddClaim(claimEmail);
                identity.AddClaim(claimImageLarge);
                identity.AddClaim(claimImageSmall);

                ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);

                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Register() {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Register(Usuario user) {
            HttpStatusCode res = await this.service.RegisterUser(user.Name, user.Tag, user.Email, user.Password);
            if(res == HttpStatusCode.OK) {
                return RedirectToAction("Login");
            } else {
                return View();
            }
        }

        public IActionResult AccesoDenegado() {
            return View();
        }

        public async Task<IActionResult> LogOut() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("TOKEN");
            return RedirectToAction("Index", "Landing");
        }
    }
}
