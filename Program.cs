using BulkChanges.Models.Settings;
using BulkChanges.Operations;
using BulkChanges.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var _builder = Host.CreateApplicationBuilder(args);
var _configuration = _builder.Configuration;
var _services = _builder.Services;

_services.Configure<BulkChangesSettings>(config: _configuration.GetSection(BulkChangesSettings.SectionKey),
                                        configureBinder: (options) => {
                                            options.BindNonPublicProperties = true;
                                            options.ErrorOnUnknownConfiguration = true;
                                        });
_services.AddSingleton<IBulkChangesOperation, AddPainelUltimosRelatoriosGerados>();
_services.AddHostedService<BulkChangesService>();

var _host = _builder.Build();
_host.Run();
