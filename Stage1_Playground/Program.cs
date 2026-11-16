using System;

namespace Stage1_Playground
{
    // =====================================================================================
    // 任務 1.1：認識「物件」—— 為什麼不把資料和邏輯分開？
    //
    // 這是一段完全不用物件導向、純「程序導向（Procedural）」的飲料店結帳代碼。
    // 請仔細閱讀這段代碼，思考它有什麼致命缺陷。
    // =====================================================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== 歡迎光臨 傳統程序式飲料店 ===");

            // 客人點了一杯奶茶
            Drink drink = new Drink("黑糖珍珠鮮奶", 70, 5);
            Customer customer = new Customer("小明", 100);
            int orderQuantity = 2;   // 客人要買 2 杯

            if (customer.HasEnoughMoney(drink.CalculateTotalPrice(orderQuantity)) && drink.HasEnoughStock(orderQuantity))
            {
                customer.Pay(drink.CalculateTotalPrice(orderQuantity));
                drink.DeductStock(orderQuantity);
                Console.WriteLine($"結帳成功！{customer.Name} 購買了 {orderQuantity} 杯 {drink.Name}，總共花費 {drink.CalculateTotalPrice(orderQuantity)} 元。");
            }
            else
            {
                Console.WriteLine($"結帳失敗！{customer.Name} 沒有足夠的錢或 {drink.Name} 庫存不足。");
            }

            
        }
        public class Drink
        {
            // 1. 私有欄位：存放名稱、單價、庫存（不讓外面隨意竄改）
            public string Name { get; private set; }
            private int price;
            private int stock;
            // 2. 建構子（Constructor）：建立這杯飲料時，設定初始的名稱、價格、庫存
            public Drink(string _name, int _price, int _initialStock)
            {
                Name = _name;
                price = _price;
                stock = _initialStock;
            }
            // 3. 守門員方法：如果要扣庫存，要提供什麼方法讓外面呼叫？
            //    在這個方法裡面，如何確保庫存不會被扣到小於 0？
            public bool DeductStock(int quantity)
            {
                // 這裡該怎麼寫？
                if (quantity <= stock)
                {
                    stock -= quantity;
                    return true; // 扣庫存成功
                }
                else
                {
                    return false; // 庫存不足，扣庫存失敗
                }
            }
            public int CalculateTotalPrice(int quantity)
            {
                return price * quantity;
            }
            public bool HasEnoughStock(int quantity)
            {
                return stock >= quantity;
            }
        }
        public class Customer
        {
            public string Name { get; private set; }
            private int wallet;

            public Customer(string _name, int _wallet)
            {
                Name = _name;
                wallet = _wallet;
            }

            public bool Pay(int amount)
            {
                if (HasEnoughMoney(amount))
                {
                    wallet -= amount;
                    return true; // 扣款成功
                }
                else
                {
                    return false; // 錢不夠，扣款失敗
                }
            }
            public bool HasEnoughMoney(int amount)
            {
                return wallet >= amount;
            }
        }
    }
}
