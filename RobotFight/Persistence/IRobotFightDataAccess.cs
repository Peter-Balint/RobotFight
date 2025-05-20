using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotFight.Persistence
{
    public interface IRobotFightDataAccess
    {
        Task<PlayerData[]> LoadAsync(string path);

        Task<PlayerData[]> LoadAsync(Stream stream);

        Task SaveAsync(string Path,PlayerData[] data);

        Task SaveAsync(Stream stream, PlayerData[] data);
    }
}
