using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LLMChatDesktop
{
    public partial class FormMain : Form
    {
        // 1) Basit cevap havuzu
        private readonly (string[] keys, string[] replies)[] rules =
        {
    (new[] { "merhaba", "selam", "hey", "hi" },
     new[] { "Merhaba! Nasıl yardımcı olayım?", "Selam! Bugün ne konuşalım?" }),

    (new[] { "yardım", "komut", "ne yapabiliyorsun", "help" },
     new[] { "Şunları deneyebilirsin: selam, saat, tarih, adın ne, espri, çıkış" }),

    (new[] { "saat" },
     new[] { "Şu an saat: {time}" }),

    (new[] { "tarih", "bugün günlerden ne" },
     new[] { "Bugünün tarihi: {date}" }),

    (new[] { "adın ne", "kimsin" },
     new[] { "Ben AI’sız, kural tabanlı bir chatbotum 😄", "Ben masaüstü chatbot uygulamanım." }),

    (new[] { "espri", "şaka" },
     new[] { "Debug yaparken ağlayan tek ben değilmişim.", "Kod yazdım, çalışmadı. Sonra çalıştı, nedenini ben de bilmiyorum." }),

    (new[] { "çıkış", "kapat", "bye", "görüşürüz" },
     new[] { "Görüşürüz! Uygulamayı kapatabilirsin." })
};

        // 2) Mesajdan cevap üret
        private string GetBotReply(string userText)
        {
            string text = Normalize(userText);

            // "temizle" komutu
            if (text == "temizle" || text == "clear")
            {
                try
                {
                    rtbChat.Clear();
                    System.IO.File.WriteAllText(historyFile, "");
                }
                catch { }
                return "Sohbet temizlendi.";
            }

            // "benim adım X" komutu
            if (text.StartsWith("benim adım "))
            {
                userName = userText.Substring("benim adım ".Length).Trim();
                if (userName.Length == 0) userName = "";
                return "Memnun oldum, " + userName + "!";
            }

            // isim biliniyorsa selamları kişiselleştir
            if (!string.IsNullOrWhiteSpace(userName) && (text.Contains("selam") || text.Contains("merhaba")))
            {
                return "Selam " + userName + "! Nasıl yardımcı olayım?";
            }


            // özel: saat/tarih gibi dinamik cevaplar
            foreach (var (keys, replies) in rules)
            {
                foreach (var k in keys)
                {
                    if (text.Contains(Normalize(k)))
                    {
                        string chosen = replies[new Random().Next(replies.Length)];
                        chosen = chosen.Replace("{time}", DateTime.Now.ToString("HH:mm"));
                        chosen = chosen.Replace("{date}", DateTime.Now.ToString("dd.MM.yyyy"));
                        return chosen;
                    }
                }
            }

            // hiçbir şeye uymadıysa
            return "Bunu tam anlayamadım. 'yardım' yazarsan neler yapabildiğimi gösteririm.";
        }

        // 3) Basit normalize (küçük harf + trim)
        private string Normalize(string s)
        {
            return (s ?? "")
                .Trim()
                .ToLowerInvariant();
        }
        private string userName = "";
        private readonly string historyFile = "chat_history.txt";

        public FormMain()
        {
            InitializeComponent();
            LoadHistory();


            // Enter'a basınca mesaj göndersin
            txtMessage.KeyDown += TxtMessage_KeyDown;

            // İlk mesaj (opsiyonel)
            AppendLine("Bot: Merhaba! Mesaj yazıp Gönder'e bas 😊");
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // textbox'a satır atmasın
                btnSend.PerformClick();    // buton click'i tetikle
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string userText = txtMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(userText))
                return;

            txtMessage.Clear();
            AppendLine("Sen: " + userText);

            // Şimdilik fake bot cevabı
            btnSend.Enabled = false;
            try
            {
                string botReply = GetBotReply(userText);
                AppendLine("Bot: " + botReply);
            }
            finally
            {
                btnSend.Enabled = true;
                txtMessage.Focus();
            }
        }

        private Task<string> FakeBotReply(string input)
        {
            // Burayı bir sonraki adımda gerçek LLM çağrısına çevireceğiz.
            // Şimdilik basit bir cevap dönsün:
            return Task.FromResult("Bunu anladım: \"" + input + "\" (LLM bağlayınca daha akıllı olacak)");
        }

        private void AppendLine(string text)
        {
            rtbChat.AppendText(text + Environment.NewLine);
            rtbChat.SelectionStart = rtbChat.Text.Length;
            rtbChat.ScrollToCaret();

            try
            {
                System.IO.File.AppendAllText(historyFile, text + Environment.NewLine);
            }
            catch { /* önemli değil */ }
        }

        private void LoadHistory()
        {
            try
            {
                if (System.IO.File.Exists(historyFile))
                {
                    rtbChat.Text = System.IO.File.ReadAllText(historyFile);
                    rtbChat.SelectionStart = rtbChat.Text.Length;
                    rtbChat.ScrollToCaret();
                }
            }
            catch { /* önemli değil */ }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void temizleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rtbChat.Clear();
            try
            {
                System.IO.File.WriteAllText(historyFile, "");
            }
            catch { }
        }

        private void çıkışToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void hakkındaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                  "Kural Tabanlı Masaüstü Chatbot\n\n" +
                  "• WinForms (.NET Framework)\n" +
                  "• Anahtar kelime eşleştirme\n" +
                  "• Sohbet geçmişi kaydı\n\n" +
                  "Geliştirici: Nisa İrem Dilekçi",
                  "Hakkında",
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Information
              );
        }
    }
}
