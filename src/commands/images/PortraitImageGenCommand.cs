using nai;

public class PortraitImageGenCommand : ImageGenCommand
{
    protected override (int x, int y) GetSize() => (512, 768);

    public override string Aliases => "p";
}