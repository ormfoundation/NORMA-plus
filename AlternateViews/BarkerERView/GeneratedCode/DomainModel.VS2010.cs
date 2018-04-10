﻿#region Common Public License Copyright Notice
/**************************************************************************\
* Natural Object-Role Modeling Architect for Visual Studio                 *
*                                                                          *
* Copyright © Neumont University and The ORM Foundation. All rights reserved.                     *
* Copyright © ORM Solutions, LLC. All rights reserved.                     *
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
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using DslModeling = global::Microsoft.VisualStudio.Modeling;
using DslDesign = global::Microsoft.VisualStudio.Modeling.Design;
using DslDiagrams = global::Microsoft.VisualStudio.Modeling.Diagrams;
namespace ORMSolutions.ORMArchitect.Views.BarkerERView
{
	/// <summary>
	/// DomainModel BarkerERShapeDomainModel
	/// (Preliminary) Graphical View of Barker ER Model
	/// </summary>
	[ORMSolutions.ORMArchitect.Core.Load.NORMAExtensionLoadKey("i4ZTtbpPyWFsaqPS3D+eD1ZioxOaKSSyJzcfRhC/HeZ/MeJOFcJNuJrnq9Yz1W6GNaNRXHdcn0z+qZqU+6gaCw==")]
	[DslDesign::DisplayNameResource("ORMSolutions.ORMArchitect.Views.BarkerERView.BarkerERShapeDomainModel.DisplayName", typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.BarkerERShapeDomainModel), "ORMSolutions.ORMArchitect.Views.BarkerERView.GeneratedCode.DomainModelResx")]
	[DslDesign::DescriptionResource("ORMSolutions.ORMArchitect.Views.BarkerERView.BarkerERShapeDomainModel.Description", typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.BarkerERShapeDomainModel), "ORMSolutions.ORMArchitect.Views.BarkerERView.GeneratedCode.DomainModelResx")]
	[DslModeling::DependsOnDomainModel(typeof(global::Microsoft.VisualStudio.Modeling.CoreDomainModel))]
	[DslModeling::DependsOnDomainModel(typeof(global::Microsoft.VisualStudio.Modeling.Diagrams.CoreDesignSurfaceDomainModel))]
	[DslModeling::DependsOnDomainModel(typeof(global::ORMSolutions.ORMArchitect.EntityRelationshipModels.Barker.BarkerDomainModel))]
	[DslModeling::DependsOnDomainModel(typeof(global::ORMSolutions.ORMArchitect.ORMAbstractionToBarkerERBridge.ORMAbstractionToBarkerERBridgeDomainModel))]
	[DslModeling::DependsOnDomainModel(typeof(global::ORMSolutions.ORMArchitect.Core.ShapeModel.ORMShapeDomainModel))]
	[DslModeling::DependsOnDomainModel(typeof(global::ORMSolutions.ORMArchitect.Core.ObjectModel.ORMCoreDomainModel))]
	[DslModeling::DependsOnDomainModel(typeof(global::ORMSolutions.ORMArchitect.ORMToORMAbstractionBridge.ORMToORMAbstractionBridgeDomainModel))]
	[DslModeling::DependsOnDomainModel(typeof(global::ORMSolutions.ORMArchitect.ORMAbstraction.AbstractionDomainModel))]
	[DslModeling::DependsOnDomainModel(typeof(global::ORMSolutions.ORMArchitect.Framework.FrameworkDomainModel))]
	[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Generated code.")]
	[DslModeling::DomainObjectId("2e6c7361-69ae-411a-bb82-09f844e6ba95")]
	internal partial class BarkerERShapeDomainModel : DslModeling::DomainModel
	{
		#region Constructor, domain model Id
	
		/// <summary>
		/// BarkerERShapeDomainModel domain model Id.
		/// </summary>
		public static readonly global::System.Guid DomainModelId = new global::System.Guid(0x2e6c7361, 0x69ae, 0x411a, 0xbb, 0x82, 0x09, 0xf8, 0x44, 0xe6, 0xba, 0x95);
	
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="store">Store containing the domain model.</param>
		public BarkerERShapeDomainModel(DslModeling::Store store)
			: base(store, DomainModelId)
		{
			// Call the partial method to allow any required serialization setup to be done.
			this.InitializeSerialization(store);		
		}
		
	
		///<Summary>
		/// Defines a partial method that will be called from the constructor to
		/// allow any necessary serialization setup to be done.
		///</Summary>
		///<remarks>
		/// For a DSL created with the DSL Designer wizard, an implementation of this 
		/// method will be generated in the GeneratedCode\SerializationHelper.cs class.
		///</remarks>
		partial void InitializeSerialization(DslModeling::Store store);
	
	
		#endregion
		#region Domain model reflection
			
		/// <summary>
		/// Gets the list of generated domain model types (classes, rules, relationships).
		/// </summary>
		/// <returns>List of types.</returns>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Generated code.")]	
		protected sealed override global::System.Type[] GetGeneratedDomainModelTypes()
		{
			return new global::System.Type[]
			{
				typeof(BarkerERDiagram),
				typeof(AssociationConnector),
				typeof(BarkerEntityShape),
				typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.FixUpDiagram),
				typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemAddRule),
				typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemDeleteRule),
				typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemRolePlayerChangeRule),
				typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemRolePlayerPositionChangeRule),
				typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemChangeRule),
			};
		}
		/// <summary>
		/// Gets the list of generated domain properties.
		/// </summary>
		/// <returns>List of property data.</returns>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Generated code.")]	
		protected sealed override DomainMemberInfo[] GetGeneratedDomainProperties()
		{
			return new DomainMemberInfo[]
			{
				new DomainMemberInfo(typeof(BarkerEntityShape), "UpdateCounter", BarkerEntityShape.UpdateCounterDomainPropertyId, typeof(BarkerEntityShape.UpdateCounterPropertyHandler)),
			};
		}
		#endregion
		#region Factory methods
		private static global::System.Collections.Generic.Dictionary<global::System.Type, int> createElementMap;
	
		/// <summary>
		/// Creates an element of specified type.
		/// </summary>
		/// <param name="partition">Partition where element is to be created.</param>
		/// <param name="elementType">Element type which belongs to this domain model.</param>
		/// <param name="propertyAssignments">New element property assignments.</param>
		/// <returns>Created element.</returns>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Generated code.")]	
		public sealed override DslModeling::ModelElement CreateElement(DslModeling::Partition partition, global::System.Type elementType, DslModeling::PropertyAssignment[] propertyAssignments)
		{
			if (elementType == null) throw new global::System.ArgumentNullException("elementType");
	
			if (createElementMap == null)
			{
				createElementMap = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(3);
				createElementMap.Add(typeof(BarkerERDiagram), 0);
				createElementMap.Add(typeof(AssociationConnector), 1);
				createElementMap.Add(typeof(BarkerEntityShape), 2);
			}
			int index;
			if (!createElementMap.TryGetValue(elementType, out index))
			{
				// construct exception error message		
				string exceptionError = string.Format(
								global::System.Globalization.CultureInfo.CurrentCulture,
								global::ORMSolutions.ORMArchitect.Views.BarkerERView.BarkerERShapeDomainModel.SingletonResourceManager.GetString("UnrecognizedElementType"),
								elementType.Name);
				throw new global::System.ArgumentException(exceptionError, "elementType");
			}
			switch (index)
			{
				// A constructor was not generated for BarkerERDiagram because it had HasCustomConstructor
				// set to true. Please provide the constructor below.
				case 0: return new BarkerERDiagram(partition, propertyAssignments);
				case 1: return new AssociationConnector(partition, propertyAssignments);
				case 2: return new BarkerEntityShape(partition, propertyAssignments);
				default: return null;
			}
		}
	
		private static global::System.Collections.Generic.Dictionary<global::System.Type, int> createElementLinkMap;
	
		/// <summary>
		/// Creates an element link of specified type.
		/// </summary>
		/// <param name="partition">Partition where element is to be created.</param>
		/// <param name="elementLinkType">Element link type which belongs to this domain model.</param>
		/// <param name="roleAssignments">List of relationship role assignments for the new link.</param>
		/// <param name="propertyAssignments">New element property assignments.</param>
		/// <returns>Created element link.</returns>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public sealed override DslModeling::ElementLink CreateElementLink(DslModeling::Partition partition, global::System.Type elementLinkType, DslModeling::RoleAssignment[] roleAssignments, DslModeling::PropertyAssignment[] propertyAssignments)
		{
			if (elementLinkType == null) throw new global::System.ArgumentNullException("elementLinkType");
			if (roleAssignments == null) throw new global::System.ArgumentNullException("roleAssignments");
	
			if (createElementLinkMap == null)
			{
				createElementLinkMap = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(0);
			}
			int index;
			if (!createElementLinkMap.TryGetValue(elementLinkType, out index))
			{
				// construct exception error message
				string exceptionError = string.Format(
								global::System.Globalization.CultureInfo.CurrentCulture,
								global::ORMSolutions.ORMArchitect.Views.BarkerERView.BarkerERShapeDomainModel.SingletonResourceManager.GetString("UnrecognizedElementLinkType"),
								elementLinkType.Name);
				throw new global::System.ArgumentException(exceptionError, "elementLinkType");
			
			}
			switch (index)
			{
				default: return null;
			}
		}
		#endregion
		#region Resource manager
		
		private static global::System.Resources.ResourceManager resourceManager;
		
		/// <summary>
		/// The base name of this model's resources.
		/// </summary>
		public const string ResourceBaseName = "ORMSolutions.ORMArchitect.Views.BarkerERView.GeneratedCode.DomainModelResx";
		
		/// <summary>
		/// Gets the DomainModel's ResourceManager. If the ResourceManager does not already exist, then it is created.
		/// </summary>
		public override global::System.Resources.ResourceManager ResourceManager
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return BarkerERShapeDomainModel.SingletonResourceManager;
			}
		}
	
		/// <summary>
		/// Gets the Singleton ResourceManager for this domain model.
		/// </summary>
		public static global::System.Resources.ResourceManager SingletonResourceManager
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (BarkerERShapeDomainModel.resourceManager == null)
				{
					BarkerERShapeDomainModel.resourceManager = new global::System.Resources.ResourceManager(ResourceBaseName, typeof(BarkerERShapeDomainModel).Assembly);
				}
				return BarkerERShapeDomainModel.resourceManager;
			}
		}
		#endregion
		#region Copy/Remove closures
		/// <summary>
		/// CopyClosure cache
		/// </summary>
		private static DslModeling::IElementVisitorFilter copyClosure;
		/// <summary>
		/// DeleteClosure cache
		/// </summary>
		private static DslModeling::IElementVisitorFilter removeClosure;
		/// <summary>
		/// Returns an IElementVisitorFilter that corresponds to the ClosureType.
		/// </summary>
		/// <param name="type">closure type</param>
		/// <param name="rootElements">collection of root elements</param>
		/// <returns>IElementVisitorFilter or null</returns>
		public override DslModeling::IElementVisitorFilter GetClosureFilter(DslModeling::ClosureType type, global::System.Collections.Generic.ICollection<DslModeling::ModelElement> rootElements)
		{
			switch (type)
			{
				case DslModeling::ClosureType.CopyClosure:
					return BarkerERShapeDomainModel.CopyClosure;
				case DslModeling::ClosureType.DeleteClosure:
					return BarkerERShapeDomainModel.DeleteClosure;
			}
			return base.GetClosureFilter(type, rootElements);
		}
		/// <summary>
		/// CopyClosure cache
		/// </summary>
		private static DslModeling::IElementVisitorFilter CopyClosure
		{
			get
			{
				// Incorporate all of the closures from the models we extend
				if (BarkerERShapeDomainModel.copyClosure == null)
				{
					DslModeling::ChainingElementVisitorFilter copyFilter = new DslModeling::ChainingElementVisitorFilter();
					copyFilter.AddFilter(new BarkerERShapeCopyClosure());
					copyFilter.AddFilter(new DslModeling::CoreCopyClosure());
					copyFilter.AddFilter(new DslDiagrams::CoreDesignSurfaceCopyClosure());
					
					BarkerERShapeDomainModel.copyClosure = copyFilter;
				}
				return BarkerERShapeDomainModel.copyClosure;
			}
		}
		/// <summary>
		/// DeleteClosure cache
		/// </summary>
		private static DslModeling::IElementVisitorFilter DeleteClosure
		{
			get
			{
				// Incorporate all of the closures from the models we extend
				if (BarkerERShapeDomainModel.removeClosure == null)
				{
					DslModeling::ChainingElementVisitorFilter removeFilter = new DslModeling::ChainingElementVisitorFilter();
					removeFilter.AddFilter(new BarkerERShapeDeleteClosure());
					removeFilter.AddFilter(new DslModeling::CoreDeleteClosure());
					removeFilter.AddFilter(new DslDiagrams::CoreDesignSurfaceDeleteClosure());
		
					BarkerERShapeDomainModel.removeClosure = removeFilter;
				}
				return BarkerERShapeDomainModel.removeClosure;
			}
		}
		#endregion
		#region Diagram rule helpers
		/// <summary>
		/// Enables rules in this domain model related to diagram fixup for the given store.
		/// If diagram data will be loaded into the store, this method should be called first to ensure
		/// that the diagram behaves properly.
		/// </summary>
		public static void EnableDiagramRules(DslModeling::Store store)
		{
			if(store == null) throw new global::System.ArgumentNullException("store");
			
			DslModeling::RuleManager ruleManager = store.RuleManager;
			ruleManager.EnableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.FixUpDiagram));
			ruleManager.EnableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemAddRule));
			ruleManager.EnableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemDeleteRule));
			ruleManager.EnableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemRolePlayerChangeRule));
			ruleManager.EnableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemRolePlayerPositionChangeRule));
			ruleManager.EnableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemChangeRule));
		}
		
		/// <summary>
		/// Disables rules in this domain model related to diagram fixup for the given store.
		/// </summary>
		public static void DisableDiagramRules(DslModeling::Store store)
		{
			if(store == null) throw new global::System.ArgumentNullException("store");
			
			DslModeling::RuleManager ruleManager = store.RuleManager;
			ruleManager.DisableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.FixUpDiagram));
			ruleManager.DisableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemAddRule));
			ruleManager.DisableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemDeleteRule));
			ruleManager.DisableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemRolePlayerChangeRule));
			ruleManager.DisableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemRolePlayerPositionChangeRule));
			ruleManager.DisableRule(typeof(global::ORMSolutions.ORMArchitect.Views.BarkerERView.CompartmentItemChangeRule));
		}
		#endregion
	}
		
	#region Copy/Remove closure classes
	/// <summary>
	/// Remove closure visitor filter
	/// </summary>
	internal partial class BarkerERShapeDeleteClosure : BarkerERShapeDeleteClosureBase, DslModeling::IElementVisitorFilter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public BarkerERShapeDeleteClosure() : base()
		{
		}
	}
	
	/// <summary>
	/// Base class for remove closure visitor filter
	/// </summary>
	internal partial class BarkerERShapeDeleteClosureBase : DslModeling::IElementVisitorFilter
	{
		/// <summary>
		/// DomainRoles
		/// </summary>
		private global::System.Collections.Specialized.HybridDictionary domainRoles;
		/// <summary>
		/// Constructor
		/// </summary>
		public BarkerERShapeDeleteClosureBase()
		{
			#region Initialize DomainData Table
			#endregion
		}
		/// <summary>
		/// Called to ask the filter if a particular relationship from a source element should be included in the traversal
		/// </summary>
		/// <param name="walker">ElementWalker that is traversing the model</param>
		/// <param name="sourceElement">Model Element playing the source role</param>
		/// <param name="sourceRoleInfo">DomainRoleInfo of the role that the source element is playing in the relationship</param>
		/// <param name="domainRelationshipInfo">DomainRelationshipInfo for the ElementLink in question</param>
		/// <param name="targetRelationship">Relationship in question</param>
		/// <returns>Yes if the relationship should be traversed</returns>
		public virtual DslModeling::VisitorFilterResult ShouldVisitRelationship(DslModeling::ElementWalker walker, DslModeling::ModelElement sourceElement, DslModeling::DomainRoleInfo sourceRoleInfo, DslModeling::DomainRelationshipInfo domainRelationshipInfo, DslModeling::ElementLink targetRelationship)
		{
			return DslModeling::VisitorFilterResult.Yes;
		}
		/// <summary>
		/// Called to ask the filter if a particular role player should be Visited during traversal
		/// </summary>
		/// <param name="walker">ElementWalker that is traversing the model</param>
		/// <param name="sourceElement">Model Element playing the source role</param>
		/// <param name="elementLink">Element Link that forms the relationship to the role player in question</param>
		/// <param name="targetDomainRole">DomainRoleInfo of the target role</param>
		/// <param name="targetRolePlayer">Model Element that plays the target role in the relationship</param>
		/// <returns></returns>
		public virtual DslModeling::VisitorFilterResult ShouldVisitRolePlayer(DslModeling::ElementWalker walker, DslModeling::ModelElement sourceElement, DslModeling::ElementLink elementLink, DslModeling::DomainRoleInfo targetDomainRole, DslModeling::ModelElement targetRolePlayer)
		{
			if (targetDomainRole == null) throw new global::System.ArgumentNullException("targetDomainRole");
			return this.DomainRoles.Contains(targetDomainRole.Id) ? DslModeling::VisitorFilterResult.Yes : DslModeling::VisitorFilterResult.DoNotCare;
		}
		/// <summary>
		/// DomainRoles
		/// </summary>
		private global::System.Collections.Specialized.HybridDictionary DomainRoles
		{
			get
			{
				if (this.domainRoles == null)
				{
					this.domainRoles = new global::System.Collections.Specialized.HybridDictionary();
				}
				return this.domainRoles;
			}
		}
	
	}
	/// <summary>
	/// Copy closure visitor filter
	/// </summary>
	internal partial class BarkerERShapeCopyClosure : BarkerERShapeCopyClosureBase, DslModeling::IElementVisitorFilter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public BarkerERShapeCopyClosure() : base()
		{
		}
	}
	/// <summary>
	/// Base class for copy closure visitor filter
	/// </summary>
	internal partial class BarkerERShapeCopyClosureBase : DslModeling::CopyClosureFilter, DslModeling::IElementVisitorFilter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public BarkerERShapeCopyClosureBase():base()
		{
		}
	}
	#endregion
		
}

