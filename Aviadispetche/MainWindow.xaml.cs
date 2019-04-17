using System;
using System.Collections.Generic;
using System.Windows;
using MySql.Data.MySqlClient;




namespace Aviadispetcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connStr;
        public List<Flight> fList = new List<Flight>(85);
        int flightNum;
        bool flightAdd = false;

        private void OpenDbFile()
        {
            try
            {
                connStr = "Server = 127.0.0.1; Database = aviadispetcher; Uid = root; Pwd = ;";
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = new MySqlCommand();
                string commandString = "SELECT * FROM rozklad;";
                command.CommandText = commandString;
                command.Connection = conn;
                MySqlDataReader reader;
                command.Connection.Open(); //тут проблема
                reader = command.ExecuteReader();
                int i = 0;
                while (reader.Read())
                {
                    fList.Add(new Flight((string)reader["number"], (string)reader["city"],
                        (System.TimeSpan)reader["depature_time"], (int)reader["free_seats"]));
                    i += 1;
                }
                reader.Close();
                FlightListDG.ItemsSource = fList;

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message + char.ConvertFromUtf32(13)+
                    char.ConvertFromUtf32(13) + "Для завантаження файлу " +
                    "виконайте команду Файл-Завантажити","Помилка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FlightListDG.ItemsSource = null;
                fList.Clear();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + char.ConvertFromUtf32(13),
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            OpenDbFile();
        }

        private void InfoFlightForm_Loaded(object sender, RoutedEventArgs e)
        {
            OpenDbFile();
            groupBox1.Visibility = Visibility.Hidden;

            this.Width = FlightListDG.Margin.Left + FlightListDG.RenderSize.Width + 50;
            this.Height = FlightListDG.Margin.Top + FlightListDG.RenderSize.Height + 50;

            numFlightGroupBox.Visibility = Visibility.Hidden;
        }

        private void EditDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            numFlightGroupBox.Visibility = Visibility.Visible;

            this.Width = numFlightGroupBox.Margin.Left + numFlightGroupBox.RenderSize.Width + 20;
            this.Height = FlightListDG.Margin.Top + FlightListDG.RenderSize.Height + 50 +
                          numFlightGroupBox.Margin.Top + numFlightGroupBox.RenderSize.Height + 20;

            flightAdd = false;
        }

        private void FlightListDG_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Flight editerFlight = FlightListDG.SelectedItem as Flight;
            try
            {
                numFlightTextBox.Text = editerFlight.Number;
                cityFlightTextBox.Text = editerFlight.City;
                timeFlightTextBox.Text = editerFlight.Departure_time.ToString(@"hh\:mm");
                freeSeatsTextBox.Text = editerFlight.Free_seats.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + char.ConvertFromUtf32(13) + char.ConvertFromUtf32(13), "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeFlightListData(int num)
        {
            TimeSpan depTime;
            if (flightAdd)
            {
                fList.Add(new Flight("","",TimeSpan.Zero, 0));
            }

            fList[num].Number = numFlightTextBox.Text;
            fList[num].City = cityFlightTextBox.Text;
            if (TimeSpan.TryParse(timeFlightTextBox.Text, out depTime))
            {
                fList[num].Departure_time = depTime;
            }

            fList[num].Free_seats = Convert.ToInt16(freeSeatsTextBox.Text);

            FlightListDG.ItemsSource = null;
            FlightListDG.ItemsSource = fList;
            if (flightAdd)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connStr))
                    using (MySqlCommand cmd =
                        new MySqlCommand(
                            "INSERT INTO rozklad (Number, City, Depature_time, Free_seats) VALUES (?,?,?,?)",
                            conn))
                    {
                        cmd.Parameters.Add("@number", MySqlDbType.VarChar, 6).Value = numFlightTextBox.Text;
                        cmd.Parameters.Add("@city", MySqlDbType.VarChar, 25).Value = cityFlightTextBox.Text;
                        cmd.Parameters.Add("@depature_time", MySqlDbType.Time).Value = depTime;
                        cmd.Parameters.Add("@free_seats", MySqlDbType.Int16, 4).Value =
                            Convert.ToInt16(freeSeatsTextBox.Text);
                        cmd.Parameters.Add("@id", MySqlDbType.Int16, 11).Value = num + 1;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = "";
                    if (ex.Message == "Unable to connect to any of the specified MySQL hosts.")
                    {
                        errMsg = "Підключення веб-сервер MySQL та завантажте дані командою Файл-Завантажити";
                    }
                    else
                    {
                        errMsg = "Для завантаження даних виконайте команду Файл-Завантаджити";
                    }

                    MessageBox.Show(ex.Message + char.ConvertFromUtf32(13) + char.ConvertFromUtf32(13) + errMsg,
                        "Помика",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connStr))
                    using (MySqlCommand cmd =
                        new MySqlCommand(
                            "UPDATE rozklad SET number = ?, city = ?, depature_time = ?, free_seats = ? WHERE id = ?",
                            conn))
                    {
                        cmd.Parameters.Add("@number", MySqlDbType.VarChar, 6).Value = numFlightTextBox.Text;
                        cmd.Parameters.Add("@city", MySqlDbType.VarChar, 25).Value = cityFlightTextBox.Text;
                        cmd.Parameters.Add("@depature_time", MySqlDbType.Time).Value = depTime;
                        cmd.Parameters.Add("@free_seats", MySqlDbType.Int16, 4).Value =
                            Convert.ToInt16(freeSeatsTextBox.Text);
                        cmd.Parameters.Add("@id", MySqlDbType.Int16, 11).Value = num + 1;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = "";
                    if (ex.Message == "Unable to connect to any of the specified MySQL hosts.")
                    {
                        errMsg = "Підключення веб-сервер MySQL та завантажте дані командою Файл-Завантажити";
                    }
                    else
                    {
                        errMsg = "Для завантаження даних виконайте команду Файл-Завантаджити";
                    }

                    MessageBox.Show(ex.Message + char.ConvertFromUtf32(13) + char.ConvertFromUtf32(13) + errMsg,
                        "Помика",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
 
        }
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeFlightListData(flightNum);
        }

        private void AddDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            numFlightGroupBox.Visibility = Visibility.Visible;

            this.Width = numFlightGroupBox.Margin.Left + numFlightGroupBox.RenderSize.Width + 20;
            this.Height = FlightListDG.Margin.Top + FlightListDG.RenderSize.Height + 50 +
                          numFlightGroupBox.Margin.Top + numFlightGroupBox.RenderSize.Height + 20;

            flightAdd = true;

            flightNum = fList.Count;
        }

        private void FillCityList()
        {
            bool nameExist = false;
            cityList.Items.Add(fList[0].City);

            for (int i = 1; i < fList.Count; i++) //ошибка
            {
                for (int j = 0; j < cityList.Items.Count; j++)
                {
                    if (cityList.Items[j].ToString() == fList[i].City)
                    {
                        nameExist = true;
                    }
                }

                if (!nameExist)
                {
                    cityList.Items.Add(fList[i].City);
                }

                nameExist = false;
            }
        }

        private void SelectXMenuItem_Click(object sender, RoutedEventArgs e)
        {
            groupBox1.Visibility = Visibility.Visible;

            this.Width = 1200000;
            this.Height = 290;
            cityList.Items.Clear();
            FillCityList();
        }
    }
}
