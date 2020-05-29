public interface IQueryComand
{
    bool completed { get; }

    void Execute();
    void Cancel();
}
