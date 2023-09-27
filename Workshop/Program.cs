using System;
using System.Collections.Generic;
using System.Linq;

namespace Workshop
{
    internal class Program
    {
        const ConsoleKey ShowQueueCommand = ConsoleKey.Q;
        const ConsoleKey ServeCarCommand = ConsoleKey.W;
        const ConsoleKey ExitCommand = ConsoleKey.Escape;

        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            Workshop workshop = new Workshop();

            bool isWorking = true;

            while (isWorking)
            {
                ShowCommands();
                workshop.ShowBalance();
                Console.WriteLine("\nСклад:");
                workshop.ShowStock();

                ConsoleKeyInfo userInput = Console.ReadKey(true);

                switch (userInput.Key)
                {
                    case ShowQueueCommand:
                        workshop.ShowQueue();
                        break;

                    case ServeCarCommand:
                        workshop.ServeCar();
                        break;

                    case ExitCommand:
                        isWorking = false;
                        break;
                }

                Console.ReadKey(true);
                Console.Clear();
            }
        }

        static void ShowCommands()
        {
            Console.WriteLine($"{ShowQueueCommand} - показать очередь машин");
            Console.WriteLine($"{ServeCarCommand} - обслужить машину в очереди");
            Console.WriteLine($"{ExitCommand} - закрыть программу");
        }
    }

    static class UserUtils
    {
        private static Random _random = new Random();

        public static int GetRandomNumber(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue + 1);
        }

        public static int GetNumberInRange(int minValue, int maxValue)
        {
            int number = 0;
            bool isCorrect = false;

            while (isCorrect == false)
            {
                Console.Write("Введите число - ");
                isCorrect = int.TryParse(Console.ReadLine(), out number);

                if (isCorrect == true)
                {
                    if (number <= minValue || number > maxValue)
                    {
                        isCorrect = false;
                    }
                }
            }

            return number;
        }
    }

    class Workshop
    {
        private readonly List<Shelf> _shelves;
        private Queue<Car> _carQueue;
        private Builder _builder = new Builder();
        private int _balance;
        private int _workFee = 50;

        public Workshop()
        {
            CreateBalance();
            _shelves = new List<Shelf>(_builder.GetStock());
            _carQueue = new Queue<Car>(_builder.CreateCarQueue());
        }

        public void ShowStock()
        {
            foreach (Shelf shelf in _shelves)
            {
                shelf.ShowInfo();
            }
        }

        public void ShowQueue()
        {
            Console.WriteLine($"\nМашин в очереди: {_carQueue.Count}");
            int number = 1;

            foreach (Car car in _carQueue)
            {
                Console.WriteLine($"Состояние {number}-го автомобиля:");
                ShowDetailsState(car.GetDetails());
                number++;
            }
        }

        public void ServeCar()
        {
            if (_carQueue.Count > 0)
                FixCar(_carQueue.Dequeue());
            else
                Console.WriteLine("Очередь пуста!");
        }

        public void ShowBalance()
        {
            Console.WriteLine($"Баланс автосервиса: {_balance}");
        }

        private bool TryGetShelfWithDetail(Detail detail, out Shelf shelfWithDetail)
        {
            foreach (Shelf shelf in _shelves)
            {
                if (shelf.GetDetailName() == detail.Name)
                {
                    shelfWithDetail = shelf;
                    return shelf.Amount > 0;
                }
            }

            shelfWithDetail = null;
            return false;
        }

        private int GetBrokenDetailsCount(List<Detail> details)
        {
            return GetBrokenDetails(details).Count;
        }

        private List<Detail> GetBrokenDetails(List<Detail> details)
        {
            List<Detail> brokenDetails = new List<Detail>();

            foreach (Detail detail in details)
            {
                if (detail.IsBroken == true)
                    brokenDetails.Add(detail);

            }

            return brokenDetails;
        }

        private void ShowDetailsState(List<Detail> details)
        {
            if (GetBrokenDetailsCount(details) == 0)
            {
                Console.WriteLine("У автомобиля ничего не сломано, человек явно зря приехал...");
            }
            else
            {
                for (int i = 0; i < details.Count; i++)
                {
                    if (details[i].IsBroken == true)
                    {
                        Console.WriteLine($"У этого автомобиля сломано {details[i].Name}");
                    }
                }
            }

            Console.WriteLine("---------------------------------------------------");
        }

        private void FixCar(Car car)
        {
            Console.WriteLine();
            List<Detail> carDetails = car.GetDetails();
            List<Detail> brokenDetails = GetBrokenDetails(carDetails);

            if (brokenDetails.Count == 0)
            {
                Console.WriteLine($"У машины ничего не сломано. Человек заплатил {_workFee} за осмотр.");
                _balance += _workFee;
                return;
            }

            ShowDetailsState(carDetails);

            foreach (Detail brokenDetail in brokenDetails)
            {
                if (TryGetShelfWithDetail(brokenDetail, out Shelf shelf))
                {
                    Detail detail = shelf.GiveDetail();
                    Console.WriteLine($"Деталь {detail.Name} заменена. Вы получили {detail.Price} денег за деталь и {_workFee} за работу.");
                    car.SwapDetail(brokenDetail, detail);
                    _balance += detail.Price + _workFee;
                }
                else
                {
                    int fine = shelf.GetDetailPrice() / 2;
                    Console.WriteLine($"К сожалению, на складе нет {shelf.GetDetailName()}");
                    Console.WriteLine($"Мы заплатили неустойку {fine}");
                    _balance -= fine;
                }
            }
        }

        private void CreateBalance()
        {
            int minBalance = 200;
            int maxBalance = 500;

            _balance = UserUtils.GetRandomNumber(minBalance, maxBalance);
        }
    }

    class Car
    {
        private List<Detail> _details;

        public Car(List<Detail> details)
        {
            _details = details;
        }

        public List<Detail> GetDetails()
        {
            return new List<Detail>(_details);
        }

        public void SwapDetail(Detail oldDetail, Detail newDetail)
        {
            if (_details.Contains(oldDetail))
            {
                _details.Remove(oldDetail);
                _details.Add(newDetail);
            }
        }
    }

    class Shelf
    {
        private Detail _detail;

        public Shelf(Detail detail, int amount)
        {
            _detail = detail;
            Amount = amount;
        }

        public int Amount { get; protected set; }

        public void ShowInfo()
        {
            Console.WriteLine($"Деталь {_detail.Name}, цена {_detail.Price}. Количество - {Amount}");
        }

        public Detail GiveDetail()
        {
            Amount--;
            return new Detail(_detail);
        }

        public int GetDetailPrice()
        {
            return _detail.Price;
        }

        public string GetDetailName()
        {
            return _detail.Name;
        }
    }

    class Builder
    {
        public List<Shelf> GetStock()
        {
            int minDetails = 1;
            int maxDetails = 3;
            List<Shelf> items = new List<Shelf>();
            List<Detail> details = GetDetails();

            for (int i = 0; i < details.Count; i++)
            {
                int amount = UserUtils.GetRandomNumber(minDetails, maxDetails);
                items.Add(new Shelf(details[i], amount));
            }

            return items;
        }

        public Queue<Car> CreateCarQueue()
        {
            Queue<Car> carQueue = new Queue<Car>();

            int minAmount = 5;
            int maxAmount = 5;
            int queueSize = UserUtils.GetRandomNumber(minAmount, maxAmount);

            for (int i = 0; i < queueSize; i++)
            {
                List<Detail> details = GetDetails();

                for (int j = 0; j < details.Count; j++)
                {
                    details[j].InitiateState();
                }

                carQueue.Enqueue(new Car(details));
            }

            return carQueue;
        }

        private List<Detail> GetDetails()
        {
            int enginePrice = 300;
            int wheelPrice = 150;
            int suspensionPrice = 400;

            List<Detail> details = new List<Detail>();

            details.Add(new Detail("Двигатель", enginePrice));
            details.Add(new Detail("Колесо", wheelPrice));
            details.Add(new Detail("Подвеска", suspensionPrice));

            return details;
        }
    }

    class Detail
    {
        public Detail(Detail detail)
        {
            Name = detail.Name;
            Price = detail.Price;
        }

        public Detail(string name, int price)
        {
            Name = name;
            Price = price;
        }

        public string Name { get; protected set; }
        public int Price { get; protected set; }
        public bool IsBroken { get; protected set; } = false;

        public void ShowInfo()
        {
            Console.WriteLine($"Деталь {Name}, цена - {Price}");
        }

        public void InitiateState()
        {
            IsBroken = UserUtils.GetRandomNumber(0, 1) == 0;
        }
    }
}