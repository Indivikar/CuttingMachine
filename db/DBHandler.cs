using SchneidMaschine.model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SchneidMaschine.db
{
    public class DBHandler
    {
        private DataModel dataModel;
        private SqlConnection connection;
        

        public DBHandler(DataModel dataModel)
        {
            this.dataModel = dataModel;

            //string path = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            //this.connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=""" + path + @"\\db\\Database1.mdf"";Integrated Security=True;User Instance=True");

            string connect_string = Properties.Settings.Default.DatabaseConnectionString;
            this.connection = new SqlConnection(connect_string);
        }

        public long GetStreifen40erKurzHeute()
        {
            string wert = "0";

            string sql = "SELECT * FROM [HeuteTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["streifen_40er_kurz"]);
                    Console.WriteLine("streifen_40er_kurz -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetStreifen40erLangHeute()
        {
            string wert = "0";

            string sql = "SELECT * FROM [HeuteTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["streifen_40er_lang"]);
                    Console.WriteLine("streifen_40er_lang -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetStreifen70erDeckelHeute()
        {
            string wert = "0";

            string sql = "SELECT * FROM [HeuteTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["streifen_70er_deckel"]);
                    Console.WriteLine("streifen_70er_deckel -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetRolleAbgewickeltHeute()
        {
            string wert = "0";

            string sql = "SELECT * FROM [HeuteTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["rolle_abgewickelt"]);
                    Console.WriteLine("streifen_70er_deckel -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public bool isResetHeute()
        {
            DateTime dateDB = DateTime.Parse("23.11.2020 00:00:00");

            string sql = "SELECT * FROM [HeuteTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    dateDB = DateTime.Parse(String.Format("{0}", reader["time_last_insert"]));
                    Console.WriteLine("time_last_insert -> " + dateDB);
                }
            }

            connection.Close();

            DateTime dateNow = DateTime.Now;
            string dateNowStr = dateNow.ToString("yyyy-MM-dd");

            string dateDBStr = dateDB.ToString("yyyy-MM-dd");

            Console.WriteLine("dateNowStr -> " + dateNowStr);
            Console.WriteLine("dateDBStr -> " + dateDBStr);

            //DateTime now = DateTime.Now;
            //SqlDateTime sqlNow = new SqlDateTime(now);
            bool wert = dateNowStr.Equals(dateDBStr); // false

            return !wert;
        }

        public long GetStreifen40erKurzRolle()
        {
            string wert = "0";

            string sql = "SELECT * FROM [RollenTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["streifen_40er_kurz"]);
                    Console.WriteLine("streifen_40er_kurz -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetStreifen40erLangRolle()
        {
            string wert = "0";

            string sql = "SELECT * FROM [RollenTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["streifen_40er_lang"]);
                    Console.WriteLine("streifen_40er_lang -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetStreifen70erDeckelRolle()
        {
            string wert = "0";

            string sql = "SELECT * FROM [RollenTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["streifen_70er_deckel"]);
                    Console.WriteLine("streifen_70er_deckel -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetRollenLaengeAktuell()
        {
            string wert = "";

            string sql = "SELECT * FROM [RollenTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["rollen_laenge_aktuell"]);
                    Console.WriteLine("rollen_laenge_aktuell -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetRollenLaengeGesamt()
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

            return long.Parse(wert);
        }


        public long GetStreifen40erKurzLangzeit()
        {
            string wert = "0";

            string sql = "SELECT * FROM [LaufzeitTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["streifen_40er_kurz"]);
                    Console.WriteLine("streifen_40er_kurz -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetStreifen40erLangLangzeit()
        {
            string wert = "0";

            string sql = "SELECT * FROM [LaufzeitTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["streifen_40er_lang"]);
                    Console.WriteLine("streifen_40er_lang -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetStreifen70erDeckelLangzeit()
        {
            string wert = "0";

            string sql = "SELECT * FROM [LaufzeitTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["streifen_70er_deckel"]);
                    Console.WriteLine("streifen_70er_deckel -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
        }

        public long GetVerbrauchteRollenLangzeit()
        {
            string wert = "0";

            string sql = "SELECT * FROM [LaufzeitTotal] WHERE id=1";

            SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wert = String.Format("{0}", reader["verbrauchte_rollen"]);
                    Console.WriteLine("verbrauchte_rollen -> " + wert);
                }
            }

            connection.Close();

            return long.Parse(wert);
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

        public void updateCut()
        {
            try
            {
                Statistik stats = dataModel.Statistik;

                string sql_update_heute = "UPDATE HeuteTotal " +
                            "SET " +
                                "streifen_40er_kurz = " + stats.HeuteStreifen40erKurz + ", " +
                                "streifen_40er_lang = " + stats.HeuteStreifen40erLang + ", " +
                                "streifen_70er_deckel = " + stats.HeuteStreifen70erDeckel + ", " +
                                "rolle_abgewickelt = " + stats.HeuteRolleAbgewickelt + ", " +
                                "time_last_insert = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                            "WHERE id = 1";

                string sql_update_rolle = "UPDATE RollenTotal " +
                                            "SET " +
                                                "streifen_40er_kurz = " + stats.RolleStreifen40erKurz + ", " +
                                                "streifen_40er_lang = " + stats.RolleStreifen40erLang + ", " +
                                                "streifen_70er_deckel = " + stats.RolleStreifen70erDeckel + ", " +
                                                "rollen_laenge_aktuell = " + stats.RolleIstLaenge + "" +
                                            "WHERE id = 1";

                string sql_update_langzeit = "UPDATE LaufzeitTotal " +
                                                "SET " +
                                                    "streifen_40er_kurz = " + stats.LangzeitStreifen40erKurz + ", " +
                                                    "streifen_40er_lang = " + stats.LangzeitStreifen40erLang + ", " +
                                                    "streifen_70er_deckel = " + stats.LangzeitStreifen70erDeckel + "" +
                                                "WHERE id = 1";


                connection.Open();

                SqlCommand command_update_heute = new SqlCommand(sql_update_heute, connection);
                command_update_heute.ExecuteNonQuery();

                SqlCommand command_update_rolle = new SqlCommand(sql_update_rolle, connection);
                command_update_rolle.ExecuteNonQuery();

                SqlCommand command_update_langzeit = new SqlCommand(sql_update_langzeit, connection);
                command_update_langzeit.ExecuteNonQuery();

                connection.Close();

                dataModel.initStatsHeute();
                dataModel.initStatsRolle();
                dataModel.initStatsLangzeit();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception occured while creating table:" + e.Message + "\t" + e.GetType());
                connection.Close();
            }
        }

        public void updateVerbrauchteRolle()
        {
            try
            {
                Statistik stats = dataModel.Statistik;

                string sql_update_langzeit = "UPDATE LaufzeitTotal " +
                                                "SET verbrauchte_rollen = " + stats.LangzeitVerbrauchteRollen +
                                                "WHERE id = 1";

                connection.Open();

                SqlCommand command_update_langzeit = new SqlCommand(sql_update_langzeit, connection);
                command_update_langzeit.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception occured while creating table:" + e.Message + "\t" + e.GetType());
                connection.Close();
            }
        }

        public void resetHeute()
        {
            try
            {
                string sql_update_heute = "UPDATE HeuteTotal " +
                                    "SET " +
                                        "streifen_40er_kurz = 0, " +
                                        "streifen_40er_lang = 0, " +
                                        "streifen_70er_deckel = 0, " +
                                        "rolle_abgewickelt = 0, " +
                                        "time_last_insert = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                                    "WHERE id = 1";

                connection.Open();

                SqlCommand command = new SqlCommand(sql_update_heute, connection);
                command.ExecuteNonQuery();

                connection.Close();

                dataModel.initStatsHeute();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception occured while creating table:" + e.Message + "\t" + e.GetType());
                connection.Close();
            }
        }

        public void resetRolle()
        {
            try
            {
                string sql_update_rolle = "UPDATE RollenTotal " +
                                    "SET " +
                                        "streifen_40er_kurz = 0, " +
                                        "streifen_40er_lang = 0, " +
                                        "streifen_70er_deckel = 0, " +
                                        "rollen_laenge_aktuell = " + GetRollenLaengeGesamt() +                                   
                                    "WHERE id = 1";

                connection.Open();

                SqlCommand command = new SqlCommand(sql_update_rolle, connection);
                command.ExecuteNonQuery();

                connection.Close();

                dataModel.initStatsRolle();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception occured while creating table:" + e.Message + "\t" + e.GetType());
                connection.Close();
            }
        }

        public void resetLangzeit()
        {
            try
            {
                string sql_update_rolle = "UPDATE LaufzeitTotal " +
                                    "SET " +
                                        "streifen_40er_kurz = 0, " +
                                        "streifen_40er_lang = 0, " +
                                        "streifen_70er_deckel = 0, " +
                                        "verbrauchte_rollen = 0" +
                                    "WHERE id = 1";

                connection.Open();

                SqlCommand command = new SqlCommand(sql_update_rolle, connection);
                command.ExecuteNonQuery();

                connection.Close();

                dataModel.initStatsLangzeit();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception occured while creating table:" + e.Message + "\t" + e.GetType());
                connection.Close();
            }
        }

        public void UpdateTest(long wert)
        {
            string sql = "UPDATE [RollenTotal] SET streifen_40er_kurz = '999' WHERE Id=1";

            connection.Open();

            SqlCommand command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();

            connection.Close();

            GetStreifen40erKurzRolle();

            //dataModel.initStatsRolle();
        }

        public void UpdateTest2(long wert)
        {
            string sql = "UPDATE RollenTotal (streifen_40er_kurz) " +
                            "VALUES (999) " +
                            "WHERE Id = '1'"; 

            connection.Open();

            SqlCommand command = new SqlCommand(sql, connection);
            //command.Parameters.AddWithValue("@Id", 0);
            //command.Parameters.AddWithValue("@streifen_40er_kurz", 999);

            command.ExecuteNonQuery();

            connection.Close();

            GetStreifen40erKurzRolle();

            //dataModel.initStatsRolle();
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
