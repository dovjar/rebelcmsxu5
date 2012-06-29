using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Persistence.Model.IO
{

    public class File : TypedEntity
    {
        public File()
        {
            this.SetupFromSchema<FileSchema>();
            IsContainer = false;
        }

        public File(TypedEntity fromEntity)
        {
            this.SetupFromEntity(fromEntity);
        }

        /// <summary>
        /// Creates a new file object with string contents. <paramref name="textContent"/> will be encoded as UTF32.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="textContent"></param>
        public File(string fileName, string textContent)
            : this()
        {
            Mandate.ParameterNotNullOrEmpty(fileName, "fileName");
            if (textContent == null) textContent = string.Empty;
            Name = fileName;
            ContentBytes = Encoding.UTF32.GetBytes(textContent);
        }

        /// <summary>
        /// Creates a new file object with byte contents.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentBytes"></param>
        public File(string fileName, byte[] contentBytes)
            : this()
        {
            Mandate.ParameterNotNullOrEmpty(fileName, "fileName");
            Name = fileName;
            ContentBytes = contentBytes;
        }

        public File(HiveId id)
            : this()
        {
            Id = id;
        }

        public string Name
        {
            get { return this.Attribute<string>("name"); }
            set
            {                
                if (!RootedPath.IsNullOrWhiteSpace() && Path.GetFileName(RootedPath) != value)
                {
                    //if the locatin is set, then we need to update it too
                    var rootLocation = RootedPath.Substring(0, RootedPath.LastIndexOf(Name));
                    Attributes["rootedPath"].SetDefaultValue(Path.Combine(rootLocation, value));
                }
                Attributes["name"].SetDefaultValue(value);
            }
        }

        public bool IsContainer
        {
            get { return this.Attribute("isContainer", false); }
            set { Attributes["isContainer"].SetDefaultValue(value); }
        }

        public string RootedPath
        {
            get { return this.Attribute<string>("rootedPath"); }
            set
            {
                Attributes["rootedPath"].SetDefaultValue(value);
                
                //as we are changing the location the name will need to be updated as well
                var fileName = Path.GetFileName(value);
                if (Name != fileName)
                {
                    Attributes["name"].SetDefaultValue(fileName);
                }
            }
        }

        public string RootRelativePath
        {
            get { return this.Attribute<string>("rootRelativePath"); }
            set { Attributes["rootRelativePath"].SetDefaultValue(value); }
        }

        public string PublicUrl
        {
            get { return this.Attribute<string>("publicUrl"); }
            set { Attributes["publicUrl"].SetDefaultValue(value); }
        }

        public byte[] ContentBytes
        {
            get
            {
                var content = (byte[])Attributes["contentBytes"].GetDefaultValue();
                if (content == null)
                {
                    if (ContentStreamFactory == null)
                        return new byte[0];
                        // throw new InvalidOperationException("The ContentStream property has not been set, therefore the Content bytes cannot be accessed");
                    using (var streamValue = ContentStreamFactory.Invoke(this))
                    {
                        ContentBytes = content = streamValue.ReadAllBytes().StripUTF8BOMs();
                    }
                }
                return content;
            }
            set
            {
                if (IsContainer)
                    throw new InvalidOperationException(
                        string.Format(
                            "Entity '{0}' is a container and hence cannot have content assigned to it. To set content ensure that the IsContainer property is false.",
                            Id));

                Attributes["contentBytes"].SetDefaultValue(value);
            }
        }

        protected Func<File, Stream> ContentStreamFactory { get; private set; }

        public void SetContentStreamFactory(Func<File, Stream> streamFactory)
        {
            ContentStreamFactory = streamFactory;
        }
    }
}
