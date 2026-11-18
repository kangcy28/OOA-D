using System;

namespace Stage6_StatePattern
{
    // =========================================================================
    // 【任務 6.1 反面教材】：把狀態機寫成到處散落的 switch / if-else
    // 痛點：每當新增一個「狀態」(例如：Maintenance 維護中、WinnerState 中獎)，
    // 你就必須回頭修改「每一個動作方法」裡的 switch，引發雙維度膨脹的維護地獄！
    // =========================================================================

    public enum MachineState
    {
        NoCoin,
        HasCoin,
        SoldOut
    }

    public class BadVendingMachine
    {
        public MachineState State { get; private set; }
        public int Stock { get; private set; }

        public BadVendingMachine(int initialStock)
        {
            if (initialStock < 0) throw new ArgumentOutOfRangeException(nameof(initialStock), "庫存不能為負數");
            Stock = initialStock;
            State = initialStock > 0 ? MachineState.NoCoin : MachineState.SoldOut;
        }

        public void InsertCoin()
        {
            switch (State)
            {
                case MachineState.NoCoin:
                    Console.WriteLine("投入硬幣成功，請選擇飲料。");
                    State = MachineState.HasCoin;
                    break;
                case MachineState.HasCoin:
                    Console.WriteLine("已經投過硬幣了，請勿重複投幣！");
                    break;
                case MachineState.SoldOut:
                    Console.WriteLine("商品已售罄，無法投幣，退回硬幣。");
                    break;
            }
        }

        public void EjectCoin()
        {
            switch (State)
            {
                case MachineState.NoCoin:
                    Console.WriteLine("尚未投幣，無法退幣。");
                    break;
                case MachineState.HasCoin:
                    Console.WriteLine("退幣成功，請取回硬幣。");
                    State = MachineState.NoCoin;
                    break;
                case MachineState.SoldOut:
                    Console.WriteLine("售罄狀態且未投幣，無幣可退。");
                    break;
            }
        }

        public void PressButton()
        {
            switch (State)
            {
                case MachineState.NoCoin:
                    Console.WriteLine("請先投入硬幣再按購買鍵！");
                    break;
                case MachineState.HasCoin:
                    Console.WriteLine("購買成功，正在掉出飲料...");
                    Stock--;
                    State = Stock > 0 ? MachineState.NoCoin : MachineState.SoldOut;
                    break;
                case MachineState.SoldOut:
                    Console.WriteLine("商品已售罄，無法出貨。");
                    break;
            }
        }
    }

    // =========================================================================
    // 【任務 6.2 正確架構】：狀態模式 (State Pattern)
    // 核心精神：將「每個狀態」昇華為獨立類別，每個狀態只管好自己該做什麼、
    // 以及執行完後要轉換到哪一個下一個狀態。
    // =========================================================================

    /// <summary>
    /// 狀態抽象契約：定義自動販賣機支援的所有外部動作
    /// </summary>
    public interface IVendingMachineState
    {
        string Name { get; }
        void InsertCoin(VendingMachine machine);
        void EjectCoin(VendingMachine machine);
        void PressButton(VendingMachine machine);
    }

    /// <summary>
    /// Context（上下文）：保有當前狀態與庫存，對外提供行為並委派給 CurrentState (Tell, Don't Ask)
    /// </summary>
    public class VendingMachine
    {
        public IVendingMachineState CurrentState { get; private set; }
        public int Stock { get; private set; }
        public bool IsLuckyWinner { get; set; }

        public VendingMachine(int initialStock)
        {
            if (initialStock < 0) throw new ArgumentOutOfRangeException(nameof(initialStock), "庫存不能為負數");
            Stock = initialStock;
            CurrentState = initialStock > 0 ? new NoCoinState() : new SoldOutState();
        }

        public void TransitionTo(IVendingMachineState newState)
        {
            CurrentState = newState ?? throw new ArgumentNullException(nameof(newState));
            Console.WriteLine($"  [狀態轉換] ➔ 目前狀態：{CurrentState.Name}");
        }

        public void ReleaseDrink()
        {
            if (Stock <= 0)
                throw new InvalidOperationException("庫存不足，無法出貨！");

            Stock--;
            Console.WriteLine($"  [出貨完成] 飲料已掉出！剩餘庫存：{Stock}");
        }

        public void InsertCoin() => CurrentState.InsertCoin(this);
        public void EjectCoin() => CurrentState.EjectCoin(this);
        public void PressButton() => CurrentState.PressButton(this);
    }

    /// <summary>
    /// 狀態 1：尚未投幣 (NoCoinState)
    /// </summary>
    public class NoCoinState : IVendingMachineState
    {
        public string Name => "尚未投幣 (NoCoin)";

        public void InsertCoin(VendingMachine machine)
        {
            Console.WriteLine("投入硬幣成功！");
            machine.TransitionTo(new HasCoinState());
        }

        public void EjectCoin(VendingMachine machine)
        {
            Console.WriteLine("尚未投幣，無法退幣。");
        }

        public void PressButton(VendingMachine machine)
        {
            Console.WriteLine("請先投入硬幣再按購買鍵！");
        }
    }

    /// <summary>
    /// 狀態 2：已投幣 (HasCoinState)
    /// </summary>
    public class HasCoinState : IVendingMachineState
    {
        public string Name => "已投幣 (HasCoin)";

        public void InsertCoin(VendingMachine machine)
        {
            Console.WriteLine("已經投過硬幣了，請勿重複投幣！");
        }

        public void EjectCoin(VendingMachine machine)
        {
            Console.WriteLine("退幣成功，請取回硬幣。");
            machine.TransitionTo(new NoCoinState());
        }

        public void PressButton(VendingMachine machine)
        {
            Console.WriteLine("購買成功，準備出貨...");
            machine.ReleaseDrink();

            if (machine.Stock > 0)
            {
                if (machine.IsLuckyWinner)
                {
                    Console.WriteLine("🎉 恭喜中獎「再來一罐」！請再按一次購買鍵領取免費飲料！");
                    machine.IsLuckyWinner = false; // 重置中獎標記
                    machine.TransitionTo(new WinnerState());
                }
                else
                {
                    machine.TransitionTo(new NoCoinState());
                }
            }
            else
            {
                Console.WriteLine("庫存已歸零！");
                machine.TransitionTo(new SoldOutState());
            }
        }
    }

    /// <summary>
    /// 狀態 3：已售罄 (SoldOutState)
    /// </summary>
    public class SoldOutState : IVendingMachineState
    {
        public string Name => "已售罄 (SoldOut)";

        public void InsertCoin(VendingMachine machine)
        {
            Console.WriteLine("商品已售罄，無法投幣，退回硬幣。");
        }

        public void EjectCoin(VendingMachine machine)
        {
            Console.WriteLine("目前無硬幣可退。");
        }

        public void PressButton(VendingMachine machine)
        {
            Console.WriteLine("商品已售罄，無法出貨。");
        }
    }

    /// <summary>
    /// 【任務 6.3 擴充狀態】狀態 4：中獎再來一罐 (WinnerState)
    /// </summary>
    public class WinnerState : IVendingMachineState
    {
        public string Name => "中獎再來一罐 (Winner)";

        public void InsertCoin(VendingMachine machine)
        {
            Console.WriteLine("恭喜中獎中！請直接按購買鍵領取免費飲料，無需投幣（狀態不變）。");
        }

        public void EjectCoin(VendingMachine machine)
        {
            Console.WriteLine("上枚硬幣已結帳，目前為免費贈品領取狀態，無法退幣（狀態不變）。");
        }

        public void PressButton(VendingMachine machine)
        {
            Console.WriteLine("🎁 兌換中獎贈品，準備免費掉出第二罐飲料...");
            machine.ReleaseDrink();

            if (machine.Stock > 0)
            {
                machine.TransitionTo(new NoCoinState());
            }
            else
            {
                Console.WriteLine("庫存已歸零！");
                machine.TransitionTo(new SoldOutState());
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== 階段六：複雜狀態機重構實戰 (State Pattern) ===\n");

            var machine = new VendingMachine(initialStock: 3);
            Console.WriteLine($"初始庫存：{machine.Stock}，初始狀態：{machine.CurrentState.Name}\n");

            Console.WriteLine("1. 測試未投幣直接購買：");
            machine.PressButton();

            Console.WriteLine("\n2. 測試投幣後退幣：");
            machine.InsertCoin();
            machine.EjectCoin();

            Console.WriteLine("\n3. 測試第 1 次正常購買（未中獎，庫存剩 2）：");
            machine.InsertCoin();
            machine.InsertCoin(); // 重複投幣防呆
            machine.PressButton();

            Console.WriteLine("\n4. 測試第 2 次購買並觸發「中獎再來一罐 (WinnerState)」：");
            machine.IsLuckyWinner = true;
            machine.InsertCoin();
            machine.PressButton(); // 掉出第 1 罐後進入 WinnerState
            machine.InsertCoin();  // 測試中獎狀態投幣不變
            machine.EjectCoin();   // 測試中獎狀態退幣不變
            machine.PressButton(); // 免費掉出第 2 罐，庫存歸零進入 SoldOutState

            Console.WriteLine("\n5. 測試售罄後投幣：");
            machine.InsertCoin();
        }
    }
}
