using Model.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Data
{
    public abstract class GameSerializer : IGameSerializer
    {
        public SerializationCallback LogCallback { get; set; }
        public abstract string FileExtension { get; }

        public abstract void Serialize(Game game, string filePath);
        public abstract Game Deserialize(string filePath);

        protected void ValidateGame(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game), "Игровая сессия не может быть null");
        }

        protected void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым");

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
    }
    public class GameSerializationException : Exception
    {
        public GameSerializationException(string message) : base(message) { }
        public GameSerializationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
