using olympo_webapi.Models;
using Microsoft.EntityFrameworkCore;
using olympo_webapi.Infrastructure;
using System.Threading.Tasks;
using System.Collections.Generic;

public class UserExerciseRepository : IUserExerciseRepository
{
    private readonly ConnectionContext _context;

    public UserExerciseRepository(ConnectionContext context)
    {
        _context = context;
    }

    public async Task DeleteAsync(int userId, int exerciseId)
    {
        var userExercise = await _context.UserExercises
            .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.ExerciseId == exerciseId);

        if (userExercise != null)
        {
            _context.UserExercises.Remove(userExercise);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<UserExercise>> GetAllAsync()
    {
        return await _context.UserExercises.ToListAsync();
    }

    public async Task<UserExercise?> GetByIdAsync(int id)
    {
        return await _context.UserExercises.FindAsync(id);
    }

    public async Task<IEnumerable<UserExercise>> GetByUserIdAsync(int userId)
    {
        return await _context.UserExercises
            .Where(ue => ue.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserExercise>> GetByExerciseIdAsync(int exerciseId)
    {
        return await _context.UserExercises
            .Where(ue => ue.ExerciseId == exerciseId)
            .ToListAsync();
    }

    public async Task AddAsync(UserExercise userExercise)
    {
        await _context.UserExercises.AddAsync(userExercise);
        var result = await _context.SaveChangesAsync();
        Console.WriteLine($"UserExercise salvo no banco. Linhas afetadas: {result}");
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.UserExercises.AnyAsync(ue => ue.Id == id);
    }

    public async Task<IEnumerable<Session>> GetSessionsByUserAndExerciseAsync(int userId, int exerciseId)
    {
        return await _context.Sessions
            .Where(s => s.ExerciseId == exerciseId && s.UserId == userId)
            .ToListAsync();
    }

}