using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using RobotFight_Avalonia.ViewModels;
using RobotFight_Avalonia.Views;
using RobotFight.Model;
using RobotFight.Persistence;
using System;
using System.IO;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Avalonia.Platform.Storage;
using Avalonia.Platform;


namespace RobotFight_Avalonia
{

    public partial class App : Application
    {
        private RobotFightModel _model = null!;
        private RobotFightViewModel _viewModel = null!;
        private TopLevel? TopLevel
        {
            get
            {
                return ApplicationLifetime switch
                {
                    IClassicDesktopStyleApplicationLifetime desktop => TopLevel.GetTopLevel(desktop.MainWindow),
                    ISingleViewApplicationLifetime singleViewPlatform => TopLevel.GetTopLevel(singleViewPlatform.MainView),
                    _ => null
                };
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {

            IRobotFightDataAccess fileAccess = new RobotFightFileAccess();
            _model = new RobotFightModel(fileAccess);
            _model.GameOver += (sender, args) =>
            {
                MessageBoxManager.GetMessageBoxStandard(
                    "Game Over",
                    args.ToString() + '!',
                    ButtonEnum.Ok,Icon.Info)
                    .ShowAsync();
            };

            _viewModel = new RobotFightViewModel(_model);
            _viewModel.OnLoadEvent += LoadGame;
            _viewModel.OnSaveEvent += SaveGame;
            _viewModel.OnInsufficientActionsEvent +=  (sender, args) => {
                 MessageBoxManager.GetMessageBoxStandard(
                     "Insufficient inputs",
                     "Provide 5 inputs first for your robot!",
                     ButtonEnum.Ok, Icon.Error)
                    .ShowAsync();
            };


            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _viewModel
                };
                desktop.Startup += async (s, e) =>
                {
                    //_model.NewGame(); // indításkor új játékot kezdünk

                    // betöltjük a felfüggesztett játékot, amennyiben van
                    try
                    {
                        await _model.LoadGameAsync(
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RobotFightSuspendedGame"));
                    }
                    catch { }
                };

                desktop.Exit += async (s, e) =>
                {
                    // elmentjük a jelenleg folyó játékot
                    try
                    {
                        await _model.SaveGameAsync(
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RobotFightSuspendedGame"));
                        // mentés a felhasználó Documents könyvtárába, oda minden bizonnyal van jogunk írni
                    }
                    catch { }
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = _viewModel
                };
                if (Application.Current?.TryGetFeature<IActivatableLifetime>() is { } activatableLifetime)
                {
                    activatableLifetime.Activated += async (sender, args) =>
                    {
                        if (args.Kind == ActivationKind.Background)
                        {
                            // betöltjük a felfüggesztett játékot, amennyiben van
                            try
                            {
                                await _model.LoadGameAsync(
                                    Path.Combine(AppContext.BaseDirectory, "RobotFightSuspendedGame"));
                            }
                            catch
                            {
                            }
                        }
                    };
                    activatableLifetime.Deactivated += async (sender, args) =>
                    {
                        if (args.Kind == ActivationKind.Background)
                        {
                            // elmentjük a jelenleg folyó játékot
                            try
                            {
                                await _model.SaveGameAsync(
                                    Path.Combine(AppContext.BaseDirectory, "RobotFightSuspendedGame"));
                                // Androidon az AppContext.BaseDirectory az alkalmazás adat könyvtára, ahova
                                // akár külön jogosultság nélkül is lehetne írni
                            }
                            catch{ }
                        }
                    };
                }
            }
            base.OnFrameworkInitializationCompleted();
        }

        #region VM eventhandling

        private async void LoadGame(object? sender, EventArgs e)
        {
            if (TopLevel == null)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                        "RobotFight",
                        "A fájlkezelés nem támogatott!",
                        ButtonEnum.Ok, Icon.Error)
                    .ShowAsync();
                return;
            }
            try
            {
                var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Loading RobotFight game",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                    new FilePickerFileType("RobotFight game")
                    {
                        Patterns = new[] { "*.rf" }
                    }
                }
                });

                if (files.Count > 0)
                {
                    /*string path = files[0].Path.ToString();//may work like this
                    await _model.LoadGameAsync(path);*/
                    using (var stream = await files[0].OpenReadAsync())
                    {
                        await _model.LoadGameAsync(stream);
                    }
                }
            }
            catch
            {
                await MessageBoxManager.GetMessageBoxStandard(
                        "RobotFight",
                        "A fájl betöltése sikertelen!",
                        ButtonEnum.Ok, Icon.Error)
                    .ShowAsync();
            }
        }
        private async void SaveGame(object? sender, EventArgs e)
        {
            if (TopLevel == null)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                        "RobotFight",
                        "A fájlkezelés nem támogatott!",
                        ButtonEnum.Ok, Icon.Error)
                    .ShowAsync();
                return;
            }

            try
            {
                var file = await TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
                {
                    Title = "Saving RobotFight game",
                    FileTypeChoices = new[]
                    {
                    new FilePickerFileType("RobotFight game ")
                    {
                        Patterns = new[] { "*.rf" }
                    }
                }
                });

                if (file != null)
                {
                    // játék mentése
                    using (var stream = await file.OpenWriteAsync())
                    {
                        await _model.SaveGameAsync(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                        "RobotFight",
                        "A fájl mentése sikertelen!" + ex.Message,
                        ButtonEnum.Ok, Icon.Error)
                    .ShowAsync();
            }
        }

        #endregion
    }
}
