﻿using Microsoft.AspNetCore.Http;
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

        
        






    }
}