using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TukaranWebApp.Models
{
    public class Account
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

}
