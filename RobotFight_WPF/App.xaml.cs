using Microsoft.Win32;
using RobotFight.Model;
using RobotFight.Persistence;
using RobotFight_WPF.View;
using RobotFight_WPF.ViewModel;
using System.Configuration;
using System.Data;
using System.Windows;

namespace RobotFight_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private RobotFightViewModel _viewModel = null!;
        private MainWindow _view = null!;
        private RobotFightModel _model = null!;

        public App()
        {
            Startup += OnStartup;

        }
        private void OnStartup(object sender, StartupEventArgs e)
        {
            IRobotFightDataAccess fileAccess = new RobotFightFileAccess();
            _model = new RobotFightModel(fileAccess);

            _viewModel = new RobotFightViewModel(_model);
            _viewModel.OnExitEvent += ExitDialog;
            _viewModel.OnLoadEvent += LoadGame;
            _viewModel.OnSaveEvent += SaveGame;

            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Show();
        }

        #region VM eventhandling
        private void ExitDialog(object? sender, EventArgs e) 
        {
            if (MessageBox.Show("Are you sure you want to exit?", "RobotFight", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _view.Close();
            }
        }
        private async void LoadGame(object? sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog(); // dialógusablak
                openFileDialog.Title = "Loading a RobotFight game";
                openFileDialog.Filter = "RobotFight game (.rf)|*.rf";
                if (openFileDialog.ShowDialog() == true)
                {
                    await _model.LoadGameAsync(openFileDialog.FileName);
                }
            }
            catch
            {
                MessageBox.Show("Loading Failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void SaveGame(object? sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Saving RobotFight game";
                saveFileDialog.Filter = "RobotFight game (.rf)|*.rf";
                if (saveFileDialog.ShowDialog() == true)
                {
                    await _model.SaveGameAsync(saveFileDialog.FileName);
                }
            }
            catch
            {
                MessageBox.Show("Save Failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }

}
