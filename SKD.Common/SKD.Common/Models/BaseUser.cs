using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using Plugin.CloudFirestore.Converters;
using SKD.Common.Themes;

namespace SKD.Common.Models
{
    public abstract class BaseUser : BaseModel
    {
        protected static IFirestore Firestore => CrossCloudFirestore.Current.Instance;

        [Ignored]
        public IDocumentReference Doc => Firestore.Collection("Users").Document(UID);

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [DocumentConverter(typeof(EnumStringConverter))]
        public Theme DesiredTheme { get; set; }
    }

}
