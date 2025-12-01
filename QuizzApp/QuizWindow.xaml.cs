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
using System.Windows.Shapes;
using QuizzApp.Model;

namespace QuizzApp
{
    /// <summary>
    /// Логика взаимодействия для QuizWindow.xaml
    /// </summary>
    public partial class QuizWindow : Window
    {
        private QuizDBEntities _db = new QuizDBEntities();
        private int _quizId;
        private List<Questions> _questions;
        private int _currentQuestionIndex = 0;
        private int _correctCount = 0;
        private bool _isQuizCompleted = false;

        public QuizWindow(int quizId)
        {
            InitializeComponent();
            _quizId = quizId;
            LoadQuestions();
            ShowQuestion();
        }

        private void LoadQuestions()
        {
            _questions = _db.Questions
                .Include("Options")
                .Where(q => q.QuizId == _quizId)
                .ToList();
        }

        private void ShowQuestion()
        {
            SpOptions.Children.Clear();

            if (_currentQuestionIndex >= _questions.Count)
            {
                ShowResult();
                return;
            }

            var q = _questions[_currentQuestionIndex];
            TbQuestion.Text = q.QuestionText;

            var options = q.Options.OrderBy(o => o.OptionId).ToList();
            foreach (var opt in options)
            {
                var rb = new RadioButton
                {
                    Content = opt.OptionText,
                    Tag = opt.IsCorrect,
                    Style = (Style)FindResource("AnswerRadioButton"),
                    Margin = new Thickness(0, 5, 0, 5),
                    IsEnabled = !_isQuizCompleted
                };
                SpOptions.Children.Add(rb);
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            var selected = SpOptions.Children.OfType<RadioButton>()
                .FirstOrDefault(r => r.IsChecked == true);

            if (selected == null)
            {
                MessageBox.Show("Выберите ответ!");
                return;
            }

            bool isCorrect = (bool)selected.Tag;
            if (isCorrect) _correctCount++;

            foreach (RadioButton rb in SpOptions.Children.OfType<RadioButton>())
            {
                bool rbIsCorrect = (bool)rb.Tag;
                if (rbIsCorrect)
                {
                    rb.Background = Brushes.LightGreen;
                }
                else
                {
                    rb.Background = Brushes.LightPink;
                }
                rb.IsEnabled = false;
            }

            var timer = new System.Threading.Timer(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    _currentQuestionIndex++;
                    ShowQuestion();
                });
            }, null, 1500, System.Threading.Timeout.Infinite);
        }

        private void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            ShowResult();
        }

        private void ShowResult()
        {
            if (_isQuizCompleted) return;

            _isQuizCompleted = true;
            int total = _questions.Count;
            int wrong = total - _correctCount;

            MessageBox.Show($"Результат:\nПравильных: {_correctCount}\nНеправильных: {wrong}");

            _db.Results.Add(new Results
            {
                QuizId = _quizId,
                TotalQuestions = total,
                CorrectAnswers = _correctCount
            });
            _db.SaveChanges();

            BtnNext.IsEnabled = false;
            BtnFinish.Content = "Закрыть";
            BtnFinish.Click -= BtnFinish_Click;
            BtnFinish.Click += (s, e) => Close();
        }
    }
}
