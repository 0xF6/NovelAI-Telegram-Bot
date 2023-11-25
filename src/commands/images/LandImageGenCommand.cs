public class LandImageGenCommand : ImageGenCommand
{
    protected override (int x, int y) GetSize() => (768, 512);

    public override List<string> Aliases
        => new() { "/l" };
}