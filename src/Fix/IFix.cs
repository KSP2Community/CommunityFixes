namespace CommunityFixes.Fix;

public interface IFix
{
    public string Name { get; }
    public void OnInitialized();
}