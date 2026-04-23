using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.interfaces;
using Google.Apis.Auth.OAuth2;


namespace Services.Factories
{
    public class YoutubeManagerFactory : IMediaManager
    {
        public static async Task<YoutubeManager> CreateYoutubeManagerInstance()
        {
            UserCredential userCredential;

            using (var stream = new FileStream("C:\\Users\\Administrador\\Desktop\\GeradorVideosFinal\\Core\\Services\\client_secret_104744707615-tac6povnpknvr1mfh5a7q81mild4bceb.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
            {
                userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { "https://www.googleapis.com/auth/youtube.upload" },
                    "user",
                    CancellationToken.None
                );
            }

            return new YoutubeManager(userCredential);
        }

        public string DeleteMedia()
        {
            throw new NotImplementedException();
        }

        public Task UploadMedia(string title, string description, List<string> tags, byte[] Videobytes)
        {
            throw new NotImplementedException();
        }
    }
}
