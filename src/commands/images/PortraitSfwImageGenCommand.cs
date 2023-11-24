public class PortraitSfwImageGenCommand : SfwImageGenCommand
{
    protected override (int x, int y) GetSize() => (512, 768);

    public override List<string> Aliases
        => new() { "/sfwp@novelai_sanrioslut_bot", "/sfwp" };
}