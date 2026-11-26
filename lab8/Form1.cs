using System.Collections.Concurrent;

namespace lab8
{
    public partial class Form1 : Form
    {
        private readonly Button btnStart = new Button();
        private readonly ListBox listBox = new ListBox();

        // Колекція машин, заповнюється випадково.
        private readonly List<Car> cars = GenerateCars(30);

        public Form1()
        {
            InitializeComponent();
            SetupInterface();
        }

        // Створює count машин з випадковою маркою, роком і номером.
        private static List<Car> GenerateCars(int count)
        {
            string[] brands = { "BMW", "Audi", "Toyota", "Skoda" };
            string[] models = { "X5", "A6", "Camry", "Octavia" }; // модель для кожної марки
            string letters = "ABCEHIKMOPTX";
            Random random = new Random();
            List<Car> result = new List<Car>();

            for (int i = 0; i < count; i++)
            {
                int b = random.Next(brands.Length);
                result.Add(new Car
                {
                    Brand = brands[b],
                    Model = models[b],
                    Year = random.Next(2000, 2026),
                    Plate = $"{letters[random.Next(letters.Length)]}{letters[random.Next(letters.Length)]}"
                          + $"{random.Next(1000, 10000)}"
                          + $"{letters[random.Next(letters.Length)]}{letters[random.Next(letters.Length)]}"
                });
            }

            return result;
        }

        private void SetupInterface()
        {
            Text = "Лабораторна №8 — Варіант 3";
            ClientSize = new Size(600, 450);
            StartPosition = FormStartPosition.CenterScreen;

            btnStart.Text = "Запустити";
            btnStart.SetBounds(20, 16, 120, 32);
            btnStart.Click += BtnStart_Click;

            listBox.SetBounds(20, 60, 560, 370);
            listBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            Controls.Add(btnStart);
            Controls.Add(listBox);
        }

        private async void BtnStart_Click(object? sender, EventArgs e)
        {
            btnStart.Enabled = false;
            listBox.Items.Clear();

            listBox.Items.Add("PLINQ: машини марки BMW");
            var bmw = cars.AsParallel().Where(c => c.Brand == "BMW").ToList();
            foreach (Car c in bmw)
            {
                listBox.Items.Add("  " + c);
            }

            listBox.Items.Add("Виробник / споживач:");
            await Task.Run(() => ProducerConsumer());

            btnStart.Enabled = true;
        }

        private void ProducerConsumer()
        {
            ConcurrentBag<Car> bag = new ConcurrentBag<Car>();

            AutoResetEvent dataReady = new AutoResetEvent(false);

            Thread producer = new Thread(() =>
            {
                foreach (Car car in cars)
                {
                    bag.Add(car);
                    Log("Виробник додав: " + car);
                    dataReady.Set(); // повідомляємо споживача
                    Thread.Sleep(100);
                }
            });

            Thread consumer = new Thread(() =>
            {
                int taken = 0;
                while (taken < cars.Count)
                {
                    dataReady.WaitOne(); // чекаємо сигнал від виробника
                    while (bag.TryTake(out Car? car))
                    {
                        Log("Споживач забрав: " + car);
                        taken++;
                    }
                }
            });

            producer.Start();
            consumer.Start();
            producer.Join();
            consumer.Join();
            dataReady.Dispose();
        }

        // Додає рядок у список з потоку виробника або споживача.
        private void Log(string text)
        {
            listBox.Invoke(() => listBox.Items.Add("  " + text));
        }

        private class Car
        {
            public string Brand { get; set; } = "";
            public string Model { get; set; } = "";
            public int Year { get; set; }
            public string Plate { get; set; } = "";

            public override string ToString() => $"{Brand} {Model}, {Year}, {Plate}";
        }
    }
}
