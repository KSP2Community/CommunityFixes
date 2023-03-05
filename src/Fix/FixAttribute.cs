namespace CommunityFixes.Fix;

[AttributeUsage(AttributeTargets.Class)]
public class FixAttribute: Attribute
{
    public string Name { get; }

    public FixAttribute(string name)
    {
        Name = name;
    }
}