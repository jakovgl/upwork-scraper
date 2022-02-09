using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Telegram.Bot;
using upwork_scraper.obj;

namespace upwork_scraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var settings = LoadSettings();
            
            using var client = new HttpClient();
            
            var notifiedJobs = new ArrayList();

            while (true)
            {
                Console.WriteLine("Running a task...");
                await RunTask(client, settings, notifiedJobs);
                Thread.Sleep(10000);
            }
        }

        private static async Task RunTask(HttpClient client, Settings settings, ArrayList notifiedJobs)
        {
            const string dividerBold = "==========================================================================================";
            const string divider = "------------------------------------------------------------------------------------------";

            var response = await SendRequest(client, settings);

            Console.WriteLine(dividerBold);
            Console.WriteLine($"Status Response Code: {response.StatusCode}");
            Console.WriteLine(dividerBold);

            var jobs = await DeserializeResponse(response);

            foreach (var job in jobs)
            {
                if (
                    (job.ShortEngagement is null || job.ShortEngagement.Equals(settings.Engagement)) &&
                    (job.Attrs.Select(y => y.PrettyName).Intersect(settings.Categories).Any()) &&
                    (!notifiedJobs.Contains(job.Uid))
                )
                {
                    Console.WriteLine(job.Title);
                    Console.WriteLine(divider);
                    await SendTelegramMessage(settings, job);
                    notifiedJobs.Add(job.Uid);
                }
            }
        }

        private static Settings LoadSettings()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            return config.GetRequiredSection("Settings").Get<Settings>();
        }

        private static async Task<HttpResponseMessage> SendRequest(HttpClient client, Settings settings)
        {
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://www.upwork.com/ab/find-work/api/feeds/embeddings-recommendations"),
                Headers =
                {
                    { "authority", "www.upwork.com" },
                    { "sec-ch-ua", "Opera\";v=\"83\", \"Chromium\";v=\"97\", \";Not A Brand\";v=\"99" },
                    { "x-odesk-user-agent", "oDesk LM" },
                    { "vnd-eo-trace-id", "6da38ca96ad39133-SEA" },
                    { "authorization", $"Bearer {settings.Bearer}" },
                    { "accept", "application/json, text/plain, */*" },
                    { "sec-ch-ua-mobile", "?0" },
                    { "x-requested-with", "XMLHttpRequest" },
                    { "vnd-eo-parent-span-id", "d437576a-cf86-4a60-a9a2-30faa8eb9f4c" },
                    { "user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36 OPR/83.0.4254.27" },
                    { "vnd-eo-span-id", "749e1dba-ce9e-4253-9555-263314c68e90" },
                    { "sec-ch-ua-platform", "\"macOS\"" },
                    { "sec-fetch-site", "same-origin" },
                    { "sec-fetch-mode", "cors" },
                    { "sec-fetch-dest", "empty" },
                    { "referer", "https://www.upwork.com/nx/find-work/most-recent"},
                    { "accept-language", "en-GB,en-US;q=0.9,en;q=0.8" },
                    { "cookie", "visitor_id=78.0.134.76.1634819542579000; lang=en; survey_allowed=true; cookie_prefix=; cookie_domain=.upwork.com; spt=4abfde3d-a46e-451c-9d8b-f1fbdc05a7a7; track_url_params=%5B%5D; restriction_verified=1; pxcts=f2e55240-326a-11ec-bdf9-f784122b70a5; _pxvid=f08dce52-326a-11ec-a16a-494c6b4b6e50; keen={%22uuid%22:%228dd514a8-a9b7-49f0-a0c7-a12928d493aa%22%2C%22initialReferrer%22:%22https://www.google.com/%22}; IR_gbd=upwork.com; G_ENABLED_IDPS=google; ufx03Y_1pc=c7c5ed96-be35-4caf-bf42-7175b6bc8d14; sliguid=e978162b-bf42-42d0-8ab6-faf922e69eb0; slirequested=true; __pdst=aa9065cdee254900844eb56810a89c55; bmuid=1634819549469-7E82315B-8303-476D-9FA9-C4BC8C43A1E6; recognized=5897bb04; UserPrivacySettings=%7B%22AlertBoxClosed%22%3A%222021-10-21T12%3A33%3A17.349Z%22%2C%22Groups%22%3A%7B%22Targeting%22%3Atrue%7D%7D; channel=other; device_view=full; _gcl_au=1.1.1406894807.1642973586; _rdt_uuid=1642973589238.e258a0dd-8c04-453b-9403-8fb39d4bf651; _fbp=fb.1.1642973591141.184636131; cb_user_id=null; cb_group_id=null; cb_anonymous_id=%22e18d5e85-7048-456d-90f4-8cfb975bd2df%22; _gcl_aw=GCL.1642973797.CjwKCAiAlrSPBhBaEiwAuLSDUGclLpm0hJqQ3XHTsE-tDG1GhHX8zrngZMb9uF6Y2XcT87nIX0C5HBoC_94QAvD_BwE; _gac_UA-62227314-1=1.1642973797.CjwKCAiAlrSPBhBaEiwAuLSDUGclLpm0hJqQ3XHTsE-tDG1GhHX8zrngZMb9uF6Y2XcT87nIX0C5HBoC_94QAvD_BwE; console_user=5897bb04; user_uid=1331886708213829632; company_last_accessed=d36437341; current_organization_uid=1331886708251578369; enabled_ff=CI11132Air2Dot75,CI9570Air2Dot5,u0021CI10270Air2Dot5QTAllocations,u0021CI10857Air3Dot0,u0021air2Dot76,u0021SSINav,OTBnrOn,u0021OTBnr; _hp2_id.1058175795=%7B%22userId%22%3A%226472667673761463%22%2C%22pageviewId%22%3A%227517604746209650%22%2C%22sessionId%22%3A%227795614537940173%22%2C%22identity%22%3A%221331886708213829632%22%2C%22trackerVersion%22%3A%224.0%22%2C%22identityField%22%3Anull%2C%22isIdentified%22%3A1%7D; cdContextId=38; cdSNum=1643824401104-sjt0000545-36cbdc46-70b0-4128-9a5a-21e710ef9890; _clck=yrpecz|1|eyo|0; _ga=GA1.1.1644398457.1634819546; _hp2_props.2858077939=%7B%22container_id%22%3A%22GTM-P8M8MVZ%22%2C%22user_context%22%3A%22freelancer%22%2C%22user_logged_in%22%3Atrue%7D; _hp2_id.2858077939=%7B%22userId%22%3A%226644779547180854%22%2C%22pageviewId%22%3A%226962150645739133%22%2C%22sessionId%22%3A%22803143065320094%22%2C%22identity%22%3A%221331886708213829632%22%2C%22trackerVersion%22%3A%224.0%22%2C%22identityField%22%3Anull%2C%22isIdentified%22%3A1%7D; IR_13634=1643900746392%7C0%7C1643900746392%7C%7C; _dpm_id.5831=545291fc-04ba-4e5b-b54d-6a478b43ec08.1634819548.15.1643900747.1643897768.74a9c688-fb6b-4b65-bccf-78e1c4b2b533; _uetvid=f394f070326a11ecbcf36bcecad243c9; _sp_id.2a16=a86268f4-4645-4ffa-8453-89c0a30c00ef.1634819546.15.1643900747.1643897804.7064f83b-82c0-45da-9800-a664cf7494a8; _ga_KSM221PNDX=GS1.1.1643900180.12.1.1643902027.0; dash_company_last_accessed=1331886708251578369; hide_dash_notif_bar=true; forterToken=532b675c3ef54e1b89f0e6d0330f1413_1644243835633_167_UAL9_9ck; odesk_signup.referer.raw=https%3A%2F%2Fwww.upwork.com%2Fnx%2Ffind-work%2Fmost-recent; master_access_token=c968711d.oauth2v2_2446b53b0d6c1b2195e3418de93cae46; oauth2_global_js_token=oauth2v2_11931f13b623267d688a8b6e8c25769c; user_oauth2_slave_access_token=c968711d.oauth2v2_2446b53b0d6c1b2195e3418de93cae46:1331886708213829632.oauth2v2_ec4ab8b4745544c1c4cca6524a20317d; __cfruid=d9d7b97ab1d10401bc619686b47f9cfecefdb439-1644307609; _pxhd=K6rSdnARO193cew8aKyz1Vv0y1PmzbJeILwzh3frVhKr2dpqgsYLhwKHKJOGLTYS5YowMhPEVdpAb/A/jCkibw==:efv-KZXvY-ar0Owy2MQlOOmB8Ujz01gNfGf7b/aC74sSuNTIQKHOScCbNVJKWxW0Bgv8kP2CDFgHy8U6WH-r1bOSLzyYIO-lqjJYFex3qrQ=; __cf_bm=9TxNMlHUWZwrIYD.TZYNkkDGjWDSrf9wRUZgvspyxOg-1644309565-0-AQk8tRi5RkmIIkTPH/cY3ic4Oey0UMndHYMwe2zAneU2rGSQeuURhNW25kx+A1NYrxaYtx0cNJYyYuq9DSVUvxA=; XSRF-TOKEN=9fa957138cc6b2ad2430561bebb63631" }
                }
            };

            return await client.SendAsync(requestMessage);
        }

        private static async Task<List<Job>> DeserializeResponse(HttpResponseMessage response)
        {
            var deserializedObjects = JsonConvert.DeserializeObject<Response>(await response.Content.ReadAsStringAsync())?.Results;

            if (deserializedObjects == null)
            {
                Console.WriteLine("There has been an error deserializing json.");
                Environment.Exit(0);
            }

            return deserializedObjects;
        }

        private static async Task SendTelegramMessage(Settings settings, Job job)
        {
            var bot = new TelegramBotClient(settings.TelegramApiKey);
            await bot.SendTextMessageAsync(settings.TelegramChatId, GetMessageFromJob(job));
        }

        private static string GetMessageFromJob(Job job)
        {
            var sb = new StringBuilder();
            sb.Append("We found a new job you should apply to: \n");
            sb.Append($"Title: {job.Title}\n");
            sb.Append($"Uid: {job.Uid}\n");
            sb.Append($"ProposalsTier: {job.ProposalsTier}\n");
            sb.Append($"Duration: {job.Duration}\n");
            sb.Append($"Description: {job.Description}\n");
            sb.Append($"Link: https://www.upwork.com/jobs/{job.CipherText}");

            return sb.ToString();
        }
    }
}