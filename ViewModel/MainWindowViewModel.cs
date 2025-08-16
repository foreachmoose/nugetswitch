using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NuGetSwitch.Message;
using NuGetSwitch.Model;
using NuGetSwitch.Service;

namespace NuGetSwitch.ViewModel;

/// <summary>
/// Main window view model for the NuGetSwitch application.
/// 
/// Implements the <see cref="ObservableObject" /></summary>
/// <seealso cref="ObservableObject" />
public class MainWindowViewModel : ObservableObject
{
    // Workspace document file extension. Use ".tmp" so git ignores it
    private const string WorkspaceFileExtension = ".nugetswitch.tmp";

    // Services
    private readonly IDialogService m_dialogService;
    private readonly StorageService m_storageService;
    private readonly IMessenger m_messenger;

    private VsSolution? m_solution;
    private WorkspaceDocument? m_workspaceDocument;
    private List<string> m_nugetPackageIds = [];
    private string m_title;
    private int m_selectedNuGetPackageIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
    /// </summary>
    /// <param name="dialogService">The file service.</param>
    /// <param name="storageService">The storage service.</param>
    /// <param name="messenger">The messenger.</param>
    /// <param name="gitViewModel">The git staus view model.</param>
    /// <param name="statusMessageViewModel">The status message view model.</param>
    public MainWindowViewModel(IDialogService dialogService, StorageService storageService, IMessenger messenger, GitViewModel gitViewModel, StatusMessageViewModel statusMessageViewModel)
    {
        m_dialogService = dialogService;
        m_storageService = storageService;
        m_messenger = messenger;
        m_title = "";

        GitViewModel = gitViewModel;
        StatusMessageViewModel = statusMessageViewModel;

        OpenSolutionCommand = new RelayCommand(OpenSolution, CanExecuteOpenSolution);
        CloseSolutionCommand = new RelayCommand(CloseSolution, CanExecuteCloseSolution);
        AddLocalReferencesCommand = new RelayCommand(AddLocalReferences, CanExecuteAddLocalReferences);
        SwitchCommand = new RelayCommand(Switch, CanExecuteSwitch);
        OpenExplorerCommand = new RelayCommand(OpenExplorer, CanExecuteOpenExplorer);
        DeleteObjFoldersCommand = new RelayCommand(DeleteObjFolders, CanDeleteObjFolders);
    }

    /// <summary>
    /// Gets the close workspace command.
    /// </summary>
    /// <value>The close workspace command.</value>
    public RelayCommand CloseSolutionCommand { get; }

    /// <summary>
    /// Gets the open solution command.
    /// </summary>
    /// <value>The open solution command.</value>
    public RelayCommand OpenSolutionCommand { get; }

    /// <summary>
    /// Gets the add local references command.
    /// </summary>
    /// <value>The add local references command.</value>
    public RelayCommand AddLocalReferencesCommand { get; }

    /// <summary>
    /// Gets the switch command.
    /// </summary>
    /// <value>The switch command.</value>
    public RelayCommand SwitchCommand { get; }

    /// <summary>
    /// Gets the open explorer command.
    /// </summary>
    /// <value>The open explorer command.</value>
    public RelayCommand OpenExplorerCommand { get; }

    /// <summary>
    /// Gets the delete object folders command.
    /// </summary>
    /// <value>The delete object folders command.</value>
    public RelayCommand DeleteObjFoldersCommand { get; }

    /// <summary>
    /// True if solution can be closed
    /// </summary>
    /// <returns>System.Boolean.</returns>
    private bool CanExecuteCloseSolution()
    {
        return m_solution != null;
    }

    /// <summary>
    /// True if a solution can be opened
    /// </summary>
    /// <returns>System.Boolean.</returns>
    private bool CanExecuteOpenSolution()
    {
        return m_solution == null;
    }

    /// <summary>
    /// True if local references can be added
    /// </summary>
    /// <returns>System.Boolean.</returns>
    private bool CanExecuteAddLocalReferences()
    {
        return SelectedNuGetPackageIndex >= 0 && SelectedNuGetPackageIndex < NuGetPackageIds.Count;
    }

    /// <summary>
    /// True if a switch can be done
    /// </summary>
    /// <returns>System.Boolean.</returns>
    private bool CanExecuteSwitch()
    {
        return m_workspaceDocument != null && m_workspaceDocument.AnySelectedLibraries();
    }

    /// <summary>
    /// True if Explorer can be opened
    /// </summary>
    /// <returns>System.Boolean.</returns>
    private bool CanExecuteOpenExplorer()
    {
        return m_solution != null;
    }

    /// <summary>
    /// True if object folders can be deleted
    /// </summary>
    /// <returns>System.Boolean.</returns>
    private bool CanDeleteObjFolders()
    {
        return m_solution != null && m_solution.Projects.Any();
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title
    {
        get => m_title;
        set => SetProperty(ref m_title, value);
    }


    /// <summary>
    /// Gets the git status view model.
    /// </summary>
    /// <value>The git status view model.</value>
    public GitViewModel GitViewModel { get; }

    /// <summary>
    /// Gets the status message view model.
    /// </summary>
    /// <value>The status message view model.</value>
    public StatusMessageViewModel StatusMessageViewModel { get; }

    /// <summary>
    /// Gets the list of NuGet package IDs
    /// </summary>
    /// <value>The nu get package ids.</value>
    public List<string> NuGetPackageIds
    {
        get => m_nugetPackageIds;
        set => SetProperty(ref m_nugetPackageIds, value);
    }

    /// <summary>
    /// Gets or sets the index of the selected nu get package.
    /// </summary>/
    /// <value>The index of the selected nu get package.</value>
    public int SelectedNuGetPackageIndex
    {
        get => m_selectedNuGetPackageIndex;
        set
        {
            SetProperty(ref m_selectedNuGetPackageIndex, value);

            OnPropertyChanged(nameof(Libraries));

            AddLocalReferencesCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Gets the list of selected libraries.
    /// </summary>
    /// <value>The libraries.</value>
    public IEnumerable<string> Libraries
    {
        get
        {
            if (m_workspaceDocument == null || SelectedNuGetPackageIndex < 0 || SelectedNuGetPackageIndex >= NuGetPackageIds.Count)
            {
                return [];
            }

            // Read directly from the document
            string packageId = NuGetPackageIds[SelectedNuGetPackageIndex];
            var libraries = m_workspaceDocument.GetSelectedLibraries(packageId);

            if (libraries == null)
            {
                return [];
            }

            return libraries.ToList();
        }
    }

    /// <summary>
    /// Deletes the selected items.
    /// </summary>
    /// <param name="toRemove">To remove.</param>
    public void DeleteSelectedItems(IEnumerable<string> toRemove)
    {
        if (m_workspaceDocument == null)
            throw new InvalidOperationException("No workspace document");

        string packageId = NuGetPackageIds[SelectedNuGetPackageIndex];

        m_workspaceDocument.RemoveLibraries(packageId, toRemove);

        OnPropertyChanged(nameof(Libraries));
    }

    /// <summary>
    /// True if document is dirty
    /// </summary>
    /// <value>The is dirty.</value>
    public bool IsDirty => m_workspaceDocument?.IsDirty ?? false;

    /// <summary>
    /// Opens the solution.
    /// </summary>
    public async void OpenSolution()
    {
        try
        {
            string? filePath = m_dialogService.ShowSolutionOpenFileDialog();

            if (filePath == null)
            {
                return; // User cancelled the file dialog
            }

            // Check if a workspace document already exists
            string workspaceFile = $"{filePath}{WorkspaceFileExtension}";
            try
            {
                WorkspaceDocument? document = await m_storageService.LoadAsync<WorkspaceDocument>(workspaceFile);
                if (document is not null)
                {
                    m_workspaceDocument = document;
                    m_messenger.Send(new StatusMessage($"Workspace document found: {workspaceFile}"));
                }
                else
                {
                    m_workspaceDocument = new WorkspaceDocument();
                    m_messenger.Send(new StatusMessage($"No workspace document found for: {filePath}"));
                }
            }
            catch (Exception e)
            {
                m_workspaceDocument = new WorkspaceDocument();
                m_messenger.Send(new StatusMessage($"Failed to load workspace document: {e.Message}"));
            }

            ParseVsSolutionFile(filePath);

            Title = filePath;

            m_messenger.Send(new SolutionOpenedMessage(m_solution));

            NotifyCanExecuteChanged();
        }
        catch (Exception e)
        {
            m_dialogService.ShowError($"Failed to open solution: {e.Message}");
        }
    }

    /// <summary>
    /// Adds local references to the selected NuGet package in the workspace document.
    /// </summary>
    public void AddLocalReferences()
    {
        if (m_solution == null)
            throw new InvalidOperationException("No solution open");
        if (m_workspaceDocument == null)
            throw new InvalidOperationException("No workspace document");

        IEnumerable<string>? files = m_dialogService.ShowAddReferencesFileDialog(m_solution.SolutionFolder);

        if (files == null || !files.Any())
        {
            return;
        }

        string packageId = NuGetPackageIds[SelectedNuGetPackageIndex];

        m_workspaceDocument.AddLocalReferences(packageId, files);

        OnPropertyChanged(nameof(Libraries));

        NotifyCanExecuteChanged();
    }


    /// <summary>
    /// Switches from NuGet references to local references
    /// </summary>
    public async void Switch()
    {
        try
        {
            if (m_solution == null)
                throw new InvalidOperationException("No solution open");
            if (m_workspaceDocument == null)
                throw new InvalidOperationException("No workspace document");

            m_messenger.Send(new ClearStatusMessages());

            List<string> status = await m_solution.Switch(m_workspaceDocument);

            foreach (string message in status)
            {
                m_messenger.Send(new StatusMessage(message));
            }

            NotifyCanExecuteChanged();

            m_messenger.Send(new SwitchMessage(m_solution));
        }
        catch (Exception e)
        {
            m_dialogService.ShowError($"Failed to switch: {e.Message}");
        }
    }

    /// <summary>
    /// Closes solution and saves the workspace file.
    /// </summary>
    public async void CloseSolution()
    {
        try
        {
            if (m_solution == null)
                throw new InvalidOperationException("No solution open");
            if (m_workspaceDocument == null)
                throw new InvalidOperationException("No workspace document");

            // Save next to solution file
            string workspaceFile = $"{m_solution.FilePath}{WorkspaceFileExtension}";

            // Save the current state of the application
            if (m_workspaceDocument.IsDirty)
            {
                await m_storageService.SaveAsync(workspaceFile, m_workspaceDocument);
                m_messenger.Send(new StatusMessage($"Workspace saved: {m_workspaceDocument}"));
            }

            m_workspaceDocument = null;

            NuGetPackageIds = [];

            m_messenger.Send(new ClearStatusMessages());

            m_messenger.Send(new SolutionClosedMessage(m_solution));

            m_solution = null;

            Title = "";

            OnPropertyChanged(nameof(Libraries));
        }
        catch (Exception error)
        {
            m_dialogService.ShowError(error.Message);
        }
        finally
        {
            NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Deletes the object folders.
    /// </summary>
    public async void DeleteObjFolders()
    {
        try
        {
            if (m_solution == null)
                throw new InvalidOperationException("No solution open");

            m_messenger.Send(new ClearStatusMessages());

            List<string> messages = await m_solution.DeleteObjFolders();

            foreach (string message in messages)
            {
                m_messenger.Send(new StatusMessage(message));
            }

        }
        catch (Exception e)
        {
            m_dialogService.ShowError($"Failed to delete obj folders: {e.Message} ");
        }
    }

    /// <summary>
    /// Opens the explorer.
    /// </summary>
    public void OpenExplorer()
    {
        try
        {
            if (m_solution == null)
                throw new InvalidOperationException("No solution open");

            System.Diagnostics.Process.Start("explorer.exe", m_solution.SolutionFolder);
        }
        catch (Exception ex)
        {
            m_dialogService.ShowError($"Failed to open explorer: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses the Visual Studio solution file and all its projects
    /// and populates the NuGet package table.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    private void ParseVsSolutionFile(string filePath)
    {
        m_solution = new VsSolution(filePath);
        m_solution.Load();

        NuGetPackageIds = m_solution.NuGetPackageIds;
    }

    /// <summary>
    /// Update the command states based on the current workspace state.
    /// </summary>
    private void NotifyCanExecuteChanged()
    {
        OpenSolutionCommand.NotifyCanExecuteChanged();
        CloseSolutionCommand.NotifyCanExecuteChanged();
        OpenSolutionCommand.NotifyCanExecuteChanged();
        AddLocalReferencesCommand.NotifyCanExecuteChanged();
        SwitchCommand.NotifyCanExecuteChanged();
        OpenExplorerCommand.NotifyCanExecuteChanged();
        DeleteObjFoldersCommand.NotifyCanExecuteChanged();
    }
}