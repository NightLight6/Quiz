using QuizzApp.Model;
using System.Linq;
using System.Windows;
using QuizzApp.Helpers;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace QuizzApp
{
    public partial class MainWindow : Window
    {
        private QuizDBEntities _db = new QuizDBEntities();
        private Quizzes _selectedQuiz;

        public MainWindow()
        {
            InitializeComponent();
            LoadQuizzes();
        }

        private void LoadQuizzes()
        {
            LvQuizzes.ItemsSource = _db.Quizzes.Include("Questions").ToList();
        }

        private void LvQuizzes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LvQuizzes.SelectedItem is Quizzes quiz)
            {
                _selectedQuiz = quiz;
                LvQuestions.ItemsSource = quiz.Questions.ToList();
            }
            else
            {
                LvQuestions.ItemsSource = null;
            }
        }

        private void LvQuizzes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LvQuizzes.SelectedItem is Quizzes quiz)
            {
                var quizWindow = new QuizWindow(quiz.QuizId);
                quizWindow.ShowDialog();
            }
        }

        private void BtnNewQuiz_Click(object sender, RoutedEventArgs e)
        {
            var inputWindow = new InputWindow("Название викторины:");
            if (inputWindow.ShowDialog() == true)
            {
                _db.Quizzes.Add(new Quizzes { Title = inputWindow.InputText });
                _db.SaveChanges();
                LoadQuizzes();
                // Новая викторина не выделяется автоматически — по желанию можно добавить логику выделения последней
            }
        }

        private void BtnAddQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedQuiz == null)
            {
                MessageBox.Show("Сначала выберите викторину!");
                return;
            }

            var quizId = _selectedQuiz.QuizId;
            var addWindow = new AddQuestionWindow(quizId);
            addWindow.ShowDialog();

            LoadQuizzes();
            RefreshSelection(quizId);
        }

        private void BtnDeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedQuiz == null)
            {
                MessageBox.Show("Выберите викторину для удаления.");
                return;
            }

            try
            {
                if (MessageBox.Show($"Удалить викторину '{_selectedQuiz.Title}' и все вопросы?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    var questionIds = _db.Questions
                        .Where(q => q.QuizId == _selectedQuiz.QuizId)
                        .Select(q => q.QuestionId)
                        .ToList();

                    var optionsToDelete = _db.Options
                        .Where(o => questionIds.Contains(o.QuestionId))
                        .ToList();

                    _db.Options.RemoveRange(optionsToDelete);
                    _db.Questions.RemoveRange(_db.Questions.Where(q => q.QuizId == _selectedQuiz.QuizId));
                    _db.Quizzes.Remove(_selectedQuiz);
                    _db.SaveChanges();

                    RefreshSelection(null);
                    LoadQuizzes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            QuizzApp.Helpers.JsonService.ExportToJSON();
            MessageBox.Show("Экспорт завершён!");
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            QuizzApp.Helpers.JsonService.ImportFromJSON();
            RefreshSelection(null);
            LoadQuizzes();
            MessageBox.Show("Импорт завершён!");
        }

        private void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (LvQuestions.SelectedItem is Questions question)
            {
                var quizId = _selectedQuiz?.QuizId;
                var editWindow = new EditQuestionWindow(question.QuestionId);
                if (editWindow.ShowDialog() == true)
                {
                    LoadQuizzes();
                    RefreshSelection(quizId);
                }
            }
        }

        private void DeleteQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (LvQuestions.SelectedItem is Questions question)
            {
                if (MessageBox.Show($"Удалить вопрос?\n{question.QuestionText}", "Подтверждение",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _db.Questions.Remove(question);
                    _db.SaveChanges();

                    var quizId = _selectedQuiz?.QuizId;
                    LoadQuizzes();
                    RefreshSelection(quizId);
                }
            }
        }

        /// <summary>
        /// Обновляет выделение викторины и список вопросов на основе QuizId.
        /// Если quizId = null — сбрасывает выделение.
        /// </summary>
        private void RefreshSelection(int? quizId)
        {
            if (quizId.HasValue)
            {
                _selectedQuiz = _db.Quizzes.Include("Questions").FirstOrDefault(q => q.QuizId == quizId.Value);
                LvQuizzes.SelectedItem = _selectedQuiz;
                LvQuestions.ItemsSource = _selectedQuiz?.Questions.ToList();
            }
            else
            {
                _selectedQuiz = null;
                LvQuizzes.SelectedItem = null;
                LvQuestions.ItemsSource = null;
            }
        }
    }
}