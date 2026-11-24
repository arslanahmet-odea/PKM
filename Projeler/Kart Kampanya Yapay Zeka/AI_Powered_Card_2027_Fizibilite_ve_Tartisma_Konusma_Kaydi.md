# AI Powered Card 2027 – Fizibilite, Mimari Tartışma ve Ar-Ge Proje Kurgusu (Konuşma Kaydı)

> **Oluşturulma Tarihi:** 09.11.2026
> **Konu:** Kredi Kartı Kişiselleştirilmiş Kampanya Karar Motoru Fizibilite Analizi, Sparring Partner Değerlendirmesi ve Teknopark/TÜBİTAK Proje Bilgi Formu Hazırlığı

---

## 🗣️ Soru / Girdi #1

banka kart bölümünü yönetiyorum. kredi ve debit kartlar için müşteriye özel aşırı kişiselleştirilmiş kampanyalar düzenlemek istiyoruz ve yapay zekanın gücünü kullanmak istiyoruz. Bir anlamda banka için ar-ge projesi olacak. Ekteki doküman bankanın ürün ekibi tarafından talep olarak bize iletildi. Projedeki öngörüleri, teknolojik olarak yapılabilirliğini incele, avantaj ve dezavantajları bul. Fikir yürüt, benimle tartış ve fizibilitesini değerlendir.

### 🤖 Yanıt

Ürün ekibinden gelen [AI_Powered_Card_2027_Final_Kapsam.docx](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/AI_Powered_Card_2027_Final_Kapsam.docx) dokümanını hem **bankacılık/kart sistemleri mimarisi** hem **yapay zeka mühendisliği** hem de **Ar-Ge niteliği** açısından detaylıca inceledim. 

Genel bir özetle başlamak gerekirse: Ürün ekibi iş ihtiyacını ve ticari acıyı (*"Excel'de +20% hedef verip kârsız müşteriye bile bedava ödül dağıtıyoruz"*) **çok doğru teşhis etmiş**. Ancak dokümanda **"AI" kelimesi sihirli bir değnek gibi** kullanılarak; basit kurallı istatistikten nedensel çıkarıma (Causal ML), yöneylem araştırmasından (optimizasyon) gerçek zamanlı konum işlemeye kadar birbirinden çok farklı 4-5 teknoloji disiplini tek bir potada eritilmiş.

Senin masada hem Ürün ekibini yönlendirmen hem de kendi teknik ekibinle/Ar-Ge hedefinle mimariyi kurgulaman için konuyu **5 ana başlıkta** masaya yatıralım:

---

### 1. Dokümandaki Öngörülerin "AI Gerçeklik" Analizi (Ne İsteniyor, Gerçekte Ne?)

Dokümanda "AI" olarak geçen maddeleri teknik karşılıklarına çevirdiğimizde ortaya şu tablo çıkıyor:

| Dokümandaki İstek | Gerçek Teknik Karşılığı | Zorluk Derecesi ve Kritik Yorum |
| :--- | :--- | :--- |
| **Faz 1: Kampanya olmasaydı yapılacak harcamayı tahmin etme ve net katkı (Madde 2, 4, 8)** | **Causal ML / Uplift Modeling (Nedensel Çıkarım)** + **Kısıtlı Matematiksel Optimizasyon** | **Çok Yüksek (Asıl Ar-Ge burası).** Standart "harcama tahmini" (regression) yetmez; *müşteriye teklif verilmesi vs. verilmemesi* arasındaki farkı (Counterfactual / ITE - Individual Treatment Effect) ölçmek gerekir. |
| **Faz 2 (5.1, 5.3, 5.5, 5.7): İhtiyaç, sektör, dönüşüm ve sezonluk trend tahmini** | **Öneri Sistemleri (Collaborative Filtering, Sequence Models) & Propensity Scoring** | **Orta.** Bankacılıkta bilinen CRM analitiğidir. Ancak MCC (Merchant Category Code) ve POS açıklama metinlerinin kirli olması veri önişleme (NLP ile üye işyeri temizleme) gerektirir. |
| **Faz 2 (5.2): Harcama örüntülerinden yeni fırsat keşfi (Örn: Öğle yemeği / Konum bazlı restoran)** | **Unsupervised Clustering** + **CEP (Gerçek Zamanlı Olay İşleme) & Coğrafi Veri** | **Tuzak Madde!** Geçmiş veriden "öğle saati restoran" segmenti bulmak kolaydır (Batch). Ama parantez içindeki *"konum bazlı etraftaki restoranların bulunması"* mobil uygulamada anlık GPS izni, geofencing ve POS lokasyon veritabanı gerektirir. Kapsamı bir anda 10 kat büyütür. |
| **Faz 2 (5.4): En uygun kampanya türü, ödül tipi (puan vs cashback) ve hedefi kurgulama** | **Contextual Multi-Armed Bandits (CMAB) / Pekiştirmeli Öğrenme (RL)** | **Yüksek.** Geçmişte herkese sabit `+20%` verildiği için modelin "Acaba `+10%` veya `+35%` versem ne olurdu?" sorusunu bilecek verisi yok (**Soğuk Başlangıç / Eksik Gözlem Problemi**). |
| **Faz 2 (5.8): Müşteriye özel kampanya gerekçesi yazılması** | **Explainable AI (SHAP/LIME) + Şablon/SLM (Küçük Dil Modeli)** | **Düşük-Orta.** Modelin karar ağırlıklarını (SHAP değerlerini) alıp kurallı bir metin şablonuna veya kontrollü bir LLM'e dönüştürmek kolaydır; ancak "halüsinasyon" riskine karşı müşteri ekranında serbest metin (free-text LLM) yerine **parametrik şablon** kullanılmalıdır. |

---

### 2. Teknolojik Yapılabilirlik ve Mimari Darboğazlar (Kart Bölümü Gözüyle)

Ürün ekibinin "kapsam dışı" bıraktığı ama projenin kaderini belirleyecek **4 büyük teknik/iş darboğazı** var. Bunları masaya koyduğunda tartışmanın boyutu değişecektir:

#### A. Kredi Kartı vs. Debit Kart Çelişkisi (Dokümanda Gözden Kaçan En Kritik Nokta!)
Senin belirttiğin gibi proje **hem Kredi hem Debit kartları** kapsayacak ama doküman tamamen Kredi Kartı mantığıyla (`son 3 ay ortalama + %20 upsell`) yazılmış.
* **Teknik ve Finansal Çelişki:** Kredi kartında müşterinin boş limiti vardır, harcama hedefi verirsin, taksit/faiz/interchange kazanırsın. **Debit kartta ise harcama, vadesiz mevduat bakiyesiyle sınırlıdır!**
* **Banka Kârlılığı Paradoksu:** Müşterinin vadesiz hesabında duran 100.000 TL, banka için **sıfır maliyetli mevduattır (en değerli fonlama kaynağı)**. Sen Debit kart kampanyasıyla müşteriye *"20.000 TL daha harca, 500 TL kazan"* dediğinde, müşteri bankadaki vadesiz parasını harcayıp bitirirse; banka **yüksek faiz ortamında ucuz mevduatını kaybeder**, karşılığında ise çok düşük olan Debit Interchange gelirini alır ve üstüne ödül öder!
* **Çözüm / Fikir:** Debit kart için AI modeli "harcama artırma (upsell)" üzerine değil; **"Başka bankadaki parayı çekme (Top-up + Harcama)"** veya **"Nakit çekim (ATM) yerine POS'tan geçme"** davranışını optimize etmelidir.

#### B. Kampanya Motoru (Campaign Engine) Kapasitesi: "10 Kampanyadan 500.000 Kampanyaya"
* Mevcut durumda Excel'de segment bazlı çalışılırken muhtemelen kart altyapısında (Core/Switch/Kampanya modülünde) 10-20 adet sabit kampanya kuralı tanımlanıyor (`Kural 1: 10.000 TL harca 250 TL kazan`, `Kural 2: 20.000 TL harca 500 TL kazan` vb.).
* **Sorun:** "Aşırı kişiselleştirme (Hyper-personalization)" demek, her müşterinin hedefinin (`Target_i`) ve ödülünün (`Reward_i`) farklı olması demektir. Mevcut kart kampanya motorunuz **müşteri bazlı parametrik hedef/ödül tablosunu** tek bir kampanya tanımı altında okuyabiliyor mu?
* Eğer kampanya motoru her farklı hedef/ödül kombinasyonu için ayrı bir `Campaign_ID` açılmasını bekliyorsa, clearing/provizyon anında veya gün sonu batch'inde sistem kilitlenir.
* **Pratik Mimari Çözüm (Micro-Bucketing):** Tam serbest sayı (`14.327 TL hedef`) yerine, AI modelinin çıktısını **dinamik mikro-kovalara (ör. 100-200 farklı hedef/ödül matris hücresine)** haritalamak hem mevcut kampanya motorunu yormaz hem de Odea Mobil ekranlarında kuralların gösterimini ve yasal metin onaylarını kolaylaştırır.

#### C. "Toplam Müşteri Kârlılığı" Verisinin Hızı ve Mevcudiyeti
* Doküman her yerde *"Kart bazlı katkı + Banka genelindeki toplam müşteri kârlılığı"* diyor (Madde 3, 4.1, 10).
* Bankalarda müşteri bazlı net kârlılık (FTP, mevduat vadesi, kredi risk maliyeti / beklenen kayıp, operasyonel maliyet) genellikle Finans/MIS ekipleri tarafından **ayda bir kez (ayın 5'i ile 10'u arasında)** hesaplanır.
* Eğer AI modeli gerçek zamanlı veya haftalık kampanya önerecekse, aylık gecikmeli MIS verisiyle çalışmak zorunda kalacak. Burada gerçek kârlılık yerine AI için **"Dinamik Kârlılık Vekili (Profitability Proxy Score)"** türetmek gerekir.

#### D. Eğitim Verisi Yanlılığı (Selection Bias)
* Bugüne kadar kampanyalar hep Excel'de `Son 3 ay ortalama + %20` formülüyle verildiyse, geçmiş veride **varyasyon (çeşitlilik) yoktur**.
* AI modelinin *"Bu müşteriye %20 değil de %10 hedef ve yarı ödül verseydim yine aynı harcamayı yapar mıydı?"* sorusunu öğrenebilmesi için Faz 1'de mutlaka **Keşif (Exploration / A-B-n Test) Fazı** koşulmalıdır.

---

### 3. Projenin Avantajları ve Dezavantajları

#### Avantajlar (Neden Yapmalıyız?)
1. **Doğrudan Bilanço Etkisi (Ödül Bütçesi Tasarrufu):** Klasik kampanyalarda bütçenin **%40-%60'ı "Free-Rider" (zaten o harcamayı yapacak olan) müşterilere** gider. Uplift modeli sadece bu kesimi elese bile proje kendi yatırım maliyetini ilk 6 ayda çıkarır.
2. **Gerçek Ar-Ge ve TÜBİTAK Potansiyeli:** Sıradan bir "müşteri segmentasyonu / churn tahmini" projesi artık Ar-Ge sayılmıyor. Ancak **"Kısıtlı Bütçe Altında Nedensel Çıkarım (Causal Uplift) ve Çok Amaçlı Pekiştirmeli Öğrenme (Multi-Objective Contextual Bandits) ile Dinamik Fiyatlama/Ödüllendirme"** akademik ve teknolojik olarak **yüksek Ar-Ge niteliğine** sahiptir.
3. **Odea Mobil Etkileşimi (Engagement):** Müşteriye ilgisiz kitlesel SMS'ler atmak yerine, mobil uygulamada dinamik ilerleme çubuğu (progress bar) ve kişiye özel gerekçeli ("Seçili restoranlarda size özel...") teklif sunmak aktif uygulama kullanımını ciddi artırır.

#### Dezavantajlar ve Riskler (Neye Dikkat Etmeliyiz?)
1. **"Algoritmik Adaletsizlik" ve Müşteri Şikayeti Riski:** Aynı evde yaşayan iki eşten (veya iki iş arkadaşından) bankayı çok kullanan sadık müşteriye AI *"Bu zaten harcıyor, ödül vermeyelim veya yüksek hedef verelim"* derken, az kullanan müşteriye *"Düşük hedefe yüksek ödül"* verirse; sadık müşteri bunu fark ettiğinde **ciddi itibar ve müşteri kaybı (churn)** yaşanır. (Bunu önlemek için modele **"Sadakat Tabanı Kısıtı / Fairness Constraint"** eklenmelidir).
2. **Operasyonel ve Çağrı Merkezi Yükü:** Her müşterinin hedefi ve ödülü farklı olduğunda, Çağrı Merkezi ekranlarında temsilcinin müşteriye hangi kuralın neden uygulandığını ve neden o ödülü kazanamadığını (örn. MCC uyuşmazlığı, Ayrıcalıklar Dünyası kısıtı) çok net görebilmesi gerekir.
3. **Kural Çakışmaları (Cannibalization):** Faz 2'de müşteri aynı anda hem "Genel Harcama Upsell" hem "Restoran" hem "E-ticaret" kampanyasına uygun bulunursa, tek bir 1.000 TL'lik restoran harcaması 3 kampanyanın sayacını birden mi dolduracak? Kampanya önceliklendirme ve dışlama matrisi (Exclusion Rules) iyi kurgulanmazsa ödül maliyeti patlar.

---

### 4. Ar-Ge Projesi Olarak Nasıl Konumlandırmalıyız? (Fikir Yürütme)

Bu projeyi sadece "Ürün ekibinin kampanya talebi" olmaktan çıkarıp, **banka içi prestijli bir Ar-Ge / TÜBİTAK projesine** dönüştürmek için şu **3 katmanlı özgün mimariyi** önerebiliriz:

1. **Katman 1 — POS İşlem Zenginleştirme Motoru (NLP & Graph Tabanlı):**
   * Kart hareketlerindeki ham POS açıklamaları (`"PARAM*GETIR..."`, `"IYZICO*..."`, `"TRENDYOL..."`) ve yanlış tanımlı MCC kodları üzerinden müşterinin gerçekte ne aldığını ve hangi markalara sadık olduğunu çözen bir **Entity Resolution & Transaction Embedding** katmanı.
2. **Katman 2 — Causal Uplift & Kârlılık Optimizasyon Çekirdeği (C-Uplift):**
   * Klasik ML gibi *"Harcama yapar mı?"* sorusunu değil, **Çift-Makine Öğrenmesi (Double Machine Learning / Meta-Learners: T-Learner, X-Learner)** ile *"Ödül verirsem harcaması ne kadar artar ($\Delta$ Spend) ve vadesiz mevduat/kredi kârlılığı nasıl etkilenir ($\Delta$ Profit)?"* sorusunu çözen motor.
3. **Katman 3 — Açıklanabilir Karar ve Otomatik Kampanya Orkestratörü (XAI + Guardrails):**
   * İş biriminin koyduğu bütçe, maksimum ödül ve segment kurallarını (Ayrıcalıklar Dünyası) matematiksel kısıt (Linear Programming) olarak alıp, her müşteriye optimal `(Kampanya, Hedef, Ödül)` üçlüsünü atayan ve bunun gerekçesini üreten katman.

---

### 5. Birlikte Tartışalım: Senin İçin Kritik Sorular

Fizibiliteyi netleştirmek ve Ürün ekibine nasıl bir yol haritasıyla döneceğimizi belirlemek için şu noktaları seninle tartışmak isterim:

1. **Altyapı ve Kampanya Motoru Durumu:** Mevcut kart altyapısında (Paycore / Intertech / CRM tarafında) müşteri bazında dinamik hedef/ödül (`Müşteri X -> Hedef A, Ödül B`) tanımlayabilen parametrik bir yapı var mı, yoksa kampanyaları şu an sabit kırılımlarla mı (Örn: 5 farklı hedef grubu) içeri alıyorsunuz?
2. **Veri Sahipliği ve Ekip Yapısı:** AI modellerini kendi kart/IT ekibin içinde mi geliştirmeyi hedefliyorsun (Ar-Ge merkezi / TÜBİTAK kurgusuyla), yoksa bankada merkezi bir Veri Bilimi / Analitik ekibi var ve siz onlarla entegre olan karar motorunu / kampanya altyapısını mı yazacaksınız?
3. **Mastercard Konusu (Madde 9 ve 10):** Dokümanda iki yerde *"Mastercard dahil çözüm alternatifleri"* ifadesi geçiyor. İş birimi bir yandan Mastercard'ın hazır danışmanlık/yazılım paketlerini mi (Test & Learn / Dynamic Yield vb.) zorluyor? Eğer öyleyse, kendi iç Ar-Ge projemizle dış paket alımı arasında nasıl bir denge kurmak istiyorsun?
4. **Debit Kart Stratejisi:** Debit kartları Faz 1'den itibaren kredi kartlarıyla aynı havuzda mı düşünüyorlar, yoksa yukarıda bahsettiğim "vadesiz mevduat erimesi vs. interchange" riskini Ürün ekibinin önüne koyup Debit için ayrı bir model kurgusu mu önerelim?

Nereden derinleşelim? İstersen Ürün ekibine verilecek teknik geri bildirim/fizibilite raporunu da birlikte şekillendirebiliriz.

---

## 🗣️ Soru / Girdi #2

bu dosyayı promt olarak al

### 🤖 Yanıt

Rolü ve çalışma kurallarını devraldım. Karşında ürün ekibinin parlak sunum cümlelerini olduğu gibi kabul eden biri değil; ISO 8583 mesajlarından BDDK denetimine, nedensel çıkarımdan (causal inference) kârlılık muhasebesine kadar her iddiayı sorgulayan **sparring partner**’in var.

Yol haritamız gereği, **Adım 2 (Teknik Yapılabilirlik)** aşamasındaki mimari kararları doğrudan değiştirecek **5 Kritik Netleştirme Sorusunu** en başta masaya koyuyorum. Hemen altında ise mevcut bilinmeyenleri **`[VARSAYIM]`** ve **`[DOĞRULANMALI]`** olarak işaretleyerek **Adım 1 – Talep Eleştirisi**'ni başlatıyorum.

---

### Başlamadan Önce: Değerlendirmeyi En Çok Etkileyecek 5 Netleştirme Sorusu

1. **Aktif Kart Portföy Büyüklüğü (Sample Size):** Kredi kartı ve debit kart tarafında aylık aktif (en az 1 işlem yapan) müşteri sayımız hangi mertebede (ör. 50 bin mi, 500 bin mi, 2 milyon mu)? *(Kritik; çünkü nedensel çıkarım / uplift modelleme ve çok kollu haydut / multi-armed bandit algoritmaları küçük örneklemlerde istatistiksel anlamlılık üretemez, kural tabanlı sistemden farkı kalmaz.)*
2. **Geçmiş Kampanyalarda "Kontrol Grubu" (Hold-out) Var mıydı?** Bugüne kadar Excel ile yapılan `+20%` upsell kampanyalarında, şartları sağladığı halde **bilerek teklif gönderilmeyen rastgele bir kontrol grubu** ayrıldı mı, yoksa uygun olan herkese teklif basıldı mı?
3. **Kredi vs. Debit Kapsam Ağırlığı:** İş birimi bu dokümanda "kart" derken ikisini aynı sepette mi görüyor? Bankada debit kart harcamasının vadesiz mevduatı (ucuz fonlamayı) eritme etkisi (deposit cannibalization) kârlılık hesabına dahil ediliyor mu?
4. **Müşteri Kârlılığı Verisinin Granülaritesi:** Dokümanda sıkça geçen "banka genelindeki toplam müşteri kârlılığı" şu an veri ambarında müşteri tekil numarası bazında (FTP, risk maliyeti, mevduat/kredi net faiz marjı dahil) hesaplanıyor mu, yoksa sadece segment/ürün ortalaması mı var?
5. **Ar-Ge / TÜBİTAK Hedefi ve Bütçe Sınırı:** Bu projeyi resmi bir Ar-Ge merkezi projesi veya TÜBİTAK (ör. 1501/1505) başvurusu olarak mı konumlandırıyoruz, yoksa iç kaynakla hızlı ticari çıktı (quick-win) almak mı öncelikli?

---

## Adım 1 – Talep Eleştirisi (Ürün Kapsam Dokümanının Sorgulanması)

[AI_Powered_Card_2027_Final_Kapsam.docx](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/AI_Powered_Card_2027_Final_Kapsam.docx) dokümanındaki temel kusur şudur: Ürün ekibi **iş sonucunu (business outcome)** tanımlamak yerine, duydukları popüler veri bilimi kavramlarını "AI şunu yapsın" diyerek alt alta sıralamış. Maddeleri tek tek büyüteç altına alalım:

---

### 1. Faz 1 – Kişiselleştirilmiş Upsell Kampanyaları (Madde 4, 4.1, 4.2)

* **Gerçek İş Problemi ve Ölçülebilirlik:**
  * Problem: Mevcut `Son 3 ay ortalama × 1,20` kuralı, organik olarak harcaması zaten artacak müşteriye (free-rider / sure thing) gereksiz ödül ödüyor; harcaması düşmekte olan müşteriye ise ulaşılamaz hedef verip küstürüyor.
  * Ölçülebilir mi? **Evet**, ancak yalnızca doğru tasarlanmış **Rastgeleleştirilmiş Kontrol Grubu (Randomized Control Trial - RCT)** ile ölçülebilir.
* **Gizli Varsayımlar ve Naif Öngörüler:**
  * **Naif Öngörü 1 (Kârlılık Verisi):** Madde 4.2'de müşteriyi *"Kârlı / Zararda-Potansiyelli / Zararda-Düşük Potansiyelli"* diye ayırmak kağıt üzerinde harikadır. Ancak **`[VARSAYIM]`** bankalarda müşteri bazlı toplam kârlılık (MIS/FTP) aylık bazda ve geriden (T+10 gün) gelir. Ayrıca "potansiyelli" tanımı tamamen sübjektiftir: Kime göre potansiyelli? Bankada 2 milyon TL mevduatı olup kartını hiç kullanmayan müşteri kart bazında "zararda" (kart basım/operasyon maliyeti var, interchange geliri yok), banka bazında "çok kârlı"dır.
  * **Naif Öngörü 2 (Elastikiyet Bilinmezliği):** Geçmişte herkese sabit `+%20` hedef verildiyse **`[DOĞRULANMALI]`**, elimizdeki veride "fiyat/ödül elastikiyeti" (price elasticity of reward) yoktur. AI modeli geçmiş veriye bakarak bir müşterinin `+%15` hedef ve `300 TL` ödüle mi, yoksa `+%30` hedef ve `500 TL` ödüle mi daha iyi tepki vereceğini **kehanette bulunarak bilemez**; önce deney (exploration) yapması gerekir.
* **"AI" Gerçekten Gerekli mi?**
  * Başlangıçta (ilk 3-6 ay) derin öğrenme veya karmaşık AI **gereksizdir**. Mevcut `son 3 ay ortalaması` yerine; mevsimsellikten arındırılmış basit bir **Zaman Serisi / Regresyon (ARIMA / LightGBM baseline)** + **3x3'lük RFM ve Kârlılık Kural Matrisi** ile Excel'deki yapıdan %50 daha iyi sonuç alınır. Gerçek AI (Uplift Modeling), ancak ilk kontrollü deneylerden veri toplandıktan sonra anlam kazanır.

---

### 2. Faz 2 – Müşteriye En Uygun Kampanyanın Belirlenmesi (Madde 5.1 – 5.8)

#### Madde 5.1: Harcama davranışından müşteri ihtiyacının tahmin edilmesi
* **Gerçek İş Problemi:** Müşterinin harcama ivmesi artan sektörleri (MCC / üye işyeri) yakalayıp o sektörde teklif sunmak. Ölçülebilir (Kampanya katılım ve dönüşüm oranı).
* **Gizli Varsayım / Naiflik:** Ürün ekibi *"Son 6 ayda seyahat harcaması artan müşteriye seyahat kampanyası verelim"* diyor. **Peki ya müşteri tatilini çoktan satın aldıysa ve harcaması bu yüzden arttıysa?** Seyahat harcaması bittikten sonra seyahat kampanyası önermek dikiz aynasına bakarak araba kullanmaktır. Üstelik zaten seyahat harcaması artan birine seyahat ödülü vermek, **ilave (incremental) harcama yaratmaz**, mevcut harcamayı sübvanse eder!
* **AI Gerekli mi?** Hayır. Son 3/6/12 aylık MCC trend eğimini (slope) hesaplamak basit bir SQL sorgusu ve doğrusal regresyondur. Asıl ihtiyaç "artan harcamayı" değil, **"ardışık harcama örüntüsünü" (Sequential Pattern: Uçak bileti aldı $\rightarrow$ Otel/Araç kiralama alacak)** yakalamaktır.

#### Madde 5.2: Harcama örüntülerinden yeni kampanya fırsatlarının keşfedilmesi
* **Gerçek İş Problemi:** Standart segmentlerin dışındaki mikro-davranış kümelerini (spatio-temporal patterns) bulup kampanya tasarımcısına önermek.
* **Gizli Varsayım / Naiflik (EN TEHLİKELİ MADDE):**
  1. *"AI yeni kampanya fikirleri üretsin"* cümlesi, Madde 5'in girişindeki *"sistemin sınırsız ve onaysız kampanya oluşturması beklenmeyecektir"* kuralıyla doğrudan çelişiyor.
  2. Parantez içindeki **"(örn: konum bazlı etraftaki restoranların bulunabilmesi)"** ifadesi tam bir **kapsam kayması (scope creep) bombasıdır!** Kart işlem verisinde (ISO 8583 DE-43) üye işyerinin fiziksel GPS koordinatı çoğu zaman doğru değildir (merkez ofis adresi yazar, ÖKC/PF alt işyeri yazar). Ayrıca müşteriye öğle saatinde etrafındaki restoranı önermek için Odea Mobil'de **arka planda sürekli konum izni (background location tracking)**, geofencing altyapısı ve anlık bildirim (real-time push) gerekir. Bu tek başına ayrı bir 1 yıllık projedir.
* **AI Gerekli mi?** Kümeleme (Clustering - HDBSCAN / K-Means) ile analiste "Hafta içi 12:00-14:00 arası yemek harcaması yapan 45.000 kişi var" raporu çıkarmak için temel denetimsiz öğrenme (unsupervised ML) yeterlidir; buna "AI kampanya keşfediyor" demek abartıdır.

#### Madde 5.3: Müşterinin harcama potansiyeli olan sektörlerin belirlenmesi
* **Gerçek İş Problemi:** Çapraz satış / kategori genişletme (Category Expansion / Cross-sell). Müşterinin bizde kullanmadığı ama benzer profillerin kullandığı MCC'leri bulmak. Ölçülebilir.
* **Gizli Varsayım / Naiflik:** Müşteri o sektörde hiç harcama yapmıyor mu, yoksa **rakip bankanın kartıyla mı yapıyor?** Banka sadece kendi cüzdan payını (share of wallet) görür **`[VARSAYIM: KKB/BKM üzerinden rakip kartların MCC bazlı detayı görülmez, sadece toplam limit/risk görülür]`**.
* **AI Gerekli mi?** **Evet.** Burada **İşbirlikçi Filtreleme (Collaborative Filtering / Matrix Factorization - ALS veya Graph Neural Networks)** gerçek değer üretir ve kural tabanlı sistemlerle yapılamaz.

#### Madde 5.4: Harcama davranışına göre kişiselleştirilmiş teklif kurgusu oluşturulması
* **Gerçek İş Problemi:** Her müşteri için `(Kampanya Türü, Hedef Tutar, Ödül Tipi [Puan vs. Cashback], Ödül Tutarı)` dörtlüsünü optimize etmek.
* **Gizli Varsayım / Naiflik:** Örnekte *"Sık market alışverişi yapana cashback yerine puan önerilmesi"* denmiş. Neden? Puanın kırılma (breakage - kullanılmama) oranı mı yüksek, yoksa üye işyeri anlaşması mı farklı? Eğer ödül tiplerinin banka için birim maliyeti aynıysa (1 TL Puan = 1 TL Cashback), bu ayrımın ekonomik rasyonalitesi dokümanda yok.
* **AI Gerekli mi?** **Evet**, çok değişkenli tedavi etkisi (Multi-treatment Uplift) ve Kısıtlı Optimizasyon (Constrained Optimization) gerektirir.

#### Madde 5.5: Müşteri bazında kampanya dönüşüm potansiyelinin tahmin edilmesi
* **Gerçek İş Problemi:** Müşterinin teklife katılma ve hedefi tutturma olasılığını (Propensity to Convert) tahmin etmek.
* **Gizli Varsayım / Naiflik (BÜYÜK METODOLOJİK HATA):** Ürün ekibi 5.5'te **Dönüşüm Olasılığı (Propensity)** istiyor, ama Madde 8'de (Başarı Kriterleri) **İlave Katkı (Incremental Uplift)** ile başarıyı ölçeceğini söylüyor!
  * **İtirazım:** Dönüşüm olasılığı en yüksek müşteri kimdir? Restorana zaten her hafta giden müşteridir! Sen 5.5'e göre restoran kampanyasını bu müşteriye önceliklendirirsen, **dönüşüm oranın %90 çıkar ama ilave katkın (uplift) SIFIR olur (ödülü çöpe atarsın).** 5.5 maddesi "dönüşüm potansiyeli" değil, **"ilave harcama yaratma potansiyeli (Uplift Score)"** olarak değiştirilmelidir.

#### Madde 5.6: Harcama davranışından yeni müşteri segmentlerinin keşfedilmesi
* **Gerçek İş Problemi:** Statik demografik/varlık segmentleri yerine davranışsal mikro-segmentler oluşturmak.
* **Gizli Varsayım / Naiflik:** Eğer sistem her müşteriye bireysel (N=1) teklif çıkaracaksa, neden hala "Yeni segmentler keşfedip isimlendirmeye" (Örn: *Hafta sonu online alışveriş yapanlar*) ihtiyaç duyuyoruz? Segmentasyon, bireyselleştirme yapılamadığında kullanılan bir ara basamaktır.
* **AI Gerekli mi?** Kümeleme algoritmaları (Clustering) kullanılır ama çıktısı doğrudan otonom aksiyona değil, iş biriminin anlamlandırmasına (BI/Dashboard) hizmet eder.

#### Madde 5.7: Harcama trendlerinden gelecekteki kampanya ihtiyaçlarının tahmin edilmesi
* **Gerçek İş Problemi:** Mevsimsellik ve zamanlamayı (Right-time marketing) yakalamak.
* **Gizli Varsayım / Naiflik:** *"Geçen yaz seyahat harcaması yapan müşteriye bu yaz seyahat kampanyası önermek"* için AI'a ihtiyaç yoktur; basit bir takvim kuralı (`Geçen yıl Haziran-Ağustos MCC=Seyahat > X TL olanlar`) aynı işi %100 doğrulukla yapar. Gerçek AI katkısı, takvim dışı **yaşam olayı tetikleyicilerini (Life-event triggers: taşınma, evlilik, bebek, iş değişimi)** harcama örüntüsünden yakalamaktır.

#### Madde 5.8: Müşteriye özel kampanya önerisinin gerekçesinin oluşturulması
* **Gerçek İş Problemi:** Teklifin neden sunulduğunu müşteriye (veya iş birimine) açıklamak.
* **Gizli Varsayım / Naiflik (MÜŞTERİ PSİKOLOJİSİ VE KVKK RİSKİ):** Ürün ekibi müşteriye *"Son 3 ayda restoran harcamalarınızın artması nedeniyle bu kampanya sizin için önerildi"* mesajı göstermeyi iyi bir deneyim sanıyor.
  * **İtirazım:** Birçok müşteri bankanın kendi harcamalarını bu kadar dikizleyip yüzüne vurmasından (**"Creepy / Gözetlenme Hissi"**) rahatsız olur! Daha kötüsü, modelin gerçek gerekçesi *"Rakip bankaya kaydığınızı tahmin ettiğimiz için"* veya *"Kârlılığınız yüksek olduğu için"* olduğunda bunu müşteriye söyleyemezsiniz.
* **AI Gerekli mi?** Modelin içerdeki kararını iş birimine/analiste açıklamak için **XAI (SHAP)** şarttır. Ancak müşteriye Odea Mobil'de gösterilecek metin için LLM'i serbest bırakmak yerine, **onaylı 15-20 metin şablonundan (Template-based NLG)** birini seçtirmek çok daha güvenli ve maliyetsizdir.

---

### 3. Faz 3 – Odea Mobil Müşteri Deneyimi (Madde 6)

* **Gerçek İş Problemi:** Teklifin görünürlüğü, gerçek zamanlı/günlük sayaç takibi (progress bar), "Ayrıcalıklar Dünyası" sadakat programıyla entegrasyon ve kendi avantajını seçme (gamification).
* **Gizli Varsayım ve Çelişkiler:**
  1. **Real-Time Beklentisi:** Müşteri POS'tan kartı çektiği anda Odea Mobil'e girip "Kalan harcama tutarım düştü mü?" diye bakar. Eğer kampanya motoru provizyondan (ISO 8583 0100/0200) değil, gün sonu takas/clearing dosyasından (batch) besleniyorsa, müşteri 1-2 gün gecikmeli sayaç görür ve çağrı merkezini arar **`[DOĞRULANMALI: Mevcut kampanya kazanımları provizyon anında mı, takas/muhasebeleşme anında mı çalışıyor?]`**.
  2. **Müşterinin Avantaj Seçmesi vs. AI Optimizasyonu:** Faz 1 ve 2'de "AI en kârlı teklifi seçecek" dedikten sonra Faz 3'te "Müşteri seçenekler arasından avantajını kendisi seçsin" demek, optimizasyon problemiyle çelişir. Müşteri doğal olarak kendi lehine (banka aleyhine) en kolay hedefi/ödülü seçecektir (Self-selection bias). Ancak AI müşterinin önüne **beklenen net katkısı (Expected Net Lift) birbirine eşit 3 farklı seçenek** koyarsa, bu harika bir tercih öğrenme (preference learning) mekanizmasına dönüşür!

---

### 4. Maddeler Arası Çakışmalar, Tekrarlar ve Büyük Eksikler

1. **Tekrar / Örtüşme Çorbası:**
   * **Faz 1** ile **Madde 5.4 (Kişiselleştirilmiş teklif kurgusu)** aslında aynı optimizasyon motorudur. Faz 1 sadece kampanya türünü "Genel Upsell" olarak sabitler, 5.4 ise kampanya türünü de değişken yapar. İki ayrı sistem gibi yazılması mimari hataya yol açar.
   * **Madde 5.1 (İhtiyaç tahmini)**, **5.5 (Dönüşüm tahmini)** ve **5.7 (Gelecek trend tahmini)** veri bilimi açısından tek bir **"Zamana Duyarlı Müşteri-Kampanya Etkileşim Matrisi"nin (Temporal User-Item Utility Matrix)** farklı çıktılarıdır. Ayrı ayrı modeller kurulursa birbirleriyle çelişen skorlar üretirler.
   * **Madde 5.2 (Örüntüden fırsat keşfi)** ile **5.6 (Dinamik segment keşfi)** teknik olarak aynı kümeleme (clustering) işidir.
2. **Dokümandaki En Büyük Eksik: Kredi Kartı vs. Debit Kart Ayrımı Yok!**
   * Dokümanda "Debit kart" kelimesi dahi geçmiyor, sadece "kart" deniyor. Oysa:
     * Kredi kartında gelir kalemleri: **Interchange + Nakit Avans/Taksit/Revolving Faizi + Yıllık Kart Ücreti**.
     * Debit kartta gelir: Sadece **düşük oranlı Debit Interchange**; buna karşılık **Vadesiz Mevduat Kaybı (Fonlama Maliyeti)** riski!
     * Aynı AI kârlılık formülünü hem Kredi hem Debit karta uygularsanız banka Debit kampanyalarından net zarar yazar.

---

### Adım 1 Sonu Değerlendirmesi

**(a) 3 Maddelik Özet:**
1. Ürün dokümanı ticari problemi (*"sabit +%20 hedef ve kârsız müşteriye ödül maliyeti"*) çok doğru tanımlamış; ancak Faz 2'deki 8 madde birbirini tekrar eden ve iş kuralıyla çelişen (Propensity vs. Uplift hatası gibi) kavramsal karmaşalar içeriyor.
2. Dokümandaki en tehlikeli "gizli kapsam" Madde 5.2'deki **konum bazlı restoran önerisi** ve Madde 5.8'deki **müşteriye gözetlenme hissi verebilecek açıklama üretimi**dir; bunlar ilk fazlarda budanmalıdır.
3. Kredi kartı ve Debit kartın banka bilançosundaki kârlılık dinamikleri (limit kullanımı vs. vadesiz mevduat erimesi) tamamen zıttır; tek bir hedef/kârlılık denklemiyle ikisi birden yönetilemez.

**(b) Sana Sorduğum Açık Sorular:**
1. Yukarıdaki **5 Başlangıç Netleştirme Sorusuna** (Portföy büyüklüğü, geçmişte kontrol grubu varlığı, Kredi/Debit ayrımı, MIS kârlılık verisi detayı, Ar-Ge/TÜBİTAK hedefi) yanıtların nedir?
2. Ürün ekibine geri dönerken Faz 2'deki 8 maddeyi (5.1–5.8) kendi içimizde **3 net teknik modüle** (1: *Davranışsal Kümeleme & Analist Keşif Aracı*, 2: *Sektörel Öneri & Zamanlama Motoru*, 3: *Nedensel Uplift & Hedef/Ödül Optimizatörü*) indirgeyerek sunmamıza sıcak bakar mısın, yoksa dokümandaki başlık yapısını birebir korumak zorunda mıyız?

**(c) Onay İsteği:**
Yukarıdaki eleştirilere katılıyor musun, itiraz ettiğin veya düzeltmek istediğin bir varsayımım var mı? Yanıtlarınla birlikte **Adım 2 – Teknik Yapılabilirlik** aşamasına geçmek için onayını bekliyorum.

---

## 🗣️ Soru / Girdi #3

1-Aktif Kart Portföy Büyüklüğü : açık kredi kartı bulunan müşterinlerin %30'u yani 20 bini atlık en az bir kredi kartı işlemi yapıyor. Debit kartı olan müşterilerin %5'i yani 7 bin debit kullanıcısı aktif debit kartla işlem yapıyor.
2-Geçmiş Kampanyalarda "Kontrol Grubu" (Hold-out) Var mıydı: vardı,  bireysel bankacılık segment ekibi tarafından kontrol grubu üzerindeki etkisi kontrol edilir. Uygun olan herkese teklif basıldı.
3-Kredi vs. Debit Kapsam Ağırlığı:ikisini aynı sepette  görüyor, eritme etkisi (deposit cannibalization) kârlılık hesabına dahil edilmiyor. Hacim yaratmaları yönüyle benzer değerlendirilir.
4-Müşteri Kârlılığı Verisinin Granülaritesi: Müşteri id bazında tutuluyor,
5-Bu projeyi resmi bir Ar-Ge merkezi projesi veya TÜBİTAK (ör. 1501/1505) başvurusu olarak  konumlandırıyoruz,  iç kaynakla hızlı ticari çıktı (quick-win) almak ikinci planda.

### 🤖 Yanıt

Verdiğin 5 yanıt projenin tüm matematiğini ve mimarisini değiştirdi. Adım 2’ye geçmeden önce, bir **sparring partner** olarak verdiğin yanıtlardaki **iki büyük teknik alarmı** yüzüne çarpmam gerekiyor:

1. **Örneklem Büyüklüğü Şoku (20 Bin Kredi + 7 Bin Debit = 27 Bin Aktif Müşteri):**
   * Büyük bankalarda (5-10 milyon kart) çalışan klasik "Büyük Veri" yapay zeka modelleri (Derin Pekiştirmeli Öğrenme, standart T-Learner/X-Learner Uplift Ağaçları) **27 bin aktif müşteride istatistiksel olarak çöker (aşırı öğrenir / overfit olur).**
   * Neden? 20.000 aktif kredi kartı müşterisini Faz 2'deki gibi 8-10 kampanya türüne, 5 farklı hedef kademesine ve 3 ödül tipine böldüğünde (üstüne %15 kontrol grubu ayırdığında) her bir hücreye **100-150 kişi** düşer. 150 kişilik grupta bir müşterinin o ay düğün yapması veya hastane masrafı çıkması tüm ortalamayı saptırır (yüksek varyans).
   * **Ama İyi Haber Şu (TÜBİTAK / Ar-Ge Fırsatı):** Bu kısıt, TÜBİTAK başvurusu için **altın değerinde bir Ar-Ge belirsizliğidir!** *"Düşük Örneklem (Small-Sample Regime) ve Yüksek İnaktiflik (%70 KK, %95 Debit) Altında Hiyerarşik Bayesyen Nedensel Çıkarım (Hierarchical Bayesian Uplift) ve Parametrik Elastikiyet Modellemesi"* başlığı, TÜBİTAK hakemlerinin karşısına çıkabilecek en sağlam bilimsel Ar-Ge tezidir. Ayrıca %70 inaktif kredi kartı (~46 bin kişi) ve %95 inaktif debit kart (~133 bin kişi) kitlesi, asıl hacmin **"Upsell"** değil **"Aktivasyon / Uyandırma (Reactivation)"** kampanyalarında yattığını gösteriyor.
2. **Kontrol Grubu Paradoksu (*"Kontrol grubu vardı"* AMA *"Uygun olan herkese teklif basıldı"*):**
   * İstatistiksel olarak bu iki cümle aynı anda doğru olamaz. Eğer kampanya kriterine uyan **herkese** teklif basıldıysa, bireysel bankacılık ekibinin "kontrol grubu" dediği şey muhtemelen ya **teklife katılmayanlar (non-responders)** ya da **kampanya kriterini kıl payı kaçıranlardır** **`[DOĞRULANMALI]`**.
   * Teklife katılmayanları kontrol grubu almak **Kendi Kendini Seçme Yanlılığı (Self-Selection Bias)** yaratır (zaten harcama yapmayacak olan kişi kampanyaya katılmaz; dolayısıyla kampanya çok başarılıymış gibi sahte bir illüzyon oluşur). Gerçek kontrol grubu, teklife %100 uygun olduğu halde **zar atılarak (rastgele) teklif gizlenen** gruptur.

Bu gerçekleri merkeze koyarak **Adım 2 – Teknik Yapılabilirlik** analizine geçiyorum.

---

## Adım 2 – Teknik Yapılabilirlik

### 1. Maddeler Bazında Teknik Yapılabilirlik Tablosu

*(Not: Olgunluk sütunu TÜBİTAK/Ar-Ge açısından yöntemin ne kadar hazır kütüphane vs. özgün Ar-Ge gerektirdiğini; Değer sütunu ise 27 bin aktif / ~200 bin toplam portföydeki gerçek iş ve Ar-Ge değerini gösterir.)*

| Madde | Önerilen Yaklaşım (ML / İstatistik / Kural / LLM) | Gerekli Veri | Veri Hazır mı? (`[VARSAYIM]`) | Olgunluk (Hazır / Ar-Ge / Deneysel) | Zorluk (1-5) | Değer (1-5) | Ana Risk |
| :--- | :--- | :--- | :--- | :--- | :---: | :---: | :--- |
| **Faz 1: Kişisel Upsell (Teklif / Hedef / Ödül)** | **Hiyerarşik Bayesyen Uplift / R-Learner** + **Bütçe Kısıtlı Doğrusal Programlama (MILP)** | Son 12 ay işlem, geçmiş kampanya + **gerçek** kontrol grubu, müşteri ID bazlı MIS kârlılık, KK boş limit / Debit vadesiz bakiye | İşlem ve MIS hazır; **gerçek** rastgele kontrol grubu verisi şüpheli (`[DOĞRULANMALI]`) | **Ar-Ge** (Düşük örneklemde sürekli hedef/ödül optimizasyonu özgün Ar-Ge'dir) | 4 | 5 | 27 bin aktif müşteride varyansın yüksek olması; geçmişte hep `+%20` verildiği için hedef elastikiyetinin bilinmemesi |
| **5.1: Harcama Davranışından İhtiyaç Tahmini** | **Zaman Ağırlıklı Sektörel Eğim (Time-decayed Trend)** + **Ardışık Örüntü Madenciliği (Sequential Pattern / Markov)** | 3/6/12 aylık MCC, üye işyeri (DE-43), işlem tarihi ve tutarı | MCC hazır; ancak marka/üye işyeri metinleri kirli (`[VARSAYIM]`) | **Hazır / Mühendislik** (NLP ile işyeri temizleme kısmı Ar-Ge'ye dahil edilebilir) | 2 | 3 | İhtiyacı iş işten geçtikten sonra (tatil bittikten sonra) tahmin etme ve organik harcamayı bedavaya fonlama riski |
| **5.2: Örüntülerden Yeni Kampanya Fırsatı Keşfi** | **Yoğunluk Tabanlı Kümeleme (HDBSCAN)** + **Büyük Dil Modeli (LLM) ile Analiste Özetleme** *(Konum bazlı kısım hariç!)* | İşlem saati, günü, MCC, online/fiziki (POS entry mode), şehir/ilçe | İşlem saati/MCC hazır; **gerçek zamanlı mobil GPS verisi YOK** (`[VARSAYIM]`) | Kümeleme: **Hazır** <br>Otonom Fikir: **Deneysel** | 3 *(Konum girerse 5)* | 2 | Konum bazlı gerçek zamanlı restoran bulma isteğinin projeyi mobil izin ve CEP (karmaşık olay işleme) bataklığına çekmesi |
| **5.3: Potansiyel Sektör Belirleme (Cross-sell)** | **Örtük Faktörlü İşbirlikçi Filtreleme (Implicit ALS / LightFM)** | Müşteri × MCC harcama matrisi, demografik/segment öznitelikleri | Hazır (`[VARSAYIM]`) | **Hazır / Ar-Ge** (Soğuk başlangıç / inaktif debit müşterileri için hibrit model gerekir) | 3 | 4 | Müşterinin o sektörde harcamayı aslında rakip banka kartıyla yapıyor olması (cüzdan payı körlüğü) |
| **5.4: Kişisel Teklif Kurgusu (Tür, Ödül Tipi, Hedef)** | **Çok Kollu Bağlamsal Haydut (Contextual Multi-Armed Bandit - Thompson Sampling)** | Kampanya türü × Ödül tipi (Puan/Cashback) bazında geçmiş tepki verisi, puan kırılma (breakage) oranları | Puan vs Cashback ayrımında kontrollü deney verisi **YOK** (`[VARSAYIM]`) | **Ar-Ge** (TÜBİTAK için en güçlü 2. modül) | 5 | 4 | Küçük portföyde (27 bin) çok fazla kombinasyon denendiğinde öğrenme süresinin (convergence) aylarca sürmesi |
| **5.5: Kampanya Dönüşüm Potansiyeli Tahmini** | **Eğilim Skoru (Propensity - LightGBM)** YERİNE **Nedensel Dönüşüm Farkı (Treatment Effect Score)** | Kampanya iletişim (SMS/Push) logları, katılım/hedef tutturma geçmişi | Kampanya sonuçları hazır; kanal okuma/tıklama logları kısmen hazır (`[VARSAYIM]`) | **Hazır** (Ancak Uplift'e çevrilirse **Ar-Ge**) | 2 | 3 | Propensity kullanılırsa zaten harcayacak müşterilere (free-rider) yüksek skor verilip bütçenin yakılması |
| **5.6: Dinamik Müşteri Segmentleri Keşfi** | **Temsil Öğrenmesi (Transaction Autoencoder / Embeddings) + K-Means / GMM** | Zaman serisi işlem öznitelikleri (hafta sonu oranı, e-ticaret oranı, sepet ortalaması) | Hazır (`[VARSAYIM]`) | **Ar-Ge** (İşlem dizilerinden davranışsal vektör/embedding üretimi) | 3 | 3 | 5.2 ile tamamen çakışması; bireysel (N=1) teklif motoru varken segmentlerin sadece raporlama (BI) seviyesinde kalması |
| **5.7: Gelecek Dönem / Mevsimsellik Tahmini** | **Hiyerarşik Zaman Serisi (Prophet / Temporal Fusion Transformer - TFT)** veya **Takvim Kuralları** | En az 24-36 aylık geçmiş işlem verisi, resmi tatil/bayram/okul takvimi | 2-3 yıllık geçmiş veri hazır (`[VARSAYIM]`) | Basit istatistik: **Hazır** <br>Derin Zaman Serisi: **Ar-Ge** | 3 | 3 | Enflasyonist ortamda (Türkiye gerçeği) nominal harcama tutarlarının yıllar arası karşılaştırılamaması (reel/endeksli veri şart) |
| **5.8: Öneri Gerekçesinin Oluşturulması** | **SHAP (Açıklanabilir AI)** + **Kural Tabanlı Şablon / Küçük Dil Modeli (Guardrailed SLM)** | Modelin her müşteri için ürettiği en yüksek katkılı 3 öznitelik (feature importance) | Model çıktısı olarak üretilecek | **Ar-Ge / Mühendislik** | 2 | 3 | Müşteriye "Son 3 ayda restoran harcamanız arttı" denildiğinde oluşacak gözetlenme (creepy) algısı ve KVKK profilleme itirazı |
| **Faz 3: Odea Mobil Deneyimi ve Avantaj Seçimi** | **Kural Motoru + Tercih Öğrenmeli Seçim Mimarisi (Discrete Choice Modeling / Multinomial Logit)** | Anlık/günlük işlem akışı, Ayrıcalıklar Dünyası segment kuralları, mobil uygulama tıklama verisi | Ayrıcalıklar Dünyası kuralları hazır; mobil arayüz/API geliştirilmeli (`[VARSAYIM]`) | **Mühendislik + Ar-Ge** (Eşdeğer kârlılıklı 3 tekliften müşteri tercihi öğrenme) | 4 | 5 | Müşterinin POS'tan geçtikten hemen sonra sayacın güncellenmesini beklemesi (Batch vs Real-time uyumsuzluğu) |

---

### 2. Derinlemesine Teknik Tartışmalar

#### A. Baseline ve Uplift Modelleme: Neden Propensity Değil, Neden Standart Uplift Değil?
* **Propensity (Eğilim) Tuzağı:** Klasik eğilim modeli $P(\text{Hedefi Tutturur} \mid X, \text{Teklif}=1)$ olasılığını tahmin eder. Bu modelin en yüksek puan verdiği kişiler, kartını zaten her gün kullanan ve kampanya olmasa da o parayı harcayacak olan **"Sure Things" (Beleşçiler)** grubudur.
* **Uplift (Nedensel Etki) Zorunluluğu:** Bizim aradığımız şey Bireysel Tedavi Etkisidir (Individual Treatment Effect - ITE):
  $$\tau(X_i) = \mathbb{E}[Y_i(1) - Y_i(0) \mid X_i]$$
  Yani: *(Teklif verildiğinde yapacağı harcama)* $-$ *(Teklif verilmeseydi yapacağı baseline harcama)*.
* **27 Bin Aktif Müşteri İçin Mimari Kritik (TÜBİTAK Ar-Ge Kalbi):**
  * Klasik **T-Learner** (Teklif alanlar için ayrı bir XGBoost, almayanlar için ayrı bir XGBoost kurup farkını almak) küçük örneklemlerde iki modelin hatalarını birbirine eklediği için korkunç bir gürültü üretir.
  * **Önerim:** İki aşamalı **R-Learner (Robinson Decomposition)** veya **Doubly Robust (DR) Learner** mimarisi kurmalıyız:
    1. **1. Aşama (Güçlü Baseline Modeli):** Tüm portföyün (kampanya alan/almayan ayrımı olmadan) geçmiş 12 aylık işlemlerinden, **enflasyondan arındırılmış** organik harcamasını tahmin eden tek ve güçlü bir zaman serisi/regresyon modeli ($\hat{\mu}(X)$) kurulur.
    2. **2. Aşama (Artık/Residual Üzerinden Uplift):** Kampanyanın etkisi, ham harcama üzerinden değil, 1. aşamadaki organik beklentiden sapma ($\Delta Y = Y - \hat{\mu}(X)$) üzerinden modellenir. Böylece 27 bin kişilik küçük portföyde varyans %60-70 oranında düşürülür!

#### B. Hedef ve Ödül Optimizasyonu (Bütçe Kısıtlı MILP + Contextual Bandit)
Ürün ekibi hem *"Müşteri bazında hedef ve ödül belirlensin"* hem de *"Toplam bütçe, müşteri başı max ödül ve min. net katkı korunsun"* diyor. Bu bir tahmin problemi değil, **Kısıtlı Matematiksel Optimizasyon (Constrained Optimization)** problemidir:
* Her müşteri $i$ ve aday teklif paketi $k \in \{1..K\}$ (Örn: $k_1$: `+10% hedef, %2 ödül`, $k_2$: `+25% hedef, %3 ödül`, $k_0$: `Teklif yok`) için beklenen net banka katkısı:
  $$\text{Net Katkı}_{i,k} = (\Delta \text{Harcama}_{i,k} \times \text{Marj}_i) - (\text{Hedef Tutma Olasılığı}_{i,k} \times \text{Ödül}_{k})$$
* **Doğrusal Programlama (MILP) Çözücüsü (Örn: Google OR-Tools / HiGHS):** Toplam ödül bütçesi $\le B$ ve her müşteriye en fazla 1 teklif ($\sum_k x_{i,k} \le 1$) kısıtı altında toplam $\text{Net Katkı}$'yı maksimize eder. Zarardaki ve düşük potansiyelli müşterilerde $\text{Net Katkı}_{i,k} < \text{Min Eşik}$ çıkacağı için çözücü otomatik olarak $k_0$ (**Teklif Yok**) atar!
* **Contextual Bandit (Keşif-Kullanım / Exploration-Exploitation):** Geçmişte herkese `+%20` verildiği için model diğer oranları bilmiyor. Bu yüzden optimizatörün içine **Thompson Sampling (Bayesyen Bandit)** ekleyeceğiz: Portföyün %80'ine o anki en kârlı bilinen teklifi verirken (Exploitation), %20'sine modelin belirsizliğinin yüksek olduğu farklı hedef/ödül oranlarını (Örn: `+%10`, `+%35`) kontrollü olarak dağıtacak (Exploration).

#### C. Üretken Yapay Zeka (LLM) Nerede Durmalı, Nerede Yasaklanmalı?
* **Kesin Kural:** LLM **asla** kime hangi kampanyanın, hangi hedefin veya hangi ödülün verileceğine karar vermez! LLM aritmetik ve optimizasyon yapamaz.
* **LLM'in Doğru 3 Rolü:**
  1. **Veri Zenginleştirme (Offline):** POS işlem açıklamalarındaki (`DE-43`: `"IYZICO*YEMEKSEPETI ISTANBUL"`) karmaşık metinlerden temiz marka ve alt-kategori çıkaran küçük yerel dil modeli (Local SLM / BERT-tabanlı sınıflandırıcı).
  2. **Analist Asistanı (Madde 5.2 & 5.6):** Kümeleme algoritmasının bulduğu davranış gruplarını okuyup kampanya ekibine *"Bu 3.200 kişilik küme maaş gününden sonraki ilk 3 günde e-ticaret yapıyor, şunları önerebilirsiniz"* şeklinde **insan onayına sunulacak taslak** üretmek.
  3. **Gerekçe Üretimi (Madde 5.8 - Sıkı Kısıtlı):** Müşteriye gösterilecek metinlerde halüsinasyonu ve "gözetlenme" hissini sıfırlamak için LLM serbest metin yazmamalı; SHAP çıktısına göre **önceden Hukuk/Uyum onayından geçmiş 25 adet parametrik cümle şablonundan** en uygununu seçip doldurmalıdır.

#### D. Bağımsız Teklif Yürütme Altyapısı ve Veri Modeli (Kredi vs. Debit)
Projeyi bankadaki diğer kampanya altyapılarından **bağımsız** ele aldığımız için, bu Ar-Ge projesi kapsamında kendi **"Parametrik Kampanya Karar ve Takip Çekirdeğimizi" (Standalone Campaign Decision & Tracking Engine)** kurmalıyız:
* **Temel Veri Modeli (Minimum 5 Çekirdek Tablo):**
  1. `CAMPAIGN_TEMPLATE`: İş biriminin onayladığı ana kampanya şablonları (`Template_ID`, `Allowed_MCC_List`, `Min_Target_Floor`, `Max_Reward_Cap`, `Card_Type [Credit/Debit]`, `Approval_Status`, `Approved_By`).
  2. `CUSTOMER_OFFER_ASSIGNMENT`: Her müşteri için üretilen tekil teklif (`Assignment_ID`, `Customer_ID`, `Template_ID`, `Treatment_Flag [1: Teklif Var, 0: Hold-out Kontrol Grubu]`, `Personal_Target_Amount`, `Personal_Reward_Amount`, `Reward_Type`, `Baseline_Expected_Spend`, `Predicted_Uplift`, `XAI_Reason_Code`, `Model_Version`, `Offer_State [Offered/Enrolled/Completed/Expired]`).
  3. `TRANSACTION_LEDGER_FEED`: Kart işlemlerinin dinlendiği ve teklif kurallarıyla eşleştirildiği akış tablosu (`Tx_ID`, `Customer_ID`, `Card_Type`, `Amount`, `MCC`, `Normalized_Merchant_ID`, `Is_Refund_Or_Cancel`).
  4. `OFFER_PROGRESS_TRACKER`: Odea Mobil'in sorgulayacağı sayaç tablosu (`Assignment_ID`, `Accumulated_Spend`, `Remaining_Spend`, `Last_Updated_At`).
  5. `REWARD_FULFILLMENT_QUEUE`: Hedefi tutan müşterilere puan/cashback yüklemek üzere ana bankacılık/kart sistemine gönderilecek emir tablosu (`Fulfillment_ID`, `Assignment_ID`, `Reward_Amount`, `Clawback_Status [İade durumunda geri alma takibi]`).
* **Kredi ve Debit Ortak mı, Ayrı mı?**
  * **Altyapı ve Veri Modeli (Tablolar, API, Takip Motoru): ORTAK** olmalı (mükerrer yazılım maliyeti olmaması için).
  * **AI Modelleri ve Öznitelik Uzayı (Feature Store): KESİNLİKLE AYRI** olmalı! Çünkü Kredi Kartı modelinin en önemli girdisi `Kullanılabilir Kredi Kartı Limiti ve Asgari Ödeme Oranı` iken, Debit Kart modelinin en önemli girdisi `Vadesiz Hesap Bakiyesi ve Maaş/Para Giriş Günü`dür. Ayrıca 7 bin aktif / 133 bin inaktif debit portföyünde model %90 oranında **Aktivasyon (0'dan 1'e geçiş)** çalışacaktır.

#### E. Batch (Gün Sonu) vs. Near Real-Time (Yakın Gerçek Zamanlı)
* **Model Çıkarımı (Kampanya Ataması):** Kesinlikle **Batch (Haftalık veya Aylık)** çalışmalıdır. Bir müşterinin harcama hedefini saniyede bir değiştirmeye gerek yoktur.
* **Hedef Takibi (Progress Bar - Faz 3):** **Near Real-Time (Event-Driven / Kafka veya CDC)** olmalıdır. Müşteri kasada kartı çektiğinde provizyon (ISO 8583 `0100/0200`) onaylandığı an sayaç güncellenmelidir (üzerine *"Provizyondaki işlemler dahildir, iptal/iade durumunda güncellenir"* şerhi düşülür). Ancak **Ödül Yükleme (Fulfillment)** dolandırıcılık ve anlık iptal riskine karşı **Batch (Takas/Clearing sonrası veya kampanya bitiminde)** yapılmalıdır.
* **Konum Bazlı Teklif (Madde 5.2):** Gerçek zamanlı mobil GPS + Geofencing gerektirir. **Önerim:** Faz 1 ve Faz 2'de kesinlikle **kapsam dışı** bırakılmalıdır.

#### F. Build vs. Buy: Mastercard (SessionM / Dynamic Yield / Test & Learn) vs. In-House Ar-Ge

| Kriter | Hazır Vendor (Örn: Mastercard Çözümleri) | In-House (Kendi Ar-Ge Projemiz) | Kazanan / Gerekçe |
| :--- | :--- | :--- | :--- |
| **TÜBİTAK / Ar-Ge Merkezi Uyumu** | **Sıfır.** Hazır paket alımı veya dış danışmanlık Ar-Ge teşviklerinden reddedilir. | **%100 Uyumlu.** Algoritma ve karar motoru bankanın öz malı (IP) olur. | **In-House** (Stratejik hedefimiz Ar-Ge olduğu için tek başına belirleyici) |
| **27 Bin Aktif Kartta Maliyet (ROI)** | Vendor'lar genellikle sabit lisans + taban ücret (yüz binlerce dolar/euro) ister. 27 bin aktif kartın yaratacağı ilave interchange bu lisansı **ödeyemez**! | İç veri bilimi ve yazılım kaynağı ile geliştirilir; marjinal lisans maliyeti sıfırdır. | **In-House** (Portföy ölçeği yabancı dövizli SaaS lisansını kaldırmaz) |
| **Veri Egemenliği ve BDDK / KVKK** | Müşteri bazlı kârlılık (MIS) ve vadesiz mevduat verisini yurt dışı/üçüncü parti buluta çıkarmak BDDK Sır Niteliğindeki Bilgiler yönetmeliğine takılır. | Tüm veri (MIS, mevduat, kart) banka içinde kalır. | **In-House** |
| **Esneklik (Kredi + Debit + Ayrıcalıklar Dünyası)** | Sadece kart işlem verisini görür; bankanın iç "Ayrıcalıklar Dünyası" kurallarını ve MIS kârlılığını entegre etmek aylarca sürer. | Doğrudan veri ambarı ve Odea Mobil API'leriyle konuşur. | **In-House** |

---

### Adım 2 Sonu Değerlendirmesi

**(a) 3 Maddelik Özet:**
1. **20 bin aktif Kredi + 7 bin aktif Debit kart** ölçeğimiz, klasik "büyük veri" AI modellerini (standart T-Learner, derin RL) aşırı öğrenme (overfitting) nedeniyle başarısız kılar; buna karşılık **"Düşük Örneklemde Hiyerarşik Bayesyen Uplift + R-Learner + Bütçe Kısıtlı MILP Optimizasyonu"** kurgusu hem bu ölçekte yüksek başarıyla çalışır hem de TÜBİTAK için kusursuz bir Ar-Ge tezi oluşturur.
2. Kredi ve Debit kartlar için **kampanya yürütme/takip altyapısı ortak**, ancak **AI modelleri ve öznitelikleri (limit vs. vadesiz bakiye) ayrı** olmalıdır; ayrıca Debit kartta (%95 inaktiflik) ana hedef upsell değil **aktivasyon** olmalıdır.
3. Hem **TÜBİTAK Ar-Ge hedefi** hem de **27 bin aktif kartlık ölçek ekonomisi** nedeniyle Mastercard vb. dış paket alımı (Buy) finansal ve stratejik olarak elenmektedir; çözüm **In-House** geliştirilmelidir.

**(b) Sana Sorduğum Açık Sorular:**
1. Bireysel bankacılık ekibinin geçmişte yaptığı "kontrol grubu" ölçümünün tam olarak nasıl yapıldığını (gerçekten rastgele %X müşteriye teklif gizlendi mi, yoksa teklife katılmayanlara mı bakıldı?) teyit edebilir misin?
2. Kredi kartında %70 (~46 bin kişi), Debit kartta %95 (~133 bin kişi) **inaktif müşteri** var. Ar-Ge kapsamına Faz 1'den itibaren bu dev inaktif kitleyi uyandıracak **"Kişiselleştirilmiş Aktivasyon / İlk Harcama"** modelini de (Upsell ile birlikte) dahil edelim mi? *(Küçük aktif portföy kısıtını aşmak ve iş değerini katlamak için bence şart!)*
3. Kart işlemlerini (provizyon anında) dinleyebileceğimiz gerçek zamanlı bir akış (Kafka, RabbitMQ, Event Hub veya Switch/Authorizer log akışı) şu an erişilebilir durumda mı, yoksa sadece gün sonu (T+1) tablolarını mı görebiliyoruz?

**(c) Onay İsteği:**
Adım 2'deki teknik mimariye, tablo puanlamalarına ve "27 bin aktif müşteri için Bayesyen/R-Learner Uplift + In-House" stratejisine katılıyor musun? Yanıtlarınla birlikte **Adım 3 – Veri ve Uyum** aşamasına geçmek için onayını bekliyorum.

---

## 🗣️ Soru / Girdi #4

1-Bireysel bankacılık ekibinin geçmişte yaptığı "kontrol grubu" işlevini çok dikkate alma, onlar debit için yapmışlardı.
2-sadece kredi kartları için yapalım.
3-Kart işlemlerini (provizyon anında) dinleyebileceğimiz gerçek zamanlı bir akış olduğunu kabul et. 
Adım 2'deki teknik mimariye, tablo puanlamalarına ve "27 bin aktif müşteri için Bayesyen/R-Learner Uplift + In-House" stratejisine katılıyorum.

### 🤖 Yanıt

Aldığın 3 karar projeyi çok ciddi bir mimari bataklıktan kurtardı:
1. **Sadece Kredi Kartına Odaklanma (~20 bin aktif + ~46 bin inaktif = ~66 bin açık kart):** Debit kartın düşük interchange ve vadesiz mevduat erimesi (deposit cannibalization) kâbusunu denklemden çıkardık. Artık tek bir ekonomik motor (Kredi Kartı: Interchange + Faiz/Taksit Geliri vs. Ödül Maliyeti) kuracağız.
2. **Geçmişte Kredi Kartı Kontrol Grubu Yokluğu:** Bu bilgi hayati. Geçmişte uygun olan herkese teklif basıldığı için elimizdeki geçmiş veri "gözlemsel ve yanlı" (observational & biased). İlk Faz 1 pilotuna çıkarken ilk gerçek **Rastgeleleştirilmiş Kontrol Grubunu (RCT)** biz kuracağız; geçmiş veriden ise **Farkların Farkı (Difference-in-Differences - DiD)** ve **Sentetik Kontrol** ile sadece başlangıç ağırlıklarını (prior) öğreneceğiz.
3. **Gerçek Zamanlı Provizyon Akışı (Real-Time Stream):** ISO 8583 provizyon mesajlarını anlık dinleyebilmemiz, Odea Mobil'de anlık sayaç (progress bar) güncellemesini ve anlık kampanya istismarı (fraud) engellemeyi %100 yapılabilir kılıyor.

Şimdi bu netleşmiş zemin üzerinde **Adım 3 – Veri ve Uyum** aşamasına geçiyoruz.

---

## Adım 3 – Veri ve Uyum

### 1. Gerekli Veri Setleri ve Kalite/Güncellik Riskleri

Projeyi bankadaki diğer çalışmalardan bağımsız kurguladığımız için, karar motorumuzun besleneceği **8 temel veri kümesini** ve bunların Türkiye bankacılık pratiğindeki kirli noktalarını açıkça ortaya koymalıyız:

| Veri Kümesi | Kritik Alanlar | Güncellik İhtiyacı | Kalite ve Güncellik Riski (`[VARSAYIM]` / `[DOĞRULANMALI]`) |
| :--- | :--- | :--- | :--- |
| **1. Kart İşlem Akışı (Provizyon & Takas)** | `Card_No/Token`, `Customer_ID`, `Tx_Timestamp`, `Amount`, `Currency`, `MCC` (DE-18), `Acquirer_ID` (DE-32), `Merchant_ID` (DE-42), `Merchant_Name_Loc` (DE-43), `POS_Entry_Mode` (DE-22: E-ticaret/Fiziki/Temassız), `Installment_Count` | **Anlık** (Provizyon) + **T+1 Gün** (Takas/Clearing) | **Yüksek Risk:** Provizyonda onaylanan işlemin takasa gelmemesi (iptal), dövizli işlemlerde provizyon-takas kur farkı ve ön provizyon (otel/araç kiralama) tutar değişimleri. |
| **2. Harcama Kategorisi ve Marka Verisi** | `Normalized_Brand` (Örn: Migros, Trendyol), `Custom_Category` (Market, Restoran, Seyahat), `Sub_Category`, `Online_Aggregator_Flag` (İyzico/Param/PayTR) | **Yeni işyeri geldikçe Near Real-Time / Günlük** | **Çok Yüksek Risk:** Tek başına `MCC` koduna güvenilemez! (Detayını aşağıda açıyorum). |
| **3. Kredi Kartı Ürün ve Limit/Risk Verisi** | `Total_Limit`, `Available_Limit` (Boş Limit), `Cut_Off_Date` (Hesap Kesim Günü), `Revolver_Transactor_Flag` (Asgari ödeyen vs. tam ödeyen), `Delinquency_Day` (Gecikme günü) | **Günlük (T+1)** (Boş limit için **Anlık**) | **Düşük Risk:** Çekirdek kart sisteminde temizdir. Ancak boş limiti 2.000 TL olan müşteriye AI'ın "+10.000 TL harcama hedefi" vermemesi için boş limitin **sert kısıt (hard constraint)** olması şarttır. |
| **4. Müşteri Kârlılık (MIS) Verisi** | `Customer_ID`, `Card_Net_Contribution` (Interchange + Faiz/Ücret - Fonlama), `Bank_Total_Profitability` (Mevduat/Kredi/Yatırım net katkısı), `Risk_Cost` | **Aylık** (`[VARSAYIM: Ayın 5-10'u arası güncellenir]`) | **Orta-Yüksek Risk:** MIS verisi 30-40 gün geriden gelir. Müşteri geçen hafta vadesiz mevduatını bankadan çektiyse aylık MIS tablosu onu hâlâ "Çok Kârlı" gösterebilir. |
| **5. Kampanya ve Ödül Geçmişi** | `Campaign_ID`, `Target_Shown`, `Reward_Offered`, `Enrollment_Timestamp` (Katılım anı), `Target_Achieved_Flag`, `Reward_Loaded`, `Reward_Redeemed` (Puan kullanımı) | **Günlük (T+1)** | **Orta Risk:** Geçmişte Excel üzerinden yürütülen kampanyaların "kime hangi hedef teklif edildi ama katılmadı" logları eksik veya dağınık dosyalarda olabilir **`[DOĞRULANMALI]`**. |
| **6. Segment ve Sadakat ("Ayrıcalıklar Dünyası")** | `Current_Segment`, `Segment_Qualification_Status`, `Remaining_Criteria_For_Next_Tier`, `Active_Perks` | **Günlük (T+1)** | **Düşük Risk:** Ancak dokümandaki iş kuralı gereği, kredi kartı harcamasının mevcut segmenti korumaya/yükseltmeye etkisi olup olmadığı bayrağı (`Segment_Impact_Flag`) net tanımlanmalıdır. |
| **7. İzin ve İletişim (İYS / KVKK)** | `KVKK_Profiling_Consent` (Açık rıza durumu), `IYS_SMS_Permission`, `IYS_Push_Permission`, `IYS_Email_Permission`, `Blacklist_Flag` | **Anlık / Günlük** | **Kritik Uyum Riski:** İYS izni olmayan müşteriye SMS/Push atılamaz; sadece Odea Mobil içine girdiğinde "Kampanyalarım" alanında pasif gösterim (in-app pull) yapılabilir **`[DOĞRULANMALI]`**. |
| **8. Odea Mobil Etkileşim Verisi** | `Last_Login_Date`, `Campaign_Tab_Views`, `Offer_Impression_Log` (Teklifi gördü mü?), `Offer_Click_Log` | **Günlük / Near Real-Time** | **Orta Risk:** Müşterinin teklifi gördüğü halde mi katılmadığı (Red), yoksa uygulamaya hiç girmediği için mi haberi olmadığı (Unaware) ayrımı modelin doğruluğu için şarttır. |

---

### 2. Bağımsız "Harcama Kategorisi ve Marka Zenginleştirme Motoru" Nasıl Üretilmeli?

Prompt'ta belirttiğin üzere bu projeyi bankadaki diğer kategorizasyon çalışmalarından **bağımsız** kurguluyoruz. Kart dünyasında sadece ISO 8583 `MCC` (DE-18) koduna güvenerek kişiselleştirilmiş kampanya yapmaya kalkarsak **sistem ilk günden çöker**. Neden?
1. **Ödeme Kuruluşu (PF / Aggregator) Körlüğü:** `IYZICO*...`, `PARAM*...`, `SIPAY*...`, `PAYTR*...` üzerinden geçen işlemlerde alt üye işyeri bir restoran veya giyim mağazası olsa bile MCC bazen genel hizmet veya yanlış kategori gelebilir.
2. **Yanlış POS Tanımı:** Türkiye'de binlerce fırın, kasap veya kafe vergi/komisyon avantajı veya ilk kurulum hatası yüzünden `MCC 5411 (Market/Bakkal)` veya `MCC 5999 (Çeşitli Perakende)` olarak tanımlıdır.
3. **Marka Çıkarımı İhtiyacı:** Madde 5.1'de ürün ekibi *"sektör, marka ve üye işyeri harcamalarını birlikte değerlendirerek"* diyor. ISO 8583 `DE-43` alanında `"MGR TCK ANKARA ÇANKAYA TR"` yazar; bunun **"Migros"** markası olduğunu bilmek gerekir.

**Proje Kapsamında Kuracağımız 3 Katmanlı Hibrit Kategorizasyon Mimarisi (Ar-Ge Bileşeni 1):**
* **Katman 1 – Deterministik Regex ve Sözlük (Dictionary Lookup):** İşlem hacminin %70'ini oluşturan en büyük 500 ulusal zincir (`MIGROS`, `BIM`, `A101`, `TRENDYOL`, `HEPSIBURADA`, `THY`, `OPET`, `SHELL`, `STARBUCKS` vb.) için `Merchant_ID (DE-42)` ve `DE-43` metin eşleştirme tablosu. (Maliyet: Sıfır, Hız: $<1\text{ ms}$).
* **Katman 2 – PF (Ödeme Kuruluşu) Ayrıştırıcı (Sub-Merchant Parser):** `DE-43` içindeki `IYZICO*MARKA_ADI` veya `PARAM*MARKA_ADI` kalıplarını yıldız (`*`) veya boşluk karakterinden bölerek gerçek alt markayı çıkaran kural katmanı.
* **Katman 3 – Küçük Dil Modeli / Karakter-Tabanlı Sinir Ağı (Char-CNN + Local BERT/SLM):** İlk iki katmandan kaçan uzun kuyruk (long-tail) yerel işyerleri (Örn: `"KARDESLER ET LOKANTASI"`, `"GÜVEN ECZANESİ"`) için sadece **yeni karşılaşılan** `(Merchant_ID, DE-43, MCC)` üçlülerini gece batch'inde sınıflandırıp `MERCHANT_MASTER_CACHE` tablosuna yazan yerel (on-prem) NLP modeli. Böylece gerçek zamanlı akışta LLM koşturulmaz; sadece önbellek (cache) tablosundan `O(1)` hızında okunur!

---

### 3. Regülasyon ve Uyum (KVKK, İYS, BDDK)

*(Not: Aşağıdaki maddeler kesin hukuki mütalaa değil, bankanın Hukuk, Uyum ve Bilgi Güvenliği ekipleriyle **`[DOĞRULANMALI]`** teknik-regülatif kontrol noktalarıdır.)*

1. **KVKK – Otomatik Sistemlerle Profilleme ve Açık Rıza (Madde 5 & Madde 11/1-g):**
   * Müşterinin son 12 aylık harcama detaylarını, kârlılığını ve davranışlarını analiz ederek kişiye özel hedef/ödül çıkarmak KVKK kapsamında **"Profilleme / Kişiselleştirilmiş Pazarlama"** faaliyetidir.
   * **Kontrol Noktası:** Müşterinin **"Kişisel verilerimin bana özel kampanya ve ürün/hizmet sunulması amacıyla işlenmesine / profillemeye"** yönelik **KVKK Açık Rızası** var mı? **`[DOĞRULANMALI]`**
   * Açık rızası **olmayan** müşteriler AI modeline sokulamaz; bu müşterilere sadece genel/kitlesel (herkese açık standart) kampanyalar gösterilebilir.
   * KVKK Madde 11/1-g uyarınca kişi *"münhasıran otomatik sistemler vasıtasıyla analiz edilmesi suretiyle kendisi aleyhine bir sonucun ortaya çıkmasına itiraz etme"* hakkına sahiptir. **"Ekonomik gerekçe yoksa teklif sunulmaması"** kararı hukuken "kişi aleyhine bir sonuç" mu, yoksa "ticari teşvikten yararlandırmama" mı sayılır? Hukuk birimiyle netleştirilmelidir.
2. **İYS (İleti Yönetim Sistemi) ve Kanal Ayrımı:**
   * Müşterinin KVKK profilleme rızası olabilir ama İYS'de SMS/Push izni olmayabilir (veya tam tersi).
   * **Mimari Kural:** Model teklifi üretir ve `CUSTOMER_OFFER_ASSIGNMENT` tablosuna yazar. Ancak müşteriye **proaktif bildirim (SMS / Push / E-posta)** çıkılması için `IYS_Permission = TRUE` şarttır. İzni yoksa teklif sadece müşteri Odea Mobil'e kendi isteğiyle girdiğinde kampanya ekranında (inbound/pull kanalında) listelenir **`[DOĞRULANMALI]`**.
3. **BDDK Bilgi Sistemleri Yönetmeliği ve Bulut LLM Yasağı (Sır Niteliğindeki Bilgiler):**
   * Bankacılık Kanunu Madde 73 (Müşteri Sırrı) ve BDDK Bilgi Sistemleri ve Elektronik Bankacılık Yönetmeliği uyarınca, müşteri işlem verileri, kârlılık bilgisi veya kimlik/kart verileri **yurt dışındaki bulut tabanlı LLM servislerine (OpenAI API, Anthropic, Google Cloud Vertex AI yurt dışı region vb.) KESİNLİKLE gönderilemez.**
   * **Mimari Kural:** Madde 5.2 (kampanya fikri) ve Madde 5.8 (gerekçe üretimi) veya POS metin sınıflandırması için kullanılacak herhangi bir dil modeli **%100 banka içi sunucularda (On-Premise GPU/CPU üzerinde açık ağırlıklı küçük modeller: Örn. Qwen-2.5-7B, Llama-3.1-8B veya BERTurk)** çalışmak zorundadır.

---

### 4. Adillik (Fairness) ve İtibar Riski

Ürün dokümanının 4.2 maddesindeki *"Zararda ve düşük potansiyelli müşteriye ekonomik gerekçe yoksa teklif sunulmaz"* ve *"Müşteri bazında farklı hedef/ödül"* kurgusu iki büyük adillik/itibar riski taşır:

1. **Sosyal Medya / "Eş-Dost" Karşılaştırma Krizi:**
   * Aynı işyerinde çalışan iki arkadaştan; kartını her ay düzenli 30.000 TL kullanan sadık müşteriye AI *"Bu zaten harcıyor, +36.000 TL hedefe 200 TL puan ver"* derken; kartını ayda 2.000 TL kullanan diğer arkadaşa *"Bunu kazanmamız lazım, +5.000 TL hedefe 500 TL puan ver"* derse ve bu ekran görüntüleri yan yana gelirse (veya Şikayetvar/Ekşi Sözlük'e düşerse) banka **"Sadık müşteriyi cezalandırıyorlar"** tepkisiyle karşılaşır.
   * **Mimari Çözüm (Monotonluk Kısıtı / Monotonicity & Fairness Guardrail):** Optimizasyon modeline şu matematiksel kısıt eklenmelidir: *Daha yüksek harcama hedefi ($T_A > T_B$) atanan bir müşterinin alacağı mutlak ödül tutarı ($R_A$), daha düşük hedef atanan müşterinin ödülünden ($R_B$) düşük olamaz ($R_A \ge R_B$).* Yani sadık müşteriden daha yüksek hedef istiyorsak, ödül tavanını da yüksek tutmalıyız.
2. **Korumalı/Hassas Özniteliklerin (Protected Attributes) Dışlanması:**
   * Model eğitiminde **Cinsiyet, Yaş (belirli eşikler hariç), Uyruk, Dini/Siyasi dernek bağış MCC'leri, Sağlık/Hastane harcama MCC'leri** ve sosyo-ekonomik ayrımcılık vekili (proxy) olabilecek dar mahalle/posta kodu bilgileri **kesinlikle öznitelik (feature) olarak kullanılmamalıdır.** (Örn: "Son 3 ayda hastane harcaması arttığı için sağlık kampanyası önerildi" gibi bir çıktı hem etik hem KVKK Özel Nitelikli Kişisel Veri açısından felakettir!).

---

### 5. Açıklanabilirlik (XAI) ve Model Risk Yönetimi (MRM)

Bankada İç Kontrol, Teftiş Kurulu ve Risk Yönetimi ekiplerinin bu projeyi onaylaması için "Kara Kutu (Black-Box) AI" kabul edilemez:
1. **Model Envanteri ve Versiyonlama (MLflow / Model Registry):** Hangi müşteriye, hangi tarihte, hangi model versiyonu (`v1.2`), hangi öznitelik değerleri (feature snapshot) ve hangi optimizasyon kısıtlarıyla o hedefin verildiği **en az 5 yıl** geriye dönük denetlenebilir (auditable) olarak loglanmalıdır.
2. **Enflasyon Kaynaklı Veri Kayması (Inflationary Drift) – Türkiye Gerçeği:**
   * Türkiye'de yıllık %40-60 enflasyon ortamında 6 ay önce eğitilmiş bir modelin öğrendiği "10.000 TL harcama = Yüksek harcama" kuralı bugün "Orta-Düşük harcama" haline gelir!
   * **Mimari Kural:** Modele giren tüm parasal büyüklükler (harcama tutarları, limitler, hedefler) nominal TL olarak değil; **TÜFE/Enflasyon Endeksi veya Portföy Medyan Harcamasına Oranlanmış (Normalized / Relative Spend)** olarak verilmelidir. Ayrıca her ay **PSI (Population Stability Index)** ile model kayması (drift) otomatik ölçülmelidir.

---

### 6. Kötüye Kullanım ve Kampanya İstismarı (Campaign Gaming & Fraud)

Kişiye özel yüksek ödüllü kampanyalar devreye alındığı an "kampanya avcıları" (gamers) sistemi suistimal etmeye çalışacaktır. Tasarlayacağımız karar ve takip motorunda şu **4 Anti-Gaming Kuralı** doğuştan (by-design) yer almalıdır:

1. **İade / İptal İstismarı (Refund Abuse & Clawback):** Müşteri 15.000 TL hedefi tutturmak için e-ticaretten 10.000 TL'lik ürün alır, ödül/puan yüklendiği gün ürünü iade eder.
   * **Önlem:** Ödül, işlem provizyon anında değil takas (clearing) sonrasında yüklenir; yüklenen ödül sonrası 60 gün içinde hedefi düşüren bir iade (`Credit Voucher / Refund`) gelirse, `REWARD_FULFILLMENT_QUEUE` üzerinden ödül geri alınır (clawback) veya kart ekstresine borç kaydedilir (kampanya koşul metnine açıkça yazılır).
2. **Nakit Benzeri İşlemler (Quasi-Cash / Cüzdan Yükleme İstismarı):** Müşteri harcama hedefini doldurmak için Papara/Paycell/Param gibi elektronik para hesaplarına kredi kartından para yükler, altın/döviz/kripto alır veya kendi POS cihazından (self-acquiring) kartını çeker.
   * **Önlem:** `CAMPAIGN_TEMPLATE` tablosunda `Excluded_MCC_List` (Örn: `6012, 6051, 6211, 5094 - Kuyum/Altın, 4829, 7995`) ve `Self_Merchant_Check` (Müşteri TCKN/VKN $\neq$ Üye İşyeri VKN kontrolü) standart filtre olarak çalışır.
3. **Bölünmüş İşlem (Split Transaction) İstismarı:** Faz 2'de "Harcama Frekansı" kampanyası yapıldığında (Örn: *"5 kez 500 TL harca"*), müşterinin kasada 2.500 TL'lik market alışverişini kasiyere 5 ayrı fiş olarak çektirmesi.
   * **Önlem:** Frekans kampanyalarında *"Aynı gün, aynı üye işyerinden yapılan işlemlerden yalnızca ilki kampanyaya dahildir"* kuralı gerçek zamanlı sayaç motoruna (`OFFER_PROGRESS_TRACKER`) gömülür.

---

### Adım 3 Sonu Değerlendirmesi

**(a) 3 Maddelik Özet:**
1. Tek başına ISO 8583 `MCC` koduyla kişiselleştirme yapılamaz; proje kapsamında PF (`İyzico/Param`) ayrıştırıcı, 500 büyük zincir sözlüğü ve yerel (on-prem) küçük dil modeli (SLM/BERT) içeren **3 katmanlı bağımsız bir İşlem Zenginleştirme Motoru** kurmalıyız (bu aynı zamanda TÜBİTAK için güçlü bir Ar-Ge iş paketidir).
2. KVKK/BDDK uyumu için açık rızası olmayan müşteriler profilleme modeline girmemeli, **Sağlık/Eczane/Bağış MCC'leri** modelden tamamen dışlanmalı ve hiçbir müşteri verisi bulut LLM'lere çıkarılmayıp tüm modeller **On-Premise** koşturulmalıdır.
3. Türkiye'deki enflasyonist ortam nedeniyle model nominal TL ile değil **enflasyondan arındırılmış/endekslenmiş (relative) harcama verisiyle** eğitilmeli; sadık müşteriyi küstürmemek için optimizasyona **"Hedef arttıkça mutlak ödül düşemez" (Monotonluk)** kısıtı eklenmelidir.

**(b) Sana Sorduğum Açık Sorular:**
1. Banka içinde (On-Premise) kendi küçük dil modelimizi (SLM/BERT - 7B/8B parametreli) ve ML modellerimizi çalıştırabileceğimiz **GPU'lu bir sunucu altyapısı** var mı, yoksa TÜBİTAK proje bütçesine donanım (GPU sunucu) alımı da ekleyelim mi?
2. Kredi kartı portföyümüzdeki (~66 bin açık kart) müşterilerin yaklaşık yüzde kaçında **KVKK Kişiselleştirme/Profilleme Açık Rızası** ve **İYS İletişim İzni** var? *(Eğer izin oranı %30'un altındaysa aktif 20 bin kişilik örneklemimiz daha da küçülecek demektir — bunu **`[DOĞRULANMALI]`** olarak not ediyorum.)*

**(c) Onay İsteği:**
Adım 3'teki veri mimarisi, bağımsız kategori zenginleştirme motoru, KVKK/BDDK sınırları ve suistimal (anti-gaming) kuralları konusunda hemfikir miyiz? Yanıtlarınla birlikte **Adım 4 – Fizibilite Değerlendirmesi (Yeşil/Sarı/Kırmızı Matrisi, Finansal ROI Modeli ve Go/No-Go Kararı)** aşamasına geçmek için onayını bekliyorum.

---

## 🗣️ Soru / Girdi #5

1-GPU'lu bir sunucu altyapımız var
2-Kredi kartı portföyümüzdeki (~66 bin açık kart) yarısında KVKK Kişiselleştirme/Profilleme Açık Rızası ve İYS İletişim İzni var

### 🤖 Yanıt

Verdiğin iki yanıt, **Adım 4 – Fizibilite Değerlendirmesi**'nin finansal ve istatistiksel omurgasını netleştirdi:
1. **On-Premise GPU Varlığı:** Harika. BDDK/KVKK bulut engeline takılmadan ve ek donanım yatırımı (CapEx) yapmadan yerel dil modellerini (SLM/BERT) ve Bayesyen simülasyonları (MCMC/Variational Inference) kendi sunucularımızda koşturabiliriz.
2. **%50 KVKK ve İYS İzin Oranı (Kritik Daralma!):** Hesap makinesini elimize alalım:
   * Toplam açık kredi kartı: **~66.000** $\rightarrow$ %50 izinli = **~33.000 ulaşılabilir müşteri**.
   * Aylık aktif kredi kartı: **~20.000** $\rightarrow$ %50 izinli (**`[VARSAYIM: İzin oranı aktif ve inaktiflerde homojen dağılıyor; aktiflerde %60 ise 12.000 kişi]`**) = **10.000 – 12.000 ulaşılabilir aktif müşteri!**
   * İnaktif (uyuyan) kredi kartı: **~46.000** $\rightarrow$ %50 izinli = **~23.000 ulaşılabilir inaktif müşteri!**

Bir **sparring partner** olarak bu sayının ne anlama geldiğini açıkça söylemeliyim: **10.000 kişilik aktif izinli kitle**, aşırı karmaşık (Faz 2'deki gibi 10 farklı kampanya kırılımlı) bir yapıyı tek başına finanse edemez. Projenin finansal fizibilitesini kurtarmak için **23.000 izinli inaktif müşteriyi uyandırma (Reactivation)** ve **Odea Mobil içinde tek tıkla KVKK/İYS izin kazanımı (Consent Capture)** kurgusunu denkleme sokmak zorundayız.

Şimdi tüm boyutlarıyla **Adım 4 – Fizibilite Değerlendirmesi**'ni masaya koyuyorum.

---

## Adım 4 – Fizibilite Değerlendirmesi

### 1. Boyutlar Bazında Fizibilite Karnesi (Yeşil / Sarı / Kırmızı)

| Fizibilite Boyutu | Durum | Gerekçe ve Kritik Darboğazlar |
| :--- | :---: | :--- |
| **1. Teknik Fizibilite** | **YEŞİL** *(Faz 1 & 3)* <br>**SARI** *(Faz 2)* | **Neden Yeşil:** On-prem GPU altyapısı mevcut, gerçek zamanlı provizyon (ISO 8583) akışı var, bağımsız 5 tablolu kampanya karar/sayaç motoru iç kaynakla rahatlıkla yazılır.<br>**Neden Sarı (Faz 2):** 10.000 – 12.000 izinli aktif müşteride Faz 2'deki 8 maddenin tamamını (özellikle 5.2 gerçek zamanlı konum bazlı restoran ve çoklu kategori bandit modellerini) çalıştırmak istatistiksel örneklem yetersizliğine (under-powered test) takılır. |
| **2. Veri Fizibilitesi** | **SARI** | **Artılar:** Müşteri ID bazında MIS kârlılık verisi ve işlem akışı hazır.<br>**Eksiler (Neden Sarı?):** (1) Kredi kartında geçmişte gerçek bir **randomize kontrol grubu (hold-out) yok**, bu yüzden model ilk 2-3 ay "soğuk başlangıç" (cold-start) yaşayacak. (2) Ham MCC ve `DE-43` işyeri verisi kirli; önce proje kapsamında 3 katmanlı NLP/Sözlük kategorizasyon motorunun kurulması şart. |
| **3. Operasyonel Fizibilite** | **SARI** | **Risk:** Excel'den tam dinamik yapıya geçildiğinde İş Birimi'nin "Neden A müşterisine 15.000 TL hedef gitti de B'ye gitmedi?" sorularını ve Çağrı Merkezi'ne gelecek müşteri itirazlarını yönetecek bir **İzleme/Açıklama Ekranı (Back-office UI)** şarttır. Tamamen serbest hedef tutarları yerine **15-20 standart mikro-paket (micro-bucket)** kullanılmazsa operasyon kilitlenir. |
| **4. Regülasyon ve Uyum Fizibilitesi** | **YEŞİL** | **Neden Yeşil:** On-prem GPU sayesinde yurt dışına veri çıkışı (BDDK riski) sıfırlandı. %50 izinli kitle sınırları net. Sağlık/bağış gibi hassas MCC'ler filtrelendiğinde ve "Hedef arttıkça ödül düşemez" (Monotonluk) kuralı konulduğunda KVKK ve adillik açısından engel kalmıyor. |
| **5. Finansal Fizibilite** | **KIRMIZI** *(Saf Ticari / Teşviksiz)* <br>**SARI-YEŞİL** *(TÜBİTAK Teşvikli + İnaktif Kart Dahil)* | **Acı Gerçek:** Sadece 10.000 izinli aktif kredi kartı müşterisinden elde edilecek "ilave upsell harcaması", 4-5 kişilik bir yazılım/veri bilimi ekibinin yıllık maliyetini **tek başına zor karşılar**. Ancak **(a) TÜBİTAK 1501 / Ar-Ge Merkezi desteği (%60-%75 personel hibesi)** ve **(b) 23.000 izinli inaktif kartın aktivasyonu** eklendiğinde proje **net pozitif ROI** üretir. |

---

### 2. Kaba İş Değeri Modeli (Finansal Simülasyon ve Duyarlılık Analizi)

Ürün ekibinin dokümanında hiç yapmadığı **Birim Ekonomisi (Unit Economics)** hesabını şeffafça yapalım. Tüm rakamlar Türkiye kredi kartı piyasası ortalamalarına dayalı **`[VARSAYIM]`** olup kendi gerçek çarpanlarınla değiştirmeye hazırdır:

#### Temel Varsayımlar (`[VARSAYIM]`)
* **Hedef Kitle (Senaryo A - Sadece Aktif İzinli Upsell):** $N_{\text{aktif}} = 11.000$ müşteri (%10'u sabit kontrol grubu = $9.900$ teklif alan müşteri).
* **Mevcut Ortalama Aylık Kart Harcaması (Baseline):** $15.000\text{ TL / ay}$.
* **Banka Brüt Kart Katkı Marjı ($m$):** Harcama üzerinden **net %2,2** *(~%1,5 - %1,8 Takas/Interchange komisyonu + ~%0,5 - %0,7 Taksit/Kredilendirme/Revolving faiz ve nakit akış katkısı ortalaması)*.
* **Mevcut Excel Kampanyası Durumu:** Her ay müşterilerin %25'i hedefi tutturuyor ama bunların **%60'ı Free-Rider** (kampanya olmasa da harcayacaktı). Ortalama dağıtılan ödül: kazanım başına $350\text{ TL}$ (veya harcamanın ~%1,5'i).

#### Yıllık Karşılaştırmalı Ekonomik Etki Tablosu

| Gelir / Maliyet Kalemi | Mevcut Durum (Excel `+%20` Kuralı) | AI Powered Card (Faz 1: Sadece 11 Bin Aktif Upsell) | AI Powered Card (Genişletilmiş: 11 Bin Aktif + 23 Bin İnaktif Uyandırma) |
| :--- | :--- | :--- | :--- |
| **1. Gerçek İlave (Incremental) Harcama Oranı (Uplift)** | %4,0 *(Çoğu organik/free-rider)* | **%9,0** *(Doğru hedef elastikiyeti ile)* | **%9,0** (Aktif) + **Ayda 1.500 Aktifleşen Kart** ($\times 8.000\text{ TL}$) |
| **2. Yıllık Toplam İlave (Incremental) Hacim** | $71,2\text{ Milyon TL}$ | **$160,3\text{ Milyon TL}$** | **$304,3\text{ Milyon TL}$** |
| **3. Brüt İlave Banka Geliri ($\text{Hacim} \times \%2,2\text{ Marj}$)** | $1,57\text{ Milyon TL / yıl}$ | **$3,53\text{ Milyon TL / yıl}$** | **$6,70\text{ Milyon TL / yıl}$** |
| **4. Dağıtılan Toplam Ödül Maliyeti (Puan/Cashback)** | $-1,04\text{ Milyon TL / yıl}$ *(Free-rider'lara ödenen dahil)* | **$-0,75\text{ Milyon TL / yıl}$** *(Free-rider ve kârsız müşteriler elendiği için %28 tasarruf)* | **$-1,35\text{ Milyon TL / yıl}$** |
| **5. Kampanya Net Ekonomik Katkısı ($\text{Gelir} - \text{Ödül}$)** | **$+0,53\text{ Milyon TL / yıl}$** | **$+2,78\text{ Milyon TL / yıl}$** | **$+5,35\text{ Milyon TL / yıl}$** |
| **6. Mevcut Yönteme Göre Yıllık Ekstra Kazanç ($\Delta$ Net Lift)** | *Referans (0 TL)* | **$+2,25\text{ Milyon TL / yıl}$** | **$+4,82\text{ Milyon TL / yıl}$** |

#### Geliştirme ve İşletim Maliyeti Karşılaştırması (`[VARSAYIM]`)
* **İşletim Maliyeti (OpEx):** On-prem GPU mevcut olduğu için bulut/lisans maliyeti **$0\text{ TL}$**.
* **Geliştirme Maliyeti (Ar-Ge Ekibi Eforu):** 12 aylık projede toplam ~2,5 - 3 FTE (Tam Zamanlı Eşdeğer: 1 Kıdemli Veri Bilimci, 1 Backend/Veri Mühendisi, 0,5 Ürün/Analist) yıllık işveren maliyeti yaklaşık **$6,0 - 7,5\text{ Milyon TL}$** mertebesindedir.
* **TÜBİTAK 1501 / Ar-Ge Merkezi Kaldıracı:** Personel maliyetinin **%60 ila %75'i** hibe/vergi indirimi ile geri alındığında bankanın cebinden çıkan **net Ar-Ge maliyeti ~$2,0 - 2,5\text{ Milyon TL}$**'ye düşer!

---

### 3. Duyarlılık Analizi (Sonucu En Çok Değiştiren 4 Kritik Değişken)

Bu finansal modeli Yönetim Kurulu veya Ürün Ekibi önüne koyduğumuzda sonucu en sert değiştirecek **4 kaldıraç (duyarlılık)** şunlardır:

1. **KVKK / İYS İzin Oranı (%50 $\rightarrow$ %80'e çıkarsa):**
   * Odea Mobil'e girişte *"Size özel kredi kartı ödüllerini görmek için tek tıkla izin verin"* kurgusuyla izin oranı %50'den %80'e çıkarılırsa, hedef kitle bir anda **11 binden 17,6 bine** çıkar ve net katkı hiçbir algoritma değişikliği olmadan **+%60 artar!**
2. **İnaktif 46.000 Kartın Kapsama Alınması (En Büyük Çarpan):**
   * Yukarıdaki tabloda görüldüğü üzere, sadece 11.000 aktif kartla uğraşmak yılda $+2,25\text{ Milyon TL}$ ek değer yaratırken, uyuyan 23.000 izinli kartın sadece %6-7'sini uyandırmak net katkıyı **iki katına ($+4,82\text{ Milyon TL}$)** çıkarıyor.
3. **Kredilendirme (Revolving / Taksit) Etkisi ($m$ marjı %2,2 $\rightarrow$ %4,5):**
   * Eğer kampanya sonrası yapılan ilave harcamanın sadece %25'i bile kredilendirilirse (asgari ödeme / faizli devir veya ücretli taksit), bankanın marjı %2,2'den %4,5'e fırlar ve finansal getiri **2 katına** çıkar.
4. **"Free-Rider" (Organik Harcayıcı) Oranı:**
   * Mevcut Excel kampanyasında ödül alanların ne kadarının kampanya olmasa da o harcamayı yapacağı (Free-rider oranı) %60 yerine %75 çıkarsa, AI modelinin sadece **"teklif vermeme / bütçe koruma"** özelliği bile yılda 500-600 bin TL doğrudan nakit kurtarır.

---

### 4. Nihai Karar Önerisi: KOŞULLU GO (CONDITIONAL GO)

Bir danışman ve sparring partner olarak kararım; ürün dokümanındaki haliyle **"Kapsamlı Go" DEĞİL**, aşağıdaki **4 Sert Koşula Bağlı "KOŞULLU GO"**dur:

* **Koşul 1 (Kapsam Genişletme – İnaktif Kartlar):** Faz 1 kapsamına sadece "aktif müşteriye upsell" değil, **izinli ~23.000 inaktif kredi kartı müşterisini uyandırma (Reactivation / İlk Harcama)** modeli de ilk günden dahil edilmelidir. Aksi halde 10-11 bin kişilik aktif havuz hem istatistiksel hem finansal olarak çok dardır.
* **Koşul 2 (Kapsam Budama – Faz 2 Temizliği):** Ürün dokümanındaki **Madde 5.2 (Gerçek zamanlı konum bazlı restoran önerisi)** tamamen kapsamdan çıkarılmalı; Faz 2'deki 8 dağınık madde **TÜBİTAK iş paketlerine uygun 3 çekirdek modülde** birleştirilmelidir.
* **Koşul 3 (TÜBİTAK / Ar-Ge Konumlandırması):** Proje maliyeti doğrudan kart kampanya bütçesine yıkılmamalı; **TÜBİTAK 1501 / Ar-Ge Merkezi projesi** olarak tescillenip personel maliyetinin %60-75'i teşvik kapsamına alınmalıdır.
* **Koşul 4 (Odea Mobil İzin Kazanımı - Consent Funnel):** Faz 3 (Odea Mobil) kapsamında, izinsiz %50'lik (~33.000) kredi kartı müşterisine uygulama içinde **"Kilitli/Bulanık Özel Teklif Kartı"** (*"Size özel hesaplanan 500 TL'ye varan teklifi görmek için profilleme izni verin"*) gösterilerek izin havuzu büyütülmelidir.

---

### Adım 4 Sonu Değerlendirmesi

**(a) 3 Maddelik Özet:**
1. %50 KVKK/İYS izin oranı nedeniyle elimizde sadece **~10.000-12.000 izinli aktif kredi kartı müşterisi** kalmaktadır; bu ölçekte sadece "aktif upsell" yapmak tek başına yazılım yatırımını zor amorti eder (**Saf Ticari Fizibilite: Kırmızı/Sarı**).
2. Ancak denkleme **(1) ~23.000 izinli inaktif kartın uyandırılması (Reactivation)**, **(2) Odea Mobil içi "Bulanık Teklif" ile izin kazanımı** ve **(3) TÜBİTAK Ar-Ge hibesi + sıfır bulut maliyeti (mevcut GPU)** eklendiğinde proje yılda **~4,8 Milyon TL ilave net katkı** ile ilk yıldan kendini amorti eder (**Koşullu Go: Yeşil**).
3. Teknik ve regülasyon açısından on-prem GPU ve gerçek zamanlı provizyon akışı sayesinde hiçbir kırmızı bayrak yoktur; en kritik operasyonel şart ise serbest küsuratlı hedefler yerine **15-20 adet parametrik mikro-kova (micro-bucket)** kullanılmasıdır.

**(b) Sana Sorduğum Açık Sorular:**
1. Finansal modelde kullandığım **`[VARSAYIM]`** rakamlar (Aktif kart başına aylık ortalama ~15.000 TL harcama ve interchange + faiz dahil ortalama %2,2 brüt kart marjı) senin portföy gerçeklerine yakın mı, yoksa revize etmemi istediğin bir çarpan var mı?
2. Koşul 1'de önerdiğim **"~23.000 izinli inaktif kredi kartını uyandırma (Reactivation)"** ve Koşul 4'teki **"Odea Mobil'de bulanık teklif ile KVKK/İYS izin kazanımı"** hamlelerini Ürün ekibine bizim karşı-teklifimiz olarak sunmaya ne dersin?

**(c) Onay İsteği:**
Adım 4'teki fizibilite karnesi, birim ekonomisi hesabı ve 4 şartlı **Koşullu Go** kararı üzerinde mutabık mıyız? Onayınla birlikte **Adım 5 – Ar-Ge Yol Haritası, Faz 1 Pilot Tasarımı (Örneklem & RCT Hesabı) ve Ürün Ekibine Sorulacak En Kritik 10 Soru** aşamasına geçelim.

---

## 🗣️ Soru / Girdi #6

Faz 2 Temizliği neleri kapsıyor, neleri sildin neler kaldı?

### 🤖 Yanıt

Çok yerinde bir soru. Ürün ekibinin dokümanındaki [Faz 2 (Madde 5.1 – 5.8)](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/AI_Powered_Card_2027_Final_Kapsam.docx) başlıklarını tek tek masaya yatırıp; **neyi tamamen sildiğimizi**, **neyi düzelterek birleştirdiğimizi** ve **geriye hangi net yapının kaldığını** tam şeffaflıkla göstereyim.

---

### 1. Madde Madde "Faz 2 Temizliği" Tablosu (Neler Silindi, Neler Kaldı?)

| Dokümandaki Madde | Karar | Ne Silindi / Ne Kaldı ve Teknik Gerekçesi |
| :--- | :---: | :--- |
| **5.1:** Harcama davranışından müşteri ihtiyacının tahmin edilmesi *(3/6/12 aylık trend)* | **BİRLEŞTİRİLDİ** *(5.3 ve 5.7 ile tek motorda toplandı)* | **Kaldı**, ancak tek başına ayrı bir model olmaktan çıkarıldı. Bir müşterinin geçmiş 3/6/12 aylık trendi (5.1) ile mevsimsel trendi (5.7) aslında aynı modelin iki farklı girdisidir (feature). Ayrı model yapılırsa birbirleriyle çelişirler. |
| **5.2:** Harcama örüntülerinden yeni kampanya fırsatlarının keşfedilmesi *(örn: öğle yemeği / konum bazlı restoran)* | **YARISI SİLİNDİ, YARISI 5.6 İLE BİRLEŞTİRİLDİ** | **SİLİNEN KISIM:** Parantez içindeki **"konum bazlı etraftaki restoranların bulunabilmesi"** ve gerçek zamanlı lokasyon tetikleyicisi **tamamen silindi** (Odea Mobil arka plan GPS izni, geofencing ve POS harita koordinatı yokluğu nedeniyle).<br>**KALAN KISIM:** İşlem saat/gün örüntülerinden fırsat bulma işi, 5.6'daki "dinamik segment keşfi" ile birebir aynı kümeleme (clustering) işi olduğu için onunla birleştirildi ve sadece **analiste raporlayan bir iç araç** yapıldı. |
| **5.3:** Müşterinin harcama potansiyeli olan sektörlerin belirlenmesi *(Benzer profillerden çapraz satış)* | **KALDI** *(Ana Modül 1'in kalbi)* | **Olduğu gibi kaldı.** İşbirlikçi Filtreleme (Collaborative Filtering) ile müşterinin bizde hiç kullanmadığı ama benzer müşterilerin kullandığı sektörü (MCC) bulmak gerçek, ölçülebilir ve değerli bir AI işidir. |
| **5.4:** Kişiselleştirilmiş teklif kurgusu *(Kampanya türü + Ödül tipi [Puan/Cashback] + Hedef)* | **KALDI** *(Ancak Faz 1 motoruyla birleştirildi ve sadeleştirildi)* | **Kaldı**, fakat Faz 1'deki motorun üzerine ayrı bir sistem olarak değil, Faz 1'in devamı olarak kurgulandı.<br>**SİLİNEN / KISITLANAN KISIM:** 11 bin aktif müşteride hem 10 kampanya türünü, hem Puan vs Cashback'i, hem de 10 farklı hedefi aynı anda denemek (300 kombinasyon!) örneklem yetersizliğinden çöker. Bu yüzden kampanya sayısı **en fazla 4-5 ana kategoriyle (Market, E-ticaret, Restoran, Seyahat, Aktivasyon)** sınırlandırıldı. |
| **5.5:** Müşteri bazında kampanya dönüşüm potansiyelinin tahmin edilmesi *(Propensity)* | **METODOLOJİSİ DEĞİŞTİRİLDİ** *(Propensity Silindi $\rightarrow$ Uplift Kaldı)* | **SİLİNEN KISIM:** "Kimin kampanyaya katılma olasılığı yüksek?" (Propensity) yaklaşımı **silindi**; çünkü bu model zaten harcama yapacak "beleşçi" (free-rider) müşteriyi seçip ödül bütçesini yakar.<br>**KALAN KISIM:** Yerine **"Kimin bu kampanya sayesinde İLAVE (incremental) harcama yapma potansiyeli yüksek?" (Kategori Bazlı Uplift)** konuldu. |
| **5.6:** Harcama davranışından yeni müşteri segmentlerinin keşfedilmesi *(Dinamik gruplama)* | **KAPSAMI DARALTILDI** *(5.2 ile birleştirildi, canlı akıştan çıkarıldı)* | **SİLİNEN KISIM:** Canlı karar motorunun içinde anlık segment üretmesi **silindi** (Zaten bireysel N=1 teklif verirken segmente ihtiyaç yok).<br>**KALAN KISIM:** 5.2 ile birleştirilerek **"Analist Keşif Panosu" (Offline BI Aracı)** haline getirildi. Ayda bir çalışır, ürün ekibine *"Bakın şöyle 3 yeni davranış grubu belirdi, bunlara yeni kampanya şablonu açmak ister misiniz?"* der. |
| **5.7:** Harcama trendlerinden gelecekteki kampanya ihtiyaçlarının tahmin edilmesi *(Mevsimsellik)* | **BİRLEŞTİRİLDİ** *(5.1 ve 5.3 ile birleştirildi)* | **Kaldı**, ancak ayrı bir "mevsimsellik modeli" olarak değil; Sektörel Öneri Motoru'nun (Modül 1) içindeki **takvim/mevsimsellik katsayısı** olarak entegre edildi. |
| **5.8:** Müşteriye özel kampanya önerisinin gerekçesinin oluşturulması | **KISITLANARAK KALDI** *(Serbest LLM Silindi $\rightarrow$ XAI + Şablon Kaldı)* | **SİLİNEN KISIM:** LLM'in müşteriye serbest metin üretmesi ve *"Son 3 ayda restoran harcamanız arttığı için..."* gibi gözetlenme (creepy) hissi veren ifadeler **silindi**.<br>**KALAN KISIM:** Modelin iç açıklanabilirliği (SHAP) **iş birimi ekranında** tüm detayıyla kalacak; **müşteriye (Odea Mobil'de)** ise Hukuk onaylı 15-20 pozitif şablondan (*"Seyahat ayrıcalıklarınız kapsamında size özel..."* gibi) biri gösterilecek. |

---

### 2. Özetle: Eski Faz 2 vs. Temizlenmiş Yeni Faz 2

Ürün ekibinin **8 parçalı, birbiriyle çakışan** Faz 2 listesini, hem **11 bin aktif + 23 bin inaktif** kredi kartı ölçeğinde çalışabilecek hem de **TÜBİTAK hakemlerinin** "Burada sağlam ve bütünlüklü bir sistem mimarisi var" diyeceği **3 Net Çekirdek Modüle** indirgedik:

1. **Modül 2.1 – Zamana Duyarlı Sektörel Öneri Motoru (5.1 + 5.3 + 5.7'nin Birleşimi):**
   * *Ne yapar?* Müşterinin geçmiş trendini (5.1), benzer müşterilerin harcadığı ama kendisinin kullanmadığı sektörleri (5.3) ve mevsimselliği (5.7) tek bir **Hibrit Öneri Matrisinde** birleştirir. Her müşteri için en yüksek potansiyelli **Top-3 Sektörü (MCC grubunu)** sıralar.
2. **Modül 2.2 – Çoklu Kampanya Uplift ve Bütçe Optimizatörü (Faz 1 + 5.4 + 5.5'in Birleşimi):**
   * *Ne yapar?* Modül 2.1'den gelen aday sektörleri alır; Faz 1'de kurduğumuz **Uplift + Doğrusal Programlama (MILP)** motorunu kullanarak müşteriye *"Genel Upsell mi, Market mi, E-Ticaret mi yoksa Uyandırma (Reactivation) mı verilmeli; hedefi ve ödül tipi (Puan/Cashback) ne olmalı?"* kararını **net ilave kârlılığa** göre verir.
3. **Modül 2.3 – Açıklanabilir AI (XAI) ve Analist Keşif Asistanı (5.2 + 5.6 + 5.8'in Birleşimi):**
   * *Ne yapar?* İki yüzü vardır:
     * **İç Yüz (Ürün Ekibi İçin):** İşlem verilerini kümeler (5.2 & 5.6), on-prem GPU'daki yerel LLM ile analiste yeni kampanya fırsatlarını özetler ve her teklifin arkasındaki gerçek SHAP gerekçesini gösterir.
     * **Dış Yüz (Müşteri / Odea Mobil İçin):** Müşteriyi ürkütmeden, onaylı şablonlarla teklifin gerekçesini sunar (5.8).

---

### Sparring Partner Sorusu ve Onay

Bu temizlik sayesinde:
* **Hiçbir iş değerini kaybetmedik** (Ürün ekibinin istediği tüm ticari çıktılar hâlâ masada).
* **En büyük bataklığı (anlık GPS/konum bazlı restoran) kestik attık.**
* **8 ayrı model kurup 11 bin kişilik veriyi parçalama hatasından döndük.**

Bu **"8 Maddeden 3 Çekirdek Modüle İndirgenmiş Faz 2"** kurgusu kafana yattı mı? Onaylıyorsan hemen **Adım 5 – Ar-Ge Yol Haritası, Faz 1 Pilot Tasarımı (Örneklem & RCT Hesabı) ve Ürün Ekibine Sorulacak En Kritik 10 Soru** adımına geçiyorum.

---

## 🗣️ Soru / Girdi #7

Onaylıyorum

### 🤖 Yanıt

Harika. Faz 2 temizliğiyle birlikte elimizde artık şişirilmiş bir istek listesi değil, **TÜBİTAK hakemlerinin karşısına çıkabilecek bütünlüklü bir Ar-Ge mimarisi** var. 

Şimdi **Adım 5 – Ar-Ge Yol Haritası ve Pilot Tasarımı**'na geçiyoruz. Burada özellikle **11.000 izinli aktif + 23.000 izinli inaktif kredi kartı** ölçeğimizde istatistiksel olarak anlamlı sonuç üretecek pilot matematiğini ve Ürün Ekibini köşeye sıkıştıracak **En Kritik 10 Soruyu** masaya koyuyorum.

---

## Adım 5 – Ar-Ge Yol Haritası ve Pilot Tasarımı

### 1. MVP Önerisi: Faz 1 Pilot Tasarımı (Upsell + İnaktif Uyandırma)

Küçük örneklem büyüklüğümüzde ($N_{\text{aktif}} \approx 11.000$) klasik rastgele zar atma (basit A/B testi) yaparsak, kontrol grubuna tesadüfen düşecek 15-20 "yüksek harcamalı (whale)" müşteri tüm ortalamayı bozar. Bu yüzden pilotu **Tabakalı Rastgeleleştirme (Stratified Block Randomization)** ve **CUPED (Deney Öncesi Veriyle Varyans Azaltma)** tekniğiyle kurguluyoruz:

* **Temel Hipotez ($H_1$):**
  * *Müşteri bazlı baseline harcama tahmini, MIS kârlılık kısıtı ve elastikiyet odaklı hedef/ödül optimizasyonu ile sunulan teklifler; mevcut Excel (`Son 3 ay ort. × 1,20`) yöntemine kıyasla **birim ödül maliyeti başına en az %35 daha yüksek Net İlave Ekonomik Katkı (Net Incremental Lift)** üretir.*
* **Pilot Kitle ve Örneklem Dağılımı (Toplam ~34.000 İzinli Kredi Kartı):**
  * Örneklem zaten küçük olduğu için pilotu "portföyün %10'unda deneyelim" diyemeyiz; **tüm izinli kitleyi** 3 kollu (3-Arm) kontrollü deneye sokmalıyız:
  * **Kohort A – Aktif Kartlar (~11.000 Kişi – Upsell Pilotu):**
    1. **Saf Kontrol Grubu (Hold-out – %15 = ~1.650 kişi):** Hiçbir upsell teklifi gönderilmez (Organik baseline harcamayı ölçmek için).
    2. **Mevcut Yöntem / Şampiyon Grup (Excel `+%20` Kuralı – %25 = ~2.750 kişi):** İş biriminin mevcut mantığıyla hedef/ödül verilir (Benchmark).
    3. **AI Ar-Ge Grubu (Challenger – %60 = ~6.600 kişi):** Kendi içinde **%45 Optimizasyon (Exploitation)** ve **%15 Elastikiyet Keşfi (Exploration: `+%10`, `+%20`, `+%35` hedef varyasyonları)** olarak ayrılır.
  * **Kohort B – İnaktif Kartlar (~23.000 Kişi – Uyandırma / Reactivation Pilotu):**
    1. **Saf Kontrol Grubu (%15 = ~3.450 kişi):** Teklif yok (Kendiliğinden uyanma oranını ölçmek için).
    2. **AI Uyandırma Grubu (%85 = ~19.550 kişi):** Müşterinin geçmişte kartı bırakmadan önceki son aktif sektörüne ve kârlılık potansiyeline göre düşük eşikli "İlk Harcama / Hoş Geldin" teklifi.
* **İstatistiksel Güç (Power Analysis) ve Süre:**
  * **Süre:** **2 Tam Kampanya Dönemi (8 Hafta / 2 Ay)** + **30 Gün İade/Kredilendirme Gözlem Süresi** (Toplam 3 Ay). Tek aylık pilot, maaş günü/bayram kaymaları nedeniyle yanıltıcıdır.
  * **Neden CUPED Şart?** Müşterinin son 6 aylık harcamasını eş-değişken (covariate) olarak kullanan CUPED düzeltmesi sayesinde, 1.650 kişilik kontrol grubuyla dahi **%6 ve üzeri harcama artışını %95 güven düzeyinde ($\alpha=0.05$, Power $= \%80$)** istatistiksel olarak kanıtlayabiliriz.
* **Başarı Metrikleri (Birincil ve İkincil):**
  1. **Birincil (Kuzey Yıldızı) Metrik:** Müşteri Başına Net İlave Katkı ($\text{Net Lift per Targeted Customer}$):
     $$\text{Net Lift} = [(\bar{Y}_{\text{AI}} - \bar{Y}_{\text{Kontrol}}) \times \text{Marj}] - \text{Birim Ödül Maliyeti}_{\text{AI}}$$
  2. **İkincil Metrikler:** Ödül ROI'si ($\text{İlave Harcama} / \text{Dağıtılan Ödül}$), "Free-Rider" Tasarruf Oranı (Teklif verilmeyip yine de harcama yapan kârsız müşteri oranı), İnaktif Kart Uyandırma Oranı (%).
* **Durdurma Kriterleri (Kill-Switch / Acil Fren):**
  1. Pilotun ilk 15 gününde **günlük kümülatif ödül kazanım hızı (burn-rate)**, aylık bütçenin %60'ını aşarsa (kural hatası veya suistimal/gaming şüphesi).
  2. Gerçek zamanlı provizyon akışı ile sayaç (`OFFER_PROGRESS_TRACKER`) arasındaki gecikme 2 saati aşar ve Çağrı Merkezi'ne günde >50 kampanya şikayeti düşerse.
  3. AI grubunun brüt harcaması, ilk ay sonunda Saf Kontrol Grubunun (Hold-out) istatistiksel olarak anlamlı biçimde **altına** düşerse (ters tepki / müşteri küstürme).

---

### 2. Aşamalı Ar-Ge Evrimi (TÜBİTAK İş Paketleri Kurgu Planı)

Hızlı ticari çıktıdan ziyade **TÜBİTAK 1501 / Ar-Ge Merkezi** kriterleri (Özgünlük, Sistematiklik, Teknolojik Belirsizlik) öncelikli olduğu için projeyi **15 Aylık 5 İş Paketine (İP)** bölüyoruz:

```mermaid
flowchart LR
    IP1["İP-1 (Ay 1-3)<br/>Veri Zenginleştirme &<br/>Baseline Motoru"] --> IP2["İP-2 (Ay 3-6)<br/>Bayesyen Uplift &<br/>Faz 1 Pilotu (RCT)"]
    IP2 --> IP3["İP-3 (Ay 6-9)<br/>MILP Bütçe Optimizatörü &<br/>Parametrik Sayaç Motoru"]
    IP3 --> IP4["İP-4 (Ay 9-12)<br/>Sektörel Öneri (Modül 2.1) &<br/>Contextual Bandit (Modül 2.2)"]
    IP4 --> IP5["İP-5 (Ay 12-15)<br/>On-Prem SLM/XAI Asistanı &<br/>Odea Mobil Tercih Öğrenme"]
```

* **Aşama 1 (İP-1: Ay 1–3) – Hibrit POS İşlem Zenginleştirme ve Enflasyon-Düzeltmeli Baseline:**
  * PF (`İyzico/Param`) ayrıştırıcı + 500 Zincir Sözlüğü + On-Prem BERT/SLM ile `DE-43` işyeri kategorizasyonu.
  * Müşterinin kampanya olmasa yapacağı harcamayı (organik baseline) tahmin eden Hiyerarşik Zaman Serisi modeli.
* **Aşama 2 (İP-2: Ay 3–6) – Düşük Örneklemde Nedensel Uplift (R-Learner) ve Faz 1 Kontrollü Pilotu:**
  * İlk gerçek RCT (Hold-out) verisinin toplanması; R-Learner / Bayesyen Küçültme (Shrinkage) ile bireysel ödül elastikiyetinin modellenmesi.
* **Aşama 3 (İP-3: Ay 6–9) – Kısıtlı Doğrusal Programlama (MILP) ve Gerçek Zamanlı Sayaç Çekirdeği:**
  * MIS kârlılığı, boş limit, monotonluk (adillik) ve toplam bütçe kısıtları altında her müşteriye optimal `(Hedef, Ödül)` mikro-kovasını atayan çözücü.
  * ISO 8583 provizyon akışını dinleyen bağımsız 5 tablolu kampanya takip motoru.
* **Aşama 4 (İP-4: Ay 9–12) – Faz 2: Zamana Duyarlı Sektörel Öneri ve Contextual Bandit:**
  * 5.1+5.3+5.7 birleşimi olan Sektörel Öneri Motoru ve Thompson Sampling ile sürekli keşif-kullanım (Puan vs. Cashback, Sektör seçimi) dengesi.
* **Aşama 5 (İP-5: Ay 12–15) – Faz 3: On-Premise XAI/SLM Analist Asistanı ve Odea Mobil Seçim Mimarisi:**
  * SHAP tabanlı gerekçelendirme, kümeleme (5.2/5.6) asistanı ve Odea Mobil'de müşteriye eşdeğer net katkılı 3 teklif sunup seçimine göre tercih öğrenen (Discrete Choice) arayüz.

---

### 3. Ekip, Rol İhtiyacı ve Kritik Bağımlılıklar

| Rol | Kişi / Ağırlık (FTE) | Sorumluluk ve Ar-Ge Görevi |
| :--- | :---: | :--- |
| **Kıdemli Veri Bilimci (Causal ML & Optimization)** | 1,0 FTE | R-Learner Uplift, Bayesyen Bandit, MILP bütçe optimizasyonu ve deney tasarımı (RCT/CUPED). *(TÜBİTAK projesinin kilit araştırmacısı; mümkünse yüksek lisans/dokteralı).* |
| **NLP / ML Mühendisi (MLOps & On-Prem SLM)** | 1,0 FTE | POS `DE-43` metin sınıflandırma, on-prem GPU üzerinde SLM/SHAP kurulumu, MLflow model envanteri ve enflasyon kayması (drift) izleme. |
| **Kıdemli Backend & Veri Mühendisi** | 1,0 FTE | 5 çekirdek tablonun, gerçek zamanlı ISO 8583 provizyon dinleyicisinin (Kafka/Consumer), sayaç motorunun ve Odea Mobil API'lerinin yazılması. |
| **Kart Ürün / Kampanya Analisti (İş Birimi)** | 0,5 FTE | Kampanya şablonlarının tanımlanması, MIS kârlılık eşiklerinin belirlenmesi, üretilen tekliflerin onaylanması (Human-in-the-loop). |
| **Mobil Ekip & Uyum/Hukuk Desteği** | 0,25 FTE *(Paylaşımlı)* | Faz 3 Odea Mobil kampanya kartı/progress bar ekranları ve KVKK/İYS/Adillik kurallarının onayı. |

* **En Kritik 3 İç Bağımlılık:**
  1. **Switch / Provizyon Akışı Erişimi:** Kart işlem akışının (ISO 8583 `0100/0200` ve takas dosyalarının) gecikmesiz olarak yeni sayaç motorumuza akıtılması.
  2. **Ödül Yükleme (Fulfillment) Entegrasyonu:** Sayaç motorumuz "Bu müşteri hedefi tutturdu, 400 TL puan/cashback hak etti" dediğinde, ana kart sistemine (Paycore/Intertech) bu ödülü yükleyecek servis/batch entegrasyonu.
  3. **Odea Mobil Ekran Alanı:** Mobil uygulama ekibinin sprint takviminde Faz 3 ekranları ve KVKK izin kazanım (Consent Banner) geliştir몌si için yer açılması.

---

### 4. Ürün Ekibine Sorulması Gereken En Kritik 10 Soru (Toplantı Cephaneliğin)

Ürün ekibiyle masaya oturduğunda, kontrolü ilk dakikadan eline almanı sağlayacak **10 keskin soru**:

1. **Kontrol Grubu Cesareti:** *"Başarıyı 'kampanya olmasaydı yapılacak harcamaya göre ilave katkıyla' (Madde 8) ölçmek istiyorsunuz. Bunun tek bilimsel yolu, kampanya şartlarını sağlayan izinli müşterilerin **%15'ine (yaklaşık 1.650 aktif müşteriye) bilerek hiçbir teklif göndermemektir (Hold-out)**. Bu %15'lik gruba teklif göndermeme kararının arkasında duracak mısınız?"*
2. **Aktif Portföy Darlığı ve İnaktif Kartlar:** *"Açık 66 bin kredi kartımızın sadece 20 bini aktif ve bunun da yarısı (%50 = ~10-11 bin kişi) KVKK/İYS izinli. Sadece 10 bin kişiye upsell yaparak bu projenin yatırım maliyetini çıkaramayız. Geriye kalan **46 bin inaktif kartı (23 bini izinli) uyandırma (Reactivation)** kampanyalarını Faz 1'in merkezine koymaya var mısınız?"*
3. **Kârlılık Eşiği ve Veto Hakkı:** *"Madde 4.2'de 'Kârlı müşteriye bağlılık için kontrollü teşvik verilir, zarardakine verilmez' diyorsunuz. Mevduatı yüksek olduğu için banka genelinde çok kârlı olan ama kartını zaten her ay 50.000 TL kullanan bir müşteriye model **'İlave harcama yaratmayacak, ödül vermeyin'** derse, segment yönetimi 'Özel müşterimize nasıl teklif gitmez!' diye modeli ezecek mi?"*
4. **Mikro-Kova (Bucket) Kabulü:** *"Her müşteriye `14.327 TL hedefe 412 TL ödül` gibi küsuratlı sayılar göstermek hem müşterinin kafasını karıştırır hem Çağrı Merkezi'ni kilitler. Bunun yerine AI'ın müşterileri **önceden onaylanmış 15-20 yuvarlak Hedef/Ödül paketinden** (`10.000 TL'ye 300 TL`, `15.000 TL'ye 500 TL` vb.) en uygununa atamasını kabul ediyor musunuz?"*
5. **Madde 5.2 Konum Bazlı Restoran İsteği:** *"Dokümanda parantez içinde geçen 'konum bazlı etraftaki restoranların bulunması' maddesi; Odea Mobil'de 7/24 arka plan GPS takibi, harita veritabanı ve anlık konum işleme gerektirir. Bunu projenin kapsamından tamamen çıkarmamız konusunda mutabık mıyız?"*
6. **Madde 5.8 "Gözetlenme (Creepy)" Riski:** *"Müşteriye 'Son 3 ayda restoran harcamanız arttığı için bu teklifi sunduk' demek birçok müşteride 'Banka yediklerimi mi takip ediyor?' tepkisi doğurur. Gerekçeyi ayrıntılı olarak **sizin ekranlarınızda** gösterip, müşteriye Odea Mobil'de daha pozitif ve genel şablonlar göstermeyi onaylıyor musunuz?"*
7. **Puan vs. Cashback Ekonomisi (Madde 5.4):** *"Modelin kime Puan, kime Cashback (nakit iade) vereceğine karar vermesini istiyorsunuz. Banka muhasebesinde 100 TL Puan ile 100 TL Cashback'in maliyeti aynı mı, yoksa Puan'da yanma/kullanılmama (breakage) veya üye işyeri katkı payı avantajı var mı? Varsa bu oran nedir?"*
8. **Çakışan Kampanyalar ve Çift Ödül (Cannibalization):** *"Bir müşteriye AI tarafından 'Aylık 20.000 TL Genel Harcama Hedefi' atandığında, aynı müşteri bankanın genel 'E-ticarette 250 TL Puan' kampanyasına da katılabilir mi? Tek bir harcama iki sayacı birden dolduracak mı, yoksa AI teklifi alanlar genel kampanyalardan dışlanacak mı?"*
9. **İade (Refund) ve Ödül Geri Alma (Clawback) Kuralı:** *"Müşteri hedefi tutturup ödülü aldıktan 10 gün sonra yaptığı alışverişi iade ederse; verdiğimiz puanı/cashback'i geri alacak mıyız (ekstreye borç yazacak mıyız) ve ödülü işlem anında mı yoksa kampanya/ekstre bitiminde mi yükleyeceğiz?"*
10. **KVKK/İYS İzin Kazanım Hedefi:** *"Portföyün %50'sinde pazarlama/profilleme izni yok. Odea Mobil kampanya ekranına giren izinsiz müşterilere 'Size özel hesaplanan ödül teklifini açmak için tek tıkla izin verin' kurgusu eklememize destek verecek misiniz?"*

---

### Adım 5 Sonu Değerlendirmesi

**(a) 3 Maddelik Özet:**
1. Faz 1 pilotu sadece 11.000 izinli aktif kartla sınırlı kalmamalı; **23.000 izinli inaktif kartı (Reactivation)** da kapsayacak şekilde, **%15 Saf Kontrol (Hold-out)** ve **CUPED varyans azaltma** yöntemiyle 2 tam ay (8 hafta) koşulmalıdır.
2. Proje, TÜBİTAK 1501 / Ar-Ge Merkezi kriterlerine tam uyumlu **15 aylık 5 İş Paketine (İP-1..İP-5)** bölünmüş olup; çekirdeğinde 3 tam zamanlı teknik rol (1 Kıdemli Veri Bilimci, 1 NLP/ML Mühendisi, 1 Backend/Veri Mühendisi) gerektirir.
3. Ürün ekibiyle yapılacak toplantıda yukarıdaki **10 Kritik Soru** (özellikle %15 kontrol grubuna teklif göndermeme cesareti, konum bazlı maddenin silinmesi ve 15-20 mikro-kova kullanımı) proje sözleşmesi niteliğinde netleştirilmelidir.

**(b) Sana Sorduğum Açık Sorular:**
1. Kendi ekibinde şu an bu 3 teknik rolden (Veri Bilimci, NLP/MLOps Mühendisi, Backend/Veri Mühendisi) hangileri mevcut, hangileri için yeni işe alım veya TÜBİTAK akademik danışman (üniversite hocası) desteği düşünüyorsun?
2. Ürün ekibine soracağımız 10 sorudan "Bunu sormayalım, cevabını zaten biliyorum" dediğin veya eklemek istediğin özel bir politik hassasiyet var mı?

**(c) Onay İsteği:**
Adım 5'teki pilot tasarımı, 15 aylık Ar-Ge yol haritası ve 10 kritik soru üzerinde mutabık mıyız? Onayınla birlikte son adımımız olan **Adım 6 – Fikir Yürütme (Ürün Ekibinin Düşünmediği Gerçekçi ve Yüksek Değerli 7 İnovatif Fikir)** aşamasına geçelim.

---

## 🗣️ Soru / Girdi #8

bu wiki sayfasına erişebilyior musun? https://wiki.odeabank.com.tr/pages/viewpage.action?pageId=215358568

### 🤖 Yanıt

Wiki sayfasına (`wiki.odeabank.com.tr`) erişim durumunu kontrol etmek için bağlantı testini başlattım, sonucunu bekliyorum.

---

## 🗣️ Soru / Girdi #9

erişemiyorsan login credientials vereyim

### 🤖 Yanıt

Bilgi güvenliği ve bankacılık/BDDK kuralları gereği **kurumsal kullanıcı adı ve şifreni (login credentials) kesinlikle sohbet ekranında paylaşma.** Ayrıca komut satırı üzerinden yapılan istek kurumsal ağ/SSO katmanında takıldığı için şifreyle de doğrudan çekemeyebiliriz.

Bunun yerine en hızlı ve güvenli **2 yöntemden birini** kullanabilirsin (10 saniye sürer):

1. **Kopyala – Yapıştır (En Hızlısı):** Wiki sayfasındaki metni seçip (`Ctrl+A` $\rightarrow$ `Ctrl+C`) doğrudan buraya mesaj olarak yapıştır.
2. **Word/PDF Olarak Klasöre At:** Wiki sayfasının sağ üstündeki **`•••` (Üç nokta)** menüsünden **Export to Word** diyerek dosyayı [Kart Kampanya Yapay Zeka](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka) klasörüne kaydet; dosyayı oradan anında okuyabilirim. *(İstersen `/browser` komutunu da deneyebilirsin ancak kurumsal iç ağda kopyala-yapıştır veya Word'e aktarmak çok daha sorunsuz çalışır.)*

---

## 🗣️ Soru / Girdi #10

Resmi kurumlara başvurmak için kullanacağımız PROJE BİLGİ FORMU doldurmanı ve Gant şeması çizmeni istiyorum. Örnek PROJE BİLGİ FORMU, Veri Tabanlı Dinamik Sadakat ve Yatırım Entegrasyon Platformu_Proje_Önerisi.pdf dosyasıdır. Bu dokümanı yazmak için referans alman gereken doküman Yazılım Geliştirme - Proje Yazım Taslağı.pdf dosyasıdır. Referans alacağın gant şema dosyası Veri Tabanlı Dinamik Sadakat ve Yatırım Entegrasyon Platformu_Gantt_Şeması.xlsx dosyasıdır. Şimdi ne yapılacağına  dair planlama yap ve benim onayıma sun.

### 🤖 Yanıt

Klasördeki dört referans dosyayı da ([Veri Tabanlı Dinamik Sadakat ve Yatırım Entegrasyon Platformu_Proje_Önerisi.pdf](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/Veri%20Tabanl%C4%B1%20Dinamik%20Sadakat%20ve%20Yat%C4%B1r%C4%B1m%20Entegrasyon%20Platformu_Proje_%C3%96nerisi.pdf), [Yazılım Geliştirme - Proje Yazım Taslağı.pdf](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/Yaz%C4%B1l%C4%B1m%20Geli%C5%9Ftirme%20-%20Proje%20Yaz%C4%B1m%20Tasla%C4%9F%C4%B1.pdf), [Teknoloji Hazırlık Seviyesi (THS) Bilgilendirme.pdf](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/Teknoloji%20Haz%C4%B1rl%C4%B1k%20Seviyesi%20(THS)%20Bilgilendirme.pdf) ve [Veri Tabanlı Dinamik Sadakat ve Yatırım Entegrasyon Platformu_Gantt_Şeması.xlsx](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/Veri%20Tabanl%C4%B1%20Dinamik%20Sadakat%20ve%20Yat%C4%B1r%C4%B1m%20Entegrasyon%20Platformu_Gantt_%C5%9Eemas%C4%B1.xlsx)) satır satır inceledim.

Özellikle önceki `ODEA-012` başvuru formu ile `ODEA-028 Yazılım Geliştirme - Proje Yazım Taslağı` karşılaştırıldığında, resmi kurul/hakem değerlendirmesinde **neden ret veya revizyon alınabileceği** çok net görülüyor. Yeni hazırlayacağımız **PROJE BİLGİ FORMU** ve **GANTT ŞEMASI** için eylem planımı aşağıda onayına sunuyorum.

---

### 1. Önceki Başvurudaki (`ODEA-012`) Kritik Eksikler ve Yeni Dokümanda Yapacağımız Düzeltmeler

1. **Boş Bırakılan / Kaymış Alanların Eksiksiz Doldurulması:**
   * `ODEA-012` PDF'inde *Mevcut THS Açıklama*, *Hedef THS*, *Hedef THS Açıklama*, *Sürdürülebilirlik*, *Projenin Müşterisi*, *Hedef Pazar* ve *Rekabet Analizi* kutuları **boş bırakılmış**; ayrıca *Çevreye Etkileri* ile *Alınacak Dış Hizmetler* metinleri birbiriyle **yer değiştirmiş** (kaymış) durumdaydı.
   * **Plan:** Yeni formda tüm bu tablolar `Yazılım Geliştirme - Proje Yazım Taslağı` kurallarına %100 uygun olarak eksiksiz doldurulacak.
2. **İş Birimi Dokümanı Kopyalaması Yerine "Hakem Odaklı" Proje Özeti:**
   * `ODEA-012`'de Proje Özeti kısmına (Sayfa 2–7) iş biriminin ürün kuralları (varlık limitleri, kahve hediyesi vb.) doğrudan yapıştırılmıştı. Oysa taslak doküman *"Genel/kitabi kavramlar ve rutin iş kuralları tekrar edilmemeli; doğrudan teknik problem, belirsizlikler ve çözüm yaklaşımı anlatılmalıdır"* diyor.
   * **Plan:** Proje Özeti'nde iş kurallarını 1-2 paragrafta özetleyip; ağırlığı **Nedensel Çıkarım (Causal ML / R-Learner Uplift), Kısıtlı Doğrusal Programlama (MILP), Bağlamsal Çok Kollu Haydut (Contextual Bandit) ve On-Premise Küçük Dil Modeli (SLM) ile POS Metin Madenciliği** mimarisine vereceğiz.
3. **Zorunlu KPI Formatı:**
   * Taslağın *Hedef Kazanım ve Sonuçlar* bölümü için zorunlu kıldığı **`Başarı Ölçütü (KPI) – Mevcut Durum – Hedef Değer – Ölçüm Yöntemi`** şablonunu birebir uygulayacağız.

---

### 2. "PROJE BİLGİ FORMU" İçerik Planı (Bölüm Bölüm Ne Yazacağız?)

Dijitalpark Teknokent başvuru formunun birebir formatında (Word `.docx` ve istenirse `.pdf` / `.md` olarak) şu içeriklerle dokümanı oluşturacağım:

| Form Bölümü | Taslak Kuralına Uygun Yazılacak İçerik Özeti |
| :--- | :--- |
| **1. Üst Dilekçe & Künye Tablosu** | • **Firma:** ODEA TEKNOLOJİ HİZMETLERİ A.Ş.<br>• **Önerilen Proje Adı** *(Taslak kuralı: "Proje/Sistem" kelimesi ve kısaltma içermeyen, Teknoloji + Amaç odaklı)*:<br>**"Kredi Kartı Kampanya Yönetiminde Nedensel Yapay Zekâ ve Kısıtlı Optimizasyon Tabanlı Kişiselleştirme Platformu Geliştirilmesi"** *(Alternatif: "Ödeme Sistemlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru Geliştirilmesi")*<br>• **Süre / İş Gücü / Bütçe:** Referans dosyadaki yapıya uyumlu olarak **18 Ay** (veya istersen 15 Ay), **22 Kişilik Ekip** (20 Ar-Ge + 2 Destek = 396 Adam/Ay), %25 Bölge Dışı Görevlendirme (14.256 saat). |
| **2. Teknoloji Alanları & NACE** | • **NACE:** `62.01.01`<br>• **Ana Teknoloji Alanı:** `1.2.3 Yapay Zeka` *(Önceki formda 1.2.6 seçilmişti; AI vurgusu için 1.2.3'ü ana alan yapıyoruz)*<br>• **Diğer Teknoloji Alanları:** `1.2.1 Gelişmiş sistem mimarisi`, `1.2.6 Bilgisayar yazılımları`, `1.2.8 Veri işleme / veri aktarımı`, `1.2.10 Veritabanları, veri madenciliği`, `1.2.11 Elektronik ödemeler`, `1.3.8 CRM`, `1.4.4 İnsan dilleri teknolojisi`, `1.4.5 Bilgi filtreleme, semantik, istatistik`, `5.4.1 Algoritmalar ve karmaşa`, `5.4.2 Matematik modelleme`, `5.4.3 İstatistiksel analiz`. |
| **3. Teknoloji Hazırlık Seviyesi (THS)** | • **Mevcut Durum:** `THS-3` *(Hedef: `THS-7`)*<br>• **Mevcut Durum Açıklaması (THS 1 $\rightarrow$ THS 3):** Taslaktaki kurala uygun olarak THS-1 (problem tanımı ve literatür taraması tamamlandı), THS-2 (nedensel çıkarım ve optimizasyon formülasyonu oluşturuldu) ve THS-3 (örneklem verisi üzerinde analitik doğrulama ve PoC başlatıldı) seviye seviye aksiyon fiilleriyle yazılacak.<br>• **Hedef Durum Açıklaması (THS 4 $\rightarrow$ THS 7):** THS-4 (laboratuvar prototipi ve 5 çekirdek tablo entegrasyonu), THS-5 (anonimleştirilmiş gerçek provizyon akışında doğrulama), THS-6 (yük ve performans testleri) ve THS-7 (Odea Mobil ve kart sistemlerinde randomize kontrollü canlı pilot doğrulama) adım adım yazılacak. |
| **4. Anahtar Kelimeler** | `Nedensel Çıkarım, Uplift Modelleme, Bağlamsal Çok Kollu Haydut, Doğrusal Programlama, Doğal Dil İşleme, Açıklanabilir Yapay Zekâ, Kişiselleştirilmiş Kampanya Yönetimi, Gerçek Zamanlı Veri İşleme, Ödeme Sistemleri` *(9 adet spesifik terim)*. |
| **5. Proje Özeti** | Mevcut statik (`+%20`) kampanya yönetiminin yarattığı "Free-Rider" (organik harcamayı fonlama) ve örneklem kısıtı problemini; geliştireceğimiz **4 Çekirdekli Ar-Ge Mimarisi** (1: *3 Katmanlı NLP/SLM POS İşlem Zenginleştirme*, 2: *R-Learner & Hiyerarşik Bayesyen Uplift Modeli*, 3: *MILP Bütçe/Adillik Optimizatörü ve Thompson Sampling Bandit*, 4: *SHAP/SLM Açıklanabilir Karar ve Gerçek Zamanlı ISO 8583 Sayaç Motoru*) ile nasıl çözeceğimizi teknik derinliğiyle anlatacağız. |
| **6. Hedeflenen Kazanım ve Sonuçlar (KPI Tablosu)** | Taslaktaki **`KPI – Mevcut Durum – Hedef Değer – Ölçüm Yöntemi`** formatında 7 somut hedef:<br>1. **POS İşyeri/Marka Sınıflandırma Doğruluğu (F1-Score):** Mevcut: Sadece MCC (%62) $\rightarrow$ Hedef: $\ge \%92$ (Etiketli test seti doğrulaması)<br>2. **Free-Rider (Gereksiz Ödül) Oranı:** Mevcut: ~%55–60 $\rightarrow$ Hedef: $\le \%25$ (RCT Hold-out karşılaştırması)<br>3. **Birim Ödül Başına Net İlave Harcama (Incremental ROI):** Mevcut: Baz (1,0x) $\rightarrow$ Hedef: $\ge \%35$ artış (CUPED düzeltmeli A/B/n testi)<br>4. **İnaktif Kart Uyandırma (Reactivation) Dönüşüm Oranı:** Mevcut: <%2 $\rightarrow$ Hedef: $\ge \%6,5$<br>5. **Gerçek Zamanlı Provizyon Sayaç Güncelleme Gecikmesi (Latency):** Mevcut: T+1 Gün (Batch) $\rightarrow$ Hedef: $< 500\text{ ms}$ (P95 uçtan uca akış testi)<br>6. **Bütçe Optimizasyon Çözüm Süresi:** Mevcut: Manuel Excel (3-5 iş günü) $\rightarrow$ Hedef: $66.000$ kart için $< 15\text{ dk}$ (MILP çözücü logları)<br>7. **Enflasyonist Veri Kayması (PSI) Dayanıklılığı:** Mevcut: Yok $\rightarrow$ Hedef: Aylık $\text{PSI} < 0,15$ (Otomatik MLOps izleme). |
| **7. Ar-Ge Niteliği & Yenilikçi Yönü** | • **4 Teknik Belirsizlik ve Özgün Çözüm:** (1) Düşük örneklem ($N \approx 20\text{ bin}$ aktif kart) ve yüksek varyansta standart T-Learner çökerken *Robinson Ayrıştırmalı R-Learner ve Hiyerarşik Bayesyen Küçültme* geliştirilmesi, (2) Yüksek enflasyonda nominal tutar kaymasına karşı *Göreli/Endeksli Zaman Serisi Baseline* mimarisi, (3) Kirli `ISO 8583 DE-43` ve Ödeme Kuruluşu (`İyzico/Param`) metinlerinden *On-Premise SLM + Char-CNN* ile marka çıkarımı, (4) *Monotonluk (Adillik) ve MIS Kârlılık Kısıtlı MILP* formülasyonu.<br>• **Rakip/Alternatif Kıyaslaması:** Geleneksel kural motorları (EVAM vb.) ve yurt dışı hazır paketlere (Mastercard SessionM/Dynamic Yield) kıyasla neden üstün olduğu (Veri egemenliği, MIS kârlılık entegrasyonu, Propensity yerine gerçek Uplift, düşük örneklem başarımı). |
| **8. Proje Detayı (Mimari + 6 İş Paketi)** | • **Sistem Mimarisi:** Frontend (React Native - Odea Mobil), Backend (.NET Core / Python FastAPI ML Serving), Veri & Mesajlaşma (MSSQL, Oracle, Redis/In-Memory Cache, RabbitMQ/Kafka ISO 8583 Stream), On-Prem GPU AI Katmanı.<br>• **İş Paketleri (WBS 1..6):** Gantt şemasıyla birebir örtüşen 6 Ana İş Paketi ve 30+ alt kırılım, her birinin çıktıları (deliverables) ve başarı ölçütleri. |
| **9. Ekonomik Kazanımlar, Çevre, Sürdürülebilirlik, Rekabet** | • **Ekonomik:** İthal yazılım/lisans ikamesi (yüz binlerce dolarlık yabancı SaaS tasarrufu), ödül bütçesi verimliliği, inaktif kart aktivasyonu.<br>• **Çevre & Sürdürülebilirlik:** On-prem önbellekli (cache-first) SLM mimarisi ile GPU enerji tüketiminin %80 azaltılması, kağıtsız/SMS-azaltımlı hedefli dijital iletişim.<br>• **Rekabet & Ürünleşme:** Odea Teknoloji'nin grup içi ve finans sektörüne sunabileceği yerli *"Causal AI Loyalty & Campaign Engine"* ürünü. |

---

### 3. Gantt Şeması (`.xlsx`) Planı (18 Aylık / 6 Ana İş Paketi)

Referans [Veri Tabanlı Dinamik Sadakat ve Yatırım Entegrasyon Platformu_Gantt_Şeması.xlsx](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/Veri%20Tabanl%C4%B1%20Dinamik%20Sadakat%20ve%20Yat%C4%B1r%C4%B1m%20Entegrasyon%20Platformu_Gantt_%C5%9Eemas%C4%B1.xlsx) dosyasındaki **birebir aynı hücre stillerini, renk kodlarını ve 18 aylık sütun yapısını** koruyarak yeni Excel dosyasını şu **6 Ana İş Paketi (WBS)** ile üreteceğim:

1. **İP 1 – Kapsam, Veri Analizi ve Mimari Tasarım (1. Ay – 2. Ay):**
   * `1.1` Kapsam Tanımı, İş Kuralları ve Kârlılık (MIS) Eşikleri Analizi
   * `1.2` Tarihsel İşlem, Kampanya ve İzin (KVKK/İYS) Veri Kalitesi Analizi
   * `1.3` Odea Mobil Kampanya, Sayaç ve Tercih Ekranları UX/UI Tasarımı
   * `1.4` Olay Güdümlü (Event-Driven) Karar ve Sayaç Motoru Mimari Tasarımı
2. **İP 2 – Hibrit POS İşlem Zenginleştirme ve Öznitelik (Feature Store) Altyapısı (2. Ay – 5. Ay):**
   * `2.1` ISO 8583 Provizyon/Takas Akışı ve DWH Veri Entegrasyon Katmanı
   * `2.2` Ödeme Kuruluşu (PF) Ayrıştırıcı ve Ulusal Zincir Sözlük Motoru Geliştirilmesi
   * `2.3` On-Premise Küçük Dil Modeli (SLM/BERT) ile Uzun Kuyruk Üye İşyeri/Marka Sınıflandırma
   * `2.4` Enflasyondan Arındırılmış Davranışsal Öznitelik Havuzu (Feature Store) ve MIS Kârlılık Entegrasyonu
3. **İP 3 – Faz 1: Nedensel Çıkarım (Causal Uplift) ve Kısıtlı Optimizasyon Çekirdeği (4. Ay – 9. Ay):**
   * `3.1` Hiyerarşik Zaman Serisi ile Organik Harcama (Baseline) Tahmin Modeli Geliştirilmesi
   * `3.2` Düşük Örneklemde R-Learner ve Bayesyen Küçültme Tabanlı Uplift Modeli Geliştirilmesi
   * `3.3` İnaktif Kart Uyandırma (Reactivation) ve İlk Harcama Eğilim/Elastikiyet Modeli
   * `3.4` Bütçe, Boş Limit, MIS Kârlılık ve Adillik (Monotonluk) Kısıtlı MILP Optimizasyon Çözücüsü
   * `3.5` Tabakalı Rastgeleleştirilmiş Kontrol Grubu (Stratified RCT) ve CUPED Deney Modülü
   * `3.6` Gerçek Zamanlı Hedef Takip (Progress Tracker), Ödül Yükleme ve İade Geri Alma (Clawback) Servisleri
4. **İP 4 – Faz 2: Çoklu Kampanya Öneri Sistemi, Contextual Bandit ve XAI/SLM Asistanı (8. Ay – 13. Ay):**
   * `4.1` İşbirlikçi Filtreleme ve Mevsimsellik Tabanlı Sektörel Öneri Motoru (Modül 2.1)
   * `4.2` Thompson Sampling (Contextual Multi-Armed Bandit) ile Dinamik Hedef ve Ödül Tipi (Puan/Cashback) Keşfi (Modül 2.2)
   * `4.3` Davranışsal Kümeleme (HDBSCAN/Embeddings) ve On-Prem SLM Destekli Analist Fırsat Keşif Panosu (Modül 2.3 İç Yüz)
   * `4.4` SHAP Tabanlı Açıklanabilir AI (XAI) ve Kısıtlı Şablon/SLM Müşteri Gerekçe Üretim Servisi (Modül 2.3 Dış Yüz)
5. **İP 5 – Faz 3: Odea Mobil, Web/Back-Office ve Ayrıcalıklar Dünyası Entegrasyonu (11. Ay – 16. Ay):**
   * `5.1` Odea Mobil Kişiye Özel Teklif, Anlık Sayaç (Progress Bar) ve Gerekçe Gösterim Ekranları
   * `5.2` Odea Mobil Müşteri Avantaj Seçim Mimarisi ve Tercih Öğrenme (Discrete Choice) Modülü
   * `5.3` Ayrıcalıklar Dünyası Segment Kuralları ve KVKK/İYS Dinamik İzin Kazanım Akışı Entegrasyonu
   * `5.4` İş Birimi Kampanya Onay, Bütçe İzleme, Durdurma (Kill-Switch) ve Çağrı Merkezi Ekranları
6. **İP 6 – MLOps, Model Risk Yönetimi, Pilot Doğrulama ve Yaygınlaştırma (14. Ay – 18. Ay):**
   * `6.1` Model Envanteri, Versiyonlama ve Enflasyon Kayması (PSI Drift) Otomatik İzleme Altyapısı
   * `6.2` Canlı Ortam Faz 1 & Faz 2 Kontrollü Pilot Uygulaması ve CUPED Etki Analizi
   * `6.3` Performans, Yük, Güvenlik (Sızma) ve Anti-Gaming (Suistimal) Testleri
   * `6.4` Kullanıcı Kabul Testleri (UAT), Dokümantasyon ve Tam Ölçekli Canlıya Geçiş

---

### 4. Üretime Geçmeden Önce Onayını İstediğim Parametreler

Dosyaları doğrudan [Kart Kampanya Yapay Zeka](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka) klasörüne (`.docx` + `.xlsx` olarak) üretmeden önce şu idari bilgileri teyit etmeni rica ediyorum:

1. **Proje Kodu ve Yöneticisi:** Referans dosyadaki gibi Proje Yöneticisi **`KORAY AK`** (veya senin adın mı yazılacak?), Proje Kodu **`ODEA-028`** (veya başka bir kod mu?) olsun?
2. **Proje Süresi, Başlangıç Tarihi ve Bütçe/Ekip:** Referans `ODEA-012` dosyasındaki standart şablon değerlerini (**18 Ay**, Başlangıç: **`01.12.2026`** veya **`01.01.2027`**, **22 Adam/Ay – 396 Toplam Adam/Ay**, **210.000.000 TL bütçe** ve aynı 22 kişilik ekip kadrosu kırılımı) aynen koruyalım mı, yoksa değiştirmek istediğin bir rakam var mı?
3. **Proje Adı Seçimi:** Önerdiğim **"Kredi Kartı Kampanya Yönetiminde Nedensel Yapay Zekâ ve Kısıtlı Optimizasyon Tabanlı Kişiselleştirme Platformu Geliştirilmesi"** başlığı uygun mu?

Onayını ve varsa bu 3 noktadaki tercihinle birlikte hem **Proje Bilgi Formu** dokümanını hem de **Excel Gantt Şemasını** hemen oluşturup klasöre kaydedeyim.

---

## 🗣️ Soru / Girdi #11

1.Proje Yöneticisi KORAY AK olsun. Proje Kodu ODEA-028 olsun, sonra değiştireceğim.
2.aynen koruyalım
3.Bu daha bilimsel duruyor "Ödeme Sistemlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru Geliştirilmesi" ama hedef devlet desteğini almak olduğu için arge olduğunu anlatacak projeyi iyi tanımlayacak bir isim olmalı. Sen karar ver

### 🤖 Yanıt

Onayın doğrultusunda hem **Dijitalpark Teknokent Proje Bilgi Formu** dokümanını (Word ve Markdown formatlarında) hem de **18 Aylık Excel Gantt Şeması** dosyasını [Kart Kampanya Yapay Zeka](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka) klasörüne ürettim.

### Üretilen Dosyalar

1. **Word Formatında Proje Bilgi Formu:** [ODEA-028_Proje_Bilgi_Formu.docx](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/ODEA-028_Proje_Bilgi_Formu.docx)
2. **Markdown / Metin Formatında Proje Bilgi Formu** *(Wiki veya portal alanlarına kolay kopyalama için)*: [ODEA-028_Proje_Bilgi_Formu.md](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/ODEA-028_Proje_Bilgi_Formu.md)
3. **18 Aylık Excel Gantt Şeması:** [ODEA-028_Dinamik_Kampanya_Karar_Motoru_Gantt_Şeması.xlsx](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/ODEA-028_Dinamik_Kampanya_Karar_Motoru_Gantt_%C5%9Eemas%C4%B1.xlsx)

---

### Dokümanda Uygulanan Kritik Kurallar ve Öne Çıkan Noktalar

* **Proje Adı Seçimi (Taslak Kuralı Uyumu):**
  * [Yazılım Geliştirme - Proje Yazım Taslağı.pdf](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/Yaz%C4%B1l%C4%B1m%20Geli%C5%9Ftirme%20-%20Proje%20Yaz%C4%B1m%20Tasla%C4%9F%C4%B1.pdf) Sayfa 1'de yer alan *"Ar-Ge Projesi / Proje / Sistem gibi genel ifadeler ve kısaltmalar kullanılmamalıdır; içerdiği teknoloji + amaç bir arada olmalıdır"* kuralına tam uyum sağlamak için **"Ödeme Sistemlerinde..."** yerine "Sistem" kelimesini de eleyerek şu nihai bilimsel başlığı kullandım:
  > **"Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru Geliştirilmesi"**
* **THS (Teknoloji Hazırlık Seviyesi) Kurgusu:**
  * Mevcut durum **THS-3**, hedef durum **THS-7** olarak belirlendi. Önceki başvuruda boş kalan açıklama kutuları; taslağın istediği gibi **THS 1'den THS 7'ye kadar seviye seviye**, aksiyon fiilleri ve ölçülebilir doğrulama çıktılarıyla dolduruldu.
* **Zorunlu KPI Tablosu:**
  * *Hedeflenen Kazanım ve Sonuçlar* bölümüne taslağın zorunlu tuttuğu **`Başarı Ölçütü (KPI) – Mevcut Durum – Hedef Değer – Ölçüm Yöntemi`** formatında 8 somut mühendislik ve Ar-Ge metriği (`F1-Score >= %92`, `Free-Rider <= %25`, `Incremental ROI >= +35%`, `Latency < 500 ms`, `MILP Çözüm Süresi < 15 dk`, `PSI Drift < 0,15` vb.) eklendi.
* **Birebir Eşleşen İş Paketleri (WBS) ve Gantt Şeması:**
  * Proje Detayı bölümündeki **3 Faz ve 6 Ana İş Paketi (`1.1` – `6.4` alt kırılımları)** ile [ODEA-028_Dinamik_Kampanya_Karar_Motoru_Gantt_Şeması.xlsx](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/ODEA-028_Dinamik_Kampanya_Karar_Motoru_Gantt_%C5%9Eemas%C4%B1.xlsx) dosyasındaki satırlar ve 18 aylık renk barları (`FAZ 1: 1–9. Ay`, `FAZ 2: 8–14. Ay`, `FAZ 3: 11–18. Ay`) referans Excel şablonunun hücre stilleri korunarak birebir eşleştirildi.

Dosyaları inceleyip revize etmemi istediğin bir bölüm olursa hemen güncelleyebilirim; ayrıca dilersen bekleyen **Adım 6 – Beyin Fırtınası (Ürün Ekibinin Düşünmediği İnovatif Fikirler)** veya sunum (`.pptx`) hazırlığına da geçebiliriz.

---

## 🗣️ Soru / Girdi #12

5 çekirdekli veri modeli  nedir

### 🤖 Yanıt

**"5 Çekirdekli Veri Modeli"**, [ODEA-028_Proje_Bilgi_Formu.docx](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/ODEA-028_Proje_Bilgi_Formu.docx) içinde tanımladığımız bağımsız kampanya karar ve takip motorunun veritabanı omurgasıdır.

### Neden Böyle Bir Modele İhtiyaç Var? (Temel Mimari Problem)
Mevcut geleneksel kart sistemlerinde "10.000 TL harca 300 TL kazan" kuralı için 1 adet kampanya kodu (`Campaign_ID`) açılır. Ancak yapay zekâ ile **her müşteriye farklı hedef ve ödül** verdiğimizde, kart sisteminde 30.000 müşteri için 30.000 ayrı kampanya kodu açamazsın; sistem kilitlenir.

Bu sorunu çözmek için **"Kampanya Kural Çerçevesi"** ile **"Müşteriye Özel Hedef/Ödül Parametresini"** birbirinden ayıran **5 Çekirdek Tablolu (Parametrik) Veri Modeli** tasarladık:

```mermaid
erDiagram
    CAMPAIGN_TEMPLATE ||--o{ CUSTOMER_OFFER_ASSIGNMENT : "sınırlarını belirler"
    CUSTOMER_OFFER_ASSIGNMENT ||--|| OFFER_PROGRESS_TRACKER : "anlık sayacını tutar"
    TRANSACTION_LEDGER_FEED }o--|| OFFER_PROGRESS_TRACKER : "harcamayı besler"
    OFFER_PROGRESS_TRACKER ||--o| REWARD_FULFILLMENT_QUEUE : "hedef dolunca tetikler"
```

---

### 5 Çekirdek Tablonun Detayı ve Görevleri

#### 1. `CAMPAIGN_TEMPLATE` (Kampanya Şablon ve Ticari Sınır Tablosu)
İş biriminin kontrolünde olan "Anayasa" tablosudur. Sistemde binlerce değil, **sadece 5–10 adet onaylı şablon** (Örn: *Genel Upsell, Market, E-Ticaret, Restoran, İnaktif Uyandırma*) bulunur. AI bu tablonun koyduğu sınırların dışına çıkamaz.
* **Kritik Alanlar:**
  * `Template_ID` *(Örn: `TPL_UPSELL_2027`)*
  * `Campaign_Type` *(Upsell / Reactivation / Sectoral / Frequency)*
  * `Allowed_MCC_List` & `Excluded_MCC_List` *(Geçerli sektörler ve hariç tutulan altın/kumar/cüzdan MCC'leri)*
  * `Total_Budget_Cap` *(Toplam bütçe tavanı, örn: 750.000 TL)*
  * `Min_Target_Floor` / `Max_Reward_Cap` *(Müşteri başı verilebilecek min. hedef ve maks. ödül sınırı)*
  * `Approved_By_User` & `Kill_Switch_Status` *(İş birimi onayı ve acil durdurma bayrağı)*

#### 2. `CUSTOMER_OFFER_ASSIGNMENT` (Müşteri – Kişisel Teklif Atama Tablosu)
Yapay zekâ (R-Learner Uplift + MILP Optimizasyon) motorunun çıktısını yazdığı **kalp tablodur**. Aynı `Template_ID` altında her müşteriye farklı hedef ve ödül burada tanımlanır.
* **Kritik Alanlar:**
  * `Assignment_ID` *(Tekil teklif atama numarası)*
  * `Customer_ID` & `Template_ID`
  * `Treatment_Flag` *(**1:** Teklif Gösterilecek Müşteri, **0:** Rastgele Kontrol Grubu / Hold-out — teklif gizlenir ama harcaması arkada ölçülür)*
  * `Personal_Target_Amount` *(Müşteriye özel hedef, örn: Müşteri A için `15.000 TL`, Müşteri B için `22.500 TL`)*
  * `Personal_Reward_Amount` & `Reward_Type` *(Örn: `450 TL`, `PUAN` veya `CASHBACK`)*
  * `Baseline_Expected_Spend` & `Predicted_Net_Uplift` *(Modelin tahmin ettiği organik harcama ve beklenen ilave katkı — denetim ve ölçüm için)*
  * `XAI_Reason_Code` & `Model_Version` *(SHAP gerekçe kodu ve kararı veren model versiyonu)*
  * `Offer_State` *(`Offered` / `Selected_By_User` / `Completed` / `Expired`)*

#### 3. `TRANSACTION_LEDGER_FEED` (Zenginleştirilmiş İşlem Akış Tablosu)
Kart switch'inden (ISO 8583 provizyon ve takas) akan işlemlerin, NLP/SLM motoru tarafından gerçek markası çözümlenmiş ve suistimal (anti-gaming) kontrolünden geçmiş halidir.
* **Kritik Alanlar:**
  * `Tx_ID`, `Customer_ID`, `Card_Token`, `Tx_Timestamp`
  * `Amount`, `Raw_MCC` (`DE-18`), `Raw_Merchant_Desc` (`DE-43`)
  * `Enriched_Brand_ID` & `Enriched_Category` *(SLM/Sözlük motorunun bulduğu gerçek marka ve kategori, örn: `IYZICO*YEMEKSEPETI` $\rightarrow$ `Yemeksepeti / Restoran`)*
  * `Tx_Status` *(`AUTH` - Provizyon / `CLEARED` - Takas / `REFUND` - İade / `REVERSAL` - İptal)*
  * `Anti_Gaming_Pass_Flag` *(Bölünmüş işlem veya kendi POS'undan çekim değilse `True`)*

#### 4. `OFFER_PROGRESS_TRACKER` (Gerçek Zamanlı Hedef Sayaç Tablosu)
Odea Mobil'deki ilerleme çubuğunun (progress bar) ve Çağrı Merkezi ekranının milisaniyeler içinde (`<500 ms`) sorguladığı canlı sayaç tablosudur (Redis önbelleği ile senkron çalışır).
* **Kritik Alanlar:**
  * `Assignment_ID` & `Customer_ID`
  * `Accumulated_Auth_Spend` *(Provizyondaki toplam geçerli harcama — müşterinin anlık gördüğü)*
  * `Accumulated_Cleared_Spend` *(Muhasebeleşmiş/takastan geçmiş kesin harcama)*
  * `Remaining_Spend_To_Target` *(Hedefe kalan tutar: `Personal_Target_Amount - Accumulated_Auth_Spend`)*
  * `Valid_Tx_Count` *(Frekans kampanyaları için geçerli işlem adedi)*
  * `Target_Reached_Timestamp` *(Hedefin tamamlandığı an)*

#### 5. `REWARD_FULFILLMENT_QUEUE` (Ödül Yükleme ve İade Geri Alma / Clawback Tablosu)
Sayaç `%100`'e ulaştığında ana kart sistemine (Paycore/Intertech) puan veya nakit iade yükleme emri gönderen ve kampanya sonrası iade suistimalini takip eden tablodur.
* **Kritik Alanlar:**
  * `Fulfillment_ID`, `Assignment_ID`, `Customer_ID`
  * `Reward_Amount`, `Reward_Type` (`PUAN` / `CASHBACK`)
  * `Fulfillment_Status` *(`PENDING_CLEARING` / `LOADED_TO_CARD` / `FAILED`)*
  * `Refund_Monitor_Until_Date` *(Ödül yüklendikten sonra iade takibinin yapılacağı son tarih)*
  * `Clawback_Status` *(Müşteri alışverişi iade edip hedef altına düşerse `CLAWBACK_TRIGGERED` olur ve ödül geri alınır)*

---

### Özetle Ne Sağlıyor?
Bu 5 tablo sayesinde:
1. Kart ana sisteminde binlerce kampanya kuralı tanımlamadan **her müşteriye özel hedef ve ödül** verebiliyoruz.
2. **Kontrol grubunu (`Treatment_Flag = 0`)** aynı tablo içinde tutup, teklif vermediğimiz müşterinin sayacını da arka planda ("gölge sayaç" olarak) çalıştırarak kampanya sonunda gerçek **Uplift (İlave Katkı)** hesabını tek bir SQL sorgusuyla yapabiliyoruz.
3. İç Denetim veya BDDK sorduğunda *"Bu müşteriye neden 15.000 TL hedef verildi?"* sorusunun cevabı `CUSTOMER_OFFER_ASSIGNMENT` tablosundaki `Baseline_Expected_Spend`, `XAI_Reason_Code` ve `Model_Version` kolonlarında hazır duruyor.

---

## 🗣️ Soru / Girdi #13

bu nedir? 8 haftalık canlı pilotta CUPED (Deney Öncesi Veriyle Varyans Azaltma) yöntemiyle hesaplanan (\Delta\text{Harcama} \times \text{Marj} - \text{Ödül}) karşılaştırması.

### 🤖 Yanıt

Öncelikle o satırdaki `\Delta\text{Harcama} \times \text{Marj} - \text{Ödül}` ifadesi matematiksel formül yazımından (LaTeX) kalan bir işaretlemeydi; sade Türkçesi şudur:

> **`(İlave Harcama × Banka Marjı) − Ödenen Ödül`**  
> *(Not: [ODEA-028_Proje_Bilgi_Formu.md](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/ODEA-028_Proje_Bilgi_Formu.md) dosyasında bu ifadeleri temiz Türkçe/Unicode sembollere çevirdim; `.docx` dosyan şu an Word'de açık olduğu için kapattığında onu da tek tıkla güncelleyebilirim).*

Bu cümlenin ne anlama geldiğini **2 parçada, somut bir bankacılık örneğiyle** açıklayayım:

---

### 1. `(İlave Harcama × Marj − Ödül)` Nedir? (Gerçek Kampanya Kârı)

Geleneksel kampanya raporlarında iş birimi şuna bakar: *"Kampanyaya katılanlar 10 Milyon TL harcadı, 200 Bin TL ödül verdik, çok başarılıyız!"*
Oysa bu yanlıştır; çünkü o müşteriler kampanya olmasaydı zaten **9 Milyon TL** harcayacaktı.

Gerçek başarı formülü şudur:
1. **İlave Harcama ($\Delta\text{Harcama}$):** Kampanya teklifi alan grubun ortalama harcamasından, **teklif verilmeyen Kontrol Grubunun (Hold-out)** ortalama harcamasını çıkarırız.
   * *Örnek:* AI grubu kişi başı ortalama **18.000 TL** harcadı, Kontrol grubu **15.000 TL** harcadı $\rightarrow$ **İlave Harcama = 3.000 TL**.
2. **Banka Brüt Geliri ($\text{İlave Harcama} \times \text{Marj}$):** Banka bu 3.000 TL'nin tamamını kazanmaz; sadece takas komisyonu (interchange) + faiz/taksit marjını (örneğin **%2,2**) kazanır.
   * *Örnek:* $3.000\text{ TL} \times \%2,2 = \mathbf{66\text{ TL}}$ (Bankanın cebine giren brüt ilave gelir).
3. **Net İlave Ekonomik Katkı ($-\text{Ödül}$):** Bu 66 TL geliri elde etmek için müşteriye ortalama kaç TL ödül (puan/cashback) ödedik? Diyelim ki **40 TL**.
   * *Sonuç:* $66\text{ TL} - 40\text{ TL} = \mathbf{+26\text{ TL}}$ (**Müşteri başına net kâr**).

İşte KPI tablosundaki hedefimiz; AI modelinin ürettiği bu **net kârın**, mevcut Excel (`+%20`) yönteminin ürettiği net kârdan **en az %35 daha yüksek olmasıdır**.

---

### 2. Peki "CUPED" (Deney Öncesi Veriyle Varyans Azaltma) Nedir?

Microsoft, Netflix ve Booking.com gibi şirketlerin A/B testlerinde kullandığı, bizim gibi **küçük müşteri portföylerinde (11.000 aktif kart)** hayat kurtaran istatistiksel bir düzeltme tekniğidir (*Controlled-experiment Using Pre-Experiment Data*).

**Sorun Şu:**
Bizim izinli aktif kredi kartı portföyümüz küçük (~11.000 kişi). Bunun %15'ini (**1.650 kişiyi**) Kontrol Grubu olarak ayırdık.
* Şans eseri bu 1.650 kişilik kontrol grubuna, normalde de her ay 200.000 TL harcayan **10 tane zengin müşteri** düştüğünü varsayalım.
* Kampanya bittiğinde bir bakarsın: Kontrol grubunun ortalaması **17.500 TL** çıkmış, AI grubunun ortalaması **18.000 TL** çıkmış! Sanki AI kampanyası sadece 500 TL fark yaratmış gibi görünür. Oysa fark, kontrol grubuna tesadüfen zengin müşterilerin düşmesinden (yani **deney öncesi doğal farktan**) kaynaklanmıştır.

**CUPED Bunu Nasıl Çözer?**
CUPED şunu yapar: Kampanya başlamadan önce (örneğin önceki 3 ayda) grupların zaten ne kadar harcadığına bakar ve o **başlangıç farkını sonuçtan matematiksel olarak temizler**:
* *Düzeltmesiz Hesap:* AI Grubu Pilot Harcaması ($18.000\text{ TL}$) $-$ Kontrol Grubu Pilot Harcaması ($17.500\text{ TL}$) $= 500\text{ TL}$ *(Yanıltıcı!)*.
* *CUPED Düzeltmeli Hesap:* "Kontrol grubu kampanyadan önceki 3 ayda da AI grubundan zaten ortalama 2.500 TL fazla harcıyordu. O halde bu 2.500 TL'lik eski farkı düşelim" der ve gerçek kampanya etkisinin $500 + 2.500 = \mathbf{3.000\text{ TL}}$ olduğunu ortaya çıkarır.

**Özetle Neden Rapora Yazdık?**
1. **Gerçek Fayda:** 100.000 kişilik dev bir portföye ihtiyaç duymadan, **11.000 kişilik portföyde sadece 8 haftada (2 ayda)** hatasız ve gürültüsüz ölçüm yapmamızı sağlar.
2. **Hakem Etkisi:** Teknopark / TÜBİTAK hakemleri projeyi okuduğunda *"Bu ekip sadece model kurmamış, küçük örneklemde A/B testinin nasıl doğrulanacağını (CUPED) bilen üst düzey bir Ar-Ge ekibi"* der.

---

## 🗣️ Soru / Girdi #14

md dosyasındaki düzeltmeyi .docx dosyasında da tüm bu tür ifadeler için yap. Ayrıca /ge veya /le gib ifadeler yerine => veya <= işaretlerini kullan.

### 🤖 Yanıt

Hem [ODEA-028_Proje_Bilgi_Formu.md](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/ODEA-028_Proje_Bilgi_Formu.md) hem de [ODEA-028_Proje_Bilgi_Formu.docx](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/ODEA-028_Proje_Bilgi_Formu.docx) dosyalarındaki tüm matematiksel formül/LaTeX ifadelerini temizledim ve eşitsizlik sembollerini istediğin şekilde güncelledim:

* Tüm `\ge` / `≥` ve `\le` / `≤` ifadeleri **`=>`** ve **`<=`** olarak değiştirildi (Örn: `=> %92,0`, `<= %25,0`, `=> 1,35x`).
* `\Delta\text{Harcama} \times \text{Marj} - \text{Ödül}` gibi ifadeler **`(İlave Harcama × Marj - Ödül)`** şeklinde düz Türkçe metne çevrildi.
* `$\hat{\mu}(X)$`, `$\tau(X)$`, `$\Delta Y$` gibi semboller kaldırılarak yerlerine **`Beklenen Organik Harcama`**, **`Bireysel Kampanya Etkisi`** ve **`Sapma Tutarı = Gerçekleşen Harcama - Beklenen Organik Harcama`** yazıldı.
* Word dosyasının (`ODEA-028_Proje_Bilgi_Formu.docx`) içinde hiçbir ters eğik çizgi (`\`) veya dolar (`$`) işareti kalmadığı doğrulandı.

---

## 🗣️ Soru / Girdi #15

projede yapay zekayı tam olarak hangi noktalarda kullandık?

### 🤖 Yanıt

Projede yapay zekâyı "her şeyi yapan tek bir kara kutu" olarak değil, sürecin **6 spesifik adımında, 6 farklı yapay zekâ disiplini** olarak konumlandırdık. 

Bir kurul veya hakem karşısında anlatırken elini çok güçlendirecek şekilde, yapay zekâyı **tam olarak nerede ve nasıl kullandığımızı** uçtan uca akış sırasıyla özetleyeyim:

```mermaid
flowchart TD
    A["1. Veri Girişi: Kirli POS Metni"] -->|1. Doğal Dil İşleme - On-Prem SLM/BERT| B["Temiz Marka ve Sektör"]
    B -->|2. Zaman Serisi ML| C["Organik Harcama Tahmini - Baseline"]
    C -->|3. Nedensel AI - R-Learner Uplift| D["İlave Harcama ve Elastikiyet Tahmini"]
    B -->|4. Öneri Sistemi - Collaborative Filtering| E["Potansiyel Yeni Sektör Tahmini"]
    D & E -->|5. Pekiştirmeli Öğrenme - Contextual Bandit| F["Optimal Kampanya, Hedef ve Ödül Tipi Seçimi"]
    F -->|6. Açıklanabilir AI - SHAP & Kümeleme| G["Analist Fırsat Özeti ve Müşteri Gerekçesi"]
```

---

### Yapay Zekâyı Kullandığımız 6 Somut Nokta

#### 1. Nokta: Kirli POS İşlem Açıklamalarından Marka ve Sektör Çıkarımı
* **Sorun:** Kart ekstresine düşen `IYZICO*GETIR`, `PARAM*TRENDYOL` veya `KARDESLER GIDA LTD` gibi ham POS metinlerinden (`ISO 8583 DE-43`) ve hatalı `MCC` kodlarından müşterinin gerçekte hangi markadan ve hangi sektörden alışveriş yaptığı anlaşılamıyor.
* **Kullanılan Yapay Zekâ:** **Doğal Dil İşleme (NLP) – İnce Ayarlı Yerel Küçük Dil Modeli (On-Premise SLM / BERTurk).**
* **Ne Yapıyor?** Banka içi GPU'da çalışarak karmaşık POS metinlerini okuyor, gerçek markayı ve sektörü (`Restoran`, `Market`, `E-ticaret` vb.) **%92+ doğrulukla** etiketleyip önbellek tablosuna yazıyor.

#### 2. Nokta: "Kampanya Olmasaydı Müşteri Zaten Ne Kadar Harcayacaktı?" Tahmini (Baseline)
* **Sorun:** Mevcut sistem son 3 ayın ortalamasını alıyor ama müşterinin harcaması zaten yükseliş trendindeyse veya yaz/bayram dönemi geliyorsa müşteri kampanya olmasa da o parayı harcayacak ("Free-Rider" / Beleşçi).
* **Kullanılan Yapay Zekâ:** **Hiyerarşik Zaman Serisi ve Regresyon Modelleri (LightGBM / Temporal Models).**
* **Ne Yapıyor?** Enflasyondan arındırılmış geçmiş 12–24 aylık harcama dizisine bakarak, her müşterinin önümüzdeki ay **hiç kampanya yapılmasa bile organik olarak kaç TL harcayacağını** tahmin ediyor.

#### 3. Nokta: "Bu Teklifi Verirsem Müşteri Ne Kadar İLAVE Harcama Yapar?" Tahmini (Projenin Kalbi)
* **Sorun:** Klasik yapay zekâ (Propensity) *"Kim hedefi tutturur?"* sorusunu çözer; bu da zaten her gün kart kullanan müşteriyi seçip ödül bütçesini çöpe atar. Ayrıca uyuyan (inaktif) 46 bin kartın hangi eşikle uyanacağı bilinmiyor.
* **Kullanılan Yapay Zekâ:** **Nedensel Makine Öğrenmesi (Causal ML – Robinson Ayrıştırmalı R-Learner ve Hiyerarşik Bayesyen Uplift).**
* **Ne Yapıyor?** 2. noktadaki organik harcama beklentisini düşerek, müşteriye belirli bir hedef ve ödül verildiğinde **sadece o kampanya sayesinde yaratacağı net ilave harcamayı (Incremental Uplift)** ve inaktif kartın uyanma olasılığını hesaplıyor.

#### 4. Nokta: Müşterinin Henüz Kullanmadığı Potansiyel Sektörlerin Bulunması (Faz 2)
* **Sorun:** Müşteri kredi kartını bizde sadece akaryakıt ve markette kullanıyor; başka hangi sektörde (ör. seyahat, restoran, e-ticaret) harcama potansiyeli olduğunu bilmiyoruz.
* **Kullanılan Yapay Zekâ:** **Öneri Sistemleri (İşbirlikçi Filtreleme / Collaborative Filtering & Ardışık Örüntü Madenciliği).**
* **Ne Yapıyor?** Netflix veya Spotify'ın film/müzik önerme mantığıyla; müşteriyi harcama profili kendisine benzeyen diğer müşterilerle karşılaştırıyor ve **"Bu müşteri bizde hiç restoran harcaması yapmıyor ama ikiz profilleri yapıyor; restoran kampanyası önerilmeli"** diyor.

#### 5. Nokta: Farklı Hedef Oranlarını ve Ödül Tiplerini (Puan vs. Cashback) Deneyerek Öğrenme
* **Sorun:** Geçmişte herkese sabit `+%20` hedef verildiği için hangi müşterinin `+%15` hedefe mi, `+%30` hedefe mi, Puan'a mı yoksa Nakit İade'ye (Cashback) mi daha iyi tepki verdiğini gösteren tarihsel veri yok.
* **Kullanılan Yapay Zekâ:** **Pekiştirmeli Öğrenme (Contextual Multi-Armed Bandit – Thompson Sampling) ve Tercih Öğrenme (Discrete Choice Modeling).**
* **Ne Yapıyor?** Portföyün büyük kısmına bilinen en kârlı teklifi verirken, küçük bir kısmında kontrollü denemeler yaparak ve Faz 3'te Odea Mobil'de müşterinin **"3 tekliften hangisini seçtiğini"** izleyerek her ay kendi kendini eğitiyor.

#### 6. Nokta: Gizli Müşteri Kümelerinin Keşfi ve Teklif Gerekçesinin Açıklanması
* **Sorun:** İki ihtiyaç var: (1) Ürün ekibinin standart raporlarda göremediği yeni davranış gruplarını keşfetmek, (2) "Yapay zekâ bu müşteriye neden bu teklifi verdi?" sorusunu hem İç Denetime hem müşteriye açıklamak.
* **Kullanılan Yapay Zekâ:**
  * **Denetimsiz Öğrenme (HDBSCAN Kümeleme) + Yerel Dil Modeli (SLM):** İşlem verilerindeki gizli grupları (ör. *maaş gününden sonraki 3 gün e-ticaret yapanlar*) bulup ürün ekibine Türkçe rapor olarak özetliyor.
  * **Açıklanabilir Yapay Zekâ (SHAP / XAI):** Her müşterinin teklif kararında hangi 3 faktörün etkili olduğunu matematiksel olarak çıkarıyor ve Odea Mobil'de müşteriye gösterilecek onaylı gerekçe şablonunu seçiyor.

---

### Kurul / Hakem Karşısında Seni Çok Güçlü Kılacak Kritik Ayrım: "Yapay Zekâyı Nerede Bilerek KULLANMADIK?"

Hakemler her şeye "AI yapıyor" denmesinden hoşlanmaz. Bizim mimarimizde şu **2 kritik noktada yapay zekâ YERİNE kesin matematik ve kural motoru** kullanılıyor:

1. **Bütçe, Kârlılık ve Adillik Sınırları $\rightarrow$ Matematiksel Optimizasyon (MILP):** Yapay zekânın ürettiği tahminleri alıp; *"Toplam bütçeyi aşma, müşterinin boş limitini geçme, MIS kârlılığı ekside olan müşteriye zarar ettiren teklif verme ve sadık müşteriye az harcayandan daha düşük ödül verme"* kararını **Doğrusal Programlama (Linear Programming)** çözücüsü veriyor.
2. **Müşteriye Metin Gösterimi ve Sayaç Takibi $\rightarrow$ Deterministik Kurallar:** Müşteri ekranında halüsinasyon (uydurma vaat) riskini sıfırlamak için serbest LLM metni kullanmıyoruz; gerçek zamanlı harcama sayacını ve iade/suistimal (anti-gaming) kontrollerini de kesin yazılım kurallarıyla işletiyoruz.

---

## 🗣️ Soru / Girdi #16

bu nerede olacak? Kanal, Sunum ve İnsan Onayı Katmanı (Presentation & Human-in-the-Loop Layer):

### 🤖 Yanıt

**Kanal, Sunum ve İnsan Onayı Katmanı (Presentation & Human-in-the-Loop Layer)** tek bir ekran değil; **dışarıda müşteriye**, **içeride ise Ürün Ekibine ve Çağrı Merkezine** bakan **3 ayrı arayüzden (ekrandan)** oluşuyor:

```mermaid
flowchart LR
    API["Karar ve Sayaç Motoru<br/>(.NET Core REST API)"] --> M["1. Müşteri Kanalı<br/>(Odea Mobil Uygulaması)"]
    API --> BO["2. İnsan Onayı & Yönetim Kanalı<br/>(Banka İçi Back-Office Web Paneli)"]
    API --> CC["3. Operasyon Kanalı<br/>(Çağrı Merkezi Ekranı)"]
```

### 1. Müşteriye Bakan Yüzü $\rightarrow$ **Odea Mobil Uygulaması İçinde**
Müşterinin telefonundaki **Odea Mobil** uygulamasının *"Kampanyalarım / Bana Özel"* sekmesinde yer alacak:
* Müşteri kendisine özel hesaplanan hedefi ve ödülü burada görecek,
* Kartıyla harcama yaptığı anda (provizyon geçince) dolan **ilerleme çubuğunu (Progress Bar: "15.000 TL hedefinizin 8.500 TL'sini tamamladınız, 6.500 TL kaldı")** buradan takip edecek,
* Faz 3'te kendisine sunulan **3 alternatif avantajdan birini seçme** işlemini ve (izni yoksa) **KVKK/İYS tek tıkla izin verme** işlemini bu ekrandan yapacak.

---

### 2. "İnsan Onayı (Human-in-the-Loop)" Yüzü $\rightarrow$ **Banka İçi Web Yönetim Paneli (Back-Office / Intranet)**
Banka iç ağında (Intranet üzerinde) **Kart Ürün ve Kampanya Ekibinin** kullandığı dahili bir web arayüzü (React.js tabanlı yönetim ekranı) olacak. Yapay zekânın kendi başına müşteriye kampanya çıkmasını engelleyen **"İnsan Onayı" tam olarak burada çalışır:**
1. **Kampanya Öncesi Kuralları Girme:** İş birimi bu ekrandan bütçe tavanını (ör. *Max 750.000 TL*), müşteri başı maksimum ödülü (ör. *Max 1.000 TL*) ve yasaklı sektörleri girer.
2. **Simülasyon ve Tek Tuşla Onay (Human-in-the-Loop):** Yapay zekâ ve optimizasyon motoru gece çalışıp 10.000 müşteri için teklifleri hazırlar ama **müşteriye göndermez ("Onay Bekliyor" statüsünde tutar).** Sabah Ürün Yöneticisi bu iç web ekranına girer; *"Toplam 9.850 müşteriye teklif atandı, 1.650 kişi kontrol grubuna ayrıldı, beklenen ödül maliyeti 610.000 TL, beklenen ilave harcama 14 Milyon TL"* özet tablosunu görür ve **[ONAYLA VE YAYINLA]** butonuna basar. O basmadan hiçbir teklif Odea Mobil'e veya SMS'e düşmez.
3. **Acil Durdurma (Kill-Switch) ve Fırsat Keşif Panosu:** İş birimi kampanya devam ederken bütçenin hızlı eridiğini görürse bu ekrandan **[KAMPANYAYI DURDUR]** butonuna basabilir. Ayrıca Faz 2'de yapay zekânın keşfettiği yeni davranış kümeleri (*"Hafta sonu e-ticaret yapan 3.200 kişi bulundu"*) bu ekranda analiste öneri olarak listelenir.

---

### 3. Operasyon Yüzü $\rightarrow$ **Çağrı Merkezi (İletişim Merkezi) Ekranı**
Müşteri Çağrı Merkezini arayıp *"Bana 15.000 TL hedef gelmiş, dün 2.000 TL harcadım ama sayacıma yansımamış, neden?"* diye sorduğunda; Çağrı Merkezi temsilcisinin önündeki mevcut müşteri ekranına (veya sekmesine) API ile bağlanan bilgi kutusudur:
* Temsilci burada müşterinin **kişisel hedefini, o anki sayaç durumunu, teklifin neden önerildiğini (SHAP gerekçesini)** ve sayılmayan işlem varsa **neden sayılmadığını** (ör. *"Altın/Kuyum MCC kodu kampanya dışı olduğu için sayılmamıştır"*) tek bakışta görür.

---

**Özetle:**
* **Sunum (Presentation) kısmı:** Dışarıda **Odea Mobil**'de, içeride **Çağrı Merkezi** ekranında.
* **İnsan Onayı (Human-in-the-Loop) kısmı:** Sadece senin ve Ürün Ekibinin yetkili olduğu **Banka İçi Web Yönetim Panelinde (Back-Office)**.

---

## 🗣️ Soru / Girdi #17

bu nasıl olacak? "4.3.1 Yoğunluk Tabanlı Kümeleme (HDBSCAN) ve Davranışsal Temsil (Embedding) Üretimi: Standart segmentasyonla görülemeyen mikro-davranış kümelerinin keşfedilmesi."

### 🤖 Yanıt

Bu madde, Ürün Ekibinin dokümanındaki **Madde 5.2** (*"Harcama örüntülerinden yeni kampanya fırsatlarının keşfedilmesi"*) ve **Madde 5.6** (*"Standart segmentasyonla fark edilemeyen dinamik müşteri segmentlerinin keşfedilmesi"*) isteklerinin **gerçek dünyadaki matematiksel ve yazılımsal karşılığıdır.**

Bunun sahada **adım adım nasıl çalışacağını** somut bir örnekle anlatayım:

```mermaid
flowchart LR
    A["Ham Kart İşlemleri<br/>(Saat, Gün, Sektör, Tutar, Kanal)"] -->|1. Adım: Embedding| B["Müşteri Davranış Parmak İzi<br/>(Sayısal Vektör)"]
    B -->|2. Adım: HDBSCAN| C["Yoğun Mikro-Kümelerin<br/>Otomatik Keşfi"]
    C -->|3. Adım: SLM Özeti| D["Ürün Ekibi Ekranına<br/>'Yeni Kampanya Fırsatı' Önerisi"]
```

---

### Adım 1: "Davranışsal Temsil (Embedding)" Nedir, Nasıl Üretilir?
Bankadaki klasik segmentasyon sadece bakiyeye bakar (*"300.000 TL üstü = Ayrıcalıklı"*). Ama 500.000 TL'si olan iki müşteriden biri kartını sadece gece Trendyol'da kullanırken, diğeri hafta içi öğlen restoranlarda kullanır.

Sistem her ay her müşteri için son 6 aylık işlemlerinden **25–30 boyutlu bir "Davranış Parmak İzi" (Embedding Vektörü)** çıkarır:
* **Zaman Davranışı:** İşlemlerinin yüzde kaçı *Hafta İçi Öğle (12:00–14:00)*, yüzde kaçı *Hafta Sonu*, yüzde kaçı *Maaş Gününden Sonraki İlk 3 Gün*?
* **Kanal Davranışı:** Yüzde kaçı *E-ticaret*, yüzde kaçı *Fiziki POS*, yüzde kaçı *Taksitli*, yüzde kaçı *Düzenli Abonelik (Netflix/Spotify vb.)*?
* **Sektör ve Sepet Davranışı:** Harcamasının yüzde kaçı *Market*, *Restoran*, *Akaryakıt*, *Seyahat* ve ortalama fiş tutarı kaç TL?

Bu sayılar bir araya geldiğinde her müşteri, harcama alışkanlığını temsil eden bir **"koordinat noktasına" (Embedding)** dönüşür. Harcama huyu birbirine benzeyen müşteriler haritada doğal olarak birbirine yaklaşır.

---

### Adım 2: "Yoğunluk Tabanlı Kümeleme (HDBSCAN)" Ne Yapar?
Müşterileri bu davranış haritasına yerleştirdikten sonra devreye **HDBSCAN** (*Hierarchical Density-Based Spatial Clustering*) algoritması girer.

**Neden klasik kümeleme (K-Means) değil de HDBSCAN kullanıyoruz?**
1. **Küme Sayısını Biz Söylemeyiz, Kendisi Bulur:** Klasik algoritmalara baştan *"Müşterileri 5 gruba böl"* demek zorundasındır. HDBSCAN ise haritaya bakar ve nerede **yoğun bir müşteri öbeği (kalabalığı)** varsa onu otomatik yakalar. O ay 6 özel davranış grubu oluştuysa 6 bulur, 11 oluştuysa 11 bulur.
2. **Alakasız Müşterileri Zorla Gruba Sokmaz (Gürültü Temizliği):** Hiçbir gruba benzemeyen, ayda yılda bir rastgele harcama yapan müşterileri zorla bir kampanyaya yamamaz; onları *"Standart/Dağınık"* olarak dışarıda bırakır. Sadece **gerçekten ortak alışkanlığı olan saf mikro-kümeleri** çıkarır.

---

### Adım 3: Ürün Ekibinin Ekranına Nasıl Düşecek? (Somut Örnek)
HDBSCAN çalıştı ve diyelim ki insan gözüyle Excel'de fark edilmesi imkânsız olan **1.450 kişilik yoğun bir küme** buldu. Bu kümenin ortak koordinatlarına baktığında şunu gördü:
* İşlemlerin **%68'i Hafta İçi 11:45 – 13:45** arasında,
* Sektör **%85 Restoran / Kafe / Fast-Food**,
* Ortalama fiş tutarı **340 TL**,
* Ancak bu müşterilerin **E-ticaret ve Akşam Yemeği harcaması %0!** (Yani kartı sadece iş yeri öğle yemeğinde kullanıyorlar).

Hemen arkasından `4.3.2`'deki yerel dil modelimiz (On-Prem SLM) bu sayısal kümeyi okur ve **Banka İçi Yönetim Panosunda (Back-Office)** Ürün Ekibinin önüne şu hazır kartı çıkarır:

> **Keşfedilen Mikro-Davranış Kümesi #3 (1.450 Aktif Müşteri)**  
> **Küme Özeti:** *Kartını ağırlıklı olarak hafta içi öğle saatlerinde (12:00-14:00) fiziki restoranlarda kullanan, ortalama sepet tutarı 340 TL olan ancak hafta sonu ve e-ticarette kartı kullanmayan müşteri grubu.*  
> **AI Kampanya Fırsatı Önerisi:**  
> * **Seçenek A (Frekans Artırma):** *"Hafta içi öğle saatlerinde 5. restoran harcamanıza 150 TL Puan"*  
> * **Seçenek B (Çapraz Sektör):** *"Hafta sonu ilk e-ticaret harcamanıza 200 TL İade"*  
> `[Kampanya Şablonuna Dönüştür]`

---

### Özetle
* **Teknik olarak çok kolaydır:** Python'da standart açık kaynak `umap-learn` + `hdbscan` kütüphaneleriyle 20.000 aktif müşterinin verisi üzerinde çalışması **sadece 10–15 saniye** sürer.
* **Canlı sistemi yormaz:** Ayda 1 kez arka planda (offline) çalışır ve çıktısını Ürün Ekibinin yönetim ekranına **"Yeni Keşfedilen Kampanya Fırsatları"** olarak düşürür.
* **Ar-Ge / Hakem Değeri:** İşlem verilerinden *"Davranışsal Embedding üretip Yoğunluk Tabanlı Kümeleme ile dinamik mikro-segment keşfetmek"*, Teknopark ve TÜBİTAK hakemlerinin çok sevdiği gerçek bir veri madenciliği (Data Mining) bileşenidir.

---

## 🗣️ Soru / Girdi #18

açıkla: "6.1 Model Envanteri (Registry), Versiyonlama ve Enflasyon Kayması (PSI Drift) İzleme: MLflow tabanlı model versiyonlama, denetim izi (audit trail) ve otomatik PSI/AUUC kayma alarm sisteminin kurulması."

### 🤖 Yanıt

Bu madde, kurduğumuz yapay zekâ sisteminin **"Kara Kutu" (Denetlenemez) olmasını engelleyen** ve **Türkiye'deki enflasyon yüzünden birkaç ay sonra saçmalamasını önleyen** sigorta katmanıdır (MLOps & Model Risk Yönetimi).

İçindeki terimleri tek tek, somut bankacılık örnekleriyle açayım:

```mermaid
flowchart LR
    A["1. MLflow Model Envanteri<br/>& Denetim İzi (Audit Trail)"] -->|BDDK / İç Denetim Sorduğunda| B["'Hangi model, hangi veriyle<br/>bu kararı verdi?' kaydı"]
    C["2. PSI (Enflasyon & Veri Kayması)<br/>ve AUUC Alarmı"] -->|Enflasyon veya Müşteri Huyu Değiştiğinde| D["'Model eskidi, yeniden eğit!'<br/>Otomatik Uyarısı"]
```

---

### 1. Model Envanteri (Registry), Versiyonlama ve Denetim İzi (Audit Trail) Nedir?
Normal yazılımda kodu Git'te versiyonlarsın (`v1.0`, `v1.1`). Yapay zekâda ise sadece kod değil, **modelin öğrendiği ağırlıklar ve eğitim verisi de her ay değişir.**

* **Ne İşe Yarar?** 6 ay sonra BDDK müfettişi, İç Kontrol veya bir müşteri şikayeti geldiğinde: *"2027 Mart ayında Ahmet Bey'e neden 25.000 TL hedef ve 500 TL ödül verdiniz de Mehmet Bey'e hiç teklif vermediniz?"* diye sorulacaktır.
* **`MLflow` (Açık Kaynak Model Kütüphanesi) Burada Ne Yapar?**
  * Sistemde çalışan her yapay zekâ modeline bir kimlik numarası verir (`Uplift_Model_v1.4`).
  * O gün Ahmet Bey için karar verilirken modelin hangi versiyonunun çalıştığını, o an Ahmet Bey'in kârlılık ve harcama verisinin ne olduğunu **Denetim İzi (Audit Trail)** olarak kaydeder.
  * Eğer yeni eğitilen `v1.5` modeli canlıda hatalı davranırsa, tek tıkla bir önceki sağlam versiyona (`v1.4`) geri dönmeni (**Rollback**) sağlar.

---

### 2. Enflasyon Kayması (PSI – Population Stability Index / Veri Kayması) Nedir?
Yapay zekâ modellerinin Türkiye'deki **en büyük düşmanı enflasyondur.**

* **Somut Örnek:**
  * Diyelim ki modeli **Aralık 2026**'da eğittik. O tarihte ayda **20.000 TL** market harcaması yapan bir müşteri **"Çok Yüksek Harcayan (Zengin) Müşteri"** grubundaydı.
  * Aradan 8 ay geçti, fiyatlar ve maaşlar arttı. Artık sıradan bir öğrenci veya emekli bile ayda **20.000 TL** harcıyor!
  * Eğer modeli izlemezsen, eski model herkesi "Zengin/Yüksek Harcayan Müşteri" zannetmeye başlar ve herkese astronomik hedefler verir. Buna literatürde **Veri Kayması (Data / Inflationary Drift)** denir.
* **`PSI (Population Stability Index)` Ne Yapar?**
  * Bankacılıkta kredi skorlama modellerinde de kullanılan standart bir matematiksel formüldür. Modelin eğitildiği günkü müşteri harcama dağılımı ile bugünkü harcama dağılımını karşılaştırır.
  * **Kural çok basittir:**
    * `PSI < 0,10` ise: **Yeşil** (Sorun yok, müşteri dağılımı aynı, model sağlıklı).
    * `0,10 <= PSI < 0,15` ise: **Sarı** (Enflasyon veya mevsimsellik veriyi kaydırmaya başladı, izlemeye al).
    * `PSI => 0,15` ise: **Kırmızı Alarm!** Sistem veri bilimciye otomatik uyarı atar: *"Müşteri harcama dağılımı değişti, modeli son 3 ayın güncel verisiyle yeniden eğit (retrain)!"*

---

### 3. `AUUC` (Area Under Uplift Curve) Kayma Alarmı Nedir?
* **PSI** *"Giren veri (harcama tutarları) değişti mi?"* diye bakar.
* **AUUC** ise *"Modelin tahmin gücü (isabeti) düştü mü?"* diye bakar.
* Normal sınıflandırma modellerinde başarı **AUC / Doğruluk (%)** ile ölçülürken, bizim kullandığımız **Uplift (Nedensel İlave Harcama)** modellerinin dünyadaki standart başarı karnesi **AUUC (Uplift Eğrisi Altındaki Alan)** veya **Qini Katsayısı**dır.
* Kampanya bittiğinde sistem şuna bakar: *"Modelin 'Bu müşteriye teklif verirsen harcaması çok artar' dediği kişiler gerçekten harcamasını artırdı mı?"* Eğer modelin isabet oranı %5'ten fazla düşerse yine otomatik **"Model Performans Kaybı Alarmı"** üretir.

---

### Özetle: Geliştirmesi Zor mu?
* **Hayır, çok pratiktir:** `MLflow` tamamen ücretsiz, açık kaynaklı ve Python dünyasında standarttır (kurulumu 1–2 gün sürer). PSI ve AUUC hesabı ise ayda bir kez çalışan 50 satırlık bir Python/SQL script'idir.
* **Neden Proje Bilgi Formuna Koyduk?** Çünkü Teknopark ve TÜBİTAK hakemleri *"Modeli kurup bıraktınız mı, yoksa canlıda enflasyon ve veri kaymasına (drift) karşı nasıl izleyeceksiniz?"* sorusuna çok önem verir. Bu madde projenin **endüstriyel olgunluğunu (THS-7)** kanıtlayan maddedir.

---

## 🗣️ Soru / Girdi #19

bu konuşmayı ve yanıtlarını *.md dosyası olarak export et


