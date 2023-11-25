namespace nai.commands.images;

public class WallPaperGenCommand : ImageGenCommand
{
    public override List<string> Aliases => new()
    {
        "/wallpaper"
    };
    protected override ValueTask OnFillAdditionalData(NovelAIinput input)
    {
        input.parameters.sm = true;
        return base.OnFillAdditionalData(input);
    }

    protected override (int x, int y) GetSize() => new(1920, 1088);
}