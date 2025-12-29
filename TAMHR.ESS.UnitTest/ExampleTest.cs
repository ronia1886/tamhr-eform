using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TAMHR.ESS.UnitTest
{
    public class ExampleTest
    {
        [Fact]
        public void BasicTest1()
        {
            Assert.Equal(1, 1);
        }
        [Fact]
        public void BasicTest2()
        {
            Assert.NotNull("test");
        }
        [Fact]
        public void PassingTest()
        {
            Assert.Equal(4, Add(2, 2));
        }

        int Add(int x, int y)
        {
            return x + y;
        }
    }
}
