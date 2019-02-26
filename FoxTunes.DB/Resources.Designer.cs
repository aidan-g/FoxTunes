﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FoxTunes {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("FoxTunes.DB.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO &quot;PlaylistItems&quot; (&quot;LibraryItem_Id&quot;, &quot;Sequence&quot;, &quot;DirectoryName&quot;, &quot;FileName&quot;, &quot;Status&quot;) 
        ///SELECT &quot;LibraryItems&quot;.&quot;Id&quot;, @sequence, &quot;LibraryItems&quot;.&quot;DirectoryName&quot;, &quot;LibraryItems&quot;.&quot;FileName&quot;, @status
        ///FROM &quot;LibraryHierarchyItems&quot;
        ///	JOIN &quot;LibraryHierarchyItem_LibraryItem&quot; 
        ///		ON &quot;LibraryHierarchyItems&quot;.&quot;Id&quot; = &quot;LibraryHierarchyItem_LibraryItem&quot;.&quot;LibraryHierarchyItem_Id&quot;
        ///	JOIN &quot;LibraryItems&quot;
        ///		ON &quot;LibraryItems&quot;.&quot;Id&quot; = &quot;LibraryHierarchyItem_LibraryItem&quot;.&quot;LibraryItem_Id&quot;
        ///WHERE &quot;LibraryHierarchyItems&quot;.&quot;I [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AddLibraryHierarchyNodeToPlaylist {
            get {
                return ResourceManager.GetString("AddLibraryHierarchyNodeToPlaylist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to WITH 
        ///&quot;MetaDataItems_Lookup&quot; AS
        ///(
        ///	SELECT *
        ///	FROM &quot;MetaDataItems&quot; 
        ///	WHERE &quot;Name&quot; = @name AND &quot;Type&quot; = @type AND &quot;Value&quot; = @value
        ///)
        ///
        ///INSERT INTO &quot;MetaDataItems&quot; (&quot;Name&quot;, &quot;Type&quot;, &quot;Value&quot;) 
        ///SELECT @name, @type, @value
        ///WHERE NOT EXISTS(SELECT * FROM &quot;MetaDataItems_Lookup&quot;);
        ///
        ///WITH 
        ///&quot;MetaDataItems_Lookup&quot; AS
        ///(
        ///	SELECT *
        ///	FROM &quot;MetaDataItems&quot; 
        ///	WHERE &quot;Name&quot; = @name AND &quot;Type&quot; = @type AND &quot;Value&quot; = @value
        ///),
        ///
        ///&quot;LibraryItem_MetaDataItem_Lookup&quot; AS 
        ///(
        ///	SELECT &quot;LibraryItem_MetaDataItem&quot;.*
        ///	FROM &quot; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AddLibraryMetaDataItems {
            get {
                return ResourceManager.GetString("AddLibraryMetaDataItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to WITH 
        ///&quot;MetaDataItems_Lookup&quot; AS
        ///(
        ///	SELECT *
        ///	FROM &quot;MetaDataItems&quot; 
        ///	WHERE &quot;Name&quot; = @name AND &quot;Type&quot; = @type AND &quot;Value&quot; = @value
        ///)
        ///
        ///INSERT INTO &quot;MetaDataItems&quot; (&quot;Name&quot;, &quot;Type&quot;, &quot;Value&quot;) 
        ///SELECT @name, @type, @value
        ///WHERE NOT EXISTS(SELECT * FROM &quot;MetaDataItems_Lookup&quot;);
        ///
        ///WITH 
        ///&quot;MetaDataItems_Lookup&quot; AS
        ///(
        ///	SELECT *
        ///	FROM &quot;MetaDataItems&quot; 
        ///	WHERE &quot;Name&quot; = @name AND &quot;Type&quot; = @type AND &quot;Value&quot; = @value
        ///),
        ///
        ///&quot;PlaylistItem_MetaDataItem_Lookup&quot; AS 
        ///(
        ///	SELECT &quot;PlaylistItem_MetaDataItem&quot;.*
        ///	FROM [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AddPlaylistMetaDataItems {
            get {
                return ResourceManager.GetString("AddPlaylistMetaDataItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO &quot;PlaylistSort&quot; (&quot;PlaylistItem_Id&quot;, &quot;Value1&quot;, &quot;Value2&quot;, &quot;Value3&quot;, &quot;Value4&quot;, &quot;Value5&quot;, &quot;Value6&quot;, &quot;Value7&quot;, &quot;Value8&quot;, &quot;Value9&quot;, &quot;Value10&quot;)
        ///VALUES (@playlistItemId, @value1, @value2, @value3, @value4, @value5, @value6, @value7, @value8, @value9, @value10);.
        /// </summary>
        internal static string AddPlaylistSequenceRecord {
            get {
                return ResourceManager.GetString("AddPlaylistSequenceRecord", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE FROM &quot;LibraryItem_MetaDataItem&quot;
        ///WHERE &quot;LibraryItem_Id&quot; = @itemId.
        /// </summary>
        internal static string ClearLibraryMetaDataItems {
            get {
                return ResourceManager.GetString("ClearLibraryMetaDataItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE FROM &quot;PlaylistItem_MetaDataItem&quot;
        ///WHERE &quot;PlaylistItem_Id&quot; = @itemId.
        /// </summary>
        internal static string ClearPlaylistMetaDataItems {
            get {
                return ResourceManager.GetString("ClearPlaylistMetaDataItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO &quot;LibraryHierarchyItems&quot; (&quot;LibraryHierarchy_Id&quot;, &quot;LibraryHierarchyLevel_Id&quot;, &quot;Value&quot;, &quot;IsLeaf&quot;)
        ///SELECT &quot;LibraryHierarchy_Id&quot;, &quot;LibraryHierarchyLevel_Id&quot;, &quot;Value&quot;, &quot;IsLeaf&quot;
        ///FROM &quot;LibraryHierarchy&quot;
        ///GROUP BY &quot;LibraryHierarchy_Id&quot;, &quot;LibraryHierarchyLevel_Id&quot;, &quot;Value&quot;, &quot;IsLeaf&quot;;
        ///
        ///
        ///INSERT INTO &quot;LibraryHierarchyItem_Parent&quot; (&quot;LibraryHierarchyItem_Id&quot;, &quot;LibraryHierarchyItem_Parent_Id&quot;)
        ///SELECT &quot;LibraryHierarchyItems&quot;.&quot;Id&quot;, &quot;LibraryHierarchyItems_Parent&quot;.&quot;Id&quot;
        ///FROM &quot;LibraryHierarchyItems&quot; 
        ///	JOIN &quot;L [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string EndBuildLibraryHierarchies {
            get {
                return ResourceManager.GetString("EndBuildLibraryHierarchies", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT &quot;LibraryHierarchyItems&quot;.&quot;Id&quot;, &quot;LibraryHierarchy_Id&quot;, &quot;Value&quot;, &quot;IsLeaf&quot;
        ///FROM &quot;LibraryHierarchyItems&quot;
        ///	LEFT JOIN &quot;LibraryHierarchyItem_Parent&quot;
        ///		ON &quot;LibraryHierarchyItem_Parent&quot;.&quot;LibraryHierarchyItem_Id&quot; = &quot;LibraryHierarchyItems&quot;.&quot;Id&quot;
        ///WHERE &quot;LibraryHierarchy_Id&quot; = @libraryHierarchyId
        ///	AND ((@libraryHierarchyItemId IS NULL AND &quot;LibraryHierarchyItem_Parent&quot;.&quot;LibraryHierarchyItem_Parent_Id&quot; IS NULL) OR &quot;LibraryHierarchyItem_Parent&quot;.&quot;LibraryHierarchyItem_Parent_Id&quot; = @libraryHierarchyItemId)
        ///ORDER BY [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetLibraryHierarchyNodes {
            get {
                return ResourceManager.GetString("GetLibraryHierarchyNodes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to WITH &quot;CurrentSequence&quot;
        ///AS
        ///(	
        ///		SELECT &quot;Sequence&quot; 
        ///		FROM &quot;PlaylistItems&quot;
        ///		WHERE &quot;Id&quot; = @id
        ///)
        ///
        ///UPDATE &quot;PlaylistItems&quot;
        ///SET &quot;Sequence&quot; = 
        ///(
        ///	CASE WHEN (SELECT &quot;Sequence&quot; FROM &quot;CurrentSequence&quot;) &lt; @sequence THEN
        ///		&quot;Sequence&quot; - 1
        ///	ELSE
        ///		&quot;Sequence&quot; + 1
        ///	END
        ///)
        ///WHERE 
        ///(
        ///	(SELECT &quot;Sequence&quot; FROM &quot;CurrentSequence&quot;) &lt; @sequence 
        ///		AND &quot;Sequence&quot; BETWEEN (SELECT &quot;Sequence&quot; FROM &quot;CurrentSequence&quot;) 
        ///			AND @sequence
        ///) 
        ///OR 
        ///(
        ///	&quot;Sequence&quot; BETWEEN @sequence 
        ///		AND (SELECT &quot;Sequence&quot; FROM &quot;Current [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MovePlaylistItem {
            get {
                return ResourceManager.GetString("MovePlaylistItem", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE FROM [LibraryHierarchy_LibraryHierarchyItem];
        ///
        ///DELETE FROM [LibraryHierarchyItem_LibraryItem];
        ///
        ///DELETE FROM [LibraryHierarchyItem_Parent];
        ///
        ///DELETE FROM [LibraryHierarchyItems];.
        /// </summary>
        internal static string RemoveLibraryHierarchyItems {
            get {
                return ResourceManager.GetString("RemoveLibraryHierarchyItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE FROM [LibraryItem_MetaDataItem]
        ///WHERE [LibraryItem_Id] IN
        ///(
        ///	SELECT [Id]
        ///	FROM [LibraryItems]
        ///	WHERE [LibraryItems].[Status] = @status
        ///);
        ///
        ///DELETE FROM [PlaylistItem_MetaDataItem]
        ///WHERE [PlaylistItem_Id] IN
        ///(
        ///	SELECT [PlaylistItems].[Id]
        ///	FROM [PlaylistItems]
        ///		JOIN [LibraryItems] ON [PlaylistItems].[LibraryItem_Id] = [LibraryItems].[Id]
        ///	WHERE [LibraryItems].[Status] = @status
        ///);
        ///
        ///DELETE FROM [PlaylistItems]
        ///WHERE [Id] IN
        ///(
        ///	SELECT [PlaylistItems].[Id]
        ///	FROM [PlaylistItems]
        ///		JO [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string RemoveLibraryItems {
            get {
                return ResourceManager.GetString("RemoveLibraryItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE FROM [PlaylistItem_MetaDataItem]
        ///WHERE [PlaylistItem_Id] IN
        ///(
        ///	SELECT [Id]
        ///	FROM [PlaylistItems]
        ///	WHERE [Status] = @status
        ///);
        ///
        ///DELETE FROM [PlaylistItems]
        ///WHERE [Status] = @status.
        /// </summary>
        internal static string RemovePlaylistItems {
            get {
                return ResourceManager.GetString("RemovePlaylistItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO &quot;MetaDataItems&quot; (&quot;Name&quot;, &quot;Type&quot;, &quot;Value&quot;)
        ///SELECT @name, @type, @value
        ///WHERE NOT EXISTS(SELECT * FROM &quot;MetaDataItems&quot; WHERE &quot;Name&quot; = @name AND &quot;Type&quot; = @type AND &quot;Value&quot; = @value);
        ///
        ///WITH &quot;MetaData&quot;
        ///AS
        ///(
        ///	SELECT 
        ///		&quot;LibraryItem_MetaDataItem&quot;.&quot;LibraryItem_Id&quot; AS &quot;Id&quot;,
        ///		&quot;MetaDataItems&quot;.&quot;Name&quot;,
        ///		&quot;MetaDataItems&quot;.&quot;Value&quot;
        ///	FROM &quot;LibraryItem_MetaDataItem&quot; 
        ///		JOIN &quot;MetaDataItems&quot; 
        ///			ON &quot;MetaDataItems&quot;.&quot;Id&quot; = &quot;LibraryItem_MetaDataItem&quot;.&quot;MetaDataItem_Id&quot;
        ///),
        ///
        ///&quot;Artist&quot;
        ///AS
        ///(
        ///	SELECT &quot;Arti [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string UpdateLibraryVariousArtists {
            get {
                return ResourceManager.GetString("UpdateLibraryVariousArtists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO &quot;MetaDataItems&quot; (&quot;Name&quot;, &quot;Type&quot;, &quot;Value&quot;)
        ///SELECT @name, @type, @value
        ///WHERE NOT EXISTS(SELECT * FROM &quot;MetaDataItems&quot; WHERE &quot;Name&quot; = @name AND &quot;Type&quot; = @type AND &quot;Value&quot; = @value);
        ///
        ///WITH &quot;MetaData&quot;
        ///AS
        ///(
        ///	SELECT 
        ///		&quot;PlaylistItem_MetaDataItem&quot;.&quot;PlaylistItem_Id&quot; AS &quot;Id&quot;,
        ///		&quot;MetaDataItems&quot;.&quot;Name&quot;,
        ///		&quot;MetaDataItems&quot;.&quot;Value&quot;
        ///	FROM &quot;PlaylistItem_MetaDataItem&quot; 
        ///		JOIN &quot;MetaDataItems&quot; 
        ///			ON &quot;MetaDataItems&quot;.&quot;Id&quot; = &quot;PlaylistItem_MetaDataItem&quot;.&quot;MetaDataItem_Id&quot;
        ///),
        ///
        ///&quot;Artist&quot;
        ///AS
        ///(
        ///	SELECT &quot; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string UpdatePlaylistVariousArtists {
            get {
                return ResourceManager.GetString("UpdatePlaylistVariousArtists", resourceCulture);
            }
        }
    }
}
