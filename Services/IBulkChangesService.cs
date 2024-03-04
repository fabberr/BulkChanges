using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace BulkChanges.Services;

internal interface IBulkChangesService : IHostedService
{
    /// <summary>
    ///     Copia o arquivo original localizado em <paramref name="originalPath"/>
    ///     em memória para processamento, de forma assíncrona.<br/>
    ///     O arquivo em disco ficará aberto apenas enquanto é copiado.
    /// </summary>
    /// <param name="originalPath">
    ///     Caminho absoluto do até o arquivo original.
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}"/><br/>
    ///     Uma Task que resultará em um <see cref="MemoryStream"/> contento a
    ///     cópia em memória do arquivo original.
    /// </returns>
    Task<MemoryStream> LoadFileAsync(string originalPath);

    /// <summary>
    ///     Salva as alterações em disco.
    /// </summary>
    /// <param name="updatedContents">
    ///     Um <see cref="MemoryStream"/> contendo as alterações a serem salvas.
    /// </param>
    /// <param name="originalPath">
    ///     Caminho absoluto do até o arquivo original.
    /// </param>
    /// <returns>
    ///     <see cref="Task"/>
    /// </returns>
    Task SaveChangesAsync(MemoryStream updatedContents, string originalPath);
}
