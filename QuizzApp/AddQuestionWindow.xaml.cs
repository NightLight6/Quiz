using QuizzApp.Model;
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

namespace QuizzApp
{
    /// <summary>
    /// Логика взаимодействия для AddQuestionWindow.xaml
    /// </summary>
    public partial class AddQuestionWindow : Window
    {
        private QuizDBEntities _db = new QuizDBEntities();

        public AddQuestionWindow(int quizId)
        {
            InitializeComponent();
            CbQuizzes.ItemsSource = _db.Quizzes.ToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CbQuizzes.SelectedItem == null || string.IsNullOrWhiteSpace(TbQuestion.Text))
            {
                MessageBox.Show("Выберите викторину и введите вопрос.");
                return;
            }

            var quiz = (Quizzes)CbQuizzes.SelectedItem;
            var question = new Questions
            {
                QuizId = quiz.QuizId,
                QuestionText = TbQuestion.Text.Trim()
            };
            _db.Questions.Add(question);
            _db.SaveChanges();

            // Правильный ответ
            _db.Options.Add(new Options
            {
                QuestionId = question.QuestionId,
                OptionText = TbCorrect.Text.Trim(),
                IsCorrect = true
            });

            // Неправильные
            foreach (string line in TbIncorrect.Text.Split('\n'))
            {
                var text = line.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    _db.Options.Add(new Options
                    {
                        QuestionId = question.QuestionId,
                        OptionText = text,
                        IsCorrect = false
                    });
                }
            }

            _db.SaveChanges();
            MessageBox.Show("Вопрос добавлен!");
            Close();
        }
    }
}
