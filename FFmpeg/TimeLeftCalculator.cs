using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Allows calculating the time left for a FFmpeg process.
    /// </summary>
    public class TimeLeftCalculator {
        private KeyValuePair<DateTime, long>[] progressHistory;
        private int iterator = 0;
        private bool fullCycle = false;
        /// <summary>
        /// Gets or sets the total number of frames to encode.
        /// </summary>
        public long FrameCount { get; private set; }
        /// <summary>
        /// Gets or sets the number of status entries to store. The larger the number, the slower the time left will change.
        /// </summary>
        public int HistoryLength { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TimeLeftCalculator class.
        /// </summary>
        /// <param name="frameCount">The total number of frames to encode.</param>
        public TimeLeftCalculator(long frameCount) : this(frameCount, 20) {
        }

        /// <summary>
        /// Initializes a new instance of the TimeLeftCalculator class.
        /// </summary>
        /// <param name="frameCount">The total number of frames to encode.</param>
        /// <param name="historyLength">The number of status entries to store. The larger the number, the slower the time left will change.</param>
        public TimeLeftCalculator(long frameCount, int historyLength) {
            this.FrameCount = frameCount;
            this.HistoryLength = historyLength;
            progressHistory = new KeyValuePair<DateTime, long>[historyLength];
        }

        /// <summary>
        /// Calculates the time left.
        /// </summary>
        /// <param name="pos">The current frame position.</param>
        /// <returns>The estimated time left.</returns>
        public TimeSpan Calculate(long pos) {
            TimeSpan Result = TimeSpan.Zero;
            progressHistory[iterator] = new KeyValuePair<DateTime, long>(DateTime.Now, pos);

            int PosFirst = -1;
            if (fullCycle) {
                PosFirst = (iterator + 1) % HistoryLength;
            } else if (iterator > 0)
                PosFirst = 0;

            if (PosFirst > -1) {
                TimeSpan SampleWorkTime = progressHistory[iterator].Key - progressHistory[PosFirst].Key;
                long SampleWorkFrame = progressHistory[iterator].Value - progressHistory[PosFirst].Value;
                double ProcessingFps = SampleWorkFrame / SampleWorkTime.TotalSeconds;
                long WorkLeft = FrameCount - pos;
                if (WorkLeft > 0)
                    Result = TimeSpan.FromSeconds(WorkLeft / ProcessingFps);
            }

            iterator = (iterator + 1) % HistoryLength;
            if (iterator == 0)
                fullCycle = true;

            return Result;
        }
    }
}
