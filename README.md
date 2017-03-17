# FFmpeg.NET
A .NET Wrapper for FFmpeg

## Features
- Manages processes programatically (start, track, cancel)
- Parses FFmpeg's output to track progress and create a rich user interface
- Provides easy access to common functions, and can be extended to call and track any other console process
- Can also call avs2yuv to pipe an Avisynth stream to FFmpeg, and it will manage processes accordingly

## Classes
1. MediaProcesses *(static)*
- GetFFmpegProcesses
- SoftKill *(allows FFmpeg to finish writing its files when canceling)*
2. MediaInfo *(static)*
- GetFileInfo *(can be used as a replacement for MediaInfo.DLL)*
- GetFrameCount
3. MediaMuxer *(static) provides functions to manage streams losslessly*
- Muxe
- Concatenate
- ExtractVideo
- ExtractAudio
4. MediaEncoder *(static)*
- ConvertToAvi
- Encode
5. FFmpegProcess
- Run
- RunFFmpeg
- RunAvisynthToFFmpeg
- RunAvisynth
- RunAsCommand
- Cancel
- +Many properties and events related to the job, process and video.

## Display Options
1. FFmpegDisplayMode
- None
- Native *(standard console box)*
- Interface *(allows implementing a rich user interface via events)*
- ErrorOnly *(displays console output if an error occurs)*
2. ProcessStartOptions
- DisplayMode *(see above)*
- Title *(when displaying custom interface)*
- Priority *(CPU)*
- JobId *(allows several processes to be routed to the same UI)*
- IsMainTask *(when implementing custom UI, whether to track the output of this process)*
- FrameCount *(if input file doesn't provide frame count, such as when using pipe input, you can provide it here)*
- ResumePos *(set this if you're resuming a job to facilitate the UI)*
- Timeout *(cancels the task after a period)*
- event Started *(allows the caller to grab the FFmpegProcess)*

## How To Use
Before using these classes, you must configure FFmpegConfig.
- FFmpegPath *(default = "ffmpeg.exe")*
- Avs2yuvPath *(default = "avs2yuv.exe")*
- UserInterfaceManager *(create your derived class here to create an UI)*

By default, canceling tasks is done with a soft kill to let FFmpeg close its files in a clean state. 
**This implementation, however, won't work for Console Applications.** If you are using this class in a 
console application, you must handle FFmpegProcess.CloseProcess event and implement a work-around as 
described [here](http://stackoverflow.com/a/29274238/3960200); or simply call Process.Kill if you don't mind a hard kill.

## Cool Usages

1. **Avisynth Pause/Resume.** Encoding videos with Avisynth can take a long time, and crashes may occur, or you may
want to use your computer for something else while it is running. The provided interface makes it easy to run 
Avisynth via avs2yuv and pipe the output to FFmpeg. The user can then click "Cancel" to stop the job in a clean state,
and you can count the number of encoded frames, edit the script to start at that position, and resume from there. 
Once the whole video has been processed, you can use MediaMuxer.Concatenate to merge all the segments and voilà!

2. **Multi-Tasking.** You may need to run several tasks, some in parallel, some in sequence. For example, 
while the video is encoding, you may want to extract the audio to WAV and run OpusEnc or NeroAac to encode the audio in parallel.
Once both the audio and video and done, you then may want to muxe them back together. You can manage that whole process in the
same UI window by first calling FFmpegConfig.UserInterfaceManager.Start with a JobId and calling FFmpegConfig.UserInterfaceManager.End 
when done. All processes started with that JobId will be routed to the same UI. Tasks with IsMainTask will be parsed allowing 
you to display FFmpeg's progress with a progress bar, while tasks with IsMainTask=false will allow displaying 
"Extracting audio", "Encoding Audio" and "Muxing Audio and Video" while those are running in the background. You can also easily
track and cancel the whole chain of commands.

See ExampleApplication to view FFmpeg.NET in action.

## About The Author

[Etienne Charland](https://www.spiritualselftransformation.com), known as the Emergence Guardian, helps starseeds reconnect with their soul power to accomplish the purpose 
they came here to do. [You can read his book at Satrimono Publishing.](https://satrimono.com/) Warning: non-geek zone.
