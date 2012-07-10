using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Rebel.Cms.Web.Dashboards;
using Rebel.Cms.Web.Dashboards.Filters;
using Rebel.Cms.Web.Dashboards.Rules;
using Rebel.Cms.Web.DependencyManagement.DemandBuilders;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.Macros;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.Controllers;
using Rebel.Cms.Web.Mvc.Controllers.BackOffice;
using Rebel.Cms.Web.Surface;
using Rebel.Cms.Web.System;
using Rebel.Cms.Web.Trees;

using Rebel.Framework;
using Rebel.Framework.DependencyManagement;
using Rebel.Framework.Diagnostics;
using Rebel.Framework.Security;
using Rebel.Framework.Tasks;
using MenuItem = Rebel.Cms.Web.Model.BackOffice.Trees.MenuItem;
using MenuItemAttribute = Rebel.Cms.Web.Model.BackOffice.Trees.MenuItemAttribute;
using MenuItemMetadata = Rebel.Cms.Web.Model.BackOffice.Trees.MenuItemMetadata;

namespace Rebel.Cms.Web.DependencyManagement
{
    /// <summary>
    /// Registers all Rebel system components/plugins
    /// </summary>
    public class RebelComponentRegistrar : IComponentRegistrar
    {

        private bool _macroEnginesRegistered;
        private bool _surfaceControllersRegistered;
        private bool _editorControllersRegistered;
        private bool _menuItemsRegistered;
        private bool _treeControllersRegistered;
        private bool _tasksRegistered;
        private bool _propEditorsRegistered;
        private bool _paramEditorsRegistered;
        private bool _dashboardMatchRulesRegistered;
        private bool _dashboardFiltersRegistered;
        private bool _permissionsRegistered;

        private readonly Assembly[] _pluginAssemblies;
        private readonly Assembly[] _localAssemblies;

        public RebelComponentRegistrar()
        {
            //determine types that need to be scanned, we only look this up once for perf reasons
            _pluginAssemblies = PluginManager.ReferencedPlugins.Select(x => x.ReferencedAssembly).ToArray();

            _localAssemblies = TypeFinder.GetFilteredBinFolderAssemblies(_pluginAssemblies).ToArray();
        }

        /// <summary>
        /// For each plugin found, this method is called for it's type to check if the plugin has an DemandsDependenciesAttribute
        /// and if so, builds its dependencies.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="t"></param>
        protected virtual void BuildComponentDependencies(IContainerBuilder builder, Type t)
        {
            DemandsDependenciesDemandRunniner.Run(builder, t);
        }

        /// <summary>
        /// Searches filtered local assemblies and plugin assemblies for the Plugin type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeFinder"></param>
        /// <returns></returns>
        private IEnumerable<Type> FindTypesInRequiredAssemblies<T>(TypeFinder typeFinder)
        {
            var types = new List<Type>();
            
            //register the built in types
            types.AddRange(typeFinder.FindClassesOfType<T>(_localAssemblies));

            //register any types defined in plugins
            types.AddRange(typeFinder.FindClassesOfType<T, AssemblyContainsPluginsAttribute>(_pluginAssemblies));
            return types;
        } 

        public virtual void RegisterMacroEngines(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_macroEnginesRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterMacroEngines start took {0}ms", () => timer);
                _macroEnginesRegistered = true;
            }))
            {
                //now register each type in the container and also add it to our collection);
                foreach (var t in FindTypesInRequiredAssemblies<AbstractMacroEngine>(typeFinder))
                {
                    var engineType = t;
                    RegisterComponent<AbstractMacroEngine, MacroEngineAttribute, MacroEngineMetadata>(
                        t, builder, true,
                        (pluginDef, attribute, registrar) =>
                        registrar
                            .WithMetadata<MacroEngineMetadata, string>(am => am.EngineName, attribute.EngineName)
                            .WithMetadata<MacroEngineMetadata, bool>(am => am.IsInternalRebelEngine,
                                                                engineType.GetCustomAttributes(typeof(RebelMacroEngineAttribute), false).Any())
                                                                .ScopedAs.Singleton()); //only need one each    
                }
            }
        }

        public virtual void RegisterDashboardFilters(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_dashboardFiltersRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterDashboardFilters start took {0}ms", () => timer);
                _dashboardFiltersRegistered = true;
            }))
            {
                var types = new List<Type>(FindTypesInRequiredAssemblies<DashboardFilter>(typeFinder));
                
                //now register each type in the container and also add it to our collection);
                foreach (var t in types)
                {
                    RegisterComponent<DashboardFilter, DashboardFilterMetadata>(t, builder, null);
                }
            }
        }

        public virtual void RegisterDashboardMatchRules(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_dashboardMatchRulesRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterDashboardMatchRules start took {0}ms", () => timer);
                _dashboardMatchRulesRegistered = true;
            }))
            {                
                //now register each type in the container and also add it to our collection);
                foreach (var t in FindTypesInRequiredAssemblies<DashboardMatchRule>(typeFinder))
                {
                    RegisterComponent<DashboardMatchRule, DashboardRuleMetadata>(t, builder, null);
                }
            }
        }

        /// <summary>
        /// Finds & Registers the Surface controllers
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        public virtual void RegisterSurfaceControllers(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_surfaceControllersRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterSurfaceControllers start took {0}ms", () => timer);
                _surfaceControllersRegistered = true;
            }))
            {                
                //now register each type in the container and also add it to our collection);
                foreach (var t in FindTypesInRequiredAssemblies<SurfaceController>(typeFinder))
                {
                    var surfaceType = t;

                    RegisterComponent<SurfaceController, SurfaceAttribute, SurfaceMetadata>(
                        t, builder, false,
                        (pluginDef, attribute, registrar) =>
                        registrar
                            .WithMetadata<SurfaceMetadata, string>(am => am.ControllerName, RebelController.GetControllerName(surfaceType))
                            .WithMetadata<SurfaceMetadata, bool>(am => am.HasChildActionMacros,
                                                                 surfaceType.GetMethods().Any(a => a.GetCustomAttributes(typeof (ChildActionOnlyAttribute), false).Any())));
                }
            }


        }

        /// <summary>
        /// Register the editor controllers
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        public virtual void RegisterEditorControllers(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_editorControllersRegistered) return;
            
            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterEditorControllers start took {0}ms", () => timer);
                _editorControllersRegistered = true;
            }))
            {                
                //now register each type in the container and also add it to our collection
                foreach (var t in FindTypesInRequiredAssemblies<AbstractEditorController>(typeFinder))
                {
                    var editorType = t;

                    RegisterComponent<AbstractEditorController, EditorAttribute, EditorMetadata>(
                        t, builder,true,
                        (pluginDef, attribute, registrar) =>
                        registrar
                            .WithMetadata<EditorMetadata, string>(am => am.ControllerName, RebelController.GetControllerName(editorType))
                            .WithMetadata<EditorMetadata, bool>(am => am.HasChildActionDashboards, attribute.HasChildActionDashboards)
                            .WithMetadata<EditorMetadata, bool>(am => am.IsInternalRebelEditor,
                                                                editorType.GetCustomAttributes(typeof (RebelEditorAttribute), false).Any()));

                }
            }


        }

        /// <summary>
        /// Registers all menu items
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        public virtual void RegisterMenuItems(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_menuItemsRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterMenuItems start took {0}ms", () => timer);
                _menuItemsRegistered = true;
            }))
            {
                foreach (var t in FindTypesInRequiredAssemblies<MenuItem>(typeFinder))
                {
                    RegisterComponent<MenuItem, MenuItemAttribute, MenuItemMetadata>(t, builder,true,
                        (pluginDef, attribute, registrar) =>
                                registrar
                                    .WithMetadata<MenuItemMetadata, string>(am => am.Title, attribute.Title)
                                    .WithMetadata<MenuItemMetadata, bool>(am => am.SeperatorBefore, attribute.SeparatorBefore)
                                    .WithMetadata<MenuItemMetadata, bool>(am => am.SeperatorAfter, attribute.SeparatorAfter)
                                    .WithMetadata<MenuItemMetadata, string>(am => am.Icon, attribute.Icon)
                                    .WithMetadata<MenuItemMetadata, string>(am => am.OnClientClick, attribute.OnClientClick)
                                    .ScopedAs.Singleton()); //only need one each    

                }
            }

        }

        /// <summary>
        /// Register the tree controllers
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        public virtual void RegisterTreeControllers(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_treeControllersRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterTreeControllers start took {0}ms", () => timer);
                _treeControllersRegistered = true;
            }))
            {

                //now register each type in the container and also add it to our collection
                foreach (var t in FindTypesInRequiredAssemblies<TreeController>(typeFinder))
                {
                    var treeType = t;

                    RegisterComponent<TreeController, TreeAttribute, TreeMetadata>(t, builder, true,
                        (pluginDef, attribute, registrar) =>
                                registrar
                                    .WithMetadata<EditorMetadata, string>(am => am.ControllerName, RebelController.GetControllerName(treeType))
                                    .WithMetadata<TreeMetadata, string>(am => am.TreeTitle, attribute.TreeTitle)
                                    .WithMetadata<TreeMetadata, bool>(am => am.IsInternalRebelTree,
                                                       treeType.GetCustomAttributes(typeof(RebelTreeAttribute), false).Any()));

                }
            }


        }

        /// <summary>
        /// Registers all RebelPropertyEditors
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        public virtual void RegisterPropertyEditors(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_propEditorsRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterPropertyEditors start took {0}ms", () => timer);
                _propEditorsRegistered = true;
            }))
            {
                foreach (var t in FindTypesInRequiredAssemblies<PropertyEditor>(typeFinder))
                {
                    var propEditorType = t;

                    //builder.ForType(propEditorType).Register();

                    RegisterComponent<PropertyEditor, PropertyEditorAttribute, PropertyEditorMetadata>(t, builder,true,
                        (pluginDef, attribute, registrar) =>
                                registrar
                                    .WithMetadata<PropertyEditorMetadata, string>(am => am.Name, attribute.Name)
                                    .WithMetadata<PropertyEditorMetadata, string>(am => am.Alias, attribute.Alias)
                                    .WithMetadata<PropertyEditorMetadata, bool>(am => am.IsContentPropertyEditor, attribute.IsContentPropertyEditor)
                                    .WithMetadata<PropertyEditorMetadata, bool>(am => am.IsParameterEditor, attribute.IsParameterEditor)
                                    .WithMetadata<PropertyEditorMetadata, bool>(am => am.IsInternalRebelEditor,
                                                       propEditorType.GetCustomAttributes(typeof(RebelPropertyEditorAttribute), false).Any()));

                }
            }

        }

        /// <summary>
        /// Registers all RebelParameterEditors
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        public virtual void RegisterParameterEditors(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_paramEditorsRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterParameterEditors start took {0}ms", () => timer);
                _paramEditorsRegistered = true;
            }))
            {

                foreach (var t in FindTypesInRequiredAssemblies<AbstractParameterEditor>(typeFinder))
                {
                    RegisterComponent<AbstractParameterEditor, ParameterEditorAttribute, ParameterEditorMetadata>(t, builder, true,
                        (pluginDef, attribute, registrar) =>
                                registrar
                                    .WithMetadata<ParameterEditorMetadata, string>(am => am.Name, attribute.Name)
                                    .WithMetadata<ParameterEditorMetadata, string>(am => am.Alias, attribute.Alias)
                                    .WithMetadata<ParameterEditorMetadata, Guid>(am => am.PropertyEditorId, attribute.PropertyEditorId));

                }
            }

        }

        /// <summary>
        /// Registers all tasks.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="typeFinder">The type finder.</param>
        /// <remarks></remarks>
        public virtual void RegisterTasks(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_tasksRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterTasks start took {0}ms", () => timer);
                _tasksRegistered = true;
            }))
            {
                foreach (var t in FindTypesInRequiredAssemblies<AbstractTask>(typeFinder))
                {
                    RegisterComponent<AbstractTask, TaskAttribute, TaskMetadata>(t, builder, false,
                        (pluginDef, attribute, registrar) =>
                            {
                                //if there's no attribute since we're not requiring that all AbstractTasks have one,
                                //then don't register it into IoC as it might be something like our 'Delegate' task 
                                //or another custom one that is added to the task manager at runtime
                                if (attribute == null) return;
                                
                                registrar
                                    .WithMetadata<TaskMetadata, string>(metadata => metadata.TriggerName, attribute.Trigger)
                                    .WithMetadata<TaskMetadata, bool>(metadata => metadata.ContinueOnError, attribute.ContinueOnFailure)
                                    .ScopedAs.Singleton();
                            }); //only need one each 

                }
            }
        }

        /// <summary>
        /// Registers the permissions.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="typeFinder">The type finder.</param>
        public virtual void RegisterPermissions(IContainerBuilder builder, TypeFinder typeFinder)
        {
            if (_permissionsRegistered) return;

            using (DisposableTimer.Start(timer =>
            {
                LogHelper.TraceIfEnabled<RebelComponentRegistrar>("RegisterPermissions start took {0}ms", () => timer);
                _permissionsRegistered = true;
            }))
            {                
                foreach (var t in FindTypesInRequiredAssemblies<Permission>(typeFinder))
                {
                    RegisterComponent<Permission, PermissionAttribute, PermissionMetadata>(t, builder, true,
                        (pluginDef, attribute, registrar) =>
                                registrar
                                    .WithMetadata<PermissionMetadata, string>(metadata => metadata.Name, attribute.Name)
                                    .WithMetadata<PermissionMetadata, string>(metadata => metadata.Type, attribute.Type)
                                    .WithMetadata<PermissionMetadata, UserType>(metadata => metadata.UserType, attribute.UserType)
                                    .ScopedAs.Singleton()); //only need one each 
                }
            }
        }

        /// <summary>
        /// Registers a component in the container with the specified base class and with it's associated metadata
        /// </summary>
        /// <typeparam name="TBaseType">The base type to register the component as in the container</typeparam>
        /// <typeparam name="TAttribute">The component attribute type</typeparam>
        /// <typeparam name="TMetadata">The metadata type</typeparam>
        /// <param name="componentType">The type of the component</param>
        /// <param name="builder"></param>
        /// <param name="requiresMandatoryAttribute">true if an exception should be thrown if the plugin requires to be attributed</param>
        /// <param name="registerMetadata">A callback method to register more meta data for the component</param>
        public void RegisterComponent<TBaseType, TAttribute, TMetadata>(
            Type componentType,
            IContainerBuilder builder,
            bool requiresMandatoryAttribute,
            Action<PluginDefinition, TAttribute, IDependencyRegistrar<object>> registerMetadata)
            where TMetadata : PluginMetadataComposition
            where TAttribute : PluginAttribute
        {
            var componentBaseType = typeof(TBaseType);
            var attributeType = typeof(TAttribute);

            //need to get the attribute
            var attrs = componentType.GetCustomAttributes(attributeType, false).Cast<TAttribute>();

            if (requiresMandatoryAttribute && !attrs.Any())
                throw new InvalidOperationException(
                    string.Format("Every {0} must be attributed with the {1} attribute", componentBaseType.Name, attributeType.Name));

            if (!attrs.Any())
            {
                //if there are no attributes found then we have a locally declared plugin (i.e. like a locally declared surface controller)
                PerformComponentRegistration<TBaseType, TAttribute, TMetadata>(componentType, builder, registerMetadata);
            }
            else
            {
                //add a registration for each attribute found, this supports registering multiple plugins per plugin class if it contains
                //multiple plugin attributes.
                foreach (var attr in attrs)
                {
                    PerformComponentRegistration<TBaseType, TAttribute, TMetadata>(componentType, builder, registerMetadata, attr);
                }
            }
        }

        /// <summary>
        /// Registers a component in the container with the specified base class and with it's associated metadata
        /// </summary>
        /// <typeparam name="TBaseType"></typeparam>
        /// <typeparam name="TAttribute"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        /// <param name="componentType"></param>
        /// <param name="builder"></param>
        /// <param name="registerMetadata"></param>
        /// <param name="attr"></param>
        private void PerformComponentRegistration<TBaseType, TAttribute, TMetadata>(
            Type componentType,
            IContainerBuilder builder,
            Action<PluginDefinition, TAttribute, IDependencyRegistrar<object>> registerMetadata,
            TAttribute attr = null)
            where TMetadata : PluginMetadataComposition
            where TAttribute : PluginAttribute
        {
            //get reference to this component's plugin definition. It may be null if this is not a plugin component
            var pluginDefList =
                PluginManager.ReferencedPlugins.Where(x => x.ReferencedAssembly == componentType.Assembly)
                    .ToList();

            if (pluginDefList.Count > 1)
            {
                throw new InvalidOperationException("The plugin " + componentType.FullName + " was found in more than one assembly: " + string.Join(Environment.NewLine, pluginDefList.Select(x => x.OriginalAssemblyFile.FullName).ToArray()));
            }

            var pluginDef = pluginDefList.SingleOrDefault();

            //register the component in the container with the basic Plugin meta data
            var registrar = builder.For(componentType)
                .KnownAs<TBaseType>()
                .WithMetadata<TMetadata, PluginDefinition>(am => am.PluginDefinition, pluginDef)
                .WithMetadata<TMetadata, Type>(am => am.ComponentType, componentType);

            //if a plugin attribute was found then add the plugin id to the metadata
            //if it is null, then it is a locally declared plugin
            if (attr != null)
            {
                registrar.WithMetadata<TMetadata, Guid>(am => am.Id, attr.Id);
            }

            //call the call back method, NOTE: the attribute can be null if requiresMandatoryAttribute = false so handlers will need to check!
            registerMetadata.IfNotNull(x => x.Invoke(pluginDef, attr, registrar));

            //build the componenet dependencies
            BuildComponentDependencies(builder, componentType);
        }

        /// <summary>
        /// Registers a simple component in the container with the specified base class and with it's associated metadata.
        /// </summary>
        /// <typeparam name="TBaseType"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        /// <param name="componentType"></param>
        /// <param name="builder"></param>
        /// <param name="registerMetadata"></param>
        public void RegisterComponent<TBaseType, TMetadata>(
            Type componentType,
            IContainerBuilder builder,
            Action<PluginDefinition, IDependencyRegistrar<object>> registerMetadata)
            where TMetadata : PluginMetadataComposition
        {
            //get reference to this component's plugin definition. It may be null if this is not a plugin component
            var pluginDef =
                PluginManager.ReferencedPlugins.Where(x => x.ReferencedAssembly == componentType.Assembly)
                    .SingleOrDefault();

            //register the component in the container with the basic Plugin meta data
            var registrar = builder.For(componentType)
                .KnownAs<TBaseType>()
                .WithMetadata<TMetadata, PluginDefinition>(am => am.PluginDefinition, pluginDef)
                .WithMetadata<TMetadata, Type>(am => am.ComponentType, componentType);

            //call the call back method
            registerMetadata.IfNotNull(x => x.Invoke(pluginDef, registrar));

            //build the componenet dependencies
            BuildComponentDependencies(builder, componentType);
        }

    }
}
