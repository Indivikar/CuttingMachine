using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Numerics;
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



        public string GetRollenLaenge()
        {
            string wert = "";

            string sql = "SELECT * FROM [RollenLaenge] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["RollenLaengeGesamt"]);
                    Console.WriteLine("RollenLaengeGesamt -> " + wert);
                }
            }

            connection.Close();

            return wert;
        }

        public string GetRollenLaengeAktuell()
        {
            string wert = "";

            string sql = "SELECT * FROM [RollenTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["RollenLaengeAktuell"]);
                    Console.WriteLine("RollenLaengeAktuell -> " + wert);
                }
            }

            connection.Close();

            return wert;
        }

        public void CreateTable() 
        {
            try
            {
                String query = "CREATE TABLE LaufzeitTotal(ID int IDENTITY(1, 1) PRIMARY KEY, geschnitten_40er bigint, geschnitten_70er bigint, Rollen bigint)";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                command.ExecuteNonQuery();

                Console.WriteLine("Table Created Successfully...");
                connection.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("exception occured while creating table:" + e.Message + "\t" + e.GetType());
                connection.Close();
            }
        }

        public void UpdateTest(long wert)
        {
            string sql = "UPDATE LaufzeitTotal SET geschnitten_40er = " + wert + " WHERE id = 1";

            connection.Open();

            SqlCommand command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();

            connection.Close();

        }

        public void SelectTest() 
        {
            string sql = "SELECT * FROM [LaufzeitTotal]";

            connection.Open();

            SqlCommand command = new SqlCommand(sql, connection);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Console.WriteLine("DB -> " + String.Format("{0}", reader["geschnitten_40er"]));
                }
            }

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine("DB -> " + String.Format("{0}", reader["geschnitten_70er"]));
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
