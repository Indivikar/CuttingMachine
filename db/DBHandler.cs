using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchneidMaschine.db
{
    class DBHandler
    {
        private SqlConnection connection;

        public DBHandler()
        {
            string connect_string = Properties.Settings.Default.DatabaseConnectionString;
            this.connection = new SqlConnection(connect_string);
        }

        public void SelectTest() 
        {
            string sql = "SELECT * FROM [Table]";

            connection.Open();

            SqlCommand command = new SqlCommand(sql, connection);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Console.WriteLine("DB -> " + String.Format("{0}", reader["FullName"]));
                }
            }

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine("DB -> " + String.Format("{0}", reader["Email"]));
                }
            }

            connection.Close();
        }

        public void InsertTest()
        {
            String query = "INSERT INTO [Table] (FullName,Email,Address,Phone,Gender) VALUES (@fullName,@email,@address,@phone,@gender)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {

                //command.Parameters.AddWithValue("@id", 0);
                command.Parameters.AddWithValue("@fullName", "abc");
                command.Parameters.AddWithValue("@email", "abc");
                command.Parameters.AddWithValue("@address", "abc");
                command.Parameters.AddWithValue("@phone", "abc");
                command.Parameters.AddWithValue("@gender", 0);

                connection.Open();
                //int result = command.ExecuteNonQuery();

                // Check Error
                //if (result < 0)
                    //Console.WriteLine("Error inserting data into Database!");
            }

            connection.Close();
        }
    }
}
