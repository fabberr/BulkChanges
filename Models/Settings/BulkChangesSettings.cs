using System.Collections.Generic;
using System.Linq;

namespace BulkChanges.Models.Settings;

/// <summary>
///     Enumera as estratégias empregadas na hora de salvar as alterações em disco.
/// </summary>
internal enum BulkChangesMode {
    /// <summary>
    ///     Preserva o arquivo original. O arquivo atualizado será salvo em
    ///     disco em um diretório separado.<br/>
    ///     Este é o comportamento padrão.
    /// </summary>
    PreserveOriginal = 1,

    /// <summary>
    ///     O arquivo original será substituido pelo atualizado em disco.
    /// </summary>
    ReplaceOriginal = 2,

    /// <summary>
    ///     Um backup do arquivo original será criado em disco antes de ser
    ///     substituido.<br/>
    ///     A cópia será criada com o prefixo <c>_BKUP_</c>.
    /// </summary>
    BackupAndReplaceOriginal = 3,
}

internal sealed class BulkChangesSettings
{
    public const string SectionKey = "BulkChanges";

    /// <summary>
    ///     Diretório raiz de onde serão extraídos os arquivos para alterar.
    /// </summary>
    public string RootDirectory { get; private set; } = "./";

    /// <summary>
    ///     Diretório de saída onde os arquivos atualizados serão salvos em
    ///     disco quando <see cref="Mode"/> estiver setado para
    ///     <see cref="BulkChangesMode.PreserveOriginal"/>.
    /// </summary>
    public string OutputDirectory { get; private set; } = "./output";

    /// <summary>
    ///     Lista de caminhos relativos.<br/>
    ///     Quando especificado, as alterações serão feitas apenas sobre estes
    ///     arquivos. Caso contrário, todos os arquivos presentes no diretório
    ///     <see cref="RootDirectory"/> serão alterados.
    /// </summary>
    public IEnumerable<string> FileList { get; private set; } = Enumerable.Empty<string>();

    /// <summary>
    ///     Lista de caminhos relativos.<br/>
    ///     Nomes dos arquivos que deverão ser ignorados.
    /// </summary>
    public IEnumerable<string> IgnoreFiles { get; private set; } = Enumerable.Empty<string>();

    /// <summary>
    ///     Define se um backup do arquivo antigo deverá ser criado ou não.
    /// </summary>
    public BulkChangesMode Mode { get; private set; } = BulkChangesMode.PreserveOriginal;
}
