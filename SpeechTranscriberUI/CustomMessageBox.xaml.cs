// CustomMessageBox.xaml.cs
// This file defines the CustomMessageBox class, which is a custom implementation of a message box for a WPF application.
// The CustomMessageBox class supports data binding for its title, message, and icon properties, and provides visibility properties for various buttons.
// It also includes command properties for handling button clicks and returns a MessageBoxResult based on user interaction.
// Additionally, the file contains a helper class, CustomMessageBoxHelper, which provides static methods to display the custom message box with or without an owner window.

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO; // Add this using directive

namespace SpeechTranscriberUI;

/// <summary>
/// Interaction logic for Window1.xaml
/// </summary>
public partial class CustomMessageBox : Window, INotifyPropertyChanged
{
    public CustomMessageBox()
    {
        InitializeComponent();
    }

    // Properties for data binding
    private string _title;
    public string TitleText
    {
        get => _title;
        set { _title = value; OnPropertyChanged(nameof(TitleText)); }
    }

    private string _message;
    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(nameof(Message)); }
    }

    private BitmapImage _icon;
    public BitmapImage IconMy
    {
        get => _icon;
        set { _icon = value; OnPropertyChanged(nameof(IconMy)); }
    }

    // Button Visibility Properties
    public Visibility IsOKVisible { get; set; }
    public Visibility IsCancelVisible { get; set; }
    public Visibility IsYesVisible { get; set; }
    public Visibility IsNoVisible { get; set; }

    // Commands
    public ICommand OKCommand { get; set; }
    public ICommand CancelCommand { get; set; }
    public ICommand YesCommand { get; set; }
    public ICommand NoCommand { get; set; }

    // Result
    public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

    public CustomMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        InitializeComponent();
        DataContext = this;

        // Set Title and Message
        TitleText = title;
        Message = message;

        // Set Icon
        IconMy = LoadIcon(icon);

        // Configure Buttons
        ConfigureButtons(buttons);

        // Initialize Commands
        OKCommand = new RelayCommand(o => { Result = MessageBoxResult.OK; this.Close(); });
        CancelCommand = new RelayCommand(o => { Result = MessageBoxResult.Cancel; this.Close(); });
        YesCommand = new RelayCommand(o => { Result = MessageBoxResult.Yes; this.Close(); });
        NoCommand = new RelayCommand(o => { Result = MessageBoxResult.No; this.Close(); });
    }

    private BitmapImage LoadIcon(MessageBoxImage icon)
    {
        try
        {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string imagesFolder = @"./";
            string? iconFileName = icon switch
            {
                MessageBoxImage.Information => "info.png",
                MessageBoxImage.Error => "error.png",
                MessageBoxImage.Warning => "warning.png",
                MessageBoxImage.Question => "question.png",
                _ => null
            };

            if (iconFileName != null)
            {
                string iconPath = System.IO.Path.Combine(imagesFolder, iconFileName);
                if (File.Exists(iconPath))
                {
                    return new BitmapImage(new Uri(iconPath, UriKind.RelativeOrAbsolute));
                }
                else
                {
                    // Handle missing file scenario
                    // Optionally log or throw an exception
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error)
            // For now, we'll just ignore and return null
        }

        return null;
    }

    private void ConfigureButtons(MessageBoxButton buttons)
    {
        switch (buttons)
        {
            case MessageBoxButton.OK:
                IsOKVisible = Visibility.Visible;
                IsCancelVisible = Visibility.Collapsed;
                IsYesVisible = Visibility.Collapsed;
                IsNoVisible = Visibility.Collapsed;
                break;
            case MessageBoxButton.OKCancel:
                IsOKVisible = Visibility.Visible;
                IsCancelVisible = Visibility.Visible;
                IsYesVisible = Visibility.Collapsed;
                IsNoVisible = Visibility.Collapsed;
                break;
            case MessageBoxButton.YesNo:
                IsOKVisible = Visibility.Collapsed;
                IsCancelVisible = Visibility.Collapsed;
                IsYesVisible = Visibility.Visible;
                IsNoVisible = Visibility.Visible;
                break;
            case MessageBoxButton.YesNoCancel:
                IsOKVisible = Visibility.Collapsed;
                IsCancelVisible = Visibility.Visible;
                IsYesVisible = Visibility.Visible;
                IsNoVisible = Visibility.Visible;
                break;
            default:
                IsOKVisible = Visibility.Visible;
                IsCancelVisible = Visibility.Collapsed;
                IsYesVisible = Visibility.Collapsed;
                IsNoVisible = Visibility.Collapsed;
                break;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // RelayCommand Implementation
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}

public static class CustomMessageBoxHelper
{
    /// <summary>
    /// Displays a custom message box.
    /// </summary>
    /// <param name="owner">The owner window.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The title of the message box.</param>
    /// <param name="buttons">The buttons to include.</param>
    /// <param name="icon">The icon to display.</param>
    /// <returns>The MessageBoxResult based on user interaction.</returns>
    public static MessageBoxResult Show(Window owner, string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        var msgBox = new CustomMessageBox(message, title, buttons, icon)
        {
            Owner = owner
        };

        msgBox.ShowDialog();
        return msgBox.Result;
    }

    /// <summary>
    /// Overload without owner.
    /// </summary>
    public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        var msgBox = new CustomMessageBox(message, title, buttons, icon);
        msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        msgBox.ShowDialog();
        return msgBox.Result;
    }
}
