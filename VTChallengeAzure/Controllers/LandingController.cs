using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using NugetVTChallenge.Models;
using VTChallengeAzure.Helpers;
using VTChallengeAzure.Services;

namespace VTChallengeAzure.Controllers {
    public class LandingController : Controller {

        private ServiceVTChallenge service;
        private ServiceStorageBlob serviceBlob;
        private HelperBlob helper;
  

        public LandingController(ServiceVTChallenge service, ServiceStorageBlob serviceBlob, HelperBlob helper) {
            this.service = service;
            this.serviceBlob = serviceBlob;
            this.helper = helper;
        }

        public async Task<IActionResult> Index() {
            List<TournamentComplete> tournaments = await this.service.GetAllTournaments();
            foreach(TournamentComplete t in tournaments) {
                t.Image = await this.helper.GetUri(t.Image);
            }

            return View(tournaments);
        }
    }
}
