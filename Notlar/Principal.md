# Principal .NET Developer Interview Rehberi

Bu doküman Principal .NET Developer pozisyonu için hazırlanmış kapsamlı bir rehberdir. Sorular farklı alanlarda hazırlanmış, detaylı açıklamalar, örnekler, zorluk seviyeleri ve takip soruları içerir.

---

## 1. .NET Core / C# Advanced

### Soru 1
**Soru:** `async/await` ve `Task.Run` arasındaki farklar nelerdir? Hangi senaryolarda kullanılır?  
**Beklenen Cevap:** `async/await` I/O-bound işlemler için, `Task.Run` CPU-bound işlemler için kullanılır.  
**Ayrıntılı Açıklama / Örnek:** `async/await` thread’i bloklamadan çalışır. `Task.Run` CPU yoğun işlerde thread pool’dan thread alır. Örnek: DB çağrısı için `async/await`, hesaplama için `Task.Run`.  
**Zorluk Seviyesi:** 3  
**Takip Soruları:** ValueTask farkı, thread pool limitleri, performans etkisi

### Soru 2
**Soru:** IAsyncEnumerable<T> ile IEnumerable<T> farkı nedir?  
**Beklenen Cevap:** IAsyncEnumerable asenkron veri stream sağlar.  
**Ayrıntılı Açıklama / Örnek:** Async stream, büyük dataset’leri async olarak enumerate etmek için kullanılır. Örnek: async foreach ile API’den veri akışı.  
**Zorluk Seviyesi:** 3  
**Takip Soruları:** Performans etkisi, cancellation token kullanımı

### Soru 3
**Soru:** Dependency Injection ile Scoped, Singleton, Transient farkları nelerdir?  
**Beklenen Cevap:** Scoped: request başına, Singleton: app lifetime, Transient: her resolve’da yeni instance.  
**Ayrıntılı Açıklama / Örnek:** Singleton içinde scoped kullanmak tehlikelidir; memory leak veya stale data oluşabilir.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** IoC container konfigürasyon örneği

### Soru 4
**Soru:** Reflection kullanımı performansı etkiler mi? Hangi durumlarda kullanılır?  
**Beklenen Cevap:** Evet, runtime maliyeti vardır. Genellikle framework, serialization veya metaprogramming için kullanılır.  
**Ayrıntılı Açıklama / Örnek:** Örnek: property adını string ile almak veya dynamic object mapping yapmak.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Expression trees ile performans optimizasyonu

### Soru 5
**Soru:** Span<T> ve Memory<T> nedir, performans optimizasyonu için nasıl kullanılır?  
**Beklenen Cevap:** Bellek üzerinde slice ve stack-allocated veriler için kullanılır, GC pressure azaltır.  
**Ayrıntılı Açıklama / Örnek:** Örnek: büyük byte array işlemleri için Span kullanmak, ekstra kopyalamayı önler.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Stackalloc ile kombinasyon, async kullanımı

### Soru 6
**Soru:** ValueTask ve Task farkı nedir?  
**Beklenen Cevap:** Task her zaman heap üzerinde, ValueTask daha hafif ve re-use edilebilir.  
**Ayrıntılı Açıklama / Örnek:** Kısa-lived veya sıklıkla dönen async metodlarda ValueTask performans avantajı sağlar.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** ConfigureAwait, multiple await kullanımı

### Soru 7
**Soru:** lock, SemaphoreSlim, Monitor, ConcurrentDictionary farkları nedir?  
**Beklenen Cevap:** Thread safety sağlarlar ama farklı senaryolar için uygundur.  
**Ayrıntılı Açıklama / Örnek:** lock basit thread safety sağlar, SemaphoreSlim async uyumlu, Monitor advanced lock. ConcurrentDictionary concurrent collection için optimize.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Deadlock senaryoları, async/await ile uyumluluk

### Soru 8
**Soru:** EF Core’da N+1 query problem nasıl ortaya çıkar ve çözümü nedir?  
**Beklenen Cevap:** Lazy loading ile her entity için ayrı query. Eager loading veya projection ile çözülür.  
**Ayrıntılı Açıklama / Örnek:** `.Include(u => u.Orders)` kullanımı veya DTO mapping ile optimize.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Performans metrikleri, batch query

### Soru 9
**Soru:** CQRS pattern’i nasıl uygularsınız ve ne zaman tercih edilir?  
**Beklenen Cevap:** Read ve Write operasyonları ayrı katmanlara ayrılır; yüksek performans ve maintainability sağlar.  
**Ayrıntılı Açıklama / Örnek:** Komutlar için CommandHandler, sorgular için QueryHandler kullanılır. Örnek: Inventory write, Reporting read.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Event sourcing ile kombinasyon

### Soru 10
**Soru:** Circuit breaker pattern nedir ve .NET’de nasıl uygulanır?  
**Beklenen Cevap:** Remote service hatalarını yönetir, retry ve fallback ile kombinlenir.  
**Ayrıntılı Açıklama / Örnek:** Polly library ile uygulanabilir. Örnek: API 3 kez fail olursa fallback’a geç.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Timeout pattern, retry policy kombinasyonu

---

## 2. Microservices & Distributed Systems

### Soru 11
**Soru:** Microservice’ler arası synchronous ve asynchronous iletişim farkları nelerdir?  
**Beklenen Cevap:** Sync: request-response, Async: event-driven.  
**Ayrıntılı Açıklama / Örnek:** Sync hızlı ama tight coupling, Async loose coupling, scalability sağlar. Örnek: Kafka event tüketimi.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Eventual consistency, retry pattern

### Soru 12
**Soru:** Event-driven architecture avantajları nelerdir?  
**Beklenen Cevap:** Loose coupling, scalability, async processing sağlar.  
**Ayrıntılı Açıklama / Örnek:** Kafka, RabbitMQ ile mesaj tabanlı eventler. Örnek: OrderCreated event -> Inventory update.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Eventual consistency senaryosu

### Soru 13
**Soru:** SAGA pattern nedir ve ne zaman kullanılır?  
**Beklenen Cevap:** Distributed transaction management için.  
**Ayrıntılı Açıklama / Örnek:** Örnek: Ödeme -> Stok -> Fatura zinciri, rollback event ile geri alınabilir.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** 2PC ile farkları

### Soru 14
**Soru:** Circuit breaker, retry, fallback pattern’leri nasıl uygulanır?  
**Beklenen Cevap:** Remote service hatalarında resiliency sağlar.  
**Ayrıntılı Açıklama / Örnek:** Polly kütüphanesi ile .NET uygulamasında uygulanabilir.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Timeout ve bulkhead kombinasyonu

### Soru 15
**Soru:** API Gateway kullanmanın avantajları ve dezavantajları nelerdir?  
**Beklenen Cevap:** Centralized routing, authentication, throttling. Dezavantaj: single point of failure.  
**Ayrıntılı Açıklama / Örnek:** Örnek: Ocelot veya YARP ile microservices yönlendirme.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Rate limiting ve caching stratejileri

## 2. Microservices & Distributed Systems (Devam)

### Soru 16
**Soru:** Service Discovery nedir ve microservice mimarisinde neden önemlidir?  
**Beklenen Cevap:** Servislerin adreslerini dinamik olarak bulmayı sağlar.  
**Ayrıntılı Açıklama / Örnek:** Eureka veya Consul kullanılarak servisler birbirini bulabilir. Load balancer ile birlikte çalışır.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** DNS-based discovery vs client-side discovery

### Soru 17
**Soru:** Distributed caching ne zaman ve nasıl kullanılmalıdır?  
**Beklenen Cevap:** Sık kullanılan veriler için, performansı artırmak ve DB yükünü azaltmak için kullanılır.  
**Ayrıntılı Açıklama / Örnek:** Redis veya Memcached ile microservices caching yapılabilir. Örnek: Product catalog cache.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Cache invalidation stratejileri

### Soru 18
**Soru:** Idempotency nedir ve API tasarımında neden önemlidir?  
**Beklenen Cevap:** Aynı isteğin tekrar edilmesi durumunda sonucu değiştirmemesi.  
**Ayrıntılı Açıklama / Örnek:** Payment API’sinde duplicate ödeme önlemek için idempotency key kullanılır.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** RESTful API’de uygulama örnekleri

### Soru 19
**Soru:** Event Sourcing nedir ve hangi durumlarda tercih edilir?  
**Beklenen Cevap:** State yerine event’lerin saklanmasıdır; audit ve rollback avantajı sağlar.  
**Ayrıntılı Açıklama / Örnek:** Örnek: Banka hesap işlemleri, her transaction bir event olarak kaydedilir.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** CQRS ile kombinasyon, snapshot kullanımı

### Soru 20
**Soru:** API Rate Limiting neden önemlidir ve nasıl uygulanır?  
**Beklenen Cevap:** Servisleri overload’dan korur.  
**Ayrıntılı Açıklama / Örnek:** Token bucket veya leaky bucket algoritmaları ile API gateway üzerinde uygulanabilir.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Distributed rate limiting senaryoları

---

## 3. Scalability & Performance

### Soru 21
**Soru:** Vertical vs Horizontal Scaling farkları nelerdir?  
**Beklenen Cevap:** Vertical: mevcut sunucuyu büyütme, Horizontal: daha fazla sunucu ekleme.  
**Ayrıntılı Açıklama / Örnek:** Horizontal scaling genellikle microservices ile uyumludur; load balancer gerektirir.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Auto-scaling senaryoları

### Soru 22
**Soru:** Load Balancing türleri nelerdir ve ne zaman tercih edilir?  
**Beklenen Cevap:** Round-robin, least connections, IP-hash gibi türler vardır.  
**Ayrıntılı Açıklama / Örnek:** Round-robin basit ve yaygın; least connections yoğun trafikte daha iyi performans sağlar.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Sticky session avantajları

### Soru 23
**Soru:** Thread Pool nedir ve performansı nasıl etkiler?  
**Beklenen Cevap:** Thread reuse sağlar ve sistem kaynaklarını optimize eder.  
**Ayrıntılı Açıklama / Örnek:** CPU-bound task’lar ThreadPool ile çalıştırılır; task sayısı çok fazla ise blocking olabilir.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** MaxThreadPool ayarları, async/await ile ilişkisi

### Soru 24
**Soru:** Profiling ve Performance tuning nasıl yapılır?  
**Beklenen Cevap:** Bottleneck belirlemek için kullanılır.  
**Ayrıntılı Açıklama / Örnek:** dotTrace, PerfView veya built-in Diagnostic Tools ile CPU, Memory ve IO analizi yapılabilir.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Memory leak tespiti, GC optimizasyonları

### Soru 25
**Soru:** Caching stratejileri nelerdir ve ne zaman tercih edilir?  
**Beklenen Cevap:** In-memory, distributed, write-through, write-behind gibi stratejiler vardır.  
**Ayrıntılı Açıklama / Örnek:** Redis distributed cache; write-through DB ile senkronize, write-behind async update.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Cache consistency sorunları

## 4. Database / EF Core

### Soru 26
**Soru:** ACID prensipleri nelerdir ve neden önemlidir?  
**Beklenen Cevap:** Atomicity, Consistency, Isolation, Durability. Veri tutarlılığı için gereklidir.  
**Ayrıntılı Açıklama / Örnek:** Örnek: Banka transferinde ya tüm işlem yapılır ya da hiçbiri.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Isolation level ve deadlock örnekleri

### Soru 27
**Soru:** EF Core’da Lazy Loading, Eager Loading ve Explicit Loading farkları nelerdir?  
**Beklenen Cevap:** Lazy: ihtiyaç olunca yükler, Eager: Include ile önceden yükler, Explicit: manuel yükler.  
**Ayrıntılı Açıklama / Örnek:** `.Include(u => u.Orders)` ile eager loading yapılır.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Performans etkisi ve N+1 problem çözümü

### Soru 28
**Soru:** TransactionScope ve DbContext transaction farkları nelerdir?  
**Beklenen Cevap:** TransactionScope distributed transaction için, DbContext local transaction için uygundur.  
**Ayrıntılı Açıklama / Örnek:** TransactionScope ile birden fazla DB üzerinde tek transaction yapılabilir.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Nested transaction senaryoları

### Soru 29
**Soru:** Indexing nedir ve performansı nasıl etkiler?  
**Beklenen Cevap:** Sorgu hızını artırır ama write maliyeti vardır.  
**Ayrıntılı Açıklama / Örnek:** Clustered vs Non-clustered index. Örnek: ProductId üzerinde index sorgu hızını artırır.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Composite index, covering index

### Soru 30
**Soru:** NoSQL ve SQL veri tabanları arasındaki farklar nelerdir?  
**Beklenen Cevap:** SQL: relational, schema; NoSQL: flexible, scalable.  
**Ayrıntılı Açıklama / Örnek:** MongoDB, Cassandra; Redis key-value store.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Eventual consistency, CAP theorem

---

## 5. Cloud / Azure

### Soru 31
**Soru:** Azure App Service ile Azure Functions arasındaki fark nedir?  
**Beklenen Cevap:** App Service: full web app, Functions: serverless, event-driven.  
**Ayrıntılı Açıklama / Örnek:** Functions, mikro işlerde, trigger bazlı çalışır.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Cold start problemi, scaling

### Soru 32
**Soru:** Azure Service Bus ve Event Grid farkları nelerdir?  
**Beklenen Cevap:** Service Bus: queue-based, point-to-point; Event Grid: publish-subscribe, event-driven.  
**Ayrıntılı Açıklama / Örnek:** Service Bus ile mesaj sırası garanti edilir, Event Grid ile event broadcast edilir.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Dead-letter queue, retry policy

### Soru 33
**Soru:** Cloud scalability ve availability nasıl sağlanır?  
**Beklenen Cevap:** Load balancing, auto-scaling, multiple regions.  
**Ayrıntılı Açıklama / Örnek:** Azure Traffic Manager ile global routing, multiple App Service instances.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** High availability SLA, failover senaryoları

### Soru 34
**Soru:** Azure Key Vault kullanım senaryoları nelerdir?  
**Beklenen Cevap:** Secrets, keys, certificates yönetimi ve güvenliği.  
**Ayrıntılı Açıklama / Örnek:** DB connection string veya API keys güvenli şekilde saklanır ve çekilir.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Managed identity ile erişim

### Soru 35
**Soru:** Serverless mimarinin avantajları ve dezavantajları nelerdir?  
**Beklenen Cevap:** Avantaj: ölçeklenebilir, maliyet etkin; Dezavantaj: cold start, debugging zor.  
**Ayrıntılı Açıklama / Örnek:** Azure Functions, AWS Lambda.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Durability ve state management

---

## 6. Security

### Soru 36
**Soru:** JWT token nedir ve nasıl doğrulanır?  
**Beklenen Cevap:** JSON Web Token, kimlik doğrulama ve authorization için kullanılır.  
**Ayrıntılı Açıklama / Örnek:** Token imzalanır, signature doğrulanarak payload güvenliği sağlanır.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Refresh token, token expiration

### Soru 37
**Soru:** OAuth2 ve OpenID Connect farkları nelerdir?  
**Beklenen Cevap:** OAuth2 authorization framework; OIDC identity layer sağlar.  
**Ayrıntılı Açıklama / Örnek:** OIDC ile kullanıcı identity bilgisi alınabilir.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Implicit vs Authorization Code flow

### Soru 38
**Soru:** Secure coding ve OWASP top 10 güvenlik açıkları nelerdir?  
**Beklenen Cevap:** SQL Injection, XSS, CSRF gibi.  
**Ayrıntılı Açıklama / Örnek:** Parameterized queries, input validation, proper authentication.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Pen-testing örnekleri

### Soru 39
**Soru:** HTTPS, SSL/TLS ve sertifika doğrulaması nasıl çalışır?  
**Beklenen Cevap:** Veri transferi şifrelenir; server identity doğrulanır.  
**Ayrıntılı Açıklama / Örnek:** SSL handshake, public/private key, certificate chain.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Mutual TLS, HSTS

### Soru 40
**Soru:** Sensitive data encryption ve hashing farkları nelerdir?  
**Beklenen Cevap:** Encryption reversible, hashing tek yönlü.  
**Ayrıntılı Açıklama / Örnek:** Password hashing için bcrypt, data encryption için AES.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Salting, key management

---

## 7. Architecture / Design Patterns

### Soru 41
**Soru:** Singleton, Factory, Repository, Unit of Work pattern’leri ne zaman kullanılır?  
**Beklenen Cevap:** Singleton: tek instance, Factory: object creation, Repository: DB abstraction, Unit of Work: transaction yönetimi.  
**Ayrıntılı Açıklama / Örnek:** Örnek: Repository ile EF Core işlemleri, Unit of Work ile commit/rollback.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Dependency Injection ile kombinasyon

### Soru 42
**Soru:** SOLID prensipleri nelerdir ve örnekleri?  
**Beklenen Cevap:** Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion.  
**Ayrıntılı Açıklama / Örnek:** Örnek: SRP -> Class tek sorumluluk, DIP -> high-level module abstraction üzerinden dependency.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Anti-pattern örnekleri

### Soru 43
**Soru:** Microservices vs Monolithic Architecture avantaj ve dezavantajları?  
**Beklenen Cevap:** Monolithic: kolay başlangıç, zor scaling; Microservices: kolay scaling, complex deployment.  
**Ayrıntılı Açıklama / Örnek:** Örnek: e-commerce microservices (order, inventory, payment).  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Hybrid architecture senaryoları

### Soru 44
**Soru:** Domain Driven Design (DDD) nedir?  
**Beklenen Cevap:** İş domainine odaklanan, bounded context ile modelleme yaklaşımı.  
**Ayrıntılı Açıklama / Örnek:** Entities, ValueObjects, Aggregates ile modelleme yapılır.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** CQRS ve Event Sourcing ile kombinasyon

### Soru 45
**Soru:** Logging ve Monitoring best practices nelerdir?  
**Beklenen Cevap:** Centralized logging, structured logs, metrics ve alerts.  
**Ayrıntılı Açıklama / Örnek:** Serilog, Seq, Prometheus, Grafana.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Distributed tracing, correlation ID

### Soru 46
**Soru:** Dependency Injection ve Inversion of Control neden önemlidir?  
**Beklenen Cevap:** Loose coupling ve testability sağlar.  
**Ayrıntılı Açıklama / Örnek:** Service registration ve constructor injection ile uygulanır.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Scoped vs Singleton lifetimes

### Soru 47
**Soru:** Circuit breaker, retry ve bulkhead pattern’leri ne zaman kullanılmalı?  
**Beklenen Cevap:** Remote service failure, resiliency ve fault-tolerance için.  
**Ayrıntılı Açıklama / Örnek:** Polly library ile implement.  
**Zorluk Seviyesi:** 5  
**Takip Soruları


### Soru 48
**Soru:** Bulkhead pattern nedir ve hangi senaryolarda kullanılır?  
**Beklenen Cevap:** Sistem kaynaklarını izole ederek bir bölümün hata yapmasının diğer bölümleri etkilemesini önler.  
**Ayrıntılı Açıklama / Örnek:** Örnek: Microservice içinde farklı queue processing’leri ayrı thread pool veya container’da çalıştırmak, bir kuyruğun çökmesi diğerlerini etkilemez.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Circuit breaker ve retry ile kombinasyonu

### Soru 49
**Soru:** Eventual consistency nedir ve microservice mimarisinde nasıl uygulanır?  
**Beklenen Cevap:** Sistemin zaman içinde tutarlı hale gelmesi; immediate consistency garanti edilmez.  
**Ayrıntılı Açıklama / Örnek:** Örnek: Order ve Inventory service ayrı DB’de, event-driven update ile zamanla tutarlılık sağlanır.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Conflict resolution ve idempotency

### Soru 50
**Soru:** Polyglot persistence nedir ve hangi durumlarda tercih edilir?  
**Beklenen Cevap:** Farklı veri tabanlarını veri tipine göre kullanmak.  
**Ayrıntılı Açıklama / Örnek:** Örnek: Transactional data için SQL, caching için Redis, logging için NoSQL (MongoDB).  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Data migration ve integration sorunları

### Soru 51
**Soru:** Observability kavramı nedir ve .NET uygulamalarında nasıl sağlanır?  
**Beklenen Cevap:** Metrics, logging ve tracing ile sistem davranışının izlenmesi.  
**Ayrıntılı Açıklama / Örnek:** OpenTelemetry, Prometheus, Grafana ile monitoring; distributed tracing ile request flow takibi.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Correlation ID, latency measurement

### Soru 52
**Soru:** Rate limiting ve throttling arasındaki fark nedir?  
**Beklenen Cevap:** Rate limiting: belirli bir süre içinde istek sayısını sınırlar; Throttling: anlık yükü yönetir.  
**Ayrıntılı Açıklama / Örnek:** API Gateway’de token bucket veya leaky bucket algoritmaları kullanılır.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Distributed system uygulamaları

### Soru 53
**Soru:** Deadlock nedir ve .NET uygulamalarında nasıl önlenir?  
**Beklenen Cevap:** Thread’lerin birbirini beklemesi sonucu tüm sistemin durması.  
**Ayrıntılı Açıklama / Örnek:** Lock sıralamasına dikkat ederek, timeout kullanarak veya Concurrent Collections ile önlenebilir.  
**Zorluk Seviyesi:** 5  
**Takip Soruları:** Async/await ile deadlock senaryoları

### Soru 54
**Soru:** Scalability test ve load test nasıl yapılır?  
**Beklenen Cevap:** Sistem kapasitesini ve performansını ölçmek için yapılır.  
**Ayrıntılı Açıklama / Örnek:** JMeter, k6 veya Azure Load Testing ile testler yapılır; bottleneck tespiti.  
**Zorluk Seviyesi:** 4  
**Takip Soruları:** Stress test, spike test farkları
