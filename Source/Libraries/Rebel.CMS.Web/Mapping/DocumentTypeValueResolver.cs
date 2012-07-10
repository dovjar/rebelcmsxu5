using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Rebel.Cms.Domain.BackOffice.Editors;
using Rebel.Framework.Persistence.Model;

namespace Rebel.Cms.Domain.Mapping
{
    public class DocumentTypeValueResolver : ValueResolver<TypedPersistenceEntity, DocumentType>
    {
        private readonly IEnumerable<DocumentType> _docTypes;

        #region Overrides of ValueResolver<HiveEntityUri,AttributeTypeDefinition>

        public DocumentTypeValueResolver(IEnumerable<DocumentType> docTypes)
        {
            _docTypes = docTypes;
        }

        protected override DocumentType ResolveCore(TypedPersistenceEntity source)
        {
            return _docTypes.FirstOrDefault(x => x.Id == source.AttributionSchema.Id);
        }

        #endregion
    }
}