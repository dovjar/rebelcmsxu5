using System.Web.Security;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Framework;
using RebelCms.Framework.Context;
using RebelCms.Framework.TypeMapping;
using StringExtensions = RebelCms.Framework.StringExtensions;

namespace RebelCms.Cms.Web.Mapping
{
    /// <summary>
    /// Type mapper for Membership models
    /// </summary>
    public sealed class MembershipModelMapper : AbstractFluentMappingEngine
    {
        public MembershipModelMapper(IFrameworkContext frameworkContext)
            : base(frameworkContext)
        {
        }

        public override void ConfigureMappings()
        {
            //creates a map to go from a UserEditorModel to an *EXISTING* member.
            this.CreateMap<UserEditorModel, MembershipUser>()
                .ForMember(x => x.UserName, x => x.MapFrom(y => y.Username))
                .ForMember(x => x.Email, x => x.MapFrom(y => y.Email))
                .AfterMap((s, t) =>
                    {
                        //if the password is being changed
                        if (!StringExtensions.IsNullOrWhiteSpace(s.OldPassword) && !StringExtensions.IsNullOrWhiteSpace(s.ConfirmPassword))
                        {
                            t.ChangePassword(s.OldPassword, s.Password);
                        }
                    });

        }
    }
}