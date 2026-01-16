// <copyright file="FileSystemItem.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Core.Entities
{
    using System;

    /// <summary>
    /// Represents an abstract base class for items in the file system.
    /// </summary>
    public abstract class FileSystemItem
    {
        /// <summary>
        /// Gets or sets the name of the file system item.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full path of the file system item.
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the item was last modified.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the version information of the file.
        /// </summary>
        public string FileVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets the size of the item in bytes.
        /// </summary>
        public abstract long Size { get; }

        /// <summary>
        /// Gets the file extension or type indicator.
        /// </summary>
        public abstract string Extension { get; }
    }
}
