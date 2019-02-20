using System;
using Moq;
using Xunit;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class TimeLeftCalculatorFactoryTests {

        public ITimeLeftCalculatorFactory SetupFactory() {
            return new TimeLeftCalculatorFactory();
        }

        [Fact]
        public void Constructor_Empty_Success() => new TimeLeftCalculatorFactory().Create(0);

        [Fact]
        public void Constructor_Dependency_Success() => new TimeLeftCalculatorFactory(Mock.Of<IEnvironmentService>()).Create(0);

        [Fact]
        public void Constructor_NullDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new TimeLeftCalculatorFactory(null));

        [Theory]
        [InlineData(100)]
        public void Create_1Param_ValidInstance(int frameCount) {
            var Factory = SetupFactory();

            var Result = Factory.Create(frameCount);

            Assert.NotNull(Result);
            Assert.IsType<TimeLeftCalculator>(Result);
            Assert.Equal(frameCount, Result.FrameCount);
        }

        [Theory]
        [InlineData(100, 30)]
        public void Create_2Params_ValidInstance(int frameCount, int historyLength) {
            var Factory = SetupFactory();

            var Result = Factory.Create(frameCount, historyLength);

            Assert.NotNull(Result);
            Assert.IsType<TimeLeftCalculator>(Result);
            Assert.Equal(frameCount, Result.FrameCount);
            Assert.Equal(historyLength, Result.HistoryLength);
        }
    }
}
