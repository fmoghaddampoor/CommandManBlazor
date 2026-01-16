// <copyright file="FavoriteItem.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Core.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a favorite item in the application, which can be a file or a folder.
    /// </summary>
    public class FavoriteItem
    {
        /// <summary>
        /// Gets or sets the name of the favorite item.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full path to the favorite item.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the item is a folder.
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Gets or sets the list of child favorite items, used for nested favorites.
        /// </summary>
        public List<FavoriteItem> Children { get; set; } = new ();
    }
}
