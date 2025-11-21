using Application.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.IO;

namespace Services
{
    public class YoutubeManager : IMediaManager
    {
        private YouTubeService _ytService;

        public YoutubeManager(UserCredential userCredential) {

            _ytService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApiKey = "AIzaSyClI3psvxH04l5fVcmRFymcw5zEDKIpdYg",
                ApplicationName = this.GetType().ToString()
            });
        }

        public string DeleteMedia()
        {
            return "Media deleted successfully.";
        }

        public async Task UploadMedia(string title,string description,List<string> tags, byte[] Videobytes)
        {
            var video = new Video();
            
            video.Snippet = new VideoSnippet()
            {
                Title = title,
                Description = description,
                Tags = tags,
                CategoryId = "24"
            };

            video.Status = new VideoStatus()
            {
                PrivacyStatus = "public"
            };

            using (var MemoryStream = new MemoryStream(Videobytes))
            {
                var videosInsertRequest = _ytService.Videos.Insert(video, "snippet,status", MemoryStream, "video/*");
                MemoryStream.Position = 0;
                await videosInsertRequest.UploadAsync();
            }
        }
    }
}
