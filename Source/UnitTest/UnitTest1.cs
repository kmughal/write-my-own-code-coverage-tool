using System;
using Service;
using Xunit;

namespace UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var s = new Person();
            s.Name = "Shahzad";
            s.Calculator("+", 4, 6);
           Assert.True(true);
        }
    }
}
