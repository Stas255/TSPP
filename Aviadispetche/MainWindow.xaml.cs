using System;
using System.Collections.Generic;
using System.Windows;
using MySql.Data.MySqlClient;
using System.IO;



namespace Aviadispetcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string filePath;//папка із виконуваним файлом програми
        Microsoft.Office.Interop.Word.Application wordApp;
        Microsoft.Office.Interop.Word.Document wordDoc;
        string connStr;
        public List<Flight> fList = new List<Flight>(85);
        public List<Flight> selectedCityList = new List<Flight>();
        public List<Flight> selectedCityTimeList = new List<Flight>();
        DateTime timeFlight;
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
            this.Height = FlightListDG.Margin.Top + FlightListDG.RenderSize.Height + 50; 
        }

        private void InfoFlightForm_Loaded(object sender, RoutedEventArgs e)
        {
            OpenDbFile();
            if (Flight.logUser == 1)
            {
                menu1.Items.Remove(menu1.Items[1]);
            }
            else if (Flight.logUser == 2)
            {
                menu1.Items.Remove(menu1.Items[2]);
            }
            Button3.Visibility = Visibility.Hidden;
            groupBox1.Visibility = Visibility.Hidden;
            groupBox2.Visibility = Visibility.Hidden;
            numFlightGroupBox.Visibility = Visibility.Hidden;

            this.Width = FlightListDG.Margin.Left + FlightListDG.RenderSize.Width + 50;
            this.Height = FlightListDG.Margin.Top + FlightListDG.RenderSize.Height + 50;

        }

        private void EditDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            numFlightGroupBox.Visibility = Visibility.Visible;
            this.Height = FlightListDG.Margin.Top + FlightListDG.RenderSize.Height + 50 + 
                           numFlightGroupBox.RenderSize.Height;
            saveButton.Content = "Редагувати";
            flightAdd = false;
        }

        private void FlightListDG_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            flightNum = FlightListDG.SelectedIndex;
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

            this.Height = FlightListDG.Margin.Top + FlightListDG.RenderSize.Height + 50 +
                          numFlightGroupBox.RenderSize.Height;
            flightAdd = true;
            saveButton.Content = "Додати";
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

            this.Width = numFlightGroupBox.Margin.Left + numFlightGroupBox.RenderSize.Width + groupBox1.Width + 40;
            cityList.Items.Clear();
            FillCityList();
        }

        private List<Flight> SelectX(string cityX = "")
        {
            List<Flight> selectedList = new List<Flight>();
            selectXList.Items.Clear();
            cityX = Convert.ToString(cityList.Items[cityList.SelectedIndex]);
            int j = 0;
            for (int i = 0; i < fList.Count; i++) //???
            {
                if (cityX == fList[i].City)
                {
                    selectedList.Add(fList[i]);
                    j++;
                }
            }
            return selectedList;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string selectedCity = "";
            selectedCity = Convert.ToString(cityList.Items[cityList.SelectedIndex]);

            selectedCityList = SelectX(selectedCity);

            for (int i = 0; i < selectedCityList.Count; i++)
            {
                if (selectedCityList[i] != null)
                {
                    selectXList.Items.Add(selectedCityList[i].Number + " "
                                                                     + selectedCityList[i].Departure_time);
                }
            }
        }

        private void SelectXYMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (selectXList.Items.Count > 0)
            {
                groupBox2.Visibility = Visibility.Visible;
                Button3.Visibility = Visibility.Visible;
                this.Width = numFlightGroupBox.Margin.Left + numFlightGroupBox.RenderSize.Width + groupBox1.Width +groupBox2.Width + 40;
            }
            else
            {
                MessageBox.Show("Недостатньо даних!" + char.ConvertFromUtf32(13) +
                                             "Спочатку потрібно виконати команду"+ char.ConvertFromUtf32(13) + 
                                             "Пошук-За містом призначення", "Увага",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private List<Flight> SelectXY(DateTime DeadLine)
        {
            List<Flight> selectedList = new List<Flight>();

            DateTime[] fTime = new DateTime[selectXList.Items.Count];
            int j = 0;
            for (int i = 0; i < selectXList.Items.Count; i++)
            {
                string strList = selectXList.Items[i].ToString();
                if ((strList) != " ")
                {
                    int charNum1;
                    charNum1 = strList.IndexOf(" ");
                    strList = strList.Substring(charNum1 + 1, strList.Length - charNum1 - 1);
                    fTime[j] = DateTime.Parse(strList);
                    j++;
                }
            }

            j = 0;
            for (int i = 0; i < selectXList.Items.Count; i++)
            {
                if (DeadLine.TimeOfDay > fTime[i].TimeOfDay)
                {
                    selectedList.Add(selectedCityList[i]);
                    j++;
                }
            }
            return selectedList;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            timeFlight = Convert.ToDateTime(sTime.Text);

            selectXList1.Items.Clear();
            selectedCityTimeList = SelectXY(timeFlight);
            for (int i = 0; i < selectedCityTimeList.Count; i++)
            {
                if (selectedCityTimeList[i] != null)
                {
                    selectXList1.Items.Add(selectedCityTimeList[i].Number + " вільно " +
                                           selectedCityTimeList[i].Free_seats + " місць");
                }
            }
        }

        private void WriteData(List<Flight> selXList, List<Flight> selXYList)
        {
            filePath = Environment.CurrentDirectory.ToString();
            try
            {
                wordApp = new Microsoft.Office.Interop.Word.Application();
                wordDoc = wordApp.Documents.Add(filePath + "\\Шаблон_Пошуку_рейсів.dot");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + char.ConvertFromUtf32(13)+
                    "Недостатньо даних!" + char.ConvertFromUtf32(13) +
                                "Помістіть файл Шаблон_Пошуку_рейсів.dot" + char.ConvertFromUtf32(13) +
                                "у каталог із exe-файлом програми і повторіть збереження", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string selectedCity = cityList.SelectedItem.ToString();

            ReplaceText(selXList, 1);
            ReplaceText(selectedCity, "[X]");

            ReplaceText(selXList, 2);
            ReplaceText(selectedCity, "[Y]");

            
            wordDoc.Save();
            if (wordDoc != null)
            {
                wordDoc.Close();
            }
            if (wordApp != null)
            {
                wordApp.Quit();
            }
        }

        private void ReplaceText(string textToReplace, string replacedText)
        {
            Object missing = Type.Missing;

            Microsoft.Office.Interop.Word.Range selText;
            selText = wordDoc.Range(wordDoc.Content.Start, wordDoc.Content.End);

            Microsoft.Office.Interop.Word.Find find = wordApp.Selection.Find;
            find.Text = replacedText;
            find.Replacement.Text = textToReplace;
            Object wrap = Microsoft.Office.Interop.Word.WdFindWrap.wdFindContinue;
            Object replace = Microsoft.Office.Interop.Word.WdReplace.wdReplaceAll;

            find.Execute(FindText: Type.Missing,
                MatchCase: false,
                MatchWholeWord: false,
                MatchWildcards: false,
                MatchSoundsLike: missing,
                MatchAllWordForms: false,
                Forward: true,
                Wrap: wrap,
                Format:false,
                ReplaceWith: missing, Replace: replace);
        }

        private void ReplaceText(List<Flight> selectedLixt, int numTable)
        {
            for (int i = 0; i < selectedLixt.Count; i++)
            {
                if (selectedLixt[i] != null)
                {
                    wordDoc.Tables[numTable].Rows.Add();
                    wordDoc.Tables[numTable].Cell(2 + i, 1).Range.Text =
                        selectedLixt[i].Number;
                    wordDoc.Tables[numTable].Cell(2 + i, 2).Range.Text =
                        selectedLixt[i].Departure_time.ToString();
                    if (numTable == 2)
                    {
                        wordDoc.Tables[numTable].Cell(2 + i, 3).Range.Text =
                            selectedLixt[i].Free_seats.ToString();
                    }
                }
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            WriteData(selectedCityList,selectedCityTimeList);
        }

        private void SaveDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Button3_Click(sender, e);
        }
    }
}
