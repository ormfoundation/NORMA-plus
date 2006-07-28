#region Common Public License Copyright Notice
/**************************************************************************\
* Neumont Object-Role Modeling Architect for Visual Studio                 *
*                                                                          *
* Copyright � Neumont University. All rights reserved.                     *
*                                                                          *
* The use and distribution terms for this software are covered by the      *
* Common Public License 1.0 (http://opensource.org/licenses/cpl) which     *
* can be found in the file CPL.txt at the root of this distribution.       *
* By using this software in any fashion, you are agreeing to be bound by   *
* the terms of this license.                                               *
*                                                                          *
* You must not remove this notice, or any other, from this software.       *
\**************************************************************************/
#endregion

// It is much easier to write custom serialization xml if the
// serializer is allowed to spit the default names for the links.
// However, if there is no custom information on a link, we always
// block cross-model link serialization, so any extension models
// with links back into the core model are not written by default, making
// it more difficult to custom-serialize extensions than it needs to be.
// To temporary disable this, uncomment the following line.
//#define WRITE_ALL_DEFAULT_LINKS
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.Xml.XPath;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Design;
using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Shell;
using Neumont.Tools.ORM.Framework;
using Neumont.Tools.ORM.ObjectModel;
using Neumont.Tools.ORM.ShapeModel;

namespace Neumont.Tools.ORM.Shell
{
	#region Public Enumerations
	/// <summary>
	/// Supported operations for element custom serialization.
	/// </summary>
	[Flags]
	public enum ORMCustomSerializedElementSupportedOperations
	{
		/// <summary>
		/// No operations are supported.
		/// </summary>
		None = 0x00,
		/// <summary>
		/// Child element information is supported.
		/// </summary>
		ChildElementInfo = 0x01,
		/// <summary>
		/// Custom element information is supported.
		/// </summary>
		ElementInfo = 0x02,
		/// <summary>
		/// Custom property information is supported.
		/// </summary>
		PropertyInfo = 0x04,
		/// <summary>
		/// Custom link information is supported.
		/// </summary>
		LinkInfo = 0x08,
		/// <summary>
		/// The CustomSerializedChildRoleComparer method is supported
		/// </summary>
		CustomSortChildRoles = 0x10,
		/// <summary>
		/// Set if some of the properties are written as elements and others are written as properties.
		/// </summary>
		MixedTypedAttributes = 0x20,
		/// <summary>
		/// A child LinkInfo is actually the back link to the aggregating object. These
		/// elements have an id, but no ref.
		/// </summary>
		EmbeddingLinkInfo = 0x40,
	}
	/// <summary>
	/// Write style for element custom serialization.
	/// </summary>
	public enum ORMCustomSerializedElementWriteStyle
	{
		/// <summary>
		/// Dont write.
		/// </summary>
		NotWritten = 0xFF,
		/// <summary>
		/// Write as an element.
		/// </summary>
		Element = 0x00,
		/// <summary>
		/// Write as a double tagged element.
		/// </summary>
		DoubleTaggedElement = 0x01,
		/// <summary>
		/// Used for links. Write as an element, but write the link
		/// id, properties, and referencing child elements at this location.
		/// </summary>
		PrimaryLinkElement = 0x02,
		/// <summary>
		/// Used for embedding links. Write as a child element of the
		/// embedded object. Writes the link id. Any properties on the link
		/// and referencing child elements are written at this location.
		/// </summary>
		EmbeddingLinkElement = 0x03,
	}
	/// <summary>
	/// Write style for property custom serialization.
	/// </summary>
	public enum ORMCustomSerializedAttributeWriteStyle
	{
		/// <summary>
		/// Dont write.
		/// </summary>
		NotWritten = 0xFF,
		/// <summary>
		/// Write as an property.
		/// </summary>
		Attribute = 0x00,
		/// <summary>
		/// Write as an element.
		/// </summary>
		Element = 0x01,
		/// <summary>
		/// Write as a double tagged element.
		/// </summary>
		DoubleTaggedElement = 0x02
	}
	/// <summary>
	/// An enum used for deserialization to determine if
	/// an element name and namespace is recognized by a
	/// custom serialized element.
	/// </summary>
	public enum ORMCustomSerializedElementMatchStyle
	{
		/// <summary>
		/// The element is not recognized, don't process it
		/// </summary>
		None,
		/// <summary>
		/// The element matched an property written out as an element.
		/// The DoubleTageName property (if it is not null) specifies the
		/// double tag name (the tag inside this element where the property
		/// data is stored). The guid identifying the DomainPropertyInfo is
		/// returned in the Guid property.
		/// </summary>
		Property,
		/// <summary>
		/// The element matches a single contained role. The guid identifying
		/// the DomainPropertyInfo is returned in the SingleOppositeDomainRoleGuid property.
		/// </summary>
		SingleOppositeDomainRole,
		/// <summary>
		/// The element matches a single contained role and the link must
		/// be created as an explicit subtype of the relationship specified by the
		/// domain role. The guid identifying the DomainPropertyInfo is returned in the
		/// SingleOppositeDomainRoleGuid property and the guid identifying the DomainRelationshipInfo
		/// is returned by the ExplicitRelationshipGuid property.
		/// </summary>
		SingleOppositeDomainRoleExplicitRelationshipType,
		/// <summary>
		/// The element matches more than one contained role. The guids identifying
		/// the roles are returned in the OppositeDomainRoleGuidCollection property
		/// </summary>
		MultipleOppositeDomainRoles,
		/// <summary>
		/// The element matches more than one contained role and the link must
		/// be created as an explicit subtype of the relationship specified by the
		/// meta role. The guids identifying the roles are returned in the
		/// OppositeDomainRoleGuidCollection property. The guid identifying the
		/// DomainRelationshipInfo is returned by the ExplicitRelationshipGuid property.
		/// </summary>
		MultipleOppositeMetaRolesExplicitRelationshipType,
	}
	#endregion Public Enumerations
	#region Public Classes
	/// <summary>
	/// Custom serialization information.
	/// </summary>
	public abstract class ORMCustomSerializedInfo
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		protected ORMCustomSerializedInfo()
		{
		}
		/// <summary>
		/// Main Constructor
		/// </summary>
		/// <param name="customPrefix">The custom prefix to use.</param>
		/// <param name="customName">The custom name to use.</param>
		/// <param name="customNamespace">The custom namespace to use.</param>
		/// <param name="doubleTagName">The name of the double tag.</param>
		protected ORMCustomSerializedInfo(string customPrefix, string customName, string customNamespace, string doubleTagName)
		{
			myCustomPrefix = customPrefix;
			myCustomName = customName;
			myCustomNamespace = customNamespace;
			myDoubleTagName = doubleTagName;
		}

		private string myCustomPrefix;
		private string myCustomName;
		private string myCustomNamespace;
		private string myDoubleTagName;

		/// <summary>
		/// Return true if no values are set in the structure
		/// </summary>
		public virtual bool IsDefault
		{
			get
			{
				return myCustomPrefix == null && myCustomName == null && myCustomNamespace == null && myDoubleTagName == null;
			}
		}

		/// <summary>
		/// The custom prefix to use.
		/// </summary>
		/// <value>The custom prefix to use.</value>
		public string CustomPrefix
		{
			get { return myCustomPrefix; }
			set { myCustomPrefix = value; }
		}
		/// <summary>
		/// The custom name to use.
		/// </summary>
		/// <value>The custom name to use.</value>
		public string CustomName
		{
			get { return myCustomName; }
			set { myCustomName = value; }
		}
		/// <summary>
		/// The custom namespace to use.
		/// </summary>
		/// <value>The custom namespace to use.</value>
		public string CustomNamespace
		{
			get { return myCustomNamespace; }
			set { myCustomNamespace = value; }
		}
		/// <summary>
		/// The name of the double tag.
		/// </summary>
		/// <value>The name of the double tag.</value>
		public string DoubleTagName
		{
			get { return myDoubleTagName; }
			set { myDoubleTagName = value; }
		}
	}
	/// <summary>
	/// Custom serialization information for elements.
	/// </summary>
	public class ORMCustomSerializedElementInfo : ORMCustomSerializedInfo
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		protected ORMCustomSerializedElementInfo()
		{
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="writeStyle">The style to use when writing.</param>
		public ORMCustomSerializedElementInfo(ORMCustomSerializedElementWriteStyle writeStyle)
		{
			myWriteStyle = writeStyle;
		}
		/// <summary>
		/// Main Constructor
		/// </summary>
		/// <param name="customPrefix">The custom prefix to use.</param>
		/// <param name="customName">The custom name to use.</param>
		/// <param name="customNamespace">The custom namespace to use.</param>
		/// <param name="writeStyle">The style to use when writting.</param>
		/// <param name="doubleTagName">The name of the double tag.</param>
		public ORMCustomSerializedElementInfo(string customPrefix, string customName, string customNamespace, ORMCustomSerializedElementWriteStyle writeStyle, string doubleTagName)
			: base(customPrefix, customName, customNamespace, doubleTagName)
		{
			myWriteStyle = writeStyle;
		}

		private ORMCustomSerializedElementWriteStyle myWriteStyle;

		/// <summary>
		/// Default ORMCustomSerializedElementInfo
		/// </summary>
		public static readonly ORMCustomSerializedElementInfo Default = new ORMCustomSerializedElementInfo();

		/// <summary>
		/// Return true if no values are set in the structure
		/// </summary>
		public override bool IsDefault
		{
			get
			{
				return myWriteStyle == 0 && base.IsDefault;
			}
		}

		/// <summary>
		/// The style to use when writting.
		/// </summary>
		/// <value>The style to use when writting.</value>
		public ORMCustomSerializedElementWriteStyle WriteStyle
		{
			get { return myWriteStyle; }
			set { myWriteStyle = value; }
		}
	}
	/// <summary>
	/// Custom serialization information for properties.
	/// </summary>
	public class ORMCustomSerializedPropertyInfo : ORMCustomSerializedInfo
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		protected ORMCustomSerializedPropertyInfo()
		{
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="writeStyle">The style to use when writting.</param>
		public ORMCustomSerializedPropertyInfo(ORMCustomSerializedAttributeWriteStyle writeStyle)
		{
			myWriteStyle = writeStyle;
		}
		/// <summary>
		/// Main Constructor
		/// </summary>
		/// <param name="customPrefix">The custom prefix to use.</param>
		/// <param name="customName">The custom name to use.</param>
		/// <param name="customNamespace">The custom namespace to use.</param>
		/// <param name="writeCustomStorage">true to write when custom storage.</param>
		/// <param name="writeStyle">The style to use when writting.</param>
		/// <param name="doubleTagName">The name of the double tag.</param>
		public ORMCustomSerializedPropertyInfo(string customPrefix, string customName, string customNamespace, bool writeCustomStorage, ORMCustomSerializedAttributeWriteStyle writeStyle, string doubleTagName)
			: base(customPrefix, customName, customNamespace, doubleTagName)
		{
			myWriteCustomStorage = writeCustomStorage;
			myWriteStyle = writeStyle;
		}

		private bool myWriteCustomStorage;
		private ORMCustomSerializedAttributeWriteStyle myWriteStyle;

		/// <summary>
		/// Default ORMCustomSerializedPropertyInfo
		/// </summary>
		public static readonly ORMCustomSerializedPropertyInfo Default = new ORMCustomSerializedPropertyInfo();

		/// <summary>
		/// true to write when custom storage.
		/// </summary>
		/// <value>true to write when custom storage.</value>
		public bool WriteCustomStorage
		{
			get { return myWriteCustomStorage; }
			set { myWriteCustomStorage = value; }
		}
		/// <summary>
		/// The style to use when writting.
		/// </summary>
		/// <value>The style to use when writting.</value>
		public ORMCustomSerializedAttributeWriteStyle WriteStyle
		{
			get { return myWriteStyle; }
			set { myWriteStyle = value; }
		}
	}
	/// <summary>
	/// Custom serialization information for child elements.
	/// </summary>
	public class ORMCustomSerializedChildElementInfo : ORMCustomSerializedElementInfo
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		protected ORMCustomSerializedChildElementInfo()
		{
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="customName">The custom name to use.</param>
		/// <param name="childGuids">The child element guids.</param>
		public ORMCustomSerializedChildElementInfo(string customName, params Guid[] childGuids)
			: base(null, customName, null, ORMCustomSerializedElementWriteStyle.Element, null)
		{
			myGuidList = childGuids;
		}
		/// <summary>
		/// Main Constructor
		/// </summary>
		/// <param name="customPrefix">The custom prefix to use.</param>
		/// <param name="customName">The custom name to use.</param>
		/// <param name="customNamespace">The custom namespace to use.</param>
		/// <param name="writeStyle">The style to use when writting.</param>
		/// <param name="doubleTagName">The name of the double tag.</param>
		/// <param name="childGuids">The child element guids.</param>
		public ORMCustomSerializedChildElementInfo(string customPrefix, string customName, string customNamespace, ORMCustomSerializedElementWriteStyle writeStyle, string doubleTagName, params Guid[] childGuids)
			: base(customPrefix, customName, customNamespace, writeStyle, doubleTagName)
		{
			myGuidList = childGuids;
		}

		private IList<Guid> myGuidList;

		/// <summary>
		/// Default ORMCustomSerializedChildElementInfo
		/// </summary>
		public new static readonly ORMCustomSerializedChildElementInfo Default = new ORMCustomSerializedChildElementInfo();

		/// <summary>
		/// Test if the list of child elements contains the specified guid
		/// </summary>
		public bool ContainsGuid(Guid guid)
		{
			return myGuidList != null && myGuidList.Contains(guid);
		}
	}
	/// <summary>
	/// Data returned by IORMCustomSerializedElement.MapElementName.
	/// </summary>
	public struct ORMCustomSerializedElementMatch
	{
		/// <summary>
		/// Holds a single role guid, or the explicit relationship guid
		/// </summary>
		private Guid mySingleGuid;
		/// <summary>
		/// Holds multiple guids, or the single role guid if an explicit
		/// relationship guid is provided
		/// </summary>
		private Guid[] myMultiGuids;
		private ORMCustomSerializedElementMatchStyle myMatchStyle;
		private string myDoubleTagName;
		/// <summary>
		/// The element was recognized as a meta property.
		/// </summary>
		/// <param name="metaAttributeGuid">The guid identifying the meta property</param>
		/// <param name="doubleTagName">the name of the double tag, if any</param>
		public void InitializeAttribute(Guid metaAttributeGuid, string doubleTagName)
		{
			mySingleGuid = metaAttributeGuid;
			myMatchStyle = ORMCustomSerializedElementMatchStyle.Property;
			myDoubleTagName = (doubleTagName != null && doubleTagName.Length != 0) ? doubleTagName : null;
		}
		/// <summary>
		/// The element was recognized as an opposite role player
		/// </summary>
		/// <param name="oppositeDomainRoleIds">1 or more opposite meta role guids</param>
		public void InitializeRoles(params Guid[] oppositeDomainRoleIds)
		{
			Debug.Assert(oppositeDomainRoleIds != null && oppositeDomainRoleIds.Length != 0);
			if (oppositeDomainRoleIds.Length == 1)
			{
				mySingleGuid = oppositeDomainRoleIds[0];
				myMultiGuids = null;
				myMatchStyle = ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRole;
			}
			else
			{
				mySingleGuid = Guid.Empty;
				myMultiGuids = oppositeDomainRoleIds;
				myMatchStyle = ORMCustomSerializedElementMatchStyle.MultipleOppositeDomainRoles;
			}
			myDoubleTagName = null;
		}
		/// <summary>
		/// The element was recognized as an opposite role player. Optimized overload
		/// for 1 element.
		/// </summary>
		/// <param name="oppositeDomainRoleId">The opposite meta role guid</param>
		public void InitializeRoles(Guid oppositeDomainRoleId)
		{
			mySingleGuid = oppositeDomainRoleId;
			myMultiGuids = null;
			myMatchStyle = ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRole;
		}
		/// <summary>
		/// The element was recognized as an opposite role player of an explicit link type
		/// </summary>
		/// <param name="explicitRelationshipGuid">The guid of the meta relationship to create</param>
		/// <param name="oppositeDomainRoleIds">1 or more opposite meta role guids</param>
		public void InitializeRolesWithExplicitRelationship(Guid explicitRelationshipGuid, params Guid[] oppositeDomainRoleIds)
		{
			Debug.Assert(oppositeDomainRoleIds != null && oppositeDomainRoleIds.Length != 0);
			if (oppositeDomainRoleIds.Length == 1)
			{
				mySingleGuid = explicitRelationshipGuid;
				myMultiGuids = oppositeDomainRoleIds;
				myMatchStyle = ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRoleExplicitRelationshipType;
			}
			else
			{
				mySingleGuid = explicitRelationshipGuid;
				myMultiGuids = oppositeDomainRoleIds;
				myMatchStyle = ORMCustomSerializedElementMatchStyle.MultipleOppositeMetaRolesExplicitRelationshipType;
			}
			myDoubleTagName = null;
		}
		/// <summary>
		/// The element was recognized as an opposite role player of an explicit link type.
		/// Optimized overload for 1 element.
		/// </summary>
		/// <param name="explicitRelationshipGuid">The guid of the meta relationship to create</param>
		/// <param name="oppositeDomainRoleId">The opposite meta role guid</param>
		public void InitializeRolesWithExplicitRelationship(Guid explicitRelationshipGuid, Guid oppositeDomainRoleId)
		{
			mySingleGuid = explicitRelationshipGuid;
			myMultiGuids = new Guid[] { oppositeDomainRoleId };
			myMatchStyle = ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRoleExplicitRelationshipType;
		}
		/// <summary>
		/// The guid identifying the meta property. Valid for a match
		/// style of Property.
		/// </summary>
		public Guid DomainPropertyId
		{
			get
			{
				return (myMatchStyle == ORMCustomSerializedElementMatchStyle.Property) ? mySingleGuid : Guid.Empty;
			}
		}
		/// <summary>
		/// The guid identifying the opposite meta role if there is only
		/// one matching meta role. Valid for a match style of SingleOppositeDomainRole.
		/// </summary>
		public Guid SingleOppositeDomainRoleId
		{
			get
			{
				switch (myMatchStyle)
				{
					case ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRole:
						return mySingleGuid;
					case ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRoleExplicitRelationshipType:
						return myMultiGuids[0];
					default:
						return Guid.Empty;
				}
			}
		}
		/// <summary>
		/// The guids identifying multiple opposite meta roles. Valid for a match
		/// style of MultipleOppositeDomainRoles.
		/// </summary>
		public IList<Guid> OppositeDomainRoleIdCollection
		{
			get
			{
				switch (myMatchStyle)
				{
					case ORMCustomSerializedElementMatchStyle.MultipleOppositeDomainRoles:
					case ORMCustomSerializedElementMatchStyle.MultipleOppositeMetaRolesExplicitRelationshipType:
						return myMultiGuids;
					default:
						return null;
				}
			}
		}
		/// <summary>
		/// The guid identifying the meta relationship of the explicit relationship
		/// type to create. Validate for match styles of SingleOppositeDomainRoleExplicitRelationshipType
		/// and MultipleOppositeMetaRolesExplicitRelationshipType.
		/// </summary>
		public Guid ExplicitRelationshipGuid
		{
			get
			{
				switch (myMatchStyle)
				{
					case ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRoleExplicitRelationshipType:
					case ORMCustomSerializedElementMatchStyle.MultipleOppositeMetaRolesExplicitRelationshipType:
						return mySingleGuid;
					default:
						return Guid.Empty;
				}
			}
		}
		/// <summary>
		/// The type of element match
		/// </summary>
		/// <value>ORMCustomSerializedElementMatchStyle</value>
		public ORMCustomSerializedElementMatchStyle MatchStyle
		{
			get
			{
				return myMatchStyle;
			}
		}
		/// <summary>
		/// The double tag name for an property. null if the MatchStyle
		/// is not Property or if there is no double tag for this element.
		/// </summary>
		public string DoubleTagName
		{
			get
			{
				return myDoubleTagName;
			}
		}
	}
	#endregion Public Classes
	#region Public Interfaces
	/// <summary>
	/// The interface for getting custom element namespaces.
	/// </summary>
	public interface IORMCustomSerializedDomainModel
	{
		/// <summary>
		/// Return all namespaces used by custom elements in this model.
		/// </summary>
		/// <returns>Custom element namespaces. return value [*, 0] contains
		/// the prefix, [*, 1] contains the associated xml namespace, and [*, 2] contains
		/// the name of the schema file with no path. The schema file must be built into the
		/// model's assembly as an embedded resource (set via the Build Action property when the
		/// file is selected in the solution explorer) with the same namespace as the metamodel.
		/// This is most easily done by placing the schema file in the same directory as the
		/// model file, and making the namespace for the model file correspond to the default
		/// namespace for the directory (the project default namespace with the directory path appended)</returns>
		string[,] GetCustomElementNamespaces();
		/// <summary>
		/// Return the default element prefix for elements where the
		/// prefix is not specified
		/// </summary>
		string DefaultElementPrefix { get;}
		/// <summary>
		/// Return the meta class guids for all root elements.
		/// </summary>
		Guid[] GetRootElementClasses();
		/// <summary>
		/// Determine if a class or relationship should be serialized. This allows
		/// the serialization engine to do a meta-level sanity check before retrieving
		/// elements, and reduces the number of 'NotWritten' elements in the
		/// serialization specification file.
		/// </summary>
		/// <param name="store">The store to check</param>
		/// <param name="classInfo">The class or relationship to test</param>
		/// <returns>true if the element should be serialized</returns>
		bool ShouldSerializeDomainClass(Store store, DomainClassInfo classInfo);
		/// <summary>
		/// Map an xml namespace name and element name to a meta class guid
		/// </summary>
		/// <param name="xmlNamespace">The namespace of a top-level element (directly
		/// inside the ORM2 tag)</param>
		/// <param name="elementName">The name of the element to match</param>
		/// <returns>The guid of a DomainClassInfo, or Guid.Empty if not recognized</returns>
		Guid MapRootElement(string xmlNamespace, string elementName);
		/// <summary>
		/// Map an xml namespace name and element name to a meta class guid
		/// </summary>
		/// <param name="xmlNamespace">The namespace of the xml element</param>
		/// <param name="elementName">The name of the element to match</param>
		/// <returns>A meta class guid, or Guid.Empty if the name is not recognized</returns>
		Guid MapClassName(string xmlNamespace, string elementName);
	}
	/// <summary>
	/// The interface for getting element custom serialization information.
	/// </summary>
	public interface IORMCustomSerializedElement
	{
		/// <summary>
		/// Returns the supported operations.
		/// </summary>
		ORMCustomSerializedElementSupportedOperations SupportedCustomSerializedOperations { get;}
		/// <summary>
		/// Returns custom serialization information for child elements.
		/// </summary>
		/// <returns>Custom serialization information for child elements.</returns>
		ORMCustomSerializedChildElementInfo[] GetCustomSerializedChildElementInfo();
		/// <summary>
		/// Returns custom serialization information for elements.
		/// </summary>
		ORMCustomSerializedElementInfo CustomSerializedElementInfo { get;}
		/// <summary>
		/// Returns custom serialization information for properties.
		/// </summary>
		/// <param name="domainPropertyInfo">The property info.</param>
		/// <param name="rolePlayedInfo">If this is implemented on a ElementLink-derived class, then the
		/// played role is the role player containing the reference to the opposite role. Always null for a
		/// class element.</param>
		/// <returns>Custom serialization information for properties.</returns>
		ORMCustomSerializedPropertyInfo GetCustomSerializedPropertyInfo(DomainPropertyInfo domainPropertyInfo, DomainRoleInfo rolePlayedInfo);
		/// <summary>
		/// Returns custom serialization information for links.
		/// </summary>
		/// <param name="rolePlayedInfo">The role played.</param>
		/// <param name="elementLink">The link instance</param>
		/// <returns>Custom serialization information for links.</returns>
		ORMCustomSerializedElementInfo GetCustomSerializedLinkInfo(DomainRoleInfo rolePlayedInfo, ElementLink elementLink);
		/// <summary>
		/// Get a comparer to sort custom role elements. Affects the element order
		/// for nested child (aggregated) and link (referenced) elements
		/// </summary>
		IComparer<DomainRoleInfo> CustomSerializedChildRoleComparer { get;}
		/// <summary>
		/// Attempt to map an element name to a custom serialized child element.
		/// </summary>
		/// <param name="elementNamespace">The full xml namespace of the element to match. Note
		/// that using prefixes is not robust, so the full namespace needs to be specified.</param>
		/// <param name="elementName">The local name of the element</param>
		/// <param name="containerNamespace">The full xml namespace of the container to match. A
		/// container element is an element with no id or ref parameter.</param>
		/// <param name="containerName">The local name of the container</param>
		/// <returns>ORMCustomSerializedElementMatch. Use the MatchStyle property to determine levels of success.</returns>
		ORMCustomSerializedElementMatch MapChildElement(string elementNamespace, string elementName, string containerNamespace, string containerName);
		/// <summary>
		/// Attempt to map an property name to a custom serialized property
		/// for this element.
		/// </summary>
		/// <param name="xmlNamespace">The full xml namespace of the element to match. Note
		/// that using prefixes is not robust, so the full namespace needs to be specified.</param>
		/// <param name="attributeName">The local name of the property</param>
		/// <returns>A DomainPropertyId, or Guid.Empty. Use Guid.IsEmpty to test.</returns>
		Guid MapAttribute(string xmlNamespace, string attributeName);
		/// <summary>
		/// Check the current state of the object to determine
		/// if it should be serialized or not.
		/// </summary>
		/// <returns>false to block serialization</returns>
		bool ShouldSerialize();
	}
	#endregion Public Interfaces
	#region New Serialization
	/// <summary>
	///New Serialization
	/// </summary>
	public partial class ORMSerializer
	{
		#region Constants
		// These need to be "static readonly" rather than "const" so that other assemblies compiled against us
		// can detect the latest version at runtime.

		/// <summary>
		/// The standard prefix for the prefix used on the root node of the ORM document
		/// </summary>
		public static readonly string RootXmlPrefix = "ormRoot";
		/// <summary>
		/// The tag name for the element used as the root node of the ORM document
		/// </summary>
		public static readonly string RootXmlElementName = "ORM2";
		/// <summary>
		/// The namespace for the root node of the ORM document
		/// </summary>
		public static readonly string RootXmlNamespace = "http://schemas.neumont.edu/ORM/2006-04/ORMRoot";
		#endregion // Constants

		/// <summary>
		/// Used for sorting.
		/// </summary>
		/// <param name="writeStyle">An property write style.</param>
		/// <returns>A number to sort with.</returns>
		private static int PropertyWriteStylePriority(ORMCustomSerializedAttributeWriteStyle writeStyle)
		{
			switch (writeStyle)
			{
				case ORMCustomSerializedAttributeWriteStyle.Attribute:
					return 0;
				case ORMCustomSerializedAttributeWriteStyle.Element:
					return 1;
				case ORMCustomSerializedAttributeWriteStyle.DoubleTaggedElement:
					return 2;
			}
			return 3;
		}
		/// <summary>
		/// Used for serializing properties.
		/// </summary>
		/// <param name="guid">The GUID to convert.</param>
		/// <returns>An XML encoded string.</returns>
		private static string ToXml(Guid guid)
		{
			return '_' + XmlConvert.ToString(guid).ToUpperInvariant();
		}
		/// <summary>
		/// Serializes a property value to XML.
		/// </summary>
		/// <param name="element">The <see cref="ModelElement"/> containing the property to be serialized.</param>
		/// <param name="property">The <see cref="DomainPropertyInfo"/> to be serialized.</param>
		/// <returns>An XSD-appropriate <see cref="String"/> that represents the serialized value of the property.</returns>
		private static string ToXml(ModelElement element, DomainPropertyInfo property)
		{
			object value = property.GetValue(element);
			if (value == null)
			{
				return null;
			}
			Type type = property.PropertyType;
			if (!type.IsEnum)
			{
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Empty:
					case TypeCode.DBNull:
						return null;
					case TypeCode.String:
						return (string)value;
					case TypeCode.DateTime:
						return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Utc);
					case TypeCode.UInt64:
						return XmlConvert.ToString((ulong)value);
					case TypeCode.Int64:
						return XmlConvert.ToString((long)value);
					case TypeCode.UInt32:
						return XmlConvert.ToString((uint)value);
					case TypeCode.Int32:
						return XmlConvert.ToString((int)value);
					case TypeCode.UInt16:
						return XmlConvert.ToString((ushort)value);
					case TypeCode.Int16:
						return XmlConvert.ToString((short)value);
					case TypeCode.Byte:
						return XmlConvert.ToString((byte)value);
					case TypeCode.SByte:
						return XmlConvert.ToString((sbyte)value);
					case TypeCode.Char:
						return XmlConvert.ToString((char)value);
					case TypeCode.Boolean:
						return XmlConvert.ToString((bool)value);
					case TypeCode.Decimal:
						return XmlConvert.ToString((decimal)value);
					case TypeCode.Double:
						return XmlConvert.ToString((double)value);
					case TypeCode.Single:
						return XmlConvert.ToString((float)value);
				}
				if (type == typeof(Guid))
				{
					return ToXml((Guid)value);
				}
				else if (type == typeof(TimeSpan))
				{
					return XmlConvert.ToString((TimeSpan)value);
				}
			}
			Design.ORMTypeDescriptor.TypeDescriptorContext context = Design.ORMTypeDescriptor.CreateTypeDescriptorContext(element, property);
			return context.PropertyDescriptor.Converter.ConvertToInvariantString(context, value);
		}
		/// <summary>
		/// Used for serializing child elements.
		/// </summary>
		/// <param name="childElementInfo">The child element info to search.</param>
		/// <param name="guid">The GUID to find.</param>
		/// <returns>An index or -1.</returns>
		private static int FindGuid(ORMCustomSerializedChildElementInfo[] childElementInfo, Guid guid)
		{
			int count = childElementInfo.Length;

			for (int index = 0; index < count; ++index)
			{
				if (childElementInfo[index].ContainsGuid(guid))
				{
					return index;
				}
			}

			return -1;
		}
		/// <summary>
		/// Sorts mixed typed properties.
		/// </summary>
		/// <param name="customElement">The element.</param>
		/// <param name="rolePlayedInfo">The role being played.</param>
		/// <param name="properties">The element's properties.</param>
		private static void SortProperties(IORMCustomSerializedElement customElement, DomainRoleInfo rolePlayedInfo, ref IList<DomainPropertyInfo> properties)
		{
			int propertyCount = properties.Count;
			if (propertyCount > 0)
			{
				ORMCustomSerializedPropertyInfo[] customInfo = new ORMCustomSerializedPropertyInfo[propertyCount];
				int[] indices = new int[propertyCount];
				for (int i = 0; i < propertyCount; ++i)
				{
					indices[i] = i;
					customInfo[i] = customElement.GetCustomSerializedPropertyInfo(properties[i], rolePlayedInfo);
				}
				Array.Sort<int>(indices, delegate(int index1, int index2)
				{
					ORMCustomSerializedPropertyInfo customInfo1 = customInfo[index1];
					ORMCustomSerializedPropertyInfo customInfo2 = customInfo[index2];
					int ws0 = PropertyWriteStylePriority(customInfo1.WriteStyle);
					int ws1 = PropertyWriteStylePriority(customInfo2.WriteStyle);

					if (ws0 > ws1)
					{
						return 1;
					}
					else if (ws0 < ws1)
					{
						return -1;
					}

					return 0;
				});
				for (int i = 0; i < propertyCount; ++i)
				{
					if (indices[i] != i)
					{
						DomainPropertyInfo[] reorderedList = new DomainPropertyInfo[propertyCount];
						for (int j = 0; j < propertyCount; ++j)
						{
							reorderedList[indices[j]] = properties[j];
						}
						properties = reorderedList;
						break;
					}
				}
			}
			return;
		}
		/// <summary>
		/// Writes a customized begin element tag.
		/// </summary>
		/// <param name="file">The file to write to.</param>
		/// <param name="customInfo">The customized tag info.</param>
		/// <param name="defaultPrefix">The default prefix.</param>
		/// <param name="defaultName">The default tag name.</param>
		/// <returns>true if the begin element tag was written.</returns>
		private static bool WriteCustomizedStartElement(XmlWriter file, ORMCustomSerializedElementInfo customInfo, string defaultPrefix, string defaultName)
		{
			if (customInfo != null)
			{
				switch (customInfo.WriteStyle)
				{
					case ORMCustomSerializedElementWriteStyle.NotWritten:
						{
							return false;
						}
					case ORMCustomSerializedElementWriteStyle.DoubleTaggedElement:
						{
							string prefix = (customInfo.CustomPrefix != null ? customInfo.CustomPrefix : defaultPrefix);
							string name = (customInfo.CustomName != null ? customInfo.CustomName : defaultName);

							file.WriteStartElement
							(
								prefix,
								name,
								customInfo.CustomNamespace
							);
							file.WriteStartElement
							(
								prefix,
								customInfo.DoubleTagName != null ? customInfo.DoubleTagName : name,
								customInfo.CustomNamespace
							);

							return true;
						}
				}

				file.WriteStartElement
				(
					customInfo.CustomPrefix != null ? customInfo.CustomPrefix : defaultPrefix,
					customInfo.CustomName != null ? customInfo.CustomName : defaultName,
					customInfo.CustomNamespace
				);
			}
			else
			{
				file.WriteStartElement(defaultPrefix, defaultName, null);
			}
			return true;
		}
		/// <summary>
		/// Writes a customized end element tag.
		/// </summary>
		/// <param name="file">The file to write to.</param>
		/// <param name="customInfo">The customized tag info.</param>
		private static void WriteCustomizedEndElement(XmlWriter file, ORMCustomSerializedElementInfo customInfo)
		{
			if (customInfo != null)
			{
				switch (customInfo.WriteStyle)
				{
#if DEBUG
					case ORMCustomSerializedElementWriteStyle.NotWritten:
						{
							Debug.Fail("WriteCustomizedEndElement - ORMCustomSerializedElementWriteStyle.DontWrite");
							throw new InvalidOperationException();
						}
#endif
					case ORMCustomSerializedElementWriteStyle.DoubleTaggedElement:
						{
							file.WriteEndElement();
							break;
						}
				}
			}

			file.WriteEndElement();

			return;
		}
		/// <summary>
		/// Find the parent model for this element.
		/// </summary>
		/// <param name="element">A ModelElement being serialized</param>
		/// <returns>IORMCustomSerializedDomainModel, or null</returns>
		private static IORMCustomSerializedDomainModel GetParentModel(ModelElement element)
		{
			return element.Store.GetDomainModel(element.GetDomainClass().DomainModel.Id) as IORMCustomSerializedDomainModel;
		}
		/// <summary>
		/// Determine based on the type of role and opposite role player if any elements of
		/// the given type should be serialized.
		/// </summary>
		/// <param name="parentModel">The parent model of an element</param>
		/// <param name="role">The role played</param>
		/// <returns>true if serialization should continue</returns>
		private static bool ShouldSerializeDomainRole(IORMCustomSerializedDomainModel parentModel, DomainRoleInfo role)
		{
			if (parentModel == null)
			{
				return true;
			}
			Store store = ((DomainModel)parentModel).Store;
			return parentModel.ShouldSerializeDomainClass(store, role.DomainRelationship) && parentModel.ShouldSerializeDomainClass(store, role.OppositeDomainRole.RolePlayer);
		}
		/// <summary>
		/// Determine if an element should be serialized
		/// </summary>
		/// <param name="modelElement">Element to test</param>
		/// <returns>true unless the element is custom serialized and ShouldSerialize returns false.</returns>
		private static bool ShouldSerializeElement(ModelElement modelElement)
		{
			IORMCustomSerializedElement customElement = modelElement as IORMCustomSerializedElement;
			return (customElement != null) ? customElement.ShouldSerialize() : true;
		}
		/// <summary>
		/// Get the default prefix for an element from the meta model containing the element
		/// </summary>
		private static string DefaultElementPrefix(ModelElement element)
		{
			string retVal = null;
			IORMCustomSerializedDomainModel parentModel = GetParentModel(element);
			if (parentModel != null)
			{
				retVal = parentModel.DefaultElementPrefix;
			}
			return retVal;
		}
		/// <summary>
		/// Serializes a property.
		/// </summary>
		/// <param name="file">The file to write to.</param>
		/// <param name="element">The element.</param>
		/// <param name="customElement">The element as a custom element.</param>
		/// <param name="rolePlayedInfo">The role being played.</param>
		/// <param name="property">The element's property to write.</param>
		/// <param name="isCustomProperty">true if the property has custom info.</param>
		private static void SerializeProperties(XmlWriter file, ModelElement element, IORMCustomSerializedElement customElement, DomainRoleInfo rolePlayedInfo, DomainPropertyInfo property, bool isCustomProperty)
		{
			if (!isCustomProperty)
			{
				if (property.Kind != DomainPropertyKind.CustomStorage)
				{
					file.WriteAttributeString(property.Name, ToXml(element, property));
				}
				return;
			}

			ORMCustomSerializedPropertyInfo customInfo = customElement.GetCustomSerializedPropertyInfo(property, rolePlayedInfo);

			if (property.Kind != DomainPropertyKind.CustomStorage || customInfo.WriteCustomStorage)
			{
				if (customInfo.WriteStyle != ORMCustomSerializedAttributeWriteStyle.Attribute || file.WriteState != WriteState.Element)
				{
					switch (customInfo.WriteStyle)
					{
						default:
							{
								file.WriteElementString
								(
									customInfo.CustomPrefix != null ? customInfo.CustomPrefix : DefaultElementPrefix(element),
									customInfo.CustomName != null ? customInfo.CustomName : property.Name,
									customInfo.CustomNamespace,
									ToXml(element, property)
								);
								break;
							}
						case ORMCustomSerializedAttributeWriteStyle.NotWritten:
							{
								break;
							}
						case ORMCustomSerializedAttributeWriteStyle.DoubleTaggedElement:
							{
								string prefix = (customInfo.CustomPrefix != null ? customInfo.CustomPrefix : DefaultElementPrefix(element));
								string name = (customInfo.CustomName != null ? customInfo.CustomName : property.Name);

								file.WriteStartElement
								(
									prefix,
									name,
									customInfo.CustomNamespace
								);
								file.WriteElementString
								(
									prefix,
									customInfo.DoubleTagName != null ? customInfo.DoubleTagName : name,
									customInfo.CustomNamespace,
									ToXml(element, property)
								);
								file.WriteEndElement();

								break;
							}
					}
				}
				else
				{
					file.WriteAttributeString
					(
						customInfo.CustomPrefix,
						customInfo.CustomName != null ? customInfo.CustomName : property.Name,
						customInfo.CustomNamespace,
						ToXml(element, property)
					);
				}
			}

			return;
		}
		/// <summary>
		/// Serializes all properties of an element.
		/// </summary>
		/// <param name="file">The file to write to.</param>
		/// <param name="element">The element.</param>
		/// <param name="customElement">The element as a custom element.</param>
		/// <param name="rolePlayedInfo">The role being played.</param>
		/// <param name="properties">The element's properties.</param>
		/// <param name="hasCustomAttributes">true if the element has properties with custom info.</param>
		private static void SerializeProperties(XmlWriter file, ModelElement element, IORMCustomSerializedElement customElement, DomainRoleInfo rolePlayedInfo, IList<DomainPropertyInfo> properties, bool hasCustomAttributes)
		{
			for (int index = 0, count = properties.Count; index < count; ++index)
			{
				DomainPropertyInfo property = properties[index];

				SerializeProperties
				(
					file,
					element,
					customElement,
					rolePlayedInfo,
					property,
					hasCustomAttributes
				);
			}
			return;
		}
		/// <summary>
		/// Serializes a link.
		/// </summary>
		/// <param name="file">The file to write to.</param>
		/// <param name="link">The link.</param>
		/// <param name="rolePlayer">The role player.</param>
		/// <param name="oppositeRolePlayer">The opposite role player.</param>
		/// <param name="rolePlayedInfo">The role being played.</param>
		private void SerializeLink(XmlWriter file, ElementLink link, ModelElement rolePlayer, ModelElement oppositeRolePlayer, DomainRoleInfo rolePlayedInfo)
		{
			ORMCustomSerializedElementSupportedOperations supportedOperations = ORMCustomSerializedElementSupportedOperations.None;
			ORMCustomSerializedElementInfo customInfo = ORMCustomSerializedElementInfo.Default;
			IList<DomainPropertyInfo> properties = null;
			string defaultPrefix;
			bool hasCustomAttributes = false;

			if (!ShouldSerializeElement(link) || !ShouldSerializeElement(rolePlayer))
			{
				return;
			}
			IORMCustomSerializedElement rolePlayerCustomElement = rolePlayer as IORMCustomSerializedElement;
			IORMCustomSerializedElement customElement = rolePlayerCustomElement;
			ORMCustomSerializedElementWriteStyle writeStyle;
			bool aggregatingLink = false;
			bool writeContents = customElement != null &&
				0 != (customElement.SupportedCustomSerializedOperations & ORMCustomSerializedElementSupportedOperations.LinkInfo) &&
				((writeStyle = customElement.GetCustomSerializedLinkInfo(rolePlayedInfo.OppositeDomainRole, link).WriteStyle) == ORMCustomSerializedElementWriteStyle.PrimaryLinkElement ||
				(aggregatingLink = writeStyle == ORMCustomSerializedElementWriteStyle.EmbeddingLinkElement));

			if (writeContents)
			{
				customElement = link as IORMCustomSerializedElement;
				properties = link.GetDomainClass().AllDomainProperties;
				defaultPrefix = DefaultElementPrefix(link);
			}
			else
			{
				defaultPrefix = DefaultElementPrefix(rolePlayer);
			}

			if (customElement != null)
			{
				supportedOperations = customElement.SupportedCustomSerializedOperations;

				if (0 != (customElement.SupportedCustomSerializedOperations & ORMCustomSerializedElementSupportedOperations.MixedTypedAttributes) && properties != null)
				{
					SortProperties(customElement, rolePlayedInfo, ref properties);
				}
				hasCustomAttributes = (supportedOperations & ORMCustomSerializedElementSupportedOperations.PropertyInfo) != 0;

				IORMCustomSerializedElement tagCustomElement = customElement;
				if (writeContents)
				{
					tagCustomElement = rolePlayerCustomElement;
					if (tagCustomElement != null)
					{
						if (0 != (tagCustomElement.SupportedCustomSerializedOperations & ORMCustomSerializedElementSupportedOperations.LinkInfo))
						{
							customInfo = tagCustomElement.GetCustomSerializedLinkInfo(rolePlayedInfo.OppositeDomainRole, link);
						}
					}
				}
				else if ((supportedOperations & ORMCustomSerializedElementSupportedOperations.LinkInfo) != 0)
				{
					customInfo = customElement.GetCustomSerializedLinkInfo(rolePlayedInfo.OppositeDomainRole, link);
					if (customInfo.IsDefault && GetParentModel(rolePlayer) != GetParentModel(oppositeRolePlayer))
					{
						return;
					}
				}
#if !WRITE_ALL_DEFAULT_LINKS
				else if (GetParentModel(rolePlayer) != GetParentModel(oppositeRolePlayer))
				{
					return;
				}
#endif // WRITE_ALL_DEFAULT_LINKS
			}

			if (!WriteCustomizedStartElement(file, customInfo, defaultPrefix, string.Concat(rolePlayedInfo.DomainRelationship.Name, ".", rolePlayedInfo.OppositeDomainRole.Name)))
			{
				return;
			}

			Guid keyId = writeContents ? link.Id : oppositeRolePlayer.Id;
			if (writeContents)
			{
				ReadOnlyCollection<DomainRoleInfo> rolesPlayed = link.GetDomainClass().AllDomainRolesPlayed;
				bool writeChildren = aggregatingLink || rolesPlayed.Count != 0;

				if (writeChildren)
				{
					// UNDONE: Be smarter here. If none of the relationships for the played
					// roles are actually serialized, then we don't need this at all.
					file.WriteAttributeString("id", ToXml(keyId));
				}
				if (!aggregatingLink)
				{
					file.WriteAttributeString("ref", ToXml(oppositeRolePlayer.Id));
				}

				SerializeProperties(file, link, customElement, rolePlayedInfo, properties, hasCustomAttributes);

				if (writeChildren)
				{
					ORMCustomSerializedChildElementInfo[] childElementInfo;
					bool groupRoles;

					childElementInfo = ((groupRoles = (0 != (supportedOperations & ORMCustomSerializedElementSupportedOperations.ChildElementInfo))) ? customElement.GetCustomSerializedChildElementInfo() : null);

					//write children
					SerializeChildElements(file, link, customElement, childElementInfo, rolesPlayed, 0 != (supportedOperations & ORMCustomSerializedElementSupportedOperations.CustomSortChildRoles), groupRoles, defaultPrefix);
				}
			}
			else
			{
				file.WriteAttributeString("ref", ToXml(keyId));
			}

			WriteCustomizedEndElement(file, customInfo);

			return;
		}
		/// <summary>
		/// Serializes a child element.
		/// </summary>
		/// <param name="file">The file to write to.</param>
		/// <param name="childElement">The child element.</param>
		/// <param name="rolePlayedInfo">The role being played.</param>
		/// <param name="oppositeRoleInfo">The opposite role being played.</param>
		/// <param name="customInfo">The custom element info.</param>
		/// <param name="defaultPrefix">The default prefix.</param>
		/// <param name="writeBeginElement">true to write the begin element tag.</param>
		/// <returns>true if the begin element tag was written.</returns>
		private bool SerializeChildElement(XmlWriter file, ModelElement childElement, DomainRoleInfo rolePlayedInfo, DomainRoleInfo oppositeRoleInfo, ORMCustomSerializedElementInfo customInfo, string defaultPrefix, bool writeBeginElement)
		{
			bool ret = false;
			DomainClassInfo lastChildClass = null;
			IORMCustomSerializedDomainModel parentModel = null;
			// If there class derived from the role player, then the class-level serialization settings may be
			// different than they were on the class specified on the role player, we need to check explicitly,
			// despite the earlier call to ShouldSerializeDomainRole
			bool checkSerializeClass = oppositeRoleInfo.RolePlayer.AllDescendants.Count != 0;
			Store store = myStore;
			bool isAggregate = rolePlayedInfo.IsEmbedding;
			bool oppositeIsAggregate = oppositeRoleInfo.IsEmbedding;
			IORMCustomSerializedElement testChildInfo;

			if (!isAggregate &&
				(!oppositeIsAggregate ||
				(oppositeIsAggregate &&
				null != (testChildInfo = childElement as IORMCustomSerializedElement) &&
				0 != (testChildInfo.SupportedCustomSerializedOperations & ORMCustomSerializedElementSupportedOperations.EmbeddingLinkInfo) &&
				testChildInfo.GetCustomSerializedLinkInfo(oppositeRoleInfo, null).WriteStyle == ORMCustomSerializedElementWriteStyle.EmbeddingLinkElement))) //write link
			{
				ReadOnlyCollection<ElementLink> links = rolePlayedInfo.GetElementLinks<ElementLink>(childElement);
				int linksCount = links.Count;
				if (links.Count != 0)
				{
					bool checkSerializeLinkClass = rolePlayedInfo.DomainRelationship.AllDescendants.Count != 0;
					DomainRelationshipInfo lastLinkClass = null;
					IORMCustomSerializedDomainModel linkParentModel = null;
					for (int i = 0; i < linksCount; ++i)
					{
						// Verify that the link itself should be serialized
						ElementLink link = links[i];
						if (checkSerializeLinkClass)
						{
							DomainRelationshipInfo linkClass = link.GetDomainRelationship();
							if (linkClass != lastLinkClass)
							{
								lastLinkClass = linkClass;
								linkParentModel = GetParentModel(link);
							}
							if (linkParentModel != null && !linkParentModel.ShouldSerializeDomainClass(store, linkClass))
							{
								continue;
							}
						}

						// Verify that the opposite role player class should be serialized
						ModelElement oppositeRolePlayer = oppositeRoleInfo.GetRolePlayer(link);
						if (checkSerializeClass)
						{
							DomainClassInfo childClass = oppositeRolePlayer.GetDomainClass();
							if (childClass != lastChildClass)
							{
								lastChildClass = childClass;
								parentModel = GetParentModel(oppositeRolePlayer);
							}
							if (parentModel != null && !parentModel.ShouldSerializeDomainClass(store, childClass))
							{
								continue;
							}
						}

						if (writeBeginElement && !ret && customInfo != null)
						{
							if (!WriteCustomizedStartElement(file, customInfo, defaultPrefix, customInfo.CustomName))
							{
								return false;
							}
							ret = true;
						}
						SerializeLink(file, link, childElement, oppositeRolePlayer, rolePlayedInfo);
					}
				}
			}
			else if (isAggregate) //write child
			{
				LinkedElementCollection<ModelElement> children = rolePlayedInfo.GetLinkedElements(childElement);
				int childCount = children.Count;

				if (childCount != 0)
				{
					string containerName = null;
					bool initializedContainerName = !writeBeginElement;

					for (int iChild = 0; iChild < childCount; ++iChild)
					{
						ModelElement child = children[iChild];

						if (checkSerializeClass)
						{
							DomainClassInfo childClass = child.GetDomainClass();
							if (childClass != lastChildClass)
							{
								lastChildClass = childClass;
								parentModel = GetParentModel(child);
							}
							if (parentModel != null && !parentModel.ShouldSerializeDomainClass(store, childClass))
							{
								continue;
							}
						}

						if (ShouldSerializeElement(child))
						{
							if (customInfo == null)
							{
								defaultPrefix = DefaultElementPrefix(child);
							}
							if (!initializedContainerName)
							{
								containerName = string.Concat(rolePlayedInfo.DomainRelationship.Name, ".", rolePlayedInfo.OppositeDomainRole.Name);
								initializedContainerName = true;
							}
							if (!SerializeElement(file, child, customInfo, defaultPrefix, ref containerName))
							{
								return false;
							}
						}
					}
					ret = writeBeginElement && initializedContainerName && containerName == null;
				}
			}
			return ret;
		}
		private void SerializeChildElements(XmlWriter file, ModelElement element, IORMCustomSerializedElement customElement, ORMCustomSerializedChildElementInfo[] childElementInfo, IList<DomainRoleInfo> rolesPlayed, bool sortRoles, bool groupRoles, string defaultPrefix)
		{
			int rolesPlayedCount = rolesPlayed.Count;
			IORMCustomSerializedDomainModel parentModel = GetParentModel(element);

			//sort played roles
			if (sortRoles && rolesPlayedCount != 0)
			{
				IComparer<DomainRoleInfo> comparer = customElement.CustomSerializedChildRoleComparer;
				if (comparer != null)
				{
					((List<DomainRoleInfo>)(rolesPlayed = new List<DomainRoleInfo>(rolesPlayed))).Sort(comparer);
				}
			}

			//write children
			if (groupRoles)
			{
				bool[] written = new bool[rolesPlayedCount];

				for (int index0 = 0; index0 < rolesPlayedCount; ++index0)
				{
					if (!written[index0])
					{
						DomainRoleInfo rolePlayedInfo = rolesPlayed[index0];
						if (!ShouldSerializeDomainRole(parentModel, rolePlayedInfo))
						{
							written[index0] = true;
							continue;
						}
						DomainRoleInfo oppositeRoleInfo = rolePlayedInfo.OppositeDomainRole;
						ORMCustomSerializedChildElementInfo customChildInfo;
						bool writeEndElement = false;

						int childIndex = FindGuid(childElementInfo, oppositeRoleInfo.Id);
						customChildInfo = (childIndex >= 0) ? childElementInfo[childIndex] : null;
						string defaultChildPrefix = (customChildInfo != null) ? defaultPrefix : null;

						written[index0] = true;
						if (SerializeChildElement(file, element, rolePlayedInfo, oppositeRoleInfo, customChildInfo, defaultChildPrefix, true))
						{
							writeEndElement = true;
						}

						if (customChildInfo != null)
						{
							for (int index1 = index0 + 1; index1 < rolesPlayedCount; ++index1)
							{
								if (!written[index1])
								{
									rolePlayedInfo = rolesPlayed[index1];
									if (!ShouldSerializeDomainRole(parentModel, rolePlayedInfo))
									{
										written[index1] = true;
										continue;
									}
									oppositeRoleInfo = rolePlayedInfo.OppositeDomainRole;

									if (customChildInfo.ContainsGuid(oppositeRoleInfo.Id))
									{
										written[index1] = true;
										if (SerializeChildElement(file, element, rolePlayedInfo, oppositeRoleInfo, customChildInfo, defaultChildPrefix, !writeEndElement))
										{
											writeEndElement = true;
										}
									}
								}
							}
						}

						if (writeEndElement)
						{
							WriteCustomizedEndElement(file, customChildInfo);
						}
					}
				}
			}
			else
			{
				for (int index = 0; index < rolesPlayedCount; ++index)
				{
					DomainRoleInfo rolePlayedInfo = rolesPlayed[index];
					if (!ShouldSerializeDomainRole(parentModel, rolePlayedInfo))
					{
						continue;
					}
					if (SerializeChildElement(file, element, rolePlayedInfo, rolePlayedInfo.OppositeDomainRole, null, null, true))
					{
						WriteCustomizedEndElement(file, null);
					}
				}
			}

			return;
		}
		/// <summary>
		/// Recursivly serializes elements.
		/// </summary>
		/// <param name="file">The file to write to.</param>
		/// <param name="element">The element.</param>
		/// <param name="containerCustomInfo">The container element's custom serialization information.</param>
		/// <param name="containerPrefix">The container element's prefix.</param>
		/// <param name="containerName">The container element's name.</param>
		/// <returns>false if the container element was not written.</returns>
		private bool SerializeElement(XmlWriter file, ModelElement element, ORMCustomSerializedElementInfo containerCustomInfo, string containerPrefix, ref string containerName)
		{
			if (!ShouldSerializeElement(element)) return true;
			ORMCustomSerializedElementSupportedOperations supportedOperations;
			ORMCustomSerializedChildElementInfo[] childElementInfo = null;
			DomainClassInfo classInfo = element.GetDomainClass();
			ORMCustomSerializedElementInfo customInfo;
			IORMCustomSerializedElement customElement = element as IORMCustomSerializedElement;
			IList<DomainPropertyInfo> properties = classInfo.AllDomainProperties;
			ReadOnlyCollection<DomainRoleInfo> rolesPlayed = classInfo.AllDomainRolesPlayed;
			string defaultPrefix = DefaultElementPrefix(element);
			bool roleGrouping = false;
			bool isCustom = (customElement != null);

			//load custom information
			if (isCustom)
			{
				supportedOperations = customElement.SupportedCustomSerializedOperations;

				if (0 != (customElement.SupportedCustomSerializedOperations & ORMCustomSerializedElementSupportedOperations.MixedTypedAttributes))
				{
					SortProperties(customElement, null, ref properties);
				}
				if (roleGrouping = (0 != (supportedOperations & ORMCustomSerializedElementSupportedOperations.ChildElementInfo)))
				{
					childElementInfo = customElement.GetCustomSerializedChildElementInfo();
				}
				if ((supportedOperations & ORMCustomSerializedElementSupportedOperations.ElementInfo) != 0)
				{
					customInfo = customElement.CustomSerializedElementInfo;
					if (customInfo.WriteStyle == ORMCustomSerializedElementWriteStyle.NotWritten) return true;
				}
				else
				{
					customInfo = ORMCustomSerializedElementInfo.Default;
				}
			}
			else
			{
				supportedOperations = ORMCustomSerializedElementSupportedOperations.None;
				customInfo = ORMCustomSerializedElementInfo.Default;
			}

			//write container begin element
			if (containerName != null)
			{
				if (!WriteCustomizedStartElement(file, containerCustomInfo, containerPrefix, containerName))
				{
					return false;
				}
				containerName = null;
			}

			//write begin element tag
			if (!WriteCustomizedStartElement(file, customInfo, defaultPrefix, classInfo.Name)) return true;
			file.WriteAttributeString("id", ToXml(element.Id));

			//write properties
			SerializeProperties
			(
				file,
				element,
				customElement,
				null,
				properties,
				(supportedOperations & ORMCustomSerializedElementSupportedOperations.PropertyInfo) != 0
			);

			//write children
			SerializeChildElements(file, element, customElement, childElementInfo, rolesPlayed, 0 != (supportedOperations & ORMCustomSerializedElementSupportedOperations.CustomSortChildRoles), roleGrouping, defaultPrefix);

			//write end element tag
			WriteCustomizedEndElement(file, customInfo);

			return true;
		}
		/// <summary>
		/// Recursivly serializes elements.
		/// </summary>
		/// <param name="file">The file to write to.</param>
		/// <param name="element">The element.</param>
		private void SerializeElement(XmlWriter file, ModelElement element)
		{
			string containerName = null;
			SerializeElement(file, element, null, null, ref containerName);
			return;
		}
		/// <summary>
		/// New XML Serialization
		/// </summary>
		public void Save(Stream stream)
		{
			XmlWriterSettings xmlSettings = new XmlWriterSettings();
			XmlWriter file;
			Store store = myStore;
			ICollection<DomainModel> values = store.DomainModels;

			xmlSettings.IndentChars = "\t";
			xmlSettings.Indent = true;

			file = XmlWriter.Create(stream, xmlSettings);
			file.WriteStartElement(RootXmlPrefix, RootXmlElementName, RootXmlNamespace);

			//serialize namespaces
			foreach (DomainModel value in values)
			{
				IORMCustomSerializedDomainModel ns = value as IORMCustomSerializedDomainModel;

				if (ns != null)
				{
					string[,] namespaces = ns.GetCustomElementNamespaces();

					for (int index = 0, count = namespaces.GetLength(0); index < count; ++index)
					{
						//if (/*namespaces[index].Length==2 && */namespaces[index,0] != null && namespaces[index,1] != null)
						file.WriteAttributeString("xmlns", namespaces[index, 0], null, namespaces[index, 1]);
					}
				}
			}

			//serialize all root elements
			IElementDirectory elementDir = myStore.ElementDirectory;
			foreach (DomainModel value in values)
			{
				IORMCustomSerializedDomainModel ns = value as IORMCustomSerializedDomainModel;

				if (ns != null)
				{
					Guid[] metaClasses = ns.GetRootElementClasses();
					if (metaClasses != null)
					{
						int classCount = metaClasses.Length;
						for (int i = 0; i < classCount; ++i)
						{
							ReadOnlyCollection<ModelElement> elements = elementDir.FindElements(metaClasses[i]);
							int elementCount = elements.Count;
							for (int j = 0; j < elementCount; ++j)
							{
								SerializeElement(file, elements[j]);
							}
						}
					}
				}
			}

			file.WriteEndElement();
			file.Close();

			return;
		}
	}
	#endregion New Serialization
	#region New Deserialization
	public partial class ORMSerializer
	{
		/// <summary>
		/// Structure used to map a guid to one or multiple
		/// placeholder elements. Because the type to create
		/// is not known until the placeholder is no longer
		/// needed, it is possible to choose different opposite
		/// placeholder types for the same Guid, so we need
		/// to be prepared to have more than one placeholder
		/// in the store for a single element.
		/// </summary>
		private struct PlaceholderElement
		{
			private ModelElement mySingleElement;
			private Collection<ModelElement> myMultipleElements;
			/// <summary>
			/// Call to create a placeholder element of the given type
			/// in the store. A placeholder is always created with a new
			/// Guid.
			/// </summary>
			/// <param name="store">Context store</param>
			/// <param name="classInfo">The classInfo of the role player. Note
			/// that it is very possible that the classInfo will be abstract. The
			/// descendants are searched to find the first non-abstract class</param>
			/// <returns>A new model element, or an existing placeholder.</returns>
			public ModelElement CreatePlaceholderElement(Store store, DomainClassInfo classInfo)
			{
				ModelElement retVal = FindElementOfType(classInfo);
				if (retVal == null)
				{
					retVal = RealizeClassInfo(store.ElementFactory, classInfo);
					if (myMultipleElements != null)
					{
						myMultipleElements.Add(retVal);
					}
					else if (mySingleElement != null)
					{
						myMultipleElements = new Collection<ModelElement>();
						myMultipleElements.Add(mySingleElement);
						myMultipleElements.Add(retVal);
						mySingleElement = null;
					}
					else
					{
						mySingleElement = retVal;
					}
				}
				return retVal;
			}
			/// <summary>
			/// Create a ModelElement of a type compatible with the specified classInfo
			/// </summary>
			/// <param name="elementFactory">The ElementFactory from the context store</param>
			/// <param name="classInfo">The meta information for the class to create</param>
			/// <returns>ModelElement</returns>
			private ModelElement RealizeClassInfo(ElementFactory elementFactory, DomainClassInfo classInfo)
			{
				Type implClass = classInfo.ImplementationClass;
				if (implClass.IsAbstract || implClass == typeof(ModelElement)) // The class factory won't create a raw model element
				{
					DomainClassInfo descendantInfo = FindCreatableClass(classInfo.AllDescendants); // Try the cheap search first
					if (descendantInfo != null)
					{
						descendantInfo = FindCreatableClass(classInfo.AllDescendants);
					}
					Debug.Assert(descendantInfo != null); // Some descendant should always be creatable, otherwise there could not be a valid link
					classInfo = descendantInfo;
				}
				DomainRelationshipInfo relationshipInfo;
				ModelElement retVal = null;
				int domainRoleCount;
				ReadOnlyCollection<DomainRoleInfo> domainRoles;
				if (null != (relationshipInfo = classInfo as DomainRelationshipInfo) &&
					0 != (domainRoleCount = (domainRoles = relationshipInfo.DomainRoles).Count))
				{
					// If this is a link element, then we need to create it with dummy role players
					// to go along with the dummy element. The framework will allow the create of
					// an ElementLink using CreateElement, but there is no way to remove that element
					// unless it has the correct roles players attached to it.
					RoleAssignment[] assignments = new RoleAssignment[domainRoleCount];
					for (int i = 0; i < domainRoleCount; ++i)
					{
						DomainRoleInfo roleInfo = domainRoles[i];
						assignments[i] = new RoleAssignment(roleInfo.Id, RealizeClassInfo(elementFactory, roleInfo.RolePlayer));
					}
					retVal = elementFactory.CreateElementLink(relationshipInfo, assignments);
				}
				else
				{
					retVal = elementFactory.CreateElement(classInfo);
				}
				return retVal;
			}
			private static DomainClassInfo FindCreatableClass(IList<DomainClassInfo> classInfos)
			{
				DomainClassInfo retVal = null;
				int count = classInfos.Count;
				if (count != 0)
				{
					for (int i = 0; i < count; ++i)
					{
						DomainClassInfo testInfo = classInfos[i];
						if (!testInfo.ImplementationClass.IsAbstract)
						{
							retVal = testInfo;
							break;
						}
					}
				}
				return retVal;
			}
			/// <summary>
			/// Find a placeholder instance with the specified meta type
			/// </summary>
			/// <param name="classInfo">The type to search for</param>
			/// <returns>The matching element, or null</returns>
			private ModelElement FindElementOfType(DomainClassInfo classInfo)
			{
				DomainRelationshipInfo relInfo = classInfo as DomainRelationshipInfo;
				if (relInfo != null)
				{
					ElementLink link;
					if (mySingleElement != null)
					{
						if (null != (link = mySingleElement as ElementLink) &&
							link.GetDomainRelationship() == relInfo)
						{
							return mySingleElement;
						}
					}
					else if (myMultipleElements != null)
					{
						foreach (ModelElement mel in myMultipleElements)
						{
							if (null != (link = mel as ElementLink) &&
								link.GetDomainRelationship() == relInfo)
							{
								return mel;
							}
						}
					}
				}
				else
				{
					if (mySingleElement != null)
					{
						if (mySingleElement.GetDomainClass() == classInfo)
						{
							return mySingleElement;
						}
					}
					else if (myMultipleElements != null)
					{
						foreach (ModelElement mel in myMultipleElements)
						{
							if (mel.GetDomainClass() == classInfo)
							{
								return mel;
							}
						}
					}
				}
				return null;
			}
			/// <summary>
			/// Take all placeholder elements, transfer all roles to the
			/// realElement, and then remove the placeholder element from the
			/// store.
			/// </summary>
			/// <param name="realElement">An element created with the real
			/// identity and type.</param>
			public void FulfilPlaceholderRoles(ModelElement realElement)
			{
				if (mySingleElement != null)
				{
					FulfilPlaceholderRoles(realElement, mySingleElement);
				}
				else
				{
					Debug.Assert(myMultipleElements != null);
					foreach (ModelElement placeholder in myMultipleElements)
					{
						FulfilPlaceholderRoles(realElement, placeholder);
					}
				}
			}
			private static void FulfilPlaceholderRoles(ModelElement realElement, ModelElement placeholder)
			{
				ReadOnlyCollection<ElementLink> links = DomainRoleInfo.GetAllElementLinks(placeholder);
				int linkCount = links.Count;
				for (int i = linkCount - 1; i >= 0; --i) // Walk backwards, we're messing with the list contents
				{
					ElementLink link = links[i];
					ReadOnlyCollection<DomainRoleInfo> domainRoles = link.GetDomainRelationship().DomainRoles;
					int domainRoleCount = domainRoles.Count;
					for (int j = 0; j < domainRoleCount; ++j)
					{
						DomainRoleInfo roleInfo = domainRoles[j];
						if (roleInfo.GetRolePlayer(link) == placeholder)
						{
							roleInfo.SetRolePlayer(link, realElement);
						}
					}
				}
				RemoveDetachedPlaceholder(placeholder);
			}
			private static void RemoveDetachedPlaceholder(ModelElement placeholder)
			{
				// If the placeholder is a dummy link, then make sure that all of its
				// children are removed
				ElementLink linkPlaceholder = placeholder as ElementLink;
				if (linkPlaceholder != null)
				{
					RemoveDetachedLinkPlaceholder(linkPlaceholder);
				}
				else
				{
					placeholder.Delete();
				}
			}
			private static void RemoveDetachedLinkPlaceholder(ElementLink linkPlaceholder)
			{
				DomainRelationshipInfo relationshipInfo = linkPlaceholder.GetDomainRelationship();
				ReadOnlyCollection<DomainRoleInfo> domainRoles = relationshipInfo.DomainRoles;
				int domainRoleCount = domainRoles.Count;
				if (domainRoleCount != 0)
				{
					// Cache the role players up front so we can recursively delete them
					// Note that deleting a role player before deleting the link is likely to
					// delete the link as well because most role players are non-optional.
					// Delete propagation may also apply to the opposite role player, which
					// we don't want in case there are elements attached to that link that
					// do not delete automatically. To handle this case, we cache the role
					// players, delete the link without propagating deletion to the
					// roles, then delete the role players explicitly.
					ModelElement[] rolePlayers = new ModelElement[domainRoleCount];
					for (int i = 0; i < domainRoleCount; ++i)
					{
						rolePlayers[i] = domainRoles[i].GetRolePlayer(linkPlaceholder);
					}

					// Remove the link without removing the role players
					Guid[] domainRoleGuids = new Guid[domainRoles.Count];
					for (int i = 0; i < domainRoleGuids.Length; ++i)
					{
						domainRoleGuids[i] = domainRoles[i].Id;
					}
					linkPlaceholder.Delete(domainRoleGuids);

					// Remove the role players
					for (int i = 0; i < domainRoleCount; ++i)
					{
						Debug.Assert(!rolePlayers[i].IsDeleted);
						RemoveDetachedPlaceholder(rolePlayers[i]);
					}
				}
				else
				{
					linkPlaceholder.Delete();
				}

			}
		}
		private Dictionary<string, Guid> myCustomIdToGuidMap;
		private INotifyElementAdded myNotifyAdded;
		private Dictionary<Guid, PlaceholderElement> myPlaceholderElementMap;
		private Dictionary<string, IORMCustomSerializedDomainModel> myXmlNamespaceToModelMap;

        #region Member Variables
        /// <summary>
        /// Ref Counter for suspending/resuming Modeling Rule Engine
        /// </summary>
        private int myRuleSuspendCount;

        /// <summary>
        /// Current store object. Set in constructor
        /// </summary>
        private readonly Store myStore;
        #endregion // Member Variables
        #region Constructor
        /// <summary>
        /// Create a serializer on the given store
        /// </summary>
        /// <param name="store">Store instance</param>
        public ORMSerializer(Store store)
        {
            myStore = store;
        }
        #endregion // Constructor
        #region Rule Suspension
        /// <summary>
        /// Block rules on store during serialization/deserialization
        /// </summary>
        public bool RulesSuspended
        {
            get
            {
                return myRuleSuspendCount > 0;
            }
            set
            {
                if (value)
                {
                    // Turn on for first set
                    if (1 == ++myRuleSuspendCount)
                    {
                        myStore.RuleManager.SuspendRuleNotification();
                    }
                }
                else
                {
                    // Turn off for balanced call
                    Debug.Assert(myRuleSuspendCount > 0);
                    if (0 == --myRuleSuspendCount)
                    {
                        myStore.RuleManager.ResumeRuleNotification();
                    }
                }
            }
        }
        #endregion // Rule Suspension



		/// <summary>
		/// Load the stream contents into the current store
		/// </summary>
		/// <param name="stream">An initialized stream</param>
		/// <param name="fixupManager">Class used to perfom fixup operations
		/// after the load is complete.</param>
		public void Load(Stream stream, DeserializationFixupManager fixupManager)
		{
			// Leave rules on so all of the links reconnect. Links are not saved.
			RulesSuspended = true;
			try
			{
				myNotifyAdded = fixupManager as INotifyElementAdded;
				XmlReaderSettings settings = new XmlReaderSettings();
				XmlSchemaSet schemas = settings.Schemas;
				Type schemaResourcePathType = GetType();
				schemas.Add(RootXmlNamespace, new XmlTextReader(schemaResourcePathType.Assembly.GetManifestResourceStream(schemaResourcePathType, "ORM2Root.xsd")));

				// Extract namespace and schema information from the different meta models
				ICollection<DomainModel> domainModels = myStore.DomainModels;
				Dictionary<string, IORMCustomSerializedDomainModel> namespaceToModelMap = new Dictionary<string, IORMCustomSerializedDomainModel>();
				foreach (DomainModel domainModel in domainModels)
				{
					IORMCustomSerializedDomainModel customSerializedDomainModel = domainModel as IORMCustomSerializedDomainModel;
					if (customSerializedDomainModel != null)
					{
						string[,] namespaces = customSerializedDomainModel.GetCustomElementNamespaces();
						int namespaceCount = namespaces.GetLength(0);
						for (int i = 0; i < namespaceCount; ++i)
						{
							string namespaceURI = namespaces[i, 1];
							namespaceToModelMap.Add(namespaceURI, customSerializedDomainModel);
							string schemaFile = namespaces[i, 2];
							if (schemaFile != null && schemaFile.Length != 0)
							{
								schemaResourcePathType = domainModel.GetType();
								schemas.Add(namespaceURI, new XmlTextReader(schemaResourcePathType.Assembly.GetManifestResourceStream(schemaResourcePathType, schemaFile)));
							}
						}
					}
				}
				myXmlNamespaceToModelMap = namespaceToModelMap;
				NameTable nameTable = new NameTable();
				settings.NameTable = nameTable;


#if DEBUG
				// Skip validation when the shift key is down in debug mode
				if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == 0)
				{
#endif // DEBUG
					settings.ValidationType = ValidationType.Schema;
#if DEBUG
				}
#endif // DEBUG
				// UNDONE: MSBUG Figure out why this transaction is needed. If it is ommitted then the EdgePointCollection
				// for each of the lines on the diagram is not initialized during Diagram.HandleLineRouting and none of the lines
				// are drawn. This behavior appears to be related to the diagram.GraphWrapper.IsLoading setting, which changes with
				// an extra transaction. However, the interactions between deserialization and diagram initialization are extremely
				// complex, so I'm not sure exactly what is happening here.
				using (Transaction t = myStore.TransactionManager.BeginTransaction())
				{
					using (XmlTextReader xmlReader = new XmlTextReader(new StreamReader(stream), nameTable))
					{
						using (XmlReader reader = XmlReader.Create(xmlReader, settings))
						{
							while (reader.Read())
							{
								if (reader.NodeType == XmlNodeType.Element)
								{
									if (!reader.IsEmptyElement && reader.NamespaceURI == RootXmlNamespace && reader.LocalName == RootXmlElementName)
									{
										while (reader.Read())
										{
											XmlNodeType nodeType = reader.NodeType;
											if (nodeType == XmlNodeType.Element)
											{
												bool processedRootElement = false;
												IORMCustomSerializedDomainModel metaModel;
												if (namespaceToModelMap.TryGetValue(reader.NamespaceURI, out metaModel))
												{
													Guid classGuid = metaModel.MapRootElement(reader.NamespaceURI, reader.LocalName);
													if (!classGuid.Equals(Guid.Empty))
													{
														processedRootElement = true;
														ProcessClassElement(reader, metaModel, CreateElement(reader.GetAttribute("id"), null, classGuid), null);
													}
												}
												if (!processedRootElement)
												{
													PassEndElement(reader);
												}
											}
											else if (nodeType == XmlNodeType.EndElement)
											{
												break;
											}
										}
									}
								}
							}
						}
					}
					if (fixupManager != null)
					{
						fixupManager.DeserializationComplete();
					}
					t.Commit();
				}
			}
			finally
			{
				RulesSuspended = false;
			}
		}
		private delegate ElementLink CreateAggregatingLink(string idValue);
		/// <summary>
		/// Process a newly created element. The element will have an
		/// Id set only. The id and ref properties should be ignored.
		/// </summary>
		/// <param name="reader">Reader set to the root node</param>
		/// <param name="customModel">The custom serialized meta model</param>
		/// <param name="element">Newly created element</param>
		/// <param name="createAggregatingLinkCallback">A callback to pre-create the aggregating link before the aggregated element has finished processing</param>
		private void ProcessClassElement(XmlReader reader, IORMCustomSerializedDomainModel customModel, ModelElement element, CreateAggregatingLink createAggregatingLinkCallback)
		{
			IORMCustomSerializedElement customElement = element as IORMCustomSerializedElement;
			DomainDataDirectory dataDir = myStore.DomainDataDirectory;
			#region Property processing
			// Process all properties first
			if (reader.MoveToFirstAttribute())
			{
				do
				{
					string attributeName = reader.LocalName;
					string namespaceName = reader.NamespaceURI;
					//derived objects are prefixed by an underscore and do not need to be read in
					if (!(namespaceName.Length == 0 && (attributeName == "id" || attributeName == "ref" || attributeName[0] == '_')))
					{
						Guid attributeGuid = new Guid();
						DomainPropertyInfo attributeInfo = null;
						if (customElement != null)
						{
							attributeGuid = customElement.MapAttribute(namespaceName, attributeName);
							if (!attributeGuid.Equals(Guid.Empty))
							{
								attributeInfo = dataDir.FindDomainProperty(attributeGuid);
							}
						}
						if (attributeInfo == null && namespaceName.Length == 0)
						{
							attributeInfo = element.GetDomainClass().FindDomainProperty(attributeName, true);
						}
						if (attributeInfo != null)
						{
							SetPropertyValue(element, attributeInfo, reader.Value);
						}
					}
				} while (reader.MoveToNextAttribute());
			}
			reader.MoveToElement();
			#endregion // Property processing
			ProcessChildElements(reader, customModel, element, customElement, createAggregatingLinkCallback);
		}
		private static Guid GetDomainObjectIdFromTypeName(string typeName)
		{
			Type type = Type.GetType(typeName, false, false);
			if (type != null)
			{
				object[] domainObjectIdAttributeArray = type.GetCustomAttributes(typeof(DomainObjectIdAttribute), false);
				if (domainObjectIdAttributeArray.Length > 0)
				{
					return ((DomainObjectIdAttribute)domainObjectIdAttributeArray[0]).Id;
				}
			}
			return Guid.Empty;
		}
		private void ProcessChildElements(XmlReader reader, IORMCustomSerializedDomainModel customModel, ModelElement element, IORMCustomSerializedElement customElement, CreateAggregatingLink createAggregatingLinkCallback)
		{
			if (reader.IsEmptyElement)
			{
				return;
			}
			DomainDataDirectory dataDir = myStore.DomainDataDirectory;
			string elementName;
			string namespaceName;
			string containerName = null;
			string containerNamespace = null;
			bool testForAggregatingLink = createAggregatingLinkCallback != null && customElement != null && 0 != (customElement.SupportedCustomSerializedOperations & ORMCustomSerializedElementSupportedOperations.EmbeddingLinkInfo);
			IORMCustomSerializedDomainModel containerRestoreCustomModel = null;
			DomainRoleInfo containerOppositeDomainRole = null;
			while (reader.Read())
			{
				XmlNodeType outerNodeType = reader.NodeType;
				if (outerNodeType == XmlNodeType.Element)
				{
					elementName = reader.LocalName;
					namespaceName = reader.NamespaceURI;
					string idValue = reader.GetAttribute("id");
					string refValue = reader.GetAttribute("ref");
					bool aggregatedClass = (object)idValue != null && (object)refValue == null;
					DomainRoleInfo oppositeDomainRole = null;
					DomainClassInfo oppositeDomainClass = null;
					DomainRelationshipInfo explicitRelationshipType = null;
					bool oppositeDomainClassFullyDeterministic = false;
					bool resolveOppositeDomainClass = false;
					IList<Guid> oppositeDomainRoleIds = null;
					IORMCustomSerializedDomainModel restoreCustomModel = null;
					bool nodeProcessed = false;
					ORMCustomSerializedElementMatch aggregatingLinkMatch;
					DomainRoleInfo testAggregatingRole;
					if (aggregatedClass &&
						testForAggregatingLink &&
						(aggregatingLinkMatch = customElement.MapChildElement(namespaceName, elementName, containerNamespace, containerName)).MatchStyle == ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRole &&
						null != (testAggregatingRole = dataDir.FindDomainRole(aggregatingLinkMatch.SingleOppositeDomainRoleId)) &&
						testAggregatingRole.IsEmbedding)
					{
						testForAggregatingLink = false;
						ElementLink aggregatingLink = createAggregatingLinkCallback(idValue);
						if (aggregatingLink != null)
						{
							ProcessClassElement(reader, customModel, aggregatingLink, null);
						}
						nodeProcessed = true;
					}
					else if (aggregatedClass && containerName == null)
					{
						// All we have is the class name, go look for an appropriate aggregate
						if (customModel != null)
						{
							Guid domainClassId = customModel.MapClassName(namespaceName, elementName);
							if (!domainClassId.Equals(Guid.Empty))
							{
								oppositeDomainClass = dataDir.FindDomainClass(domainClassId);
							}
						}
						if (oppositeDomainClass == null)
						{
							Type namespaceType = (customModel != null) ? customModel.GetType() : element.GetType();
							oppositeDomainClass = dataDir.FindDomainClass(string.Concat(namespaceType.Namespace, ".", elementName));
						}
						if (oppositeDomainClass != null)
						{
							// Find the aggregating role that maps to this class
							ReadOnlyCollection<DomainRoleInfo> aggregatingRoles = element.GetDomainClass().AllDomainRolesPlayed;
							int rolesCount = aggregatingRoles.Count;
							for (int i = 0; i < rolesCount; ++i)
							{
								DomainRoleInfo aggregatingRole = aggregatingRoles[i];
								if (aggregatingRole.IsEmbedding)
								{
									DomainRoleInfo testRole = aggregatingRole.OppositeDomainRole;
									if (testRole.RolePlayer == oppositeDomainClass)
									{
										oppositeDomainRole = testRole;
										break;
									}
								}
							}
						}
					}
					else
					{
						bool handledByCustomElement = false;
						if (customElement != null)
						{
							handledByCustomElement = true;
							ORMCustomSerializedElementMatch match = default(ORMCustomSerializedElementMatch);
							if (aggregatedClass)
							{
								// The meta role information should be available from the container name only
								match = customElement.MapChildElement(null, null, containerNamespace, containerName);
							}
							else
							{
								match = customElement.MapChildElement(namespaceName, elementName, containerNamespace, containerName);
							}
							switch (match.MatchStyle)
							{
								case ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRole:
									oppositeDomainRole = dataDir.FindDomainRole(match.SingleOppositeDomainRoleId);
									resolveOppositeDomainClass = true;
									break;
								case ORMCustomSerializedElementMatchStyle.SingleOppositeDomainRoleExplicitRelationshipType:
									explicitRelationshipType = dataDir.FindDomainRelationship(match.ExplicitRelationshipGuid);
									oppositeDomainRole = dataDir.FindDomainRole(match.SingleOppositeDomainRoleId);
									resolveOppositeDomainClass = true;
									break;
								case ORMCustomSerializedElementMatchStyle.MultipleOppositeDomainRoles:
									oppositeDomainRoleIds = match.OppositeDomainRoleIdCollection;
									break;
								case ORMCustomSerializedElementMatchStyle.MultipleOppositeMetaRolesExplicitRelationshipType:
									explicitRelationshipType = dataDir.FindDomainRelationship(match.ExplicitRelationshipGuid);
									oppositeDomainRoleIds = match.OppositeDomainRoleIdCollection;
									break;
								case ORMCustomSerializedElementMatchStyle.Property:
									{
										DomainPropertyInfo attributeInfo = dataDir.FindDomainProperty(match.DomainPropertyId);
										if (match.DoubleTagName == null)
										{
											// Reader the value off directly
											SetPropertyValue(element, attributeInfo, reader.ReadString());
											nodeProcessed = true;
										}
										else
										{
											// Look for the inner tag
											string matchName = match.DoubleTagName;
											while (reader.Read())
											{
												XmlNodeType innerType = reader.NodeType;
												if (innerType == XmlNodeType.Element)
												{
													if (reader.LocalName == matchName && reader.NamespaceURI == namespaceName)
													{
														SetPropertyValue(element, attributeInfo, reader.ReadString());
														nodeProcessed = true;
													}
													else
													{
														PassEndElement(reader);
													}
												}
												else if (innerType == XmlNodeType.EndElement)
												{
													break;
												}
											}
										}
										break;
									}
								case ORMCustomSerializedElementMatchStyle.None:
									handledByCustomElement = false;
									break;
							}
						}
						if (!handledByCustomElement)
						{
							if (aggregatedClass)
							{
								if (containerOppositeDomainRole != null)
								{
									oppositeDomainRole = containerOppositeDomainRole;
									resolveOppositeDomainClass = true;
								}
							}
							else if (refValue != null)
							{
								IORMCustomSerializedDomainModel childModel;
								if (elementName.IndexOf('.') > 0 && myXmlNamespaceToModelMap.TryGetValue(namespaceName, out childModel))
								{
									if (childModel != customModel && customModel != null)
									{
										restoreCustomModel = customModel;
										customModel = childModel;
									}
									DomainRoleInfo domainRole = dataDir.FindDomainRole(GetDomainObjectIdFromTypeName(childModel.GetType().Namespace + "." + elementName));
									// Fallback on the two standard meta models
									if (domainRole == null)
									{
										domainRole = dataDir.FindDomainRole(GetDomainObjectIdFromTypeName(typeof(ModelElement).Namespace + "." + elementName));
										if (domainRole == null)
										{
											domainRole = dataDir.FindDomainRole(GetDomainObjectIdFromTypeName(typeof(Diagram).Namespace + "." + elementName));
										}
									}
									if (domainRole != null)
									{
										oppositeDomainRole = domainRole;
										resolveOppositeDomainClass = true;
									}
								}
							}
							else if (containerName == null)
							{
								// The tag name should have the format Rel.Role. Assume the relationship
								// is in the same namespace as the model associated with the xml namespace.
								// Models can nest elements inside base models, so we can't assume the node
								// is in the same code namespace as the parent. Also, if the implementation
								// class of the parent element has been upgraded (with DomainClassInfo.UpgradeImplementationClass)
								// then the ImplemtationClass of the parent node will be in the wrong namespace.
								// The model elements themselves are more stable, use them.
								IORMCustomSerializedDomainModel childModel;
								if (elementName.IndexOf('.') > 0 && myXmlNamespaceToModelMap.TryGetValue(namespaceName, out childModel))
								{
									DomainRoleInfo metaRole = dataDir.FindDomainRole(GetDomainObjectIdFromTypeName(childModel.GetType().Namespace + "." + elementName));
									// Fallback on the two standard meta models
									if (metaRole == null)
									{
										metaRole = dataDir.FindDomainRole(GetDomainObjectIdFromTypeName(typeof(ModelElement).Namespace + "." + elementName));
										if (metaRole == null)
										{
											metaRole = dataDir.FindDomainRole(GetDomainObjectIdFromTypeName(typeof(Diagram).Namespace + "." + elementName));
										}
									}
									if (metaRole != null)
									{
										containerOppositeDomainRole = metaRole;
										containerRestoreCustomModel = customModel;
										customModel = childModel;
									}
								}
								// If this is an unrecognized node without an id or ref then push
								// the container node (we only allow container depth of 1)
								// and continue to loop.
								if (!reader.IsEmptyElement)
								{
									containerName = elementName;
									containerNamespace = namespaceName;
								}
								nodeProcessed = true;
							}
						}
					}
					if (!nodeProcessed)
					{
						if (oppositeDomainRole != null)
						{
							if (resolveOppositeDomainClass)
							{
								oppositeDomainClass = oppositeDomainRole.RolePlayer;
								// If the opposite role player does not have any derived class in
								// the model then we know what type of element to create. Otherwise,
								// we need to create the element as a pending element if it doesn't exist
								// already.
								oppositeDomainClassFullyDeterministic = oppositeDomainClass.AllDescendants.Count == 0;
								if (aggregatedClass && !oppositeDomainClassFullyDeterministic)
								{
									DomainClassInfo testMetaClass = null;
									IORMCustomSerializedDomainModel elementModel = myXmlNamespaceToModelMap[namespaceName];
									if (elementModel == null)
									{
										elementModel = customModel;
									}
									if (elementModel != null)
									{
										Guid mappedGuid = elementModel.MapClassName(namespaceName, elementName);
										if (!mappedGuid.Equals(Guid.Empty))
										{
											testMetaClass = dataDir.FindDomainClass(mappedGuid);
										}
									}
									if (testMetaClass == null)
									{
										Type namespaceType = (elementModel != null) ? elementModel.GetType() : element.GetType();
										testMetaClass = dataDir.FindDomainClass(string.Concat(namespaceType.Namespace, ".", elementName));
									}
									oppositeDomainClass = testMetaClass;
									oppositeDomainClassFullyDeterministic = true;
								}
							}
						}
						else if (oppositeDomainRoleIds != null)
						{
							// In this case we have multiple opposite meta role guids, so we
							// always have to rely on the aggregated element to find the data
							Debug.Assert(aggregatedClass);
							if (customModel != null)
							{
								Guid mappedGuid = customModel.MapClassName(namespaceName, elementName);
								if (!mappedGuid.Equals(Guid.Empty))
								{
									oppositeDomainClass = dataDir.FindDomainClass(mappedGuid);
								}
							}
							if (oppositeDomainClass == null)
							{
								Type namespaceType = (customModel != null) ? customModel.GetType() : element.GetType();
								oppositeDomainClass = dataDir.FindDomainClass(string.Concat(namespaceType.Namespace, ".", elementName));
							}
							if (oppositeDomainClass != null)
							{
								oppositeDomainClassFullyDeterministic = true;
								int roleGuidCount = oppositeDomainRoleIds.Count;
								for (int i = 0; i < roleGuidCount; ++i)
								{
									DomainRoleInfo testRoleInfo = dataDir.FindDomainRole(oppositeDomainRoleIds[i]);
									if (oppositeDomainClass.IsDerivedFrom(testRoleInfo.RolePlayer.Id))
									{
										oppositeDomainRole = testRoleInfo;
#if DEBUG
										for (int j = i + 1; j < roleGuidCount; ++j)
										{
											testRoleInfo = dataDir.FindDomainRole(oppositeDomainRoleIds[j]);
											Debug.Assert(testRoleInfo == null || !oppositeDomainClass.IsDerivedFrom(testRoleInfo.RolePlayer.Id), "Custom serialization data does not provide a unique deserialization map for a combined element.");
										}
#endif // DEBUG
										break;
									}
								}
							}
						}
						if (oppositeDomainClass != null)
						{
							Debug.Assert(oppositeDomainRole != null);
							// Create a new element and make sure the relationship
							// to this element does not already exist. This obviously requires one
							// relationship of each type between any two objects, which is a reasonable assumption
							// for a well-formed model.
							bool isNewElement;
							string elementId = aggregatedClass ? idValue : refValue;
							ModelElement oppositeElement = CreateElement(elementId, oppositeDomainClass, Guid.Empty, !oppositeDomainClassFullyDeterministic, out isNewElement);
							bool createLink = true;
							if (aggregatedClass)
							{
								ProcessClassElement(
									reader,
									customModel,
									oppositeElement,
									delegate(string aggregateIdValue)
									{
										ElementLink retVal = null;
										if (createLink)
										{
											createLink = false;
											retVal = CreateElementLink(aggregateIdValue, element, oppositeElement, oppositeDomainRole, explicitRelationshipType);
										}
										return retVal;
									});
							}
							if (!isNewElement)
							{
								LinkedElementCollection<ModelElement> oppositeRolePlayers = oppositeDomainRole.GetLinkedElements(oppositeElement);
								int oppositeCount = oppositeRolePlayers.Count;
								for (int i = 0; i < oppositeCount; ++i)
								{
									if (element == oppositeRolePlayers[i])
									{
										createLink = false;
										break;
									}
								}
							}
							if (createLink)
							{
								ElementLink newLink = CreateElementLink(aggregatedClass ? null : idValue, element, oppositeElement, oppositeDomainRole, explicitRelationshipType);
								if (!aggregatedClass && idValue != null)
								{
									ProcessClassElement(reader, customModel, newLink, null);
								}
							}
						}
						else
						{
							PassEndElement(reader);
						}
					}
					if (restoreCustomModel != null)
					{
						customModel = restoreCustomModel;
					}
				}
				else if (outerNodeType == XmlNodeType.EndElement)
				{
					if (containerName != null)
					{
						// Pop the container node
						containerName = null;
						containerNamespace = null;
						containerOppositeDomainRole = null;
						if (containerRestoreCustomModel != null)
						{
							customModel = containerRestoreCustomModel;
							containerRestoreCustomModel = null;
						}
					}
					else
					{
						break;
					}
				}
			}
		}
		/// <summary>
		/// Set the value of the specified property on the model element
		/// </summary>
		/// <param name="element">The element to modify</param>
		/// <param name="domainPropertyInfo">The meta property to set</param>
		/// <param name="stringValue">The new value of the property</param>
		private static void SetPropertyValue(ModelElement element, DomainPropertyInfo domainPropertyInfo, string stringValue)
		{
			PropertyInfo propertyInfo = domainPropertyInfo.PropertyInfo;
			Type propertyType = domainPropertyInfo.PropertyType;
			object objectValue = null;
			if (!propertyType.IsEnum)
			{
				switch (Type.GetTypeCode(propertyType))
				{
					case TypeCode.DateTime:
						objectValue = XmlConvert.ToDateTime(stringValue, XmlDateTimeSerializationMode.Utc);
						break;
					case TypeCode.UInt64:
						objectValue = XmlConvert.ToUInt64(stringValue);
						break;
					case TypeCode.Int64:
						objectValue = XmlConvert.ToInt64(stringValue);
						break;
					case TypeCode.UInt32:
						objectValue = XmlConvert.ToUInt32(stringValue);
						break;
					case TypeCode.Int32:
						objectValue = XmlConvert.ToInt32(stringValue);
						break;
					case TypeCode.UInt16:
						objectValue = XmlConvert.ToUInt16(stringValue);
						break;
					case TypeCode.Int16:
						objectValue = XmlConvert.ToInt16(stringValue);
						break;
					case TypeCode.Byte:
						objectValue = XmlConvert.ToByte(stringValue);
						break;
					case TypeCode.SByte:
						objectValue = XmlConvert.ToSByte(stringValue);
						break;
					case TypeCode.Char:
						objectValue = XmlConvert.ToChar(stringValue);
						break;
					case TypeCode.Boolean:
						objectValue = XmlConvert.ToBoolean(stringValue);
						break;
					case TypeCode.Decimal:
						objectValue = XmlConvert.ToDecimal(stringValue);
						break;
					case TypeCode.Double:
						objectValue = XmlConvert.ToDouble(stringValue);
						break;
					case TypeCode.Single:
						objectValue = XmlConvert.ToSingle(stringValue);
						break;
					case TypeCode.String:
						objectValue = stringValue;
						break;
					case TypeCode.Object:
						{
							if (propertyType == typeof(Guid))
							{
								objectValue = XmlConvert.ToGuid(stringValue);
							}
							else if (propertyType == typeof(TimeSpan))
							{
								objectValue = XmlConvert.ToTimeSpan(stringValue);
							}
							break;
						}
				}
			}
			if (objectValue == null)
			{
				Design.ORMTypeDescriptor.TypeDescriptorContext context = Design.ORMTypeDescriptor.CreateTypeDescriptorContext(element, domainPropertyInfo);
				objectValue = context.PropertyDescriptor.Converter.ConvertFromInvariantString(context, stringValue);
			}
			if (objectValue != null)
			{
				domainPropertyInfo.SetValue(element, objectValue);
			}
		}
		/// <summary>
		/// Create an element link after verifying that the link needs to be created
		/// </summary>
		/// <param name="idValue">The value of the id for the link, or null</param>
		/// <param name="rolePlayer">The near role player</param>
		/// <param name="oppositeRolePlayer">The opposite role player</param>
		/// <param name="oppositeMetaRoleInfo">The opposite meta role</param>
		/// <param name="explicitMetaRelationshipInfo">The relationship type to create.
		/// Derived from oppositeMetaRoleInfo if not specified.</param>
		/// <returns>The newly created element link</returns>
		private ElementLink CreateElementLink(string idValue, ModelElement rolePlayer, ModelElement oppositeRolePlayer, DomainRoleInfo oppositeMetaRoleInfo, DomainRelationshipInfo explicitMetaRelationshipInfo)
		{
			// Create an element link. There is no attempt here to determine if the link already
			// exists in the store;
			Guid id = (idValue == null) ? Guid.NewGuid() : GetElementId(idValue);

			// An element link is always created as itself or as a placeholder, so the guid
			// should never be in use. Placeholders are created if a forward reference is made
			// to a class that is a link.
			Debug.Assert(null == myStore.ElementDirectory.FindElement(id));
			PlaceholderElement placeholder = default(PlaceholderElement);
			Dictionary<Guid, PlaceholderElement> placeholderMap = null;
			bool existingPlaceholder = false;
			if (idValue != null)
			{
				// See if this has been created before as a placeholder
				placeholderMap = myPlaceholderElementMap;
				if (placeholderMap != null)
				{
					existingPlaceholder = placeholderMap.TryGetValue(id, out placeholder);
				}
			}
			ElementLink retVal = myStore.ElementFactory.CreateElementLink(
				explicitMetaRelationshipInfo ?? oppositeMetaRoleInfo.DomainRelationship,
				new PropertyAssignment[]{new PropertyAssignment(ElementFactory.IdPropertyAssignment, id)},
				new RoleAssignment(oppositeMetaRoleInfo.OppositeDomainRole.Id, rolePlayer),
				new RoleAssignment(oppositeMetaRoleInfo.Id, oppositeRolePlayer));
			if (myNotifyAdded != null)
			{
				myNotifyAdded.ElementAdded(retVal);
			}
			if (existingPlaceholder)
			{
				placeholder.FulfilPlaceholderRoles(retVal);
				placeholderMap.Remove(id);
			}
			return retVal;
		}
		/// <summary>
		/// Create a class element with the id specified in the reader
		/// </summary>
		/// <param name="idValue">The id for this element in the xml file</param>
		/// <param name="domainClassInfo">The meta class info of the element to create. If null,
		/// the domainClassId is used to find the class info</param>
		/// <param name="domainClassId">The identifier for the class</param>
		/// <returns>A new ModelElement</returns>
		private ModelElement CreateElement(string idValue, DomainClassInfo domainClassInfo, Guid domainClassId)
		{
			bool isNewElement;
			return CreateElement(idValue, domainClassInfo, domainClassId, false, out isNewElement);
		}
		/// <summary>
		/// Create a class element with the id specified in the reader
		/// </summary>
		/// <param name="idValue">The id for this element in the xml file</param>
		/// <param name="domainClassInfo">The meta class info of the element to create. If null,
		/// the domainClassId is used to find the class info</param>
		/// <param name="domainClassId">The identifier for the class</param>
		/// <param name="createAsPlaceholder">The provided meta class information is not unique.
		/// If this element is not already created then add it with a separate tracked id so it can
		/// be replaced later by the fully resolved type. All role players will be automatically
		/// moved from the pending placeholder when the real element is created</param>
		/// <param name="isNewElement">true if the element is actually created, as opposed
		/// to being identified as an existing element</param>
		/// <returns>A new ModelElement</returns>
		private ModelElement CreateElement(string idValue, DomainClassInfo domainClassInfo, Guid domainClassId, bool createAsPlaceholder, out bool isNewElement)
		{
			isNewElement = false;

			// Get a valid guid identifier for the element
			Guid id = GetElementId(idValue);

			// See if we've already created this element as the opposite role player in a link
			ModelElement retVal = myStore.ElementDirectory.FindElement(id);
			if (retVal == null)
			{
				PlaceholderElement placeholder = default(PlaceholderElement);
				Dictionary<Guid, PlaceholderElement> placeholderMap = myPlaceholderElementMap;
				bool existingPlaceholder = placeholderMap != null && placeholderMap.TryGetValue(id, out placeholder);
				// The false parameter indicates that OnInitialize should not be called, which
				// is standard fare for deserialization routines.
				if (domainClassInfo == null)
				{
					domainClassInfo = myStore.DomainDataDirectory.GetDomainClass(domainClassId);
				}
				Type implClass = domainClassInfo.ImplementationClass;
				// Any request to create a DomainRelationshipInfo as an element instead of
				// an element link means a forward reference to a link object. Always create
				// this as a placeholder, given that we will eventually realize this as
				// a real link.
				if (createAsPlaceholder || implClass.IsAbstract || domainClassInfo is DomainRelationshipInfo)
				{
					if (placeholderMap == null)
					{
						myPlaceholderElementMap = placeholderMap = new Dictionary<Guid, PlaceholderElement>();
					}
					retVal = placeholder.CreatePlaceholderElement(myStore, domainClassInfo);
					placeholderMap[id] = placeholder;
				}
				else
				{
					retVal = myStore.ElementFactory.CreateElement(domainClassInfo, new PropertyAssignment(ElementFactory.IdPropertyAssignment, id));
					isNewElement = true;
					if (myNotifyAdded != null)
					{
						myNotifyAdded.ElementAdded(retVal);
					}
					if (existingPlaceholder)
					{
						placeholder.FulfilPlaceholderRoles(retVal);
						placeholderMap.Remove(id);
					}
				}
			}
			return retVal;
		}
		/// <summary>
		/// Parse or generate a guid from the passed in identifier
		/// </summary>
		/// <param name="id">A string taken from an id or ref tag in the xml file. If the
		/// value cannot be interpreted as a guid then generated a new guid and keep a map
		/// from this name to the generated guid</param>
		/// <returns>A non-empty Guid</returns>
		private Guid GetElementId(string id)
		{
			Guid retVal = new Guid();
			bool haveGuid = false;
			try
			{
				if (id[0] == '_')
				{
					retVal = new Guid(id.Substring(1));
					haveGuid = true;
				}
			}
			catch (FormatException)
			{
				// Swallow
			}
			if (!haveGuid)
			{
				if (myCustomIdToGuidMap == null)
				{
					myCustomIdToGuidMap = new Dictionary<string, Guid>(StringComparer.Ordinal);
				}
				else
				{
					haveGuid = myCustomIdToGuidMap.TryGetValue(id, out retVal);
				}
				if (!haveGuid)
				{
					retVal = Guid.NewGuid();
					myCustomIdToGuidMap[id] = retVal;
				}
			}
			return retVal;
		}
		/// <summary>
		/// Move the reader to the node immediately after the end element corresponding to the current open element
		/// </summary>
		/// <param name="reader">The XmlReader to advance</param>
		private static void PassEndElement(XmlReader reader)
		{
			if (!reader.IsEmptyElement)
			{
				bool finished = false;
				while (!finished && reader.Read())
				{
					switch (reader.NodeType)
					{
						case XmlNodeType.Element:
							PassEndElement(reader);
							break;

						case XmlNodeType.EndElement:
							finished = true;
							break;
					}
				}
			}
		}
	}
	#endregion // New Deserialization
}
