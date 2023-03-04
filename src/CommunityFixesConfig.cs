
using SpaceWarp.API.Configuration;
using Newtonsoft.Json;
namespace CommunityFixes;

// Define our config class with the [ModConfig] attribute
[ModConfig]
[JsonObject(MemberSerialization.OptOut)]
public class CommunityFixesConfig {
    [ConfigField("funny number")]
    [ConfigDefaultValue(69)]
    public int funny_number;
}
