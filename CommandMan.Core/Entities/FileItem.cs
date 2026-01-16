// <copyright file="FileItem.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Core.Entities
{
    /// <summary>
    /// Represents a file in the file system.
    /// </summary>
    public class FileItem : FileSystemItem
    {
        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        public override long Size => this.FileSize;

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        public override string Extension => System.IO.Path.GetExtension(this.Name);
    }
}
