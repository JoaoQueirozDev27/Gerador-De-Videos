using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public record ContentSourceKey(
    int? GlobalId,
    int? SceneId,
    int? LayerId)
    {
        public bool IsValid()
        {
            if (GlobalId == null && SceneId == null)
                return false;

            if (LayerId != null && SceneId == null)
                return false;

            return true;
        }
    }
}
