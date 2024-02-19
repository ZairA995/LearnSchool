using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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


namespace LearnSchool
{
    /// <summary>
    /// Логика взаимодействия для ShowOrders.xaml
    /// </summary>
    public partial class ShowOrders : Window
    {
        BD database = new BD(); //экземпляр класса с подключением к БД
        DataModel[] lists; //массив, в котором будут храниться данные из базы
        //получает путь к каталогу с изображениями (каталог находится в папке с проектом)
        public string DestinationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Catalog";
        public ShowOrders()
        {
            InitializeComponent();
            //запрос на получение данных
            lists = GetCatalog("select concat(FirstName,' ',LastName,' ',Patronymic),Email,Phone,StartTime,Title " +
                "from Client, Service, ClientService " +
                "where Client.ID = ClientService.ClientID and Service.ID = ClientService.ServiceID " +
                "and DATEDIFF(day, StartTime, getdate()) < 2 order by StartTime").ToArray();
            catalog.ItemsSource = lists;
            foreach (var item in lists)
            {
                int time = item.DateDiffMinute;
                if (time > 59) //если осталось больше 59 минут
                {
                    //MessageBox.Show((time / 60) + " часов " + (time - ((time / 60)*60)) + " минут").ToString();
                    item.DateDiffHour = time / 60;
                    item.DateDiffMinute = time - ((time / 60) * 60);
                }
                else if (item.DateDiffMinute < 59)
                {
                   // MessageBox.Show("0 часов " + (time - ((time / 60) * 60)) + " минут").ToString();
                    item.DateDiffHour = 0;
                    item.DateDiffMinute = time - ((time / 60) * 60);
                }
            }
            

            current_rows.Text = lists.Length.ToString();
            total_rows.Text = GetTotalRows().ToString();
        }

        private int GetTotalRows()
        {
            lists = GetCatalog("select concat(FirstName,' ',LastName,' ',Patronymic),Email,Phone,StartTime,Title " +
                "from Client, Service, ClientService " +
                "where Client.ID = ClientService.ClientID and Service.ID = ClientService.ServiceID").ToArray();
            return lists.Length;
        }
        public IEnumerable<DataModel> GetCatalog(string sql)
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
                        var StartTim = read.GetDateTime(3);
                        DataModel result = new DataModel()
                        {
                            FIO = read.GetString(0),
                            Email = read.GetString(1),
                            Phone = read.GetString(2),
                            StartTime = StartTim.ToString("yyyy-MM-dd HH:mm"),
                            Title = read.GetString(4),
                            DateDiffHour = (int)Math.Floor(Math.Abs((DateTime.Now - StartTim).TotalHours)),
                            DateDiffMinute = (int)Math.Floor(Math.Abs((DateTime.Now - StartTim).TotalMinutes))
                        };

                        yield return result;
                    }
                }
            }
        }
    }
}
