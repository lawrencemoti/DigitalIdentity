using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class DbConnStrings
    {
        public record DbSecret(string username, string password, string host, int port, string dbname);
    }
}
