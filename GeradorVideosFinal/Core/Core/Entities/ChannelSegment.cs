using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Domain.Entities
{
    public class ChannelSegment : Base
    {
        Channel Channel { get; set; }
        Segment Segment { get; set; }
    }
}
