using System.Windows;
using SPP2LetterSearch.ViewModels;

namespace SPP2LetterSearch
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = new MainWindow();
            mainWindow.DataContext = new SearchViewModel();
            mainWindow.Show();
        }
    }
}
