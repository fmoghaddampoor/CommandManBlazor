// <copyright file="DirectoryItem.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Core.Entities
{
    /// <summary>
    /// Represents a directory in the file system.
    /// </summary>
    public class DirectoryItem : FileSystemItem
    {
        /// <summary>
        /// Gets the size of the directory (always returns 0).
        /// </summary>
        public override long Size => 0;

        /// <summary>
        /// Gets the directory indicator string.
        /// </summary>
        public override string Extension => "<DIR>";
    }
}
