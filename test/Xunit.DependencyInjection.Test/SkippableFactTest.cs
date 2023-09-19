using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System.Runtime.CompilerServices;

namespace Xunit.DependencyInjection.Test;

public class SkippableFactTest
{
    public SkippableFactTest(IDependency _) { }

    [SkippableFact]
    public TestAwaitable SkipTest() => new();

    [SkippableTheory]
    [InlineData(1)]
    public FSharpAsync<Unit> TheoryTest(int index) => FSharpAsync.AwaitTask(Delay(index));

    private static async Task Delay(int index)
    {
        await Task.Delay(100);

        Skip.If(true, "Skip " + index);
    }

    public class TestAwaitable
    {
        private bool _isCompleted;

        private readonly List<Action> _onCompletedCallbacks = new();

        public TestAwaitable()
        {
            // Simulate a brief delay before completion
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(100);

                SetCompleted();
            });
        }

        private void SetCompleted()
        {
            _isCompleted = true;

            foreach (var callback in _onCompletedCallbacks) callback();
        }

        public TestAwaiter GetAwaiter() => new(this);

        public readonly struct TestAwaiter : INotifyCompletion
        {
            private readonly TestAwaitable _owner;

            public TestAwaiter(TestAwaitable owner) : this() => _owner = owner;

            public bool IsCompleted => _owner._isCompleted;

            public void OnCompleted(Action continuation)
            {
                if (_owner._isCompleted) continuation();
                else _owner._onCompletedCallbacks.Add(continuation);
            }

            public void GetResult() => Skip.If(true, "Alway skip");
        }
    }
}
