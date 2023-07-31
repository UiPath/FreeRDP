using Nito.AsyncEx;

namespace UiPath.SessionTools.Tests;

internal sealed class ProgressMonitor : IProgress<string>
{
    private readonly AsyncMonitor _asyncMonitor = new();
    private readonly List<string> _values = new();

    public async Task WaitForValue(string value, CancellationToken ct)
    {
        using (await _asyncMonitor.EnterAsync(ct))
        {
            while (!_values.Any(c => c.TrimEnd() == value))
            {
                await _asyncMonitor.WaitAsync(ct);
            }
        }
    }

    async void IProgress<string>.Report(string value)
    {
        using (await _asyncMonitor.EnterAsync())
        {
            _values.Add(value);
            _asyncMonitor.PulseAll();
        }
    }
}
