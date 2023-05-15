using Microsoft.AspNetCore.Mvc;
using NugetVTChallenge.Models;
using VTChallengeAzure.Services;

namespace VTChallengeAzure.Controllers {
    public class LandingController : Controller {

        private ServiceVTChallenge service;

        public LandingController(ServiceVTChallenge service) {
            this.service = service;
        }

        public async Task<IActionResult> Index() {
            List<TournamentComplete> tournaments = await this.service.GetAllTournaments();
            return View(tournaments);
        }
    }
}
