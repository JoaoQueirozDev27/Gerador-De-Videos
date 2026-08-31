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
        /*
        public static bool TryCreateNewInstance(int?[] Ids, out ContentSourceKey ContentSourceKey)
        {
            for (int i = 0; i < 3; i++) {
                if (Ids[i] < 0)
                {
                    ContentSourceKey = new ContentSourceKey(null,null,null);
                    return false;
                }

            }
        }
        */
    }
}
