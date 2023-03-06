using AIVTuberView.Service;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AIVTuberView
{
    public partial class VTuberView : Form
    {
        private readonly YoutubeLiveService yt;
        private readonly ChatGPTService cgpt;
        private readonly Live2DService l2d;

        private string Topic;
        private CommentEvent CacheComment;
        private int IdleTick = 0;
        private Task Ticker;
        private ulong Viewers = 0;
        public VTuberView()
        {
            yt = new YoutubeLiveService();
            cgpt = new ChatGPTService();
            l2d = new Live2DService();
            InitializeComponent();
        }

        private async void VTuberView_Load(object sender, EventArgs e)
        {
            Console.SetOut(new ControlWriter(richTextBox1));
            l2d.InitChromium(Live2DView);
            yt.CommentReceived += Yt_CommentReceived;
            Topic = await yt.StartNewStream(cgpt);
            yt.InitChromium("https://www.youtube.com/watch?v=" + yt.StreamID, ChatboxView);
            string url = "https://www.youtube.com/watch?v=" + yt.StreamID;
            try
            {
                _ = Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _ = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _ = Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    _ = Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                Ticker = new Task(async () =>
                {
                    int loopTick = 0;
                    while (true)
                    {
                        loopTick++;
                        if(Viewers == 0 && loopTick > 300)
                        {
                            Viewers = yt.GetConcurrentViewers();
                            loopTick = 0;
                        }
                        else if(Viewers > 0 && Viewers < 10 && loopTick > 600)
                        {
                            Viewers = yt.GetConcurrentViewers();
                            loopTick = 0;
                        }
                        else if (Viewers >= 10 && Viewers <= 20 && loopTick > 3000)
                        {
                            Viewers = yt.GetConcurrentViewers();
                            loopTick = 0;
                        }
                        else if(Viewers > 20 && loopTick > 30000)
                        {
                            Viewers = yt.GetConcurrentViewers();
                            loopTick = 0;
                        }
                        if (!l2d.IsReplying)
                        {
                            IdleTick++;
                        }
                        else
                        {
                            IdleTick = 0;
                        }
                        if (IdleTick > 10 && CacheComment != null)
                        {
                            Yt_CommentReceived(this, CacheComment);
                            CacheComment = null;
                        }
                        else if (IdleTick > 20 && CacheComment == null && Viewers > 1)
                        {
                            CreateSelfTalk();
                        }
                        await Task.Delay(100);
                    }
                });
                _ = Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    Ticker.Start();
                });
            }
        }

        private async void CreateSelfTalk()
        {
            if (l2d.IsReplying)
            {
                IdleTick = 0;
                return;
            }
            IdleTick = 0;
            if (l2d.WaitChatGPTResponse())
            {
                string message = await cgpt.CreateTalk();
                await l2d.Reply(message);
            }
        }

        private async void Yt_CommentReceived(object sender, CommentEvent e)
        {
            if (l2d.IsReplying)
            {
                CacheComment = e;
                return;
            }
            try
            {
                IdleTick = 0;
                if (l2d.WaitChatGPTResponse(e.Comment))
                {
                    string response = await cgpt.GetResponse(e.Comment, e.User);
                    await l2d.Reply(response);
                }
            }
            catch
            {

            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.Invoke(delegate
            {
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            });
        }
    }
    public class ControlWriter : TextWriter
    {
        private Control textbox;
        public ControlWriter(Control textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            textbox.Invoke(delegate() {
                textbox.Text += value;
            });
        }

        public override void Write(string value)
        {
            textbox.Invoke(delegate () {
                textbox.Text += value;
            });
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}