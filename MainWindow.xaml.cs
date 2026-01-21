using System.Windows;
using System.Windows.Input;
using SPP2LetterSearch.Models;
using SPP2LetterSearch.ViewModels;

namespace SPP2LetterSearch
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var viewModel = DataContext as SearchViewModel;
                if (viewModel?.SearchCommand.CanExecute(null) == true)
                {
                    viewModel.SearchCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void ResultsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as System.Windows.Controls.DataGrid;
            if (grid?.SelectedItem is SearchResultItem item)
            {
                var viewModel = DataContext as SearchViewModel;
                if (viewModel?.DoubleClickCommand.CanExecute(item) == true)
                {
                    viewModel.DoubleClickCommand.Execute(item);
                }
            }
        }
    }
}
