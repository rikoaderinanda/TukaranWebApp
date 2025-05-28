using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TukaranWebApp.Models;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace TukaranWebApp.Data
{
    public class AccountRepository
    {
        private readonly AppDbConnection _db;
        public AccountRepository(AppDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Account>("SELECT * FROM Accounts");
        }

        public async Task<Account> GetByIdAsync(int id)
        {
            using var conn = _db.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Account>(
                "SELECT * FROM Accounts WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Account account)
        {
            using var conn = _db.CreateConnection();
            var sql = @"INSERT INTO Accounts (Username, PasswordHash) 
                        VALUES (@Username, @PasswordHash)";
            return await conn.ExecuteAsync(sql, account);
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