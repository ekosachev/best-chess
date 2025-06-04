using Model.Core;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Data
{
    public delegate void SerializationCallback(string message);

    public interface IGameSerializer
    {
        void Serialize(Game game, string filePath);
        Game Deserialize(string filePath);
        SerializationCallback LogCallback { get; set; }
        string FileExtension { get; }
    }
}
