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
using System.Runtime.Serialization;

namespace SevenZip
{
    /// <summary>
    /// Base SevenZip exception class
    /// </summary>
    [Serializable]
    public class SevenZipException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the SevenZipException class
        /// </summary>
        public SevenZipException() : base("SevenZip unknown exception.") { }
        /// <summary>
        /// Initializes a new instance of the SevenZipException class
        /// </summary>
        /// <param name="defaultMessage">Default exception message</param>
        public SevenZipException(string defaultMessage)
            : base(defaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipException class
        /// </summary>
        /// <param name="defaultMessage">Default exception message</param>
        /// <param name="message">Additional detailed message</param>
        public SevenZipException(string defaultMessage, string message)
            : base(defaultMessage + " Message: " + message) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipException class
        /// </summary>
        /// <param name="defaultMessage">Default exception message</param>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public SevenZipException(string defaultMessage, string message, Exception inner)
            : base(defaultMessage + (defaultMessage.EndsWith(" ", StringComparison.CurrentCulture) ? "" : " Message: ") + message, inner) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipException class
        /// </summary>
        /// <param name="defaultMessage">Default exception message</param>
        /// <param name="inner">Inner exception occured</param>
        public SevenZipException(string defaultMessage, Exception inner)
            : base(defaultMessage, inner) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected SevenZipException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Exception class for ArchiveExtractCallback
    /// </summary>
    [Serializable]
    public class ExtractionFailedException : SevenZipException
    {
        /// <summary>
        /// Exception dafault message which is displayed if no extra information is specified
        /// </summary>
        public const string DefaultMessage = "Could not extract files!";
        /// <summary>
        /// Initializes a new instance of the ExtractionFailedException class
        /// </summary>
        public ExtractionFailedException() : base(DefaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the ExtractionFailedException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public ExtractionFailedException(string message) : base(DefaultMessage, message) { }
        /// <summary>
        /// Initializes a new instance of the ExtractionFailedException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public ExtractionFailedException(string message, Exception inner) : base(DefaultMessage, message, inner) { }
        /// <summary>
        /// Initializes a new instance of the ExtractionFailedException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected ExtractionFailedException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
    /// <summary>
    /// Exception class for ArchiveUpdateCallback
    /// </summary>
    [Serializable]
    public class CompressionFailedException : SevenZipException
    {
        /// <summary>
        /// Exception dafault message which is displayed if no extra information is specified
        /// </summary>
        public const string DefaultMessage = "Could not pack files!";
        /// <summary>
        /// Initializes a new instance of the CompressionFailedException class
        /// </summary>
        public CompressionFailedException() : base(DefaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the CompressionFailedException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public CompressionFailedException(string message) : base(DefaultMessage, message) { }
        /// <summary>
        /// Initializes a new instance of the CompressionFailedException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public CompressionFailedException(string message, Exception inner) : base(DefaultMessage, message, inner) { }
        /// <summary>
        /// Initializes a new instance of the CompressionFailedException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected CompressionFailedException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
    
    /// <summary>
    /// Exception class for LZMA operations
    /// </summary>
    [Serializable]
    public class LzmaException : SevenZipException
    {
        /// <summary>
        /// Exception dafault message which is displayed if no extra information is specified
        /// </summary>
        public const string DefaultMessage = "Specified stream is not a valid LZMA compressed stream!";
        /// <summary>
        /// Initializes a new instance of the LzmaException class
        /// </summary>
        public LzmaException() : base(DefaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the LzmaException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public LzmaException(string message) : base(DefaultMessage, message) { }
        /// <summary>
        /// Initializes a new instance of the LzmaException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public LzmaException(string message, Exception inner) : base(DefaultMessage, message, inner) { }
        /// <summary>
        /// Initializes a new instance of the LzmaException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected LzmaException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
    /// <summary>
    /// Exception class for 7-zip archive open or read operations
    /// </summary>
    [Serializable]
    public class SevenZipArchiveException : SevenZipException
    {
        /// <summary>
        /// Exception dafault message which is displayed if no extra information is specified
        /// </summary>
        public const string DefaultMessage = "Invalid archive: open/read error! Is it encrypted and a wrong password was provided?";
        /// <summary>
        /// Initializes a new instance of the SevenZipArchiveException class
        /// </summary>
        public SevenZipArchiveException() : base(DefaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipArchiveException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public SevenZipArchiveException(string message) : base(DefaultMessage, message) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipArchiveException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public SevenZipArchiveException(string message, Exception inner) : base(DefaultMessage, message, inner) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipArchiveException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected SevenZipArchiveException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
    /// <summary>
    /// Exception class for empty common root if file name array in SevenZipCompressor
    /// </summary>
    [Serializable]
    public class SevenZipInvalidFileNamesException : SevenZipException
    {
        /// <summary>
        /// Exception dafault message which is displayed if no extra information is specified
        /// </summary>
        public const string DefaultMessage = "Invalid file names have been specified: ";
        /// <summary>
        /// Initializes a new instance of the SevenZipInvalidFileNamesException class
        /// </summary>
        public SevenZipInvalidFileNamesException() : base(DefaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipInvalidFileNamesException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public SevenZipInvalidFileNamesException(string message) : base(DefaultMessage, message) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipInvalidFileNamesException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public SevenZipInvalidFileNamesException(string message, Exception inner) : base(DefaultMessage, message, inner) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipInvalidFileNamesException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected SevenZipInvalidFileNamesException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
    /// <summary>
    /// Exception class for fail to create an archive in SevenZipCompressor
    /// </summary>
    [Serializable]
    public class SevenZipCompressionFailedException : SevenZipException
    {
        /// <summary>
        /// Exception dafault message which is displayed if no extra information is specified
        /// </summary>
        public const string DefaultMessage = "The compression has failed for an unknown reason with code ";
        /// <summary>
        /// Initializes a new instance of the SevenZipCompressionFailedException class
        /// </summary>
        public SevenZipCompressionFailedException() : base(DefaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipCompressionFailedException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public SevenZipCompressionFailedException(string message) : base(DefaultMessage, message) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipCompressionFailedException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public SevenZipCompressionFailedException(string message, Exception inner) : base(DefaultMessage, message, inner) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipCompressionFailedException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected SevenZipCompressionFailedException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
    /// <summary>
    /// Exception class for fail to extract an archive in SevenZipExtractor
    /// </summary>
    [Serializable]
    public class SevenZipExtractionFailedException : SevenZipException
    {
        /// <summary>
        /// Exception dafault message which is displayed if no extra information is specified
        /// </summary>
        public const string DefaultMessage = "The extraction has failed for an unknown reason with code ";
        /// <summary>
        /// Initializes a new instance of the SevenZipExtractionFailedException class
        /// </summary>
        public SevenZipExtractionFailedException() : base(DefaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipExtractionFailedException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public SevenZipExtractionFailedException(string message) : base(DefaultMessage, message) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipExtractionFailedException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public SevenZipExtractionFailedException(string message, Exception inner) : base(DefaultMessage, message, inner) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipExtractionFailedException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected SevenZipExtractionFailedException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
    /// <summary>
    /// Exception class for 7-zip library operations
    /// </summary>
    [Serializable]
    public class SevenZipLibraryException : SevenZipException
    {
        /// <summary>
        /// Exception dafault message which is displayed if no extra information is specified
        /// </summary>
        public static readonly string DefaultMessage = "Can not load 7-zip library or internal COM error!";
        /// <summary>
        /// Initializes a new instance of the SevenZipLibraryException class
        /// </summary>
        public SevenZipLibraryException() : base(DefaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipLibraryException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public SevenZipLibraryException(string message) : base(DefaultMessage, message) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipLibraryException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public SevenZipLibraryException(string message, Exception inner) : base(DefaultMessage, message, inner) { }
        /// <summary>
        /// Initializes a new instance of the SevenZipLibraryException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected SevenZipLibraryException(
            SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
