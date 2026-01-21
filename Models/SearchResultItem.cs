using System;
using System.ComponentModel;

namespace SPP2LetterSearch.Models
{
    public class SearchResultItem : INotifyPropertyChanged
    {
        private double _score;
        private string _snippet;
        private bool _isSelected;

        public string LetterNo { get; set; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string FolderName { get; set; }

        public double Score
        {
            get => _score;
            set { if (_score != value) { _score = value; OnPropertyChanged(nameof(Score)); } }
        }

        public string Snippet
        {
            get => _snippet;
            set { if (_snippet != value) { _snippet = value; OnPropertyChanged(nameof(Snippet)); } }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
