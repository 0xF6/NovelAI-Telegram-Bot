using Flurl.Http;
using NAIBot.db;

namespace NAIBot.nai;

public class NovelAI
{
    public FlurlClient flurl;

    public NovelAI()
    {
        flurl = new FlurlClient("https://api.novelai.net/ai/generate-image");

        // set up headers for correct handle request in novel ai server
        flurl.WithHeader("Content-Type", "application/json");
        flurl.WithHeader("Accept", "*/*");
        flurl.WithHeader("origin", "https://novelai.net");
        flurl.WithHeader("referer", "https://novelai.net/");
        flurl.WithHeader("sec-ch-ua", "\"Microsoft Edge\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24\"");
        flurl.WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.62");
        flurl.WithHeader("Authorization",
            $"Bearer {Db.GetNovelAIToken()}");
    }


    public IFlurlRequest Request()
        => flurl.Request();
}