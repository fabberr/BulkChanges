using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BulkChanges.Operations;

internal interface IBulkChangesOperation
{
    /// <summary>
    ///     Faz as alterações necessária sobre um arquivo.
    /// </summary>
    /// <param name="originalContents">
    ///     Uma stream contendo o conteúdo do arquivo original.
    /// </param>
    /// <param name="encoding">
    ///     Codificação do arquivo original.
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}"/><br/>
    ///     Uma Task que resultará em um <see cref="MemoryStream"/> contendo o
    ///     conteúdo do arquivo alterado.
    /// </returns>
    Task<MemoryStream> DoChangesAsync(MemoryStream originalContents, Encoding encoding);
}
