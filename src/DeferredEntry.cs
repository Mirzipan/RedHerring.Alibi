namespace RedHerring.Alibi;

internal struct DeferredEntry: IComparable<DeferredEntry>
{
    public long InvokedAt;
    public long Period;
    public DeferredUpdate Update;

    #region Lifecycle

    public DeferredEntry(DeferredUpdate update) : this(0L, 0L, update)
    {
    }

    public DeferredEntry(long invokedAt, long period, DeferredUpdate update)
    {
        InvokedAt = invokedAt;
        Period = period;
        Update = update;
    }

    #endregion Lifecycle

    #region Comparable

    public int CompareTo(DeferredEntry other)
    {
        return (InvokedAt + Period).CompareTo((other.InvokedAt + other.Period));
    }

    #endregion Comparable
}