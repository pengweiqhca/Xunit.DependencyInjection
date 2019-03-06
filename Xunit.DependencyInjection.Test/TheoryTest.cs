using System.Collections.Generic;

namespace Xunit.DependencyInjection.Test
{
    public class TheoryTest
    {
        [Theory]
        [MemberData(nameof(GetComplexData))]
        public void ComplexParameterizedTest(string arg1, Dictionary<string, string> arg2, Dictionary<string, string> arg3)
        {
        }

        [Theory]
        [MemberData(nameof(GetSimpleData))]
        public void SimpleParameterizedTest(string arg1, int arg2)
        {
            Assert.Equal("Test", arg1);
            Assert.Equal(1, arg2);
        }

        [Theory]
        [InlineData("Test", 1)]
        public void InlineDataTest(string arg1, int arg2)
        {
            Assert.Equal("Test", arg1);
            Assert.Equal(1, arg2);
        }

        public static IEnumerable<object[]> GetSimpleData()
        {
            yield return new object[]
            {
                "Test", 1
            };
        }

        public static IEnumerable<object[]> GetComplexData()
        {
            yield return new object[]
            {
                "Test",
                new Dictionary<string, string>
                {
                    { "Key", "Value"}
                },
                new Dictionary<string, string>
                {
                    { "Key", "Value"}
                }
            };
        }
    }
}
