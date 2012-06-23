using System;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Mvc.ModelBinders.BackOffice
{
    /// <summary>
    /// Model binder for DocumentTypeProperty
    /// </summary>
    [ModelBinderFor(typeof (DocumentTypeProperty))]
    public class DocumentTypePropertyModelBinder : StandardModelBinder
    {
        private readonly IRebelCmsApplicationContext _applicationContext;

        public DocumentTypePropertyModelBinder(IRebelCmsApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// Ensures that the DataType property of the model is set correctly.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// The DataType property may not be null, in which case we need to ensure that the DataTypeId and DataType.Id are the same, if they are
        /// not, it means the editor changed the DataType so we need to go look it up by the new Id and re-map. Otherwise if it is null, this generally
        /// means that its a new property so we need to lookup the DataType by the DataTypeId and re-map.
        /// </remarks>
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var docTypeProperty = (DocumentTypeProperty)base.BindModel(controllerContext, bindingContext);
            if (docTypeProperty == null) 
                return null;

            //now that all properties are bound normally, we can check the DataType property
            var dataType = docTypeProperty.DataType;
            if (!docTypeProperty.DataTypeId.IsNullValueOrEmpty() && (dataType == null || dataType.Id != docTypeProperty.DataTypeId))
            {
                //we need to lookup the data type
                using (var uow = _applicationContext.Hive.OpenReader<IContentStore>())
                {
                    var attributeType = uow.Repositories.Schemas.Get<AttributeType>(docTypeProperty.DataTypeId);
                    if (attributeType == null)
                        throw new InvalidOperationException("Cannot set a document type property's DataTypeId to an Id that does not exist for an AttributeType in the repository");
                    docTypeProperty.DataType = _applicationContext.FrameworkContext.TypeMappers.Map<DataType>(attributeType);
                }
            }

            return docTypeProperty;
        }

    }
}