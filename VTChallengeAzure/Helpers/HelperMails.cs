using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;

namespace VTChallengeAzure.Helpers {
    public class HelperMails {

        private MediaTypeWithQualityHeaderValue Header;

        public HelperMails() {
            this.Header = new MediaTypeWithQualityHeaderValue("application/json");
        }


        public async Task SendMailAsync (string email, string asunto, string mensaje) {
            string urlEmail = "https://prod-173.westeurope.logic.azure.com:443/workflows/d130204b5ea348329acb16aff4da8f73/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=P_VKuP8C4LKw2kRjvj90wdU4I_WYv9VkP2NXfOnYqNs";
            var model = new {
                email = email,
                asunto = asunto,
                mensaje = mensaje
            };
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                string json = JsonConvert.SerializeObject(model);
                StringContent content =
                    new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync(urlEmail, content);
            }
        }

        public string PlantillaInscriptionPlayer(string name, string nombreTorneo, string fecha, string server, string organizador) {
            string html = "<p>Estimado" + name + ",</p>" +
                           "<p>¡Gracias por unirse a nuestro torneo! Nos complace informarle que su registro ha sido confirmado y ahora forma parte del grupo de participantes del torneo <b>" + nombreTorneo + "</b></p>" +
                            "<p>El torneo se llevará a cabo el día" + fecha + " y tendrá lugar en el siguiente link: " + server + " .Por favor, asegúrese de estar al menos 30 minutos antes de que comience para completar el proceso de registro.</p>" +
                            "<p>Le deseamos la mejor de las suertes en el torneo y esperamos que tenga una experiencia emocionante y satisfactoria. Si tiene alguna pregunta o inquietud, no dude en ponerse en contacto con nosotros.</p>" +
                            "<p>¡Gracias de nuevo por unirse a nosotros!</p>" +
                            "<p>Atentamente,<br>" + organizador + "</p>";
            return html;
        }

        public string PlantillaInscriptionOrg(string playerName, int inscripciones) {
            string html = "<p>El jugador: " + playerName + " se ha unido al torneo</p>" +
                          "<p>Quedan " + inscripciones + " inscripciones</p>";
            return html;
        }

        public string PlantillaRemoveUserTournament() {
            string html = "Por decisión del organizador fuiste expulsado";
            return html;
        }
    }
}
