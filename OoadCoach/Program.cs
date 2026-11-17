using System;
using System.IO;
using System.Text.Json;

namespace OoadCoach
{
    public class LearningState
    {
        public int CurrentStage { get; set; } = 5;
        public string StageName { get; set; } = "階段五：進階 OOA/D —— 值物件 (Value Object) 與裝飾者模式 (Decorator)";
        public int CurrentTask { get; set; } = 1;
        public string TaskTitle { get; set; } = "任務 5.1 & 5.2：Entity vs. Value Object 與 DateRange 防超賣區間重疊判定";
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
            Console.WriteLine($"  - 專案目錄：C:\\Users\\USER\\Desktop\\OOAD");
            Console.WriteLine($"  - 完整進度表：請參閱專案目錄下的 ooad_learning_tracker.md");
            Console.ResetColor();
        }

        private static void ShowChallenge()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n🎯 【當前挑戰題目：階段五 - 值物件 (Value Object) 與裝飾者模式 (Decorator)】");
            Console.WriteLine("請打開 Stage5_AdvancedOOAD/Program.cs 查看進階面試重構情境：");
            Console.WriteLine("1. 【任務 5.1 & 5.2 - DateRange 值物件】：");
            Console.WriteLine("   - 為什麼 Guest 是 Entity（需要 ID），而 DateRange 或 Money 是 Value Object（不可變、由值決定相等性）？");
            Console.WriteLine("   - 如何封裝 DateRange.OverlapsWith(DateRange other) 來解決訂房/會議室防超賣時段衝突判定？");
            Console.WriteLine("2. 【任務 5.3 - Decorator 裝飾者模式】：");
            Console.WriteLine("   - 如果 VIP 9 折 + 滿千折百 + 生日折 50 要能「任意動態疊加」，不改 Order 要怎麼包起來？");
            Console.WriteLine("\n💡 提示：輸入 /hint 獲取思路引導，或輸入 /mock 查看面試題庫！");
            Console.ResetColor();
        }

        private static void ShowHint()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n💡 【階段五思路提示】");
            Console.WriteLine("1. 值物件 (Value Object)：沒有獨立生命週期 ID，建立後不可修改 (Immutable)，且把日期驗證、重疊計算都封裝在自己身上！");
            Console.WriteLine("2. 兩個半開區間 [StartA, EndA) 與 [StartB, EndB) 重疊條件：StartA < EndB && StartB < EndA。");
            Console.WriteLine("3. 裝飾者模式 (Decorator)：裝飾者類別「既實作 IDiscountStrategy，建構子又包著另一個 IDiscountStrategy」！");
            Console.ResetColor();
        }

        private static void ShowExam()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n🏆 【階段五晉級考核條件】");
            Console.WriteLine("1. 清楚區分 Entity 與 Value Object，成功封裝不可變的 DateRange 值物件與重疊檢查。");
            Console.WriteLine("2. 能用 Decorator Pattern 動態組合多重優惠規則，扛住面試官的「策略疊加追問」。");
            Console.WriteLine("3. 通過白板面試官的邊界測試追問（例如退房當天同一天新房客入住算不算重疊？）。");
            Console.WriteLine("\n完成任務 5.1 ~ 5.3 後，在 AI 聊天室申請階段五考核（或隨時輸入「切換模擬面試」）！");
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
