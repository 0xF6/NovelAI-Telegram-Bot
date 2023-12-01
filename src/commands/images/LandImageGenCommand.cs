using nai;

public class LandImageGenCommand : ImageGenCommand
{
    protected override (int x, int y) GetSize() => (768, 512);

    public override string Aliases => "l";
}