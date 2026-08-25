using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ContentSources.GoogleImageContent
{
    public class GoogleImageContentResponse : IContent
    {
        public int Id { get; set; }
        public string Title { get ; set ; }
        public string Result { get; set; }
    }
}
