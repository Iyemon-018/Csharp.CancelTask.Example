// See https://aka.ms/new-console-template for more information

using Csharp.CancelTask.Example;

var client = new Client();
;
// CancellationTokenSource は using することで Dispose したときに内部タイマーが停止するのでリークすることはない。
// cf. https://github.com/dotnet/runtime/blob/215b39abf947da7a40b0cb137eab4bceb24ad3e3/src/libraries/System.Private.CoreLib/src/System/Threading/CancellationTokenSource.cs#L445
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(5));           // この方法は「キャンセルできる」であって呼び出し側からすると「使いやすいとは言えない」コードになっている。

// 上記の実装だと以下のコードで5秒経過すると System.Threading.Tasks.TaskCanceledException がスローされる。
await client.SendAsync(cts.Token);

Console.WriteLine("Hello, World!");
