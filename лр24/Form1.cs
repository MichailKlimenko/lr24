using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Entity.Infrastructure;
using System.Configuration;

namespace лр24
{
    public partial class Form1 : Form
    {
        private readonly EntityCRUDService<Bicycle> _bicycleService;
        private readonly EntityCRUDService<Client> _clientService;
        private readonly EntityCRUDService<Rental> _rentalService;
        public Form1()
        {
            InitializeComponent();
            var context = new BikeRentalDbContext();
            _bicycleService = new EntityCRUDService<Bicycle>(context);
            _clientService = new EntityCRUDService<Client>(context);
            _rentalService = new EntityCRUDService<Rental>(context);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "bikeRentalDBDataSet.Rentals". При необходимости она может быть перемещена или удалена.
            this.rentalsTableAdapter.Fill(this.bikeRentalDBDataSet.Rentals);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "bikeRentalDBDataSet.Clients". При необходимости она может быть перемещена или удалена.
            this.clientsTableAdapter.Fill(this.bikeRentalDBDataSet.Clients);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "bikeRentalDBDataSet.Bicycles". При необходимости она может быть перемещена или удалена.
            this.bicyclesTableAdapter.Fill(this.bikeRentalDBDataSet.Bicycles);

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            string firstName = textBox4.Text;
            string lastName = textBox5.Text;
            string phoneNumber = textBox6.Text;

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phoneNumber))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var newClient = new Client
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber
            };

            await _clientService.AddEntityAsync(newClient);

            RefreshDataGridViewClients();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string model = textBox1.Text;
            string frameSize = textBox2.Text;
            decimal rentalCostPerHour;
            if (!decimal.TryParse(textBox3.Text, out rentalCostPerHour))
            {
                MessageBox.Show("Пожалуйста, введите корректную стоимость аренды в час.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string type = comboBox1.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(model) || string.IsNullOrEmpty(frameSize) || string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Создаем новый велосипед
            var newBicycle = new Bicycle
            {
                Model = model,
                Type = type,
                FrameSize = frameSize,
                RentalCostPerHour = rentalCostPerHour
            };

            // Добавляем в базу данных
            var bicycleService = new EntityCRUDService<Bicycle>(new BikeRentalDbContext());
            await _bicycleService.AddEntityAsync(newBicycle);

            // Обновляем DataGridView с велосипедами
            RefreshDataGridViewBicycles();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            int bicycleId;
            int clientId;
            DateTime startDateTime = dateTimePicker1.Value;
            DateTime endDateTime = dateTimePicker2.Value;

            // Проверка на корректность ввода ID велосипеда
            if (!int.TryParse(textBox7.Text, out bicycleId))
            {
                MessageBox.Show("Некорректный ID велосипеда", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка на корректность ввода ID клиента
            if (!int.TryParse(textBox8.Text, out clientId))
            {
                MessageBox.Show("Некорректный ID клиента", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка, чтобы дата окончания была позже даты начала
            if (endDateTime <= startDateTime)
            {
                MessageBox.Show("Дата окончания должна быть позже даты начала", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Получение велосипеда для расчета стоимости аренды
            var bicycle = await _bicycleService.GetEntityAsync(bicycleId);
            if (bicycle == null)
            {
                MessageBox.Show("Велосипед не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Расчет общей стоимости аренды
            var rentalDuration = endDateTime - startDateTime;
            var totalCost = (decimal)rentalDuration.TotalHours * bicycle.RentalCostPerHour;

            // Создание новой записи аренды
            var newRental = new Rental
            {
                BicycleID = bicycleId,
                ClientID = clientId,
                StartTime = startDateTime,
                EndTime = endDateTime,
                Cost = totalCost
            };

                await _rentalService.AddEntityAsync(newRental);

            // Обновление метки с общей стоимостью
            label12.Text = $"Cтоимость: {totalCost:C}";

            // Обновление DataGridView для отображения новых данных
            RefreshDataGridViewRentals();
        }

        private void RefreshDataGridViewBicycles()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = _bicycleService.GetAllEntities();
        }
        private void RefreshDataGridViewClients()
        {
            dataGridView2.DataSource = null;
            dataGridView2.DataSource = _clientService.GetAllEntities();
        }
        private void RefreshDataGridViewRentals()
        {
            dataGridView3.DataSource = null;
            dataGridView3.DataSource = _rentalService.GetAllEntities();
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            // Получаем выбранный велосипед из DataGridView
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedBicycleId = (int)dataGridView1.SelectedRows[0].Cells[0].Value;

                // Пытаемся удалить выбранный велосипед из базы данных
                bool deleted = await _bicycleService.DeleteEntityAsync(selectedBicycleId);

                if (deleted)
                {
                    // Если удаление прошло успешно, обновляем DataGridView
                    RefreshDataGridViewBicycles();
                    MessageBox.Show("Велосипед успешно удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Если удаление не удалось, выводим сообщение об ошибке
                    MessageBox.Show("Ошибка при удалении велосипеда", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Если ни один велосипед не выбран, выводим предупреждение
                MessageBox.Show("Выберите велосипед для удаления", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            // Проверяем, что в таблице выбрана хотя бы одна строка
            if (dataGridView2.SelectedRows.Count > 0)
            {
                // Получаем ID выбранного клиента
                int selectedClientId = (int)dataGridView2.SelectedRows[0].Cells[0].Value;

                // Пытаемся удалить выбранного клиента из базы данных
                bool deleted = await _clientService.DeleteEntityAsync(selectedClientId);

                if (deleted)
                {
                    // Если удаление прошло успешно, обновляем DataGridView
                    RefreshDataGridViewClients();
                    MessageBox.Show("Клиент успешно удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Если удаление не удалось, выводим сообщение об ошибке
                    MessageBox.Show("Ошибка при удалении клиента", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Если ни один клиент не выбран, выводим предупреждение
                MessageBox.Show("Выберите клиента для удаления", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            // Проверяем, что в таблице выбрана хотя бы одна строка
            if (dataGridView3.SelectedRows.Count > 0)
            {
                // Получаем ID выбранной записи о прокате
                int selectedRentalId = (int)dataGridView3.SelectedRows[0].Cells[0].Value;

                // Пытаемся удалить выбранную запись о прокате из базы данных
                bool deleted = await _rentalService.DeleteEntityAsync(selectedRentalId);

                if (deleted)
                {
                    // Если удаление прошло успешно, обновляем DataGridView
                    RefreshDataGridViewRentals();
                    MessageBox.Show("Запись о прокате успешно удалена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Если удаление не удалось, выводим сообщение об ошибке
                    MessageBox.Show("Ошибка при удалении записи о прокате", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Если ни одна запись о прокате не выбрана, выводим предупреждение
                MessageBox.Show("Выберите запись о прокате для удаления", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
