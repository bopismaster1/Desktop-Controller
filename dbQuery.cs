using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace Desktop_COntroller
{
    internal class dbQuery
    {
        string server = Properties.Settings.Default.host;
        string database = Properties.Settings.Default.database;
        string username = Properties.Settings.Default.username;
        string password = Properties.Settings.Default.password;
        public string insert_update_dete(String q) {



            try
            {
                string conString = "SERVER=" + server + "; DATABASE=" + database + "; UID=" + username + ";PASSWORD=" + password + "";
                MySqlConnection conn = new MySqlConnection(conString);
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                return "Success";
            }
            catch(Exception ex)
            {
                return "Failed: "+ex.ToString();

            }

        }

        public DataTable DbSearch(String q)
        {
            DataTable dataTableRes = new DataTable();
            string conString = "SERVER=" + server + "; DATABASE=" + database + "; UID=" + username + ";PASSWORD=" + password + "";

            using (MySqlConnection conn = new MySqlConnection(conString))
            {
                MySqlCommand cmd = new MySqlCommand(q, conn);

                conn.Open();

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

                adapter.Fill(dataTableRes);

                return dataTableRes;
            }
        }
        public  string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }
    }
}
