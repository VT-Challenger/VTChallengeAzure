using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NugetVTChallenge.Models;
using System.Net;
using System.Security.Claims;
using VTChallengeAzure.Filters;
using VTChallengeAzure.Helpers;
using VTChallengeAzure.Services;

namespace VTChallenge.Controllers {
    public class TournamentsController : Controller {

        private ServiceVTChallenge service;
        private ServiceStorageBlob serviceBlob;
        private HelperBlob helper;
        private HelperMails helperMails;


        public TournamentsController(ServiceVTChallenge service, ServiceStorageBlob serviceBlob, HelperBlob helper, HelperMails helperMails) {
            this.service = service;
            this.serviceBlob = serviceBlob;
            this.helper = helper;
            this.helperMails = helperMails;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> ListTournaments() {
            List<TournamentComplete> tournaments = await this.service.GetTournamentsAvailableRank();
            tournaments.ForEach(async t => t.Image = await this.helper.GetUri(t.Image));
            ViewData["LISTTOURNAMENTS"] = tournaments;
            return View();
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> ListTournaments(string filtro) {
            List<TournamentComplete> tournaments;
            if (filtro == null) {
                tournaments = await this.service.GetTournamentsAvailableRank();
                tournaments.ForEach(async t => t.Image = await this.helper.GetUri(t.Image));
                ViewData["LISTTOURNAMENTS"] = tournaments;
            } else {
                tournaments = await this.service.FindTournamentsAvailableRank(filtro);
                tournaments.ForEach(async t => t.Image = await this.helper.GetUri(t.Image));
                ViewData["LISTTOURNAMENTS"] = tournaments;
            }
            return View();
        }

        [AuthorizeUsers]
        public async Task<IActionResult> TournamentDetails(int tid) {
            TournamentComplete tournament = await this.service.GetTournament(tid);
            tournament.Image = await this.helper.GetUri(tournament.Image);

            ViewData["TOURNAMENT"] = tournament;
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

            if (!res) {
                //ENVIAR CORREO CON AZURE
                //DATA CORREOS
                TournamentComplete tournament = await this.service.GetTournament(tid);
                string user = HttpContext.User.Identity.Name;
                int espacios = tournament.LimitPlayers - tournament.Inscriptions;
                string contenidoOrg = this.helperMails.PlantillaInscriptionOrg(user, espacios);
                string contenidoPlayer = this.helperMails.PlantillaInscriptionPlayer(user, tournament.Name, tournament.DateInit.ToString(), tournament.Platform, tournament.Organizator);

                //CORREO AL USUARIO
                await this.helperMails.SendMailAsync(HttpContext.User.FindFirst("EMAIL").Value.ToString(), "INSCRIPCION", contenidoPlayer);

                //CORREO AL ORGANIZADOR (PONER CORREO DE ORGANIZADOR)
                //Users organizador = await this.repo.FindUserByNameAsync(tournament.Organizator);
                //await this.helperMails.SendMailAsync(organizador.Email, "INSCRIPCION", contenidoOrg);

                return RedirectToAction("TournamentDetails", "Tournaments", new { tid = tid });
            } else {
                return RedirectToAction("TournamentDetails", "Tournaments", new { tid = tid });
            }
            
        }

        [AuthorizeUsers]
        public async Task<IActionResult> ListTournamentsUser() {
            List<TournamentComplete> tournaments = await this.service.GetMyTournaments();
            tournaments.ForEach(async t => t.Image = await this.helper.GetUri(t.Image));
            ViewData["LISTTOURNAMENTSUSER"] = tournaments;
            return View();
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> ListTournamentsUser(string filtro) {
            List<TournamentComplete> tournaments;
            if (filtro == null) {
                tournaments = await this.service.GetMyTournaments();
                tournaments.ForEach(async t => t.Image = await this.helper.GetUri(t.Image));
                ViewData["LISTTOURNAMENTSUSER"] = tournaments;
            } else {
                tournaments = await this.service.GetMyTournamentsFiltereds(filtro);
                tournaments.ForEach(async t => t.Image = await this.helper.GetUri(t.Image));
                ViewData["LISTTOURNAMENTSUSER"] = tournaments;
            }

            return View();
        }

        [AuthorizeUsers]
        [HttpDelete]
        public async Task<IActionResult> DeleteTournament(int tid) {
            TournamentComplete t = await this.service.GetTournament(tid);
            await this.serviceBlob.DeleteBlobAsync("usuariosblobs", t.Image);
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

            string blobName = "vtchallenge" + HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value + "-" + obj.JsonTournament.Name.ToString().Replace(" ", "-") + ".jpg";
            obj.JsonTournament.Image = blobName;
            using (Stream stream = imageTournament.OpenReadStream()) {
                await this.serviceBlob.UploadBlobAsync("usuariosblobs", blobName, stream);
            }

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
            tournamentComplete.Image = await this.helper.GetUri(tournamentComplete.Image);
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
