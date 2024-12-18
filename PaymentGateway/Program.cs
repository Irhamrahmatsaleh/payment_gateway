using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.Data;

namespace PaymentGateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Inisialisasi database dalam scope terpisah
            await InitializeDatabaseAsync(host);

            // Menjalankan host
            await host.RunAsync();
        }

        /// <summary>
        /// Membuat IHostBuilder untuk konfigurasi aplikasi.
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        /// <summary>
        /// Metode untuk inisialisasi database dengan penanganan kesalahan yang jelas.
        /// </summary>
        private static async Task InitializeDatabaseAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<PaymentGatewayContext>();

                // Memastikan context diinisialisasi sebelum digunakan
                if (context.Database.CanConnect())
                {
                    await DbInitializer.InitializeAsync(context);
                }
                else
                {
                    throw new Exception("Database connection failed. Ensure the database is available.");
                }
            }
            catch (Exception ex)
            {
                // Mendapatkan logger dari layanan
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database.");

                // Pilihan: Anda dapat memutuskan apakah akan menghentikan aplikasi di sini
                throw; // Jika ingin menghentikan aplikasi, atau hapus baris ini untuk melanjutkan.
            }
        }
    }
}
