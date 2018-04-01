using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace ColleageSchedule
{
    class Time
    {
        public static string[] dayString = { "월", "화", "수", "목", "금", "토" };
        public static string[] timeString = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E" };
        public static int dayStringLength = dayString.Length;
        public static int timeStringLength = timeString.Length;

        public UInt16[] time;     
        public List<TextBlock> timeTextBox;
        public Time()
        {
            time = new ushort[dayStringLength];
            timeTextBox = new List<TextBlock>();
        }
        public void InitTextBox(int row, int colum, int span)
        {
            TextBlock temp = new TextBlock();
            temp.SetValue(Grid.RowProperty, row + 1);
            temp.SetValue(Grid.ColumnProperty, colum + 1);
            temp.SetValue(Grid.RowSpanProperty, span);
            temp.TextAlignment = TextAlignment.Center;
            timeTextBox.Add(temp);
        }
        public void SetText(string name, string professor, byte r, byte g, byte b)
        {
            for (int i = 0; i < timeTextBox.Count; i++)
            {
                timeTextBox[i].Text = name + "\n" + professor;
                timeTextBox[i].Background = new SolidColorBrush(Color.FromRgb(r, g, b));
            }
        }
        public new String ToString()
        {
            String text = "";

            for (int q = 0; q < time.Length; q++)
            {
                if (time[q] != 0)
                {
                    text += dayString[q];
                    for (int i = 0; i < timeStringLength; i++)
                    {
                        if ((time[q] & (UInt16)(1 << i)) != 0)
                        {
                            text += timeString[i];
                        }
                    }
                    text += " ";
                }
            }

            return text;
        }
        public void SetTimeFromText(string dayText)
        {
            string[] timeText = dayText.Split(' ');
            for (int j = 0; j < timeText.Length; j++)
            {
                int day;
                for (day = 0; day < dayStringLength && !timeText[j].Contains(Time.dayString[day]); day++) { }
                if (day == dayStringLength)
                    continue;
                
                int time=0, span = 0;
                for (int e = 0; e < timeStringLength; e++)
                {
                    if (timeText[j].Contains(timeString[e]))
                    {
                        time += 1 << e;
                        span++;
                    }
                    else if (span != 0)
                    {
                        InitTextBox(e - span - 1, day, span);
                        span = 0;
                    }
                }
                if (span != 0)
                    InitTextBox(timeStringLength - span - 1, day, span);
                this.time[day] = (UInt16)time;
            }
        }
    }

    [Serializable()]
    class Lecture
    {
        static Random rand = new Random();

        public bool Exception { get; set; }
        public string Name { get; set; }
        public string Professor { get; set; }
        public int Grade { get; set; }
        public string TimeString { get; set; }
        [NonSerialized]
        public List<Time> timeList;
        public Lecture(string _name, string _professor, int _grade,String timeString)
        {
            Exception = false;
            Name = _name;
            Professor = _professor;
            Grade = _grade;
            TimeString = timeString;
            SetTime();
        }
        private void VaildText()
        {
            TimeString = "";
            for (int i = 0; i < timeList.Count; i++)
            {
                TimeString += timeList[i].ToString();
                if (i != timeList.Count - 1)
                    TimeString += " / ";
            }
        }
        public void SetTime()
        {
            timeList = new List<Time>();
            string[] timeText = TimeString.Split('/');
            byte r= (byte)rand.Next(200), g= (byte)rand.Next(200), b= (byte)rand.Next(200);
            for (int i = 0; i < timeText.Length; i++)
            {
                timeText[i].Trim();
                if (timeText[i].Length > 2)
                {
                    Time ttt = new Time();
                    ttt.SetTimeFromText(timeText[i]);
                    if (ttt.timeTextBox.Count != 0)
                    {
                        ttt.SetText(Name, Professor, r, g, b);
                        timeList.Add(ttt);
                    }
                }
            }
            VaildText();
        }
    }

    class Schedule
    {
        public int grades;
        public UInt16 lessonDay;
        public List<int[]> lectureIndex;
    }

    public class CustomGird : Grid
    {
        Pen line = new Pen(Brushes.Black, 1);

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawRectangle(null, line, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            double height = 0;
            foreach (var r in this.RowDefinitions)
            {
                height += r.ActualHeight;
                dc.DrawLine(line, new Point(0, height), new Point(this.ActualWidth, height));
            }
            double width = 0;
            foreach (var c in this.ColumnDefinitions)
            {
                width += c.ActualWidth;
                dc.DrawLine(line, new Point(width, this.ActualHeight), new Point(width, 0));
            }
        }
    }

    public partial class MainWindow : Window
    {

        List<Lecture> lectureList = new List<Lecture>();
        List<Schedule> scheduleList = new List<Schedule>();

        public MainWindow()
        {
            InitializeComponent();

            InitGrid();

            LectureListBox.DataContext = lectureList;

            GradeBox.ItemsSource = new int[] { 1, 2, 3, 4 };
            minGradeBox.ItemsSource = new int[]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 }; 
        }

        private void InitGrid()
        {
            for (int i = 0; i < Time.timeString.Length; i++)
            {
                RowDefinition row = new RowDefinition();
                ResultSchedule.RowDefinitions.Add(row);
            }

            for (int j = 0; j < Time.dayString.Length+1; j++)
            {
                ColumnDefinition col = new ColumnDefinition();
                ResultSchedule.ColumnDefinitions.Add(col);
            }

            ClearGrid();
        }

        public void ClearGrid()
        {
            ResultSchedule.Children.Clear();
            for (int i = 1; i < Time.timeString.Length; i++)
            {
                TextBlock temp = new TextBlock();
                temp.Text = Time.timeString[i] + "교시";
                temp.SetValue(Grid.RowProperty, i);
                temp.SetValue(Grid.ColumnProperty, 0);
                temp.VerticalAlignment = VerticalAlignment.Center;
                temp.HorizontalAlignment = HorizontalAlignment.Center;
                ResultSchedule.Children.Add(temp);
            }

            for (int i = 1; i < Time.dayString.Length + 1; i++)
            {
                TextBlock temp = new TextBlock();
                temp.Text = Time.dayString[i - 1] + "요일";
                temp.SetValue(Grid.RowProperty, 0);
                temp.SetValue(Grid.ColumnProperty, i);
                temp.VerticalAlignment = VerticalAlignment.Center;
                temp.HorizontalAlignment = HorizontalAlignment.Center;
                ResultSchedule.Children.Add(temp);
            }
        }

        public void InputBoxClear()
        {
            NameBox.Text = "";
            ProfessorBox.Text = "";
            TimeBox.Text = "";
        }

        private void OnClickAddButton(object sender, RoutedEventArgs e)
        {
            Lecture lecture = new Lecture(NameBox.Text, ProfessorBox.Text, GradeBox.SelectedIndex + 1, TimeBox.Text);
            if (lecture.timeList.Count == 0)
            {
                MessageBox.Show("시간을 ( 제대로 ) 입력해주세요.");
            }
            else
            {
                lectureList.Add(lecture);
                InputBoxClear();
                LectureListBox.Items.Refresh();
                CreateSchedule();
            }
        }

        private void MakeTextBoxListFromTimeData(Schedule timeData)
        {
            for (int i = 0; i < timeData.lectureIndex.Count; i++)
            {
                Time temp = lectureList[timeData.lectureIndex[i][0]].timeList[timeData.lectureIndex[i][1]];
                for (int j = 0; j < temp.timeTextBox.Count; j++)
                    ResultSchedule.Children.Add(temp.timeTextBox[j]);
            }
        }

        private void OnInputTextForGradeBox(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if ((Key.D0 <= e.Key && e.Key <= Key.D9) || (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9))
            {
                if ((sender as TextBox).Text.Length < 2)
                    e.Handled = false;
            }

            if (Key.Back == e.Key)
            {
                e.Handled = false;
            }
        }

        private void CreateSchedule()
        {
            scheduleList.Clear();
            for (int i = 0; i < lectureList.Count; i++)
            {
                if (!lectureList[i].Exception)
                {
                    MakeSchedule(new Time(), new List<int[]>(), 0, new int[Time.dayStringLength], i, lectureList.Count);
                }
            }
            scheduleList.Sort((x, y) => x.grades.CompareTo(y.grades));
            RefreshResultList();
        }

        private void RefreshResultList()
        {
            if (ResultList == null) return;
            ResultList.Items.Clear();
            ClearGrid();
            int lessonDayCount = lessonDayCountBox.SelectedIndex;
            int minGrade = minGradeBox.SelectedIndex;
            for (int i = 0; i < scheduleList.Count; i++)
            {
                if (scheduleList[i].lessonDay == lessonDayCount || lessonDayCount == 0)
                {
                    if (minGrade <= scheduleList[i].grades)
                        ResultList.Items.Add(i.ToString());
                }
            }
        }

        private void MakeSchedule(Time test, List<int[]> testTime, int grades, int[] holiday, int start, int end)
        {
            if (start == end) return;

            if (lectureList[start].Exception)
            {
                MakeSchedule(test, testTime, grades, holiday, start + 1, end);
                return;
            }

            for (int i = 0; i < lectureList[start].timeList.Count; i++)
            {
                bool can = true;
                for (int t = 0; t < Time.dayStringLength; t++)
                    if ((test.time[t] & lectureList[start].timeList[i].time[t]) != 0)
                        can = false;

                if (can)
                {
                    for (int t = 0; t < Time.dayStringLength; t++)
                    {
                        test.time[t] = (UInt16)(test.time[t] | lectureList[start].timeList[i].time[t]);
                        if (lectureList[start].timeList[i].time[t] != 0)
                            holiday[t]++;
                    }
                    int[] temp = { start, i };
                    grades += lectureList[start].Grade;

                    testTime.Add(temp);

                    AddSchedule(grades, holiday, testTime);

                    MakeSchedule(test, testTime, grades, holiday, start + 1, end);

                    testTime.RemoveAt(testTime.Count - 1);
                    grades -= lectureList[start].Grade;
                    for (int t = 0; t < Time.dayStringLength; t++)
                    {
                        test.time[t] = (UInt16)(test.time[t] ^ lectureList[start].timeList[i].time[t]);
                        if (lectureList[start].timeList[i].time[t] != 0)
                            holiday[t]--;
                    }
                }
                else
                {
                    MakeSchedule(test, testTime, grades, holiday, start + 1, end);
                }
            }
        }

        private void AddSchedule(int grades, int[] lessonDay, List<int[]> lecture)
        {
            Schedule schedule = new Schedule
            {
                grades = grades
            };

            UInt16 temp = 0;
            for (int i = 0; i < lessonDay.Length; i++)
            {
                if (lessonDay[i] != 0) temp++;
            }
            schedule.lessonDay = temp;

            schedule.lectureIndex = new List<int[]>();
            for (int g = 0; g < lecture.Count; g++)
            {
                schedule.lectureIndex.Add(lecture[g]);
            }
            scheduleList.Add(schedule);
        }

        private void SelectItem(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox).Items.IsEmpty) return;

            ClearGrid();

            int index = Int32.Parse((string)((sender as ListBox).SelectedItem));
            MakeTextBoxListFromTimeData(scheduleList[index]);
            GradePrintBox.Text = scheduleList[index].grades + " 학점";
        }

        private void OptionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshResultList();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.DataContext is Lecture)
            {
                ((Lecture)checkBox.DataContext).Exception = checkBox.IsChecked.Value;
            }
            CreateSchedule();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.DataContext is Lecture)
            {
                lectureList.Remove((Lecture)button.DataContext);
            }
            LectureListBox.Items.Refresh();
            CreateSchedule();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("저장하지않은 내용은 지워집니다.", "새로 만들기", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                lectureList.Clear();
                scheduleList.Clear();

                ClearGrid();
                ResultList.Items.Clear();
                LectureListBox.Items.Refresh();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "lecture List |*.lec";
            saveFileDialog1.Title = "Save Lecture List";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                FileStream fs = (FileStream)saveFileDialog1.OpenFile();

                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(fs, lectureList);

                fs.Close();
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {      
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "lecture List |*.lec";
            openFileDialog1.Title = "Load Lecture List";
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName != "")
            {
                lectureList.Clear();
                scheduleList.Clear();

                ClearGrid();
                ResultList.Items.Clear();
                LectureListBox.Items.Refresh();

                FileStream fs = (FileStream)openFileDialog1.OpenFile();

                BinaryFormatter bin = new BinaryFormatter();
                lectureList = (List<Lecture>)bin.Deserialize(fs);

                fs.Close();
            }
            for (int i = 0; i < lectureList.Count; i++)
            {
                lectureList[i].SetTime();
            }
            LectureListBox.DataContext = lectureList;
            LectureListBox.Items.Refresh();
            CreateSchedule();
        }

        private void LectureListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }

    }
}
