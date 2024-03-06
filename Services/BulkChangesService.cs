using BulkChanges.Models.Settings;
using BulkChanges.Operations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ProgramSynthesis.Detection.Encoding;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BulkChanges.Services;

internal sealed class BulkChangesService(ILoggerFactory                loggerFactory,
                                         IHostApplicationLifetime      hostApplicationLifeTime,
                                         IBulkChangesOperation         bulkChangesOperation,
                                         IOptions<BulkChangesSettings> options
) : IBulkChangesService
{
    #region Dependências
    private readonly ILogger<BulkChangesService> _logger                  = loggerFactory.CreateLogger<BulkChangesService>();
    private readonly IHostApplicationLifetime    _hostApplicationLifetime = hostApplicationLifeTime;
    private readonly IBulkChangesOperation       _bulkChangesOperation    = bulkChangesOperation;
    private readonly BulkChangesSettings         _settings                = options.Value;
    #endregion

    private static Encoding? DetectEncoding(Stream stream)
    {
        try
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var encodingType = EncodingIdentifier.IdentifyEncoding(stream);
            var encodingDotNetName = EncodingTypeUtils.GetDotNetName(encodingType);

            if (!string.IsNullOrEmpty(encodingDotNetName))
            {
                return Encoding.GetEncoding(encodingDotNetName);
            }
        }
        catch
        {
            return null;
        }

        return null; // Return null or a default value in case of error
    }

    #region Métodos Interface IHostedService
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[{Timestamp:HH:mm:ss}] {ServiceName} - Iniciando no Diretório {RootDirectory}",
            DateTime.Now, nameof(BulkChangesService), _settings.RootDirectory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        );

        var fileList = _settings.FileList.Any() ? _settings.FileList.Select((fileName) => Path.Join(_settings.RootDirectory, fileName))
            : Directory.EnumerateFiles(path: _settings.RootDirectory, searchPattern: "*", searchOption: SearchOption.TopDirectoryOnly);

        var filesToChange = fileList.Select((filePath) => new FileInfo(filePath))
                                    .ExceptBy(_settings.IgnoreFiles, (file) => file.Name);

        foreach (var file in filesToChange)
        {
            _logger.LogInformation(
                "[{Timestamp:HH:mm:ss}] Processando {FileName}",
                DateTime.Now, file.Name
            );

            string originalPath = file.FullName;

            using var originalContents = await LoadFileAsync(originalPath);
            var encoding = DetectEncoding(originalContents) ?? Encoding.UTF8;
            using var updatedContents  = await _bulkChangesOperation.DoChangesAsync(originalContents, Encoding.UTF8.Equals(encoding) ? encoding : Encoding.ASCII);

            await SaveChangesAsync(updatedContents, originalPath);
        }

        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[{Timestamp:HH:mm:ss}] {ServiceName} - Finalizando",
            DateTime.Now, nameof(BulkChangesService)
        );
        return Task.CompletedTask;
    }
    #endregion

    #region Métodos Interface IBulkChangesService
    ///
    /// <inheritdoc cref="IBulkChangesService.LoadFileAsync(string)"/>
    ///
    public async Task<MemoryStream> LoadFileAsync(string originalPath)
    {
        var source = new FileStream(path: originalPath, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.Read,
                                    bufferSize: 4096, options: FileOptions.SequentialScan | FileOptions.Asynchronous);
        var destination = new MemoryStream();

        using (source) { await source.CopyToAsync(destination); }
        destination.Position = 0;

        return destination;
    }

    ///
    /// <inheritdoc cref="IBulkChangesService.SaveChangesAsync(MemoryStream, string)"/>
    ///
    public async Task SaveChangesAsync(MemoryStream updatedContents, string originalPath)
    {
        var (outputPath, fstreamOptions) = _settings.Mode switch {
            BulkChangesMode.PreserveOriginal => ConfigurePreserveOriginal(),

            BulkChangesMode.ReplaceOriginal => ConfigureReplaceOriginal(),

            BulkChangesMode.BackupAndReplaceOriginal => ConfigureReplaceOriginal(doBackup: true),

            _ => throw new NotSupportedException(),
        };

        var outputStream = new FileStream(path: outputPath, options: fstreamOptions);
        using (outputStream) { await updatedContents.CopyToAsync(outputStream); await outputStream.FlushAsync(); }

        _logger.LogInformation(
            "[{Timestamp:HH:mm:ss}] Alterações salvas: {UpdatedFilename}",
            DateTime.Now, outputPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        );

        #region Métodos Internos
        (string outputPath, FileStreamOptions fstreamOptions) ConfigurePreserveOriginal() {

            // Garante que o diretório de saída existe
            if (!Directory.Exists(_settings.OutputDirectory)) {
                Directory.CreateDirectory(_settings.OutputDirectory);

                _logger.LogInformation(
                    "[{Timestamp:HH:mm:ss}] Diretório de saída criado: {OutputDirectory}",
                    DateTime.Now, _settings.OutputDirectory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                );
            }

            // Mode: Create (ovewrite se já existe)
            return (
                outputPath: Path.Join(_settings.OutputDirectory, Path.GetFileName(originalPath)),
                fstreamOptions: new() {
                    Mode = FileMode.Create, Access = FileAccess.Write, Share = FileShare.None,
                    Options = (FileOptions.SequentialScan | FileOptions.Asynchronous)
                }
            );
        };

        (string outputPath, FileStreamOptions fstreamOptions) ConfigureReplaceOriginal(bool doBackup = false) {
            // Faz backup
            if (doBackup)
            {
                string backupFilename = Path.Join(_settings.RootDirectory, $"_BKUP_{Path.GetFileName(originalPath)}");
                var backupFile = new FileInfo(fileName: originalPath).CopyTo(destFileName: backupFilename, overwrite: true);

                _logger.LogInformation(
                    "[{Timestamp:HH:mm:ss}] Backup criado: {BackupFilename}",
                    DateTime.Now, backupFile.FullName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                );
            }

            // Mode: Truncate (deve existir, tamanho é setado para 0 bytes ao abrir)
            return (
                outputPath: originalPath,
                fstreamOptions: new() {
                    Mode = FileMode.Truncate, Access = FileAccess.Write, Share = FileShare.None,
                    Options = (FileOptions.SequentialScan | FileOptions.Asynchronous)
                }
            );
        }
        #endregion
    }
    #endregion
}
