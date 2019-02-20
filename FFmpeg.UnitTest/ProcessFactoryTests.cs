using System;
using System.Diagnostics;
using Moq;
using Xunit;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class ProcessFactoryTests {
        protected IProcessFactory SetupFactory() {
            return new ProcessFactory();
        }

        [Fact]
        public void Create_NoArg_ProcessWrapper() {
            var Factory = SetupFactory();

            var Result = Factory.Create();

            Assert.NotNull(Result);
            Assert.IsType<ProcessWrapper>(Result);
            Assert.NotNull(Result.StartInfo);
        }

        [Fact]
        public void CreateWrapper_ProcessArg_WrapperAroundProcess() {
            var Factory = SetupFactory();
            var P = new Process();

            var Result = Factory.Create(P);

            Assert.NotNull(Result);
            Assert.IsType<ProcessWrapper>(Result);
            Assert.Equal(P.StartInfo, Result.StartInfo);
        }

        [Fact]
        public void CreateWrapper_NullArg_ProcessWrapper() {
            var Factory = SetupFactory();

            var Result = Factory.Create(null);

            Assert.NotNull(Result);
            Assert.IsType<ProcessWrapper>(Result);
            Assert.NotNull(Result.StartInfo);
        }
    }
}
