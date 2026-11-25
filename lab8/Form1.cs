using System.Collections.Concurrent;

namespace lab8
{
    public partial class Form1 : Form
    {
        private readonly Button btnStart = new Button();
        private readonly ListBox listBox = new ListBox();

        // Колекція машин.
        private readonly List<Car> cars = new List<Car>
        {
            new Car { Brand = "BMW",    Model = "X5",      Year = 2018, Plate = "AA1111BB" },
            new Car { Brand = "Audi",   Model = "A6",      Year = 2015, Plate = "BC2222CD" },
            new Car { Brand = "BMW",    Model = "320i",    Year = 2020, Plate = "KA3333KA" },
            new Car { Brand = "Toyota", Model = "Camry",   Year = 2012, Plate = "AX4444XA" },
            new Car { Brand = "BMW",    Model = "M5",      Year = 2022, Plate = "AO5555OA" },
            new Car { Brand = "Skoda",  Model = "Octavia", Year = 2019, Plate = "BH6666HB" }
        };

        public Form1()
        {
            InitializeComponent();
            SetupInterface();
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

            // PLINQ-запит: тільки машини марки BMW.
            listBox.Items.Add("PLINQ: машини марки BMW");
            var bmw = cars.AsParallel().Where(c => c.Brand == "BMW").ToList();
            foreach (Car c in bmw)
            {
                listBox.Items.Add("  " + c);
            }

            // Виробник / споживач у фоні, щоб форма не "зависала".
            listBox.Items.Add("Виробник / споживач:");
            await Task.Run(() => ProducerConsumer());

            btnStart.Enabled = true;
        }

        // Один виробник і один споживач діляться ConcurrentBag.
        private void ProducerConsumer()
        {
            ConcurrentBag<Car> bag = new ConcurrentBag<Car>();

            // Виробник сигналізує споживачу, що в колекції з'явився елемент.
            AutoResetEvent dataReady = new AutoResetEvent(false);

            Thread producer = new Thread(() =>
            {
                foreach (Car car in cars)
                {
                    bag.Add(car);
                    Log("Виробник додав: " + car);
                    dataReady.Set(); // повідомляємо споживача
                    Thread.Sleep(300);
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
