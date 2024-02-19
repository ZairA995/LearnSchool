using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LearnSchool
{
    /// <summary>
    /// Логика взаимодействия для NewOrderWindow.xaml
    /// </summary>
    public partial class NewOrderWindow : Window
    {
        BD database = new BD(); //экземпляр класса с подключением к Б
        public string query;
        public int idService;
        public int idClient;
        public int hour;
        public int minute;
        public NewOrderWindow(string title)
        {
            InitializeComponent();
            dateText.DisplayDateStart = DateTime.Now;

            DataTable category = GetData("select concat(FirstName,' ',LastName,' ',Patronymic) as result from Client");
            if (category.Rows.Count > 0)
            {
                FIOText.ItemsSource = category.DefaultView;
                FIOText.DisplayMemberPath = "result";
                FIOText.SelectedIndex = 0;
            }

            titleText.Text = title;
            dateText.SelectedDate = DateTime.Now.Date;
            //загрузка данных по названию курса 
            query = $"select ID,Title, DurationInSeconds from Service where Title = '{titleText.Text}'";
            GetCatalog(query);
        }

        private DataTable GetData(string query)
        {
            DataTable data = new DataTable();
            SqlCommand command = new SqlCommand(query, database.Connection());
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(data);
            return data;
        }

        public void GetCatalog(string sql)
        {
            using (var conn = database.Connection())
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader read = cmd.ExecuteReader();
                using (read)
                {
                    while (true)
                    {
                        if (read.Read() == false) break;
                        idService = read.GetInt32(0);
                        titleText.Text = read.GetString(1); 
                        durationText.Text = read.GetInt32(2).ToString();
                    }
                }
            }
        }

        private void saveBut_Click(object sender, RoutedEventArgs e)
        {
            DataTable data = GetData($"select ID from Client where concat(FirstName,' ',LastName,' ',Patronymic) = '{FIOText.Text}'");
            if (data.Rows.Count > 0)
            {
                idClient = int.Parse(data.Rows[0][0].ToString());
            }
            
            if(hour == 0 && minute == 0) //если время не было выбрано, установить текущее
            {
                hour = DateTime.Now.Hour;
                minute = DateTime.Now.Minute;
            }
            var date = new DateTime(dateText.SelectedDate.Value.Year, dateText.SelectedDate.Value.Month, dateText.SelectedDate.Value.Day, hour, minute,0);

            
            string query = "insert into ClientService(ClientID,ServiceID,StartTime) values(@clientId,@serviceId,@startTime)";
            using (database.Connection())
            {
                using (SqlCommand command = new SqlCommand(query, database.Connection()))
                {
                    command.Parameters.Add("@clientId", SqlDbType.Int).Value = idClient;
                    command.Parameters.Add("@serviceId", SqlDbType.Int).Value = idService;
                    command.Parameters.Add("@startTime", SqlDbType.DateTime).Value = date;

                    int rows = command.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        MessageBox.Show("Добавлена новая запись");
                        this.Close();
                    }
                    else
                        MessageBox.Show("Ошибка!");
                }
            }
        }

        private void timeText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ":")
               && (!titleText.Text.Contains(":")
               && titleText.Text.Length != 0)))
            {
                e.Handled = true;
            }
        }

        private void timeText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(timeText.Text, @"^\d\d:\d\d$"))
            {
                MessageBox.Show("Неверный формат времени");
            }
            else
            {
                var paths = timeText.Text.Split(':'); //разделение строки на две части по :
                hour = int.Parse(paths[0]);
                minute = int.Parse(paths[1]);
                if (hour >= 24 || minute >= 60)
                {
                    MessageBox.Show("Некорректное время");
                    timeText.BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else
                    timeText.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
        }
    }
}
