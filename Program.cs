using System;
using System.Collections.Generic;

namespace Discount
{
    internal static class Program
    {
        private static void Main()
        {
            // исходный товар
            var item = new Item("Товар1", 1.25);

            // на витрине 4 позиции, из них 3 с разными скидками
            var showcase = new List<ISellablePosition>();
            ISellablePosition basePosition = new BasePosition(item);
            showcase.Add(basePosition);
            showcase.Add(new DiscountedPosition(basePosition, 10));
            showcase.Add(new DiscountedPosition(basePosition, 20));
            showcase.Add(new DiscountedPosition(basePosition, 30));
            PrintShowcase(showcase);

            // меняем цену товара
            item.Price = 10.35f;
            PrintShowcase(showcase);

            // меняем способ доставки и цену для базового
            basePosition.Delivery = new Pickup(5);
            PrintShowcase(showcase);

            basePosition.Delivery = new Courier(25);
            PrintShowcase(showcase);

            //Присваиваем неверный тип доставки для товара со скидкой - ошибка
            //item11.Delivery = new Courier(15);

            Console.Read();
        }

        private static void PrintShowcase(IEnumerable<ISellablePosition> showcase)
        {
            foreach (var position in showcase)
            {
                Console.WriteLine(position);
            }
            Console.WriteLine("---");
        }
    }

    public abstract class Delivery
    {
        public double Price { get; }

        protected Delivery(double price)
        {
            Price = price;
        }

        public override string ToString()
        {
            return $"{Price:0.00}";
        }
    }

    public class Pickup : Delivery
    {
        private const double DefaultPrice = 10;

        public Pickup(double price) : base(price) { }

        public Pickup() : base(DefaultPrice) { }

        public override string ToString()
        {
            return base.ToString() + " (самовывоз)";
        }
    }

    public class Courier : Delivery
    {
        private const double DefaultPrice = 50;

        public Courier(double price) : base(price) { }

        public Courier() : base(DefaultPrice) { }

        public override string ToString()
        {
            return base.ToString() + " (курьером)";
        }
    }

    public class Item
    {
        public readonly string Name;
        public double Price;

        public Item(string name, double price)
        {
            Name = name;
            Price = price;
        }
    }

    public interface ISellablePosition
    {
        Item Item { get; }
        Delivery Delivery { get; set; }
        double TotalPrice { get; }
    }

    public class BasePosition : ISellablePosition
    {
        public BasePosition(Item item)
        {
            Item = item;
            Delivery = new Courier();
        }

        public Item Item { get; }
        public Delivery Delivery { get; set; }
        public double TotalPrice => Item.Price + Delivery.Price;

        public override string ToString()
        {
            return $"Наименование: {Item.Name}" +
                   $", цена: {Item.Price:0.00}" +
                   $", доставка: {Delivery}" +
                   $", итого: {TotalPrice:0.00}";
        }
    }

    public class DiscountedPosition : ISellablePosition
    {
        private readonly ISellablePosition _position;
        private readonly int _discount;
        private Delivery _delivery;

        public Delivery Delivery
        {
            get => _delivery;
            set
            {
                if (!(value is Pickup))
                {
                    throw new ArgumentException("Недопустимый тип доставки");
                }
                _delivery = value;
            }
        }

        public Item Item => _position.Item;
        public double TotalPrice => Item.Price - GetDiscount() + Delivery.Price;

        public DiscountedPosition(ISellablePosition position, int discount)
        {
            _position = position;
            _discount = discount;
            _delivery = new Pickup();
        }

        public override string ToString()
        {
            return $"Наименование: {Item.Name}" +
                   $", цена: {Item.Price:0.00}" +
                   $", скидка: {GetDiscount():0.00} ({_discount}%)" +
                   $", доставка: {_delivery}" +
                   $", итого: {TotalPrice:0.00}";
        }

        private double GetDiscount()
        {
            return Item.Price * _discount / 100;
        }
    }
}
