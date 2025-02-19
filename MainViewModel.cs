﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HiszpanskiWpf
{
    public enum LearningDirection
    {
        SpanishToPolish,
        PolishToSpanish
    }
    public class MainViewModel : INotifyPropertyChanged
    {
        private LearningDirection _learningDirection = LearningDirection.SpanishToPolish;
        public LearningDirection LearningDirection
        {
            get => _learningDirection;
            set
            {
                _learningDirection = value;
                OnPropertyChanged(nameof(LearningDirection));

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
        }

        private void ChangeDirection(string direction)
        {
            if (Enum.TryParse(direction, out LearningDirection newDirection))
            {
                LearningDirection = newDirection;
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
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



}
