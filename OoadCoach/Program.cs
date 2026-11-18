using System;
using System.IO;
using System.Text.Json;

namespace OoadCoach
{
    public class LearningState
    {
        public int CurrentStage { get; set; } = 8;
        public string StageName { get; set; } = "🏆 全階段大滿貫結業（階段 1 ～ 階段 8 全數通關！）";
        public int CurrentTask { get; set; } = 4;
        public string TaskTitle { get; set; } = "已通關：智慧停車場系統白板面試滿分畢業！(歡迎隨時輸入「切換模擬面試」繼續對練)";
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
                Console.Write("\n[OOAD 教練] 請輸入指令 (/status, /challenge, /hint, /exam, /mock, /reset, 或 exit) > ");
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
            Console.WriteLine("  /mock      : 查看全真白板面試框架與經典題庫 (方案 B)");
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
                case "/mock":
                    ShowMockInterviewGuide();
                    break;
                case "/reset":
                    ResetProgress();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"未知的指令：{cmd}。請使用 /status, /challenge, /hint, /exam, /mock, /reset 或 exit");
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
            Console.WriteLine($"  - 專案目錄：C:\\Users\\FIT1635\\Documents\\OOA-D");
            Console.WriteLine($"  - 完整進度表：請參閱專案目錄下的 ooad_learning_tracker.md");
            Console.ResetColor();
        }

        private static void ShowChallenge()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n🎯 【當前挑戰題目：階段七 - 事件驅動與解耦 (Observer / Domain Events)】");
            Console.WriteLine("請打開 Stage7_ObserverAndEvents/Program.cs 查看電商下單服務解耦情境：");
            Console.WriteLine("1. 【任務 7.1 - 痛點診斷】：");
            Console.WriteLine("   - 為什麼 BadOrderService 把扣庫存、發點數、開立發票、寄通知全塞在 PlaceOrder 裡會引發「建構子依賴爆炸」與違反 SRP / OCP？");
            Console.WriteLine("2. 【任務 7.2 & 7.3 - Observer / Domain Events 解耦與擴充】：");
            Console.WriteLine("   - 如何透過 OrderPlacedEvent 與 IOrderPlacedHandler，讓 OrderService 只管核心下單與發布事件？");
            Console.WriteLine("   - 試著新增「風控防刷檢查」或「大數據分析埋點」訂閱者，做到對核心 OrderService 零修改！");
            Console.WriteLine("\n💡 提示：輸入 /hint 獲取思路引導，或輸入 /mock 查看面試題庫！");
            Console.ResetColor();
        }

        private static void ShowHint()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n💡 【階段七思路提示】");
            Console.WriteLine("1. 依賴反轉升級版：發布者 (OrderService) 不再認識任何具體訂閱者服務，它只認識「事件本身 (OrderPlacedEvent)」或「事件介面 (IOrderPlacedHandler)」。");
            Console.WriteLine("2. 核心責任邊界：訂單成立是「核心職責」，扣庫存/送點數/開發票/通知則是「跨領域副作用 (Side Effects)」。");
            Console.WriteLine("3. 白板面試必問：同步事件 (In-Memory) vs. 非同步訊息佇列 (Message Queue)，以及訂閱者失敗時是否該連帶回滾核心交易？");
            Console.ResetColor();
        }

        private static void ShowExam()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n🏆 【階段七晉級考核條件】");
            Console.WriteLine("1. 精準診斷高耦合 Service 建構子依賴爆炸與測試困難的根源。");
            Console.WriteLine("2. 能以 Observer / Domain Events 完成發布者與訂閱者解耦，並透過 OCP 無痛掛載新訂閱者。");
            Console.WriteLine("3. 能在白板面試追問中清楚釐清同步事件與非同步佇列的取捨，以及跨領域副作用的失敗隔離策略。");
            Console.WriteLine("\n完成任務 7.1 ~ 7.3 後，在 AI 聊天室申請階段七考核（或隨時輸入「切換模擬面試」）！");
            Console.ResetColor();
        }

        private static void ShowMockInterviewGuide()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n🎤 【方案 B：全真 OOD 白板模擬面試指南 (Low-Level Design)】");
            Console.WriteLine("標準面試解題 4 步驟：");
            Console.WriteLine("  Step 1. 需求釐清 (Clarify Requirements)：主動提問、收斂系統邊界與核心用例");
            Console.WriteLine("  Step 2. 領域分析 (OOA Core Entities)：識別實體、值物件與聚合/組合關係");
            Console.WriteLine("  Step 3. 職責與介面設計 (OOD & Patterns)：定義類別方法簽章與套用設計模式");
            Console.WriteLine("  Step 4. 應對追問與編碼 (Handle Follow-ups)：抗住需求變更並寫出核心可測代碼");
            Console.WriteLine("\n🔥 經典面試題庫：");
            Console.WriteLine("  1. 智慧停車場系統 (Parking Lot System) ⭐⭐⭐");
            Console.WriteLine("  2. 自動販賣機 / 智慧取件櫃 (Vending Machine - State Pattern) ⭐⭐⭐⭐");
            Console.WriteLine("  3. 進階電商複合促銷與會議室預約系統 ⭐⭐⭐⭐");
            Console.WriteLine("  4. 電梯調度系統 (Elevator System) ⭐⭐⭐⭐⭐");
            Console.WriteLine("\n👉 在 AI 聊天室輸入「切換模擬面試」即可立刻開始演練！");
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
