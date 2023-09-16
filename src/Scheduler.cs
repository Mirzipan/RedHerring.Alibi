using System.Diagnostics;
using System.Reactive.Disposables;

namespace RedHerring.Alibi;

public sealed class Scheduler : IDisposable
{
    private readonly SortableSet<DeferredEntry> _data = new(32, DeferredEqualityComparer.Comparer);
    private readonly Stopwatch _sw = new();

    /// <summary>
    /// Frame budget in milliseconds
    /// </summary>
    private long _frameBudget;
    /// <summary>
    /// Time when last tick started in milliseconds
    /// </summary>
    private long _time;

    private bool _tickInProgress;

    #region Lifecycle

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startingTime"></param>
    /// <param name="frameBudget">Maximum amount of time in milliseconds to spend on a single Tick</param>
    public Scheduler(TimeSpan startingTime, long frameBudget)
    {
        _time = startingTime.Ticks;
        _frameBudget = frameBudget;
    }

    /// <summary>
    /// Tick the scheduler. This will call scheduled updates, if their due time has come.
    /// </summary>
    public void Tick(TimeSpan time)
    {
        if (_tickInProgress)
        {
            // Multiple ticks are not allowed!
            return;
        }

        _tickInProgress = true;
        _time = time.Ticks;
        TickDeferredUpdates();
        _tickInProgress = false;
    }

    public void Dispose()
    {
        _data.Clear();
        _tickInProgress = false;
    }

    #endregion Lifecycle

    #region Public

    /// <summary>
    /// Changes the frame budget for deferred updates to the specified value.
    /// </summary>
    /// <param name="frameBudget">value in milliseconds</param>
    public void SetFrameBudget(long frameBudget)
    {
        _frameBudget = frameBudget;
    }

    /// <summary>
    /// Schedule an update, optionally a repeating one.
    /// </summary>
    /// <param name="update">Method to call</param>
    /// <param name="dueTime">Time in milliseconds after which to call the method</param>
    /// <returns></returns>
    public IDisposable Schedule(DeferredUpdate update, long dueTime)
    {
        var entry = new DeferredEntry(_time + Math.Max(_frameBudget, dueTime), -1L, update);
        _data.AddOrReplace(entry);
        return Disposable.Create(entry, Remove);
    }

    /// <summary>
    /// Schedule an update, optionally a repeating one.
    /// </summary>
    /// <param name="update">Method to call</param>
    /// <param name="period">Frequency (milliseconds) with which the method will be called.</param>
    /// <returns></returns>
    public IDisposable ScheduleRepeating(DeferredUpdate update, long period)
    {
        var entry = new DeferredEntry(_time + Math.Max(_frameBudget, period), Math.Max(-1L, period),
            update);
        _data.AddOrReplace(entry);
        return Disposable.Create(entry, Remove);
    }

    /// <summary>
    /// Unschedule a previously scheduled update.
    /// </summary>
    /// <param name="update">Method to unschedule</param>
    public void Unschedule(DeferredUpdate update)
    {
        Remove(new DeferredEntry(update));
    }

    /// <summary>
    /// Unschedule all currently scheduled updates.
    /// </summary>
    public void Clear()
    {
        _data.Clear();
    }

    #endregion Public

    #region Private

    private void TickDeferredUpdates()
    {
        _sw.Restart();
        _data.Sort();

        while (_sw.ElapsedMilliseconds <= _frameBudget)
        {
            if (_data.Count == 0)
            {
                break;
            }

            var entry = _data[0];
            long dueTime = entry.InvokedAt + entry.Period;
            if (dueTime >= _time)
            {
                break;
            }

            long delta = _time - entry.InvokedAt;
            _data.RemoveAt(0);

            if (entry.Period > double.Epsilon)
            {
                entry.InvokedAt = _time;
                _data.Add(entry);
            }

            try
            {
                entry.Update.Invoke(delta);
            }
            catch (Exception e)
            {
                continue;
            }
        }
    }

    #endregion Private

    #region Internal

    internal void Remove(DeferredEntry entry)
    {
        _data.Remove(entry);
    }

    #endregion Internal
}