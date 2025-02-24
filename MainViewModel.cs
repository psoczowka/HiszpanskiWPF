using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HiszpanskiWpf
{
    public enum LearningDirection
    {
        SpanishToPolish,
        PolishToSpanish
    }

    public enum HintLevel
    {
        StarsOnly,
        StarsAndLetters,
        NoHints
    }
    public class MainViewModel : INotifyPropertyChanged
    {

        public void UpdateCurrentQuestion()
        {
            if (SelectedWords.Count > 0)
            {
                var random = new Random();
                var nextWord = SelectedWords[random.Next(SelectedWords.Count)];

                // Wybierz pytanie w zależności od kierunku nauki
                string question = LearningDirection == LearningDirection.SpanishToPolish
                    ? nextWord.Spanish
                    : nextWord.Polish;

                // Uwzględnij poziom podpowiedzi
                switch (HintLevel)
                {
                    case HintLevel.StarsOnly:
                        CurrentQuestion = new string('*', question.Length);
                        break;

                    case HintLevel.StarsAndLetters:
                        var hint = new StringBuilder();
                        for (int i = 0; i < question.Length; i++)
                        {
                            // Losowo pokazujemy litery (co trzecią) zamiast gwiazdek
                            if (i % 3 == 0)
                                hint.Append(question[i]);
                            else
                                hint.Append('*');
                        }
                        CurrentQuestion = hint.ToString();
                        break;

                    case HintLevel.NoHints:
                    default:
                        CurrentQuestion = question;
                        break;
                }

                // Reset odpowiedzi użytkownika i komunikatu zwrotnego
                UserAnswer = "";
                FeedbackMessage = "";
            }
            else
            {
                CurrentQuestion = "Wybierz materiał do nauki.";
                UserAnswer = "";
                FeedbackMessage = "";
            }
        }

        // Pytanie wyświetlane w środkowym panelu
        private string _currentQuestion;
        public string CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                _currentQuestion = value;
                OnPropertyChanged(nameof(CurrentQuestion));
            }
        }

        // Śledzi, czy użytkownik odpowiedział źle na dane pytanie
        private bool _hasAnsweredWrong;
        public bool HasAnsweredWrong
        {
            get => _hasAnsweredWrong;
            set
            {
                _hasAnsweredWrong = value;
                OnPropertyChanged(nameof(HasAnsweredWrong));
            }
        }

        // Odpowiedź użytkownika
        private string _userAnswer;
        public string UserAnswer
        {
            get => _userAnswer;
            set
            {
                _userAnswer = value;
                OnPropertyChanged(nameof(UserAnswer));
            }
        }

        // Wiadomość zwrotna
        private string _feedbackMessage;
        public string FeedbackMessage
        {
            get => _feedbackMessage;
            set
            {
                _feedbackMessage = value;
                OnPropertyChanged(nameof(FeedbackMessage));
            }
        }

        // Lista wybranych lekcji
        private List<Word> _selectedWords = new List<Word>();
        public List<Word> SelectedWords
        {
            get => _selectedWords;
            private set
            {
                _selectedWords = value;
                OnPropertyChanged(nameof(SelectedWords));
            }
        }

        public void UpdateSelectedWords()
        {
            var words = Chapters
                .SelectMany(ch => ch.Lessons)
                .Where(l => l.IsSelected)
                .SelectMany(l => l.Words)
                .ToList();

            SelectedWords = words;

            // Jeśli coś zostało zaznaczone, rozpoczynamy nową sesję
            if (SelectedWords.Count > 0)
            {
                StartNewSession(); // Rozpocznij nową sesję
            }
            else
            {
                // Jeśli nic nie zostało wybrane, czyścimy pytania i licznik punktów
                _randomizedQuestions.Clear();
                _currentQuestionIndex = -1;
                Score = 0;
                TotalQuestions = 0;
                CurrentQuestion = "Wybierz materiał do nauki.";
            }
        }

        // Komenda do sprawdzenia odpowiedzi
        public ICommand CheckAnswerCommand { get; }

        // Logika sprawdzania odpowiedzi
        private async void CheckAnswer()
        {
            if (SelectedWords.Count == 0)
            {
                FeedbackMessage = "Wybierz materiał do nauki.";
                return;
            }

            var currentWord = SelectedWords.FirstOrDefault(w =>
                LearningDirection == LearningDirection.SpanishToPolish
                    ? w.Spanish == CurrentQuestion
                    : w.Polish == CurrentQuestion);

            if (currentWord != null)
            {
                bool isCorrect = LearningDirection == LearningDirection.SpanishToPolish
                    ? UserAnswer.Equals(currentWord.Polish, StringComparison.OrdinalIgnoreCase)
                    : UserAnswer.Equals(currentWord.Spanish, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    if (!HasAnsweredWrong)
                    {
                        Score++; // Dodaj punkt tylko wtedy, gdy nie było złej odpowiedzi
                        FeedbackMessage = "Dobrze! Brawo!";

                        // Natychmiast przejdź do kolejnego pytania
                        LoadNextQuestion();
                    }
                    else
                    {
                        FeedbackMessage = "Poprawnie, ale nie dostajesz punktu za wcześniejszy błąd.";

                        // Odśwież widok
                        OnPropertyChanged(nameof(FeedbackMessage));

                        // Czekaj 1 sekundę, aby użytkownik zobaczył komunikat
                        await Task.Delay(1000);

                        // Przejdź do następnego pytania
                        LoadNextQuestion();
                    }

                }
                else
                {
                    FeedbackMessage = $"Źle! Poprawna odpowiedź to: " +
                        (LearningDirection == LearningDirection.SpanishToPolish
                            ? currentWord.Polish
                            : currentWord.Spanish);

                    // Oznacz pytanie jako odpowiedziane źle
                    HasAnsweredWrong = true;
                }
            }
            else
            {
                FeedbackMessage = "Nie znaleziono odpowiedzi. Wybierz materiał ponownie.";
            }

            // Ustawienie fokusu na TextBox po sprawdzeniu odpowiedzi
            Application.Current.Dispatcher.Invoke(() =>
            {
                var textBox = Application.Current.Windows.OfType<MainWindow>()
                                .FirstOrDefault()?
                                .FindName("UserAnswerTextBox") as TextBox;
                textBox?.Focus();
            });
        }

        // Ładowanie kolejnego pytania
        private void LoadNextQuestion()
        {
            // Jeśli nie ma więcej pytań, zakończ sesję
            if (_currentQuestionIndex >= _randomizedQuestions.Count - 1)
            {
                CurrentQuestion = "Koniec materiału! Zdobyte punkty: " + Score + " / " + TotalQuestions;
                FeedbackMessage = "Sesja zakończona.";
                return;
            }

            // Przejdź do następnego pytania
            _currentQuestionIndex++;
            var nextWord = _randomizedQuestions[_currentQuestionIndex];

            // Wybierz pytanie w zależności od kierunku nauki
            string question = LearningDirection == LearningDirection.SpanishToPolish
                ? nextWord.Spanish
                : nextWord.Polish;

            // Uwzględnij poziom podpowiedzi
            switch (HintLevel)
            {
                case HintLevel.StarsOnly:
                    CurrentQuestion = new string('*', question.Length);
                    break;

                case HintLevel.StarsAndLetters:
                    var hint = new StringBuilder();
                    for (int i = 0; i < question.Length; i++)
                    {
                        if (i % 3 == 0)
                            hint.Append(question[i]);
                        else
                            hint.Append('*');
                    }
                    CurrentQuestion = hint.ToString();
                    break;

                case HintLevel.NoHints:
                default:
                    CurrentQuestion = question;
                    break;
            }

            // Reset odpowiedzi użytkownika i komunikatu zwrotnego
            UserAnswer = "";
            FeedbackMessage = "";

            // Resetuj `HasAnsweredWrong` dla nowego pytania
            HasAnsweredWrong = false;
        }


        private LearningDirection _learningDirection = LearningDirection.SpanishToPolish;
        public LearningDirection LearningDirection
        {
            get => _learningDirection;
            set
            {
                _learningDirection = value;
                OnPropertyChanged(nameof(LearningDirection));
                UpdateCurrentQuestion();

                // Automatyczne odświeżenie środkowego panelu
                // OnPropertyChanged(nameof(FilteredWords));
            }
        }

        public ICommand ChangeDirectionCommand { get; }

        public static MainViewModel Instance { get; private set; }

        public MainViewModel()
        {
            Instance = this; // Umożliwia dostęp do `Chapters` w `Lesson`
            LoadData();
            ChangeDirectionCommand = new RelayCommand<string>(ChangeDirection);
            ChangeHintLevelCommand = new RelayCommand<string>(ChangeHintLevel);
            CheckAnswerCommand = new RelayCommand(CheckAnswer);
            SkipQuestionCommand = new RelayCommand(SkipQuestion);

            // Pobierz pierwsze pytanie po załadowaniu danych
            LoadNextQuestion();
        }

        private async void SkipQuestion()
        {
            // Jeśli nie ma więcej pytań, wyświetl komunikat
            if (SelectedWords.Count == 0)
            {
                FeedbackMessage = "Wybierz materiał do nauki";
                return;
            }

            FeedbackMessage = "Pytanie pominięte";

            // Odśwież widok
            OnPropertyChanged(nameof(FeedbackMessage));

            // Czekaj 1 sekundę, aby użytkownik zobaczył komunikat
            await Task.Delay(1000);

            LoadNextQuestion();
        }

        // Lista pytań w losowej kolejności
        private List<Word> _randomizedQuestions = new List<Word>();
        private int _currentQuestionIndex = -1;

        // Licznik punktów
        private int _score = 0;
        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged(nameof(Score));
            }
        }

        // Liczba wszystkich pytań w sesji
        private int _totalQuestions;
        public int TotalQuestions
        {
            get => _totalQuestions;
            set
            {
                _totalQuestions = value;
                OnPropertyChanged(nameof(TotalQuestions));
            }
        }

        public void StartNewSession()
        {
            // Pobierz wszystkie wybrane pytania i wymieszaj je losowo
            _randomizedQuestions = SelectedWords.OrderBy(_ => Guid.NewGuid()).ToList();
            _currentQuestionIndex = -1;
            Score = 0;

            // Aktualizacja całkowitej liczby pytań
            TotalQuestions = _randomizedQuestions.Count;

            // Jeśli są jakieś pytania, rozpocznij sesję
            if (TotalQuestions > 0)
            {
                LoadNextQuestion();
            }
            else
            {
                CurrentQuestion = "Wybierz materiał do nauki.";
                FeedbackMessage = "";
            }
        }

        private void ChangeDirection(string direction)
        {
            if (Enum.TryParse(direction, out LearningDirection newDirection))
            {
                LearningDirection = newDirection;
            }
        }

        // wybieranie poziomu podpowiedzi
        private HintLevel _hintLevel = HintLevel.NoHints;
        public HintLevel HintLevel
        {
            get => _hintLevel;
            set
            {
                _hintLevel = value;
                OnPropertyChanged(nameof(HintLevel));
                UpdateCurrentQuestion();

                // Automatyczne odświeżenie środkowego panelu
                //OnPropertyChanged(nameof(FilteredWords));
            }
        }

        public ICommand ChangeHintLevelCommand { get; }
        public ICommand SkipQuestionCommand { get; }

        private void ChangeHintLevel(string level)
        {
            if (Enum.TryParse(level, out HintLevel newLevel))
            {
                HintLevel = newLevel;
            }
        }

        public List<Lesson> SelectedLessons => Chapters
            .SelectMany(ch => ch.Lessons)
            .Where(l => l.IsSelected)
            .ToList();

        private ObservableCollection<Chapter> _chapters;
        public ObservableCollection<Chapter> Chapters
        {
            get => _chapters;
            set
            {
                _chapters = value;
                OnPropertyChanged(nameof(Chapters));
            }
        }

        private void LoadData()
        {
            try
            {
                string filePath = "data.json";
                var data = DataLoader.LoadData(filePath);
                Chapters = new ObservableCollection<Chapter>(data.Chapters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wczytywania JSON: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
