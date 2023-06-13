using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TriangleDbRepository;
using TriangleProject.Shared.Models.Portelem;
using TriangleProject.Shared.Models.Editor;
using TriangleProject.Shared.Models.Matches;


namespace TriangleProject.Server.Controllers
{
    [Route("api/[controller]/{userId}")] //the controller will always get and expect this parameter from the user.
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly DbRepository _db;

        public GamesController(DbRepository db)
        {
            _db = db;
        }
        //פונקציית עזר//
        public async Task<bool> canPublish(int gameId)
        {
            object param1 = new
            {
                ID = gameId
            };

            string queryEligibleToPublish = "SELECT count(*) FROM Games WHERE Games.ID = @ID AND LENGTH(Games.GameInstruction) > 0 AND LENGTH(Games.GameFullName) > 0 AND (SELECT COUNT(*) FROM Matches WHERE Matches.GameID = Games.ID) >= 5";
            var recordEligible = await _db.GetRecordsAsync<int>(queryEligibleToPublish, param1);
            int EligibleToPublish = recordEligible.FirstOrDefault();

            if (EligibleToPublish == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGamesByUser(int userId)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {
                    object param = new
                    {
                        UserId = userId
                    };

                    string userQuery = "SELECT FirstName FROM Users WHERE ID = @UserId";
                    var userRecords = await _db.GetRecordsAsync<UserWithGames>(userQuery, param);
                    UserWithGames user = userRecords.FirstOrDefault();

                    if (user != null)
                    {
                        string gameQuery = "SELECT ID,GameFullName,GameCode,PublishStatus FROM Games WHERE UserID = @UserId ";
                        var gamesRecords = await _db.GetRecordsAsync<GameForEditor>(gameQuery, param);
                        user.UserGames = gamesRecords.ToList();
                        return Ok(user);
                    }
                    return BadRequest("User Not Found");
                }
                return BadRequest("User Not Logged In");
            }
            return BadRequest("No Session");
        }
        
        //aaaaaaaaaaaaaaaa

        [HttpGet("addGame/{gameName}")]
        public async Task<IActionResult> AddGames(int userId, string gameName)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {
                    object param = new
                    {
                        UserId = userId
                    };
                    string userQuery = "SELECT FirstName FROM Users WHERE ID = @UserId";
                    var userRecords = await _db.GetRecordsAsync<UserWithGames>(userQuery, param);
                    UserWithGames user = userRecords.FirstOrDefault();
                    if (user != null)
                    {
                        // Checka if the game name already exists
                        object existingGameParam = new
                        {
                            GameFullName = gameName,
                            UserId = userId
                        };
                        string existingGameQuery = "SELECT COUNT(*) FROM Games WHERE GameFullName = @GameFullName AND UserID = @UserId";
                        var existingGameCount1 = await _db.GetRecordsAsync<int>(existingGameQuery, existingGameParam);
                        int existingGameCount = existingGameCount1.FirstOrDefault();
                        if (existingGameCount > 0)

                        {
                            // Add a version number to the game name
                            int versionNumber = 0;
                            string newName = gameName + " (" + versionNumber + ")";
                            while (existingGameCount > 0)
                            {
                                versionNumber++;
                                newName = gameName + " (" + versionNumber + ")";
                                existingGameParam = new
                                {
                                    GameFullName = newName,
                                    UserId = userId

                                };
                                var existingGameCount2 = await _db.GetRecordsAsync<int>(existingGameQuery, existingGameParam);
                                existingGameCount = existingGameCount2.FirstOrDefault();
                            }
                            gameName = newName;
                        }

                        object newGameParam = new
                        {
                            GameFullName = gameName,
                            GameCode = 0,
                            PublishStatus = "Not Eligible",
                            UserId = userId
                        };
                        string insertGameQuery = "INSERT INTO Games (GameFullName, GameCode, PublishStatus,  UserId) " +
                                "VALUES (@GameFullName, @GameCode, @PublishStatus, @UserId)";
                        int newGameId = await _db.InsertReturnId(insertGameQuery, newGameParam);
                        if (newGameId != 0)
                        {
                            int gameCode = newGameId + 100;
                            object updateParam = new
                            {
                                ID = newGameId,
                                GameCode = gameCode
                            };
                            string updateCodeQuery = "UPDATE Games SET GameCode = @GameCode WHERE ID=@ID";
                            bool isUpdate = await _db.SaveDataAsync(updateCodeQuery, updateParam);
                            if (isUpdate == true)
                            {
                                object param2 = new
                                {
                                    ID = newGameId
                                };
                                string gameQuery = "SELECT ID,GameFullName,PublishStatus,GameCode FROM Games WHERE ID = @ID";
                                var gameRecord = await _db.GetRecordsAsync<GameForEditor>(gameQuery, param2);
                                GameForEditor newGame = gameRecord.FirstOrDefault();
                                return Ok(newGame);
                            }

                        }
                        return BadRequest("Game not created");
                    }
                    return BadRequest("User Not Found");
                }
                return BadRequest("User Not Logged In");
            }
            return BadRequest("No Session");
        }

        //aaaaaaaaaaaaaaaa

        [HttpPost("publishGame")]
        public async Task<IActionResult> publishGame(int userId, GamePublish gameToPublish)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {
                    object param = new
                    {
                        UserID = userId,
                        ID = gameToPublish.ID
                    };

                    string checkQuery = "SELECT GameFullName FROM Games WHERE UserID = @UserID AND ID=@ID";
                    var checkRecords = await _db.GetRecordsAsync<string>(checkQuery, param);
                    string gameName = checkRecords.FirstOrDefault();

                    if (gameName != null)
                    {
                        bool gameValidation = await canPublish(gameToPublish.ID);

                        if (gameValidation == false)
                        {
                            gameToPublish.PublishStatus = "Not Eligible";
                        }

                        object pubParam = new
                        {
                            ID = gameToPublish.ID,
                            PublishStatus = gameToPublish.PublishStatus
                        };

                        string updateQuery = "UPDATE Games SET PublishStatus = @PublishStatus WHERE ID=@ID";
                        bool isUpdate = await _db.SaveDataAsync(updateQuery, pubParam);

                        if (isUpdate)
                        {
                            object param2 = new
                            {
                                ID = gameToPublish.ID
                            };

                            string gameQuery = "SELECT ID, GameFullName, GameCode, PublishStatus FROM Games WHERE ID = @ID";
                            var gameRecord = await _db.GetRecordsAsync<GameForEditor>(gameQuery, param2);
                            GameForEditor gameToReturn = gameRecord.FirstOrDefault();

                            if (gameToReturn != null)
                            {
                                return Ok(gameToReturn);
                            }
                        }
                        return BadRequest("Publish update Failed");
                    }
                    return BadRequest("It's Not Your Game");
                }
                return BadRequest("User Not Logged In");
            }
            return BadRequest("No Session");
        }

        //aaaaaaaaaaaaaaaa

        [HttpDelete("{GameIdToDelete}")]
        public async Task<IActionResult> DeleteGame(int userId, int GameIdToDelete)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {
                    string checkMatchesQuery = "SELECT count(*) from Games, Matches where Games.ID = Matches.GameID and Matches.GameID = @ID";
                    var isMatchesExist = await _db.GetRecordsAsync<int>(checkMatchesQuery, new { ID = GameIdToDelete });
                    int MatchesExist = isMatchesExist.FirstOrDefault();

                    if (MatchesExist > 0)
                    {
                        string DeleteMatchesQuery = "delete from Matches where GameID = @ID";
                        bool isMatchesDeleted = await _db.SaveDataAsync(DeleteMatchesQuery, new { ID = GameIdToDelete });

                        if (isMatchesDeleted)
                        {
                            string DeleteQuery = "DELETE FROM Games WHERE ID=@ID";
                            bool isGameDeleted = await _db.SaveDataAsync(DeleteQuery, new { ID = GameIdToDelete });

                            if (isGameDeleted)
                            {
                                return Ok();
                            }
                            return BadRequest("Failed to delete game");
                        }
                        return BadRequest("Failed to delete matches");
                    }
                    else
                    {
                        string DeleteQuery = "DELETE FROM Games WHERE ID=@ID";
                        bool isGameDeleted = await _db.SaveDataAsync(DeleteQuery, new { ID = GameIdToDelete });

                        if (isGameDeleted)
                        {
                            return Ok();
                        }
                        return BadRequest("Failed to delete game");
                    }

                }
                return BadRequest("User Not Logged In");

            }
            return BadRequest("No Session");

        }

        //aaaaaaaaaaaaaaaa

        [HttpPost("EditGame")]  // מה שקורה בלחיצה על חץ אחורה

        public async Task<IActionResult> editGame(int userId, GameToUpdate gameToUpdate)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {
                    bool gameValidation = await canPublish(gameToUpdate.ID);

                    if (gameValidation == false)
                    {
                        gameToUpdate.PublishStatus = "Not Eligible";
                    }

                    object updateParam = new
                    {
                        ID = gameToUpdate.ID,
                        PublishStatus = gameToUpdate.PublishStatus,
                        GameFullName = gameToUpdate.GameFullName,
                        GameInstruction = gameToUpdate.GameInstruction
                    };

                    string UpdateGameQuery = "UPDATE Games set GameFullName = @GameFullName, GameInstruction = @GameInstruction, PublishStatus = @PublishStatus where ID =@ID";
                    bool isUpdate = await _db.SaveDataAsync(UpdateGameQuery, updateParam);

                    if (isUpdate)
                    {
                        return Ok(gameToUpdate);
                    }
                    return BadRequest("update game faild");
                }
                return BadRequest("User Not Logged In");
            }
            return BadRequest("No Session");
        }

        //aaaaaaaaaaaaaaaa



        [HttpGet("FullGameToEdit/{GameId}")] // מה שקורה שלוחצים על עריכה אחרי שיש משחק
        public async Task<IActionResult> GetFullGame(int userId, int GameId)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {
                    object param = new
                    {
                        ID = GameId
                    };

                    string GetGameQuery = "SELECT ID, GameFullName, PublishStatus, GameInstruction FROM Games WHERE ID = @ID";
                    string GetMatchesQuery = "SELECT ID, GameID, FirstMatch, SecondMatch FROM Matches WHERE GameID = @ID";

                    var recordGame = await _db.GetRecordsAsync<GameToShow>(GetGameQuery, param);
                    GameToShow game = recordGame.FirstOrDefault();

                    if (game != null)
                    {
                        var recordsMatches = await _db.GetRecordsAsync<MatchToShow>(GetMatchesQuery, param);
                        game.MatchesList = recordsMatches.ToList();
                        return Ok(game);
                    }
                    return BadRequest("Game not found");
                }
                return BadRequest("User Not Logged In");
            }
            return BadRequest("No Session");
        }
    }
}





