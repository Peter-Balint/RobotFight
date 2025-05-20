using RobotFight.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RobotFight.Persistence
{
    public class RobotFightFileAccess : IRobotFightDataAccess
    {
        public async Task<PlayerData[]> LoadAsync(string path)
        {
            return await LoadAsync(File.OpenRead(path));
        }

        public async Task<PlayerData[]> LoadAsync(Stream stream)
        {
            try
            {
                using(StreamReader reader = new StreamReader(stream))
                {
                    PlayerData[] data = new PlayerData[2];
                    for (int i = 0; i < data.Length; i++) {
                        string line = await reader.ReadLineAsync() ?? throw new Exception();
                        string[] numbers = line.Split(' ');
                        int x = int.Parse(numbers[0]);
                        int y = int.Parse(numbers[1]);
                        int health = int.Parse(numbers[2]);
                        int boardSize = int.Parse(numbers[3]);

                        Orientation orientation;
                        line = await reader.ReadLineAsync() ?? throw new Exception();
                        switch (line)
                        {
                            case "Up":
                                {
                                    orientation = Orientation.Up;
                                    break;
                                }
                            case "Right":
                                {
                                    orientation = Orientation.Right;
                                    break;
                                }
                            case "Down":
                                {
                                    orientation = Orientation.Down;
                                    break;
                                }
                            case "Left":
                                {
                                    orientation = Orientation.Left;
                                    break;
                                }
                            default: throw new Exception();
                        }
                        data[i] = new PlayerData(x, y, health, boardSize,orientation);
                    }
                    return data;
                }
            }
            catch(Exception) { throw new Exception(); }
        }

        public async Task SaveAsync(string path, PlayerData[] pd)
        {
            await SaveAsync(File.OpenWrite(path), pd);
        }

        public async Task SaveAsync(Stream stream, PlayerData[] pd)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    for (int i = 0; i < pd.Length; i++)
                    {
                        await writer.WriteLineAsync($"{pd[i].X} {pd[i].Y} {pd[i].Health} {pd[i].BoardSize}");
                        switch (pd[i].Orientation)
                        {
                            case Orientation.Up:
                                {
                                    await writer.WriteLineAsync("Up");
                                    break;
                                }
                            case Orientation.Right:
                                {
                                    await writer.WriteLineAsync("Right");
                                    break;
                                }
                            case Orientation.Down:
                                {
                                    await writer.WriteLineAsync("Down");
                                    break;
                                }
                            case Orientation.Left:
                                {
                                    await writer.WriteLineAsync("Left");
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception) { throw new Exception(); }
        }
    }
}
