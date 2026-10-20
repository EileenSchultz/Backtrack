using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BacktrackVisualizer
{
    public partial class MainWindow : Window
    {
        private PermutationsBacktrack _algorithm = new PermutationsBacktrack();
        private DispatcherTimer _timer;
        
        // Теперь мы используем перечислитель (Enumerator) вместо списка
        private IEnumerator<StepInfo> _stepEnumerator; 
        private int[] _inputArray;
        private int _foundPermutationsCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            
            // Разрешаем вводить до 12
            if (!int.TryParse(TxtN.Text, out int n) || n < 1 || n > 12)
            {
                MessageBox.Show("Введите число от 1 до 12");
                return;
            }

            if (BtnStart.Content.ToString() == "Продолжить" && _stepEnumerator != null && _inputArray != null && _inputArray.Length == n)
            {
                TxtStatus.Text = "Продолжение...";
                TxtStatus.Foreground = Brushes.Blue;
                
                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = true;
                
                _timer.Start();
                return;
            }

            // --- НОВЫЙ ЗАПУСК ---
            _inputArray = new int[n];
            for (int i = 0; i < n; i++) _inputArray[i] = i + 1;

            // Получаем итератор
            _stepEnumerator = _algorithm.GenerateSteps(_inputArray).GetEnumerator();

            // Настраиваем скорость анимации
            if (n >= 9)
                _timer.Interval = TimeSpan.FromMilliseconds(1); // Очень быстро для 9, 10, 11, 12
            else if (n >= 7)
                _timer.Interval = TimeSpan.FromMilliseconds(10); // Быстро для 7, 8
            else
                _timer.Interval = TimeSpan.FromMilliseconds(600); // Нормально для 1-6

            // Сбрасываем UI
            _foundPermutationsCount = 0;
            LstResults.Items.Clear();
            PnlCurrentPath.Children.Clear();
            TxtStatus.Text = "Старт...";
            TxtStatus.Foreground = Brushes.Blue;
            DrawOriginalArray(new bool[n]);

            BtnStart.Content = "Запустить";
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;

            _timer.Start();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            
            TxtStatus.Text = $"Остановлено. Найдено перестановок: {_foundPermutationsCount}";
            TxtStatus.Foreground = Brushes.DarkRed;
            
            BtnStart.Content = "Продолжить";
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Получаем следующий шаг "на лету"
            if (_stepEnumerator == null || !_stepEnumerator.MoveNext())
            {
                _timer.Stop();
                TxtStatus.Text = $"Готово! Всего перестановок: {_foundPermutationsCount}";
                TxtStatus.Foreground = Brushes.Green;
                
                BtnStart.Content = "Запустить";
                BtnStart.IsEnabled = true;
                BtnStop.IsEnabled = false;
                return;
            }

            var step = _stepEnumerator.Current;

            if (step.Action == "Choose")
            {
                TxtStatus.Text = $"Выбор (Choose): {step.Value}";
                TxtStatus.Foreground = Brushes.Blue;
            }
            else if (step.Action == "Undo")
            {
                TxtStatus.Text = $"Отмена (Undo): {step.Value}";
                TxtStatus.Foreground = Brushes.OrangeRed;
            }
            else if (step.Action == "BaseCase")
            {
                _foundPermutationsCount++;
                
                // Если чисел много, мы не забиваем список строчками, чтобы не завис интерфейс
                if (_inputArray.Length <= 7)
                {
                    TxtStatus.Text = $"Успех! Добавлена перестановка: [{string.Join(", ", step.Current)}]";
                    LstResults.Items.Add($"[{string.Join(", ", step.Current)}]");
                    LstResults.ScrollIntoView(LstResults.Items[LstResults.Items.Count - 1]);
                }
                else
                {
                    // Просто показываем счетчик
                    TxtStatus.Text = $"Найдено перестановок: {_foundPermutationsCount}";
                }
                TxtStatus.Foreground = Brushes.Green;
            }

            DrawOriginalArray(step.Used);
            DrawCurrentPath(step.Current);
        }

        private void DrawOriginalArray(bool[] used)
        {
            PnlOriginalArray.Children.Clear();
            for (int i = 0; i < _inputArray.Length; i++)
            {
                Border border = new Border
                {
                    Width = 30, Height = 30, // Чуть уменьшили, чтобы 12 влезло на экран
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(5),
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Background = used[i] ? Brushes.LightGray : Brushes.White
                };

                TextBlock text = new TextBlock
                {
                    Text = _inputArray[i].ToString(),
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = used[i] ? Brushes.Gray : Brushes.Black
                };

                border.Child = text;
                PnlOriginalArray.Children.Add(border);
            }
        }

        private void DrawCurrentPath(List<int> current)
        {
            PnlCurrentPath.Children.Clear();
            for (int i = 0; i < current.Count; i++)
            {
                Border border = new Border
                {
                    Width = 30, Height = 30,
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(5),
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Blue,
                    Background = Brushes.LightBlue
                };

                TextBlock text = new TextBlock
                {
                    Text = current[i].ToString(),
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                border.Child = text;
                PnlCurrentPath.Children.Add(border);
            }
        }
    }
}