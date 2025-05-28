using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Dapper;

namespace TukaranWebApp.Data
{
    public class TableInitializer
    {
        private readonly AppDbConnection _db;

        public TableInitializer(AppDbConnection db)
        {
            _db = db;
        }

        public void InitTables()
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Accounts' AND xtype='U')
                    BEGIN
                        CREATE TABLE Accounts (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Username NVARCHAR(50) NOT NULL,
                            PasswordHash NVARCHAR(255) NOT NULL
                        );
                    END
                    ";
            conn.Execute(sql);
        }
    }
}