using Microsoft.EntityFrameworkCore;

namespace LibroAPI.Models
{
    public class LibroContext : DbContext
    {
        public LibroContext(DbContextOptions<LibroContext> options) : base(options)
        {
        }
        
        public DbSet<Libro> Libros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Siembra de datos para la tabla Products

            modelBuilder.Entity<Libro>().HasData(
                new Libro
                {
                    Id = 1,
                    Titulo = "Cien años de soledad",
                    Autor = "Gabriel García Márquez",
                    AnioPublicacion = 1967
                },
                new Libro
                {
                    Id = 2,
                    Titulo = "Don Quijote de la Mancha",
                    Autor = "Miguel de Cervantes",
                    AnioPublicacion = 1605
                },
                new Libro
                {
                    Id = 3,
                    Titulo = "El amor en los tiempos del cólera",
                    Autor = "Gabriel García Márquez",
                    AnioPublicacion = 1985
                }
            );
        }
    }
}
