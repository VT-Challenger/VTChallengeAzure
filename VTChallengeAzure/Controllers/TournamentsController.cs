using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NugetVTChallenge.Models;
using System.Net;
using System.Security.Claims;
using VTChallengeAzure.Filters;
using VTChallengeAzure.Services;

namespace VTChallenge.Controllers {
    public class TournamentsController : Controller {

        private ServiceVTChallenge service;

        public TournamentsController(ServiceVTChallenge service) {
            this.service = service;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> ListTournaments() {
            ViewData["LISTTOURNAMENTS"] = await this.service.GetTournamentsAvailableRank();
            return View();
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> ListTournaments(string filtro) {
            if (filtro == null) {
                ViewData["LISTTOURNAMENTS"] = await this.service.GetTournamentsAvailableRank();
            } else {
                ViewData["LISTTOURNAMENTS"] = await this.service.FindTournamentsAvailableRank(filtro);
            }
            return View();
        }

        [AuthorizeUsers]
        public async Task<IActionResult> TournamentDetails(int tid) {

            ViewData["TOURNAMENT"] = await this.service.GetTournament(tid);
            ViewData["PLAYERSTOURNAMENT"] = await this.service.GetPlayers(tid);
            ViewData["ROUNDSNAME"] = await this.service.GetRounds(tid);
            ViewData["MATCHESTOURNAMENT"] = await this.service.GetMatches(tid);
            ViewData["TOURNAMENTWINNER"] = await this.service.GetWinners(tid);
            ViewData["VALIDATEINSCRIPTION"] = await this.service.ValidateInscription(tid);
            return View();
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> InscriptionPlayer(int tid) {
            bool res = await this.service.Inscription(tid);

            if (res) {
                //ENVIAR CORREO CON AZURE
                return RedirectToAction("TournamentDetails", "Tournaments", new { tid = tid });
            } else {
                return RedirectToAction("TournamentDetails", "Tournaments", new { tid = tid });
            }
            
        }

        [AuthorizeUsers]
        public async Task<IActionResult> ListTournamentsUser() {
            ViewData["LISTTOURNAMENTSUSER"] = await this.service.GetMyTournaments();
            return View();
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> ListTournamentsUser(string filtro) {
            if (filtro == null) {
                ViewData["LISTTOURNAMENTSUSER"] = await this.service.GetMyTournaments();
            } else {
                ViewData["LISTTOURNAMENTSUSER"] = await this.service.GetMyTournamentsFiltereds(filtro);
            }

            return View();
        }

        [AuthorizeUsers]
        public async Task<IActionResult> DeleteTournament(int tid) {
            HttpStatusCode res = await this.service.DeleteTournament(tid);
            if (res == HttpStatusCode.OK) {
                return RedirectToAction("ListTournamentsUser", "Tournaments");
            } else {
                return View();
            }
        }

        [AuthorizeUsers]
        public IActionResult CreateTournament() { return View(); }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> CreateTournament(string jsonTournament, string jsonRounds, string jsonMatches, IFormFile imageTournament) {

            InsertTournament obj = new InsertTournament {
                JsonTournament = JsonConvert.DeserializeObject<Tournament>(jsonTournament),
                JsonRounds = JsonConvert.DeserializeObject<List<Round>>(jsonRounds),
                JsonMatches = JsonConvert.DeserializeObject<List<Match>>(jsonMatches)
            };

            HttpStatusCode res = await this.service.CreateTournament(obj);

            if (res == HttpStatusCode.OK) {
                return RedirectToAction("ListTournamentsUser", "Tournaments");
            } else {
                return View();
            }
            
        }

        [AuthorizeUsers]
        public async Task<IActionResult> EditTournament(int tid) {
            TournamentComplete tournamentComplete = await this.service.GetTournament(tid);
            ViewData["PLAYERSTOURNAMENT"] = await this.service.GetPlayers(tid);
            ViewData["ROUNDSNAME"] = await this.service.GetRounds(tid);
            ViewData["MATCHESTOURNAMENT"] = await this.service.GetMatches(tid);
            return View(tournamentComplete);
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> UpdateUserTournament(int tid, string data) {
            List<Match> partidas = JsonConvert.DeserializeObject<List<Match>>(data);
            await this.service.UpdateResultMatches(partidas);
            return RedirectToAction("EditTournament", "Tournaments", new { tid = tid });
        }

        [AuthorizeUsers]
        public async Task<IActionResult> DeleteUserTournament(int tid, string uid) {
            await this.service.DeleteUserTournament(tid, uid);
            //EMAIL
            //Users player = await this.repo.FindUserAsync(uid);
            //string contenido = this.helperMails.PlantillaRemoveUserTournament();
            //await this.helperMails.SendMailAsync(player.Email, "HAS SIDO EXPULSADO DEL TORNEO", contenido);
            return RedirectToAction("EditTournament", "Tournaments", new { tid = tid });
        }

   
    }
}
