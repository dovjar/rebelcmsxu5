using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Rebel.Cms.Web.Model;
using Rebel.Framework;
using Rebel.Framework.Diagnostics;
using Rebel.Framework.Dynamics;
using Rebel.Framework.Dynamics.Attributes;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;
using Encoder = System.Drawing.Imaging.Encoder;
using File = Rebel.Framework.Persistence.Model.IO.File;

namespace Rebel.Cms.Web
{
    [DynamicExtensions]
    public static class ContentExtensions
    {
        /// <summary>
        /// Gets the media URL.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static string GetMediaUrl(this Content content)
        {
            return GetUrlHelper().GetMediaUrl(content.Id);
        }

        /// <summary>
        /// Gets the media URL.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static string GetMediaUrl(this Content content, int size)
        {
            return GetUrlHelper().GetMediaUrl(content.Id, size);
        }

        /// <summary>
        /// Gets the media URL.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns></returns>
        [DynamicExtension]
        [DynamicFieldExtension(CorePluginConstants.FileUploadPropertyEditorId, "Url")]
        public static string GetMediaUrl(this Content content, string propertyAlias)
        {
            return GetUrlHelper().GetMediaUrl(content.Id, propertyAlias);
        }

        /// <summary>
        /// Gets the media URL.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        [DynamicExtension]
        [DynamicFieldExtension(CorePluginConstants.FileUploadPropertyEditorId, "Url")]
        public static string GetMediaUrl(this Content content, string propertyAlias, int size)
        {
            return GetUrlHelper().GetMediaUrl(content.Id, propertyAlias, size);
        }

        /// <summary>
        /// Tries to swap the template of a Content item to the alt template with the supplied alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="altTemplateAlias">The alt template alias.</param>
        /// <returns></returns>
        public static bool TrySwapTemplate(this Content content, string altTemplateAlias)
        {
            if (!String.IsNullOrWhiteSpace(altTemplateAlias))
            {
                var altTemplate = content.AlternativeTemplates.SingleOrDefault(x => x.Alias != null && x.Alias.Equals(altTemplateAlias, StringComparison.InvariantCultureIgnoreCase));
                if (altTemplate != null)
                {
                    content.CurrentTemplate = altTemplate;
                    return true;
                }
            }

            return false;
        }

        internal static IDictionary<string, object> WriteUploadedFile(Guid mediaId, bool removeExistingFile, HttpPostedFileBase httpFile, IGroupUnitFactory<IFileStore> groupUnitFactory, HiveId existingFileId = default(HiveId), string thumbSizes = null)
        {
            var val = new Dictionary<string, object>();

            //add the media id to be saved
            val.Add("MediaId", mediaId.ToString("N"));

            // Check to see if we should delete the current file
            // either becuase remove file is checked, or we have a replacement file
            if (existingFileId != HiveId.Empty && (removeExistingFile || HasFile(httpFile)))
            {
                if (!existingFileId.IsNullValueOrEmpty())
                {
                    // delete entire property folder (deletes image and any thumbnails stored)
                    //var folderHiveId = HiveId.Parse("storage://file-uploader/string/" + MediaId.ToString("N"));
                    var folderHiveId = new HiveId("storage", "file-uploader", new HiveIdValue(mediaId.ToString("N")));

                    using (var uow = groupUnitFactory.Create())
                    {
                        try
                        {
                            uow.Repositories.Delete<File>(existingFileId); // Must delete file entity so that relations are deleted
                            uow.Repositories.Delete<File>(folderHiveId);
                            uow.Complete();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Warn(typeof(ContentExtensions), "Could not delete previous file and/or container", ex);
                        }
                    }
                }
            }

            // If we've received a File from the binding, we need to save it
            if (HasFile(httpFile))
            {
                // Open a new unit of work to write the file
                using (var uow = groupUnitFactory.Create())
                {
                    // Create main file
                    var file = new File
                                   {
                                       RootedPath =
                                           mediaId.ToString("N") + "/" + Path.GetFileName(httpFile.FileName)
                                                .Replace(" ", "").Replace(",", "")
                                   };

                    var stream = httpFile.InputStream;
                    if (stream.CanRead && stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        using (var mem = new MemoryStream())
                        {
                            stream.CopyTo(mem);
                            file.ContentBytes = mem.ToArray();
                        }
                    }

                    uow.Repositories.AddOrUpdate(file);

                    // Create thumbnails (TODO: Need to encapsulate this so it can be reused in other places?)
                    CreateThumbnails(uow, file, mediaId.ToString("N"), thumbSizes);

                    uow.Complete();

                    val.Add("Value", file.Id);
                }
            }
            else if (!existingFileId.IsNullValueOrEmpty() && !removeExistingFile)
            {
                val.Add("Value", existingFileId);
            }
            else
            {
                val.Add("Value", HiveId.Empty);
            }

            return val;
        }

        internal static bool HasFile(HttpPostedFileBase httpPostedFileBase)
        {
            return httpPostedFileBase != null && httpPostedFileBase.ContentLength > 0 && !String.IsNullOrEmpty(httpPostedFileBase.FileName);
        }

        internal static void CreateThumbnails(IGroupUnit<IFileStore> uow, File original, string mediaId, string thumbSizes = null)
        {
            if (original.IsImage())
            {
                var img = Image.FromStream(new MemoryStream(original.ContentBytes));

                // Create default thumbnail
                CreateThumbnail(uow, original, img, mediaId, 100);

                // Create additional thumbnails
                if (!String.IsNullOrEmpty(thumbSizes))
                {
                    var sizes = thumbSizes.Split(',');
                    foreach (var size in sizes)
                    {
                        var intSize = 0;
                        if (Int32.TryParse(size, out intSize))
                        {
                            CreateThumbnail(uow, original, img, mediaId, intSize);
                        }
                    }
                }
            }
        }

        internal static void CreateThumbnail(IGroupUnit<IFileStore> uow, File original, Image image, string mediaId, int maxWidthHeight)
        {
            var extension = Path.GetExtension(original.Name).ToLower();
            var thumbFileName = Path.GetFileNameWithoutExtension(original.Name) + "_" + maxWidthHeight + extension;

            // Create file entity
            var thumb = new File
                            {
                                RootedPath = mediaId + "/" + thumbFileName             
                            };

            // Resize image
            var val = (float)image.Width / (float)maxWidthHeight;
            var val2 = (float)image.Height / (float)maxWidthHeight;

            var num = Math.Max(val, val2);

            var num2 = (int)Math.Round((double)((float)image.Width / num));
            var num3 = (int)Math.Round((double)((float)image.Height / num));

            if (num2 == 0)
            {
                num2 = 1;
            }

            if (num3 == 0)
            {
                num3 = 1;
            }

            using(var bitmap = new Bitmap(num2, num3))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var destRect = new Rectangle(0, 0, num2, num3);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

                var imageEncoders = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo encoder = null;
                if (extension == ".png" || extension == ".gif")
                    encoder = imageEncoders.Single(t => t.MimeType.Equals("image/png"));
                else
                    encoder = imageEncoders.Single(t => t.MimeType.Equals("image/jpeg"));

                var stream = new MemoryStream();
                var encoderParameters = new EncoderParameters();
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90L);
                bitmap.Save(stream, encoder, encoderParameters);

                thumb.ContentBytes = stream.ToArray();
            }

            // Add or update file
            uow.Repositories.AddOrUpdate(thumb);

            // Create relation
            uow.Repositories.AddRelation(original, thumb, FixedRelationTypes.ThumbnailRelationType, 0, new RelationMetaDatum("size", maxWidthHeight.ToString()));
        }

        #region Helper Methods

        /// <summary>
        /// Gets a URL helper.
        /// </summary>
        /// <returns></returns>
        private static UrlHelper GetUrlHelper()
        {
            var httpContext = HttpContext.Current;

            if (httpContext == null)
                throw new InvalidOperationException();

            var httpContextBase = new HttpContextWrapper(httpContext);
            var routeData = new RouteData();
            var requestContext = new RequestContext(httpContextBase, routeData);

            return new UrlHelper(requestContext);
        }

        #endregion
    }
}
