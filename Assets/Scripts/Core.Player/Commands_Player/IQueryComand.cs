public interface IQueryComand
{
    bool completed { get; }
    bool isReady { get; }
    bool cashed { get; }

    void Cancel();
    void Execute();
    void SetUp();
}
