﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;

namespace Rebel.Framework.Persistence.Model.Constants
{
    public class RootEntitySchema : EntitySchema
    {
        public RootEntitySchema()
        {
            this.Setup("no-attributes", "System doctype: Root contains no attributes");
            this.Id = FixedHiveIds.RootSchema;
            SchemaType = FixedSchemaTypes.SystemRoot;
        }
    }
}
