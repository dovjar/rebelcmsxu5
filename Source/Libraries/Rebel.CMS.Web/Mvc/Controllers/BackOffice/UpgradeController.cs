using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using Rebel.Cms.Web.Context;
using System.Web.Security;
using FixedEntities = Rebel.Framework.Security.Model.FixedEntities;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Mvc.Controllers.BackOffice
{
    public class UpgradeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Upgrade()
        {
            var hiveManager = DependencyResolver.Current.GetService<IHiveManager>();
            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            var membershipService = appContext.Security.Users;
            
            //----------------------------------------
            // Upgrade users
            //----------------------------------------
            if (!UpgradeHelper.UsersUpgraded())
            {
                // Create core data
                using (var coreUow = hiveManager.OpenWriter<IContentStore>())
                {
                    // Create root nodes
                    coreUow.Repositories.AddOrUpdate(FixedEntities.UserProfileVirtualRoot);
                    coreUow.Repositories.AddOrUpdate(FixedEntities.MemberVirtualRoot);
                    coreUow.Repositories.AddOrUpdate(FixedEntities.MemberProfileVirtualRoot);
                    coreUow.Repositories.AddOrUpdate(FixedEntities.MemberGroupVirtualRoot);

                    // Create new schemas (MembershipUserSchema will actually overwrite the old UserSchema)
                    coreUow.Repositories.Schemas.AddOrUpdate(
                        Rebel.Framework.Security.Model.FixedSchemas.MembershipUserSchema);
                    coreUow.Repositories.Schemas.AddOrUpdate(
                        Rebel.Framework.Security.Model.FixedSchemas.UserProfileSchema);
                    coreUow.Repositories.Schemas.AddOrUpdate(
                        Rebel.Framework.Security.Model.FixedSchemas.MemberProfileSchema);

                    coreUow.Complete();
                }

                // Update users
                using (var userUow = hiveManager.OpenReader<ISecurityStore>(new Uri("security://users")))
                using (var userGroupUow = hiveManager.OpenReader<ISecurityStore>(new Uri("security://user-groups")))
                {
                    var users = userUow.Repositories.Where(
                        x =>
                        x.EntitySchema.Alias == "system-user");

                    foreach (var userEntity in users)
                    {
                        MembershipCreateStatus status;
                        var newUser = new User
                                          {
                                              Name =
                                                  userEntity.Attribute<string>(NodeNameAttributeDefinition.AliasValue),
                                              Username = userEntity.Attribute<string>("username"),
                                              Email = userEntity.Attribute<string>("email"),
                                              Password = userEntity.Attribute<string>("password"),
                                              PasswordQuestion = userEntity.Attribute<string>("passwordQuestion"),
                                              PasswordAnswer = userEntity.Attribute<string>("passwordAnswer"),
                                              IsApproved = userEntity.Attribute<bool>("isApproved"),
                                          };

                        foreach (var attr in userEntity.Attributes)
                        {
                            if (
                                newUser.EntitySchema.AttributeDefinitions.Any(
                                    x => x.Alias == attr.AttributeDefinition.Alias))
                            {
                                if (
                                    newUser.Attributes.Any(
                                        x => x.AttributeDefinition.Alias == attr.AttributeDefinition.Alias))
                                {
                                    foreach (var key in attr.Values.Keys)
                                    {
                                        if (newUser.Attributes[attr.AttributeDefinition.Alias].Values.ContainsKey(key))
                                        {
                                            newUser.Attributes[attr.AttributeDefinition.Alias].Values[key] =
                                                attr.Values[key];
                                        }
                                        else
                                        {
                                            newUser.Attributes[attr.AttributeDefinition.Alias].Values.Add(key,
                                                                                                          attr.Values[
                                                                                                              key]);
                                        }
                                    }
                                }
                                else
                                {
                                    newUser.Attributes.Add(attr);
                                }
                            }
                        }

                        newUser.Groups =
                            userGroupUow.Repositories.GetParentRelations(userEntity.Id,
                                                                         FixedRelationTypes.UserGroupRelationType)
                                .Select(x => x.SourceId).ToArray();

                        membershipService.Create(newUser, out status);
                    }
                }

                // Cleanup old user data
                using (var coreUow = hiveManager.OpenWriter<IContentStore>())
                {
                    var users = coreUow.Repositories.Where(
                        x =>
                        x.EntitySchema.Alias == "system-user");

                    foreach (var user in users)
                        coreUow.Repositories.Delete<TypedEntity>(user.Id);

                    coreUow.Repositories.Schemas.Delete<EntitySchema>(FixedHiveIds.RTMUserSchema);
                    coreUow.Complete();
                }
            }


            //----------------------------------------
            // Move files 
            //----------------------------------------

            try
            {
                // ~/Views/Rebel/MacroPartials > ~/Views/MacroPartials
                MoveDirectory(Server.MapPath("~/Views/Rebel/MacroPartials"), Server.MapPath("~/Views/MacroPartials"));
            }
            catch (IOException)
            { }

            try
            {
                // ~/Views/Rebel/Partial > ~/Views/Partials
                MoveDirectory(Server.MapPath("~/Views/Rebel/Partial"), Server.MapPath("~/Views/Partials"));
            }
            catch (IOException)
            { }

            try
            {
                // ~/Views/Rebel/*.cshtml > ~/Views
                var files = Directory.GetFiles(Server.MapPath("~/Views/Rebel"), "*.cshtml", SearchOption.TopDirectoryOnly);
                foreach (var fileInfo in files.Select(file => new FileInfo(file)))
                {
                    var newFileInfo = new FileInfo(Server.MapPath("~/Views") + "/" + fileInfo.Name);
                    if(!newFileInfo.Exists)
                        global::System.IO.File.Move(fileInfo.FullName, newFileInfo.FullName);
                }
            }
            catch (IOException)
            { }


            //----------------------------------------
            // Fix broken schemas
            //----------------------------------------

            using (var coreUow = hiveManager.OpenWriter<IContentStore>())
            {
                // Fix image schema linking to wrong uploader attribute type
                var imageSchema = coreUow.Repositories.Schemas.Get<EntitySchema>(Framework.Persistence.Model.Constants.FixedHiveIds.MediaImageSchema);
                imageSchema.AttributeDefinitions[MediaImageSchema.UploadFileAlias].AttributeType = AttributeTypeRegistry.Current.GetAttributeType("uploader");
                imageSchema.SetXmlConfigProperty("icon", "tree-media-photo");
                coreUow.Repositories.Schemas.AddOrUpdate(imageSchema);

                coreUow.Complete();
            }

            return RedirectToAction("Success");
        }

        public ActionResult Success()
        {
            return View();
        }

        #region Helper Methods

        private void MoveDirectory(string source, string target, bool overrite = false)
        {
            var stack = new Stack<KeyValuePair<string, string>>();
            stack.Push(new KeyValuePair<string, string>(source, target));

            while (stack.Count > 0)
            {
                var folders = stack.Pop();
                Directory.CreateDirectory(folders.Value);
                foreach (var file in Directory.GetFiles(folders.Key, "*.*"))
                {
                    string targetFile = Path.Combine(folders.Value, Path.GetFileName(file));
                    if (global::System.IO.File.Exists(targetFile))
                    {
                        if (overrite)
                        {
                            global::System.IO.File.Delete(targetFile);
                            global::System.IO.File.Move(file, targetFile);
                        }
                    }
                    else
                    {
                        global::System.IO.File.Move(file, targetFile);
                    }
                }

                foreach (var folder in Directory.GetDirectories(folders.Key))
                {
                    stack.Push(new KeyValuePair<string, string>(folder, Path.Combine(folders.Value, Path.GetFileName(folder))));
                }
            }
            Directory.Delete(source, true);
        }

        #endregion
    }
}
