using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using System.Text.Json;
using WorkerService.Application.Configuration;
using WorkerService.Application.DTO;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceConfigurations _serviceConfigurations;

        public Worker(ILogger<Worker> logger,
            IConfiguration configuration)
        {
            _logger = logger;

            _serviceConfigurations = new ServiceConfigurations();
            new ConfigureFromConfigurationOptions<ServiceConfigurations>(
                configuration.GetSection("ServiceConfigurations"))
                    .Configure(_serviceConfigurations);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await Task.Delay(1000, stoppingToken);

                foreach (string host in _serviceConfigurations.Hosts)
                {
                    _logger.LogInformation($"Verificando a disponibilidade do host {host}");

                    var resultado = new ResultadoMonitoramento
                    {
                        Horario = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Host = host
                    };

                    // Verifica a disponibilidade efetuando um ping
                    // no host que foi configurado em appsettings.json
                    try
                    {
                        using Ping p = new();
                        var resposta = p.Send(host);
                        resultado.Status = resposta.Status.ToString();
                    }
                    catch (Exception ex)
                    {
                        resultado.Status = "Exception";
                        resultado.Exception = ex.InnerException?.Message ?? ex.Message;
                    }

                    string jsonResultado = JsonSerializer.Serialize(resultado);

                    if (resultado.Exception == null)
                        _logger.LogInformation(jsonResultado);
                    else
                        _logger.LogError(jsonResultado);
                }

                await Task.Delay(_serviceConfigurations.Intervalo, stoppingToken);
            }
        }
    }
}