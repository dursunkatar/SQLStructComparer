using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBEsitle
{
    public class DbServer
    {
        public string Ip { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public bool LoginSecure { get; set; }

        public string ConnectionString
        {
            get
            {
                return LoginSecure
                    ? string.Format(ConnectionStrings.TrustedConnection, Ip)
                    : string.Format(ConnectionStrings.StandardSecurity, Ip, Username, Password);
            }
        }
    }
}
