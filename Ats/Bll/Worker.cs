using Ats.Helper;
using Ats.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ats.Bll
{
    internal class Worker : BackgroundService
    {
        private DeviceList devices = new DeviceList();
        private readonly Logger _logger;
        private readonly Config _config;
        private DateTime lastShowStat = DateTime.MinValue;

        public Worker(Logger logger, IOptions<Config> config) 
        {
            _logger = logger;
            _config = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var assembly = this.GetType().Assembly;
            var fi = new FileInfo(assembly.Location);
            _logger.Information("Ats v{av} - build {lwt}", assembly.GetName().Version, fi.LastWriteTime);
            LoadDevices();
            var s = _config.ShowStatTimeoutMinutes > 1 ? "s" : string.Empty;
            _logger.Information(@$"Monitoring started
WorkerTimeout = {_config.WorkerTimeoutMs} ms
ThermalTarget = {_config.ThermalTargetCelcius}°C | Delta = {_config.ThermalTargetDeltaCelcius}°C
ShowStatTimeout = {_config.ShowStatTimeoutMinutes} minute{s}
");
            while (!stoppingToken.IsCancellationRequested)
            {
                Run();
                await Task.Delay(_config.WorkerTimeoutMs, stoppingToken);
            }
            _logger.Information("Monitoring stopped");
        }

        /// <summary>
        /// Scanning for hwmon devices
        /// </summary>
        private void LoadDevices()
        {
            _logger.Information("Scanning for HWMON devices");
            var directories = Directory.GetDirectories("/sys/class/hwmon");
            devices.Clear();
            foreach (var d in directories.OrderBy(x => x))
            {
                var di = new DirectoryInfo(d);
                var device = DeviceHelper.ParseDevice(d);
                devices.Add(device);
                _logger.Information("Adding device {di} : {dn}", device.Identifier, device.Name);
            }
        }

        private void Run()
        {
            Parallel.ForEach(devices, d =>
            {
                try
                {
                    DeviceHelper.LoadProperties(d);
                    //                        var data = System.Text.Json.JsonSerializer.Serialize(d);
                    //                        _tm.Trace($"{d.Name} : {data}");
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"LoadProperties error");
                }
            });

            var sensors = from item in devices
                          where item.Type == DeviceType.TemperatureSensor
                          select new
                          {
                              device = item,
                              name = item.Name,
                              temperature = (double.Parse(DeviceHelper.GetDevicePropertyValue(item, "temp1_input")) / 1000)
                          };

            var fans = from item in devices
                       where item.Type == DeviceType.FanController
                       select new
                       {
                           device = item,
                           name = item.Name,
                           speed = int.Parse(DeviceHelper.GetDevicePropertyValue(item, "pwm1"))
                       };

            var temperature = sensors.Max(x => x.temperature);
            var fanSpeedAverage = fans.Sum(x => x.speed) / fans.Count();
            if (fanSpeedAverage < 255 && temperature >= _config.ThermalTargetCelcius + _config.ThermalTargetDeltaCelcius)
            {
                Parallel.ForEach(fans, f =>
                {
                    try
                    {
                        if (f.speed < 255)
                        {
                            var newSpeed = f.speed + 5;
                            DeviceHelper.SetDevicePropertyValue(f.device, "pwm1", newSpeed);
                            _logger.Debug($"Temperature ({temperature}°C) is over the target {_config.ThermalTargetCelcius}°C. Increasing the fan {f.name} speed at {newSpeed * 100 / 255}%");
                        }
                        else
                        {
                            _logger.Verbose($"The fan {f.name} is already at max speed");
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "SetFanSpeed error");
                    }
                });
            }
            else if (fanSpeedAverage > 0 && temperature <= _config.ThermalTargetCelcius - _config.ThermalTargetDeltaCelcius)
            {
                Parallel.ForEach(fans, f =>
                {
                    try
                    {
                        if (f.speed > 0)
                        {
                            var newSpeed = f.speed - 5;
                            DeviceHelper.SetDevicePropertyValue(f.device, "pwm1", newSpeed);
                            _logger.Debug($"Temperature ({temperature}°C) is lower the target {_config.ThermalTargetCelcius}°C. Decreasing the fan {f.name} speed at {newSpeed * 100 / 255}%");
                        }
                        else
                        {
                            _logger.Verbose($"The fan {f.name} is already off");
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"SetFanSpeed error");
                    }
                });
            }

            if (lastShowStat < DateTime.Now.Subtract(new TimeSpan(0, _config.ShowStatTimeoutMinutes, 0)))
            {
                var sbSensors = new StringBuilder();
                foreach (var s in sensors)
                {
                    sbSensors.AppendLine($"Temperature sensor {s.name} is at {s.temperature}°C");
                }
                foreach (var f in fans)
                {
                    sbSensors.AppendLine($"Fan {f.name} is at {Convert.ToInt32(f.speed * 100 / 255)}% speed ({f.speed}/255)");
                }
                _logger.Information($"Sensors : \r\n{sbSensors}");
                lastShowStat = DateTime.Now;
            }
        }
    }
}
