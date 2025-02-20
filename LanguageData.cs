using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace HiszpanskiWpf
{
    public class LanguageData
    {
        public List<Chapter> Chapters { get; set; }
    }

    public class Chapter : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public List<Lesson> Lessons { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                // Jeśli użytkownik zaznacza rozdział, zaznaczamy też wszystkie lekcje
                if (Lessons != null)
                {
                    foreach (var lesson in Lessons)
                    {
                        lesson.IsSelected = value;
                    }
                }

                // Powiadomienie o zmianie wybranego materiału
                MainViewModel.Instance?.UpdateSelectedWords();
            }
        }

        public void UpdateSelection()
        {
            if (Lessons != null)
            {
                // Jeśli wszystkie lekcje są zaznaczone, oznaczamy także rozdział
                _isSelected = Lessons.All(l => l.IsSelected);
                OnPropertyChanged(nameof(IsSelected));
            }

            // Powiadomienie o zmianie wybranego materiału
            MainViewModel.Instance?.OnPropertyChanged(nameof(MainViewModel.SelectedWords));
            MainViewModel.Instance?.UpdateCurrentQuestion();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Lesson : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public List<Word> Words { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                // Znajdź rozdział, do którego należy ta lekcja i zaktualizuj jego zaznaczenie
                var parentChapter = MainViewModel.Instance?.Chapters.FirstOrDefault(ch => ch.Lessons.Contains(this));
                parentChapter?.UpdateSelection();

                // Powiadomienie o zmianie wybranego materiału
                MainViewModel.Instance?.UpdateSelectedWords();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Word
    {
        public string Spanish { get; set; }
        public string Polish { get; set; }
    }
}
