namespace Csharp.CancelTask.Example;

public class Client
{
    // 基本的に非同期処理を実行するクラスでは、以下のように CancellationToken を伝搬させる。
    // 最終的に非同期処理を実行する機能まで伝搬させてからキャンセル処理を実装する。
    // このパターンが C# における Task のキャンセル処理である。
    //
    // アプリケーションに近い層だとこの引数はデフォルト値を外したほうがいい。
    // なぜなら必ずキャンセル処理を定義することで実装漏れをふせぐことができるから。
    // ↑これは考えつかなかったので今後は取り入れたい。
    public async Task SendAsync(CancellationToken cancellationToken = default)
    {
        await SendAsyncCore(cancellationToken);
    }

    private async Task SendAsyncCore(CancellationToken cancellationToken)
    {
        foreach (var minutes in Enumerable.Range(1, 10).ToArray())
        {
            await Task.Delay(TimeSpan.FromSeconds(minutes), cancellationToken);
        }
    }
}