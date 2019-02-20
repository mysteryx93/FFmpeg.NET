using System;
using System.IO;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class FakeFileSystemService : IFileSystemService {
        public virtual string Combine(string path1, string path2) => Path.Combine(path1, path2);

        public virtual void Delete(string path) { }

        public virtual bool Exists(string path) => true;

        public virtual string GetDirectoryName(string path) => Path.GetDirectoryName(path);

        public virtual string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

        public virtual bool IsPathRooted(string path) => Path.IsPathRooted(path);

        public string GetTempFile() => "temp";

        public virtual void WriteAllText(string path, string contents) { }
    }
}
