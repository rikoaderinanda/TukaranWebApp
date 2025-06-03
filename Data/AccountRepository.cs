using Dapper;
using TukaranWebApp.Models;
using Microsoft.Data.SqlClient;

namespace TukaranWebApp.Data
{
    public interface IAccountRepository
    {
        
        Task<Account?> GetAccountLoginAsync(Account account);
        Task<Account?> CheckLoginGoogle(Account account);
        Task<IEnumerable<Account?>> GetAllAsync();
        Task<Account?> GetByUsernameAsync(string email);
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

        public async Task<Account?> CheckLoginGoogle(Account account)
        {
            var acc = await GetByUsernameAsync(account.Username ?? string.Empty);
            if (acc == null)
            {
                account.PasswordHash = "Google Login";
                acc = account;
                await CreateAsync(acc);
            }
            return acc;
        }
        public async Task<IEnumerable<Account?>> GetAllAsync()
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Account>("SELECT * FROM Accounts");
        }
        public async Task<Account?> GetAccountLoginAsync(Account account)
        {
            var username = account.Username;
            var passwordHash = account.PasswordHash;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(passwordHash))
            {
                return null;
            }

            using var conn = _db.CreateConnection();
            var res = await conn.QuerySingleOrDefaultAsync<Account>(
                "SELECT * FROM Accounts WHERE Username = @Username and PasswordHash =  @PasswordHash", new { Username = account.Username, PasswordHash = account.PasswordHash });
            return res;
        }
        public async Task<Account?> GetByUsernameAsync(string username)
        {
            try
            {
                using var conn = _db.CreateConnection();
                var sql = "SELECT * FROM Accounts WHERE Username = @Username";
                return await conn.QuerySingleOrDefaultAsync<Account>(sql, new { Username = username });
            }
            catch (SqlException ex)
            {
                // Log error
                throw new Exception("Database error. Please contact support.");
            }
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
