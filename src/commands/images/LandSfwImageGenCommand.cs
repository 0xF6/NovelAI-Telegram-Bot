public class LandSfwImageGenCommand : SfwImageGenCommand
{
    protected override (int x, int y) GetSize() => (768, 512);


    public override List<string> Aliases
        => new() { "/sfwl@novelai_sanrioslut_bot", "/sfwl" };
}