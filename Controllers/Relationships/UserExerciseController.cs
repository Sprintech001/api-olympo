using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using olympo_webapi.Models;
using olympo_webapi.Infrastructure;

namespace olympo_webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserExerciseController : ControllerBase
    {
        private readonly IUserExerciseRepository _userExerciseRepository;
        private readonly ConnectionContext _context;

        public UserExerciseController(IUserExerciseRepository userExerciseRepository, ConnectionContext context)
        {
            _userExerciseRepository = userExerciseRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserExercise>>> GetAll()
        {
            var result = await _userExerciseRepository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserExercise>>> GetByUser(int userId)
        {
            var result = await _userExerciseRepository.GetByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpGet("exercise/{exerciseId}")]
        public async Task<ActionResult<IEnumerable<UserExercise>>> GetByExercise(int exerciseId)
        {
            var result = await _userExerciseRepository.GetByExerciseIdAsync(exerciseId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] UserExercise userExercise)
        {
            Console.WriteLine($"Recebido: UserId={userExercise.UserId}, ExerciseId={userExercise.ExerciseId}");

            if (userExercise.UserId == null || userExercise.ExerciseId == null)
            {
                Console.WriteLine("UserId ou ExerciseId nulo!");
                return BadRequest("UserId e ExerciseId são obrigatórios.");
            }

            userExercise.User = null;
            userExercise.Exercise = null;

            Console.WriteLine($"Iniciando validação para UserId: {userExercise.UserId}");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userExercise.UserId);
            Console.WriteLine($"Consulta ao banco para UserId {userExercise.UserId}: {userExists}");

            if (!userExists)
            {
                Console.WriteLine($"Usuário com ID {userExercise.UserId} não encontrado no banco de dados.");
                return BadRequest($"Usuário com ID {userExercise.UserId} não encontrado.");
            }

            var exerciseExists = await _context.Exercises.AnyAsync(e => e.Id == userExercise.ExerciseId);
            Console.WriteLine($"Consulta ao banco para ExerciseId {userExercise.ExerciseId}: {exerciseExists}");

            if (!exerciseExists)
            {
                Console.WriteLine($"Exercício com ID {userExercise.ExerciseId} não encontrado no banco de dados.");
                return BadRequest($"Exercício com ID {userExercise.ExerciseId} não encontrado.");
            }

            Console.WriteLine("Adicionando UserExercise ao banco de dados.");
            await _userExerciseRepository.AddAsync(userExercise);
            Console.WriteLine("UserExercise adicionado com sucesso.");

            return Created("", userExercise);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromQuery] int userId, [FromQuery] int exerciseId)
        {
            await _userExerciseRepository.DeleteAsync(userId, exerciseId);
            return NoContent();
        }

        [HttpGet("sessions/{userId}")]
        public async Task<IActionResult> GetSessionsByUserAndExercise(int userId)
        {
            var sessions = await _context.Sessions
                .Where(s => s.UserId == userId)
                .Include(s => s.Exercise)
                .Include(s => s.User)
                .ToListAsync();

            if (sessions == null || sessions.Count == 0)
            {
                return NotFound();
            }

            return Ok(sessions);
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessionsByUserAndExercise([FromQuery] int userId, [FromQuery] int exerciseId)
        {
            var sessions = await _context.Sessions
                .Where(s => s.UserId == userId && s.ExerciseId == exerciseId)
                .Include(s => s.Exercise)
                .Include(s => s.User)
                .ToListAsync();

            if (sessions == null || sessions.Count == 0)
                return NotFound();

            return Ok(sessions);
        }
    }
}
