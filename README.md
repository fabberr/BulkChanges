# BulkChanges
Projeto .NET 8 para realizar alterações em massa em arquivos HTML utilizando a biblioteca AngleSharp.

## Configurando o Projeto

Antes de executar o programa, é necessário configurar seu comportamento através do `appsettings.json`. As configurações devem ser adicionadas na chave `BulkChanges` dentro do arquivo e devem possuir o seguinte formato:

```typescript
"BulkChanges": {
  /** Diretório raiz de onde o programa irá busar arquivos para processar. */
  "RootDirectory": string,

  /** Diretório de Saída onde os arquivos alterados serão salvos.
   * Usado somente quando `mode` for "PreserveOriginal".
  */
  "OutputDirectory": string | null,

  /** Lista de caminhos relativos.
   * Quando informado, as alterações serão feitas somente sobre os arquivos presentes nesta lista.
   * Util caso queira testar alterações somente em um arquivo.
  */
  "FileList": string[] | null,

  /** Lista de caminhos relativos.
   * Quando informado, os arquivos presentes nesta lista serão desconsiderados.
   * Liste aqui os arquivos que já foram processados previamente.
  */
  "IgnoreFiles": string[] | null,

  /**
   * Determina como os resultados serão salvos.
   *
   * "PreserveOriginal": Mantém os arquivos originais sem nenhuma alteração.
   *                     Os arquivos alterados serão salvos no diretório especificado em `OutputDirectory`.
   * 
   * "ReplaceOriginal": Os arquivos originais serão substituídos pelos alterados sem backup.
   * 
   * "BackupAndReplaceOriginal": Backup dos arquivos originais serão feitos antes de serem substituídos pelos alterados.
   *                             Os backups serão salvos no diretório especificado em `RootDirectory`.
  */
  "Mode": "PreserveOriginal" | "ReplaceOriginal" | "BackupAndReplaceOriginal"
}
```

## Executando o Projeto

Para executar o programa, basta navegar até o diretório da Solution e rodar o comando `dotnet run`.
Ou manualmente pelo Visual Studio uando o atalho `Ctrl + F5`.
