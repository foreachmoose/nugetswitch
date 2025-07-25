using System.ComponentModel;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using NuGetSwitch.Model;
using NuGetSwitch.ViewModel;
using NuGetSwitch.Service;
using System.Windows.Media.Imaging;

namespace NuGetSwitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDialogService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Images/app.png"));

            MainWindowViewModel viewModel = new MainWindowViewModel(this, new StorageService<WorkspaceDocument>());

            DataContext = viewModel;

            Closing += OnClosing;
        }

        /// <summary>
        /// Handles the <see cref="E:Closing" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void OnClosing(object? sender, CancelEventArgs e)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel) DataContext;

            // If the document is dirty, prompt the user to save changes
            if (!viewModel.IsDirty) 
                return;

            MessageBoxResult result = MessageBox.Show(this, "Workspace has unsaved changes. Do you want to save them?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
                case MessageBoxResult.Yes:
                    viewModel.CloseSolution();
                    break; 
            }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                IEnumerable<string> toRemove = LibrariesListBox.SelectedItems.Cast<string>();

                var viewModel = (MainWindowViewModel)DataContext;
                viewModel.DeleteSelectedItems(toRemove);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Shows the workspace save file dialog.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="startFolder">The start folder.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? ShowWorkspaceSaveFileDialog(string fileName, string startFolder)
        {
            // Show save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Workspace (*.json)|*.json",
                Title = "Save Workspace",
                InitialDirectory = startFolder,
                FileName = fileName
            };
            if (saveFileDialog.ShowDialog() == false)
            {
                return null;
            }

            return saveFileDialog.FileName;

        }

        /// <summary>
        /// Shows the workspace open file dialog.
        /// </summary>
        /// <param name="startFolder">The start folder.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? ShowWorkspaceOpenFileDialog(string startFolder)
        {
            // Open a file dialog to select a Workspace
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Workspace (*.json)|*.json",
                Title = "Open a Workspace File",
                InitialDirectory = startFolder
            };
            if (openFileDialog.ShowDialog() == false)
            {
                return null;
            }

            return openFileDialog.FileName;
        }

        /// <summary>
        /// Shows the solution open file dialog.
        /// </summary>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? ShowSolutionOpenFileDialog()
        {
            // Open a file dialog to select a solution file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Solution files (*.sln)|*.sln",
                Title = "Open a Solution File"
            };
            if (openFileDialog.ShowDialog() == false)
            {
                return null;
            }

            return openFileDialog.FileName;
        }

        /// <summary>
        /// Shows the add references file dialog.
        /// </summary>
        /// <param name="startFolder">The start folder.</param>
        /// <returns>System.Nullable&lt;IEnumerable&lt;System.String&gt;&gt;.</returns>
        public IEnumerable<string>? ShowAddReferencesFileDialog(string startFolder)
        {
            // Open a file dialog to select DLL files
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll",
                Title = "Select References",
                InitialDirectory = startFolder,
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == false)
            {
                return null;
            }

            return openFileDialog.FileNames;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ShowError(string message)
        {
            MessageBox.Show(this, message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Handles the Click event of the buttonMinimize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void buttonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Handles the Click event of the buttonMaximize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void buttonMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                maximizeImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/maximize.png"));
            }
            else
            {
                WindowState = WindowState.Maximized;
                maximizeImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/restore.png"));
            }
        }

        /// <summary>
        /// Handles the Click event of the buttonClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}