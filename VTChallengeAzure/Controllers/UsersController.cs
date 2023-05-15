using Microsoft.AspNetCore.Mvc;
using NugetVTChallenge.Models;
using System.Security.Claims;
using VTChallengeAzure.Filters;
using VTChallengeAzure.Services;

namespace VTChallenge.Controllers {
    public class UsersController : Controller {

        private ServiceVTChallenge serivce;

        public UsersController(ServiceVTChallenge service) {
            this.serivce = service;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> ProfileUser() {
            ViewData["TORNEOSGANADOS"] = await this.serivce.Victories();
            return View();
        }

        [AuthorizeUsers]
        public IActionResult Trayectoria() {
            return View();
        }

        [AuthorizeUsers]
        public async Task<IActionResult> UpdateProfile() {
            await this.serivce.UpdateProfile();
            return RedirectToAction("LogOut", "Managed");
        }
    }
}
