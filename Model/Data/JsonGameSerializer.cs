using Model.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Data
{
    public class JsonGameSerializer : GameSerializer
    {
        public override string FileExtension => ".json";

        public override void Serialize(Game game, string filePath)
        {
            try
            {
                ValidateGame(game);
                filePath = Path.ChangeExtension(filePath, FileExtension);
                ValidateFilePath(filePath);

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All
                };

                string json = JsonConvert.SerializeObject(game, settings);
                File.WriteAllText(filePath, json);
                LogCallback?.Invoke($"Игра успешно сохранена в {filePath}");
            }
            catch (Exception ex)
            {
                LogCallback?.Invoke($"Ошибка сохранения JSON: {ex.Message}");
                throw new GameSerializationException("Ошибка сохранения игры в JSON", ex);
            }
        }

        public override Game Deserialize(string filePath)
        {
            try
            {
                filePath = Path.ChangeExtension(filePath, FileExtension);
                ValidateFilePath(filePath);

                string json = File.ReadAllText(filePath);
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var game = JsonConvert.DeserializeObject<Game>(json, settings);

                if (game == null)
                    throw new GameSerializationException("Не удалось загрузить игровую сессию");

                LogCallback?.Invoke($"Игра успешно загружена из {filePath}");
                return game;
            }
            catch (Exception ex)
            {
                LogCallback?.Invoke($"Ошибка загрузки JSON: {ex.Message}");
                throw new GameSerializationException("Ошибка загрузки игры из JSON", ex);
            }
        }
    }
}
