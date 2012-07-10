using Rebel.Framework.Persistence.Model.Constants.Entities;

namespace Rebel.Framework.Persistence.Model.Constants
{
    public static class FixedEntities
    {
        public static SubContentRoot MediaVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.MediaVirtualRoot);
            }
        }

        public static SubContentRoot MediaRecycleBin
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.MediaRecylceBin);
            }
        }

        public static SubContentRoot ContentVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.ContentVirtualRoot);
            }
        }

        public static SubContentRoot ContentRecycleBin
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.ContentRecylceBin);
            }
        }

        public static SubContentRoot DictionaryVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.DictionaryVirtualRoot);
            }
        }
    }
}