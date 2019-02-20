using System;
using System.Collections.Generic;
using Moq;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class FakeProcessManagerFactory : IProcessManagerFactory {
        public FakeProcessManagerFactory() { }

        /// <summary>
        /// Returns the list of instances that were created by the factory.
        /// </summary>
        public virtual List<IProcessManager> Instances { get; private set; } = new List<IProcessManager>();

        public virtual CompletionStatus RunResult { get; set; } = CompletionStatus.Success;

        public virtual IFFmpegConfig Config { get; set; } = Mock.Of<IFFmpegConfig>();

        public virtual int FrameCount { get; set; }

        public virtual IProcessManager Create(ProcessOptions options = null, ProcessStartedEventHandler callback = null) {
            return Create<IProcessManager>(options, callback).Object;
        }

        public virtual IProcessManagerAvs CreateAvs(ProcessOptions options = null, ProcessStartedEventHandler callback = null) {
            var Result = Create<IProcessManagerAvs>(options, callback);
            Result.Setup(x => x.RunAvisynth(It.IsAny<string>())).Callback((string s) => Result.Object.Run("avs2yuv.exe", s)).Returns(RunResult);
            return Result.Object;
        }

        public virtual IProcessManagerFFmpeg CreateFFmpeg(ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            var Result = Create<IProcessManagerFFmpeg>(options, callback);
            Result.Setup(x => x.RunFFmpeg(It.IsAny<string>())).Callback((string s) => Result.Object.Run("ffmpeg.exe", s)).Returns(RunResult);
            return Result.Object;
        }

        public virtual Mock<T> Create<T>(ProcessOptions options = null, ProcessStartedEventHandler callback = null) where T : class, IProcessManager {
            var Result = new Mock<T>();
            // process.SetupAllProperties();
            Result.Setup(x => x.Options).Returns(options);
            Result.Setup(x => x.Run(It.IsAny<string>(), It.IsAny<string>())).Callback((string s, string a) => {
                Result.Setup(x => x.CommandWithArgs).Returns(string.Format(@"""{0}"" {1}", s, a).TrimEnd());
                Result.Setup(x => x.Run(It.IsAny<string>(), It.IsAny<string>())).Returns(RunResult);
                Result.Raise(x => x.ProcessStarted += null, Result, new ProcessStartedEventArgs());
                callback?.Invoke(Result, new ProcessStartedEventArgs());
                var Ffmpeg = Result as Mock<IProcessManagerFFmpeg>;
                if (Ffmpeg != null) {
                    Ffmpeg.Setup(x => x.FileStreams).Returns(new List<FFmpegStreamInfo>() {
                        new FFmpegVideoStreamInfo(), new FFmpegAudioStreamInfo()
                    });
                    Ffmpeg.Raise(x => x.StatusUpdated += null, Result, new StatusUpdatedEventArgs(new FFmpegStatus() { Frame = FrameCount }));
                }
                Result.Raise(x => x.ProcessCompleted += null, Result, new ProcessCompletedEventArgs());
            });
            Instances.Add(Result.Object);
            return Result;
        }

    }
}
