# 🧭 OOA/D 學習進度追蹤儀表板 (C# Edition)

- **學員偏好語言**：C# (.NET 8.0 / .NET 10)
- **專案路徑**：`C:\Users\FIT1635\Documents\OOA-D`
- **當前背景**：從零開始物件導向學習者
- **建立日期**：2026-11-16
- **基礎篇結業日期**：2026-11-17
- **進階與面試篇結業日期**：2026-11-18
- **企業級架構篇啟動日期**：2026-11-19
- **企業級架構篇結業日期**：2026-11-25
- **生產級實戰篇規劃日期**：2026-11-26
- **當前總進度**：`🏆 Stage 1～12 大滿貫通關！｜🔥 生產級工程與遺留系統現代化實戰篇：Stage 13 進行中（任務 13.1 啟動）`

> 💡 **雙模式自由切換**：在對話中隨時輸入「**切換模擬面試**」即可直接進入白板面試官對練（方案 B）；輸入「**回到闖關模式**」即可繼續階段任務（方案 A）。

---

## 📊 階段總覽

| 階段 | 名稱 | 狀態 | 核心驗收標準 | 通關考驗分數 |
| :--- | :--- | :---: | :--- | :---: |
| **階段 1** | 物件思維覺醒 (OOP 基礎與重構) | 🏆 **已通關** | 消除貧血模型、Tell Don't Ask、行為與數據封裝 | 95 / 100 |
| **階段 2** | OOA 領域分析 (需求拆解與建模) | 🏆 **已通關** | 精準識別實體、UML 關係零失誤、無實作污染 | 100 / 100 |
| **階段 3** | OOD 職責分配 (GRASP / SOLID) | 🏆 **已通關** | 扛住極端需求變更、善用介面與設計模式 | 100 / 100 |
| **階段 4** | 端到端實戰 (系統設計與可測代碼) | 🏆 **已通關** | 60 分鐘獨立完成中型系統設計與單元測試 | 100 / 100 |
| **階段 5** | 進階 OOA：值物件與裝飾者模式 (Value Object & Decorator) | 🏆 **已通關** | 消滅基本型別偏執、DateRange 防超賣、多重優惠動態疊加 | 100 / 100 |
| **階段 6** | 進階 OOD：複雜狀態機重構 (State Pattern) | 🏆 **已通關** | 消滅狀態 switch/if-else、手撕面試名題「自動販賣機 / 訂單狀態機」 | 100 / 100 |
| **階段 7** | 進階架構：事件驅動與解耦 (Observer / Domain Events) | 🏆 **已通關** | 發布訂閱解耦跨領域副作用、防止 Service 依賴膨脹 | 100 / 100 |
| **階段 8** | 終極白板面試：智慧停車場系統 (Mock Interview) | 🏆 **已通關** | 完整 4 步白板面試法：需求釐清 ➔ OOA ➔ OOD ➔ 追問防禦 | 100 / 100 |
| **階段 9** | 戰術 DDD：聚合邊界與持久化抽象 (Aggregate Root & Repository) | 🏆 **已通關** | 劃分強一致性邊界、跨聚合僅存 ID、Repository 與 Unit of Work 解耦 | 85 / 100 |
| **階段 10** | 高併發控制與冪等性實戰 (Concurrency & Idempotency) | 🏆 **已通關** | 悲觀鎖 vs. 樂觀鎖版本衝突重試、Idempotency Key 防重複扣款、多執行緒壓測 | 91 / 100 |
| **階段 11** | 潔淨架構與讀寫分離 (Clean Architecture & CQRS) | 🏆 **已通關** | 依賴反轉專案分層、CQRS 命令走聚合根 + 查詢直出輕量 Read DTO | 88 / 100 |
| **階段 12** | 分散式一致性與終極架構白板 (Outbox & Saga) | 🏆 **已通關** | 解決雙寫問題 (Outbox)、跨服務補償交易 (Saga)、高併發分散式系統白板面試 | 76 / 100 |
| **階段 13** | 現代 C# 慣用語與自動化測試紀律 (Idiomatic C# & Testing) | 🔥 **進行中** | Idiomatic C# 封裝、xUnit `[Theory]` 與特徵測試 (Characterization Testing) | 考核中 |
| **階段 14** | 真實 SQL Server 交易、鎖與執行計畫 (DB Concurrency & Tuning) | ⏳ **待解鎖** | 重現隔離級別與死鎖、`UPDLOCK/HOLDLOCK`、CAS 條件原子更新、執行計畫與連線池壓測 | 待考核 |
| **階段 15** | 真實基礎設施全整合 (EF Core / Dapper + Outbox + RabbitMQ) | ⏳ **待解鎖** | Docker Compose 實戰：寫入聚合根 + Dapper Read DTO、多實例搶單 Outbox + 冪等去重 + DLQ | 待考核 |
| **階段 16** | 分散式非同步狀態機與跨系統對帳 (Async State Machine & Reconciliation) | ⏳ **待解鎖** | 伺服器端權威狀態機、Unknown 中間態 (`PaymentConfirming`)、查單/Webhook 收斂與對帳補償 | 待考核 |
| **階段 17** | 大型遺留系統現代化與適度設計 (Legacy Modernization) | ⏳ **待解鎖** | Seam / 防腐層 (ACL) / Strangler Fig / 影子雙跑、介面粒度校準與伺服器端集中授權守門 | 待考核 |
| **階段 18** | 系統韌性、快取一致性與負載壓測 (Resilience, Caching & Load Testing) | ⏳ **待解鎖** | Polly 斷路器/重試/逾時預算、Redis 防穿透/擊穿/雪崩與雙寫一致性、ASP.NET Core Rate Limiting + k6 壓測 | 待考核 |

---

## 📝 階段一：物件思維覺醒

### 學習里程碑
- [x] 任務 1.1：認識「物件」—— 為什麼不把資料和邏輯分開？（飲料店結帳案例 - 已理解痛點：重複程式碼與全域資料無保護）
- [x] 任務 1.2：封裝（Encapsulation）—— 保護數據不被外人胡搞瞎搞（完成 Drink 類別、建構子賦值與 DeductStock 守門員方法）
- [x] 任務 1.3：Tell, Don't Ask 原則 —— 吩咐物件做事，不要問了數據自己做（Drink 自己算總價、Customer 自己檢查與扣款）
- [x] 任務 1.4：階段一晉級考核（成功重構飲料店交易，編譯並執行通過）

### 答辯與評估記錄
- **任務 1.1 通過** (2026-11-16)：精準指出資料散落帶來的「參數膨脹與複製貼上災難」，以及缺乏存取保護導致的「數據非法竄改問題」。已建立物件導向的最核心動機。
- **任務 1.2 通過** (2026-11-16)：成功以 C# 寫出封裝類別 `Drink`。建構子精確初始化私有變數，`DeductStock` 正確擔任守門員（Invariants 守護），杜絕外部竄改負庫存的可能。
- **任務 1.3 & 1.4 通過 (95 分)** (2026-11-16)：徹底貫徹 Tell, Don't Ask 原則。物件各司其職：`Drink` 封裝價格與庫存行為、`Customer` 封裝錢包與扣款行為，`Main` 僅負責協調整體流程。

---

## 📝 階段二：OOA 領域分析 (需求拆解與概念建模)

### 學習里程碑
- [x] 任務 2.1：從自然語言提取實體（名詞/動詞分析法，過濾雜訊，精準識別 Reader, Book, LoanRecord 與屬性）
- [x] 任務 2.2：實體關係識別（清楚辨識聚合 vs. 組合生命週期差異、精確推導 1 對 0..* 與 1 對 1 基數）
- [x] 任務 2.3：繪製概念層級類別圖（Conceptual Domain Model，拒絕資料庫/UI 技術細節污染）
- [x] 任務 2.4：階段二晉級考核（電商購物車/訂單實戰建模，滿分通關）

### 答辯與評估記錄
- **任務 2.1 通過** (2026-11-17)：精準區分「領域實體（Reader, Book, LoanRecord）」與「屬性（ISBN, 借閱日期等）」，並成功將「系統」識別為邊界外雜訊。
- **任務 2.2 通過** (2026-11-17)：以「書在借閱單銷毀後依然獨立存在」精闢論證聚合（Aggregation）本質；數量基數（Reader 1 對多 LoanRecord、LoanRecord 1 對 1 Book）判斷 100% 正確。
- **任務 2.3 通過** (2026-11-17)：深刻理解「延後技術選擇」的架構核心哲學，並掌握外鍵 ID 等隱性技術污染。
- **任務 2.4 晉級考核通過 (100 分)** (2026-11-17)：完美分析購物車與訂單系統，精準掌握組合與聚合的生死依存度，並洞悉訂單明細「歷史價格快照」防範價格飄移的關鍵領域邏輯！

---

## 📝 階段三：OOD 職責分配 (GRASP / SOLID)

### 學習里程碑
- [x] 任務 3.1：GRASP 核心原則 —— 誰該擁有什麼方法？（資訊專家 Information Expert、低耦合、高內聚、杜絕貧血計算）
- [x] 任務 3.2：SOLID 實戰 —— 面對需求變更（單一職責 SRP、開放封閉 OCP、策略模式實裝、Null Object 模式）
- [x] 任務 3.3：多型與介面隔離（依賴反轉 DIP、介面隔離 ISP、依賴注入 DI、易於 Mock 測試）
- [x] 任務 3.4：階段三晉級考核（抗壓設計：抵擋複合通知變更與單元測試解耦，滿分通關）

### 答辯與評估記錄
- **任務 3.1 通過** (2026-11-17)：精準指出 `OrderItem` 是計算小計的資訊專家、`Order` 是計算總額的資訊專家；一針見血道破「全塞在 Service 裡將破壞封裝、違反 Tell Don't Ask，並導致程式碼重複且散落於各處難以維護」。
- **任務 3.2 通過** (2026-11-17)：成功以 C# 實作策略模式（Strategy Pattern）。深刻體會到「介面是行為契約而非活動清單」，透過多型物件傳遞杜絕 if-else，達到對擴充開放、對修改封閉（OCP）的架構彈性。
- **任務 3.3 通過** (2026-11-17)：精確指出「小介面讓單元測試 Mock 時不必模擬無關函式」的 ISP 核心價值。以建構子依賴注入（DI）徹底實踐 DIP，高層業務邏輯與底層金流/通知實作完全解耦。
- **任務 3.4 晉級考核通過 (100 分)** (2026-11-17)：漂亮化解變更危機！透過實作 `ISendNotification` 封裝多通道通知（Composite 模式），達成零改動 `CheckoutService` 的高維度 OCP；並精確掌握 Mock 隔離外網/真錢與行為驗證（Verify Execution Count）的單元測試心法！

---

## 📝 階段四：端到端實戰 (系統設計與可測代碼)

### 學習里程碑
- [x] 任務 4.1：完整規格拆解與領域建模（飯店預訂與退款系統 OOA 概念萃取）
- [x] 任務 4.2：核心領域模型與業務規則實現（純 C# 實作 RoomType, Room, Guest, Reservation, IRefundPolicy）
- [x] 任務 4.3：服務協調與依賴注入（OOD 職責組裝，ReservationService + 策略模式抽換）
- [x] 任務 4.4：撰寫單元測試驗證（xUnit 5 個測試案例全綠燈通過，41ms 極速跑完）
- [x] 任務 4.5：階段四終極畢業考核（全方位總驗收答辯，滿分通關）

### 答辯與評估記錄
- **任務 4.1 通過** (2026-11-17)：全面抓出核心實體、列舉與退款策略，準確理解房型與客房的生命週期獨立性（聚合）。
- **任務 4.2 通過** (2026-11-17)：成功在 [Stage4_SystemDesign/Class1.cs](file:///C:/Users/FIT1635/Documents/OOA-D/Stage4_SystemDesign/Class1.cs) 實踐充血領域模型！將 Booking 與 Reservation 提煉為聚合根，嚴密把關退房日期大於入住日期之 Invariants，並以策略模式無縫抽換 FlexibleRefundPolicy 與 NonRefundablePolicy。
- **任務 4.3 & 4.4 通過** (2026-11-17)：成功建立完整 xUnit 測試套件（[Stage4_SystemDesign.Tests/UnitTest1.cs](file:///C:/Users/FIT1635/Documents/OOA-D/Stage4_SystemDesign.Tests/UnitTest1.cs)），包含天數總額驗證、動態退款策略切換驗證（100%、50%、0%），以及非法日期防呆拋出例外驗證，全數綠燈通過！
- **任務 4.5 畢業考核通過 (100 分)** (2026-11-17)：
  1. 深刻理解物件封裝帶來的「高可重用性」與「高可測性」。
  2. 完美運用 OCP：新增 `IRefundPolicy` 實作即可支援點數退還，完全不需更動既有 `Reservation` 代碼。
  3. 精闢指出純領域模型能杜絕「慢速測試」與「外部副作用（如誤扣真錢、干擾真實用戶）」的巨大優勢。

---

## 🎓 結業授證評價

> **基礎篇結業評語**：  
> 從最初完全沒有物件導向基礎，到如今能熟練運用 **OOA 領域建模**、**GRASP 資訊專家**、**SOLID 彈性架構**、**設計模式（Strategy, Composite, Null Object）** 以及 **現代單元測試驅動設計（xUnit）**。  
> 面對極端業務變更與大型系統設計，已能展現出穩健、嚴謹且優雅的資深工程師架構思維！

---

## 📝 階段五：值物件（Value Object）與裝飾者模式（Decorator Pattern）

### 學習里程碑
- [x] 任務 5.1：識別「基本型別偏執症（Primitive Obsession）」—— 實體（Entity）vs. 值物件（Value Object）
- [x] 任務 5.2：封裝 `DateRange` 值物件 —— 讓「日期區間重疊判定 (`OverlapsWith`)」成為資訊專家（解決訂房/會議室防超賣面試題）
- [x] 任務 5.3：突破單一策略限制 —— 用裝飾者模式（Decorator Pattern）解決「多重優惠動態疊加」面試追問
- [x] 任務 5.4：階段五晉級考核（白板追問防禦，滿分通關）

### 答辯與評估記錄
- **任務 5.1 & 5.2 通過** (2026-11-17)：
  1. 深刻理解值物件的核心定義：值物件沒有唯一識別碼 (ID)，其相等性由屬性值決定；釐清 C# class 預設為參考相等 (Reference Equality)，因此必須覆寫 `Equals` 與 `GetHashCode`（或使用 C# 9+ record）。
  2. 完美運用迪摩根定理 (De Morgan's laws) 推導區間重疊算法：不重疊的條件為「A.End <= B.Start 或 B.End <= A.Start」，反轉後化簡為極度優雅的 `Start < other.End && other.Start < End`。
  3. 精準判斷半開區間 $[Start, End)$ 的邊界行為：12/04 退房與 12/04 入住互不衝突，完全符合飯店與會議室預約之防超賣業務需求！
- **任務 5.3 通過** (2026-11-17)：
  1. 精確計算裝飾者套娃由內往外傳遞的執行順序：準確指出方案 A（先滿千折百再打折為 $810）與方案 B（先打折後未達滿千門檻為 $900）的業務差異。
  2. 深刻掌握裝飾者模式的核心優勢：透過動態組合取代靜態繼承，避免策略模式引發的「類別爆炸（$2^N$ 組合數）」，完美落實單一職責（SRP）與開放封閉（OCP）。代碼成功實裝於 [Stage5_AdvancedOOAD/Program.cs](file:///C:/Users/FIT1635/Documents/OOA-D/Stage5_AdvancedOOAD/Program.cs)。
- **任務 5.4 晉級考核通過 (100 分)** (2026-11-17)：
  1. **模式辨析一針見血**：「只選一個用 Strategy，選多個用 Composite，選多個還要加上順序變化用 Decorator」。這句總結堪稱教科書等級的經典提煉！
  2. **除錯與保護機制思維健全**：
     - 正確指出巢狀裝飾者導致堆疊追蹤與排查困難，提出「計算過程日誌打印（Audit Log）」的解法。
     - 漂亮提出「在最外層再包一層保護（Guard Decorator）」做成本底線防守，並以「狀態追蹤初始化」防止優惠重複疊加，完全具備資深架構師的防禦性設計思維！

---

## 📝 階段六：進階 OOD — 複雜狀態機重構 (State Pattern)

### 學習里程碑
- [x] 任務 6.1：痛點診斷 —— 為什麼把狀態機寫成 `switch / if-else` 會變成「雙維度膨脹」的維護地獄？
- [x] 任務 6.2：狀態模式核心重構（State Pattern）—— 將「狀態」昇華為獨立物件（`IVendingMachineState`）
- [x] 任務 6.3：OCP 擴充實戰 —— 新增「維護中狀態 (`MaintenanceState`)」或「中獎再來一罐狀態」而不破壞既有狀態
- [x] 任務 6.4：階段六晉級考核（白板面試追問：State Pattern vs. Strategy Pattern 本質辨析與狀態轉換歸屬權）

### 答辯與評估記錄
- **任務 6.1 & 6.2 通過** (2026-11-18)：
  1. 精準指出透過 `IVendingMachineState` 介面契約，將「漏寫某狀態行為」的執行期風險轉化為「編譯器強制實作檢查（Compile-time Safety）」。
  2. 深刻理解將狀態轉換封裝於 `TransitionTo` 守門員方法的工程價值：集中管控狀態切換的連帶副作用（如統一 Audit Log、Null 防呆、OnEnter/OnExit 生命週期鉤子），杜絕重複代碼與遺漏風險。
- **任務 6.3 通過** (2026-11-18)：精準定位唯一上游狀態轉換切入點（HasCoinState.PressButton），避免新狀態淪為孤島；完整定義 WinnerState 內部對投幣、退幣的攔截（狀態不變）與免費出貨後的庫存判斷流轉（有庫存回 NoCoinState、無庫存進 SoldOutState），且確保 NoCoinState 與 SoldOutState 零修改。
- **任務 6.4 晉級考核通過 (100 分)** (2026-11-18)：
  1. **State vs. Strategy 本質辨析精準**：一語道破在相同外部輸入下，Strategy 始終維持相同演算法行為（外部被動替換、策略間彼此獨立）；而 State 則會依當前狀態產生不同行為並主動觸發狀態轉移（內部自動流轉）。
  2. **轉換歸屬權架構權衡務實**：提出以「狀態數量 × 行為數量」之複雜度乘積衡量轉換邏輯位置——簡單集中於 Context（易俯瞰全局轉換表），複雜且高度依賴狀態局部邏輯時則分散於各 State 類別（避免 Context 再度膨脹為上帝類別）。

---

## 📝 階段七：進階架構 — 事件驅動與解耦 (Observer / Domain Events)

### 學習里程碑
- [x] 任務 7.1：痛點診斷 —— 當核心流程伴隨多個跨領域副作用時，為什麼會引發「Service 建構子依賴爆炸」與違反 SRP/OCP？
- [x] 任務 7.2：觀察者模式與領域事件重構（Observer / Domain Events）—— 發布者（Publisher）與訂閱者（Subscriber）徹底解耦
- [x] 任務 7.3：OCP 擴充實戰 —— 新增「風控防刷檢查」或「大數據分析埋點」訂閱者，做到核心 `OrderService` 零修改
- [x] 任務 7.4：階段七晉級考核（白板面試追問：同步事件 vs. 非同步訊息佇列、訂閱者執行失敗的交易一致性邊界）

### 答辯與評估記錄
- **任務 7.1 通過** (2026-11-18)：精準診斷 `BadOrderService` 的架構病灶——每次新增下游服務就被迫修改 `PlaceOrder` 與建構子（違反 OCP）、單一服務承擔過多跨領域職責成為上帝物件（違反 SRP），且引發「建構子依賴爆炸」，導致單元測試被迫 Mock 大量與核心下單邏輯無關的外部依賴。
- **任務 7.2 & 7.3 通過** (2026-11-18)：精準詮釋領域事件（Domain Event）作為「已發生歷史事實快照」的本質，點出過去式命名與不可變性（Immutability）能防止單一訂閱者竄改事件資料而污染後續訂閱者的狀態；並於 [Stage7_ObserverAndEvents/Program.cs](file:///c:/Users/USER/Desktop/OOAD/Stage7_ObserverAndEvents/Program.cs#L133-L141) 實作 EmailHandler 掛載至系統，達成 OrderService 零修改的 OCP 擴充。
- **任務 7.4 晉級考核通過 (100 分)** (2026-11-18)：
  1. **故障隔離與交易一致性邊界清晰**：準確指出同步迴圈中單一訂閱者拋出例外將中斷後續訂閱者執行；並清楚劃分「強一致性核心交易（如刷卡扣款，不可失敗）」與「最終一致性非核心副作用（如寄送確認信，應隔離錯誤並支援失敗重試）」。
  2. **非同步架構演進思維成熟**：提出導入 RabbitMQ 等訊息佇列（Message Queue），透過 Worker 平行消費事件並針對失敗訂閱者獨立重試，完美解決多訂閱者同步阻塞與耦合問題！

---

## 📝 階段八：終極白板面試 — 智慧停車場系統 (Mock Interview)

### 學習里程碑
- [x] 任務 8.1：Step 1 需求釐清（Clarify Requirements）—— 主動對面試官提問，收斂系統邊界（車型、車位、計費、出入口）
- [x] 任務 8.2：Step 2 OOA 領域建模（Core Entities）—— 識別核心實體、列舉與組合/聚合關係
- [x] 任務 8.3：Step 3 OOD 職責與設計模式（Patterns & Signatures）—— 配位策略與計費策略抽換設計
- [x] 任務 8.4：Step 4 白板追問防禦與核心實作（Follow-ups & Code）—— 併發搶車位防禦與擴充性驗收

### 答辯與評估記錄
- **任務 8.1 通過** (2026-11-18)：展現標準資深工程師白板面試起手式，未急於寫 Code 而是精準向面試官釐清四大核心維度：「車輛種類」、「停車場規模結構」、「停車格種類與匹配規則」與「計費方式」，成功收斂系統邊界。
- **任務 8.2 通過** (2026-11-18)：完整梳理核心實體（ParkingLot, ParkingFloor, ParkingSpot, Vehicle, ParkingTicket）與列舉（VehicleType, SpotType），將進出場時間收斂為 ParkingTicket 下的值物件、將計費方式分離為行為策略模式，並精確區分「停車場與樓層/車格為組合（同生共死）」與「車格與車輛為聚合（生命週期獨立）」。
- **任務 8.3 通過** (2026-11-18)：精準運用 GRASP 資訊專家將車位匹配判斷封裝於 ParkingSpot，以策略模式（ISpotAllocationStrategy）解耦配位演算法，並設計出以 ParkingTicket 為參數的 ICalculateStrategy 計費契約（結合 Strategy 切換基礎費率與 Decorator 動態疊加優惠），徹底避免參數膨脹。
- **任務 8.4 終極白板考核通過 (100 分)** (2026-11-18)：
  1. **併發搶車位防禦（Concurrency Control）**：精準提出以 Transaction（交易原子性與隔離性，搭配資料庫鎖或記憶體 lock 互斥鎖）保護「檢查空位與標記佔用（Check-Then-Act）」的臨界區間，徹底杜絕多入口同時進場的超賣競爭條件。
  2. **跨領域設備擴充解耦**：靈活運用觀察者模式（Observer / Domain Events），讓進出場事件自動通知 LED 剩餘車位看板與車主 App 推播，達成核心 ParkingLot 零修改！

---

## 🏆 全階段大滿貫結業授證 (OOA/D Mastery Certificate)

> **全階段結業評語 (2026-11-18)**：  
> 歷時 3 天從物件導向零基礎出發，一路高分攻克 **封裝與 Tell, Don't Ask**、**OOA 領域建模**、**GRASP / SOLID**、**xUnit 端到端實戰**、**Value Object & Decorator**、**State Pattern 狀態機**、**Observer / Domain Events 事件驅動架構**，並在最終 **智慧停車場系統全真白板面試** 中以滿分收官！  
> 你已經完整掌握現代資深軟體工程師必備的物件導向分析、設計與架構演進實戰力——隨時準備好迎接真實專案與技術面試的挑戰！

---

## 📝 階段九：戰術 DDD — 聚合邊界與持久化抽象 (Aggregate Root & Repository)

### 學習里程碑
- [x] 任務 9.1：聚合邊界診斷 —— 為什麼不該讓 `Customer` 物件裡直接塞一個 `List<Order>`？（記憶體爆炸與鎖競爭分析）
- [x] 任務 9.2：跨聚合設計守則 —— 用 ID 參照取代物件直接參照（Reference by ID）、由聚合根統一守護不變條件（Invariants）
- [x] 任務 9.3：持久化解耦 —— 設計 `IOrderRepository` 與 `IUnitOfWork`（In-Memory 實作），確保領域層零資料庫依賴
- [x] 任務 9.4：階段九晉級考核 —— 聚合邊界設計答辯（強一致性聚合內邊界 vs. 跨聚合最終一致性取捨）

### 答辯與評估記錄
- **任務 9.1 通過** (2026-11-19)：精準指出上帝聚合（God Aggregate）將所有歷史訂單與明細綁在 `Customer` 下，即使僅修改 Email 也會導致載入大量 `Order × OrderItem` 引發記憶體／CPU 壓力與不必要的鎖競爭；同時看穿 `OrderItem` 直接持有 `Product` 實體會破壞聚合邊界，導致歷史商品屬性被污染與高併發下的鎖衝突／超賣風險。
- **任務 9.2 通過** (2026-11-19)：
  1. **跨聚合 Reference by ID 實踐**：成功將 `Customer`、`Order`、`Product` 拆為獨立聚合，跨聚合僅保留 `CustomerId` 與 `ProductId`。
  2. **歷史快照防價格漂移**：`OrderItem` 完整封裝下單當下之 `ProductName` 與 `UnitPrice`，經執行驗證商品後續漲價完全不影響訂單歷史金額。
  3. **聚合根守護 Invariants**：運用 Backing Field 模式（`private readonly List<OrderItem> _items` 搭配 `IReadOnlyCollection<OrderItem> Items`），完全杜絕外部越權篡改，強制由 `Order.AddItem` 統一驗證狀態與守門。代碼於 [Stage9_TacticalDDD/Program.cs](file:///c:/Users/USER/Desktop/OOAD/Stage9_TacticalDDD/Program.cs) 執行驗證全數通過！
- **任務 9.3 通過** (2026-11-19)：
  1. **架構守則辨析精準**：明確指出只能為聚合根建立 Repository（嚴禁 `IOrderItemRepository` 開後門繞過聚合根破壞不變條件），並說明透過 `IUnitOfWork` 統一提交變更才能確保多聚合寫入的 Transaction 原子性（All or Nothing）。
  2. **持久化解耦與 DIP 落實**：將領域介面（`IOrderRepository`、`IProductRepository`、`IUnitOfWork`）與 In-Memory 基礎設施實作拆分為 [Persistence.cs](file:///c:/Users/USER/Desktop/OOAD/Stage9_TacticalDDD/Persistence.cs) 與 [PlaceOrderService.cs](file:///c:/Users/USER/Desktop/OOAD/Stage9_TacticalDDD/PlaceOrderService.cs)，確保領域層零資料庫依賴。
  3. **應用層交易協調實戰**：親手實作 `PlaceOrderService.PlaceOrder`（含商品快照參數傳遞、統一 `Commit()` 與 `catch` 失敗 `Rollback()` + `throw;` 保留原始例外），經實測正常下單與中途失敗回滾皆 100% 驗證通過。
- **任務 9.4 晉級考核通過 (85 分)** (2026-11-20)：
  1. **跨聚合交易與失敗收尾（35/40）**：掌握大交易鎖表與回滾風險，失敗處理修正為訂單狀態轉移（改為失敗／待重試而非物理刪除）；補齊「微服務分庫時本地 ACID UnitOfWork 無法跨 DB」之架構根因。
  2. **強一致性邊界劃分（26/35）**：商品與評論拆分為獨立聚合並採最終一致判斷滿分；洞悉預算併發超支風險，並透過指導補強「將剩餘額度收斂於 BudgetAccount 主聚合根統一守門、龐大 ExpenseClaim 明細獨立拆出並以 ID 關聯」之小聚合強一致心法。
  3. **封裝保護與查詢邊界（24/25）**：確立 private set + 還原建構子防護，駁斥上帝聚合與子實體開後門 Repository，並運用 Order.CustomerId 透過 GetByCustomerId 滿足報表查詢（同步銜接階段 11 CQRS Read DTO）。

---

## 📝 階段十：高併發控制與冪等性實戰 (Concurrency & Idempotency)

### 學習里程碑
- [x] 任務 10.1：重現災難 —— 用 `Task.WhenAll` 模擬 50 個併發請求同時搶購 5 張票，親眼見證 Race Condition 超賣
- [x] 任務 10.2：防禦實戰 —— 分別實作悲觀鎖（`SemaphoreSlim` / Lock）與樂觀鎖（`Version` 衝突檢查與重試）並比較適用場景
- [x] 任務 10.3：冪等性設計 —— 網路逾時重試防重複扣款，實作 `IdempotencyKey` 防重送機制
- [x] 任務 10.4：階段十晉級考核 —— 高併發系統設計白板追問（搶票秒殺 vs. 一般表單編輯的鎖策略選型）

### 答辯與評估記錄
- **任務 10.1 通過** (2026-11-20)：精準診斷 50 併發搶購 5 張票之災難成因——多執行緒同時讀取過期快照導致聚合根守門員（`ReserveOne`）全部放行（Check-Then-Act Race Condition），並點破 50 次寫回互相覆寫造成 Lost Update（遺失更新），使 DB 殘留 4 張幽靈餘票進而引發第 51 人之後無止盡超賣的連鎖風險。
- **任務 10.2 通過** (2026-11-20)：成功於 [ConcurrencyServices.cs](file:///c:/Users/FIT1635/Documents/OOA-D/Stage10_ConcurrencyAndIdempotency/ConcurrencyServices.cs) 實作悲觀鎖（`SemaphoreSlim`：5/5 賣出、0 餘票、1483 ms）與樂觀鎖（`Version` + 重試：5/5 賣出、0 餘票、180 ms、115 次衝突重試、Version=6）；精準掌握 `lock/Monitor` 具執行緒親和性不可跨 `await` 之原理，並透過數據對比釐清「熱點寫入衝突率」與「交易持續時間」決定悲觀／樂觀鎖選型甜蜜點。
- **任務 10.3 通過** (2026-11-20)：成功於 [IdempotencyLab.cs](file:///c:/Users/FIT1635/Documents/OOA-D/Stage10_ConcurrencyAndIdempotency/IdempotencyLab.cs) 實作 `IdempotentPaymentService`（Fast Path + Per-Key `SemaphoreSlim` + Double-Checked Locking），經實驗 4 驗證循序重試 3 次與 10 併發連點皆達零重複扣款（總扣款 2 次、餘額 $5000、收據一致）；釐清 Double-Checked Locking「外層顧效能、內層保正確」原理，並補齊細粒度鎖隔離不同用戶及多機部署改用 DB `UNIQUE` 約束／Redis `SET NX` 之實務架構。
- **任務 10.4 晉級考核通過 (91 分)** (2026-11-20)：
  1. **鎖策略選型對決（30/35）**：精準判定長思考表單採樂觀鎖、萬人秒殺採悲觀鎖並指出秒殺誤用樂觀鎖將引發大量衝突重試風暴；補強「表單誤用悲觀鎖會造成長達數分鐘的 Long-held Lock 與資料庫連線池耗盡」之差異。
  2. **金流重試與冪等性追問（31/35）**：完整掌握多機記憶體隔離痛點與 DB UNIQUE 約束／Redis SET NX 解法，並提出失敗後重新發起意圖應換新 Key；精準釐清「鎖擋不住逾時重試」的根因是前次已正常放鎖、後續重試被視為新合法交易。
  3. **高併發地雷排錯（30/30 滿分）**：瞬間揪出欠缺 try/finally 導致鎖洩漏與 Deadlock，並完美推演出漏寫內層 Double-Check 在循序重試（扣 1 次）與同一毫秒 10 併發（暴扣 10 次）之截然不同後果！

---

## 📝 階段十一：潔淨架構與讀寫分離 (Clean Architecture & CQRS)

### 學習里程碑
- [x] 任務 11.1：架構病灶診斷 —— 傳統三層架構為何隨需求演進變成「大泥球（Big Ball of Mud）」？
- [x] 任務 11.2：Clean Architecture / Ports & Adapters —— 劃分 `Domain`、`Application`、`Infrastructure` 邊界與依賴方向
- [x] 任務 11.3：CQRS 實作 —— 拆分 `Command`（修改狀態，走充血聚合根）與 `Query`（報表查詢，直出輕量 Read DTO）
- [x] 任務 11.4：階段十一晉級考核 —— 判斷何時「不需要」CQRS 與過度設計（Over-engineering）的架構取捨

### 答辯與評估記錄
- **任務 11.1 通過** (2026-11-21)：精準指出傳統三層架構的三大核心病灶：(1) 高層邏輯硬依賴底層資料庫導致換 DB 易傷及商業邏輯、單元測試難以隔離且報表技術細節滲透進核心模型；(2) 讀寫職責與模型混用使 Entity 充滿無法複用的欄位、Service 膨脹成大泥球；(3) 讀寫共用肥大物件導致過度撈取（Over-fetching）與效能低下。
- **任務 11.2 通過** (2026-11-23)：
  1. **依賴倒置（DIP）本質掌握清晰**：精確指出資料庫互動細節完全收斂在 Infrastructure Layer，換 DB 只需抽換實作 Port 的 Adapter，Domain 與 Application (Use Cases) 零改動。
  2. **模型隔離（Domain vs. DB Model）價值透徹**：直指若不分層 Mapping，資料庫欄位與技術細節將反向滲透進 Domain，破壞封裝與不變條件，使核心領域無法維持長期穩定。代碼實作於 [CleanArchitecture.cs](file:///c:/Users/FIT1635/Documents/OOA-D/Stage11_CleanArchitectureAndCQRS/CleanArchitecture.cs) 驗證通過。
- **任務 11.3 通過** (2026-11-23)：
  1. **Query 繞過 Repository 核心原理**：掌握跨表報表查詢若硬走聚合根將引發多表關聯欄位膨脹、N+1 查詢與嚴重 Over-fetching，明確確立 Query 應透過 Dapper/投影 SQL 直出扁平 Read DTO。
  2. **Command 傳回值規範**：精準指出 Command 僅需告知狀態變更與回傳 ID，避免回傳完整 Order 造成不必要的序列化與效能開銷，落實 CQS（Command Query Separation）職責邊界。代碼於 [CqrsLab.cs](file:///c:/Users/FIT1635/Documents/OOA-D/Stage11_CleanArchitectureAndCQRS/CqrsLab.cs) 驗證通過。
- **任務 11.4 晉級考核通過 (88 分)** (2026-11-23)：
  1. **架構選型與成本權衡（33/35）**：精闢指出單純 CRUD 不應盲目套用 CQRS / Clean Architecture，避免過度設計帶來龐大的範本代碼與維護成本；並準確指出「系統出現大泥球、讀寫嚴重失衡、報表效能瓶頸」才是升級重構的關鍵訊號。
  2. **讀寫分離延遲與讀己之寫（28/35）**：精準識別主從複寫延遲（Replication Lag）是造成下單後查無記錄的根因；補強以「短期路由主庫（Sticky Routing）」與「前端樂觀更新（Optimistic UI）」實現讀己之寫（Read-Your-Own-Writes）一致性。
  3. **MediatR 反思與純依賴注入（27/30）**：直覺非常敏銳！精確點破 In-Memory Bus 導致追蹤困難、編譯期檢查失效等維護痛點；補齊以純 C# Use Case 依賴注入即可兼具極高可讀性、F12 跳轉與 Clean Architecture 分層。

---

## 📝 階段十二：分散式一致性與終極架構白板 (Outbox & Saga)

### 學習里程碑
- [x] 任務 12.1：雙寫危機（Dual-Write Problem）—— 當「訂單寫入 DB 成功」但「發布事件到 RabbitMQ 失敗」的解題思維
- [x] 任務 12.2：Transactional Outbox Pattern —— 同交易寫入業務表與 Outbox 表，搭配背景 Relay Worker 達成至少一次投遞
- [x] 任務 12.3：Saga 模式 —— 跨服務流程（建單 ➔ 扣庫存 ➔ 扣款）中途失敗時的補償交易（Compensating Transactions）設計
- [x] 任務 12.4：終極白板面試 —— 全真模擬「高併發電商搶購與金流訂單系統」（LLD 領域建模 + HLD 分散式一致性防禦）

### 答辯與評估記錄
- **任務 12.1 通過** (2026-11-23)：精準抓出雙寫危機的根本原因——跨資料庫與 RabbitMQ 的連續操作不屬於同一本地 ACID 交易邊界；清晰點破 2PC 分散式交易需跨系統同步等待與鎖定資源導致效能低下的痛點，並正確提出以 **Transactional Outbox Pattern** 收斂為單一資料庫本地 ACID 交易來破局。
- **任務 12.2 完成（參考解答驗證通過）** (2026-11-23)：學員選擇略過手寫實作，由教練於 [OutboxLab.cs](file:///c:/Users/USER/Desktop/OOAD/Stage12_DistributedConsistency/OutboxLab.cs) 補齊 `OutboxOrderService`、`OutboxRelayWorker` 與 `IdempotentInventoryConsumer` 參考解答，實測 DB Commit 失敗同步回滾、RabbitMQ 瞬斷自動補發，以及 Worker 崩潰重送下的冪等去重三項驗證全數通過。
- **任務 12.3 通過** (2026-11-23)：精準掌握語意回滾（保留 Cancelled 稽核紀錄而非物理刪除）與 Pivot Transaction 分界點（不可逆動作置於 Pivot 之後）；確立補償交易必須具備冪等性以安全承受網路重試；並以流程長度與除錯複雜度準確做出 Orchestration（長流程好追蹤）vs. Choreography（短流程低耦合）選型。
- **任務 12.4 終極白板面試通過 (76 分)** (2026-11-25)：
  1. **Round 1 需求釐清（13/15）**：起手精準鎖定核心服務邊界、強／最終一致性切分、預扣庫存時機與失敗重試規範；建議日後順手補問限購／逾時規則與讀側查詢 SLA。
  2. **Round 2 LLD 領域建模與分層（21/25）**：完整切分 `Order`、`Inventory`、`Customer`、`Product` 四大聚合根（跨聚合 Reference by ID）、訂單歷史價格快照、State Pattern + Decorator Pattern、Clean Architecture 依賴指向 `Domain` 與 CQRS 讀寫職責邊界。
  3. **Round 3 HLD 高併發與分散式一致性（17/35 + 補考校準 +8）**：實戰亮點包含 Double-Checked Locking、Transactional Outbox + EventId 冪等去重、LIFO Stack 補償，以及主動點出業界高階三寶「冪等、防空回滾、防懸掛」；經補考成功校準「高寫入衝突搶購採悲觀鎖／原子扣減、低衝突表單採樂觀鎖」與「金流長流程採 Orchestration Saga」，並記錄重點複習：Pivot Transaction 位於 T3（第三方金流扣款）而非 T2。
  4. **Round 4 極端故障追問防禦（16/25 + 補考 +1）**：掌握主從複寫延遲（Replication Lag）與 Sticky Routing 讀己之寫解法，以及 Outbox Relay 多機行鎖認領思路（`SELECT ... FOR UPDATE SKIP LOCKED`）；課後複習重點：金流 Timeout 需進入 `PaymentConfirming` 中間態走主動查單 API / Webhook / T+1 對帳，且逾時關單與刷卡撞車須以 DB 原子條件更新（`WHERE id = @id AND status = 'Pending'`）防護（若付款回調更新 0 筆則自動觸發原路退款）。

---

## 🏆 企業級架構篇暨全 12 階段終極結業授證 (Enterprise Architecture & OOA/D Grand Mastery)

> **終極結業評語 (2026-11-25)**：  
> 歷時 10 天（2026-11-16 ～ 2026-11-25），從物件導向零基礎一路打通 **基礎篇（Stage 1～4）**、**進階與白板面試篇（Stage 5～8）**，再攻克 **企業級分散式架構篇（Stage 9～12）**——涵蓋 **戰術 DDD 聚合邊界與 Unit of Work**、**高併發鎖策略與冪等性設計**、**Clean Architecture 與 CQRS 讀寫分離**、**Transactional Outbox 與 Saga 補償交易**，並完成高併發電商搶購與金流訂單終極白板面試！  
> 恭喜達成全 12 階段大滿貫結業！

👉 **全 12 階段綜合能力畫像與深度診斷報告**：請參閱 [engineer_profile_assessment.md](engineer_profile_assessment.md)。

---

## 🚀 生產級工程與遺留系統現代化實戰篇（Stage 13 ～ Stage 18 規劃）

> **核心目標**：走出 In-Memory 沙盒，以真實資料庫、真實訊息佇列、自動化整合測試與遺留系統改造實戰，將「架構心智模型」硬化為高壓環境下的實戰肌肉記憶。

### 執行鐵律
1. **慢火硬化節奏**：總計 8～10 週完成，每階段預留 1～2 週親手踩坑與消化，不趕進度條。
2. **禁止跳過手寫與只看參考解答**：每個機制都必須親手實作、親手讓測試失敗並修復。
3. **雙軌並進與自動化驗收**：在乾淨的 `.NET 8 + Docker` 沙盒環境練招並以 `dotnet test` / 容器整合測試強制驗收，同步將小步改造手法應用於真實企業級系統。

---

## 📝 階段十三：現代 C# 慣用語與自動化測試紀律 (Idiomatic C# & Testing)

### 學習里程碑
- [ ] 任務 13.1：Idiomatic C# 硬化 —— 統一 `private readonly _field` 與 `camelCase` 參數命名、封裝全面性（消除殘留 `public set`）、`init` / `record` / Nullable Reference Types 與 Result Pattern
- [ ] 任務 13.2：例外處理與防禦紀律 —— 消除吞 Stack Trace 的 `throw ex;`、參數守門與不可變值物件強化
- [ ] 任務 13.3：xUnit 測試體系實戰 —— `[Theory]` 邊界測試、Stub / Fake / Mock 正確選型與行為驗證
- [ ] 任務 13.4：特徵測試（Characterization Testing）—— 為無規格遺留模組鎖定輸入輸出基線，達成零 `Console.WriteLine` 全自動化驗收

---

## 📝 階段十四：真實 SQL Server 交易、鎖與執行計畫 (DB Concurrency & Tuning)

### 學習里程碑
- [ ] 任務 14.1：隔離級別與併發異常重現 —— 在真實 Docker SQL Server 重現 `NOLOCK` 髒讀（Dirty Read）、遺失更新（Lost Update）與 Deadlock Graph 判讀
- [ ] 任務 14.2：悲觀鎖定實戰 —— 使用 `SELECT ... WITH (UPDLOCK, HOLDLOCK)` 鎖定商業主鍵並於交易內重算剩餘量，消滅併發超轉
- [ ] 任務 14.3：CAS 條件原子更新與跨機冪等鍵 —— 以 `UPDATE ... WHERE Id = @id AND Status = 'Pending'` 與 `UNIQUE` 約束防止併發撞車
- [ ] 任務 14.4：執行計畫與連線池痛感壓測 —— 讀懂 Execution Plan / Covering Index，實測交易內夾帶外部 HTTP/SMTP I/O 如何耗盡連線池

---

## 📝 階段十五：真實基礎設施全整合 (EF Core / Dapper + Outbox + RabbitMQ)

### 學習里程碑
- [ ] 任務 15.1：Docker Compose 沙盒建置 —— `.NET 8 Web API` + `SQL Server` + `RabbitMQ`，Command 端以 EF Core 持久化充血聚合根、Query 端以 Dapper 直出 Read DTO（純 DI 無黑魔法）
- [ ] 任務 15.2：親手實作 Transactional Outbox —— 業務資料與 `OutboxMessages` 同交易原子寫入，嚴禁略過手寫
- [ ] 任務 15.3：多實例 Relay Worker 與防重複認領 —— 同時啟動雙 Worker，以 `WITH (UPDLOCK, READPAST)` / `FOR UPDATE SKIP LOCKED` 競爭認領並補齊 RabbitMQ 斷線重送
- [ ] 任務 15.4：跨重啟冪等消費者與容器整合測試 —— 以 `ProcessedEvents` 主鍵去重、實作 DLQ，並以 `xUnit + Testcontainers` 通過斷線與雙 Worker 壓測

---

## 📝 階段十六：分散式非同步狀態機與跨系統對帳 (Async State Machine & Reconciliation)

### 學習里程碑
- [ ] 任務 16.1：伺服器端權威狀態機 —— 明確定義合法狀態轉換矩陣，完全拒絕 Client 偽造或跳躍狀態
- [ ] 任務 16.2：Unknown 中間態與主動收斂 —— 引入 `PaymentConfirming` / `SyncPending` 中間態，實作 `IdempotencyKey`、背景主動查單 API 與 Webhook 冪等收斂
- [ ] 任務 16.3：逾時關單與回調撞車防禦 —— 以 CAS 原子更新確保單邊勝出，落敗回調自動觸發原路退款／補償（含防空回滾與防懸掛）
- [ ] 任務 16.4：批次對帳任務（Reconciliation Job）—— 比對本地單據與外部帳單差異並自動產出異常補償清單，全程以自動化測試驗證

---

## 📝 階段十七：大型遺留系統現代化與適度設計 (Legacy Modernization)

### 學習里程碑
- [ ] 任務 17.1：遺留系統切縫與防腐層 —— 運用 Seam、Anti-Corruption Layer（ACL）隔離舊式大型資料模型，並以 Strangler Fig 漸進抽離高耦合單體分支
- [ ] 任務 17.2：影子雙跑驗證（Parallel Run）—— 在不中斷現有服務的前提下，同步比對新舊邏輯輸出差異
- [ ] 任務 17.3：介面粒度與適度設計校準 —— 收斂過度細碎的單方法 Repository 介面為語意內聚的 Query / Write Port，平衡 ISP 與維護成本
- [ ] 任務 17.4：伺服器端集中授權守門 —— 將前端按鈕隱藏與分散檢查收斂為獨立可測的 `ActionPolicy` 層，並系統化消滅動態 SQL 字串拼接

---

## 📝 階段十八：系統韌性、快取一致性與負載壓測 (Resilience, Caching & Load Testing)

### 學習里程碑
- [ ] 任務 18.1：外部依賴防雪崩 —— 導入 `.NET 8 Microsoft.Extensions.Http.Resilience` / Polly，配置 Timeout 預算、指數退避重試（區分冪等與非冪等安全邊界）、斷路器（Circuit Breaker）與艙壁隔離（Bulkhead）
- [ ] 任務 18.2：Redis 快取與三大災難防禦 —— 實作 Cache-Aside，並親手防禦快取穿透（空值快取／布隆過濾器）、快取擊穿（分散式鎖／Singleflight 防止熱點過期回源打爆 DB）與快取雪崩（TTL 隨機打散）
- [ ] 任務 18.3：資料庫與快取雙寫一致性 —— 比較並實測先更新 DB 再刪快取、延遲雙刪與搭配 Outbox 事件失效快取的適用邊界
- [ ] 任務 18.4：限流保護與 k6 實戰壓測 —— 接入 ASP.NET Core Rate Limiting（Token Bucket / Concurrency Limiter），以 k6 注入外部 API 10 秒高延遲與 200 RPS 熱點流量，驗證 P95/P99 穩定、DB 無過載且斷路器自動熔斷與恢復
