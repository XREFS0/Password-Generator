using System.Security.Cryptography;
using System.Text;
using System.Windows;
using MASA.PasswordGenerator.Core.Interfaces;

namespace MASA.PasswordGenerator.Infrastructure.Clipboard;

public class SafeClipboardService : IClipboardService
{
    private string? _lastCopiedHash;
    private CancellationTokenSource? _autoClearCts;

    public async Task CopyToClipboardAsync(string text, bool autoClear, int autoClearSeconds = 30)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        // Compute hash of the copied text to track clipboard ownership
        string currentHash = ComputeSha256(text);
        _lastCopiedHash = currentHash;

        // Cancel previous timer if any
        _autoClearCts?.Cancel();
        _autoClearCts?.Dispose();
        _autoClearCts = new CancellationTokenSource();

        // WPF Clipboard copy on UI thread dispatcher
        if (System.Windows.Application.Current != null)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                System.Windows.Clipboard.SetDataObject(text, false);
            });
        }

        if (autoClear && autoClearSeconds > 0)
        {
            var token = _autoClearCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(autoClearSeconds), token);

                    if (!token.IsCancellationRequested && System.Windows.Application.Current != null)
                    {
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                if (System.Windows.Clipboard.ContainsText())
                                {
                                    string currentClipboard = System.Windows.Clipboard.GetText();
                                    string clipboardHash = ComputeSha256(currentClipboard);

                                    // Only clear if the clipboard STILL contains our copied password
                                    if (clipboardHash == _lastCopiedHash)
                                    {
                                        System.Windows.Clipboard.Clear();
                                    }
                                }
                            }
                            catch
                            {
                                // Prevent any clipboard access collision exceptions
                            }
                        });
                    }
                }
                catch (TaskCanceledException)
                {
                    // Clean cancellation
                }
            }, token);
        }
    }

    public void ClearClipboard()
    {
        _autoClearCts?.Cancel();
        try
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.Clipboard.Clear();
                });
            }
        }
        catch
        {
            // Clipboard access fallback
        }
    }

    private static string ComputeSha256(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes);
    }
}
