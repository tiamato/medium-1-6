using System;
using System.Collections.Generic;

namespace Discount
{
    internal static class Program
    {
        private static void Main()
        {
            // �������� �����
            var item = new Item("�����1", 1.25);

            // �� ������� 4 �������, �� ��� 3 � ������� ��������
            var order = new Order();
            ISellablePosition basePosition = new BasePosition(item);
            order.AddPosition(basePosition);
            order.AddPosition(new DiscountedPosition(basePosition, 10));
            order.AddPosition(new DiscountedPosition(basePosition, 20));
            order.AddPosition(new DiscountedPosition(basePosition, 30));
            order.Print();

            // ������ ���� ������
            item.Price = 10.35f;
            order.Print();

            // ������ ������ �������� � ���� ��� ��������
            order.Delivery = new Pickup(5);
            order.Print();

            //����������� �������� ��� �������� ��� ������ � �������� �������� ������ - ������
            try
            {
                order.Delivery = new Courier(25);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("---");
            }

            //������� ������� �� �������. ��������� ��������
            order.RemoveDiscounted();
            order.Delivery = new Courier(25);
            order.Print();

            //�������� �������� ����� �� ������� - ������
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
            return $"������������: {_name}, ����: {Price:0.00}";
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
                throw new ArgumentOutOfRangeException(nameof(discount), "�������� �� ����� ���� ������ 0");

            if (discount > 100)
                throw new ArgumentOutOfRangeException(nameof(discount), "�������� �� ����� ���� ������ 100");

            _discount = discount;
        }

        public Item Item => _position.Item;
        public double TotalPrice => Item.Price - GetDiscount();

        public override string ToString()
        {
            return Item +
                   $", ������: {GetDiscount():0.00} ({_discount}%)" +
                   $", �����: {TotalPrice:0.00}";
        }

        private double GetDiscount()
        {
            return Item.Price * _discount / 100;
        }
    }

    public abstract class Delivery : Item
    {
        protected Delivery(string name, double price) : base("�������� - " + name, price) { }
    }

    public class Pickup : Delivery
    {
        private const double DefaultPrice = 10;

        public Pickup() : base("���������", DefaultPrice) { }

        public Pickup(double price) : base("���������", price) { }

    }

    public class Courier : Delivery
    {
        private const double DefaultPrice = 50;

        public Courier() : base("���������� ��������", DefaultPrice) { }

        public Courier(double price) : base("���������� ��������", price) { }
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
                if (value == null) throw new ArgumentNullException(nameof(value), "������ �������� �� ����� ���� ������");
                if (value is Courier && ExistsDiscounted())
                {
                    throw new ArgumentException("�������� �������� ����������, ��� ��� � ������ ���� ������ �� �������. ������� ������ �� ������� �� �������, ����� �������� ��� ��������", nameof(Delivery));
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
                throw new ArgumentException("������� �� ������� �� ����� ���� ��������� � ����� � ����� �������� ��������. �������� ��� �������� �� ���������", nameof(position));
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
            Console.WriteLine($"�����: {TotalPrice:0.00}");
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
