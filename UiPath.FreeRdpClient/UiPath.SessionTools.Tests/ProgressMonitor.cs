using Nito.AsyncEx;

namespace UiPath.SessionTools.Tests;

internal sealed class ProgressMonitor<T> : IProgress<T>
    where T : IEquatable<T>
{
    private readonly AsyncMonitor _asyncMonitor = new();
    private readonly List<T> _lines = new();

    public async Task WaitForCount(int count, CancellationToken ct)
    {
        using (await _asyncMonitor.EnterAsync(ct))
        {
            while (_lines.Count < count)
            {
                await _asyncMonitor.WaitAsync(ct);
            }
        }
    }

    public async Task WaitForValue(Func<T, bool> predicate, CancellationToken ct)
    {
        int cChecked = 0;

        using (await _asyncMonitor.EnterAsync(ct))
        {
            while (!_lines.Skip(cChecked).Any(predicate))
            {
                cChecked = _lines.Count;
                await _asyncMonitor.WaitAsync(ct);
            }
        }
    }

    async void IProgress<T>.Report(T value)
    {
        using (await _asyncMonitor.EnterAsync())
        {
            _lines.Add(value);
            _asyncMonitor.PulseAll();
        }
    }
}
