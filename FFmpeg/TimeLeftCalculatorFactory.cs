using System;
using System.Collections.Generic;

namespace EmergenceGuardian.FFmpeg {

    #region Interface

    /// <summary>
    /// Allows calculating the time left for a FFmpeg process.
    /// </summary>
    public interface ITimeLeftCalculatorFactory {
        /// <summary>
        /// Creates a new instance of the TimeLeftCalculator class.
        /// </summary>
        /// <param name="frameCount">The total number of frames to encode.</param>
        /// <returns>The new TimeLeftCalculator instance.</returns>
        ITimeLeftCalculator Create(long frameCount);
        /// <summary>
        /// Creates a new instance of the TimeLeftCalculator class.
        /// </summary>
        /// <param name="frameCount">The total number of frames to encode.</param>
        /// <param name="historyLength">The number of status entries to store. The larger the number, the slower the time left will change.</param>
        /// <returns>The new TimeLeftCalculator instance.</returns>
        ITimeLeftCalculator Create(long frameCount, int historyLength);
    }

    #endregion

    /// <summary>
    /// Allows calculating the time left for a FFmpeg process.
    /// </summary>
    public class TimeLeftCalculatorFactory : ITimeLeftCalculatorFactory {
        /// <summary>
        /// Creates a new instance of the TimeLeftCalculator class.
        /// </summary>
        /// <param name="frameCount">The total number of frames to encode.</param>
        /// <returns>The new TimeLeftCalculator instance.</returns>
        public ITimeLeftCalculator Create(long frameCount) => new TimeLeftCalculator(frameCount);
        /// <summary>
        /// Creates a new instance of the TimeLeftCalculator class.
        /// </summary>
        /// <param name="frameCount">The total number of frames to encode.</param>
        /// <param name="historyLength">The number of status entries to store. The larger the number, the slower the time left will change.</param>
        /// <returns>The new TimeLeftCalculator instance.</returns>
        public ITimeLeftCalculator Create(long frameCount, int historyLength) => new TimeLeftCalculator(frameCount, historyLength);
    }
}
