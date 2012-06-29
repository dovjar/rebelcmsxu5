using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Web;
using System.IO;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Compilation;
using System.Reflection;
using System.Web.Util;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.System;

using Umbraco.Framework;
using Umbraco.Framework.Configuration;
using Umbraco.Framework.Diagnostics;

// SEE THIS POST for full details of what this does
//http://shazwazza.com/post/Developing-a-plugin-framework-in-ASPNET-with-medium-trust.aspx
// THIS POST ALSO HELPS
// http://shazwazza.com/post/Taming-the-BuildManager-ASPNet-Temp-files-and-AppDomain-restarts.aspx

[assembly: PreApplicationStartMethod(typeof(PluginManager), "Initialize")]

namespace Umbraco.Cms.Web.System
{
    /// <summary>
    /// Sets the application up for the plugin referencing
    /// </summary>
    public class PluginManager
    {
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static DirectoryInfo _shadowCopyFolder;
        private static bool? _isFullTrust;
        private static PluginManagerShadowCopyType _shadowCopyType;
        private static DirectoryInfo _pluginFolder;
        private static string _pluginsHash;
        public const string PackagesFolderName = "Packages";


        /// <summary>
        /// Returns a collection of all referenced plugin assemblies that have been shadow copied
        /// </summary>
        public static IEnumerable<PluginDefinition> ReferencedPlugins { get; private set; }

        public static void Initialize()
        {
            using (new WriteLockDisposable(Locker))
            {
                using (DisposableTimer.TraceDuration<PluginManager>("Start Initialise", "End Initialise"))
                {
                    var settings = UmbracoSettings.GetSettings();

                    try
                    {
                        LogHelper.TraceIfEnabled<PluginManager>("Codegen dir is {0}", () => HttpRuntime.CodegenDir);
                    }
                    catch
                    {
                        LogHelper.TraceIfEnabled<PluginManager>("Was going to log Codegendir but couldn't get it from HttpRuntime");
                    }


                    var pluginPath = HostingEnvironment.MapPath(settings.PluginConfig.PluginsPath);

                    _pluginFolder = new DirectoryInfo(pluginPath);
                    _shadowCopyType = settings.PluginConfig.ShadowCopyType;

                    //default is bin
                    _shadowCopyFolder = new DirectoryInfo(HostingEnvironment.MapPath("~/bin"));  
                  
                    if (_shadowCopyType == PluginManagerShadowCopyType.OverrideDefaultFolder)
                    {
                        var shadowCopyPath = HostingEnvironment.MapPath(settings.PluginConfig.ShadowCopyPath);

                        //if override is set, then use the one defined in config
                        _shadowCopyFolder = new DirectoryInfo(shadowCopyPath);    
                    }
                    else if (IsFullTrust() && _shadowCopyType == PluginManagerShadowCopyType.UseDynamicFolder)
                    {
                        //if in full trust and use dynamic folder, then set to CodeGen folder
                        _shadowCopyFolder = new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);
                    }                    

                    var referencedPlugins = new List<PluginDefinition>();

                    var pluginFiles = Enumerable.Empty<FileInfo>();

                    try
                    {
                        //ensure folders are created
                        Directory.CreateDirectory(_pluginFolder.FullName);
                        if (_shadowCopyType != PluginManagerShadowCopyType.UseDynamicFolder)
                            Directory.CreateDirectory(_shadowCopyFolder.FullName);
                        Directory.CreateDirectory(Path.Combine(_pluginFolder.FullName, PackagesFolderName));

                        using (DisposableTimer.TraceDuration<PluginManager>("Finding plugin DLLs", "Finding plugin DLLs completed"))
                        {
                            //get list of all DLLs in plugins (not in bin!)
                            //this will match the plugin folder pattern
                            pluginFiles = _pluginFolder.GetFiles("*.dll", SearchOption.AllDirectories)
                                .ToArray() // -> this magically reduces the time by about 100ms
                                .Where(x => x.Directory.Parent != null && (IsPackageLibFolder(x.Directory)))
                                .ToList();
                        }

                    }
                    catch (Exception ex)
                    {
                        var fail = new ApplicationException("Could not initialise plugin folder", ex);
                        LogHelper.Error<PluginManager>(fail.Message, fail);
                        //throw fail;
                    }

                    try
                    {
                        //Detect if there are any changes to plugins and perform any cleanup if in full trust using the dynamic folder
                        if (DetectAndCleanStalePlugins(pluginFiles, _shadowCopyFolder))
                        {
                            //if plugins changes have been detected, then shadow copy and re-save the cache files

                            LogHelper.TraceIfEnabled<PluginManager>("Shadow copying assemblies");

                            //shadow copy files
                            referencedPlugins
                                .AddRange(pluginFiles.Select(plug =>
                                    new PluginDefinition(plug,
                                        GetPackageFolderFromPluginDll(plug),
                                        PerformFileDeployAndRegistration(plug, true),
                                        plug.Directory.Parent.Name == "Core")));

                            //save our plugin hash value
                            WriteCachePluginsHash(pluginFiles);
                            //save our plugins list
                            WriteCachePluginsList(pluginFiles);

                            var codeBase = typeof(PluginManager).Assembly.CodeBase;
                            var uri = new Uri(codeBase);
                            var path = uri.LocalPath;
                            var binFolder = Path.GetDirectoryName(path);

                            if (_shadowCopyFolder.FullName.InvariantEquals(binFolder))
                            {
                                LogHelper.Warn<PluginManager>("Performance: An app-pool recycle is likely to occur shortly because plugin changes were detected and shadow-copied to the bin folder");
                            }
                        }
                        else
                        {
                            LogHelper.TraceIfEnabled<PluginManager>("No plugin changes detected");

                            referencedPlugins
                                .AddRange(pluginFiles.Select(plug =>
                                    new PluginDefinition(plug,
                                        GetPackageFolderFromPluginDll(plug),
                                        PerformFileDeployAndRegistration(plug, false),
                                        plug.Directory.Parent.Name == "Core")));
                        }
                    }
                    catch(UnauthorizedAccessException)
                    {
                        //throw the exception if its UnauthorizedAccessException as this will 
                        //be because we cannot copy to the shadow copy folder, or update the umbraco plugin hash/list files.
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var fail = new ApplicationException("Could not initialise plugin folder", ex);
                        LogHelper.Error<PluginManager>(fail.Message, fail);
                        //throw fail;

                    }

                    ReferencedPlugins = referencedPlugins;
                }
            }
        }

        /// <summary>
        /// When given a .delete file this will attempt to delete the base file that the .delete was created from
        /// and then the .delete file itself. if both are removed then it will return true.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        internal static bool CleanupDotDeletePluginFiles(FileInfo f)
        {
            if (f.Extension != ".delete")
                return false;

            var baseFileName = Path.GetDirectoryName(f.FullName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(f.FullName);
            //NOTE: this is to remove our old GUID named files... we shouldn't have any of these buts sometimes things are just locked
            var base64Files = f.Directory.GetFiles(Path.GetFileName(baseFileName) + "???????????????????????????????????" + ".delete").ToList();
            Func<string, bool> delFile = x =>
                {
                    if (x.IsNullOrWhiteSpace())
                        return true;
                    if (File.Exists(x))
                    {
                        try
                        {
                            File.Delete(x);
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    return true;
                };
            //always try deleting old guid named files
            foreach (var x in base64Files)
                delFile(x.FullName);
            //try to delete the .dll file
            if (!delFile(baseFileName))
                return false;

            try
            {
                //now try to remove the .delete file
                f.Delete();
            }
            catch { }

            return true;
        }

        /// <summary>
        /// Returns the .delete file name for a particular file. If just the file name + .delete already exists and is locked,
        /// then this returns a Guid + .delete format.
        /// </summary>
        /// <param name="pluginFile"></param>
        /// <returns></returns>
        private static string GetNewDotDeleteName(FileInfo pluginFile)
        {
            if (pluginFile.Extension.ToUpper() != ".DLL")
                throw new NotSupportedException("This method only supports DLL files");

            var dotDelete = pluginFile.FullName + ".delete";
            try
            {
                if (File.Exists(dotDelete))
                    File.Delete(dotDelete);
            }
            catch
            {
                LogHelper.TraceIfEnabled<PluginManager>("Cannot remove file " + dotDelete + " it is locked, renaming to .delete file with GUID");
                //cannot delete, so we will need to use a GUID
                dotDelete = pluginFile.FullName + Guid.NewGuid().ToString("N") + ".delete";
            }
            return dotDelete;
        }

        /// <summary>
        /// Tries to delete the file, if it doesn't go nicely, we'll force rename it to .delete, if that already exists and we cannot
        /// remove the .delete file then we rename to Guid + .delete
        /// </summary>
        /// <param name="f"></param>
        /// <returns>If it deletes nicely return true, otherwise false</returns>
        internal static bool CleanupPlugin(FileInfo f)
        {
            LogHelper.TraceIfEnabled<PluginManager>("Cleaning up stale plugin " + f.FullName);

            //this is a special case, people may have been usign the CodeGen folder before so we need to cleanup those
            //files too even if we are no longer using it
            if (IsFullTrust() && _shadowCopyType != PluginManagerShadowCopyType.UseDynamicFolder)
            {
                CleanupDotDeletePluginFiles(new FileInfo(Path.Combine(AppDomain.CurrentDomain.DynamicDirectory, f.Name + ".delete")));
            }

            if (CleanupDotDeletePluginFiles(new FileInfo(f.FullName + ".delete")))
                return true;

            try
            {
                f.Delete();
                return true;
            }
            catch { }

            //if the file doesn't delete, then create the .delete file for it, hopefully the BuildManager will clean up next time
            var newName = GetNewDotDeleteName(f);

            try
            {
                File.Move(f.FullName, newName);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", Path.GetDirectoryName(newName)));
            }
            catch (IOException)
            {
                LogHelper.TraceIfEnabled<PluginManager>(f.FullName + " rename failed, cannot remove stale plugin");
                throw;
            }

            //make it an empty file
            try
            {
                (new StreamWriter(newName)).Close();
            }
            catch { }

            LogHelper.TraceIfEnabled<PluginManager>("Stale plugin " + f.FullName + " failed to cleanup successfully, a .delete file has been created for it");
            return false;
        }

        /// <summary>
        /// This checks if any of our plugins have changed, if so it will go 
        /// </summary>
        /// <returns>
        /// Returns true if there were changes to plugins and a cleanup was required, otherwise if all plugins are the same then false is returned.
        /// </returns>
        private static bool DetectAndCleanStalePlugins(IEnumerable<FileInfo> plugins, DirectoryInfo shadowCopyFolder)
        {
            //NOTE: One other sure fire way to ensure everything is cleaned up is to 'bump' the global.asax file
            // however this will cause a 2nd AppDomain restart, though it will ensure the BuildManager does all the work
            // to cleanup all files in the CodeGen folder.

            //check if anything has been changed, or if we are in debug mode then always perform cleanup
            if (ConvertPluginsHash(GetPluginsHash(plugins)) != GetCachePluginsHash())
            {
                LogHelper.TraceIfEnabled<PluginManager>("Plugin changes detected in hash");

                //we need to combine the old plugins list with the new ones and clean them out from the shadow copy folder 
                var combinedList = plugins.Concat(GetCachePluginsList(_pluginFolder))
                    .Select(x => new FileInfo(Path.Combine(shadowCopyFolder.FullName, x.Name)))
                    .DistinctBy(x => x.FullName)
                    .ToArray();
                foreach (var f in combinedList)
                {
                    //if that fails, then we will try to remove it the hard way by renaming it to .delete if it doesn't delete nicely
                    CleanupPlugin(f);
                }
                return true;
            }

            //no plugin changes found
            return false;
        }

        /// <summary>
        /// Returns the hash string for all found plugins/folders
        /// </summary>
        /// <returns></returns>
        internal static string GetPluginsHash(IEnumerable<FileInfo> plugins, bool generateNew = false)
        {
            if (generateNew || _pluginsHash.IsNullOrWhiteSpace())
            {
                using (DisposableTimer.TraceDuration<PluginManager>("Determining hash of plugins on disk", "Hash determined"))
                {
                    var hashCombiner = new HashCodeCombiner();
                    //add each unique folder to the hash
                    foreach (var i in plugins.Select(x => x.Directory).DistinctBy(x => x.FullName))
                    {
                        hashCombiner.AddFolder(i);
                    }
                    _pluginsHash = hashCombiner.GetCombinedHashCode();
                }
            }
            return _pluginsHash;
        }

        /// <summary>
        /// Returns the hash code of the plugins last loaded
        /// </summary>
        /// <returns></returns>
        private static long GetCachePluginsHash()
        {
            var filePath = Path.Combine(_pluginFolder.FullName, "Cache", "umbraco-plugins.hash");
            if (!File.Exists(filePath))
                return 0;
            var hash = File.ReadAllText(filePath, Encoding.UTF8);
            return ConvertPluginsHash(hash);
        }

        /// <summary>
        /// Returns a list of all previously copied plugins
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<FileInfo> GetCachePluginsList(DirectoryInfo pluginFolder)
        {
            var filePath = Path.Combine(pluginFolder.FullName, "Cache", "umbraco-plugins.list");
            if (!File.Exists(filePath))
                return new List<FileInfo>();
            var list = new List<FileInfo>();
            var listFile = File.ReadAllText(filePath, Encoding.UTF8);
            var sr = new StringReader(listFile);
            while (true)
            {
                var f = sr.ReadLine();
                if (f != null && !f.IsNullOrWhiteSpace())
                {
                    list.Add(new FileInfo(f));
                }
                else
                {                    
                    break;
                }
            }
            return list;
        }

        /// <summary>
        /// Converts the hash value of current plugins to long from string
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        internal static long ConvertPluginsHash(string val)
        {
            long outVal;
            if (Int64.TryParse(val, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out outVal))
            {
                return outVal;
            }
            return 0;
        }

        /// <summary>
        /// Persists a file containing a hash of all current plugins
        /// </summary>
        private static void WriteCachePluginsHash(IEnumerable<FileInfo> plugins)
        {
            var dir = Directory.CreateDirectory(Path.Combine(_pluginFolder.FullName, "Cache"));
            File.WriteAllText(Path.Combine(dir.FullName, "umbraco-plugins.hash"), GetPluginsHash(plugins), Encoding.UTF8);
        }

        /// <summary>
        /// Writes a list of all plugins files that are being copied
        /// </summary>
        /// <param name="plugins"></param>
        private static void WriteCachePluginsList(IEnumerable<FileInfo> plugins)
        {
            var sb = new StringBuilder();
            foreach (var f in plugins)
            {
                sb.AppendLine(f.Name);
            }

            var dir = Directory.CreateDirectory(Path.Combine(_pluginFolder.FullName, "Cache"));
            File.WriteAllText(Path.Combine(dir.FullName, "umbraco-plugins.list"), sb.ToString(), Encoding.UTF8);

            //Now, write this file to the shadow copy folder for information purposes. For example, if it is default (~/bin) 
            //then at least people will have a file to reference to look at what has been shadow copied vs what is part
            //of their project.

            File.WriteAllText(Path.Combine(_shadowCopyFolder.FullName, "umbraco-plugins-list.txt"),
                "This file lists all of the Umbraco plugin DLL files that have been shadow copied to this folder.\r\n" 
                + sb.ToString(), 
                Encoding.UTF8);
        }

        private static bool IsFullTrust()
        {
            if (!_isFullTrust.HasValue)
            {
                _isFullTrust = SystemUtilities.GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted;
            }
            return _isFullTrust.Value;
        }

        private static Assembly PerformFileDeployAndRegistration(FileInfo plug, bool performShadowCopy)
        {
            if (plug.Directory.Parent == null)
                throw new InvalidOperationException("The plugin directory for the " + plug.Name +
                                                    " file exists in a folder outside of the allowed Umbraco folder heirarchy");

            FileInfo shadowCopiedPlug;
            
            Assembly shadowCopiedAssembly;
            if (!IsFullTrust())
            {             
                shadowCopiedPlug = InitializeMediumTrust(plug, _shadowCopyFolder, performShadowCopy);
                shadowCopiedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(shadowCopiedPlug.FullName));
            }
            else
            {
                shadowCopiedPlug = InitializeFullTrust(plug, _shadowCopyFolder, performShadowCopy);
                shadowCopiedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(shadowCopiedPlug.FullName));
                if (_shadowCopyType == PluginManagerShadowCopyType.UseDynamicFolder)
                {
                    //add the reference to the build manager if copying to CodeGen
                    LogHelper.TraceIfEnabled<PluginManager>("Adding to BuildManager: '{0}'", () => shadowCopiedAssembly.FullName);
                    BuildManager.AddReferencedAssembly(shadowCopiedAssembly);
                }
            }

            return shadowCopiedAssembly;
        }

        /// <summary>
        /// Used to initialize plugins when running in Full Trust
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="shadowCopyPlugFolder"></param>
        /// <param name="performShadowCopy"> </param>
        /// <returns></returns>
        private static FileInfo InitializeFullTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder, bool performShadowCopy)
        {
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));
            //if instructed to not perform the copy, just return the path to where it is supposed to be
            if (!performShadowCopy && shadowCopiedPlug.Exists)
                return shadowCopiedPlug;

            LogHelper.TraceIfEnabled<PluginManager>(plug.FullName + " to " + shadowCopyPlugFolder.FullName);

            try
            {
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            catch(UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", shadowCopiedPlug.Directory.FullName));
            }
            catch (IOException)
            {
                LogHelper.TraceIfEnabled<PluginManager>(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                //this occurs when the files are locked,
                //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy


                try
                {
                    //If all else fails during the cleanup and we cannot copy over so we need to rename with a GUID
                    var dotDeleteFile = GetNewDotDeleteName(shadowCopiedPlug);
                    File.Move(shadowCopiedPlug.FullName, dotDeleteFile);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", shadowCopiedPlug.Directory.FullName));
                }
                catch (IOException)
                {
                    LogHelper.TraceIfEnabled<PluginManager>(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin");
                    throw;
                }
                //ok, we've made it this far, now retry the shadow copy
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            return shadowCopiedPlug;
        }

        /// <summary>
        /// Used to initialize plugins when running in Medium Trust
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="shadowCopyPlugFolder"></param>
        /// <param name="performShadowCopy"> </param>
        /// <returns></returns>
        private static FileInfo InitializeMediumTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder, bool performShadowCopy)
        {
            var shouldCopy = true;
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));
            //if instructed to not perform the copy, just return the path to where it is supposed to be
            if (!performShadowCopy && shadowCopiedPlug.Exists)
                return shadowCopiedPlug;

            LogHelper.TraceIfEnabled<PluginManager>(plug.FullName + " to " + shadowCopyPlugFolder.FullName);

            //check if a shadow copied file already exists and if it does, check if its updated, if not don't copy
            if (shadowCopiedPlug.Exists)
            {
                if (shadowCopiedPlug.CreationTimeUtc.Ticks == plug.CreationTimeUtc.Ticks)
                {
                    LogHelper.TraceIfEnabled<PluginManager>("Not copying; files appear identical: '{0}'", () => shadowCopiedPlug.Name);
                    shouldCopy = false;
                }
            }

            if (shouldCopy)
            {
                try
                {
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", shadowCopiedPlug.Directory.FullName));
                }
                catch (IOException)
                {
                    LogHelper.TraceIfEnabled<PluginManager>(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                    //this occurs when the files are locked,
                    //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                    //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                    try
                    {
                        var dotDeleteFile = GetNewDotDeleteName(shadowCopiedPlug);
                        File.Move(shadowCopiedPlug.FullName, dotDeleteFile);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", shadowCopiedPlug.Directory.FullName));
                    }
                    catch (IOException)
                    {
                        LogHelper.TraceIfEnabled<PluginManager>(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin");
                        throw;
                    }
                    try
                    {
                        //ok, we've made it this far, now retry the shadow copy
                        File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", shadowCopiedPlug.Directory.FullName));
                    }
                }
            }

            return shadowCopiedPlug;
        }

        /// <summary>
        /// Returns the package folder for the plugin DLL passed in
        /// </summary>
        /// <param name="pluginDll"></param>
        /// <returns>An empty string if the plugin was not found in a pckage folder</returns>
        private static string GetPackageFolderFromPluginDll(FileInfo pluginDll)
        {
            if (!IsPackageLibFolder(pluginDll.Directory))
            {
                throw new DirectoryNotFoundException("The file specified does not exist in the lib folder for a package");
            }
            //we know this folder structure is correct now so return the directory. parent  \bin\..\{PackageName}
            return pluginDll.Directory.Parent.FullName;
        }

        /// <summary>
        /// Determines if the folder is a bin plugin folder for a package
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        internal static bool IsPackagePluginFolder(DirectoryInfo folder)
        {
            if (folder.Parent == null) return false;
            if (!folder.Parent.Name.InvariantEquals(PackagesFolderName)) return false;
            return true;
            //return PackageFolderNameHasId(folder.Parent.Name);
        }

        /// <summary>
        /// Determines if the folder is a bin plugin folder for a package or for the core package
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static bool IsPackageLibFolder(DirectoryInfo folder)
        {
            if (!folder.Name.InvariantEquals("lib")) return false;
            if (folder.Parent == null) return false;
            return IsPackagePluginFolder(folder.Parent) || (folder.Parent != null && folder.Parent.Name == "Core");
        }

        ///// <summary>
        ///// Determines if the folder is a bin plugin folder for a package
        ///// </summary>
        ///// <param name="folderPath"></param>
        ///// <returns></returns>
        //internal static bool IsPackageLibFolder(string folderPath)
        //{
        //    return IsPackageLibFolder(new DirectoryInfo(folderPath));
        //}



    }
}
