public interface IQueryComand
{
    bool completed { get; }
    bool isReady { get; }
    bool needsPremovement { get; }
    bool cashed { get; }

    void SetUp();
    void UpdateCommand();
    void Execute();
    void Cancel();
}
