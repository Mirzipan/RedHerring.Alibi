namespace RedHerring.Alibi;

internal sealed class DeferredEqualityComparer: IEqualityComparer<DeferredEntry>
{
    public static readonly DeferredEqualityComparer Comparer = new();
        
    public bool Equals(DeferredEntry x, DeferredEntry y)
    {
        return Equals(x.Update, y.Update);
    }

    public int GetHashCode(DeferredEntry obj)
    {
        return (obj.Update != null ? obj.Update.GetHashCode() : 0);
    }
}