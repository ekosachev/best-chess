using Model.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Model.Data
{
    public class XmlGameSerializer : GameSerializer
    {
        public override string FileExtension => ".xml";

        public override void Serialize(Game game, string filePath)
        {
            try
            {
                ValidateGame(game);
                filePath = Path.ChangeExtension(filePath, FileExtension);
                ValidateFilePath(filePath);

                var serializer = new XmlSerializer(typeof(Game));
                using (var writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, game);
                }
                LogCallback?.Invoke($"Игра успешно сохранена в {filePath}");
            }
            catch (Exception ex)
            {
                LogCallback?.Invoke($"Ошибка сохранения XML: {ex.Message}");
                throw new GameSerializationException("Ошибка сохранения игры в XML", ex);
            }
        }

        public override Game Deserialize(string filePath)
        {
            try
            {
                filePath = Path.ChangeExtension(filePath, FileExtension);
                ValidateFilePath(filePath);

                var serializer = new XmlSerializer(typeof(Game));
                using (var reader = new StreamReader(filePath))
                {
                    var game = (Game)serializer.Deserialize(reader);
                    if (game == null)
                        throw new GameSerializationException("Не удалось загрузить игровую сессию");

                    LogCallback?.Invoke($"Игра успешно загружена из {filePath}");
                    return game;
                }
            }
            catch (Exception ex)
            {
                LogCallback?.Invoke($"Ошибка загрузки XML: {ex.Message}");
                throw new GameSerializationException("Ошибка загрузки игры из XML", ex);
            }
        }
    }
    public static class GameSerializerFactory
    {
        public static IGameSerializer CreateSerializer(SerializationFormat format)
        {
            return format switch
            {
                SerializationFormat.Json => new JsonGameSerializer(),
                SerializationFormat.Xml => new XmlGameSerializer(),
                _ => throw new ArgumentOutOfRangeException(nameof(format), "Неподдерживаемый формат сериализации")
            };
        }
    }

    public enum SerializationFormat
    {
        Json,
        Xml
    }

    public class GameSerializationException : Exception
    {
        public GameSerializationException(string message) : base(message) { }
        public GameSerializationException(string message, Exception inner) : base(message, inner) { }
    }
}
