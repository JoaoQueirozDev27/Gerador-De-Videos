using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Domain.Entities
{
    public class Video : Base
    {
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime UploadDate { get; set; }
        public int QuantityViews { get; set; }
        public int QuantityLikes { get; set; }
        public int QuantityDisLikes { get; set; }
        public List<string> Tags { get; set; }
    }
}
