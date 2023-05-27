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
    public class MatchesController : ControllerBase
    {
        private readonly DbRepository _db;

        public MatchesController(DbRepository db)
        {
            _db = db;
        }

        [HttpDelete("DeleteMatch/{MatchIdToDelete}")]
        public async Task<IActionResult> DeleteMatch(int userId, int MatchIdToDelete)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {
                    string DeleteMatchQuery = "delete from Matches where ID = @ID";
                    bool isMatchDeleted = await _db.SaveDataAsync(DeleteMatchQuery, new { ID = MatchIdToDelete });

                    if (isMatchDeleted)
                    {
                        return Ok();
                    }
                    return BadRequest("Failed to delete match");
                }
                return BadRequest("User Not Logged In");
            }
            return BadRequest("No Session");
        }

        //aaaaaaaaaaaaaaaa

        [HttpPost("EditMatch")]

        public async Task<IActionResult> EditMatch(int userId, MatchToUpdate matchToUpdate)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {
                    string UpdateMatchQuery = "UPDATE Matches set FirstMatch = @FirstMatch, SecondMatch = @SecondMatch, FirstIsText =@FirstIsText, SecondIsText =@SecondIsText where ID=@ID AND GameID = @GameID";
                    bool isUpdate = await _db.SaveDataAsync(UpdateMatchQuery, matchToUpdate);

                    if (isUpdate)
                    {
                        return Ok(matchToUpdate);
                    }
                    return BadRequest("update match faild");
                }
                return BadRequest("User Not Logged In");
            }
            return BadRequest("No Session");
        }

        //aaaaaaaaaaaaaaaa

        [HttpGet("addMatch")]
        public async Task<IActionResult> AddMatch(int userId, MatchToUpdate matchToInsert)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {
                    string insertMatchQuery = "INSERT INTO Matches (GameID,FirstMatch, SecondMatch, FirstIsText, SecondIsText) values (@GameID ,@FirstMatch ,@SecondMatch ,@FirstIsText ,@SecondIsText)";
                    int newMatchId = await _db.InsertReturnId(insertMatchQuery, matchToInsert);

                    if (newMatchId != 0)
                    {
                        return Ok(newMatchId);
                    }

                    return BadRequest("Match not created");
                }
                return BadRequest("User Not Logged In");
            }
            return BadRequest("No Session");
        }

        [HttpPost("addInstruction")]
        public async Task<IActionResult> AddInstruction(int userId, gameInsrtuctionToInsert gameInstructionToInsert)
        {
            int? sessionId = HttpContext.Session.GetInt32("userId");
            if (sessionId != null)
            {
                if (userId == sessionId)
                {



                    object newInstructionParam = new
                    {
                        ID = gameInstructionToInsert.ID,
                        GameInstruction = gameInstructionToInsert.GameInstruction

                    };

                    string UpdateGameInstructionQuery = "UPDATE Games SET GameInstruction=@GameInstruction WHERE ID=@ID";
                    bool isInstructionUpdate = await _db.SaveDataAsync(UpdateGameInstructionQuery, newInstructionParam);

                    if (isInstructionUpdate)
                    {
                        object param = new
                        {
                            ID = gameInstructionToInsert.ID

                        };
                        string gameCheckQuery = "SELECT GameFullName FROM Games WHERE ID = @ID";
                        var instructionRecord = await _db.GetRecordsAsync<string>(gameCheckQuery, param);
                        string gameCheckToReturn = instructionRecord.FirstOrDefault();

                        if (gameCheckToReturn != null)
                        {

                            if (gameCheckToReturn != gameInstructionToInsert.GameFullName)
                            {
                                object newNameParam = new
                                {
                                    ID = gameInstructionToInsert.ID,
                                    GameFullName = gameInstructionToInsert.GameFullName

                                };

                                string UpdateGameNameQuery = "UPDATE Games SET GameFullName=@GameFullName WHERE ID=@ID";
                                bool isUpdate = await _db.SaveDataAsync(UpdateGameNameQuery, newNameParam);

                                if (isUpdate)
                                {
                                    return Ok(gameInstructionToInsert.GameFullName + "," + gameInstructionToInsert.GameInstruction);
                                }
                                return BadRequest("update game name failed");

                            }
                            return Ok("same name:" + gameInstructionToInsert.GameFullName + "," + gameInstructionToInsert.GameInstruction);

                        }
                        return BadRequest("Game not found");


                    }

                    return BadRequest("update Game Instruction failed");
                }

                return BadRequest("User Not Logged In");
            }
            return BadRequest("No Session");
        }

    }



}