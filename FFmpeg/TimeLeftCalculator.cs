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
        private int iterator;
        private bool fullCycle;
        private long lastPos;
        /// <summary>
        /// Gets or sets the total number of frames to encode.
        /// </summary>
        public long FrameCount { get; set; }
        /// <summary>
        /// Gets or sets the number of status entries to store. The larger the number, the slower the time left will change.
        /// </summary>
        public int HistoryLength { get; private set; }
        /// <summary>
        /// After calling Calculate, returns the estimated processing time left.
        /// </summary>
        public TimeSpan ResultTimeLeft { get; private set; }
        /// <summary>
        /// After calling Calculate, returns the estimated processing rate per second.
        /// </summary>
        public double ResultFps { get; private set; }

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
        /// Calculates the time left and fps. Result will be in ResultTimeLeft and ResultFps.
        /// </summary>
        /// <param name="pos">The current frame position.</param>
        public void Calculate(long pos) {
            TimeSpan Result = TimeSpan.Zero;
            progressHistory[iterator] = new KeyValuePair<DateTime, long>(DateTime.Now, pos);
            lastPos = pos;

            // Calculate SampleWorkTime and SampleWorkFrame for each host
            TimeSpan SampleWorkTime = TimeSpan.Zero;
            long SampleWorkFrame = 0;
            int PosFirst = -1;
            if (fullCycle) {
                PosFirst = (iterator + 1) % HistoryLength;
            } else if (iterator > 0)
                PosFirst = 0;

            if (PosFirst > -1) {
                SampleWorkTime += progressHistory[iterator].Key - progressHistory[PosFirst].Key;
                SampleWorkFrame += progressHistory[iterator].Value - progressHistory[PosFirst].Value;
            }

            ResultFps = SampleWorkFrame / SampleWorkTime.TotalSeconds;
            long WorkLeft = FrameCount - pos;
            if (WorkLeft > 0 && ResultFps > 0)
                ResultTimeLeft = TimeSpan.FromSeconds(WorkLeft / ResultFps);


            iterator = (iterator + 1) % HistoryLength;
            if (iterator == 0)
                fullCycle = true;
        }
    }
}
