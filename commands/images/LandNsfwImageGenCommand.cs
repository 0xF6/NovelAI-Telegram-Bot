public class LandNsfwImageGenCommand : NsfwImageGenCommand
{
    protected override (int x, int y) GetSize() => (768, 512);

    public override List<string> Aliases
        => new() { "/nsfwl@novelai_sanrioslut_bot", "/nsfwl" };
}