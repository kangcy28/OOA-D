using System;
using System.IO;
using System.Text.Json;

namespace OoadCoach
{
    public class LearningState
    {
        public int CurrentStage { get; set; } = 2;
        public string StageName { get; set; } = "階段二：OOA 領域分析 (需求拆解與概念建模)";
        public int CurrentTask { get; set; } = 1;
        public string TaskTitle { get; set; } = "任務 2.1：從自然語言提取實體（名詞/動詞分析法）";
        public string Language { get; set; } = "C#";
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    class Program
    {
        private static readonly string StateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "progress.json");
        private static LearningState state = new LearningState();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            LoadState();

            PrintWelcomeBanner();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\n[OOAD 教練] 請輸入指令 (/status, /challenge, /hint, /exam, /reset, 或 exit) > ");
                Console.ResetColor();

                string input = Console.ReadLine()?.Trim() ?? "";

                if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("👋 期待下次學習，隨時再見！");
                    break;
                }

                HandleCommand(input);
            }
        }

        private static void PrintWelcomeBanner()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("===============================================================");
            Console.WriteLine("       🧭 歡迎來到 OOA/D 互動學習機器人 (C# Console 終端版)");
            Console.WriteLine("===============================================================");
            Console.ResetColor();
            Console.WriteLine("本機器人與 Antigravity AI 助教連動，隨時追蹤你的實戰進度！");
            Console.WriteLine("常用指令：");
            Console.WriteLine("  /status    : 查看當前進度與階段標準");
            Console.WriteLine("  /challenge : 顯示當前挑戰內容");
            Console.WriteLine("  /hint      : 取得解題提示");
            Console.WriteLine("  /exam      : 查看晉級通關標準與考核");
            Console.WriteLine("  /reset     : 重設進度");
            Console.WriteLine("===============================================================\n");
        }

        private static void HandleCommand(string cmd)
        {
            switch (cmd.ToLower())
            {
                case "/status":
                    ShowStatus();
                    break;
                case "/challenge":
                    ShowChallenge();
                    break;
                case "/hint":
                    ShowHint();
                    break;
                case "/exam":
                    ShowExam();
                    break;
                case "/reset":
                    ResetProgress();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"未知的指令：{cmd}。請使用 /status, /challenge, /hint, /exam, /reset 或 exit");
                    Console.ResetColor();
                    break;
            }
        }

        private static void ShowStatus()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n📊 【當前學習進度報告】");
            Console.WriteLine($"  - 當前階段：第 {state.CurrentStage} 階段 ({state.StageName})");
            Console.WriteLine($"  - 當前任務：{state.TaskTitle}");
            Console.WriteLine($"  - 偏好語言：{state.Language}");
            Console.WriteLine($"  - 專案目錄：C:\\Users\\USER\\Desktop\\OOAD");
            Console.WriteLine($"  - 完整進度表：請參閱專案目錄下的 ooad_learning_tracker.md");
            Console.ResetColor();
        }

        private static void ShowChallenge()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n🎯 【當前挑戰題目：任務 1.1】");
            Console.WriteLine("情境：請打開 Stage1_Playground/Program.cs 查看程序導向的飲料店程式碼。");
            Console.WriteLine("\n請在 Antigravity 聊天室或心中思考以下兩個問題：");
            Console.WriteLine("1. 災難情境 1：如果店裡要賣 10 種飲料，且有 3 個店員同時結帳，變數全散在 Main 會變怎樣？");
            Console.WriteLine("2. 災難情境 2：如果有人寫了 drinkStock = -99，現有程式碼有人能阻止他把庫存改成負數嗎？");
            Console.WriteLine("\n💡 提示：試著用直覺回答，或者在 AI 聊天室回覆你的答案，AI 會為你審核！");
            Console.ResetColor();
        }

        private static void ShowHint()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n💡 【思路提示】");
            Console.WriteLine("1. 資料分散：想像每多一種飲料，就要多生出 drinkName2, drinkPrice2, drinkStock2...");
            Console.WriteLine("   真實世界中，「珍珠奶茶」的名稱、價格、庫存是一起的！為甚麼在程式裡要拆開？");
            Console.WriteLine("2. 缺少守門員：在程序化代碼裡，任何地方都能改變數，根本沒有「安全檢查機制」。");
            Console.ResetColor();
        }

        private static void ShowExam()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n🏆 【階段一晉級考核條件】");
            Console.WriteLine("1. 能消滅貧血模型（不再寫只有屬性沒有行為的 class）。");
            Console.WriteLine("2. 徹底遵守『Tell, Don't Ask』原則（叫物件去扣庫存，而不是查庫存後自己改）。");
            Console.WriteLine("3. 屬性皆私有化封裝，保護業務規則不被非法竄改。");
            Console.WriteLine("\n準備好後可在 AI 對話框輸入「申請階段一考核」！");
            Console.ResetColor();
        }

        private static void ResetProgress()
        {
            state = new LearningState();
            SaveState();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ 進度已重置為初始狀態（階段一開始）！");
            Console.ResetColor();
        }

        private static void LoadState()
        {
            try
            {
                if (File.Exists(StateFilePath))
                {
                    string json = File.ReadAllText(StateFilePath);
                    var loaded = JsonSerializer.Deserialize<LearningState>(json);
                    if (loaded != null) state = loaded;
                }
                else
                {
                    SaveState();
                }
            }
            catch
            {
                state = new LearningState();
            }
        }

        private static void SaveState()
        {
            try
            {
                string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(StateFilePath, json);
            }
            catch { }
        }
    }
}
