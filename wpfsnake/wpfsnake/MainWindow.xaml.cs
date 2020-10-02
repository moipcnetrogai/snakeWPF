using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;

namespace wpfsnake{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window{
        public static string Playername = "";
        public MainWindow(){
            InitializeComponent();
        }
        private void registrate_Click(object sender, RoutedEventArgs e){
            Window1 window1 = new Window1();
            window1.Show();
            this.Close();
        }
        public void butlog_Click(object sender, RoutedEventArgs e){
            butlog.Cursor = Cursors.Wait;
            try{
                if (login.Text != ""){
                    string hash = GetMD.Getmd(GetMD.Getmd(GetMD.Getmd(GetMD.Getmd(password.Text))));
                    string sqlchek = "Select id from userdata where username='" + login.Text + "' and password='" + hash + "'";
                    SqlCommand cmd = new SqlCommand(sqlchek, Connection.MyConnection);
                    Connection.On(0);
                    int chek = 0;
                    SqlDataReader rd = cmd.ExecuteReader();
                    while (rd.Read())
                        chek = rd.GetInt32(0);
                    Connection.Off();
                    if (chek != 0){
                        butlog.Cursor = Cursors.Arrow;
                        ettentions.Text = "Сonnection is ok, you can enter online mod by using this account";
                        UserName.Text = login.Text;
                        bdrWelcomeMessage.Visibility = Visibility.Visible;
                    }
                    else{
                        butlog.Cursor = Cursors.Arrow;
                        ettention.Content = "wrong username / password";
                    }
                }
                else {
                    butlog.Cursor = Cursors.Arrow;
                    ettention.Content = "login string is empty!"; 
                }
            }
            catch (System.InvalidOperationException){
                butlog.Cursor = Cursors.Arrow;
                ettentions.Text = "Сonnection is not established, but you can enter offline mod by using entered username";
                UserName.Text = login.Text;
                bdrWelcomeMessage.Visibility = Visibility.Visible;
            }  
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e){
            wpfsnake.MainWindow.Playername = UserName.Text;
            GameWindow gameWindow = new GameWindow();
            gameWindow.Show();
            this.Close();
        }
        private void BtnNO_Click(object sender, RoutedEventArgs e){
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e){
            try { this.DragMove();}
            catch (System.InvalidOperationException) { }
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e){
            this.Close();
        }
    }
}
