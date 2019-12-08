using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBEsitle
{
    public struct DBUtilities
    {
        public static IEnumerable<string> TabloOlustur(DbServer kaynakServer, DbServer hedefServer, IEnumerable<string> tablolar)
        {
            Server _server = new Server(kaynakServer.Ip);
            _server.ConnectionContext.LoginSecure = kaynakServer.LoginSecure;

            if (!kaynakServer.LoginSecure)
            {
                _server.ConnectionContext.Login = kaynakServer.Username;
                _server.ConnectionContext.Password = kaynakServer.Password;
            }
            _server.ConnectionContext.Connect();

            string database = kaynakServer.Database;
            Scripter scripter = new Scripter(_server);
            Database _db = _server.Databases[database];
            var sqlCodes = new List<string>();

            foreach (string tablo in tablolar)
            {
                foreach (string script in _db.Tables[tablo].Script())
                    sqlCodes.Add(script);

                foreach (Index index in _db.Tables[tablo].Indexes)
                    foreach (string script in index.Script())
                        sqlCodes.Add(script);

                foreach (Trigger trigger in _db.Tables[tablo].Triggers)
                    foreach (string script in trigger.Script())
                        sqlCodes.Add(script);
            }
            return SqlExec(hedefServer, sqlCodes);

        }
        public static List<View> ViewlariGetir(DbServer server)
        {
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = server.ConnectionString;
                conn.Open();
                var liste = new List<View>();
                using (var cmd = conn.CreateCommand())
                {
                    string database = server.Database;
                    cmd.CommandText = $@"SELECT TABLE_NAME
	                                            ,VIEW_DEFINITION
                                            FROM {database}.INFORMATION_SCHEMA.VIEWS";

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        var _view = new View();
                        _view.Name = dr["TABLE_NAME"].ToString();
                        _view.SqlCreate = dr["VIEW_DEFINITION"].ToString();
                        liste.Add(_view);
                    }
                    dr.Close();
                    conn.Close();
                    return liste;
                }
            }
        }

        public static List<ProcFunc> ProsodurVeFonksiyonlariGetir(DbServer server)
        {
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = server.ConnectionString;
                conn.Open();
                var liste = new List<ProcFunc>();
                using (var cmd = conn.CreateCommand())
                {
                    string database = server.Database;
                    cmd.CommandText = $@"SELECT ROUTINE_NAME
	                                        ,ROUTINE_TYPE
	                                        ,ROUTINE_DEFINITION
                                        FROM {database}.INFORMATION_SCHEMA.ROUTINES
                                        WHERE ROUTINE_TYPE = 'PROCEDURE'
	                                        OR ROUTINE_TYPE = 'FUNCTION'
                                        ORDER BY ROUTINE_NAME ASC";

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        var _pf = new ProcFunc();
                        _pf.Name = dr["ROUTINE_NAME"].ToString();
                        _pf.Type = dr["ROUTINE_TYPE"].ToString();
                        _pf.SqlCreate = dr["ROUTINE_DEFINITION"].ToString();
                        liste.Add(_pf);
                    }
                    dr.Close();
                    conn.Close();
                    return liste;
                }
            }
        }

        public static IEnumerable<string> SqlExec(DbServer server, IEnumerable<string> sqlCodes)
        {
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = server.ConnectionString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string database = server.Database;
                    cmd.CommandText = $"USE [{database}]";
                    cmd.ExecuteNonQuery();

                    var errors = new List<string>();
                    foreach (string sql in sqlCodes)
                    {
                        cmd.CommandText = sql;
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex.Message);
                        }
                    }
                    conn.Close();
                    return errors;
                }
            }
        }

        public static List<Table> TabloVeKolonlariGetir(DbServer server)
        {
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = server.ConnectionString;
                conn.Open();
                var liste = new List<TableProperties>();
                var tablolar = new List<Table>();
                using (var cmd = conn.CreateCommand())
                {
                    string database = server.Database;
                    cmd.CommandText = $"USE [{database}]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = $@"SELECT Tablolar.TABLE_NAME
	                                        ,Columns.COLUMN_NAME
	                                        ,Columns.DATA_TYPE
	                                        ,Columns.CHARACTER_MAXIMUM_LENGTH
	                                        ,Columns.IS_NULLABLE
                                        FROM INFORMATION_SCHEMA.TABLES AS Tablolar
                                        INNER JOIN INFORMATION_SCHEMA.COLUMNS AS Columns ON Tablolar.TABLE_NAME = Columns.TABLE_NAME
                                        WHERE Tablolar.TABLE_TYPE = 'BASE TABLE'
                                        ORDER BY Tablolar.TABLE_NAME ASC";

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var tkm = new TableProperties();
                        tkm.TABLE_NAME = dr["TABLE_NAME"].ToString();
                        tkm.COLUMN_NAME = dr["COLUMN_NAME"].ToString();
                        tkm.DATA_TYPE = dr["DATA_TYPE"].ToString();
                        tkm.IS_NULLABLE = dr["IS_NULLABLE"].ToString();
                        tkm.CHARACTER_MAXIMUM_LENGTH = dr["CHARACTER_MAXIMUM_LENGTH"].ToString();
                        liste.Add(tkm);
                    }

                    liste.ForEach(item =>
                    {
                        if (!tablolar.Any(m => m.Name == item.TABLE_NAME))
                        {
                            var tbl = new Table();
                            tbl.Name = item.TABLE_NAME;
                            tablolar.Add(tbl);
                        }
                    });

                    tablolar.ForEach(m =>
                    {
                        m.Columns = new List<Column>();
                        liste.Where(k => k.TABLE_NAME == m.Name).ToList().ForEach(k =>
                        {
                            var kolon = new Column();
                            kolon.Name = k.COLUMN_NAME;

                            kolon.Type = k.CHARACTER_MAXIMUM_LENGTH != null
                            && k.CHARACTER_MAXIMUM_LENGTH != "-1"
                            && k.CHARACTER_MAXIMUM_LENGTH != ""
                        ? k.DATA_TYPE + " (" + k.CHARACTER_MAXIMUM_LENGTH + ")"
                        : k.DATA_TYPE;


                            if (k.IS_NULLABLE == "NO")
                            {
                                kolon.Type += " NOT NULL";
                            }
                            m.Columns.Add(kolon);
                        });

                    });

                    dr.Close();
                    conn.Close();
                    return tablolar;
                }
            }
        }

        public static bool BaglantiTest(DbServer server, ComboBox cbo)
        {
            cbo.Items.Clear();
            using (var conn = new SqlConnection())
            {
                try
                {
                    conn.ConnectionString = server.ConnectionString;
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT name
                                        FROM sys.databases
                                        WHERE name NOT IN (
		                                        'master'
		                                        ,'tempdb'
		                                        ,'model'
		                                        ,'msdb'
		                                        )";

                        SqlDataReader dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            cbo.Items.Add(dr["name"].ToString());
                        }
                        dr.Close();
                        conn.Close();
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
