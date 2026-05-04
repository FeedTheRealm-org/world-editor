using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public struct SubscriptionData
{
    public int Slots;
    public string PricePerSlot;
    public string TotalPrice;
}

public class SubscriptionsCallbackServer : MonoBehaviour
{
    public const int Port = 9876;
    public const string SuccessPath = "/subscription/success/";
    public const string CancelPath = "/subscription/cancel/";

    public string SuccessUrl => $"http://localhost:{Port}{SuccessPath}";
    public string CancelUrl => $"http://localhost:{Port}{CancelPath}";

    public event Action OnSuccessEvent;
    public event Action OnCancelledEvent;

    private HttpListener listener;
    private CancellationTokenSource cts;

    public async Task StartServer(SubscriptionData data)
    {
        StopServer();

        cts = new CancellationTokenSource();
        listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{Port}/subscription/");
        listener.Start();

        Debug.Log($"[SubscriptionsCallbackServer] Listening on port {Port}");
        bool isSuccess = await ListenAsync(cts.Token, data);
        if (isSuccess)
            OnSuccessEvent?.Invoke();
        else
            OnCancelledEvent?.Invoke();
    }

    public void StopServer()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;

        if (listener != null && listener.IsListening)
            listener.Stop();
        listener = null;

        Debug.Log("[SubscriptionsCallbackServer] Stopped.");
    }

    private void OnDestroy()
    {
        StopServer();
    }

    private async Task<bool> ListenAsync(CancellationToken token, SubscriptionData data)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();
                string path = ctx.Request.Url.AbsolutePath;
                bool isSuccess = path.StartsWith(SuccessPath);
                bool isCancel = path.StartsWith(CancelPath);

                if (!isSuccess && !isCancel)
                    continue;

                string html = await GetHTMLAsync(isSuccess);
                html = ReplaceTemplate(html, data);

                await RespondAsync(ctx, html, token);

                StopServer();

                return isSuccess;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return false;
        }
        return false;
    }

    private static async Task RespondAsync(
        HttpListenerContext ctx,
        string html,
        CancellationToken token
    )
    {
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        ctx.Response.ContentType = "text/html";
        ctx.Response.ContentLength64 = buffer.Length;
        await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length, token);
        ctx.Response.OutputStream.Close();
    }

    private static string ReplaceTemplate(string html, SubscriptionData data)
    {
        return html.Replace("{{.Slots}}", data.Slots.ToString())
            .Replace("{{.PricePerSlot}}", data.PricePerSlot)
            .Replace("{{.TotalPrice}}", data.TotalPrice);
    }

    private static async Task<string> GetHTMLAsync(bool isSuccess)
    {
        string path = isSuccess
            ? Path.Combine(
                Application.streamingAssetsPath,
                "Templates",
                "SubscriptionsSuccess.html"
            )
            : Path.Combine(
                Application.streamingAssetsPath,
                "Templates",
                "SubscriptionsCancel.html"
            );

        return await File.ReadAllTextAsync(path);
    }
}
