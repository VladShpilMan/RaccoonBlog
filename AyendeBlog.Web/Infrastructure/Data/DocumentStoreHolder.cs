using System.Security.Cryptography.X509Certificates;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Http;
using SessionOptions = Raven.Client.Documents.Session.SessionOptions;

namespace AyendeBlog.Web.Infrastructure.Data;

public interface IDocumentStoreHolder
{
    IDocumentStore DocumentStore { get; }
    IDocumentSession OpenSession();
    IDocumentSession OpenSession(TransactionMode transactionMode);
    IAsyncDocumentSession OpenAsyncSession();
    IAsyncDocumentSession OpenAsyncSession(TransactionMode transactionMode);
    OperationExecutor Operations();
    BulkInsertOperation BulkInsert();
    SubscriptionWorker<T> GetSubscriptionWorker<T>(SubscriptionWorkerOptions options) where T : class;
}

public class NewDatabaseSettings
{
    public string[] Urls { get; set; }
    public string DatabaseName { get; set; }
    public string CertificatePath { get; set; }
    public string CertificatePassword { get; set; }
    public int? RequestsTimeoutInSec { get; set; }
}

public class DocumentStoreHolder : IDocumentStoreHolder, IDisposable
{
    private readonly IDocumentStore _documentStore;
    private readonly NewDatabaseSettings _settings;
    private readonly ILogger<DocumentStoreHolder> _logger;

    public DocumentStoreHolder(NewDatabaseSettings settings, ILogger<DocumentStoreHolder> logger)
    {
        _settings = settings;
        _logger = logger;

        ValidateSettings(_settings);
        
        _documentStore = CreateDocumentStore();
    }
    
    public IDocumentStore DocumentStore => _documentStore;

    private void ValidateSettings(NewDatabaseSettings databaseSettings)
    {
        if (string.IsNullOrEmpty(databaseSettings.DatabaseName))
            throw new InvalidOperationException($"{nameof(databaseSettings.DatabaseName)} was not provided in the application settings.");

        if (databaseSettings.Urls == null || databaseSettings.Urls.Length == 0)
            throw new InvalidOperationException($"{nameof(databaseSettings.Urls)} was not provided in the application settings.");

#if !DEBUG
        // В Release режиме (на продакшене) мы строго требуем сертификат
        if (string.IsNullOrEmpty(databaseSettings.CertificatePath))
            throw new InvalidOperationException($"{nameof(databaseSettings.CertificatePath)} was not provided in the application settings.");

        if (File.Exists(databaseSettings.CertificatePath) == false)
            throw new InvalidOperationException($"File under {nameof(databaseSettings.CertificatePath)} does not exist.");
#endif
    }

    private DocumentStore CreateDocumentStore()
    {
        var store = new DocumentStore
        {
            Urls = _settings.Urls,
            Database = _settings.DatabaseName
        };
        
        store.Conventions.AggressiveCache.Duration = TimeSpan.FromMinutes(1);
        store.Conventions.AggressiveCache.Mode = AggressiveCacheMode.DoNotTrackChanges;

        store.OnBeforeQuery += (sender, beforeQueryExecutedArgs) =>
        {
            beforeQueryExecutedArgs.QueryCustomization.WaitForNonStaleResults(TimeSpan.FromSeconds(30));
        };
        
        if (!string.IsNullOrEmpty(_settings.CertificatePath))
        {
            var certificatePassword = _settings.CertificatePassword;
            var certificate = new X509Certificate2(_settings.CertificatePath, certificatePassword);
            store.Certificate = certificate;

            store.AfterDispose += (sender, args) =>
            {
                certificate.Dispose(); 
            };
        }

        if (_settings.RequestsTimeoutInSec.HasValue)
        {
            store.Conventions.RequestTimeout = TimeSpan.FromSeconds(_settings.RequestsTimeoutInSec.Value);
        }
        
        store.Initialize();

        _logger.LogInformation($"RavenDB DocumentStore initialized for {_settings.DatabaseName} at {_settings.Urls[0]}");

#if DEBUG
        DeployIndexes(store);
#endif
        return store;
    }
    
    private void DeployIndexes(IDocumentStore store)
    {
        // TODO: add DeployIndexes
    }

    public IDocumentSession OpenSession()
    {
        return _documentStore.OpenSession(_settings.DatabaseName);
    }

    public IDocumentSession OpenSession(TransactionMode transactionMode)
    {
        return _documentStore.OpenSession(new SessionOptions
        {
            Database = _settings.DatabaseName,
            TransactionMode = transactionMode
        });
    }

    public IAsyncDocumentSession OpenAsyncSession()
    {
        return _documentStore.OpenAsyncSession(_settings.DatabaseName);
    }

    public IAsyncDocumentSession OpenAsyncSession(TransactionMode transactionMode)
    {
        return _documentStore.OpenAsyncSession(new SessionOptions
        {
            Database = _settings.DatabaseName,
            TransactionMode = transactionMode
        });
    }

    public OperationExecutor Operations()
    {
        return _documentStore.Operations.ForDatabase(_settings.DatabaseName);
    }

    public BulkInsertOperation BulkInsert()
    {
        return _documentStore.BulkInsert(_settings.DatabaseName);
    }

    public SubscriptionWorker<T> GetSubscriptionWorker<T>(SubscriptionWorkerOptions options) where T : class
    {
        return _documentStore.Subscriptions.GetSubscriptionWorker<T>(options, _settings.DatabaseName);
    }
    
    public void Dispose()
    {
        _logger.LogInformation("Disposing DocumentStore...");
        _documentStore?.Dispose();
    }
}