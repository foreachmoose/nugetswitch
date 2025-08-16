using NuGetSwitch.Model;


namespace NuGetSwitch.Message;

/// <summary>
/// Message indicating that a Visual Studio solution has been opened.
/// Carries the <see cref="VsSolution"/> instance representing the opened solution.
/// </summary>
public sealed record SolutionOpenedMessage(VsSolution Solution);

/// <summary>
/// Message indicating that a Visual Studio solution has been closed.
/// Carries the <see cref="VsSolution"/> instance representing the closed solution.
/// </summary>
public sealed record SolutionClosedMessage(VsSolution Solution);

/// <summary>
/// Message indicating that a NuGet package switch operation has been performed on a solution.
/// Carries the <see cref="VsSolution"/> instance for which the switch was executed.
/// </summary>
public sealed record SwitchMessage(VsSolution Solution);

/// <summary>
/// Message containing a status update string for display in the UI or logs.
/// </summary>
public sealed record StatusMessage(string Status);

/// <summary>
/// Message used to request clearing of all status messages in the UI or message log.
/// </summary>
public sealed record ClearStatusMessages;
