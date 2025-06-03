using Dapper;
using TukaranWebApp.Models;

namespace TukaranWebApp.Data
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account?>> GetAllAsync();
        Task<Account?> GetByUsernameAsync(string email);
        Task<IEnumerable<Account?>> GetAccountLoginAsync(Account account);
        Task<Account?> GetByIdAsync(int id);
        Task<int> CreateAsync(Account account);
        Task<int> UpdateAsync(Account account);
        Task<int> DeleteAsync(int id);

    }
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbConnection _db;
        public AccountRepository(AppDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Account?>> GetAllAsync()
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Account>("SELECT * FROM Accounts");
        }
        public async Task<IEnumerable<Account?>> GetAccountLoginAsync(Account account)
        {
            var username = account.Username;
            var passwordHash = account.PasswordHash;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(passwordHash))
            {
                return Enumerable.Empty<Account>();
            }

            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Account>(
                "SELECT * FROM Accounts WHERE Username = @Username and PasswordHash =  @PasswordHash", new { Username = account.Username, PasswordHash = account.PasswordHash });
        }
        public async Task<Account?> GetByUsernameAsync(string username)
        {
            using var conn = _db.CreateConnection();
            var account = await conn.QuerySingleOrDefaultAsync<Account>(
                "SELECT * FROM Accounts WHERE Username = @Username", new { Username = username });
            return account;
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            using var conn = _db.CreateConnection();
            var account = await conn.QuerySingleOrDefaultAsync<Account>(
                "SELECT * FROM Accounts WHERE Id = @Id", new { Id = id });
            return account;
        }

        public async Task<int> CreateAsync(Account account)
        {
            using var conn = _db.CreateConnection();
            var sql = @"INSERT INTO Accounts (Username, PasswordHash) 
                        VALUES (@Username, @PasswordHash)";
            return await conn.ExecuteAsync(sql, new { Username = account.Username, PasswordHash = account.PasswordHash });
        }

        public async Task<int> UpdateAsync(Account account)
        {
            using var conn = _db.CreateConnection();
            var sql = @"UPDATE Accounts SET 
                        Username = @Username, 
                        PasswordHash = @PasswordHash 
                        WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, account);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var conn = _db.CreateConnection();
            return await conn.ExecuteAsync("DELETE FROM Accounts WHERE Id = @Id", new { Id = id });
        }
    }
}
