using CefSharp;
using CefSharp.WinForms;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace AIVTuberView.Service
{
    internal class YoutubeLiveService
    {
        private LiveBroadcast broadcast { get; set; }
        private YouTubeService youTube { get; set; }
        private string streamID { get; set; }
        private string WebPath { get; set; }
        private ChromiumWebBrowser browser { get; set; }

        public event EventHandler<CommentEvent> CommentReceived;
        public YoutubeLiveService()
        {

        }

        public string StartNewStream(ChatGPTService chatService)
        {
            //string title = await chatService.GetTitle();
            youTube = new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = ConfigService.Instance.Credential
            });
            LiveBroadcastSnippet broadcastSnippet = new()
            {
                Title = "Ena-Ai Chat: No Topic",
                ScheduledStartTime = DateTime.Now,
                ScheduledEndTime = DateTime.Now.AddHours(1)
            };

            LiveBroadcastStatus status = new()
            {
                PrivacyStatus = "Unlisted",
                SelfDeclaredMadeForKids = false
            };

            LiveBroadcast broadcast = new()
            {
                Kind = "youtube#liveBroadcast",
                Snippet = broadcastSnippet,
                Status = status
            };

            LiveBroadcastsResource.InsertRequest liveBroadcastInsert = youTube.LiveBroadcasts.Insert(broadcast, "status,snippet");
            this.broadcast = liveBroadcastInsert.Execute();

            LiveStreamsResource.ListRequest list = youTube.LiveStreams.List("snippet,cdn,contentDetails,status");
            list.Mine = true;
            LiveStreamListResponse response = list.Execute();
            LiveStream selectedLive = response.Items.LastOrDefault();
            if (selectedLive == null)
            {
                throw new ArgumentNullException("Created Live Stream is not found!");
            }
            LiveBroadcastsResource.BindRequest liveBroadcastsBind = youTube.LiveBroadcasts.Bind(this.broadcast.Id, "id,contentDetails");
            liveBroadcastsBind.StreamId = selectedLive.Id;
            broadcast = liveBroadcastsBind.Execute();
            streamID = broadcast.Id;

            return "Chat";
        }

        public ulong GetConcurrentViewers()
        {
            var req = youTube.LiveBroadcasts.List("statistics,snippet");
            req.Mine = true;
            var res = req.Execute();
            return res.Items.First(x => x.Id == streamID).Statistics.ConcurrentViewers ?? 0;
        }

        public string StreamID => streamID;

        public void InitChromium(string youtubeUrl, ChromiumWebBrowser browser)
        {
            this.browser = browser;
            browser.Load(youtubeUrl);
            browser.JavascriptObjectRepository.Register("bound", new CefObject(this));
            browser.FrameLoadEnd += Browser_FrameLoadEnd;
            Console.WriteLine("Current stream YT url is " + youtubeUrl);
            Console.WriteLine("Inited Youtube Services");
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (WebPath == null && e.Frame.Url.Split('=')[0] == "https://www.youtube.com/live_chat?continuation")
            {
                browser.Load(WebPath = e.Frame.Url);
            }
            else if (e.Frame.Url == WebPath)
            {
                e.Frame.ExecuteJavaScriptAsync("document.getElementById(\"chat\").style.background = \"#00FF00\";");
                e.Frame.ExecuteJavaScriptAsync("Array.prototype.slice.call(document.getElementsByTagName(\"yt-live-chat-viewer-engagement-message-renderer\")).forEach((x) => x.remove())");
                e.Frame.ExecuteJavaScriptAsync("Array.prototype.slice.call(document.getElementsByTagName(\"yt-live-chat-header-renderer\")).forEach((x) => x.remove())");
                e.Frame.ExecuteJavaScriptAsync("Array.prototype.slice.call(document.getElementsByTagName(\"yt-live-chat-message-input-renderer\")).forEach((x) => x.remove())");
                e.Frame.ExecuteJavaScriptAsync("document.getElementById(\"item-offset\").style.height = \"calc(100vh - 48px)\";");
                e.Frame.ExecuteJavaScriptAsync(@"setInterval(()=>{
                Array.prototype.slice.call(document.getElementsByTagName(""yt-live-chat-text-message-renderer"")).forEach((x) => {
                        x.style.fontSize = ""18px"";
                        x.style.background = ""rgba(0,0,0,0.5)"";
                        x.style.paddingTop = ""6px"";
                        x.style.paddingBottom = ""6px"";
                    })
                },100);");
                e.Frame.ExecuteJavaScriptAsync(@"(async function () {
                    await CefSharp.BindObjectAsync('boundAsync', 'bound');
                    var last = """";
                    setInterval(function () {
                        (function (t) {
                            if (last != (cid = t[t.length - 1].id))
                                for (var e = t.length; e--;) {
                                    if (last == t[e].id) return last = cid;
                                    t[e].children[1].children[2].textContent && bound.onText(t[e].children[1].children[1].children[1].textContent, t[e].children[1].children[2].textContent);
                                    last = cid;
                                    return;
                                }
                        })(document.getElementsByTagName(""yt-live-chat-text-message-renderer""))
                    }, 300);
                })()");
            }
        }

        private class CefObject
        {
            private readonly YoutubeLiveService service;
            public CefObject(YoutubeLiveService service)
            {
                this.service = service;
            }

            public void onText(string name, string text)
            {
                service.CommentReceived?.Invoke(this, new CommentEvent
                {
                    Comment = text,
                    User = name
                });
            }
        }
    }

    public class CommentEvent : EventArgs
    {
        public string Comment { get; set; }
        public string User { get; set; }

    }
}
