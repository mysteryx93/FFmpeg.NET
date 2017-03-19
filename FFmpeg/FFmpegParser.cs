using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Provides internal functions to parse FFmpeg's console output.
    /// </summary>
    internal static class FFmpegParser {
        /// <summary>
        /// Parses the output of FFmpeg to return the info of all input streams.
        /// </summary>
        /// <param name="outputText">The text containing the input streams to parse.</param>
        /// <returns>A list of stream info.</returns>
        internal static List<FFmpegStreamInfo> ParseFileInfo(string outputText, out TimeSpan fileDuration) {
            List<FFmpegStreamInfo> Result = new List<FFmpegStreamInfo>();
            string[] OutLines = outputText.Split('\n');

            // Parse Duration.
            fileDuration = new TimeSpan();
            string DurationTag = "  Duration: ";
            string DurationLine = OutLines.FirstOrDefault(l => l.StartsWith(DurationTag));
            if (DurationLine != null) {
                string[] DurationInfo = DurationLine.Trim().Split(new string[] { ", " }, StringSplitOptions.None);
                string DurationString = DurationInfo[0].Split(' ')[1];
                if (DurationString == "N/A")
                    fileDuration = new TimeSpan(0);
                else {
                    try {
                        fileDuration = TimeSpan.Parse(DurationString, CultureInfo.InvariantCulture);
                    }
                    catch {
                    }
                }
            }

            // Parse input streams.
            List<string> OutStreams = OutLines.Where(l => l.StartsWith("    Stream #0:")).ToList();
            FFmpegStreamInfo ItemInfo;
            foreach (string item in OutStreams) {
                ItemInfo = ParseStreamInfo(item);
                if (ItemInfo != null)
                    Result.Add(ItemInfo);
            }
            return Result;
        }

        /// <summary>
        /// Parses stream info from specified string returned from FFmpeg.
        /// </summary>
        /// <param name="item">A line of text to parse.</param>
        /// <returns>The stream info, or null if parsing failed.</returns>
        internal static FFmpegStreamInfo ParseStreamInfo(string item) {
            item = item.TrimEnd();
            string RawText = item;
            // Within parenthesis, replace ',' with ';' to be able to split properly.
            char[] itemChars = item.ToCharArray();
            bool isInParenthesis = false;
            for (int i=0; i<itemChars.Length; i++) {
                if (itemChars[i] == '(')
                    isInParenthesis = true;
                else if (itemChars[i] == ')')
                    isInParenthesis = false;
                if (isInParenthesis && itemChars[i] == ',')
                    itemChars[i] = ';';
            }
            item = new string(itemChars);

            int PosStart = 14;
            int PosEnd = -1;
            int StreamIndex;
            for (int i = PosStart; i < item.Length; i++) {
                if (!char.IsDigit(item[i])) {
                    PosEnd = i;
                    break;
                }
            }
            if (PosEnd < 0 || !int.TryParse(item.Substring(PosStart, PosEnd - PosStart), out StreamIndex))
                return null;
            // Read StreamType
            PosStart = item.IndexOf(": ", PosStart) + 2;
            PosEnd = item.IndexOf(": ", PosStart);
            if (PosStart < 0 || PosEnd < 0)
                return null;
            string StreamType = item.Substring(PosStart, PosEnd - PosStart);
            // Split stream data
            PosStart = PosEnd + 2;
            string[] StreamInfo = item.Substring(PosStart).Split(new string[] { ", " }, StringSplitOptions.None);
            if (StreamInfo.Count() == 0)
                return null;
            string StreamFormat = StreamInfo[0].Split(' ')[0];

            if (StreamType == "Video") {
                FFmpegVideoStreamInfo V = new FFmpegVideoStreamInfo();
                V.RawText = RawText;
                V.Index = StreamIndex;
                V.Format = StreamFormat;

                // Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1152 kb/s, 25 fps, 25 tbr, 90k tbn
                try {
                    string[] ColorSpaceValues = StreamInfo[1].Split('(', ')');
                    V.ColorSpace = ColorSpaceValues[0];
                    if (ColorSpaceValues.Length > 1) {
                        string[] ColorRange = ColorSpaceValues[1].Split(';');
                        if (ColorRange.Any(c => c == "tv"))
                            V.ColorRange = "tv";
                        else if (ColorRange.Any(c => c == "pc"))
                            V.ColorRange = "pc";
                        if (ColorRange.Any(c => c == "bt601"))
                            V.ColorRange = "bt601";
                        if (ColorRange.Any(c => c == "bt709"))
                            V.ColorRange = "bt709";
                    }
                    string[] Size = StreamInfo[2].Split(new string[] { "x", " [", ":", " ", "]" }, StringSplitOptions.None);
                    V.Width = int.Parse(Size[0], CultureInfo.InvariantCulture);
                    V.Height = int.Parse(Size[1], CultureInfo.InvariantCulture);
                    if (Size.Length > 2 && Size[2] == "SAR") {
                        V.SAR1 = int.Parse(Size[3], CultureInfo.InvariantCulture);
                        V.SAR2 = int.Parse(Size[4], CultureInfo.InvariantCulture);
                        V.PixelAspectRatio = Math.Round((double)V.SAR1 / V.SAR2, 3);
                        V.DAR1 = int.Parse(Size[6], CultureInfo.InvariantCulture);
                        V.DAR2 = int.Parse(Size[7], CultureInfo.InvariantCulture);
                        V.DisplayAspectRatio = Math.Round((double)V.DAR1 / V.DAR2, 3);
                    }
                    string Fps = StreamInfo.First(s => s.EndsWith("fps"));
                    Fps = Fps.Substring(0, Fps.Length - 4);
                    if (Fps != "1k") // sometimes it returns 1k ?
                        V.FrameRate = double.Parse(Fps, CultureInfo.InvariantCulture);
                }
                catch {
                }

                return V;
            } else if (StreamType == "Audio") {
                FFmpegAudioStreamInfo V = new FFmpegAudioStreamInfo();
                V.RawText = RawText;
                V.Index = StreamIndex;
                V.Format = StreamFormat;

                // Stream #0:1[0x1c0]: Audio: mp2, 44100 Hz, stereo, s16p, 224 kb/s
                try {
                    V.SampleRate = int.Parse(StreamInfo[1].Split(' ')[0], CultureInfo.InvariantCulture);
                    V.Channels = StreamInfo[2];
                    V.BitDepth = StreamInfo[3];
                    if (StreamInfo.Length > 4 && StreamInfo[4].Contains(" kb/s"))
                        V.Bitrate = int.Parse(StreamInfo[4].Split(' ')[0]);
                }
                catch {
                }
                return V;
            }
            return null;
        }

        /// <summary>
        /// Parses FFmpeg's progress into an object.
        /// </summary>
        /// <param name="text">The raw output line from FFmpeg.</param>
        /// <returns>A FFmpegProgress object.</returns>
        internal static FFmpegStatus ParseProgress(string text) {
            FFmpegStatus Result = new FFmpegStatus();
            // frame=  929 fps=0.0 q=-0.0 size=   68483kB time=00:00:37.00 bitrate=15162.6kbits/s speed=  74x    
            string[] Values = text.Split('=');
            try {
                Result.Frame = int.Parse(Values[1].TrimStart().Split(' ')[0], CultureInfo.InvariantCulture);
                Result.Fps = float.Parse(Values[2].TrimStart().Split(' ')[0], CultureInfo.InvariantCulture);
                Result.Quantizer = float.Parse(Values[3].TrimStart().Split(' ')[0], CultureInfo.InvariantCulture);
                Result.Size = Values[4].TrimStart().Split(' ')[0];
                Result.Time = TimeSpan.Parse(Values[5].TrimStart().Split(' ')[0], CultureInfo.InvariantCulture);
                Result.Bitrate = Values[6].TrimStart().Split(' ')[0];
                string SpeedString = Values[7].Trim().Split('x')[0];
                if (SpeedString != "N/A")
                    Result.Speed = float.Parse(SpeedString, CultureInfo.InvariantCulture);
            }
            catch {
            }
            return Result;
        }
    }
}
