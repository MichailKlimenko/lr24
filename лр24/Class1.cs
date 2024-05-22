using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Windows.Forms;

namespace лр24
{
    public class Bicycle
    {
        [Key]
        public int ID { get; set; }

        public string Model { get; set; }

        public string Type { get; set; }

        public string FrameSize { get; set; }

        public decimal RentalCostPerHour { get; set; }

        public string Photo { get; set; }
    }

    public class Client
    {
        [Key]
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
    }

    public class Rental
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Bicycle")]
        public int BicycleID { get; set; }

        [ForeignKey("Client")]
        public int ClientID { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal Cost { get; set; }

        public virtual Bicycle Bicycle { get; set; }

        public virtual Client Client { get; set; }
    }
    public class EntityCRUDService<T> where T : class
    {
        private readonly DbContext _context;

        public EntityCRUDService(DbContext context)
        {
            _context = context;
        }

        // Метод для добавления сущности в базу данных
        public async Task<T> AddEntityAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Метод для удаления сущности из базы данных
        public async Task<bool> DeleteEntityAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
                return false;

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // Метод для обновления сущности в базе данных
        public async Task<bool> UpdateEntityAsync(int id, T updatedEntity)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
                return false;

            _context.Entry(entity).CurrentValues.SetValues(updatedEntity);
            await _context.SaveChangesAsync();
            return true;
        }

        // Метод для получения всех сущностей из базы данных
        public List<T> GetAllEntities()
        {
            try
            {
                return _context.Set<T>().ToList();
            }
            catch (Exception ex)
            {
                // Выводим сообщение об ошибке с подробностями из внутреннего исключения
                MessageBox.Show($"Произошла ошибка при выполнении запроса к базе данных. Подробности: {ex.InnerException?.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null; // или другое подходящее действие в случае ошибки
            }
        }

        // Метод для получения сущности по ID
        public async Task<T> GetEntityAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
    }

    public class BikeRentalDbContext : DbContext
    {
        public DbSet<Bicycle> Bicycles { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        public BikeRentalDbContext() : base("name=BikeRentalDBConnectionString")
        {
        }
    }
}
