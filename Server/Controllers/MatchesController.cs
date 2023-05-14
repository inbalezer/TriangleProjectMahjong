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

    }
}