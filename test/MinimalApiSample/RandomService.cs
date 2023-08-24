namespace MinimalApiSample;

public interface IRandomService
{
    int Get();
}

public class RandomService : IRandomService
{
    public int Get() => Random.Shared.Next(10);
}
