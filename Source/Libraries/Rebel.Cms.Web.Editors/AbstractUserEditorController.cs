using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.Controllers;
using Rebel.Framework;
using Rebel.Framework.Localization;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;
using Rebel.Framework.Persistence;

namespace Rebel.Cms.Web.Editors
{
    public abstract class AbstractUserEditorController<TUserType, TEditorModel> : StandardEditorController
        where TUserType : Profile, IMembershipUser, new()
        where TEditorModel : MembershipUserEditorModel, new()
    {
        protected AbstractUserEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        public abstract IMembershipService<TUserType> MembershipService { get; }

        public abstract string GroupProviderGroupRoot { get; }

        public abstract HiveId ProfileVirtualRoot { get; }

        public abstract string CreateNewTitle { get; }

        #region Actions

        /// <summary>
        /// Displays the Create user editor 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[RebelAuthorize(Permissions = new[] { FixedPermissionIds.Create })]
        public virtual ActionResult CreateNew(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            //store the parent document type to the view
            var model = new CreateUserModel { ParentId = id.Value };
            return CreateNewView(model);
        }

        /// <summary>
        /// Creates a new user based on posted values
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CreateNew")]
        [Save]
        //[RebelAuthorize(Permissions = new[] { FixedPermissionIds.Create }, IdParameterName = "ParentId")]
        [SupportsPathGeneration]
        public ActionResult CreateNewForm(CreateUserModel createModel)
        {
            Mandate.ParameterNotNull(createModel, "createModel");
            Mandate.ParameterNotEmpty(createModel.ParentId, "createModel.ParentId");
            Mandate.ParameterNotEmpty(createModel.SelectedDocumentTypeId, "createModel.SelectedDocumentTypeId");

            //validate the model
            TryUpdateModel(createModel);
            //get the create new result view which will validate that the selected doc type id is in fact allowed
            var result = CreateNewView(createModel);
            //if at this point the model state is invalid, return the result which is the CreateNew view
            if (!ModelState.IsValid)
            {
                return result;
            }

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IContentStore>())
            {
                var schema = uow.Repositories.Schemas.GetComposite<EntitySchema>(createModel.SelectedDocumentTypeId);
                if (schema == null)
                    throw new ArgumentException(string.Format("No schema found for id: {0} on action Create", createModel.SelectedDocumentTypeId));

                //create the empty content item
                var userViewModel = CreateNewUserEntity(schema, createModel.Name, createModel.Username, createModel.Email, createModel.Password, createModel.ParentId);

                //map the Ids correctly to the model so it binds
                ReconstructModelPropertyIds(userViewModel);

                return ProcessSubmit(createModel, userViewModel, null);
                //return View(createModel);
            }
        }

        /// <summary>
        /// Action to render the editor
        /// </summary>
        /// <returns></returns>        
        public override ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            var user = MembershipService.GetById(id.Value);

            if (user == null)
                throw new ArgumentException(string.Format("No user found for id: {0} on action Edit", id));

            var userViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TUserType, TEditorModel>(user);

            EnsureViewBagData();

            return View(userViewModel);
        }

        /// <summary>
        /// Handles the editor post back
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [ActionName("Edit")]
        [HttpPost]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [PersistTabIndexOnRedirect]
        [Save]
        public ActionResult EditForm(HiveId? id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            var user = MembershipService.GetById(id.Value);

            if (user == null)
                throw new ArgumentException(string.Format("No entity for id: {0} on action EditForm", id));

            var userViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TUserType, TEditorModel>(user);

            //need to ensure that all of the Ids are mapped correctly, when editing existing content the only reason for this
            //is to ensure any new document type properties that have been created are reflected in the new content revision
            ReconstructModelPropertyIds(userViewModel);

            EnsureViewBagData();

            return ProcessSubmit(null, userViewModel, user);
        }
        
        /// <summary>
        /// JSON action to delete a node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        //[RebelAuthorize(Permissions = new[] { FixedPermissionIds.Delete })]
        public virtual JsonResult Delete(HiveId? id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            var result = MembershipService.Delete(id.Value, true);

            if(result)
            {
                Notifications.Add(new NotificationMessage("Delete.Successful".Localize(this), NotificationType.Success));
                var obj = new { message = "Success", notifications = Notifications };
                return new CustomJsonResult(() => obj.ToJsonString());
            }
            else
            {
                Notifications.Add(new NotificationMessage("Delete.Failed".Localize(this), NotificationType.Error));
                var obj = new { message = "Failed", notifications = Notifications };
                return new CustomJsonResult(() => obj.ToJsonString());
            }
        }

        /// <summary>
        /// Processes the submit.
        /// </summary>
        /// <param name="createModel">The create model.</param>
        /// <param name="editModel">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected ActionResult ProcessSubmit(CreateUserModel createModel, TEditorModel editModel, TUserType entity)
        {
            Mandate.ParameterNotNull(editModel, "model");

            var isNew = entity == null;

            // Clear out any previous groups before mapping, as we just want whatever is selected now
            editModel.Groups = Enumerable.Empty<HiveId>();

            //bind it's data
            editModel.BindModel(this);

            //if there's model errors, return the view
            if (!ModelState.IsValid)
            {
                AddValidationErrorsNotification();

                EnsureViewBagData();

                return isNew ? View("CreateNew", createModel) : View("Edit", editModel);
            }

            //persist the data
            if (isNew)
            {
                editModel.Id = HiveId.Empty;
                editModel.LastPasswordChangeDate = DateTime.UtcNow;
                editModel.LastActivityDate = DateTime.UtcNow;
                editModel.LastLoginDate = DateTime.UtcNow;

                entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TEditorModel, TUserType>(editModel);

                MembershipCreateStatus status;
                entity = MembershipService.Create(entity, out status);

                if(status != MembershipCreateStatus.Success)
                {
                    ModelState.AddModelError("", status.Localize());

                    EnsureViewBagData();

                    return View("CreateNew", createModel);
                }
            }
            else
            {
                BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(editModel, entity);

                try
                {
                    MembershipService.Update(entity);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);

                    AddValidationErrorsNotification();

                    EnsureViewBagData();

                    return View("Edit", editModel);
                }
            }

            OnAfterSave(entity);

            //TODO: Need to display the right messages
            Notifications.Add(new NotificationMessage(
                   "User.Save.Message".Localize(this),
                   "User.Save.Title".Localize(this),
                   NotificationType.Success));

            GeneratePathsForCurrentEntity(entity);

            return RedirectToAction("Edit", new { id = entity.Id });

        }

        protected abstract void GeneratePathsForCurrentEntity(TUserType entity);

        protected virtual void OnAfterSave(TUserType entity)
        {
            // To be overriden...
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Ensures the view bag data.
        /// </summary>
        protected virtual void EnsureViewBagData() { }

        /// <summary>
        /// Creates a blank user model based on the document type/entityschema for the user
        /// </summary>
        /// <returns></returns>
        protected virtual TEditorModel CreateNewEditorModel()
        {
            var user = new TEditorModel();
            var editorModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TEditorModel>(user);
            return editorModel;
        }
        
        /// <summary>
        /// This method is used for creating new content and for updating an existing entity with new attributes that may have been
        /// created on the document type (attribution schema).
        /// </summary>
        /// <param name="contentViewModel"></param>
        /// <remarks>
        /// We need to paste the model back together now with the correct Ids since we generated them on the last Action and we've re-generated them again now.
        /// This is done by looking up the Alias Key/Value pair in the posted form element for each of the model's properties. When the key value pair is found
        /// we can extract the id that was assigned to it in the HTML markup and re-assign that id to the actual property id so it binds.
        /// </remarks>
        protected void ReconstructModelPropertyIds(BasicContentEditorModel contentViewModel)
        {
            foreach (var p in contentViewModel.Properties)
            {
                var alias = p.Alias;
                //find the alias form post key/value pair that matches this alias
                var aliasKey = (Request.Form.AllKeys
                    .Where(x => x.EndsWith("__Alias__")
                                && x.StartsWith(HiveIdExtensions.HtmlIdPrefix))
                    .Where(key => Request.Form[key] == alias)).SingleOrDefault();
                if (aliasKey != null)
                {
                    //now we can extract the ID that this property was related to
                    var originalHiveId = HiveIdExtensions.TryParseFromHtmlId(aliasKey); //HiveId.TryParse(aliasKey.Split('.')[0].Split('_')[1]);)
                    if (originalHiveId.Success)
                    {
                        //find the new editor model property with the attribute definition so we can update it's Ids to the original Ids so they bind
                        var contentProperty = contentViewModel.Properties.Where(x => x.Alias == alias).Single();
                        //update the property's ID to the originally generated id
                        contentProperty.Id = originalHiveId.Result;
                    }
                }

                //if it was null it means we can't reconstruct or there's been no property data passed in
            }
        }

        /// <summary>
        /// Creates a new TEditorModel based on the persisted doc type
        /// </summary>
        /// <param name="docTypeData">The doc type data.</param>
        /// <param name="name">The name.</param>
        /// <param name="username">The username.</param>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="parentId">The parent id.</param>
        /// <returns></returns>
        protected virtual TEditorModel CreateNewUserEntity(EntitySchema docTypeData, string name, string username, string email, string password, HiveId parentId)
        {
            Mandate.ParameterNotNull(docTypeData, "docTypeData");
            Mandate.ParameterNotEmpty(parentId, "parentId");

            var user = new TUserType();
            user.SetupFromSchema(docTypeData);

            //get doc type model
            //var docType = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(docTypeData);
            //map (create) content model from doc type model
            var contentModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TUserType, TEditorModel>(user);
            contentModel.ParentId = parentId;
            contentModel.Name = name;
            contentModel.Username = username;
            contentModel.Email = email;
            contentModel.Password = contentModel.ConfirmPassword = password;
            contentModel.IsApproved = true;
            return contentModel;
        }

        /// <summary>
        /// Ensures the create wizard view bag data.
        /// </summary>
        protected virtual void EnsureCreateWizardViewBagData()
        {
            ViewBag.Title = CreateNewTitle;
            ViewBag.ControllerId = RebelController.GetControllerId<EditorAttribute>(GetType());
        }

        /// <summary>
        /// Returns the ActionResult for the CreateNew wizard view
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual ActionResult CreateNewView(CreateUserModel model)
        {
            Mandate.ParameterNotNull(model, "model");

            //lookup the doc type for the node id, find out which doc type children are allowed

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IContentStore>())
            {
                var allSchemaTypeIds = uow.Repositories.Schemas.GetDescendentRelations(ProfileVirtualRoot, FixedRelationTypes.DefaultRelationType)
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();

                var schemas = uow.Repositories.Schemas.Get<EntitySchema>(true, allSchemaTypeIds);

                //the filtered doc types to choose from based on the parent node (default is all of them)
                var docTypesInfo = schemas.Where(x => !x.IsAbstract)
                .Select(BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeInfo>);

                var availableProfileTypes = new List<SelectListItem>{new SelectListItem
                {
                    Text = "None",
                    Value = ProfileVirtualRoot.ToString()
                }};

                availableProfileTypes.AddRange(docTypesInfo.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = x.Id == model.SelectedDocumentTypeId
                }));

                model.AvailableProfileTypes = availableProfileTypes;

                EnsureCreateWizardViewBagData();
            }

            return View("CreateNew", model);
        }

        #endregion
    }
}
