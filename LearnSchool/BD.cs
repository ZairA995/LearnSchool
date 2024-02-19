using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnSchool
{
    class BD
    {
        public string conn = "Data Source=localhost;Initial Catalog=learn;User ID=SA;Password=ZairaP@ss";
        public SqlConnection Connection()
        {
            SqlConnection connection = new SqlConnection(conn);
            if(connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }
            return connection;
        }
    }
}
