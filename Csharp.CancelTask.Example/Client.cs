namespace Csharp.CancelTask.Example;

public class Client
{
    private readonly TimeSpan _timeout;
    
    public Client(TimeSpan timeout)
    {
        _timeout = timeout;
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
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        await SendCoreAsync(cts.Token);
    }

    private async Task SendCoreAsync(CancellationToken cancellationToken)
    {
        foreach (var minutes in Enumerable.Range(1, 10).ToArray())
        {
            await Task.Delay(TimeSpan.FromSeconds(minutes), cancellationToken);
        }
    }
}