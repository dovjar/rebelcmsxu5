﻿using AutoMapper;
using Rebel.Framework.Persistence.DtoModel.AutoMapped;
using RevisionData = Rebel.Framework.Persistence.Model.Versioning.RevisionData;

namespace Rebel.Tests.DomainDesign.PersistenceProviders.NHibernate
{
	public class RevisionResolver : ValueResolver<PersistenceEntityDto, RevisionData>
	{
		#region Overrides of ValueResolver<IRevisionData,RevisionData>

		protected override RevisionData ResolveCore(PersistenceEntityDto source)
		{
			return Mapper.Map<Framework.Persistence.DtoModel.AutoMapped.Versioning.RevisionData, RevisionData>(source.Revision);
		}

		#endregion
	}
}