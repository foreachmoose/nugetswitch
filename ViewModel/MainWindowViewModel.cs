using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibGit2Sharp;
using NuGetSwitch.Helper;
using NuGetSwitch.Model;
using NuGetSwitch.Service;

namespace NuGetSwitch.ViewModel
{
    /// <summary>
    /// Main window view model for the NuGetSwitch application.
    /// 
    /// Implements the <see cref="ObservableObject" /></summary>
    /// <seealso cref="ObservableObject" />
    public class MainWindowViewModel : ObservableObject
    {
        // Workspace document file extension
        private const string FileExtension = ".nugetswitch.tmp";

        // Services
        private readonly IDialogService m_dialogService;
        private readonly StorageService<WorkspaceDocument> m_storageService;

        private string m_title;
        private int m_selectedNuGetPackageIndex;
        private string? m_solutionFilePath;
        private WorkspaceDocument? m_workspaceDocument;
        private List<VsProject> m_projects = [];
        private ObservableCollection<string> m_nuGetPackageIds = [];
        private ObservableCollection<string> m_messages = [];
        

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
        /// </summary>
        /// <param name="dialogService">The file service.</param>
        /// <param name="storageService">The storage service.</param>
        public MainWindowViewModel(IDialogService dialogService, StorageService<WorkspaceDocument> storageService)
        {
            m_dialogService = dialogService;
            m_storageService = storageService;
            m_title = "";

            OpenSolutionCommand = new RelayCommand(OpenSolution, CanExecuteOpenSolution);
            CloseSolutionCommand = new RelayCommand(CloseSolution, CanExecuteCloseSolution);
            AddLocalReferencesCommand = new RelayCommand(AddLocalReferences, CanExecuteAddLocalReferences);
            SwitchCommand = new RelayCommand(Switch, CanExecuteSwitch);
            GitUndoChangesCommand = new RelayCommand(GitUndoChanges, CanExecuteGitUndoChanges);
            OpenExplorerCommand = new RelayCommand(OpenExplorer, CanExecuteOpenExplorer);
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
        /// Gets the git undo changes command.
        /// </summary>
        /// <value>The git undo changes command.</value>
        public RelayCommand GitUndoChangesCommand { get; }

        /// <summary>
        /// Gets the open explorer command.
        /// </summary>
        /// <value>The open explorer command.</value>
        public RelayCommand OpenExplorerCommand { get; }

        /// <summary>
        /// Determines whether this instance [can execute close workspace].
        /// </summary>
        /// <returns>System.Boolean.</returns>
        private bool CanExecuteCloseSolution()
        {
            return m_solutionFilePath != null;
        }

        /// <summary>
        /// Determines whether this instance [can execute open solution].
        /// </summary>
        /// <returns>System.Boolean.</returns>
        private bool CanExecuteOpenSolution()
        {
            return m_solutionFilePath == null;
        }

        /// <summary>
        /// Determines whether this instance [can execute add local references].
        /// </summary>
        /// <returns>System.Boolean.</returns>
        private bool CanExecuteAddLocalReferences()
        {
            return SelectedNuGetPackageIndex >= 0 && SelectedNuGetPackageIndex < NuGetPackageIds.Count;
        }

        /// <summary>
        /// Determines whether this instance [can execute switch].
        /// </summary>
        /// <returns>System.Boolean.</returns>
        private bool CanExecuteSwitch()
        {
            return m_workspaceDocument != null && m_workspaceDocument.AnySelectedLibraries();
        }

        /// <summary>
        /// Determines whether this instance [can execute git undo changes].
        /// </summary>
        /// <returns>System.Boolean.</returns>
        private bool CanExecuteGitUndoChanges()
        {
            return ModifiedProjectFiles.Any();
        }

        /// <summary>
        /// Determines whether this instance [can open explorer].
        /// </summary>
        /// <returns>System.Boolean.</returns>
        private bool CanExecuteOpenExplorer()
        {
            return m_solutionFilePath != null;
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
        /// Gets or sets the nu get package ids.
        /// </summary>
        /// <value>The nu get package ids.</value>
        public ObservableCollection<string> NuGetPackageIds
        {
            get => m_nuGetPackageIds;
            set
            {
                SetProperty(ref m_nuGetPackageIds, value);
                OnPropertyChanged(nameof(Libraries));
            }
        }

        /// <summary>
        /// Gets or sets the messages.
        /// </summary>
        /// <value>The messages.</value>
        public ObservableCollection<string> Messages
        {
            get => m_messages;
            set => SetProperty(ref m_messages, value);
        }

        /// <summary>
        /// Gets or sets the index of the selected nu get package.
        /// </summary>
        /// 
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
        /// Gets the libraries.
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
                throw new InvalidOperationException("No workspace docuement");

            string packageId = NuGetPackageIds[SelectedNuGetPackageIndex];

            m_workspaceDocument.RemoveLibraries(packageId, toRemove);

            OnPropertyChanged(nameof(Libraries));
        }


        /// <summary>
        /// Gets the is dirty.
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
                string workspaceFile = $"{filePath}{FileExtension}";
                try
                {
                    WorkspaceDocument? document = await m_storageService.LoadAsync(workspaceFile);
                    if (document is not null)
                    {
                        m_workspaceDocument = document;
                        Messages.Add($"Workspace document found for: {workspaceFile}");
                    }
                    else
                    {
                        m_workspaceDocument = new WorkspaceDocument();
                        Messages.Add($"No workspace document found for {filePath}");
                    }
                }
                catch (Exception e)
                {
                    m_workspaceDocument = new WorkspaceDocument();
                    Messages.Add($"Failed to load workspace document: {e.Message}");
                }
                
                ParseVsSolutionFile(filePath);

                m_solutionFilePath = filePath;

                Title = filePath;

                UpdateGitStatus();
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
            if (m_solutionFilePath == null)
                throw new InvalidOperationException("No solution open");
            if (m_workspaceDocument == null)
                throw new InvalidOperationException("No workspace document");

            string solutionFolder = Path.GetDirectoryName(m_solutionFilePath);
            IEnumerable<string>? files = m_dialogService.ShowAddReferencesFileDialog(solutionFolder);

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
        /// Gets the modified project files.
        /// </summary>
        /// <value>The modified project files.</value>
        public ObservableCollection<string> ModifiedProjectFiles { get; } = [];


        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void UpdateGitStatus()
        {
            ModifiedProjectFiles.Clear();

            string solutionFolder = Path.GetDirectoryName(m_solutionFilePath);
            string repoPath = Repository.Discover(solutionFolder);

            if (repoPath == null)
            {
                Messages.Add($"No Git repository found in the solution folder: {solutionFolder}");
                return;
            }

            using Repository repo = new Repository(repoPath);

            RepositoryStatus status = repo.RetrieveStatus();

            foreach (StatusEntry entry in status.Where(e => e.State.HasFlag(FileStatus.ModifiedInWorkdir) && e.FilePath.EndsWith("csproj")))
            {
                ModifiedProjectFiles.Add(entry.FilePath);
            }
        }

        /// <summary>
        /// Gits the undo changes.
        /// </summary>
        public void GitUndoChanges()
        {
            Messages.Clear();

            string solutionFolder = Path.GetDirectoryName(m_solutionFilePath);
            string repoPath = Repository.Discover(solutionFolder);

            if (repoPath == null)
            {
                Messages.Add($"No Git repository found in the solution folder: {solutionFolder}");
                return;
            }

            Messages.Add("Reverting changes");

            using (Repository repo = new Repository(repoPath))
            {
                repo.CheckoutPaths("HEAD", ModifiedProjectFiles, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
                Messages.Add($"Reverted {ModifiedProjectFiles.Count} modified project files to HEAD.");
            }

            ParseVsSolutionFile(m_solutionFilePath);

            UpdateGitStatus();

            NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Switches this instance.
        /// </summary>
        public void Switch()
        {
            if (m_workspaceDocument == null)
                throw new InvalidOperationException("No workspace document");

            Messages.Clear();

            // Loop through all projects and update the NuGet packages
            foreach (VsProject project in m_projects)
            {
                string solutionFolder = Path.GetDirectoryName(m_solutionFilePath);
                string fullPath = Path.Combine(solutionFolder, project.ProjectPath);

                Messages.Add($"Updating project: {fullPath}");

                // Update each nuget in the project
                foreach (NuGetPackage package in project.NuGetPackages)
                {
                    IList<string>? selectedLibraries = m_workspaceDocument.GetSelectedLibraries(package.PackageId);

                    if (selectedLibraries == null || selectedLibraries.Count == 0)
                    {
                        Messages.Add($"\tSkipping: {package.PackageId}, no dlls selected");
                        continue;
                    }

                    Messages.Add($"\tUpdating: {package.PackageId}");

                    // Remove existing PackageReference elements for this package
                    Messages.Add($"\t\tRemove package: {package.PackageId}");
                    VsProjectFileHelper.RemoveNuGetPackageReference(fullPath, package.PackageId);

                    List<(string, string)> references = [];

                    foreach (string libPath in selectedLibraries)
                    {
                        string? basePath = Path.GetDirectoryName(fullPath);
                        if (basePath == null)
                            throw new InvalidOperationException("Invalid base path");
                        string relPath = PathHelper.GetRelativePath(libPath, basePath);
                        string name = Path.GetFileNameWithoutExtension(relPath);

                        Messages.Add($"\t\tAdd reference: {relPath}");

                        references.Add((name, relPath));
                    }

                    VsProjectFileHelper.AddDllReferences(fullPath, references);
                }
            }

            UpdateGitStatus();
            
            NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Saves the workspace.
        /// </summary>
        public async void CloseSolution()
        {
            try
            {
                if (m_workspaceDocument == null)
                    throw new InvalidOperationException("No workspace document");

                // Save next to solution file
                string workspaceFile = $"{m_solutionFilePath}{FileExtension}";

                // Save the current state of the application
                if (m_workspaceDocument.IsDirty)
                {
                    await m_storageService.SaveAsync(workspaceFile, m_workspaceDocument);
                }

                m_workspaceDocument = null;
                
                NuGetPackageIds.Clear();
                Messages.Clear();
                ModifiedProjectFiles.Clear();

                m_solutionFilePath = null;

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
        /// Opens the explorer.
        /// </summary>
        public void OpenExplorer()
        {
            try
            {
                string solutionPath = Path.GetDirectoryName(m_solutionFilePath);
                System.Diagnostics.Process.Start("explorer.exe", solutionPath);
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
            NuGetPackageIds.Clear();

            // Get the solution file folder
            string? solutionFolder = Path.GetDirectoryName(filePath);

            // Read VS projects from the solution file
            m_projects = VsSolutionFileHelper.GetProjectsFromSolution(filePath);

            foreach (VsProject project in m_projects)
            {
                string fullPath = Path.Combine(solutionFolder, project.ProjectPath);

                List<NuGetPackage> packages = VsProjectFileHelper.GetNuGetPackagesFromProject(fullPath);

                foreach (NuGetPackage package in packages)
                {
                    project.NuGetPackages.Add(package);

                    if (!NuGetPackageIds.Contains(package.PackageId))
                    {
                        NuGetPackageIds.Add(package.PackageId);
                    }
                }
            }
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
            GitUndoChangesCommand.NotifyCanExecuteChanged();
            OpenExplorerCommand.NotifyCanExecuteChanged();
        }
    }
}
