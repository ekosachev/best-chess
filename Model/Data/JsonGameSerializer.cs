using Model.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
                filePath = EnsureJsonExtension(filePath);
                EnsureDirectoryExists(filePath);

                // 1. Преобразуем Game в GameDTO
                var gameDto = GameDTO.FromGame(game);

                // 2. Сериализуем DTO
                string json = JsonConvert.SerializeObject(gameDto, GetSerializerSettings());

                // 3. Записываем в файл
                File.WriteAllText(filePath, json);
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
                // 1. Проверяем файл
                filePath = EnsureJsonExtension(filePath);
                if (!File.Exists(filePath))
                    throw new GameSerializationException("Файл сохранения не найден");

                // 2. Читаем JSON
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                    throw new GameSerializationException("Файл сохранения пуст");

                // 3. Десериализуем в DTO
                var gameDto = JsonConvert.DeserializeObject<GameDTO>(json, GetSerializerSettings());
                if (gameDto == null)
                    throw new GameSerializationException("Неверный формат файла сохранения");

                // 4. Преобразуем DTO обратно в Game
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

        private JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                NullValueHandling = NullValueHandling.Include
            };
        }

        [Serializable]
        private class GameDTO
        {
            public FigureDTO[][] Board { get; set; }
            public string CurrentPlayer { get; set; }
            public (int row, int col)? EnPassantTarget { get; set; }
            public bool IsCheck { get; set; }
            public bool IsCheckmate { get; set; }
            public bool IsStalemate { get; set; }
            public bool IsDrawByRepetition { get; set; }
            public (int row, int col)? LastPawnPosition { get; set; }

            public static GameDTO FromGame(Game game)
            {
                if (game == null) throw new ArgumentNullException(nameof(game));
                if (game.Board == null) throw new InvalidOperationException("Игровая доска не инициализирована");

                return new GameDTO
                {
                    Board = SerializeBoard(game.Board),
                    CurrentPlayer = game.CurrentPlayer ?? "White",
                    EnPassantTarget = game.EnPassantTarget,
                    IsCheck = game.IsCheck,
                    IsCheckmate = game.IsCheckmate,
                    IsStalemate = game.IsStalemate,
                    IsDrawByRepetition = game.IsDrawByRepetition,
                    LastPawnPosition = game.LastPawn?.Position
                };
            }

            public Game ToGame()
            {
                var game = new Game
                {
                    CurrentPlayer = CurrentPlayer ?? "White",
                    EnPassantTarget = EnPassantTarget,
                    IsCheck = IsCheck,
                    IsCheckmate = IsCheckmate,
                    IsStalemate = IsStalemate,
                    IsDrawByRepetition = IsDrawByRepetition
                };

                game.Board = DeserializeBoard(Board, game);

                if (LastPawnPosition.HasValue)
                {
                    var pos = LastPawnPosition.Value;
                    if (pos.row >= 0 && pos.row < 8 && pos.col >= 0 && pos.col < 8)
                    {
                        game.LastPawn = game.Board[pos.row][pos.col] as Pawn;
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
        private abstract class FigureDTO
        {
            public string Type { get; set; }
            public string Color { get; set; }
            public (int row, int col) Position { get; set; }
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
                dto.Position = figure.Position;
                dto.HasMoved = figure.HasMoved;

                return dto;
            }

            public abstract Figure ToFigure();
        }

        [Serializable] private class PawnDTO : FigureDTO { public override Figure ToFigure() => new Pawn(Color, Position) { HasMoved = HasMoved }; }
        [Serializable] private class KingDTO : FigureDTO { public override Figure ToFigure() => new King(Color, Position) { HasMoved = HasMoved }; }
        [Serializable] private class QueenDTO : FigureDTO { public override Figure ToFigure() => new Queen(Color, Position) { HasMoved = HasMoved }; }
        [Serializable] private class RookDTO : FigureDTO { public override Figure ToFigure() => new Rook(Color, Position) { HasMoved = HasMoved }; }
        [Serializable] private class BishopDTO : FigureDTO { public override Figure ToFigure() => new Bishop(Color, Position) { HasMoved = HasMoved }; }
        [Serializable] private class KnightDTO : FigureDTO { public override Figure ToFigure() => new Knight(Color, Position) { HasMoved = HasMoved }; }

        private string EnsureJsonExtension(string path)
        {
            return System.IO.Path.GetExtension(path)?.Equals(".json", StringComparison.OrdinalIgnoreCase) == true
                ? path
                : path + ".json";
        }

        private void EnsureDirectoryExists(string filePath)
        {
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}