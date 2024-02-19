using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для AddUpdateWindow.xaml
    /// </summary>
    public partial class AddUpdateWindow : Window
    {
        BD database = new BD(); //экземпляр класса с подключением к Б
        public bool change_photo = false;  //флаг для проверки изменяемости фото
        public string query;
        public bool titleGood = true; //флаг для проверки уникальности названия курса
        public string status; //строка для проверки выполняемой функции (изменить или добавить)
        public bool isFinish = false;

        //получает путь к каталогу с изображениями (каталог находится в папке с проектом)
        public string DestinationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Catalog";
        public AddUpdateWindow()
        {
            InitializeComponent();
            status = "add";
            if(titleText.Text == "")
            {
                idCourse.Visibility = Visibility.Collapsed;
                idText.Visibility = Visibility.Collapsed;
            }
            //получение максимального id курса
            idText.Text = (getMaxId("select max(ID) from Service") + 1).ToString();

        }

        public AddUpdateWindow(string title)
        {
            InitializeComponent();
            status = "update";
            titleText.Text = title;
            //загрузка данных по названию курса
            query = $"select ID, Title, MainImagePath, Cost, DurationInSeconds, Discount, Description from Service where Title = '{titleText.Text}'";
            GetCatalog(query);
        }

        public bool Finish
        {
            get { return isFinish; }
        }
        /// <summary>
        /// Получение данных из базы
        /// </summary>
        /// <param name="sql">Строка запроса</param>
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
                        idText.Text = read.GetInt32(0).ToString();
                        titleText.Text = read.GetString(1);

                        BitmapImage logo = new BitmapImage();
                        if (!read.IsDBNull(2))
                        {
                            logo.BeginInit();
                            logo.UriSource = new Uri(Path.Combine(DestinationPath, read.GetString(2)));
                            logo.EndInit();
                            FilePath = read.GetString(2);
                        }
                        image.Source = logo;

                        costText.Text = ((short)read.GetDecimal(3)).ToString();
                        durationText.Text = read.GetInt32(4).ToString();
                        discountText.Text = (read.IsDBNull(4) ? 0 : read.GetDouble(5)).ToString();
                        descriptionText.Text = (read.IsDBNull(6) ? "" : read.GetString(6)).ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Получение максимального id курса
        /// </summary>
        /// <param name="query">Строка запроса</param>
        /// <returns></returns>
        private int getMaxId(string query)
        {
            int result = -1;
            SqlCommand command = new SqlCommand(query, database.Connection());
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable data = new DataTable();
            adapter.Fill(data);
            if (data.Rows.Count > 0)
            {
                result = int.Parse(data.Rows[0][0].ToString());
            }

            return result;
        }

        /// <summary>
        /// Проверка на уникальность названия курса
        /// </summary>
        /// <param name="query">Строка запроса</param>
        /// <returns></returns>
        private bool getSameTitle(string query)
        {
            bool result = false;
            SqlCommand command = new SqlCommand(query, database.Connection());
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable data = new DataTable();
            adapter.Fill(data);
            if (data.Rows.Count > 0)
            {
                if(status == "add")
                {
                    if (int.Parse(data.Rows[0][0].ToString()) == 0) result = false;
                    else result = true;
                }
                else
                {
                    if(int.Parse(data.Rows[0][0].ToString()) == 1) result = false;
                    else result = true;
               
                }
            }
            return result;
        }

        private void saveBut_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(titleText.Text) && titleGood && !string.IsNullOrWhiteSpace(costText.Text) && !string.IsNullOrWhiteSpace(durationText.Text)
                && !string.IsNullOrWhiteSpace(titleText.Text)) 
            {
                if(status == "add")
                {
                    addCourse();
                    isFinish = true;
                }
                else if(status == "update")
                {
                    updateCourse(idText.Text);
                    isFinish = true;
                }
                this.DialogResult = true;
            }
            else MessageBox.Show("Остались пустые поля");
        }

        public void updateCourse(string id)
        {
            string query = "update Service set Title = @title,Cost = @cost,Discount = @discont,DurationInSeconds = @duration," +
                "Description = @description,MainImagePath = @photo where ID = @id";
            using (database.Connection())
            {
                using (SqlCommand command = new SqlCommand(query, database.Connection()))
                {
                    command.Parameters.Add("@title", SqlDbType.VarChar).Value = titleText.Text;
                    command.Parameters.Add("@cost", SqlDbType.Money).Value = decimal.Parse(costText.Text);
                    command.Parameters.Add("@discont", SqlDbType.Float).Value = float.Parse(discountText.Text);
                    command.Parameters.Add("@duration", SqlDbType.Int).Value = int.Parse(durationText.Text);
                    command.Parameters.Add("@description", SqlDbType.VarChar).Value = descriptionText.Text;
                    command.Parameters.Add("@photo", SqlDbType.VarChar).Value = FilePath;
                    command.Parameters.Add("@id", SqlDbType.Int).Value = int.Parse(id);
                    int rows = command.ExecuteNonQuery();
                    if(rows > 0)
                        MessageBox.Show("Информация по курсу обновлена");
                    else
                        MessageBox.Show("Ошибка!");
                }
            }
        }

        public void addCourse()
        {
            string query = "insert into Service values(@title,@cost,@duration,@description,@discont,@photo)";
            using (database.Connection())
            {
                using (SqlCommand command = new SqlCommand(query, database.Connection()))
                {
                    command.Parameters.Add("@title", SqlDbType.VarChar).Value = titleText.Text;
                    command.Parameters.Add("@cost", SqlDbType.Money).Value = decimal.Parse(costText.Text);
                    command.Parameters.Add("@discont", SqlDbType.Float).Value = float.Parse(discountText.Text);
                    command.Parameters.Add("@duration", SqlDbType.Int).Value = int.Parse(durationText.Text);
                    command.Parameters.Add("@description", SqlDbType.VarChar).Value = descriptionText.Text;
                    if(FilePath == null)
                        command.Parameters.Add("@photo", SqlDbType.VarChar).Value = DBNull.Value;
                    else
                        command.Parameters.Add("@photo", SqlDbType.VarChar).Value = FilePath;

                    int rows = command.ExecuteNonQuery();
                    if (rows > 0)
                        MessageBox.Show("Добавлен новый курс");
                    else
                        MessageBox.Show("Ошибка!");
                }
            }
        }

        /// <summary>
        /// Загрузка выбранного изображения в папку изображений
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void load_imageBut_Click(object sender, RoutedEventArgs e)
        {
            if (OpenFileDialog())
            {
                FileInfo fileInf = new FileInfo(FullFilePath);
                BitmapImage logo = new BitmapImage();
                logo.BeginInit();
                logo.UriSource = new Uri(FullFilePath);
                logo.EndInit();
                image.Source = logo;
                fileInf.CopyTo(System.IO.Path.Combine(DestinationPath, FilePath), true);
            }
        }

        private void titleText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(titleText.Text))
            {
                query = $"select count(*) from Service where Title = '{titleText.Text}'";
                if (getSameTitle(query))
                {
                    MessageBox.Show("Курс с таким названием уже есть");
                    titleText.BorderBrush = new SolidColorBrush(Colors.Red);
                    titleGood = false;
                }
                else
                {
                    titleText.BorderBrush = new SolidColorBrush(Colors.Gray);
                    titleGood = true;
                }
            }
        }
        /// <summary>
        /// Валидация ввода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void descriptionText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "0123456789".IndexOf(e.Text) > 0;
        }

        private void discountText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "0123456789".IndexOf(e.Text) < 0;
        }

        public string FilePath { get; set; }
        public string FullFilePath { get; set; }
        public bool OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.SafeFileName;
                FullFilePath = openFileDialog.FileName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Проверяет, что длительность занятия находится в диапазоне от 30 минут до 4 часов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void durationText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(durationText.Text))
            {
                if(int.Parse(durationText.Text) < 30 || int.Parse(durationText.Text) > 240)
                { 
                    MessageBox.Show("Длительность курса должна быть не короче 30 минут и не дольше 4 часов (240 минут)");
                    durationText.BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else durationText.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            else durationText.BorderBrush = new SolidColorBrush(Colors.Red);   
        }

        private void costText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(costText.Text))
            {
                if (int.Parse(costText.Text) <= 0)
                {
                    MessageBox.Show("Стоимость занятия не может быть нулевой или отрицательной");
                    costText.BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else costText.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            else costText.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        private void discountText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(discountText.Text))
            {
                if (int.Parse(discountText.Text) < 0 || int.Parse(discountText.Text) >= 100)
                {
                    MessageBox.Show("Скидка занятия не может быть отрицательной или превышающей стоимость занятия");
                    discountText.BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else discountText.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            else discountText.Text = "0";
        }
    }
}
