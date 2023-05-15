using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NugetVTChallenge.Models;
using System.Net.Http.Headers;
using System.Net;
using System.Text;

namespace VTChallengeAzure.Services {
    public class ServiceVTChallenge {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private MediaTypeWithQualityHeaderValue Header;
        private string UrlApi;

        public ServiceVTChallenge(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) {
            this.UrlApi = configuration.GetValue<string>("ApiUrls:ApiVTChallenge");
            this.Header = new MediaTypeWithQualityHeaderValue("application/json");
            _httpContextAccessor = httpContextAccessor;
        }

        #region GENERAL
        private async Task<T> CallApiAsync<T>(string request) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                HttpResponseMessage response = await client.GetAsync(request);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<T>() : default(T);
            }
        }

        private async Task<HttpStatusCode> InsertApiAsync<T>(string request, T objeto) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                string json = JsonConvert.SerializeObject(objeto);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                return response.StatusCode;
            }
        }

        private async Task<HttpStatusCode> UpdateApiAsync<T>(string request, T objeto) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                string json = JsonConvert.SerializeObject(objeto);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(request, content);
                return response.StatusCode;
            }
        }

        // Se supone que en el request ya va el id. Ejemplo: http:/localhost/api/deletealgo/17
        private async Task<HttpStatusCode> DeleteApiAsync(string request) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();

                HttpResponseMessage response = await client.DeleteAsync(request);
                return response.StatusCode;
            }
        }
        #endregion

        #region GENERAL TOKEN
        private async Task<T> CallApiAsync<T>(string request, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                HttpResponseMessage response = await client.GetAsync(request);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<T>() : default(T);
            }
        }

        private async Task<HttpStatusCode> InsertApiAsync<T>(string request, T objeto, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string json = JsonConvert.SerializeObject(objeto);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                return response.StatusCode;
            }
        }

        private async Task<T> InsertApiAsync<T>(string request, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                HttpResponseMessage response = await client.PostAsync(request, null);
                return default(T);
            }
        }

        private async Task<HttpStatusCode> UpdateApiAsync<T>(string request, T objeto, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string json = JsonConvert.SerializeObject(objeto);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(request, content);
                return response.StatusCode;
            }
        }

        // Se supone que en el request ya va el id. Ejemplo: http:/localhost/api/deletealgo/17
        private async Task<HttpStatusCode> DeleteApiAsync(string request, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                HttpResponseMessage response = await client.DeleteAsync(request);
                return response.StatusCode;
            }
        }
        #endregion

        #region TOKEN
        public async Task<string?> GetToken(string user, string pass) {
            LogIn model = new LogIn { Name = user, Password = pass };
            string token = "";

            using (HttpClient client = new HttpClient()) {
                string request = "/api/auth/login";
                client.BaseAddress = new Uri(this.UrlApi + request);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string jsonModel = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonModel, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode) {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(data);
                    token = jsonObject.GetValue("response").ToString();
                }
                return (token != "") ? token : null;
            }
        }
        #endregion

        #region USUARIO
        public async Task<HttpStatusCode> RegisterUser(string name, string tag, string email, string password) {
            Usuario newUser = new Usuario {
                Uid = "",
                Name = name,
                Tag = tag,
                Email = email,
                Password = password,
                PassEncript = Encoding.ASCII.GetBytes(""),
                Salt = "",
                ConfirmPassword = "",
                ImageLarge = "",
                ImageSmall = "",
                Rank = "",
                Rol = ""
            };
            string request = "/api/auth/register";
            return await this.InsertApiAsync<Usuario>(request, newUser);
        }

        public async Task<Usuario> GetPerfil() {
            string request = "/api/usuario/profile";
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.CallApiAsync<Usuario>(request, token);
        }

        public async Task<string> UpdateProfile() {
            string request = "/api/usuario/update";
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.CallApiAsync<string>(request, token);
        }

        public async Task<int> Victories() {
            string request = "/api/usuario/victories";
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.CallApiAsync<int>(request, token);
        }
        #endregion

        #region TOURNAMENT
        public async Task<List<TournamentComplete>> GetAllTournaments() {
            string request = "/api/tournament/getalltournaments";
            return await this.CallApiAsync <List<TournamentComplete>>(request);
        }

        public async Task<TournamentComplete> GetTournament(int tid) {
            string request = "/api/tournament/gettournament/" + tid;
            return await this.CallApiAsync<TournamentComplete>(request);
        }

        public async Task<List<TournamentComplete>> GetMyTournaments() {
            string request = "/api/tournament/GetMyTournaments";
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.CallApiAsync<List<TournamentComplete>>(request,token);
        }

        public async Task<List<TournamentComplete>> GetMyTournamentsFiltereds(string filtro) {
            string request = "/api/tournament/GetMyTournamentsFiltereds/" + filtro;
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.CallApiAsync<List<TournamentComplete>>(request, token);
        }

        public async Task<List<TournamentComplete>> GetTournamentsAvailableRank() {
            string request = "/api/tournament/GetTournamentsAvailableRank";
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.CallApiAsync<List<TournamentComplete>>(request, token);
        }

        public async Task<List<TournamentComplete>> FindTournamentsAvailableRank(string filtro) {
            string request = "/api/tournament/FindTournamentsAvailableRank/" + filtro;
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.CallApiAsync<List<TournamentComplete>>(request, token);
        }

        public async Task<List<TournamentPlayers>> GetPlayers(int tid) {
            string request = "/api/tournament/GetPlayers/" + tid;
            return await this.CallApiAsync<List<TournamentPlayers>>(request);
        }

        public async Task<List<Round>> GetRounds(int tid) {
            string request = "/api/tournament/GetRounds/" + tid;
            return await this.CallApiAsync<List<Round>>(request);
        }

        public async Task<List<MatchRound>> GetMatches(int tid) {
            string request = "/api/tournament/GetMatches/" + tid;
            return await this.CallApiAsync<List<MatchRound>>(request);
        }

        public async Task<List<TournamentPlayers>> GetWinners(int tid) {
            string request = "/api/tournament/GetWinners/" + tid;
            return await this.CallApiAsync<List<TournamentPlayers>>(request);
        }

        public async Task<bool> ValidateInscription(int tid) {
            string request = "/api/tournament/ValidateInscription/" + tid;
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.CallApiAsync<bool>(request, token);
        }

        public async Task<bool> Inscription(int tid) {
            string request = "/api/tournament/Inscription/" + tid;
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.InsertApiAsync<bool>(request, token);
        }

        public async Task<HttpStatusCode> DeleteTournament(int tid) {
            string request = "/api/tournament/DeleteTournament/" + tid;
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.DeleteApiAsync(request, token);
        }

        public async Task<HttpStatusCode> CreateTournament(InsertTournament objects) {
            string request = "/api/tournament/CreateTournament";
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.InsertApiAsync<InsertTournament>(request,objects ,token);
        }

        public async Task<HttpStatusCode> UpdateResultMatches(List<Match> partidas) {
            string request = "/api/tournament/UpdateResultMatches";
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.UpdateApiAsync<List<Match>>(request, partidas ,token);
        }

        public async Task<HttpStatusCode> DeleteUserTournament(int tid, string uid) {
            string request = "/api/tournament/DeleteUserTournament/" + tid + "/" + uid;
            string token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");
            return await this.DeleteApiAsync(request , token);
        }
        #endregion

    }
}
