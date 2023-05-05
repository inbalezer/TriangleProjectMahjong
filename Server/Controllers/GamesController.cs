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
                        object newGameParam = new
                        {
                            GameFullName = gameName,
                            GameCode = 0,
                            PublishStatus = "Not Eligible",
                            // todo need to check if exists in the game table//
                            UserId = userId

                        };
                        string insertGameQuery = "INSERT INTO Games (GameFullName, GameCode, PublishStatus, UserId) " +
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
                            string updateCodeQuery = "UPDATE Games SET GameCode = @GameCode	WHERE ID=@ID";
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

    }
}