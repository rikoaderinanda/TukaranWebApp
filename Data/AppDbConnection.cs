using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace TukaranWebApp.Data
{
    public class AppDbConnection
    {
        private readonly IConfiguration _config;
        public AppDbConnection(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection CreateConnection()=> new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    }
}