using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.interfaces
{
    public interface IMediaManager
    {
        public Task UploadMedia(string title, string description, List<string> tags, byte[] Videobytes);
        public string DeleteMedia();
    }
}
