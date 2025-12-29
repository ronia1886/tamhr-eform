using System;
using System.Text.Json;
using TAMHR.ESS.Domain.Models.TimeManagement;
using Xunit;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class ProxyTimeLogTest
    {
        [Fact]
        public void Can_Create_And_Set_All_Fields()
        {
            var now = new DateTime(2025, 3, 1, 8, 0, 0);
            var later = new DateTime(2025, 3, 1, 17, 0, 0);

            var log = new ProxyTimeLog
            {
                NoReg = "EMP001",
                ProxyInBefore = now,
                ProxyOutBefore = later,
                ProxyInAfter = now.AddMinutes(-10),
                ProxyOutAfter = later.AddMinutes(5),
                ChangeState = "After",
                ChangeTime = new DateTime(2025, 3, 1, 18, 0, 0)
            };

            Assert.Equal("EMP001", log.NoReg);
            Assert.Equal(now, log.ProxyInBefore);
            Assert.Equal(later, log.ProxyOutBefore);
            Assert.Equal(now.AddMinutes(-10), log.ProxyInAfter);
            Assert.Equal(later.AddMinutes(5), log.ProxyOutAfter);
            Assert.Equal("After", log.ChangeState);
            Assert.Equal(new DateTime(2025, 3, 1, 18, 0, 0), log.ChangeTime);
        }

        [Fact]
        public void Accepts_Null_Datetimes()
        {
            var log = new ProxyTimeLog
            {
                NoReg = "EMP002",
                ProxyInBefore = null,
                ProxyOutBefore = null,
                ProxyInAfter = null,
                ProxyOutAfter = null,
                ChangeState = "Before",
                ChangeTime = new DateTime(2025, 4, 1, 9, 0, 0)
            };

            Assert.Equal("EMP002", log.NoReg);
            Assert.Null(log.ProxyInBefore);
            Assert.Null(log.ProxyOutBefore);
            Assert.Null(log.ProxyInAfter);
            Assert.Null(log.ProxyOutAfter);
            Assert.Equal("Before", log.ChangeState);
            Assert.Equal(new DateTime(2025, 4, 1, 9, 0, 0), log.ChangeTime);
        }

        [Theory]
        [InlineData("Before")]
        [InlineData("After")]
        [InlineData("before")] // case-insensitive scenario yang kadang muncul
        public void ChangeState_Allows_Conventional_Values(string state)
        {
            var log = new ProxyTimeLog { ChangeState = state, ChangeTime = DateTime.UtcNow };
            Assert.Equal(state, log.ChangeState);
        }

        [Fact]
        public void Json_RoundTrip_Should_Preserve_Set_Values()
        {
            var original = new ProxyTimeLog
            {
                NoReg = "EMP003",
                ProxyInBefore = new DateTime(2025, 5, 10, 7, 0, 0),
                ProxyOutBefore = new DateTime(2025, 5, 10, 12, 0, 0),
                ProxyInAfter = new DateTime(2025, 5, 10, 6, 50, 0),
                ProxyOutAfter = new DateTime(2025, 5, 10, 12, 15, 0),
                ChangeState = "After",
                ChangeTime = new DateTime(2025, 5, 10, 12, 30, 0)
            };

            var json = JsonSerializer.Serialize(original);
            Assert.False(string.IsNullOrWhiteSpace(json));

            var copy = JsonSerializer.Deserialize<ProxyTimeLog>(json);
            Assert.NotNull(copy);

            Assert.Equal(original.NoReg, copy.NoReg);
            Assert.Equal(original.ProxyInBefore, copy.ProxyInBefore);
            Assert.Equal(original.ProxyOutBefore, copy.ProxyOutBefore);
            Assert.Equal(original.ProxyInAfter, copy.ProxyInAfter);
            Assert.Equal(original.ProxyOutAfter, copy.ProxyOutAfter);
            Assert.Equal(original.ChangeState, copy.ChangeState);
            Assert.Equal(original.ChangeTime, copy.ChangeTime);
        }

        [Fact]
        public void Example_Duration_Computation_Is_Positive_When_After_Adjusts_Extends()
        {
            // Contoh logika di luar model (business check sederhana)
            var log = new ProxyTimeLog
            {
                ProxyInBefore = new DateTime(2025, 6, 1, 8, 0, 0),
                ProxyOutBefore = new DateTime(2025, 6, 1, 12, 0, 0),
                ProxyInAfter = new DateTime(2025, 6, 1, 7, 50, 0),
                ProxyOutAfter = new DateTime(2025, 6, 1, 12, 15, 0),
                ChangeState = "After",
                ChangeTime = new DateTime(2025, 6, 1, 13, 0, 0)
            };

            TimeSpan before = (log.ProxyOutBefore ?? DateTime.MinValue) - (log.ProxyInBefore ?? DateTime.MinValue);
            TimeSpan after = (log.ProxyOutAfter ?? DateTime.MinValue) - (log.ProxyInAfter ?? DateTime.MinValue);

            Assert.True(before > TimeSpan.Zero);
            Assert.True(after > TimeSpan.Zero);
            Assert.True(after > before); // sesudah koreksi durasi bertambah
        }
    }
}
