using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Data.SqlTypes;
using System.Data.SqlClient;

namespace wpfsnake{
    class Connection{
        static string connect = "data source=mysnakedata.database.windows.net;initial catalog=snake;persist security info=True;user id=myhanyka;password=Mihanyka_11;MultipleActiveResultSets=True;App=EntityFramework";
        public static SqlConnection MyConnection = new SqlConnection(@connect);
        public static int On(int x){
            try { MyConnection.Open(); }
            catch (System.Data.SqlClient.SqlException) { return x = 1; }
            return x = 0;
        }
        public static void Off(){
            MyConnection.Close();
        }
    }
    class GetMD{
        public static string Getmd(string input){
            using (System.Security.Cryptography.MD5 Getmd = System.Security.Cryptography.MD5.Create()){
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = Getmd.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++){
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
    public class SnakeHighscore{
        public string PlayerName { get; set; }
        public int Score { get; set; }
    }
    public class Mine{
        public UIElement UiElement { get; set; }
        public Point Position { get; set; }
    }
    public class SnakePart{
        public UIElement UiElement { get; set; }
        public Point Position { get; set; }
        public bool IsHead { get; set; }
    }
}
