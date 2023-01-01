public class PortraitNsfwImageGenCommand : NsfwImageGenCommand
{
    protected override (int x, int y) GetSize() => (512, 768);

    public override List<string> Aliases
        => new() { "/nsfwp@novelai_sanrioslut_bot", "/nsfwp" };
}