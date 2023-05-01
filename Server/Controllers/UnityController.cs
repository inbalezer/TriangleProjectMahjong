using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TriangleDbRepository;
using TriangleProject.Shared.Models.Matches;

namespace TriangleProject.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnityController : ControllerBase
    {
        private readonly DbRepository _db;
        public UnityController(DbRepository db)
        {
            _db = db;
        }

        [HttpGet("{GameCode}")]
        public async Task<IActionResult> GetGames(string GameCode)
        {
            object param1 = new
            {
                GameCode = GameCode
            };

            string queryGameId = "SELECT ID FROM Games WHERE GameCode = @GameCode";

            var recordGameId = await _db.GetRecordsAsync<int>(queryGameId, param1);
            int GameId = recordGameId.FirstOrDefault();

            if (GameId > 0)
            {
                object param = new
                {
                    ID = GameId
                };

                string queryEligibleToPublish = "SELECT Games.PublishStatus FROM Games WHERE Games.ID = @ID AND LENGTH(Games.GameInstruction) > 31 AND (SELECT COUNT(*) FROM Matches WHERE Matches.GameID = Games.ID) >= 5";
                var recordEligible = await _db.GetRecordsAsync<string>(queryEligibleToPublish, param);
                string PublishStatus = recordEligible.FirstOrDefault();

                if (PublishStatus != "Not Eligible")
                {
                    if (PublishStatus == "Published")
                    {
                        //==== Get GameInstruction from DB ====
                        string queryGame = "SELECT GameInstruction FROM Games WHERE ID = @ID";
                        var recordGame = await _db.GetRecordsAsync<Game>(queryGame, param);
                        Game game = recordGame.FirstOrDefault();
                        game.FirstMatchesContent  = new List<SingleMatchDetails>();
                        game.SecondMatchesContent = new List<SingleMatchDetails>();


                        //==== Get ALL first matches from DB ====
                        string queryMatchesPartOne = "SELECT FirstMatch FROM Matches WHERE GameID = @ID";
                        string queryIsTextPartOne = "SELECT FirstIsText FROM Matches WHERE GameID = @ID";
                        var recordsMatchesOne = await _db.GetRecordsAsync<string>(queryMatchesPartOne, param);
                        var recordsIsTextOne = await _db.GetRecordsAsync<bool>(queryIsTextPartOne, param);

                        foreach ((string match, bool isText) in recordsMatchesOne.Zip(recordsIsTextOne, (a, b) => (a, b)))
                        {
                            SingleMatchDetails singleMatch = new SingleMatchDetails();
                            singleMatch.IsText = isText;

                            if (isText)
                            {
                                singleMatch.MatchText = match;
                            }
                            else
                            {
                                singleMatch.MatchImg = match;
                            }

                            game.FirstMatchesContent.Add(singleMatch);
                        }


                        //==== Get ALL second matches from DB ====
                        string queryMatchesPartTwo = "SELECT SecondMatch FROM Matches WHERE GameID = @ID";
                        string queryIsTextPartTwo = "SELECT SecondIsText FROM Matches WHERE GameID = @ID";
                        var recordsMatchesTwo = await _db.GetRecordsAsync<string>(queryMatchesPartTwo, param);
                        var recordsIsTextTwo = await _db.GetRecordsAsync<bool>(queryIsTextPartTwo, param);

                        foreach ((string match, bool isText) in recordsMatchesTwo.Zip(recordsIsTextTwo, (a, b) => (a, b)))
                        {
                            SingleMatchDetails singleMatch = new SingleMatchDetails();
                            singleMatch.IsText = isText;

                            if (isText)
                            {
                                singleMatch.MatchText = match;
                            }
                            else
                            {
                                singleMatch.MatchImg = match;
                            }

                            game.SecondMatchesContent.Add(singleMatch);
                        }

                        return Ok(game);
                    }
                    else
                    {
                        return BadRequest("Eligible Game");
                    }
                }
                else
                {
                    return BadRequest("Game Eligible");
                }
            }
            else
            {
                return BadRequest("Game Not Exist");
            }
        }
    }
}


