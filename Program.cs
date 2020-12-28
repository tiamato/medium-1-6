using System;
using System.Collections.Generic;

namespace Discount
{
    internal static class Program
    {
        private static void Main()
        {
            // исходный товар
            var item = new Item("“овар1", 1.25);

            // на витрине 4 позиции, из них 3 с разными скидками
            var order = new Order();
            ISellablePosition basePosition = new BasePosition(item);
            order.AddPosition(basePosition);
            order.AddPosition(new DiscountedPosition(basePosition, 10));
            order.AddPosition(new DiscountedPosition(basePosition, 20));
            order.AddPosition(new DiscountedPosition(basePosition, 30));
            order.Print();

            // мен€ем цену товара
            item.Price = 10.35f;
            order.Print();

            // мен€ем способ доставки и цену дл€ базового
            order.Delivery = new Pickup(5);
            order.Print();

            //ѕрисваиваем неверный тип доставки дл€ заказа с товарами имеющими скидку - ошибка
            try
            {
                order.Delivery = new Courier(25);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("---");
            }

            //”дал€ем позиции со скидкой. ѕовтор€ем операцию
            order.RemoveDiscounted();
            order.Delivery = new Courier(25);
            order.Print();

            //ѕытаемс€ добавить товар со скидкой - ошибка
            try
            {
                order.AddPosition(new DiscountedPosition(basePosition, 30));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("---");
            }
            order.Print();


            Console.Read();
        }
    }

    public class Item
    {
        private readonly string _name;
        public double Price;

        public Item(string name, double price)
        {
            _name = name;
            Price = price;
        }

        public override string ToString()
        {
            return $"Ќаименование: {_name}, цена: {Price:0.00}";
        }
    }

    public interface ISellablePosition
    {
        Item Item { get; }
        double TotalPrice { get; }
    }

    public class BasePosition : ISellablePosition
    {
        public BasePosition(Item item)
        {
            Item = item;
        }

        public Item Item { get; }
        public double TotalPrice => Item.Price;

        public override string ToString()
        {
            return Item.ToString();
        }
    }

    public class DiscountedPosition : ISellablePosition
    {
        private readonly ISellablePosition _position;
        private readonly int _discount;

        public DiscountedPosition(ISellablePosition position, int discount)
        {
            _position = position;

            if (discount < 0)
                throw new ArgumentOutOfRangeException(nameof(discount), "«начение не может быть меньше 0");

            if (discount > 100)
                throw new ArgumentOutOfRangeException(nameof(discount), "«начение не может быть больше 100");

            _discount = discount;
        }

        public Item Item => _position.Item;
        public double TotalPrice => Item.Price - GetDiscount();

        public override string ToString()
        {
            return Item +
                   $", скидка: {GetDiscount():0.00} ({_discount}%)" +
                   $", итого: {TotalPrice:0.00}";
        }

        private double GetDiscount()
        {
            return Item.Price * _discount / 100;
        }
    }

    public abstract class Delivery : Item
    {
        protected Delivery(string name, double price) : base("ƒоставка - " + name, price) { }
    }

    public class Pickup : Delivery
    {
        private const double DefaultPrice = 10;

        public Pickup() : base("самовывоз", DefaultPrice) { }

        public Pickup(double price) : base("самовывоз", price) { }

    }

    public class Courier : Delivery
    {
        private const double DefaultPrice = 50;

        public Courier() : base("курьерска€ доставка", DefaultPrice) { }

        public Courier(double price) : base("курьерска€ доставка", price) { }
    }

    public class Order
    {
        private readonly List<ISellablePosition> _positionList;
        private Delivery _delivery;

        public Delivery Delivery
        {
            get => _delivery;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value), "—пособ доставки не может быть пустым");
                if (value is Courier && ExistsDiscounted())
                {
                    throw new ArgumentException("ƒоставка курьером невозможна, так как в заказе есть товары со скидкой. ”далите товары со скидкой из корзины, чтобы изменить тип доставки", nameof(Delivery));
                }

                _delivery = value;
            }
        }

        private double TotalPrice => GetTotalPrice();

        public Order()
        {
            Delivery = new Pickup();
            _positionList = new List<ISellablePosition>();
        }

        public void AddPosition(ISellablePosition position)
        {
            if (position is DiscountedPosition && Delivery is Courier)
            {
                throw new ArgumentException("ѕозици€ со скидкой не может быть добавлена в заказ с типом доставки курьером. »змените тип доставки на самовывоз", nameof(position));
            }

            _positionList.Add(position);
        }

        public void RemovePosition(ISellablePosition position)
        {
            _positionList?.Remove(position);
        }
        
        public void RemoveDiscounted()
        {
            if (_positionList == null) return;

            var toRemove = new List<ISellablePosition>();

            foreach (var position in _positionList)
                if (position is DiscountedPosition) toRemove.Add(position);

            foreach (var position in toRemove)
                _positionList.Remove(position);
        }

        public void Print()
        {
            if (_positionList != null)
                foreach (var position in _positionList)
                    Console.WriteLine(position);

            Console.WriteLine(Delivery);
            Console.WriteLine($"¬сего: {TotalPrice:0.00}");
            Console.WriteLine("---");
        }

        private double GetTotalPrice()
        {
            var result = Delivery.Price;
            foreach (var position in _positionList) result += position.TotalPrice;

            return result;
        }

        private bool ExistsDiscounted()
        {
            if (_positionList != null)
                foreach (var position in _positionList)
                    if (position is DiscountedPosition)
                        return true;

            return false;
        }
    }
}
