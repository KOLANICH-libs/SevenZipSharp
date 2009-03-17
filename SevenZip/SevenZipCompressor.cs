﻿/*  This file is part of SevenZipSharp.

    SevenZipSharp is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    SevenZipSharp is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with SevenZipSharp.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using SevenZip.ComRoutines;
using SevenZip.Sdk;
using SevenZip.Sdk.Compression.Lzma;

namespace SevenZip
{
    /// <summary>
    /// Class for packing files into 7-zip archives
    /// </summary>
    public sealed class SevenZipCompressor : SevenZipBase, ISevenZipCompressor
    {
        /// <summary>
        /// Changes the path to the 7-zip native library
        /// </summary>
        /// <param name="libraryPath">The path to the 7-zip native library</param>
        public static void SetLibraryPath(string libraryPath)
        {
            SevenZipLibraryManager.SetLibraryPath(libraryPath);
        }
        /// <summary>
        /// Initializes a new instance of the SevenZipCompressor class 
        /// </summary>
        public SevenZipCompressor() : base() { }
        /// <summary>
        /// Initializes a new instance of the SevenZipCompressor class 
        /// </summary>
        /// <param name="reportErrors">Throw exceptions on compression errors</param>
        public SevenZipCompressor(bool reportErrors)
            : base(reportErrors) { }
        /// <summary>
        /// Finds the common root of file names
        /// </summary>
        /// <param name="files">Array of file names</param>
        /// <returns>Common root</returns>
        private static string CommonRoot(string[] files)
        {
            List<string[]> splittedFileNames = new List<string[]>(files.Length);
            foreach (string fn in files)
            {
                splittedFileNames.Add(fn.Split('\\'));
            }
            int minSplitLength = splittedFileNames[0].Length;
            if (files.Length > 1)
            {
                for (int i = 1; i < files.Length; i++)
                {
                    if (minSplitLength > splittedFileNames[i].Length)
                    {
                        minSplitLength = splittedFileNames[i].Length;
                    }
                }
            }
            string res = "";
            for (int i = 0; i < minSplitLength; i++)
            {
                bool common = true;
                for (int j = 1; j < files.Length; j++)
                {
                    if (!(common &= splittedFileNames[j - 1][i] == splittedFileNames[j][i]))
                    {
                        break;
                    }
                }
                if (common)
                {
                    res += splittedFileNames[0][i] + '\\';
                }
                else
                {
                    break;
                }
            }
            if (String.IsNullOrEmpty(res))
            {
                throw new SevenZipInvalidFileNamesException("files must be on the same logical disk!");
            }
            return res;
        }
        /// <summary>
        /// Validates the common root
        /// </summary>
        /// <param name="commonRoot">Common root of the file names</param>
        /// <param name="files">Array of file names</param>
        private static void CheckCommonRoot(string[] files, ref string commonRoot)
        {
            if (commonRoot.EndsWith("\\", StringComparison.CurrentCulture))
            {
                commonRoot = commonRoot.Substring(0, commonRoot.Length - 1);
            }

            foreach (string fn in files)
            {
                if (!fn.StartsWith(commonRoot, StringComparison.CurrentCulture))
                {
                    throw new SevenZipInvalidFileNamesException("invalid common root.");
                }
            }
        }
        /// <summary>
        /// Ensures that directory directory is not empty
        /// </summary>
        /// <param name="directory">Directory name</param>
        /// <returns>False if is not empty</returns>
        private bool RecursiveDirectoryEmptyCheck(string directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            if (di.GetFiles().Length > 0)
            {
                return false;
            }
            bool empty = true;
            foreach (DirectoryInfo cdi in di.GetDirectories())
            {
                empty &= RecursiveDirectoryEmptyCheck(cdi.FullName);
                if (!empty)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Checks if specified List of FileInfo has an item with specified FullName
        /// </summary>
        /// <param name="fullName">Item's FullName to find</param>
        /// <param name="list">List of FileInfo to check</param>
        /// <returns>true if the list has an item with specified FullName</returns>
        private static bool ListHasFileInfo(List<FileInfo> list, string fullName)
        {
            foreach (FileInfo fi in list)
            {
                if (fi.FullName == fullName)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Makes special FileInfo array for the archive file table
        /// </summary>
        /// <param name="files">Array of files to pack</param>
        /// <param name="commonRoot">Common rooot of the file names</param>
        /// <param name="rootLength">Length of the common root of file names</param>
        /// <returns>Special FileInfo array for the archive file table</returns>
        private static FileInfo[] ProduceFileInfoArray(string[] files, string commonRoot, out int rootLength)
        {
            List<FileInfo> fis = new List<FileInfo>();
            CheckCommonRoot(files, ref commonRoot);
            rootLength = commonRoot.Length + 1;
            foreach (string f in files)
            {
                string[] splittedAfn = f.Substring(rootLength).Split('\\');
                string cfn = commonRoot;
                for (int i = 0; i < splittedAfn.Length; i++)
                {
                    cfn += '\\' + splittedAfn[i];
                    if (!ListHasFileInfo(fis, cfn))
                    {
                        fis.Add(new FileInfo(cfn));
                    }
                }

            }
            return fis.ToArray();
        }
        /// <summary>
        /// Recursive function for adding files in directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="files">List of files</param>
        /// <param name="searchPattern">Search string, such as "*.txt"</param>
        private void AddFilesFromDirectory(string directory, List<string> files, string searchPattern)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            foreach (FileInfo fi in di.GetFiles(searchPattern))
            {
                files.Add(fi.FullName);
            }
            foreach (DirectoryInfo cdi in di.GetDirectories())
            {
                AddFilesFromDirectory(cdi.FullName, files, searchPattern);
            }
        }
        /// <summary>
        /// Produces  a new instance of ArchiveUpdateCallback class
        /// </summary>
        /// <param name="files">Array of FileInfo - files to pack</param>
        /// <param name="rootLength">Length of the common root of file names</param>
        /// <param name="password">The archive password</param>
        /// <returns></returns>
        private ArchiveUpdateCallback GetArchiveUpdateCallback(FileInfo[] files, int rootLength, string password)
        {
            ArchiveUpdateCallback auc = (String.IsNullOrEmpty(password)) ? new ArchiveUpdateCallback(files, rootLength) :
                new ArchiveUpdateCallback(files, rootLength, password);
            auc.FileCompressionStarted += FileCompressionStarted;
            auc.Compressing += Compressing;
            return auc;
        }

        #region ISevenZipCompressor Members
        /// <summary>
        /// Occurs when the next file is going to be packed
        /// </summary>
        /// <remarks>Occurs when 7-zip engine requests for an input stream for the next file to pack it</remarks>
        public event EventHandler<FileInfoEventArgs> FileCompressionStarted;
        /// <summary>
        /// Occurs when data are being compressed
        /// </summary>
        /// <remarks>Use this event for accurate progress handling and various ProgressBar.StepBy(e.PercentDelta) routines</remarks>
        public event EventHandler<ProgressEventArgs> Compressing;

        #region CompressFiles function overloads

        /// <summary>
        /// Packs files into the archive
        /// </summary>
        /// <param name="fileFullNames">Array of file names to pack</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>
        public void CompressFiles(
            string[] fileFullNames, string archiveName, OutArchiveFormat format)
        {
            CompressFiles(fileFullNames, CommonRoot(fileFullNames), archiveName, format, "");
        }

        /// <summary>
        /// Packs files into the archive
        /// </summary>
        /// <param name="fileFullNames">Array of file names to pack</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>
        public void CompressFiles(
            string[] fileFullNames, Stream archiveStream, OutArchiveFormat format)
        {
            CompressFiles(fileFullNames, CommonRoot(fileFullNames), archiveStream, format, "");
        }

        /// <summary>
        /// Packs files into the archive
        /// </summary>
        /// <param name="fileFullNames">Array of file names to pack</param>
        /// <param name="commonRoot">Common root of the file names</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>
        public void CompressFiles(
            string[] fileFullNames, string commonRoot, string archiveName, OutArchiveFormat format)
        {
            CompressFiles(fileFullNames, commonRoot, archiveName, format, "");
        }

        /// <summary>
        /// Packs files into the archive
        /// </summary>
        /// <param name="fileFullNames">Array of file names to pack</param>
        /// <param name="commonRoot">Common root of the file names</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>
        public void CompressFiles(
            string[] fileFullNames, string commonRoot, Stream archiveStream, OutArchiveFormat format)
        {
            CompressFiles(fileFullNames, commonRoot, archiveStream, format, "");
        }

        /// <summary>
        /// Packs files into the archive
        /// </summary>
        /// <param name="fileFullNames">Array of file names to pack</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>
        /// <param name="password">The archive password</param>
        public void CompressFiles(
            string[] fileFullNames, string archiveName, OutArchiveFormat format, string password)
        {
            CompressFiles(fileFullNames, CommonRoot(fileFullNames), archiveName, format, password);
        }

        /// <summary>
        /// Packs files into the archive
        /// </summary>
        /// <param name="fileFullNames">Array of file names to pack</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>
        /// <param name="password">The archive password</param>
        public void CompressFiles(
            string[] fileFullNames, Stream archiveStream, OutArchiveFormat format, string password)
        {
            CompressFiles(fileFullNames, CommonRoot(fileFullNames), archiveStream, format, password);
        }

        /// <summary>
        /// Packs files into the archive
        /// </summary>
        /// <param name="fileFullNames">Array of file names to pack</param>
        /// <param name="commonRoot">Common root of the file names</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>
        /// <param name="password">The archive password</param>
        public void CompressFiles(
            string[] fileFullNames, string commonRoot, string archiveName,
            OutArchiveFormat format, string password)
        {
            using (FileStream fs = File.Create(archiveName))
            {
                CompressFiles(fileFullNames, commonRoot, fs, format, password);
            }
        }

        /// <summary>
        /// Packs files into the archive
        /// </summary>
        /// <param name="fileFullNames">Array of file names to pack</param>
        /// <param name="commonRoot">Common root of the file names</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>
        /// <param name="password">The archive password</param>
        public void CompressFiles(
            string[] fileFullNames, string commonRoot, Stream archiveStream,
            OutArchiveFormat format, string password)
        {
            int rootLength;
            FileInfo[] files = ProduceFileInfoArray(fileFullNames, commonRoot, out rootLength);
            try
            {
                SevenZipLibraryManager.LoadLibrary(this, format);
                using (OutStreamWrapper ArchiveStream = new OutStreamWrapper(archiveStream, false))
                {
                    using (ArchiveUpdateCallback auc = GetArchiveUpdateCallback(files, rootLength, password))
                    {
                        CheckedExecute(
                            SevenZipLibraryManager.OutArchive(format).UpdateItems(
                            ArchiveStream, (uint)files.Length, auc),
                            SevenZipCompressionFailedException.DefaultMessage);
                    }
                }
            }
            finally
            {
                SevenZipLibraryManager.FreeLibrary(this, format);
            }
        }
        #endregion

        #region CompressDirectory function overloads

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>
        public void CompressDirectory(
            string directory, string archiveName, OutArchiveFormat format)
        {
            CompressDirectory(directory, archiveName, format, "", "*.*", true);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>
        public void CompressDirectory(
            string directory, Stream archiveStream, OutArchiveFormat format)
        {
            CompressDirectory(directory, archiveStream, format, "", "*.*", true);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>
        /// <param name="password">The archive password</param>
        public void CompressDirectory(
            string directory, string archiveName, OutArchiveFormat format, string password)
        {
            CompressDirectory(directory, archiveName, format, password, "*.*", true);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>
        /// <param name="password">The archive password</param>
        public void CompressDirectory(
            string directory, Stream archiveStream, OutArchiveFormat format, string password)
        {
            CompressDirectory(directory, archiveStream, format, password, "*.*", true);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>
        /// <param name="recursion">Search for files recursively</param>
        public void CompressDirectory(
            string directory, string archiveName, OutArchiveFormat format, bool recursion)
        {
            CompressDirectory(directory, archiveName, format, "", "*.*", recursion);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>
        /// <param name="recursion">Search for files recursively</param>
        public void CompressDirectory(
            string directory, Stream archiveStream, OutArchiveFormat format, bool recursion)
        {
            CompressDirectory(directory, archiveStream, format, "", "*.*", recursion);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>
        /// <param name="searchPattern">Search string, such as "*.txt"</param>
        /// <param name="recursion">Search for files recursively</param>
        public void CompressDirectory(
            string directory, string archiveName, OutArchiveFormat format,
            string searchPattern, bool recursion)
        {
            CompressDirectory(directory, archiveName, format, "", searchPattern, recursion);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>
        /// <param name="searchPattern">Search string, such as "*.txt"</param>
        /// <param name="recursion">Search for files recursively</param>
        public void CompressDirectory(
            string directory, Stream archiveStream, OutArchiveFormat format,
            string searchPattern, bool recursion)
        {
            CompressDirectory(directory, archiveStream, format, "", searchPattern, recursion);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>        
        /// <param name="recursion">Search for files recursively</param>
        /// <param name="password">The archive password</param>
        public void CompressDirectory(
            string directory, string archiveName, OutArchiveFormat format,
            bool recursion, string password)
        {
            CompressDirectory(directory, archiveName, format, password, "*.*", recursion);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>        
        /// <param name="recursion">Search for files recursively</param>
        /// <param name="password">The archive password</param>
        public void CompressDirectory(
            string directory, Stream archiveStream, OutArchiveFormat format,
            bool recursion, string password)
        {
            CompressDirectory(directory, archiveStream, format, password, "*.*", recursion);
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveName">The archive file name</param>
        /// <param name="format">The archive format</param>
        /// <param name="password">The archive password</param>
        /// <param name="searchPattern">Search string, such as "*.txt"</param>
        /// <param name="recursion">Search for files recursively</param>
        public void CompressDirectory(
            string directory, string archiveName, OutArchiveFormat format,
            string password, string searchPattern, bool recursion)
        {
            using (FileStream fs = File.Create(archiveName))
            {
                CompressDirectory(directory, fs, format, password, searchPattern, recursion);
            }
        }

        /// <summary>
        /// Packs files in the directory
        /// </summary>
        /// <param name="directory">Directory directory</param>
        /// <param name="archiveStream">The archive output stream</param>
        /// <param name="format">The archive format</param>
        /// <param name="password">The archive password</param>
        /// <param name="searchPattern">Search string, such as "*.txt"</param>
        /// <param name="recursion">Search for files recursively</param>
        public void CompressDirectory(
            string directory, Stream archiveStream, OutArchiveFormat format,
            string password, string searchPattern, bool recursion)
        {
            List<string> files = new List<string>();
            if (!Directory.Exists(directory))
            {
                throw new ArgumentException("Directory \"" + directory + "\" does not exist!");
            }
            else
            {
                if (RecursiveDirectoryEmptyCheck(directory))
                {
                    throw new SevenZipInvalidFileNamesException("specified directory is empty!");
                }
                if (recursion)
                {
                    AddFilesFromDirectory(directory, files, searchPattern);
                }
                else
                {
                    foreach (FileInfo fi in (new DirectoryInfo(directory)).GetFiles(searchPattern))
                    {
                        files.Add(fi.FullName);
                    }
                }
                CompressFiles(files.ToArray(), directory, archiveStream, format, password);
            }
        }
        #endregion

        #endregion

        private static void WriteLzmaProperties(Encoder encoder)
        {
            #region LZMA properties definition
            CoderPropID[] propIDs = 
			{
				CoderPropID.DictionarySize,
				CoderPropID.PosStateBits,
				CoderPropID.LitContextBits,
				CoderPropID.LitPosBits,
				CoderPropID.Algorithm,
				CoderPropID.NumFastBytes,
				CoderPropID.MatchFinder,
				CoderPropID.EndMarker
			};
            object[] properties = 
			{
				1 << 22,
				2,
				3,
				0,
				2,
				256,
				"bt4",
				false
			};
            #endregion
            encoder.SetCoderProperties(propIDs, properties);
        }

        /// <summary>
        /// Compress the specified stream with LZMA algorithm (C# inside)
        /// </summary>
        /// <param name="inStream">The source uncompressed stream</param>
        /// <param name="outStream">The destination compressed stream</param>
        /// <param name="inLength">The length of uncompressed data (null for inStream.Length)</param>
        /// <param name="codeProgressEvent">The event for handling the code progress</param>
        public static void CompressStream(Stream inStream, Stream outStream, int? inLength, EventHandler<ProgressEventArgs> codeProgressEvent)
        {
            Encoder encoder = new Encoder();
            WriteLzmaProperties(encoder);
            encoder.WriteCoderProperties(outStream);
            long streamSize = inLength.HasValue? inLength.Value : inStream.Length;
            for (int i = 0; i < 8; i++)
                outStream.WriteByte((byte)(streamSize >> (8 * i)));
            encoder.Code(inStream, outStream, -1, -1, new LzmaProgressCallback(streamSize, codeProgressEvent));
        }

        /// <summary>
        /// Compress byte array with LZMA algorithm (C# inside)
        /// </summary>
        /// <param name="data">Byte array to compress</param>
        /// <returns>Compressed byte array</returns>
        public static byte[] CompressBytes(byte[] data)
        {
            
            using (MemoryStream inStream = new MemoryStream(data))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    Encoder encoder = new Encoder();
                    WriteLzmaProperties(encoder);
                    encoder.WriteCoderProperties(outStream);
                    long streamSize = inStream.Length;
                    for (int i = 0; i < 8; i++)
                        outStream.WriteByte((byte)(streamSize >> (8 * i)));
                    encoder.Code(inStream, outStream, -1, -1, null);
                    return outStream.ToArray();
                }
            }
        }
    }
}
