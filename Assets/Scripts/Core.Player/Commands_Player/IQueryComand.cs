public interface IQueryComand
{
    bool completed { get; }
    bool isReady { get; }
    bool cashed { get; }

    void SetUp();
    void Execute();
    void Cancel();
}
