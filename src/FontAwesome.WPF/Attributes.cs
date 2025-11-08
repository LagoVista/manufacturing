// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 96c264c1a89dec1de4d185d964725c05270435c415f42ff6f9965d1beddc13f8
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;

namespace FontAwesome.WPF
{
    /// <summary>
    /// Represents the category of a fontawesome icon.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class IconCategoryAttribute
        : Attribute
    {
        /// <summary>
        /// Gets or sets the category of the icon.
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Initializes a new instance of the FontAwesome.WPF.IconCategoryAttribute class.
        /// </summary>
        /// <param name="category">The icon category.</param>
        public IconCategoryAttribute(string category)
        {
            Category = category;
        }
    }
    /// <summary>
    /// Represents the field is an alias of another icon.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class IconAliasAttribute
        : Attribute
    { }

    /// <summary>
    /// Represents the id (css class name) of the icon.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class IconIdAttribute
        : Attribute
    {
        /// <summary>
        /// Gets or sets the id (css class name) of the icon.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the FontAwesome.WPF.IconIdAttribute class.
        /// </summary>
        /// <param name="id">The icon id (css class name).</param>
        public IconIdAttribute(string id)
        {
            Id = id;
        }
    }
}
