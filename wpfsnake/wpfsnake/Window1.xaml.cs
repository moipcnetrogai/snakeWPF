using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace wpfsnake{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window{
        public Window1(){
            InitializeComponent();
        }
        private void Back_Click(object sender, RoutedEventArgs e){
            MainWindow mainwindow = new MainWindow();
            mainwindow.Show();
            this.Close();
        }
        private void Setdata_Click(object sender, RoutedEventArgs e)
        {
            setdata.Cursor = Cursors.Wait;
            try {
                if ((password.Text == repeatpassword.Text) && (login.Text != ""))
                {
                    string hash = GetMD.Getmd(GetMD.Getmd(GetMD.Getmd(GetMD.Getmd(password.Text))));
                    string sqlchek = "Select id from userdata where username='" + login.Text + "'";
                    string insert = "insert into userdata (username,password) values ('{0}','{1}')";
                    string formatedinsert = string.Format(insert, login.Text, hash);
                    SqlCommand ins = new SqlCommand(formatedinsert, Connection.MyConnection);
                    SqlCommand cmd = new SqlCommand(sqlchek, Connection.MyConnection);
                    Connection.On(0);
                    SqlDataReader rd = cmd.ExecuteReader();
                    int chek = 0;
                    while (rd.Read())
                        chek = rd.GetInt32(0);
                    if (chek == 0)
                    {
                        setdata.Cursor = Cursors.Arrow;
                        ins.ExecuteNonQuery();
                        Connection.Off();
                        bdrWelcomeMessage.Visibility = Visibility.Visible;
                        UserName.Text = login.Text;
                    }
                    else
                    {
                        setdata.Cursor = Cursors.Arrow;
                        Connection.Off();
                        ettention.Content = "This username is taken";
                    }
                }
                else
                {
                    setdata.Cursor = Cursors.Arrow;
                    ettention.Content = "Passwords must match";
                }

            }
            catch (System.InvalidOperationException) {
                setdata.Cursor = Cursors.Arrow;
                ettentions.Text = "No connection established, but you can use offline mod from login menu";
                bdrEttentionMessage.Visibility = Visibility.Visible;
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e){
            try { this.DragMove(); }
            catch (System.InvalidOperationException) { }
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e){
            this.Close();
        }
        private void BtnOKet_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainwindow = new MainWindow();
            mainwindow.Show();
            this.Close();
        }
        private void BtnNOet_Click(object sender, RoutedEventArgs e)
        {
            bdrEttentionMessage.Visibility = Visibility.Collapsed;
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e){
            MainWindow mainwindow = new MainWindow();
            mainwindow.Show();
            this.Close();
        }
    }
}
