public class PortraitImageGenCommand : ImageGenCommand
{
    protected override (int x, int y) GetSize() => (512, 768);

    public override List<string> Aliases
        => new() { "/p" };
}