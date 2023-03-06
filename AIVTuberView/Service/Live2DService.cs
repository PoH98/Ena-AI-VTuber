using CefSharp;
using CefSharp.SchemeHandler;
using CefSharp.WinForms;
using VaderSharp2;

namespace AIVTuberView.Service
{
    public class Live2DService
    {
        private ChromiumWebBrowser browser;
        private bool ProcessingChatGPT;
        private readonly SentimentIntensityAnalyzer analyzer;
        private AudioService audio;
        private bool Inited = false;
        private Task Ticker;
        private CancellationTokenSource Cancellation;
        public Live2DService()
        {
            analyzer = new SentimentIntensityAnalyzer();
            CefSettings settings = new();
            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = "https",
                DomainName = "cefsharp",
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(
                    rootFolder: Path.Combine(Environment.CurrentDirectory, "wwwroot"),
                    hostName: "cefsharp",
                    defaultPage: "index.html" // will default to index.html
                )
            });
            settings.LogSeverity = LogSeverity.Error;
            settings.CefCommandLineArgs.Add("autoplay-policy", "no-user-gesture-required");
            settings.DisableGpuAcceleration();
            _ = Cef.Initialize(settings);
        }

        public void InitChromium(ChromiumWebBrowser browser)
        {
            if (Inited)
            {
                throw new InvalidOperationException("Live2D Service had been inited");
            }
            this.browser = browser;
            audio = new AudioService(this.browser);
            this.browser.LoadingStateChanged += Live2DView_LoadingStateChanged;
            this.browser.Load("https://cefsharp/");
            this.browser.JavascriptObjectRepository.Register("played", new PlayedObject(this)); 
            Inited = true;
            Console.WriteLine("Inited Live2D Services");
        }

        public bool IsReplying { get; private set; }
        /// <summary>
        /// Use this to stop all future processing, if true means the Live2D isn't speaking and can use ChatGPTService to get response
        /// </summary>
        public bool WaitChatGPTResponse(string message)
        {
            SentimentAnalysisResults scores = analyzer.PolarityScores(message);
            if (scores.Negative > scores.Positive && scores.Negative > scores.Neutral)
            {
                browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('Yandere');");
            }
            else if (scores.Negative > scores.Positive)
            {
                browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('Blue');");
            }
            else if (scores.Positive > scores.Neutral && scores.Positive > scores.Negative)
            {
                browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('XEye');");
            }
            else if (scores.Positive > 0.6)
            {
                browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('HeartEye');");
            }
            if (message.Contains("心心眼") || message.Contains("Love eye", StringComparison.InvariantCultureIgnoreCase))
            {
                browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('HeartEye');");
            }
            return WaitChatGPTResponse();
        }
        /// <summary>
        /// Use this to stop all future processing, if true means the Live2D isn't speaking and can use ChatGPTService to get response
        /// </summary>
        public bool WaitChatGPTResponse()
        {
            if (IsReplying)
            {
                return false;
            }
            ProcessingChatGPT = true;
            IsReplying = true;
            if (Ticker != null)
            {
                Cancellation.Cancel();
            }
            Cancellation = new CancellationTokenSource();
            Ticker = new Task(async () =>
            {
                int tick = 0;
                //wait 30 sec
                do
                {
                    tick++;
                    await Task.Delay(200);
                    if (Cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                }
                while (tick < 150);
                if (IsReplying && ProcessingChatGPT)
                {
                    //no, we won't really need wait so long time for a ChatGPT response
                    IsReplying = false;
                    ProcessingChatGPT = false;
                }
            });
            Ticker.Start();
            return true;
        }
        /// <summary>
        /// Reply user with ChatGPT response, with audio and also running expressions
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Reply(string message)
        {
            ProcessingChatGPT = false;
            if (Ticker != null)
            {
                Cancellation.Cancel();
            }
            SentimentAnalysisResults scores = analyzer.PolarityScores(message);
            if (scores.Negative > scores.Positive && scores.Negative > scores.Neutral)
            {
                browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('Yandere');");
            }
            else if (scores.Negative > scores.Positive)
            {
                browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('Blue');");
            }
            else if (scores.Positive > scores.Neutral && scores.Positive > scores.Negative)
            {
                browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('XEye');");
            }
            else if (scores.Positive > 0.6)
            {
                browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('HeartEye');");
            }
            await audio.Speech(message);
            browser.ExecuteScriptAsync("model.internalModel.motionManager.expressionManager.setExpression('Reset');");
        }

        private void Live2DView_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                _ = Task.Run(async () =>
                {
                    browser.ExecuteScriptAsync("document.body.style.overflow = 'hidden';");
                    await Task.Delay(500);
                    browser.ExecuteScriptAsync(@"window.model.anchor.set(1.3,0.1)");
                });
            }
            browser.ExecuteScriptAsync(@"(async function () {
                    await CefSharp.BindObjectAsync('playedAsync', 'played');
                })()");
        }

        private class PlayedObject
        {
            private readonly Live2DService service;
            public PlayedObject(Live2DService service)
            {
                this.service = service;
            }

            public void onPlayed()
            {
                service.IsReplying = false;
            }
        }
    }
}
