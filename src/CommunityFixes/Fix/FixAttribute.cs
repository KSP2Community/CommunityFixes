using System.Reflection;

namespace CommunityFixes.Fix;

/// <summary>
/// Attribute for all fixes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class FixAttribute: Attribute
{
    /// <summary>
    /// The name of the fix displayed as description in the config file.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new fix attribute.
    /// </summary>
    /// <param name="name">The name of the fix displayed as description in the config file.</param>
    public FixAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the instance of the fix attribute on a type.
    /// </summary>
    /// <param name="type">The type to get the fix attribute from.</param>
    /// <returns>The fix attribute instance.</returns>
    /// <exception cref="Exception">Thrown if the type does not have a fix attribute.</exception>
    internal static FixAttribute GetForType(Type type)
    {
        try
        {
            return type.GetCustomAttribute<FixAttribute>();
        }
        catch (Exception)
        {
            throw new Exception(
                $"The attribute {typeof(FixAttribute).FullName} has to be declared on the class {type.FullName}."
            );
        }
    }
}