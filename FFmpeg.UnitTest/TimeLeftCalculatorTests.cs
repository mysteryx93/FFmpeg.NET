using System;
using Moq;
using Xunit;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class TimeLeftCalculatorTests {

        #region Utility Functions

        protected const int FrameCount = 200;
        protected const int HistoryLength = 4;
        protected FakeEnvironmentService environment = new FakeEnvironmentService();

        public ITimeLeftCalculator SetupCalc() {
            environment = new FakeEnvironmentService();
            return new TimeLeftCalculator(environment, FrameCount, HistoryLength);
        }

        /// <summary>
        /// Calculates a value while ensuring ResultFps and ResultTimeLeft remain valid.
        /// </summary>
        protected void CalcValidate(ITimeLeftCalculator calc, long frame, int seconds) {
            environment.ChangeTime(seconds);
            calc.Calculate(frame);
            Assert.True(calc.ResultFps >= 0);
            Assert.True(calc.ResultTimeLeft >= TimeSpan.Zero);
        }

        #endregion

        #region Constructors

        [Fact]
        public void Constructor_FrameCount_Success() => new TimeLeftCalculator(FrameCount).Calculate(0);

        [Fact]
        public void Constructor_FrameCountHistoryLength_Success() => new TimeLeftCalculator(FrameCount, HistoryLength).Calculate(0);

        [Fact]
        public void Constructor_WithDependency_Success() => new TimeLeftCalculator(environment, FrameCount, HistoryLength).Calculate(0);

        [Fact]
        public void Constructor_NullDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new TimeLeftCalculator(null, FrameCount, HistoryLength));

        [Fact]
        public void Constructor_MinValues_Success() => new TimeLeftCalculator(environment, 0, 1).Calculate(0);

        [Theory]
        [InlineData(-100, 30)]
        [InlineData(100, -30)]
        [InlineData(100, 0)]
        public void Constructor_InvalidValues_ThrowsException(int frameCount, int historyLength) => Assert.Throws<ArgumentOutOfRangeException>(() => new TimeLeftCalculator(environment, frameCount, historyLength));

        [Fact]
        public void Init_CorrectDefaultValues() {
            var Calc = SetupCalc();

            Assert.Equal(FrameCount, Calc.FrameCount);
            Assert.Equal(HistoryLength, Calc.HistoryLength);
            Assert.Equal(0, Calc.ResultFps);
            Assert.Equal(TimeSpan.Zero, Calc.ResultTimeLeft);
        }

        #endregion

        #region Calc

        [Fact]
        public void Calc_RunSimulation_ValidResults() {
            var Calc = SetupCalc();
            int Frame = 0;

            // Test at pace of 5 fps.
            for (int i = 1; i <= 10; i++) {
                Frame += 5;
                CalcValidate(Calc, Frame, 1);
                if (i > 1) {
                    Assert.Equal(5, Calc.ResultFps);
                    Assert.Equal(40 - i, Calc.ResultTimeLeft.TotalSeconds);
                } else {
                    Assert.Equal(0, Calc.ResultFps);
                    Assert.Equal(0, Calc.ResultTimeLeft.TotalSeconds);
                }
            }

            // Test at pace of 10 frame per 2 seconds, result should remain 5 fps.
            for (int i = 1; i <= 5; i++) {
                Frame += 10;
                CalcValidate(Calc, Frame, 2);
                Assert.Equal(5, Calc.ResultFps);
                Assert.Equal(30 - (i * 2), Calc.ResultTimeLeft.TotalSeconds);
            }

            // Test at pace of 10 fps.
            for (int i = 0; i < 10; i++) {
                Frame += 10;
                CalcValidate(Calc, Frame, 1);
                Assert.InRange<double>(Calc.ResultFps, 5, 10);
                Assert.InRange<double>(Calc.ResultTimeLeft.TotalSeconds, 0, 15);
            }

            Assert.Equal(10, Calc.ResultFps);
            Assert.Equal(0, Calc.ResultTimeLeft.TotalSeconds);
        }

        [Fact]
        public void Calc_Negative_OutputWithinValidRange() {
            var Calc = SetupCalc();

            CalcValidate(Calc, -10, 0);
        }

        [Fact]
        public void Calc_MaxLong_OutputWithinValidRange() {
            var Calc = SetupCalc();

            CalcValidate(Calc, long.MaxValue - 2, 1);
            CalcValidate(Calc, long.MaxValue - 1, 1);
            CalcValidate(Calc, long.MaxValue, 1);
        }

        [Fact]
        public void Calc_DescendingFrame_OutputWithinValidRange() {
            var Calc = SetupCalc();

            CalcValidate(Calc, 10, 1);
            CalcValidate(Calc, 15, 1);
            CalcValidate(Calc, 10, 1);
            CalcValidate(Calc, 5, 1);
            CalcValidate(Calc, 15, 1);
            CalcValidate(Calc, 15, 1);
            CalcValidate(Calc, 15, 0);
            CalcValidate(Calc, 15, 0);
            CalcValidate(Calc, 15, 0);
        }

        #endregion

    }
}
