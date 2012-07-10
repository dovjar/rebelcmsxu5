using System.Collections.Generic;
using System.Linq;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Persistence.ModelFirst.Annotations;
using Rebel.Framework.Security.Model.Schemas;

namespace Rebel.Framework.Security.Model.Entities
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            this.SetupFromSchema<MasterUserProfileSchema>();

            SessionTimeout = 60;
            StartContentHiveId = HiveId.Empty;
            StartMediaHiveId = HiveId.Empty;
            Applications = Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets or sets the session timeout.
        /// </summary>
        /// <value>
        /// The session timeout.
        /// </value>
        [AttributeAlias(Alias = MasterUserProfileSchema.SessionTimeoutAlias)]
        public int SessionTimeout
        {
            get { return base.BaseAutoGet(MasterUserProfileSchema.SessionTimeoutAlias, 60); }
            set { base.BaseAutoSet(MasterUserProfileSchema.SessionTimeoutAlias, value); }
        }

        /// <summary>
        /// Gets or sets the start content hive id.
        /// </summary>
        /// <value>
        /// The start content hive id.
        /// </value>
        [AttributeAlias(Alias = MasterUserProfileSchema.StartContentHiveIdAlias)]
        public HiveId StartContentHiveId
        {
            get { return base.BaseAutoGet(MasterUserProfileSchema.StartContentHiveIdAlias, HiveId.Empty); }
            set { base.BaseAutoSet(MasterUserProfileSchema.StartContentHiveIdAlias, value); }
        }

        /// <summary>
        /// Gets or sets the start media hive id.
        /// </summary>
        /// <value>
        /// The start media hive id.
        /// </value>
        [AttributeAlias(Alias = MasterUserProfileSchema.StartMediaHiveIdAlias)]
        public HiveId StartMediaHiveId
        {
            get { return base.BaseAutoGet(MasterUserProfileSchema.StartMediaHiveIdAlias, HiveId.Empty); }
            set { base.BaseAutoSet(MasterUserProfileSchema.StartMediaHiveIdAlias, value); }
        }

        /// <summary>
        /// Gets or sets the applications.
        /// </summary>
        /// <value>
        /// The applications.
        /// </value>
        public IEnumerable<string> Applications
        {
            get
            {
                return Attributes[MasterUserProfileSchema.ApplicationsAlias].Values.Select(x => x.Value.ToString()).ToList();
            }
            set
            {
                Attributes[MasterUserProfileSchema.ApplicationsAlias].Values.Clear();
                var count = 0;
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        Attributes[MasterUserProfileSchema.ApplicationsAlias].Values.Add("val" + count, item);
                        count++;
                    }
                }
            }
        }
    }
}
