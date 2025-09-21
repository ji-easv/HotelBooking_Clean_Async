using HotelBooking.Core;
using Microsoft.AspNetCore.Mvc;


namespace HotelBooking.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomsController(IRepository<Room> repos) : Controller
    {
        // GET: rooms
        [HttpGet(Name = "GetRooms")]
        public async Task<IEnumerable<Room>> Get()
        {
            return await repos.GetAllAsync();
        }

        // GET rooms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await repos.GetAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST roooms
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Room room)
        {
            if (room == null)
            {
                return BadRequest();
            }

            await repos.AddAsync(room);
            return CreatedAtRoute("GetRooms", null);
        }


        // DELETE rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id > 0)
            {
                await repos.RemoveAsync(id);
                return NoContent();
            }
            else {
                return BadRequest();
            }
        }

    }
}
