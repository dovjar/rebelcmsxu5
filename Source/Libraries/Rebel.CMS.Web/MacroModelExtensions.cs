using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Macros;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.Controllers;
using Rebel.Cms.Web.Surface;

namespace Rebel.Cms.Web
{
  
    public static class MacroModelExtensions
    {
        
        /// <summary>
        /// Gets the surface controller meta data and a reference to the MethodInfo object for the ChildAction to render based on the 
        /// macro definition
        /// </summary>
        /// <param name="macro"></param>
        /// <param name="components"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Lazy<SurfaceController, SurfaceMetadata> GetSurfaceMacroChildAction(
            this MacroDefinition macro, 
            ComponentRegistrations components,
            out MethodInfo action)
        {

            var parsedMacroName = MacroNameParser.ParseChildActionMacroName(macro.SelectedItem);            
            
            //get the surface controller for the area/controller name
            var surfaceController = components.SurfaceControllers
                .Where(x => x.Metadata.ControllerName == parsedMacroName.ControllerName
                    && (parsedMacroName.AreaName == "" || (x.Metadata.PluginDefinition != null && x.Metadata.PluginDefinition.PackageName == parsedMacroName.AreaName)))
                .SingleOrDefault();
            
            if (surfaceController == null)
                throw new ApplicationException("Could not find the Surface controller '" + parsedMacroName.ControllerName);
            if (!surfaceController.Metadata.HasChildActionMacros)
                throw new ApplicationException("The Surface controller '" + parsedMacroName.ControllerName + "' is not advertising that it HasChildActionMacros");
            //now we need to get the controller's referenced child action
            var childAction = surfaceController.Metadata.ComponentType.GetMethods()
                .Where(x => x.Name == parsedMacroName.ActionName && x.GetCustomAttributes(typeof(ChildActionOnlyAttribute), false).Any())
                .SingleOrDefault();
            if (childAction == null)
                throw new ApplicationException("The Surface controller '" + parsedMacroName.ControllerName + "' with Action: '" + parsedMacroName.ActionName + "' could not be found or was not attributed with a ChildActionOnlyAttribute");

            action = childAction;
            return surfaceController;
        }
    }
}
