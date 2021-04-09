using Plugin.CloudFirestore.Attributes;

namespace SKD.Common.Models
{
    public class BaseModel
    {
        [Id]
        public string UID { get; set; }
    }
}
