﻿namespace nai.commands;

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

public class Img2ImgP : ImageGenCommand
{
    public override string Aliases => "img2imgp";


    protected override (int x, int y) GetSize() => (512, 768);

    protected override async ValueTask OnFillAdditionalData(NovelAIinput input)
    {
        var file = await this.BotClient.GetFileAsync(Message.ReplyToMessage!.Document!.FileId);
        using var mem = new MemoryStream();
        await BotClient.DownloadFileAsync(
            filePath: file.FilePath!,
            destination: mem);

        var powerRegex = new Regex("@power:(((\\d+\\.?\\d*)|(\\.\\d+)))");

        if (powerRegex.IsMatch(input.input.Replace('\n', ' ')))
        {
            var power = powerRegex.Match(input.input.Replace('\n', ' '));
            input.parameters.strength = float.Parse(power.Groups[1].Value) - 0.01f;
            input = input with { input = input.input.Replace(power.Groups[0].Value, "") };
            Logger.LogDebug("Success parsed power from input, power: {power}", input.parameters.strength);
        }
        else
            input.parameters.strength = 0.53f;

        var noiseRegex = new Regex("@noise:(((\\d+\\.?\\d*)|(\\.\\d+)))");

        if (noiseRegex.IsMatch(input.input.Replace('\n', ' ')))
        {
            var noise = noiseRegex.Match(input.input.Replace('\n', ' '));
            input.parameters.noise = float.Parse(noise.Groups[1].Value) - 0.01f;
            input = input with { input = input.input.Replace(noise.Groups[0].Value, "") };
            Logger.LogDebug("Success parsed noise from input, noise: {noise}", input.parameters.noise);
        }
        else
            input.parameters.noise = 0.05f;

        input.parameters.image = Convert.ToBase64String(mem.ToArray());
        input.parameters.steps = 50;
        
    }

    protected override async ValueTask<bool> OnValidate()
    {
        if (Message.ReplyToMessage is null)
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Need reply to message with image without compression?");
            return false;
        }

        if (Message.ReplyToMessage.Document is null)
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Its not reply to image without compression.");
            return false;
        }

        if (Message.ReplyToMessage?.Document is { MimeType: "image/jpeg" } or { MimeType: "image/png" })
        {
            return true;
        }

        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: $"Is not image.");
        return false;
    }
}