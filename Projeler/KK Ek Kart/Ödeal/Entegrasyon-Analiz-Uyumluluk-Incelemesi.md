# ÖdealKart Entegrasyon ve Analiz Uyumluluk İncelemesi

**İnceleme tarihi:** 14.09.2026  
**İncelenen dokümanlar:**

- [Odeal-Kart-Servisleri-Entegrasyon-Dokumani (1).pdf](Odeal-Kart-Servisleri-Entegrasyon-Dokumani%20%281%29.pdf)
- [Analiz_1_Intertech_Musteri_Yukleme.docx](Analiz_1_Intertech_Musteri_Yukleme.docx)
- [Analiz_2_PayCore_Kart_Yonetimi.docx](Analiz_2_PayCore_Kart_Yonetimi.docx)
- Bağlam kontrolü için [ÖdealKart_OdeaBank_Sunumu.pdf](ÖdealKart_OdeaBank_Sunumu.pdf)

---

## 1. Yönetici özeti

Servislerin Intertech–PayCore ayrımı **büyük ölçüde doğru** yapılmıştır:

- Müşteri, adres, telefon, il/ilçe ve yükleme para bacağı **Intertech** tarafına alınmıştır.
- Kart üretimi, kart atama, kart sorgulama, statü, izin, transfer, hareket ve ekstre servisleri **PayCore** tarafına alınmıştır.

Buna rağmen analizler mevcut haliyle geliştirme talebine dönüştürülmeye hazır değildir. Bakiye yükleme akışı, ürün/limit modeli, karttan karta transferin muhasebe bacağı, ortak hata modeli ve webhook sorumluluğu gibi kritik konular netleştirilmelidir.

Entegrasyon dokümanında **26 endpoint + 1 webhook sözleşmesi** bulunmaktadır. Bunların büyük bölümü doğru firmaya atanmıştır; ancak bakiye yüklemeye ait servislerin dış sözleşme sahibi ve Core–PayCore orkestrasyonu tanımsızdır.

---

## 2. Net değerlendirme

### Doğru yapılan ayrım

- **Intertech:** Müşteri ve referans servisleri, adres/telefon yönetimi, bakiye yüklemenin para ve muhasebe bacağı.
- **PayCore:** Kart yaşam döngüsü, kart yönetimi, kart izinleri, yükleme kart bacağı, transfer, hareket ve ekstre.
- İki analizin kapsam dışı bıraktığı konular birbirini büyük ölçüde tamamlıyor.
- PAN’ın saklanmaması, URL/query içinde taşınmaması ve kartın token ile temsil edilmesi doğru aktarılmış.
- Yükleme için idempotency, asenkron sonuç, webhook ve reversal ihtiyacı temel seviyede yakalanmış.

### Ana hüküm

Servislerin firma bazında ayrımı doğru; ancak entegrasyon dokümanıyla tam uyum için aşağıdaki kritik kararlar netleştirilmelidir:

1. Bakiye yükleme servisinin dış API sahibi ve uçtan uca akışı
2. Prepaid model ile 1 TL limitli kredi kartı yaklaşımı arasındaki çelişki
3. Müşteri adı ve soyadı alanlarının müşteri oluşturma sözleşmesine eklenmesi
4. Karttan karta transferin muhasebe, MASAK ve mutabakat sorumluluğu
5. Ortak hata modeli, OAuth2 token sahipliği ve webhook güvenilirliği

---

## 3. Servis envanteri ve firma ayrımı

| Servis ailesi | Endpoint sayısı | Doğru firma | Durum |
|---|---:|---|---|
| Müşteri oluşturma | 1 | Intertech | Uyumlu; ad/soyad eksik |
| İl/ilçe referansı | 2 | Intertech | Uyumlu |
| Adres yönetimi | 3 | Intertech | Uyumlu |
| Telefon yönetimi | 3 | Intertech | Uyumlu |
| Kart oluşturma/atama/teslimat | 4 | PayCore | Uyumlu |
| Kart bilgisi, müşteri kartları, statü, yenileme | 4 | PayCore | Uyumlu |
| Kart izinleri | 2 | PayCore | Uyumlu; varsayılanlar netleşmeli |
| Bakiye yükleme ve durum sorgulama | 2 | PayCore kart bacağı + Intertech para bacağı | Akış belirsiz |
| Karttan karta transfer | 2 | PayCore | Kısmen uyumlu; muhasebe bacağı eksik |
| Kart hareketleri | 1 | PayCore | Uyumlu |
| Ekstre liste/detay | 2 | PayCore | Uyumlu |
| Webhook | 1 | Ortak | Gönderici ve retry modeli netleşmeli |

### 3.1 Intertech analizi kapsamı

[Analiz_1_Intertech_Musteri_Yukleme.docx](Analiz_1_Intertech_Musteri_Yukleme.docx), entegrasyon dokümanındaki müşteri ve referans servislerini kapsamaktadır:

- `POST /customers`
- `GET /regions`
- `GET /regions/{region_code}/districts`
- `GET /customers/{customer_id}/addresses`
- `POST /customers/{customer_id}/addresses`
- `PUT /customers/{customer_id}/addresses/{address_id}`
- `GET /customers/{customer_id}/phones`
- `POST /customers/{customer_id}/phones`
- `PUT /customers/{customer_id}/phones/{phone_id}`

Ek olarak bakiye yüklemenin para bacağı, torba hesaptan düşüş, muhasebe kaydı, mutabakat ve reversal ihtiyacı ile birlikte Intertech tarafına alınmıştır. Bu ayrım doğrudur.

### 3.2 PayCore analizi kapsamı

[Analiz_2_PayCore_Kart_Yonetimi.docx](Analiz_2_PayCore_Kart_Yonetimi.docx), entegrasyon dokümanındaki kart taraflı servisleri kapsamaktadır:

- `POST /cards`
- `POST /cards/search`
- `PUT /cards/{card_token}/holder`
- `GET /cards/{card_token}/delivery`
- `GET /cards/{card_token}`
- `GET /customers/{customer_id}/cards`
- `PUT /cards/{card_token}/status`
- `POST /cards/{card_token}/replace`
- `GET /cards/{card_token}/permissions`
- `PUT /cards/{card_token}/permissions`
- `POST /cards/{card_token}/loads`
- `GET /cards/{card_token}/loads/{load_id}`
- `POST /transfers`
- `GET /transfers/{transfer_id}`
- `GET /cards/{card_token}/transactions`
- `GET /cards/{card_token}/statements`
- `GET /cards/{card_token}/statements/{statement_id}`

Bu ayrım “kart işlemleri PayCore’da” kuralıyla uyumludur.

---

## 4. Kritik bulgular

### 4.1 Bakiye yükleme akışı net değil

Entegrasyon dokümanı Bölüm 5.1’de `POST /cards/{card_token}/loads` servisi, Ödeal’in çağırdığı ve Ödeal IBAN’ından karta 7/24 yükleme başlatan dış sözleşme olarak tanımlanmıştır.

Analizlerde ise akış iki bacağa ayrılmıştır:

- Intertech: torba hesaptan para düşme ve muhasebe
- PayCore: kart bakiyesini artırma

Bu ayrım teknik olarak mantıklıdır; ancak aşağıdaki sorular yanıtsızdır:

1. Ödeal dışarıdan hangi sistemi çağıracak?
2. `POST /cards/{card_token}/loads` endpoint’ini Ödeal’e Intertech mi, PayCore mu, yoksa API Gateway mi sunacak?
3. Ödeal’e dönen `load_id`, Intertech işlem referansı mı yoksa PayCore referansı mı olacak?
4. Webhook’u PayCore doğrudan Ödeal’e mi gönderecek, yoksa Core sonucu konsolide edip mi gönderecek?
5. `GET /cards/{card_token}/loads/{load_id}` sonucu yalnızca kart bacağını mı, yoksa para + kart bacağının birleşik durumunu mu dönecek?

Mevcut haliyle Intertech analizi para bacağını tanımlıyor; ancak Ödeal’den gelecek dış çağrının request/response sözleşmesini tanımlamıyor. PayCore analizi ise aynı endpoint’i Core’dan gelen iç talimat gibi yorumluyor.

#### Önerilen model

Ödeal’e açılan tek giriş noktası Core/API Gateway tarafında olmalıdır:

1. Ödeal, `POST /cards/{card_token}/loads` çağrısını Core/API Gateway’e yapar.
2. Core aynı `Idempotency-Key` ile torba hesabı borçlandırır.
3. Core, içeride PayCore yükleme servisini çağırır.
4. PayCore kart bakiyesini artırır.
5. Sonuç Core’a döner.
6. Core para ve kart bacaklarının birleşik durumunu saklar.
7. Webhook Ödeal’e tek noktadan gönderilir.
8. PayCore başarısız olursa Core otomatik reversal yapar.

### 4.2 “7/24 servis” ile “gecelik batch” yaklaşımı çelişiyor

Entegrasyon dokümanı yüklemenin 7/24 çalışmasını istiyor. Intertech analizinde ise yüklemenin “gecelik pencerede”, 1–2 saat içinde 50.000–100.000 kayıt işleyeceği yazılmıştır.

Bu iki gereksinim birbirinin alternatifi değildir:

- API 7/24 erişilebilir olmalıdır.
- Gece 50.000–100.000 kayıt pik kapasitesi ayrıca karşılanmalıdır.
- Tekil REST çağrıları için TPS, timeout, rate limit ve retry kuralları tanımlanmalıdır.
- Gerekirse toplu yükleme için ayrı bir batch/bulk sözleşme eklenmelidir.

Analizdeki “gecelik kapasite” ifadesi, servisin yalnızca gece çalışacağı şeklinde yanlış yorumlanabilir.

### 4.3 Ürün ve limit modeli tutarsız

Entegrasyon dokümanı:

- Kartları açıkça prepaid olarak tanımlamaktadır.
- Ek A’da örnek ürün limitleri olarak `GROUP_2M = 2.000.000 TL` ve `GROUP_30M = 30.000.000 TL` vermektedir.

PayCore analizi ise:

- Kartın “1 TL limitli kredi kartı” olduğunu,
- Harcama kabiliyetinin günlük yüklenen bakiyeyle sağlanacağını,
- Kart limitinin sabit 1 TL olacağını yazmaktadır.

Intertech analizi “prepaid/e-para mı, 1 TL limitli kredi kartı mı?” sorusunu hâlâ karar noktası olarak açık bırakmıştır.

Bu üç yaklaşım aynı anda doğru olamaz. Karar şu konuları doğrudan etkiler:

- BIN ve ürün tanımı
- Kredi kartı/prepaid yasal sınıflandırması
- Muhasebe modeli
- Şema raporlaması
- Limit kontrolleri
- Ekstre formatı
- MASAK ve iç kontrol kuralları
- Müşteri sözleşmesi ve bilgilendirme metinleri

Bu karar netleşmeden PayCore’a BIN/ürün, Intertech’e muhasebe ve mutabakat geliştirmesi yaptırılmamalıdır.

### 4.4 Müşteri adı ve soyadı eksik

Entegrasyon dokümanındaki `POST /customers` request modelinde yalnızca şu alanlar bulunmaktadır:

- `national_identity_no`
- `birth_date`
- `addresses`
- `phones`

Ad ve soyad alanı yoktur.

Intertech analizinin gereksinim metninde “Ad, Soyad, TCKN, doğum tarihi...” yazmasına rağmen fonksiyonel alan tablosunda yine ad/soyad bulunmamaktadır. Dolayısıyla analiz hem kaynak dokümanın boşluğunu kapatmamış hem de kendi içinde çelişkilidir.

Bu durum ödeme sistemleri açısından kritiktir:

- Core müşteri kaydı isimsiz açılamaz.
- KYC/AML kontrolleri için yasal isim gerekir.
- Kart basımında kullanılan `name_on_card` ile Core’daki yasal isim farklılaşabilir.
- Müşteri şikâyeti, fraud ve chargeback süreçlerinde kimlik doğrulama zorlaşır.

`POST /customers` sözleşmesine en azından `first_name` ve `last_name` alanları eklenmelidir. İsim değişikliği için ayrıca müşteri güncelleme servisi düşünülmelidir. PayCore’daki `NAME_CHANGE` kart yenileme nedeni, Core’daki yasal ismi tek başına güncellemez.

### 4.5 Karttan karta transferin muhasebe ve yasal bacağı sahipsiz

`POST /transfers` kart işlemi olduğu için PayCore’a atanması doğrudur. Ancak transfer sonunda iki kartın bakiyesi değiştiği için banka bilançosundaki hesap/mutabakat etkisi de vardır.

Intertech analizi transferi tamamen kapsam dışı bırakmıştır. PayCore analizi ise muhasebeyi “torba hesap kararına göre kesinleşecek” diyerek ertelemiştir.

Bu yeterli değildir. Özellikle aşağıdaki durumlarda Intertech/GL tarafı gerekir:

- Farklı müşteri veya havuz hesapları arasında transfer
- Banka içi virman sayılacak işlemler
- MASAK açısından para transferi sayılabilecek işlemler
- Ücret/komisyon tahsilatı
- Başarısız transfer reversal’ı
- Günlük mutabakat ve GL doğrulaması

Ayrıca sunumda karttan karta transferin varsayılan olarak kapalı olduğu belirtilirken entegrasyon dokümanında transfer servisi açık ve PayCore analizinde kullanılabilir durumdadır. Varsayılan izin modelinde de ayrı bir `card_transfer` izni yoktur. Ürün kararı ve teknik izin modeli uyumlu hale getirilmelidir.

---

## 5. Orta öncelikli uyumsuzluklar

### 5.1 Standart hata modeli analizlere taşınmamış

Entegrasyon dokümanı hataların HTTP status kodu ve standart gövde ile döneceğini belirtmektedir:

```json
{
  "error": {
    "code": "CARD_NOT_FOUND",
    "message": "Belirtilen kart bulunamadı."
  }
}
```

Bu zorunluluk iki analizde de sözleşme gereksinimi olarak yer almamaktadır. İki farklı firma geliştirme yapacağı için hata kodları, HTTP status eşlemesi ve mesaj formatı merkezi olarak tanımlanmalıdır. Aksi halde Ödeal iki sistemden farklı hata formatı alır.

### 5.2 OAuth2 token servisinin sahibi belli değil

Her iki analiz de OAuth2 Client Credentials kullanılacağını yazıyor; ancak token endpoint’ini kimin sağlayacağı tanımlanmamıştır:

- Intertech mi?
- PayCore mu?
- API Management/API Gateway mi?
- Her iki firma için ayrı token mı alınacak?

Token endpoint, scope modeli, client bazlı yetki, token süresi ve sertifika yönetimi ortak mimari dokümanında tanımlanmalıdır.

### 5.3 İzin modeli ve varsayılanlar tutarsız

Kanonik izin modelinde şu alanlar vardır:

- `ecommerce`
- `mail_order`
- `international`
- `contactless`
- `atm_withdrawal`

PayCore analizi ayrıca “para gönderimi kapalı” ifadesini kullanmaktadır; ancak modelde böyle bir izin yoktur. Karttan karta transfer servisi varken bu kısıtın hangi alanla uygulanacağı belirsizdir.

Entegrasyon dokümanındaki örneklerde `international: true` ve `atm_withdrawal: true` görülürken analizlerde bunların varsayılan olarak kapalı olacağı yazılmıştır. Ürün varsayılanları ayrı bir parametre tablosunda açıkça tanımlanmalıdır.

### 5.4 Idempotency yaklaşımı iki analizde farklı seviyede

Entegrasyon dokümanı yükleme ve transfer için `Idempotency-Key` kullanımını “önerilir” olarak tanımlamaktadır.

- Intertech analizi bunu “uygulanır” seviyesine çıkarmıştır.
- PayCore analizi “önerilir” seviyesinde bırakmıştır.

Finansal işlemde bu alan zorunlu olmalıdır. Aynı idempotency anahtarı Core ve PayCore arasında uçtan uca taşınmalıdır; aksi halde para bacağı tek, kart bacağı mükerrer çalışabilir veya tersi olabilir.

Kart oluşturma ve kart yenileme servisleri için de mükerrerlik stratejisi düşünülmelidir.

### 5.5 Webhook sorumluluğu ve güvenilirliği eksik

Webhook’un kim tarafından gönderileceği iki bacaklı modelde belirsizdir. Ayrıca aşağıdaki operasyonel kurallar eksiktir:

- Başarısız callback’te retry sayısı ve aralığı
- Webhook mesaj sıralaması
- Tekrar eden webhook’ların idempotent işlenmesi
- İmza timestamp toleransı
- Replay attack koruması
- Callback secret rotasyonu
- Ödeal’in webhook alamadığında durum sorgulama yedek akışı

### 5.6 Pagination kapsamı genelleştirilmiş

Entegrasyon dokümanı cursor tabanlı sayfalamayı özellikle şu servislere uygulamaktadır:

- Kart hareketleri
- Ekstre dönem listesi
- Müşterinin kartları

Intertech analizi genel olarak “liste servislerinde cursor tabanlı sayfalama” demektedir. Bu, adres, telefon ve il/ilçe servisleri için kaynak dokümanda olmayan bir beklenti üretebilir. Pagination yalnızca ilgili PayCore servisleriyle sınırlandırılmalı veya entegrasyon dokümanı güncellenmelidir.

---

## 6. Ödeme sistemleri açısından ek boşluklar

1. **Kart yenilemede bakiye aktarımı**
   - Eski kart kapanıp yeni kart üretildiğinde bakiye nasıl taşınacak?
   - Bakiye kart bazında mı, müşteri/havuz bazında mı tutulacak?
   - `replaced_from_card_token` zinciri bakiye taşındığını garanti etmez.

2. **PIN yaşam döngüsü yok**
   - `is_pin_set` ve `last_pin_change_date` dönülmektedir.
   - PIN oluşturma, değiştirme, bloke kaldırma ve yeniden gönderim servisleri tanımlı değildir.

3. **3DS/OTP sorumluluğu yok**
   - `FULL_SECURE` e-ticaret seviyesi tanımlanmıştır.
   - 3DS sağlayıcısı, OTP gönderimi, doğrulama ve hata akışları belirtilmemiştir.

4. **Aktivasyon kontrolü zayıf**
   - `PUT /status` ile kart aktiflenebilmektedir.
   - Kartın teslim edildiği veya PIN alındığı doğrulanmadan aktivasyon yapılıp yapılmayacağı tanımlanmalıdır.

5. **Transfer işlemleri hareket kategorisinde yok**
   - Kart hareketleri `PURCHASE | WITHDRAWAL | REFUND | LOAD | FEE` kategorilerini içermektedir.
   - Karttan karta transferlerin hareketlerde nasıl görüneceği tanımsızdır.

6. **Durum geçiş matrisi eksik**
   - `PASSIVE → ACTIVE`, `BLOCKED → ACTIVE`, `LOST/STOLEN → CLOSED` gibi kurallar tanımlı değildir.
   - `LOST` ve `STOLEN` statülerinin geri alınıp alınamayacağı netleşmelidir.

7. **Core–PayCore statü senkronizasyon mekanizması yok**
   - PayCore analizi senkronizasyon ihtiyacını doğru tespit etmiştir.
   - Ancak bunun event, batch, sorgulama veya webhook ile nasıl yapılacağı yazılmamıştır.

8. **PAN güvenliği derinleştirilmeli**
   - `POST /cards/search` için PAN kullanımı doğru şekilde body içinde tasarlanmıştır.
   - Gateway loglarında maskeleme, request log saklama yasağı, tokenizasyon, retention ve PCI DSS sorumluluk sınırı yazılmalıdır.

9. **Kapanan/süresi dolan kart bakiyesi**
   - Kapalı veya süresi dolmuş karttaki bakiyenin iadesi, başka karta taşınması veya müşteriye ödenmesi tanımsızdır.

10. **Harcama, iade, fee ve takas muhasebesi kapsam dışı kalmış**
    - Intertech analizi yalnızca yükleme muhasebesini anlatmaktadır.
    - Kart harcaması, provizyon, clearing, refund, fee ve transfer için GL/mutabakat modeli ayrıca tasarlanmalıdır.

---

## 7. Düzeltilmesi gereken küçük sözleşme eksikleri

- Intertech analizinde adres ve telefon güncelleme endpoint’lerinin tam path’leri açık yazılmalıdır:
  - `PUT /customers/{customer_id}/addresses/{address_id}`
  - `PUT /customers/{customer_id}/phones/{phone_id}`

- PayCore analizinde `second_name_on_card` alanı eksik kalmıştır:
  - `POST /cards`
  - `PUT /cards/{card_token}/holder`

- Yükleme request/response alanları Intertech tarafında ayrıca tanımlanmalıdır:
  - `amount`
  - `currency`
  - `reference`
  - `load_id`
  - `status`
  - `failure_reason`
  - `available_balance`

- Her iki analizde de standart hata modeli ve örnek hata kodları eklenmelidir.

- Ortak `X-Correlation-ID`, API versiyonlama ve servis seviyesi gereksinimleri tanımlanmalıdır.

---

## 8. Önerilen düzeltme aksiyonları

| Öncelik | Aksiyon | Sorumlu önerisi |
|---|---|---|
| Kritik | Yükleme için dış API sahibini ve Core–PayCore saga akışını netleştir | Mimari + Intertech + PayCore |
| Kritik | Prepaid, e-para ve 1 TL limitli kredi kartı modeli kararını ver | Ürün + Hukuk + Ödeme Sistemleri |
| Kritik | `POST /customers` sözleşmesine yasal ad/soyad alanlarını ekle | Intertech + Ödeal |
| Kritik | Transfer muhasebesi, mutabakat ve MASAK sınıflandırmasını tanımla | Muhasebe + Uyum + Intertech + PayCore |
| Yüksek | Ortak hata modeli, hata kodu listesi ve HTTP status eşlemesini yaz | Tüm taraflar |
| Yüksek | OAuth2 token endpoint, scope ve client yetki modelini netleştir | API Gateway/Mimari |
| Yüksek | Webhook gönderici, retry, ordering, replay ve secret rotation kurallarını yaz | Intertech + PayCore |
| Yüksek | Idempotency-Key’i yükleme ve transferde zorunlu hale getir | Intertech + PayCore |
| Orta | PIN, 3DS ve aktivasyon akışlarını ayrıca tanımla | PayCore + Güvenlik |
| Orta | Kart yenileme, kapanma ve süresi dolma durumlarında bakiye kuralını yaz | PayCore + Muhasebe |
| Orta | Durum geçiş matrisi ve Core–PayCore senkronizasyon mekanizmasını tanımla | Intertech + PayCore |
| Orta | Pagination kapsamını dokümanla hizala | Analiz ekibi |

---

## 9. Sonuç

Firma ayrımı doğru kurulmuştur: müşteri ve referans servisleri Intertech’te, kart yaşam döngüsü PayCore’dadır. İki analiz birlikte entegrasyon dokümanındaki servislerin büyük kısmını karşılamaktadır.

Ancak aşağıdaki konular düzeltilmeden analizler geliştirme talebine dönüştürülmemelidir:

1. Bakiye yükleme için dış API sahibi ve Core–PayCore orkestrasyonu
2. Prepaid ile “1 TL limitli kredi kartı” modeli arasındaki çelişki
3. Müşteri adı/soyadı alanlarının eksikliği
4. Karttan karta transferin muhasebe ve yasal sorumluluğu
5. Ortak hata modeli, OAuth token sahipliği ve webhook güvenilirliği
6. Kart yenilemede bakiye taşıma, PIN, 3DS ve aktivasyon kuralları

Öncelikle ortak entegrasyon mimarisi ve yükleme saga akışı netleştirilmeli; ardından iki firma analizi bu ortak kararlara göre revize edilmelidir.
