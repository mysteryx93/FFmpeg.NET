using System;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class FakeEnvironmentService : IEnvironmentService {
        public DateTime CurrentTime = new DateTime(2019, 01, 01);

        public void ChangeTime(int seconds) {
            CurrentTime = CurrentTime.AddSeconds(seconds);
        }

        public DateTime Now => CurrentTime;

        public DateTime UtcNow => CurrentTime.AddHours(6);

        public string NewLine => Environment.NewLine;
    }
}
