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
                filePath = EnsureXmlExtension(filePath);
                EnsureDirectoryExists(filePath);

                var gameDto = GameDTO.FromGame(game);

                var serializer = new XmlSerializer(typeof(GameDTO));
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");

                using (var writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, gameDto, namespaces);
                }

                LogCallback?.Invoke($"Игра успешно сохранена в {filePath}");
            }
            catch (Exception ex)
            {
                LogCallback?.Invoke($"Ошибка сохранения: {ex.Message}");
                throw new GameSerializationException("Не удалось сохранить игру", ex);
            }
        }

        public override Game Deserialize(string filePath)
        {
            try
            {
                filePath = EnsureXmlExtension(filePath);
                if (!File.Exists(filePath))
                    throw new GameSerializationException("Файл сохранения не найден");

                var serializer = new XmlSerializer(typeof(GameDTO));
                GameDTO gameDto;

                using (var reader = new StreamReader(filePath))
                {
                    gameDto = serializer.Deserialize(reader) as GameDTO;
                }

                if (gameDto == null)
                    throw new GameSerializationException("Неверный формат файла сохранения");

                var game = gameDto.ToGame();
                LogCallback?.Invoke($"Игра успешно загружена из {filePath}");
                return game;
            }
            catch (Exception ex)
            {
                LogCallback?.Invoke($"Ошибка загрузки: {ex.Message}");
                throw new GameSerializationException("Не удалось загрузить игру", ex);
            }
        }

        [Serializable]
        [XmlRoot("Game")]
        public class GameDTO
        {
            [XmlArray("Board")]
            [XmlArrayItem("Row")]
            public FigureDTO[][] Board { get; set; }

            [XmlElement("CurrentPlayer")]
            public string CurrentPlayer { get; set; }

            [XmlElement("EnPassantTarget")]
            public PositionDTO EnPassantTarget { get; set; }

            [XmlElement("IsCheck")]
            public bool IsCheck { get; set; }

            [XmlElement("IsCheckmate")]
            public bool IsCheckmate { get; set; }

            [XmlElement("IsStalemate")]
            public bool IsStalemate { get; set; }

            [XmlElement("IsDrawByRepetition")]
            public bool IsDrawByRepetition { get; set; }

            [XmlElement("LastPawnPosition")]
            public PositionDTO LastPawnPosition { get; set; }

            public static GameDTO FromGame(Game game)
            {
                if (game == null) throw new ArgumentNullException(nameof(game));
                if (game.Board == null) throw new InvalidOperationException("Игровая доска не инициализирована");

                return new GameDTO
                {
                    Board = SerializeBoard(game.Board),
                    CurrentPlayer = game.CurrentPlayer ?? "White",
                    EnPassantTarget = game.EnPassantTarget.HasValue ?
                        new PositionDTO { Row = game.EnPassantTarget.Value.row, Col = game.EnPassantTarget.Value.col } :
                        null,
                    IsCheck = game.IsCheck,
                    IsCheckmate = game.IsCheckmate,
                    IsStalemate = game.IsStalemate,
                    IsDrawByRepetition = game.IsDrawByRepetition,
                    LastPawnPosition = game.LastPawn != null ?
                        new PositionDTO { Row = game.LastPawn.Position.row, Col = game.LastPawn.Position.col } :
                        null
                };
            }

            public Game ToGame()
            {
                var game = new Game
                {
                    CurrentPlayer = CurrentPlayer ?? "White",
                    EnPassantTarget = EnPassantTarget != null ? (EnPassantTarget.Row, EnPassantTarget.Col) : ((int, int)?)null,
                    IsCheck = IsCheck,
                    IsCheckmate = IsCheckmate,
                    IsStalemate = IsStalemate,
                    IsDrawByRepetition = IsDrawByRepetition
                };

                game.Board = DeserializeBoard(Board, game);

                if (LastPawnPosition != null)
                {
                    var pos = (LastPawnPosition.Row, LastPawnPosition.Col);
                    if (pos.Row >= 0 && pos.Row < 8 && pos.Col >= 0 && pos.Col < 8)
                    {
                        game.LastPawn = game.Board[pos.Row][pos.Col] as Pawn;
                    }
                }

                return game;
            }

            private static FigureDTO[][] SerializeBoard(Figure[][] board)
            {
                var result = new FigureDTO[8][];
                for (int i = 0; i < 8; i++)
                {
                    result[i] = new FigureDTO[8];
                    for (int j = 0; j < 8; j++)
                    {
                        result[i][j] = board[i][j] != null ? FigureDTO.FromFigure(board[i][j]) : null;
                    }
                }
                return result;
            }

            private static Figure[][] DeserializeBoard(FigureDTO[][] boardDto, Game game)
            {
                var board = new Figure[8][];
                for (int i = 0; i < 8; i++)
                {
                    board[i] = new Figure[8];
                    for (int j = 0; j < 8; j++)
                    {
                        board[i][j] = boardDto[i][j]?.ToFigure();
                    }
                }
                return board;
            }
        }

        [Serializable]
        public class PositionDTO
        {
            [XmlAttribute("Row")]
            public int Row { get; set; }

            [XmlAttribute("Col")]
            public int Col { get; set; }
        }

        [Serializable]
        [XmlInclude(typeof(PawnDTO))]
        [XmlInclude(typeof(KingDTO))]
        [XmlInclude(typeof(QueenDTO))]
        [XmlInclude(typeof(RookDTO))]
        [XmlInclude(typeof(BishopDTO))]
        [XmlInclude(typeof(KnightDTO))]
        public abstract class FigureDTO
        {
            [XmlAttribute("Type")]
            public string Type { get; set; }

            [XmlAttribute("Color")]
            public string Color { get; set; }

            [XmlElement("Position")]
            public PositionDTO Position { get; set; }

            [XmlAttribute("HasMoved")]
            public bool HasMoved { get; set; }

            public static FigureDTO FromFigure(Figure figure)
            {
                if (figure == null) return null;

                FigureDTO dto = figure switch
                {
                    Pawn => new PawnDTO(),
                    King => new KingDTO(),
                    Rook => new RookDTO(),
                    Knight => new KnightDTO(),
                    Bishop => new BishopDTO(),
                    Queen => new QueenDTO(),
                    _ => throw new ArgumentException($"Неизвестный тип фигуры: {figure.GetType().Name}")
                };

                dto.Type = figure.GetType().Name;
                dto.Color = figure.Color ?? "White";
                dto.Position = new PositionDTO { Row = figure.Position.row, Col = figure.Position.col };
                dto.HasMoved = figure.HasMoved;

                return dto;
            }

            public abstract Figure ToFigure();
        }

        [Serializable] public class PawnDTO : FigureDTO { public override Figure ToFigure() => new Pawn(Color, (Position.Row, Position.Col)) { HasMoved = HasMoved }; }
        [Serializable] public class KingDTO : FigureDTO { public override Figure ToFigure() => new King(Color, (Position.Row, Position.Col)) { HasMoved = HasMoved }; }
        [Serializable] public class QueenDTO : FigureDTO { public override Figure ToFigure() => new Queen(Color, (Position.Row, Position.Col)) { HasMoved = HasMoved }; }
        [Serializable] public class RookDTO : FigureDTO { public override Figure ToFigure() => new Rook(Color, (Position.Row, Position.Col)) { HasMoved = HasMoved }; }
        [Serializable] public class BishopDTO : FigureDTO { public override Figure ToFigure() => new Bishop(Color, (Position.Row, Position.Col)) { HasMoved = HasMoved }; }
        [Serializable] public class KnightDTO : FigureDTO { public override Figure ToFigure() => new Knight(Color, (Position.Row, Position.Col)) { HasMoved = HasMoved }; }

        private string EnsureXmlExtension(string path)
        {
            return Path.GetExtension(path)?.Equals(".xml", StringComparison.OrdinalIgnoreCase) == true
                ? path
                : path + ".xml";
        }

        private void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}

