using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;
using Core.Domain.Entities;

namespace Core.Domain.Entities
{
    public class Channel : Base
    {

        [Required]
        public string Description { get; set; }

        [Required]
        public ChannelSegment ChannelSegment { get; set; }

        public string Nation{ get; set; }

        public List<EnumLanguages>  Language { get; set; } = new List<EnumLanguages>();

        public int QuantityOfSubscribers { get; set; }

        public List<Video>? Videos { get; set; }

        public Channel? ParentChannel { get; set; }

        public List<Channel>? ChildChannels { get; set; }

    }
}
