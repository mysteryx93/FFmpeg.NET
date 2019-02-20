using System;
using System.IO;

namespace EmergenceGuardian.FFmpeg.Services {

    #region Interface

    /// <summary>
    /// Provides methods to access the file system.
    /// </summary>
    public interface IFileSystemService {
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>Whether the file exists.</returns>
        bool Exists(string path);
        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to be deleted.</param>
        void Delete(string path);
        /// <summary>
        /// Returns the file name of the specified path string without the extension.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The file name without extension.</returns>
        string GetFileNameWithoutExtension(string path);
        /// <summary>
        /// Gets a value indicating whether the specified path string contains a root.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>Whether the path string contains a root.</returns>
        bool IsPathRooted(string path);
        /// <summary>
        /// Combines two strings into a path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combination of the two.</returns>
        string Combine(string path1, string path2);
        /// <summary>
        /// Returns the directory information of the specified path string.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>The path of a directory.</returns>
        string GetDirectoryName(string path);
        /// <summary>
        /// Creates a uniquely-named, zero-byte temporary file on disk and returns the full path of that file.
        /// </summary>
        /// <returns>The path to the newly created file.</returns>
        string GetTempFile();
        /// <summary>
        /// Creates a new file, writes specified string to the file, and then closes the file. If the target file already exists, it is overritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        void WriteAllText(string path, string contents);
    }

    #endregion

    /// <summary>
    /// Provides methods to access the file system.
    /// </summary>
    public class FileSystemService : IFileSystemService {
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>Whether the file exists.</returns>
        public bool Exists(string path) => File.Exists(path);

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to be deleted.</param>
        public void Delete(string path) => File.Delete(path);

        /// <summary>
        /// Returns the file name of the specified path string without the extension.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The file name without extension.</returns>
        public string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

        /// <summary>
        /// Gets a value indicating whether the specified path string contains a root.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>Whether the path string contains a root.</returns>
        public bool IsPathRooted(string path) => Path.IsPathRooted(path);

        /// <summary>
        /// Combines two strings into a path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combination of the two.</returns>
        public string Combine(string path1, string path2) => Path.Combine(path1, path2);

        /// <summary>
        /// Returns the directory information of the specified path string.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>The path of a directory.</returns>
        public string GetDirectoryName(string path) => Path.GetDirectoryName(path);

        /// <summary>
        /// Creates a uniquely-named, zero-byte temporary file on disk and returns the full path of that file.
        /// </summary>
        /// <returns>The path to the newly created file.</returns>
        public string GetTempFile() => Path.GetTempFileName();

        /// <summary>
        /// Creates a new file, writes specified string to the file, and then closes the file. If the target file already exists, it is overritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);
    }
}
