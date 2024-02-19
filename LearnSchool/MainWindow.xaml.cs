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
using System.Collections.ObjectModel;
using System.IO;
using System.Data.SqlClient;

namespace LearnSchool
{
    public partial class MainWindow : Window
    {
        BD database = new BD(); //экземпляр класса с подключением к БД
        DataModel[] lists; //массив, в котором будут храниться данные из базы
        public bool change_photo = false;
        //получает путь к каталогу с изображениями (каталог находится в папке с проектом)
        public string DestinationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Catalog";

        public MainWindow()
        {
            InitializeComponent();

            //запрос на получение данных
            lists = GetCatalog("select Title, MainImagePath, Cost, DurationInSeconds, Discount from Service " +
                     "order by Cost*(100 - Discount)/100").ToArray();
            catalog.ItemsSource = lists;

            current_rows.Text = lists.Length.ToString();
            total_rows.Text = GetTotalRows().ToString();

            filtered.SelectedIndex = 5;
            sort.SelectedIndex = 1;
        }

        private int GetTotalRows()
        {
            lists = GetCatalog("select Title, MainImagePath, Cost, DurationInSeconds, Discount from Service").ToArray();
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
                        DataModel result = new DataModel()
                        {
                            Title = read.GetString(0),
                            MainImagePath = read.IsDBNull(1) ? null : Path.Combine(DestinationPath, read.GetString(1)), //тернарный оператор для возможно пустых элементов
                            Cost = ((short)read.GetDecimal(2)),
                            DurationInSeconds = read.GetInt32(3),
                            Discount = read.IsDBNull(4) ? 0 : read.GetDouble(4),
                            CostNew = (decimal)(((short)read.GetDecimal(2)) * (100 - read.GetDouble(4)) / 100)  //стоимость с учетом скидки
                        };
                        
                        yield return result;
                    }
                }
            }
        }

        private void sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void filtered_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void Filter()
        {
            int sorted_text = sort.SelectedIndex;
            int filter_text = filtered.SelectedIndex;
            var conditions = new List<string>();        //нужен, если нужно применить сразу несколько условий

            string query = "select Title, MainImagePath, Cost, DurationInSeconds, Discount from Service "; 

            switch (filter_text)
            {
                case 0:
                    conditions.Add(" Discount >= 0 and Discount < 5 ");
                    break;
                case 1:
                    conditions.Add(" Discount >= 5 and Discount < 15 ");
                    break;
                case 2:
                    conditions.Add(" Discount >= 15 and Discount < 30 ");
                    break;
                case 3:
                    conditions.Add(" Discount >= 30 and Discount < 70 ");
                    break;
                case 4:
                    conditions.Add(" Discount >= 70 and Discount < 100 ");
                    break;
                case 5:
                    break;
            }

            if(!string.IsNullOrWhiteSpace(search.Text))
            {
                conditions.Add(" Title like '%" + search.Text + "%' or Description like '%" + search.Text + "%' ");
            }

            if(conditions.Count > 0)
            {
                query += " where " + string.Join(" and ", conditions);
            }
            switch (sorted_text)
            {
                case 0:
                    query += "order by Cost*(100 - Discount)/100 ";
                    break;
                case 1:
                    query += "order by Cost*(100 - Discount)/100 desc ";
                    break;
            }

            lists = GetCatalog(query).ToArray();         //выборка по сформированному запросу
            catalog.ItemsSource = lists;

            current_rows.Text = lists.Length.ToString(); //полученное после фильтраций количество
            total_rows.Text = GetTotalRows().ToString(); //общее количество записей
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filter();
        }

        private void log_in_Click(object sender, RoutedEventArgs e)
        {
            Autorization autorization = new Autorization();
            if(autorization.ShowDialog() == true)
            {
                if(autorization.Password == "0000")
                {
                    MessageBox.Show("Администратор");
                    role.Text = "admin";
                }
                else { MessageBox.Show("Неверный пароль"); }
            }       
        }

        private void add_courseBut_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateWindow addUpdate = new AddUpdateWindow();
            if(addUpdate.ShowDialog() == true)
            {
                if(addUpdate.Finish == true)
                {
                    lists = GetCatalog("select Title, MainImagePath, Cost, DurationInSeconds, Discount from Service " +
                     "order by Cost*(100 - Discount)/100 desc").ToArray();
                    catalog.ItemsSource = lists;
                }
            }
        }

        private void update_courseBut_Click(object sender, RoutedEventArgs e)
        {
            //var tag = ((Button)sender).Tag as DataModel;
            //string name = tag.Description;
            //и тд

            var tag = Convert.ToString(((Button)sender).Tag);
            AddUpdateWindow addUpdate = new AddUpdateWindow(tag);
            if (addUpdate.ShowDialog() == true)
            {
                if (addUpdate.Finish == true)
                {
                    lists = GetCatalog("select Title, MainImagePath, Cost, DurationInSeconds, Discount from Service " +
                     "order by Cost*(100 - Discount)/100 desc").ToArray();
                    catalog.ItemsSource = lists;
                }
            }
        }

        private void delete_courseBut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void show_orderBut_Click(object sender, RoutedEventArgs e)
        {
            ShowOrders orders = new ShowOrders();
            orders.ShowDialog();
        }

        private void bor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = Convert.ToString(((Border)sender).Tag);
            NewOrderWindow window = new NewOrderWindow(tag);
            window.ShowDialog();
        }
    }
}
