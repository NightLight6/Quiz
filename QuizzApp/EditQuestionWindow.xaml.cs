using QuizzApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using QuizzApp;
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
    /// Логика взаимодействия для EditQuestionWindow.xaml
    /// </summary>
    public partial class EditQuestionWindow : Window
    {
        private QuizDBEntities _db = new QuizDBEntities();
        private int _questionId;
        private ObservableCollection<Options> _options;

        public EditQuestionWindow(int questionId)
        {
            InitializeComponent();
            _questionId = questionId;
            LoadQuestion();
        }

        private void LoadQuestion()
        {
            var question = _db.Questions.FirstOrDefault(q => q.QuestionId == _questionId);
            if (question == null) { Close(); return; }

            TbQuestion.Text = question.QuestionText;

            _options = new ObservableCollection<Options>(
                question.Options.OrderBy(o => o.OptionId).ToList()
            );
            LvOptions.ItemsSource = _options;
        }

        private void BtnAddOption_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputWindow("Новый вариант ответа:");
            if (input.ShowDialog() == true)
            {
                _options.Add(new Options { OptionText = input.InputText, IsCorrect = false });
            }
        }

        private void BtnDeleteOption_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is FrameworkElement fe && fe.Tag is Options option)
            {
                _options.Remove(option);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var question = _db.Questions.FirstOrDefault(q => q.QuestionId == _questionId);
            if (question == null) return;

            question.QuestionText = TbQuestion.Text.Trim();

            var oldOptions = _db.Options.Where(o => o.QuestionId == _questionId).ToList();
            _db.Options.RemoveRange(oldOptions);

            foreach (var opt in _options)
            {
                _db.Options.Add(new Options
                {
                    QuestionId = _questionId,
                    OptionText = opt.OptionText,
                    IsCorrect = opt.IsCorrect
                });
            }

            _db.SaveChanges();
            DialogResult = true;
            Close();
        }
    }
}
