using System;
using System.Globalization;
using System.Text;

namespace Ghostty.Core.Ssh;

/// <summary>
/// Naming and prompt text for an image pasted into an ssh pane: the image
/// is saved as a <c>.png</c>, uploaded to the shell's folder, and its
/// remote path is typed at the prompt so the program there (an AI coding
/// tool, typically) can open it. Pure, so it is unit-testable.
/// </summary>
public static class ClipboardImageUpload
{
    /// <summary>
    /// The file name for an image pasted at <paramref name="now"/>. Down
    /// to the millisecond, so two pastes in one second do not overwrite
    /// each other on the host.
    /// </summary>
    public static string FileName(DateTime now)
        => "wintty-paste-" + now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + ".png";

    /// <summary>
    /// <paramref name="remotePath"/> as text for a POSIX shell prompt.
    /// Plain path characters go as they are; anything else is single
    /// quoted. A leading <c>~/</c> stays outside the quotes so the shell
    /// still expands it.
    /// </summary>
    public static string PromptText(string remotePath)
    {
        ArgumentNullException.ThrowIfNull(remotePath);

        var prefix = "";
        var rest = remotePath;
        if (rest.StartsWith("~/", StringComparison.Ordinal))
        {
            prefix = "~/";
            rest = rest[2..];
        }

        if (!NeedsQuoting(rest)) return prefix + rest;

        var sb = new StringBuilder(prefix.Length + rest.Length + 2);
        sb.Append(prefix).Append('\'');
        foreach (var c in rest)
        {
            // Close the quote, add an escaped quote, reopen: 'it'\''s'.
            if (c == '\'') sb.Append("'\\''");
            else sb.Append(c);
        }
        return sb.Append('\'').ToString();
    }

    private static bool NeedsQuoting(string s)
    {
        foreach (var c in s)
        {
            if (!(char.IsAsciiLetterOrDigit(c) || c is '/' or '.' or '-' or '_' or '+' or ','))
                return true;
        }
        return false;
    }
}
