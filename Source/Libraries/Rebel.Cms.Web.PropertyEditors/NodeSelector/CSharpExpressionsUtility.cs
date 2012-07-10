using System;
using System.Linq;
using System.Text;
using System.Web.Compilation;
using Rebel.Cms.Web.BuildManagerCodeDelegates;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// A utility class for initializing and executing the csharp expressions for the Node Selector
    /// </summary>
    internal class CSharpExpressionsUtility
    {
        /// <summary>
        /// Get the result of the CSharp code for the node filter
        /// </summary>
        /// <param name="dataTypeId"> </param>
        /// <param name="ds"></param>
        /// <param name="nodeId"></param>
        /// <param name="backOfficeRequestContext"> </param>
        /// <returns></returns>
        internal static bool GetNodeFilterResult(IBackOfficeRequestContext backOfficeRequestContext, HiveId dataTypeId, INodeSelectorDataSource ds, HiveId nodeId)
        {
            var virtualPathId = SetupCodeDelegateForFilter(backOfficeRequestContext, dataTypeId);

            var instance = GetClassInstance<AbstractNodeFilter>(virtualPathId);
            if (instance != null)
            {                
                var entity = ds.GetEntity(nodeId);
                return instance.IsMatch(backOfficeRequestContext, entity);
            }
            return true;
        }

        /// <summary>
        /// Get the result of the CSharp code for the start node query
        /// </summary>
        /// <param name="dataTypeId"> </param>
        /// <param name="ds"></param>
        /// <param name="nodeId"></param>
        /// <param name="backOfficeRequestContext"> </param>
        /// <returns></returns>
        internal static HiveId GetStartNodeQueryResult(IBackOfficeRequestContext backOfficeRequestContext, HiveId dataTypeId, INodeSelectorDataSource ds, HiveId nodeId)
        {
            var virtualPathId = SetupCodeDelegateForQuery(backOfficeRequestContext, dataTypeId);

            var instance = GetClassInstance<AbstractStartNodeQuery>(virtualPathId);
            if (instance != null)
            {                
                var entity = ds.GetEntity(nodeId);
                var root = ds.GetEntity(ds.GetRootNodeId());
                return instance.StartNodeId(backOfficeRequestContext, entity, root);
            }
            return HiveId.Empty;
        }

        /// <summary>
        /// Returns the class instance for the virtual Path id specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="virtualPathId"></param>
        /// <returns></returns>
        private static T GetClassInstance<T>(string virtualPathId)
            where T: class
        {
            //get a csharp path for our code delegate
            var virtualPath = CodeDelegateVirtualPath.CreateFullPath(virtualPathId, "cs");
            //compile the code snippet to an assembly
            var compiledAssembly = BuildManager.GetCompiledAssembly(virtualPath);            
            if (compiledAssembly != null)
            {
                //now, we need to create instance, as we know we only have one type in the assembly, we'll just use that
                var compiledType = compiledAssembly.GetTypes().First();
                var instance = (T)Activator.CreateInstance(compiledType);
                return instance;
            }
            return null;
        }

        /// <summary>
        /// This is the method that will execute in order to return the code snippet that will be compiled by the BuildManager for the node filter
        /// </summary>
        /// <param name="backOfficeRequestContext"> </param>
        /// <param name="delegateParam"></param>
        /// <param name="virtualPathId"> </param>
        /// <returns></returns>
        internal static string GetFilter(IBackOfficeRequestContext backOfficeRequestContext, object delegateParam, string virtualPathId)
        {
            return GetCodeBlock(backOfficeRequestContext, delegateParam, virtualPathId, WrapFilter);
        }

        /// <summary>
        /// This is the method that will execute in order to return the code snippet that will be compiled by the BuildManager for the start node query
        /// </summary>
        /// <param name="backOfficeRequestContext"></param>
        /// <param name="delegateParam"></param>
        /// <param name="virtualPathId"></param>
        /// <returns></returns>
        internal static string GetStartNodeQuery(IBackOfficeRequestContext backOfficeRequestContext, object delegateParam, string virtualPathId)
        {
            return GetCodeBlock(backOfficeRequestContext, delegateParam, virtualPathId, WrapStartNodeQuery);
        }

        /// <summary>
        /// This is the method that will execute in order to return the code snippet that will be compiled by the BuildManager 
        /// </summary>
        /// <param name="backOfficeRequestContext"></param>
        /// <param name="delegateParam"></param>
        /// <param name="virtualPathId"></param>
        /// <param name="codeWrapper"></param>
        /// <returns></returns>
        private static string GetCodeBlock(
            IBackOfficeRequestContext backOfficeRequestContext, 
            object delegateParam, 
            string virtualPathId,
            Func<NodeSelectorPreValueModel, string, string> codeWrapper)
        {
            //first we need to cast the delegateParam into the object it actually is...
            //we can guarantee it is castable since we are setting it above.
            var dp = (Tuple<HiveId, CSharpCodeBlockType>)delegateParam;

            using (var uow = backOfficeRequestContext.Application.Hive.OpenReader<IContentStore>())
            {
                var dt = uow.Repositories.Schemas.Get<AttributeType>(true, dp.Item1).SingleOrDefault();
                if (dt == null)
                {
                    throw new InvalidOperationException("Could not find AttributeType with id " + dp.Item1);
                }
                var dataTypeViewModel = backOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<AttributeType, DataTypeEditorModel>(dt)
                                            .PreValueEditorModel as NodeSelectorPreValueModel;
                if (dataTypeViewModel == null)
                {
                    throw new InvalidOperationException("The AttributeType must have a " + typeof(NodeSelectorEditor).Name + " type as its PropertyEditor");
                }

                return codeWrapper(dataTypeViewModel, virtualPathId);
            }
        }

        /// <summary>
        /// Creates a class out of the query
        /// </summary>
        /// <param name="preVals"></param>
        /// <param name="virtualPathId"></param>
        /// <returns></returns>
        private static string WrapStartNodeQuery(NodeSelectorPreValueModel preVals, string virtualPathId)
        {
            //trying to include everything someone might need!
            var sb = new StringBuilder();

            sb.AppendLine(@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.ProviderSupport;
using Rebel.Hive.RepositoryTypes;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Framework.Security.Model.Schemas;
using Rebel.Framework;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model;

");
            sb.AppendLine("namespace Rebel.Cms.Web.PropertyEditors.NodeSelector.CompiledScripts {");
            sb.Append("public class ");
            sb.Append(virtualPathId);//the class name will be NS + the virtual path id used (which is alphanumeric so should work)
            sb.Append(" : Rebel.Cms.Web.PropertyEditors.NodeSelector.AbstractStartNodeQuery {");
            sb.AppendLine("");

            sb.AppendLine("public override HiveId StartNodeId(IBackOfficeRequestContext requestContext, TypedEntity currentNode, TypedEntity rootNode) {");

            sb.AppendLine(preVals.StartNodeQuery); //add the query body

            sb.AppendLine("}"); //end method
            sb.AppendLine("}"); //end class
            sb.AppendLine("}"); //end namespace

            return sb.ToString();        
        }

        /// <summary>
        /// Creates a class out of the filter
        /// </summary>
        /// <param name="preVals"></param>
        /// <param name="virtualPathId"> </param>
        /// <returns></returns>
        private static string WrapFilter(NodeSelectorPreValueModel preVals, string virtualPathId)
        {
            //trying to include everything someone might need!
            var sb = new StringBuilder();

            sb.AppendLine(@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.ProviderSupport;
using Rebel.Hive.RepositoryTypes;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Framework.Security.Model.Schemas;
using Rebel.Framework;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model;

");
            sb.AppendLine("namespace Rebel.Cms.Web.PropertyEditors.NodeSelector.CompiledScripts {");
            sb.Append("public class ");
            sb.Append(virtualPathId);//the class name will be NS + the virtual path id used (which is alphanumeric so should work)
            sb.Append(" : Rebel.Cms.Web.PropertyEditors.NodeSelector.AbstractNodeFilter {");
            sb.AppendLine("");

            sb.AppendLine("public override bool IsMatch(IBackOfficeRequestContext requestContext, TypedEntity entity) {");

            sb.AppendLine(preVals.NodeFilter); //add the filter body

            sb.AppendLine("}"); //end method
            sb.AppendLine("}"); //end class
            sb.AppendLine("}"); //end namespace

            return sb.ToString();
        }

        /// <summary>
        /// Ensures that the code delegate for the filter is in the CodeDelegatesCollection
        /// </summary>
        /// <param name="backOfficeRequestContext"></param>
        /// <param name="dataTypeId"></param>
        /// <returns></returns>
        internal static string SetupCodeDelegateForQuery(IBackOfficeRequestContext backOfficeRequestContext, HiveId dataTypeId)
        {
            return SetupCodeDelegate(backOfficeRequestContext, dataTypeId, CSharpCodeBlockType.StartNodeQuery,
                (dp, vpi) => GetStartNodeQuery(backOfficeRequestContext, dp, vpi));
        }

        /// <summary>
        /// Ensures that the code delegate for the filter is in the CodeDelegatesCollection
        /// </summary>
        /// <param name="backOfficeRequestContext"> </param>
        /// <param name="dataTypeId"></param>
        /// <returns></returns>
        internal static string SetupCodeDelegateForFilter(IBackOfficeRequestContext backOfficeRequestContext, HiveId dataTypeId)
        {
            return SetupCodeDelegate(backOfficeRequestContext, dataTypeId, CSharpCodeBlockType.NodeFilter,
                (dp, vpi) => GetFilter(backOfficeRequestContext, dp, vpi));
        }

        /// <summary>
        /// Ensures that the code delegate for the filter is in the CodeDelegatesCollection
        /// </summary>
        /// <param name="backOfficeRequestContext"></param>
        /// <param name="dataTypeId"></param>
        /// <param name="blockType"></param>
        /// <param name="codedelegate"> </param>
        /// <returns></returns>
        private static string SetupCodeDelegate(
            IBackOfficeRequestContext backOfficeRequestContext, 
            HiveId dataTypeId, 
            CSharpCodeBlockType blockType,
            Func<object, string, string> codedelegate)
        {
            var delegateParameter = new Tuple<HiveId, CSharpCodeBlockType>(dataTypeId, blockType);

            //first, check if we have this registered (multiple ids may exist for the same delegate, but in thsi case it will never be true)
            var ids = CodeDelegateVirtualPath.GetVirtualPathIdsForDelegate(delegateParameter).ToArray();
            if (ids.Any())
            {
                return ids.First();
            }

            //need to lookup the data type to get its alias as we'll use this for the path
            using (var uow = backOfficeRequestContext.Application.Hive.OpenReader<IContentStore>())
            {
                var dt = uow.Repositories.Schemas.Get<AttributeType>(true, dataTypeId).SingleOrDefault();
                if (dt == null)
                {
                    throw new InvalidOperationException("Could not find AttributeType with id " + dataTypeId);
                }
                //create a unique path for our object which will be used in the virtual path creation, if we don't set this
                //then the ToString of the object key will be used which is much harder to debug.
                var path = dt.Alias + "_" + delegateParameter.Item2 + "_"
                           + delegateParameter.GetHashCode().ToString().Replace("-", "0"); //only alphanumeric chars allowed
                var virtualPathId = CodeDelegateVirtualPath.GetOrCreateVirtualPathId(delegateParameter, path);

                //add the delegate to the collection
                CodeDelegatesCollection.TryAdd(virtualPathId, codedelegate);

                return virtualPathId;
            }
        }
    }
}