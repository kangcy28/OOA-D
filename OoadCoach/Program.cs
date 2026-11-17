using System;
using System.IO;
using System.Text.Json;

namespace OoadCoach
{
    public class LearningState
    {
        public int CurrentStage { get; set; } = 4;
        public string StageName { get; set; } = "🏆 全課程圓滿結業 (Graduated)";
        public int CurrentTask { get; set; } = 5;
        public string TaskTitle { get; set; } = "任務 4.5：階段四終極畢業考核 (通關滿分 100)";
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
            Console.WriteLine("\n🎯 【當前挑戰題目：任務 4.1 - 端到端系統實戰 (完整規格拆解與領域建模)】");
            Console.WriteLine("情境：飯店線上客房預訂與退訂系統 (Hotel Reservation System)。");
            Console.WriteLine("需求包含：房型/房間庫存、會員預訂、入住與退房日期計算、取消退款策略、押金與付款。");
            Console.WriteLine("\n請在 Antigravity 聊天室跟隨 AI 助教指引，開啟端到端整合實戰！");
            Console.WriteLine("💡 提示：輸入 /hint 獲取思路引導！");
            Console.ResetColor();
        }

        private static void ShowHint()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n💡 【思路提示】");
            Console.WriteLine("1. 先做 OOA：提取實體、畫出 UML 概念圖，不寫半行程式碼。");
            Console.WriteLine("2. 再做 OOD：用 GRASP 分配職責，用 SOLID 拆分介面與策略（如退款政策）。");
            Console.WriteLine("3. 進入 OOP 實作：撰寫純粹 C# 領域模型與單元測試！");
            Console.ResetColor();
        }

        private static void ShowExam()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n🏆 【階段四終極畢業考核條件】");
            Console.WriteLine("1. 具備面對中型陌生商業需求，獨立完成 OOA 概念建模的能力。");
            Console.WriteLine("2. 具備運用 OOD / SOLID / GRASP 分配職責並拆分介面的能力。");
            Console.WriteLine("3. 產出高內聚、低耦合、100% 可單元測試的優質 C# 程式碼！");
            Console.WriteLine("\n完成階段四後，恭喜你正式成為 OOA/D 專業架構實戰者！");
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
