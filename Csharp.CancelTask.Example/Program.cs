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

    // 上記を別メソッドにして以下のように Cancel 呼び出すと、OperationCanceledException がスローされる。
    // 呼び出し元として使う場合はこっちがメインになるはず。
    //cts.Cancel();
}
catch (TimeoutException ex)
{
    // タイムアウト発生後はこっちのルートへ。
    Console.WriteLine(ex);
}

Console.WriteLine("Hello, World!");
