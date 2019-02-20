using System;
using System.Diagnostics;
using EmergenceGuardian.FFmpeg.Services;
using Moq;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class FakeProcessFactory : IProcessFactory {
        public FakeProcessFactory() { }

        public virtual IProcess Create() => Create(null);

        public virtual IProcess Create(Process process) {
            var Result = new Mock<IProcess>();
            Result.Setup(x => x.StartInfo).Returns(new ProcessStartInfo());
            Result.Setup(x => x.HasExited).Returns(false);
            Result.Setup(x => x.WaitForExit(It.IsAny<int>())).Callback(() => Result.Setup(x => x.HasExited).Returns(true));
            return Result.Object;
        }
    }
}
