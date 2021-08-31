namespace Xunit.DependencyInjection.Test
{
    public class SkippableFactTest
    {
        [SkippableFact]
        public void SkipTest()
        {
            Skip.If(true, "Alway skip");

            Assert.False(true);
        }

        [SkippableTheory]
        [InlineData(1)]
        public void TheoryTest(int index)
        {
            Skip.If(true, "Skip " + index);

            Assert.False(true);
        }
    }
}
