// See https://aka.ms/new-console-template for more information

using Csharp.CancelTask.Example;

// 引数でタイムアウトを指定することで実装が楽になる。
// もしくはデフォルトコンストラクタでタイムアウトを実装できるようにして、
// 引数ありコンストラクタでタイムアウトを設定できるようにするのもありかも。
var client = new Client(TimeSpan.FromSeconds(5));

// CancellationTokenSource は using することで Dispose したときに内部タイマーが停止するのでリークすることはない。
// cf. https://github.com/dotnet/runtime/blob/215b39abf947da7a40b0cb137eab4bceb24ad3e3/src/libraries/System.Private.CoreLib/src/System/Threading/CancellationTokenSource.cs#L445
using var cts = new CancellationTokenSource();
try
{
    // 上記の実装だと以下のコードで5秒経過すると System.Threading.Tasks.TaskCanceledException がスローされる。
    await client.SendAsync(cts.Token);
}
catch (OperationCanceledException ex)
        when (ex.CancellationToken == cts.Token)
{
    // タイムアウト発生後はこっちのルートへ。
    // when (ex.CancellationToken == cts.Token) とすることで .SendAsync で発生したキャンセルの原因の判別が可能になる。
    // しかし、内部的に CancellationTokenSource.CreateLinkedToTokenSource を使って、かつ例外処理を実装していないとこの判別が不可能になる。
    // ex. https://github.com/dotnet/runtime/issues/21965
    // なので、上記のような内部で連結するような処理を実装した場合は例外処理もセットで実装する必要がある。
    Console.WriteLine(ex);
}

Console.WriteLine("Hello, World!");
