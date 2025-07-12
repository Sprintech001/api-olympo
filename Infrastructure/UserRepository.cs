using olympo_webapi.Models;
using Microsoft.EntityFrameworkCore;
using olympo_webapi.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace olympo_webapi.Infrastructure
{
	public class UserRepository : IUserRepository
	{
		private readonly ConnectionContext _context;
		private readonly UserManager<ApplicationUser> _userManager;


		public UserRepository(ConnectionContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}


		public async Task AddAsync(User user)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user), "O usuário não pode ser nulo.");
			}

			await _context.Users.AddAsync(user);
			await _context.SaveChangesAsync();
		}

		public async Task<List<User>> GetAsync()
		{
			return await _context.Users
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task<User?> GetByIdAsync(int id)
		{
			return await _context.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == id);
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			return await _context.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Email == email);
		}

		public async Task UpdateAsync(User user)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user), "O usuário não pode ser nulo.");
			}

			_context.Users.Update(user);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			if (_context == null)
				throw new InvalidOperationException("_context não está inicializado.");

			if (_userManager == null)
				throw new InvalidOperationException("_userManager não está inicializado.");

			var user = await _context.Users
				.Include(u => u.Gyms)
				.Include(u => u.Exercises)
				.Include(u => u.Sessions)
				.FirstOrDefaultAsync(u => u.Id == id);

			if (user == null)
				return;

			var identityUser = await _userManager.Users
				.FirstOrDefaultAsync(i => i.UserId == user.Id);

			if (identityUser != null)
			{
				var result = await _userManager.DeleteAsync(identityUser);
				if (!result.Succeeded)
				{
					throw new InvalidOperationException("Erro ao deletar ApplicationUser: " +
						string.Join(", ", result.Errors.Select(e => e.Description)));
				}
			}

			_context.Users.Remove(user);
			await _context.SaveChangesAsync();
		}



		public async Task<bool> ExistsAsync(int id)
		{
			return await _context.Users
				.AnyAsync(u => u.Id == id);
		}
	}
}
