using System.ComponentModel;
using System.Windows.Forms;

namespace BestOilApp
{
    public partial class MainForm : Form
    {
        private decimal totalIncome;
        private Product[] gasolines;
        private Product[] cafeProducts;
        private Cart cart;

        public MainForm()
        {
            InitializeComponent();

            gasolines = new Product[]
            {
                new Product { Name = "A-92", Price = 46 },
                new Product { Name = "A-95", Price = 48 },
                new Product { Name = "A-100", Price = 52 }
            };

            cafeProducts = new Product[]
            {
                new Product { Name = "Кофе", Price = 25 },
                new Product { Name = "Cнэки", Price = 40 },
                new Product { Name = "Вода", Price = 30 },
                new Product { Name = "Шоколад", Price = 14 }
            };

            cart = new Cart();
        }

        private void FillMiniCafe()
        {
            int top = 60, left = 20;

            foreach (var product in cafeProducts)
            {
                CheckBox checkBox = new CheckBox
                {
                    Name = $"CheckBox-{product.Id}",
                    Text = product.Name,
                    Location = new System.Drawing.Point(left, top)
                };

                checkBox.CheckedChanged += CheckBox_CheckedChanged;

                TextBox textBox = new TextBox
                {
                    Text = product.Price.ToString(),
                    Enabled = false,
                    Width = 65,
                    Location = new System.Drawing.Point(left + checkBox.Width, top)
                };

                NumericUpDown numericUpDown = new NumericUpDown
                {
                    Name = $"Numeric-{product.Id}",
                    Minimum = 0,
                    Enabled = false,
                    Width = 65,
                    Location = new System.Drawing.Point(left + checkBox.Width + textBox.Width, top)
                };

                numericUpDown.ValueChanged += NumericUpDown_ValueChanged;

                MiniCafeGroupBox.Controls.Add(checkBox);
                MiniCafeGroupBox.Controls.Add(textBox);
                MiniCafeGroupBox.Controls.Add(numericUpDown);

                top += checkBox.Height + 5;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            (FormBorderStyle, MaximizeBox) = (FormBorderStyle.Fixed3D, false);
            GasolineComboBox.Items.AddRange(gasolines);
            GasolineComboBox.SelectedIndex = 0;
            FillMiniCafe();

            this.FormClosing += Form1_FormClosing;
        }

        private void NumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = sender as NumericUpDown;
            string id = numericUpDown.Name.Substring(numericUpDown.Name.IndexOf('-') + 1);
            cart.ChangeAmount(id, (int)numericUpDown.Value);
            MiniCafeTotalLabel.Text = cart.TotalMiniCafe.ToString();
            TotalToPayLabel.Text = cart.TotalToPay.ToString();
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string id = checkBox.Name.Substring(checkBox.Name.IndexOf('-') + 1);
            var numeric = MiniCafeGroupBox.Controls.OfType<NumericUpDown>().FirstOrDefault(e => e.Name.Equals($"Numeric-{id}"));
            if (checkBox.Checked)
            {
                numeric.Enabled = true;
                numeric.Value = 1;
                cart.AddProduct(cafeProducts.FirstOrDefault(e => e.Id.Equals(id)));
            }
            else
            {
                numeric.Enabled = false;
                numeric.Value = 0;
                cart.RemoveProduct(cafeProducts.FirstOrDefault(e => e.Id.Equals(id)));
            }
            MiniCafeTotalLabel.Text = cart.TotalMiniCafe.ToString();
            TotalToPayLabel.Text = cart.TotalToPay.ToString();
        }

        private void GasolineComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PriceTextBox.Text = gasolines[GasolineComboBox.SelectedIndex].Price.ToString();
        }

        private void CountOrPriceRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton.Name.Equals("CountRadioButton"))
            {
                GasolinePayGroupBox.Text = "Количество:";
                GasolinePayGroupBoxLabel.Text = "л.";
                gasStationTotalLabel.Text = (CountNumeric.Value * gasolines[GasolineComboBox.SelectedIndex].Price).ToString();
                CountNumeric.Enabled = true;
                AmountNumeric.Enabled = false;
                cart.TotalGasStation = CountNumeric.Value * gasolines[GasolineComboBox.SelectedIndex].Price;
            }
            else
            {
                GasolinePayGroupBox.Text = "К выдаче:";
                GasolinePayGroupBoxLabel.Text = "грн.";
                gasStationTotalLabel.Text = (AmountNumeric.Value / gasolines[GasolineComboBox.SelectedIndex].Price).ToString();
                CountNumeric.Enabled = false;
                AmountNumeric.Enabled = true;
                cart.TotalGasStation = AmountNumeric.Value;
            }
            TotalToPayLabel.Text = cart.TotalToPay.ToString();
        }

        private void CountNumeric_ValueChanged(object sender, EventArgs e)
        {
            gasStationTotalLabel.Text = (CountNumeric.Value * gasolines[GasolineComboBox.SelectedIndex].Price).ToString();
            cart.TotalGasStation = CountNumeric.Value * gasolines[GasolineComboBox.SelectedIndex].Price;
            TotalToPayLabel.Text = cart.TotalToPay.ToString();
        }

        private void AmountNumeric_ValueChanged(object sender, EventArgs e)
        {
            gasStationTotalLabel.Text = (Math.Round(AmountNumeric.Value / gasolines[GasolineComboBox.SelectedIndex].Price, 2)).ToString();
            cart.TotalGasStation = AmountNumeric.Value;
            TotalToPayLabel.Text = cart.TotalToPay.ToString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show($"Общая сумма выручки: {totalIncome}");
        }

        private void PayButton_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                totalIncome += cart.TotalToPay;
                await Task.Delay(1000);
                var dialogResult = MessageBox.Show("Оплатить все заказы клиентов?", "Подтверждение", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    await Task.Delay(10_000);
                }
                ClearForm();
            });
        }

        private void ClearForm()
        {
            this.Invoke((MethodInvoker)(() =>
            {
                GasolineComboBox.SelectedIndex = 0;
                CountRadioButton.Checked = true;
                AmountRadioButton.Checked = false;
                AmountNumeric.Value = 0;
                CountNumeric.Value = 0;
                gasStationTotalLabel.Text = "0";

                foreach (var item in MiniCafeGroupBox.Controls.OfType<NumericUpDown>())
                {
                    item.Value = 0;
                    item.Enabled = false;
                }
                foreach (var item in MiniCafeGroupBox.Controls.OfType<CheckBox>())
                {
                    item.Checked = false;
                }
            }));
            cart = new Cart();
        }
    }

    class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    class Cart
    {
        private List<Product> products;
        public Cart()
        {
            products = new List<Product>();
        }

        public decimal TotalGasStation { get; set; }

        public decimal TotalMiniCafe
        {
            get { return products.Sum(e => e.Price * e.Count); }
        }

        public decimal TotalToPay
        {
            get { return TotalGasStation + TotalMiniCafe; }
        }

        public void AddProduct(Product product)
        {
            if (products.Any(e => e.Id.Equals(product.Id)))
            {
                products.FirstOrDefault(e => e.Id.Equals(product.Id)).Count += 1;
            }
            else
            {
                product.Count = 1;
                products.Add(product);
            }
        }

        public void RemoveProduct(Product product)
        {
            products = products.Where(e => e.Id != product.Id).ToList();
        }

        public void ChangeAmount(string productId, int count)
        {
            if (products.Any(e => e.Id.Equals(productId)))
            {
                products.FirstOrDefault(e => e.Id.Equals(productId)).Count = count;
            }
        }
    }
}
