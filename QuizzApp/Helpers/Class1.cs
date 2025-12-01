using QuizzApp.Model;
using QuizzApp.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows;

namespace QuizzApp.Helpers
{
    public static class JsonService
    {
        public static void ExportToJSON()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Сохранить викторины как JSON",
                FileName = "quizzes_backup.json"
            };

            if (dialog.ShowDialog() != true) return; 

            try
            {
                using (var db = new QuizDBEntities())
                {
                    var quizzes = db.Quizzes
                        .Include("Questions.Options")
                        .ToList();

                    var dtoQuizzes = quizzes.Select(q => new QuizDto
                    {
                        QuizId = q.QuizId,
                        Title = q.Title,
                        Description = q.Description,
                        Questions = q.Questions.Select(quest => new QuestionDto
                        {
                            QuestionId = quest.QuestionId,
                            QuestionText = quest.QuestionText,
                            Options = quest.Options.Select(opt => new OptionDto
                            {
                                OptionId = opt.OptionId,
                                OptionText = opt.OptionText,
                                IsCorrect = opt.IsCorrect
                            }).ToList()
                        }).ToList()
                    }).ToList();

                    string json = JsonConvert.SerializeObject(dtoQuizzes, Formatting.Indented);
                    File.WriteAllText(dialog.FileName, json);
                    MessageBox.Show("Экспорт успешно завершён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ImportFromJSON()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Выберите файл JSON для импорта"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                string json = File.ReadAllText(dialog.FileName);
                var dtoQuizzes = JsonConvert.DeserializeObject<List<QuizDto>>(json);

                if (dtoQuizzes == null || !dtoQuizzes.Any())
                {
                    MessageBox.Show("Файл пуст или содержит неверные данные.");
                    return;
                }

                using (var db = new QuizDBEntities())
                {
                    db.Options.RemoveRange(db.Options);
                    db.Questions.RemoveRange(db.Questions);
                    db.Quizzes.RemoveRange(db.Quizzes);
                    db.SaveChanges();

                    foreach (var dto in dtoQuizzes)
                    {
                        var quiz = new Quizzes
                        {
                            Title = dto.Title,
                            Description = dto.Description
                        };
                        db.Quizzes.Add(quiz);
                        db.SaveChanges();

                        foreach (var qDto in dto.Questions)
                        {
                            var question = new Questions
                            {
                                QuizId = quiz.QuizId,
                                QuestionText = qDto.QuestionText
                            };
                            db.Questions.Add(question);
                            db.SaveChanges();

                            foreach (var oDto in qDto.Options)
                            {
                                db.Options.Add(new Options
                                {
                                    QuestionId = question.QuestionId,
                                    OptionText = oDto.OptionText,
                                    IsCorrect = oDto.IsCorrect
                                });
                            }
                        }
                    }
                    db.SaveChanges();
                    MessageBox.Show("Импорт успешно завершён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}