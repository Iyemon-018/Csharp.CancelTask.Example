namespace Csharp.CancelTask.Example;

public class Client : IDisposable
{
    // Dispose されたときにこのクラスが呼び出している非同期処理をキャンセルするための CancellationTokenSource。
    // Dispose 呼び出された＝リクエストも終了させる 場合に使用する。
    readonly CancellationTokenSource _clientLifetimeTokenSource;

    private readonly TimeSpan _timeout;
    
    public Client(TimeSpan timeout)
    {
        _timeout = timeout;
        _clientLifetimeTokenSource = new CancellationTokenSource();
    }

    // 基本的に非同期処理を実行するクラスでは、以下のように CancellationToken を伝搬させる。
    // 最終的に非同期処理を実行する機能まで伝搬させてからキャンセル処理を実装する。
    // このパターンが C# における Task のキャンセル処理である。
    //
    // アプリケーションに近い層だとこの引数はデフォルト値を外したほうがいい。
    // なぜなら必ずキャンセル処理を定義することで実装漏れをふせぐことができるから。
    // ↑これは考えつかなかったので今後は取り入れたい。
    public async Task SendAsync(CancellationToken cancellationToken = default)
    {
        // タイムアウトの時間設定はたいていアプリケーションで固定になる。
        // なので、このクラスの呼出側から設定するのは面倒くさい。何度も呼び出すことになるので。
        // それを解決するのが CancellationTokenSource.CreateLinkedToTokenSource になる。
        // 引数の cancellationToken を連結した CancellationTokenSource を作り出す。
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_clientLifetimeTokenSource.Token, cancellationToken);
        cts.CancelAfter(_timeout);

        try
        {
            await SendCoreAsync(cts.Token);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
        {
            // when (ex.CancellationToken == cts.Token) とすることで .SendAsync で発生したキャンセルの原因の判別が可能になる。
            // しかし、内部的に CancellationTokenSource.CreateLinkedToTokenSource を使って、かつ例外処理を実装していないとこの判別が不可能になる。
            // ex. https://github.com/dotnet/runtime/issues/21965
            // なので、上記のような内部で連結するような処理を実装した場合は例外処理もセットで実装する必要がある。

            if (cancellationToken.IsCancellationRequested)
            {
                // 呼び出し元の CancellationToken が原因でキャンセルされた場合はこっちのルートを通る。
                // OperationCanceledException の第3引数に、呼び出し元の CancellationToken を渡せば
                // 呼び出し元側で指定したタイムアウトによるものかどうかが判別できる。
                throw new OperationCanceledException(ex.Message, ex, cancellationToken);
            }
            else if (_clientLifetimeTokenSource.IsCancellationRequested)
            {
                // クライアント自体が Dispose されたときに呼び出される。
                // OperationCanceledException で呼び出し元が判別できるようにするか、
                // 独自の例外で判別できるようにするかはアプリケーションの設計次第。
                throw new OperationCanceledException("Client is disposed.", ex, _clientLifetimeTokenSource.Token);
            }
            else
            {
                // SendCoreAsync 内部で発生したタイムアウトなので、それを判別できるような例外なりなんなりの実装する。
                // ここはエントリに従って TimeoutException をスローすることとする。
                throw new TimeoutException($"The request was canceled. Timeout is [{_timeout}].");
            }
        }
    }

    private async Task SendCoreAsync(CancellationToken cancellationToken)
    {
        foreach (var minutes in Enumerable.Range(1, 10).ToArray())
        {
            await Task.Delay(TimeSpan.FromSeconds(minutes), cancellationToken);
        }
    }

    public void Dispose()
    {
        _clientLifetimeTokenSource.Cancel();
        _clientLifetimeTokenSource.Dispose();
    }
}