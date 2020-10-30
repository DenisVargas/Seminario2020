public struct HitResult
{
    public bool conected;
    public bool fatalDamage;
    public bool exploded;
    public bool ignited;

    public HitResult(bool conected)
    {
        this.conected = conected;
        fatalDamage = false;
        exploded = false;
        ignited = false;
    }
}
