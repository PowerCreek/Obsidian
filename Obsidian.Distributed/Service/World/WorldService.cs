using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Distributed.Service.World
{
    public class WorldService
    {
        public virtual async Task<T> GetWorldAsync<T>(string worldId)
        {
            return default;
        }
    }
}
