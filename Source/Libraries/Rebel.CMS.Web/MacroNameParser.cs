using System;

namespace Rebel.Cms.Web
{
    /// <summary>
    /// Used to parse Macro names
    /// </summary>
    internal static class MacroNameParser
    {
        /// <summary>
        /// Parses the macro string name for a child action macro
        /// </summary>
        /// <param name="macroName"></param>
        /// <returns>returns a Tuple that contains the area name, controller name and action name</returns>
        internal static MacroNameParserResult ParseChildActionMacroName(string macroName)
        {
            var hyphenIndex = macroName.LastIndexOf('-');
            var areaName = hyphenIndex == -1
                               ? ""
                               : macroName.Substring(0, hyphenIndex);

            var controllerSection = macroName.Substring(hyphenIndex + 1, macroName.Length - hyphenIndex - 1);

            //we need to get the surface controller by name
            var controllerParts = controllerSection.Split('.');
            if (controllerParts.Length != 2)
                throw new FormatException("The string format for macro.SelectedItem for child actions must be: [AreaName-]ControllerName.ActionName");
            var controllerName = controllerParts[0].Replace(areaName + "-", ""); //strip off area name and hyphen if present (will be for plug-in based surface controllers, won't be for local ones)
            var controllerAction = controllerParts[1];

            return new MacroNameParserResult(areaName, controllerName, controllerAction);
        }
    }

    /// <summary>
    /// The result of parsing a child action macro name
    /// </summary>
    internal class MacroNameParserResult
    {
        public MacroNameParserResult(string areaName, string controllerName, string controllerAction)
        {
            AreaName = areaName;
            ControllerName = controllerName;
            ActionName = controllerAction;
        }

        internal string AreaName { get; set; }
        internal string ControllerName { get; set; }
        internal string ActionName { get; set; }
    }
}