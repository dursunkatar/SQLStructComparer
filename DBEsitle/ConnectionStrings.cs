using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBEsitle
{
    public struct ConnectionStrings
    {
        public static string StandardSecurity
        {
            get
            {
                return "Server={0};Database=master;User Id={1};Password={2};";
            }
        }

        public static string TrustedConnection
        {
            get
            {
                return "Server={0};Database=master;Trusted_Connection=True;";
            }
        }
    }
}
