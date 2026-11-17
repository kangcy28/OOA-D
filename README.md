# 🧭 OOA/D 實戰工坊 (C# Edition)
> **從零開始的物件導向分析與設計：從程序化思維、SOLID 原則、設計模式到端到端可測試架構**

[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/xUnit-100%25%20Passing-brightgreen?logo=xunit)](https://xunit.net/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20%26%20SOLID-blue)](#-核心學習里程碑)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

本專案記錄了一位軟體工程師從「完全沒寫過物件導向」，歷經四大進階階段，蛻變為具備 **OOA 領域建模**、**OOD 職責分配**、**SOLID 彈性架構** 與 **單元測試驅動開發** 能力的完整實戰歷程與程式碼範例。

---

## 📊 四大階段成果總覽

| 階段 | 核心主題 | 驗收核心 | 通關成績 |
| :--- | :--- | :--- | :---: |
| **Stage 1** | **物件思維覺醒** | 消除貧血模型、Tell Don't Ask、建立資料保護守門員 | 🏆 **95 / 100** |
| **Stage 2** | **OOA 領域分析** | 自然語言萃取實體、聚合 vs. 組合辨析、拒絕技術污染 | 🏆 **100 / 100** |
| **Stage 3** | **OOD 職責分配** | GRASP 資訊專家、SOLID 五大原則、策略模式重構 | 🏆 **100 / 100** |
| **Stage 4** | **端到端實戰** | 中型飯店預訂領域模型（DDD 聚合根）、xUnit 極速單元測試 | 🏆 **100 / 100** |

詳細進度與考核答辯歷程請參閱 [ooad_learning_tracker.md](ooad_learning_tracker.md)。

---

## 🚀 各階段實戰內容與核心架構

### 📁 Stage 1: 物件思維覺醒 (`Stage1_Playground`)
* **痛點反思**：解析「資料與邏輯分離」的程序化程式碼如何引發變數散落、參數膨脹與複製貼上災難。
* **封裝（Encapsulation）**：資料私有化，建構子建立防線，方法化身守門員守護不變條件（Invariants）。
* **Tell, Don't Ask**：吩咐物件做事（如 `drink.DeductStock()`、`customer.Pay()`），不再「問出數據後在外部自己算」。

### 📁 Stage 2: OOA 領域分析 (需求拆解與概念建模)
* **名詞/動詞分析法**：精準提煉「領域實體（Entities）」、剔除「系統環境雜訊（Noise）」。
* **生命週期關聯辨析**：
  * **聚合（Aggregation）**：整體與部分生命週期獨立（例如：房間消失，房型依舊存在；借閱單銷毀，書本依然在館內）。
  * **組合（Composition）**：生死與共（例如：購物車消失，購物車項目隨之銷毀）。
* **拒絕技術污染**：分析期延後技術選擇，不讓關聯式資料庫的外鍵 ID、UI 按鈕或 DB 連線污染領域概念。
* **領域快照意識**：訂單明細必須凍結「當下購買單價（Snapshot）」，杜絕商品牌價浮動污染歷史單據。

### 📁 Stage 3: OOD 職責分配與架構抗壓 (`Stage3_Playground`)
* **GRASP 資訊專家（Information Expert）**：把計算職責分配給擁有最多必要資訊的類別（`OrderItem` 算小計、`Order` 算總額）。
* **SRP（單一職責）與 OCP（開放封閉原則）**：
  * 徹底消除訂單中易脆且膨脹的 `if-else` 活動折扣判斷。
  * 引入 **策略模式（Strategy Pattern）** 抽象出 `IDiscountStrategy`。
  * 新增「黑五現折 500」時，**零改動** `Order` 類別即可無縫擴充。
* **空物件模式（Null Object Pattern）**：實作 `NoneDiscount` 取代危險的 `null` 檢查。
* **DIP（依賴反轉）與 ISP（介面隔離）**：
  * 核心服務透過建構子注入細粒度介面（`IPayProcessor`, `ISendNotification`）。
  * 達成「高層商業邏輯不受底層具體第三方套件綁架」與「極致好 Mock 單元測試」的架構。

### 📁 Stage 4: 端到端飯店預訂系統實戰 (`Stage4_SystemDesign` & Tests)
* **真實商業案例**：線上客房預訂與退款系統 (Hotel Reservation & Cancellation System)。
* **充血領域模型**：
  * `RoomType`（房型與每晚定價，資訊專家）
  * `Room`（房間與狀態）
  * `Reservation`（聚合根：整合客戶、房間、住宿晚數計算與狀態流轉）
  * `IRefundPolicy`（動態退款策略：寬鬆階梯退款 vs. 特惠不可退款）
* **xUnit 單元測試套件 (`Stage4_SystemDesign.Tests`)**：
  * 遵循 AAA（Arrange, Act, Assert）原則。
  * 涵蓋：天數房費計算、入住前 5 天全退 (100%)、入住前 1 天半退 (50%)、不可退款策略 (0%)，以及非法日期建構子防禦。
  * **5 個測試案例在 31 毫秒內於記憶體中全數綠燈通過**，完全不依賴外部資料庫或網路 API。

---

## 🛠️ 專案結構導覽

```text
OOA-D/
├── OOAD_Learning.sln                  # 整合解決方案
├── ooad_learning_tracker.md           # 四大階段學習進度追蹤與考核記錄
├── run_coach.bat                      # 互動學習終端教練快捷啟動腳本
│
├── OoadCoach/                         # 終端互動學習助理 (CLI Assistant)
│   ├── Program.cs
│   └── OoadCoach.csproj
│
├── Stage1_Playground/                 # 階段一：程序導向重構至物件導向
│   ├── Program.cs
│   └── Stage1_Playground.csproj
│
├── Stage3_Playground/                 # 階段三：SOLID 原則與策略模式實戰
│   ├── Program.cs
│   └── Stage3_Playground.csproj
│
├── Stage4_SystemDesign/               # 階段四：飯店預訂領域核心類別庫
│   ├── Class1.cs
│   └── Stage4_SystemDesign.csproj
│
└── Stage4_SystemDesign.Tests/         # 階段四：xUnit 單元測試專案 (31ms 全綠燈)
    ├── UnitTest1.cs
    └── Stage4_SystemDesign.Tests.csproj
```

---

## 💻 快速開始 (Getting Started)

### 環境需求
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本（支援 .NET 10）

### 1. 複製儲存庫
```bash
git clone https://github.com/kangcy28/OOA-D.git
cd OOA-D
```

### 2. 執行所有單元測試
```bash
dotnet test
```
*預期輸出：所有測試 100% 通過（5 passed, 0 failed, 31 ms）*

### 3. 執行各階段實戰示範
```bash
# 執行階段三 (策略模式與折扣計算示範)
dotnet run --project Stage3_Playground

# 啟動互動學習教練
run_coach.bat
```

---

## 🌟 架構核心收穫

1. **軟體高內聚、低耦合**：資料與專屬業務邏輯永不分家，單一事實來源（Single Source of Truth）。
2. **面對變更優雅從容**：嚴守 OCP，以多型與介面隔離變更點，新業務需求不再需要回頭修改舊有程式碼。
3. **無副作用的極速測試**：純粹的領域模型不被 UI、DB 或第三方通訊污染，以毫秒級單元測試確保軟體品質！
