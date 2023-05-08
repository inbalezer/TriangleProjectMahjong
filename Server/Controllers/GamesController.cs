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


        //[HttpGet("addGame/{gameName}")]
        //public async Task<IActionResult> AddGames(int userId, string gameName)
        //{
        //    int? sessionId = HttpContext.Session.GetInt32("userId");
        //    if (sessionId != null)
        //    {
        //        if (userId == sessionId)
        //        {
        //            object param = new
        //            {
        //                UserId = userId
        //            };
        //            string userQuery = "SELECT FirstName FROM Users WHERE ID = @UserId";
        //            var userRecords = await _db.GetRecordsAsync<UserWithGames>(userQuery, param);
        //            UserWithGames user = userRecords.FirstOrDefault();
        //            if (user != null)
        //            {
        //                object newGameParam = new
        //                {
        //                    GameFullName = gameName,
        //                    GameCode = 0,
        //                    PublishStatus = "Not Eligible",
        //                    UserId = userId

        //                };
        //                string insertGameQuery = "INSERT INTO Games (GameFullName, GameCode, PublishStatus,  UserId) " +
        //                        "VALUES (@GameFullName, @GameCode, @PublishStatus, @UserId)";
        //                int newGameId = await _db.InsertReturnId(insertGameQuery, newGameParam);
        //                if (newGameId != 0)
        //                {
        //                    int gameCode = newGameId + 100;
        //                    object updateParam = new
        //                    {
        //                        ID = newGameId,
        //                        GameCode = gameCode

        //                    };
        //                    string updateCodeQuery = "UPDATE Games SET GameCode = @GameCode	WHERE ID=@ID";
        //                    bool isUpdate = await _db.SaveDataAsync(updateCodeQuery, updateParam);
        //                    if (isUpdate == true)
        //                    {
        //                        object param2 = new
        //                        {
        //                            ID = newGameId
        //                        };
        //                        string gameQuery = "SELECT ID,GameFullName,GameCode,PublishStatus FROM Games WHERE ID = @ID";
        //                        var gameRecord = await _db.GetRecordsAsync<Game>(gameQuery, param2);
        //                        Game newGame = gameRecord.FirstOrDefault();
        //                        return Ok(newGame);
        //                    }

        //                }
        //                return BadRequest("Game not created");
        //            }
        //            return BadRequest("User Not Found");
        //        }
        //        return BadRequest("User Not Logged In");
        //    }
        //    return BadRequest("No Session");
        //}

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
                        // Check if the game name already exists
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
                                string gameQuery = "SELECT ID,GameFullName,GameCode,PublishStatus FROM Games WHERE ID = @ID";
                                var gameRecord = await _db.GetRecordsAsync<Game>(gameQuery, param2);
                                Game newGame = gameRecord.FirstOrDefault();
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
                        //bool gameValidation = false;

                        //object param1 = new
                        //{
                        //    ID = gameToPublish.ID
                        //};

                        //string queryEligibleToPublish = "SELECT Games.PublishStatus FROM Games WHERE Games.ID = @ID AND LENGTH(Games.GameInstruction) > 31 AND (SELECT COUNT(*) FROM Matches WHERE Matches.GameID = Games.ID) >= 5";
                        //var recordEligible = await _db.GetRecordsAsync<string>(queryEligibleToPublish, param1);
                        //string PublishStatus = recordEligible.FirstOrDefault();


                        string updateQuery = "UPDATE Games SET PublishStatus=@PublishStatus WHERE ID=@ID";
                        bool isUpdate = await _db.SaveDataAsync(updateQuery, gameToPublish);

                        if (isUpdate)
                        {
                            return Ok();
                        }
                        return BadRequest("Update Failed");
                    }                 
                    return BadRequest("It's Not Your Game");
                }
                return BadRequest("User Not Logged In");              
            }
            return BadRequest("No Session");
        }






    }
}

