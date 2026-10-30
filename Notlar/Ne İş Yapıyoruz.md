# Ne İş Yapıyoruz? — House of Cards (HoC) Ekibi İş Tanımları ve Sorumluluk Alanları

> **Ekip:** House of Cards (HoC) – Kartlı Ödeme Sistemleri ve ATM  
> **İlgili İş Kolu:** Bireysel Bankacılık Ödeme Sistemleri  
> **Kapsam:** Bireysel & Ticari Kredi Kartları, Bireysel & Ticari Debit Kartlar, BaaS / SME (Ödeal & TYF), Ayrıcalıklar Dünyası (Loyalty), QR Ödeme Çözümleri ve ATM Kanalı  

---

## 1. Yönetici Özeti: Ekip Misyonu ve Temel Rolümüz

**House of Cards (HoC)** ekibi olarak; bankanın kartlı ödeme sistemleri, sadakat programları, alternatif ödeme yöntemleri (QR, Cüzdan, BaaS/SME ortaklıkları) ve ATM kanalındaki tüm ürünlerin **uçtan uca yaşam döngüsünü** yönetiyoruz. 

Ekibimiz yalnızca iş birimi taleplerini analiz etmekle kalmayıp; **Inhouse Backend (BE)** ve **Frontend (FE - Mobil/Web)** geliştirmelerini bizzat yapmakta, **7+ dış teknoloji ortağı (Vendor)** ile **10+ iç/dış kurum entegrasyonunun** teknik orkestrasyonunu yürütmektedir.

### Yatayda Yürüttüğümüz 4 Temel Fonksiyon:
1. **İş ve Sistem Analizi (Analist):** İş birimi taleplerinin olgunlaştırılması, kapsam dokümanlarının ve teknik ön analizlerin hazırlanması, regülatif/yasal uyum (BDDK, BKM, MC, Visa, e-Devlet) kurallarının tasarlanması ve DWH raporlama veri haritalamalarının yapılması.
2. **Inhouse Yazılım Geliştirme (Backend & Frontend):** Mobil uygulama ve internet bankacılığındaki tüm kart, başvuru, limit, ekstre, kampanya, Ayrıcalıklar Dünyası ve QR ödeme ekranlarının (FE) ve bunları besleyen mikroservis/API katmanlarının (BE) geliştirilmesi.
3. **Vendor ve Paydaş Orkestrasyonu:** Core kart ve bankacılık sistemlerini sağlayan vendorların (**Paycore, 4arc, Intertech, DZD/Fortuna, Developus, Cubetron, Dgpays**) teknik yönetimi, talep/efor takibi ve iç ekiplerle (**Tahsis, Strateji, ITLoans, Cash Management, CRM, Operasyon, DWH**) uçtan uca entegrasyonların koordinasyonu.
4. **Kalite, Sertifikasyon, Pilot ve Canlı Destek (UAT & Monitoring):** Entegrasyon ve UAT testlerinin yürütülmesi, MND/Pilot kurgularının doğrulanması, ulusal/uluslararası sertifikasyonların (3D Secure, BKM, Mastercard, Visa) tamamlanması, sistem izleme (monitoring/alerting) ve canlı ortam sürekliliğinin sağlanması.

---

## 2. Ana Domainler Bazında İş Tanımlarımız (Ne İş Yapıyoruz?)

### 2.1. Bireysel Kredi Kartı – Başvuru, Tahsis ve Müşteri Edinimi
Müşterinin kredi kartı başvurusundan kartın basılıp teslim edilmesine ve otomatik limit/gelir teyidine kadar olan tüm dijital edini̇m süreçlerini tasarlıyor ve geliştiriyoruz:
* **Yasal Dijital Gelir Belgesi ve e-Devlet Entegrasyonları:** Gelir beyanı süreçlerinde e-Devlet yönlendirmesi, 4A/4C SGK hizmet dökümlerinin ve ek gelir belgelerinin (vergi levhası, maaş bordrosu, emekli belgesi) alınması, RPA (robot) öğrenme ve otomatik gelir hesaplama akışlarının geliştirilmesi (`PPY-2948`).
* **Mobilde Otomatik Gelir Beyanlaştırma ve Tahsis Entegrasyonu:** Son 60 günlük model gelir verisiyle mobil uygulamada otomatik gelir beyanlaştırma yapılması, beyan-model gelir karşılaştırma kurallarının işletilmesi ve Strategy Management (SM) / ITLoans tahsis akışlarına entegre edilmesi (`PPY-2118`).
* **Çok Kanallı (Omnichannel) Başvuru ve Ön Onay Otomasyonu:**
  * Mobil **Başvuru Takip Menüsü**nün uçtan uca (BE/FE) yenilenmesi.
  * **İletişim Merkezi (İM) ve Şube (Rota+)** kredi kartı başvuru süreçlerinin mobil kanala entegre edilmesi.
  * Günlük 120K+ kapasiteli **Ön Onay Süreçlerinin** Tahsis ve CRM ekipleriyle birlikte otomatize edilmesi ve çalışma kapasitesinin artırılması.
* **Kart Basım, Kurye (PTT) ve Teslimat Yönetimi:** Kredi kartı ve debit kart gönderimleri için **PTT entegrasyonu**nun yapılması, başvuru/teslimat anında manuel yeni adres girilebilmesi ve kurye takip SMS bildirim senaryolarının yönetilmesi (`PPY-1124`).

### 2.2. Sadakat Programı (Ayrıcalıklar Dünyası) ve QR Ödeme Deneyimi
Müşteri bağlılığını ve kart kullanımını artıran sadakat kurguları ile yeni nesil ödeme (QR) çözümlerinin tüm mobil/web deneyimlerini geliştiriyoruz:
* **Odea Ayrıcalıklar Dünyası (Loyalty) Geliştirmeleri:**
  * Kilitli ayrıcalıkların ve üst segment/program gösterimlerinin mobil ve web kanallarında geliştirilmesi (`PPY-2118`).
  * Bankacılık ayrıcalıkları mobil kullanım takibi, segment varlık baremleri, kazanım oranları ve maksimum cap güncellemelerinin yapılması.
  * Aile varlık ve kart harcama kazanım değerlendirme yönetimi, uygulama içi *Story* gösterimleri ve *Alterna* anket entegrasyonlarının hayata geçirilmesi.
* **POS QR Ödeme ve Kampanya Altyapısı:**
  * Mobil uygulama üzerinden **POS QR ile ödeme** akışlarının, dijital slip ve işlem sonuç ekranlarının geliştirilmesi (`PPY-1954`).
  * **Akbank POS QR entegrasyonu**, QR ödeme kampanya kurguları, müşteri memnuniyeti anketleri ve QR işlem raporlama altyapısının kurulması.

### 2.3. Kredi Kartı Ürün Özellikleri, Altyapı ve Regülasyon Yönetimi
Mevcut kart müşterilerinin günlük finansal işlemlerini, kart ayarlarını, bildirimlerini ve temel kart altyapısını yönetiyoruz:
* **Kart Limit ve Güvenlik/Ayarlar Yönetimi:** Sanal kart ve ek kart limit düzenleme ekranları (`PPY-2118`), yurt dışı fiziki ve temassız işlem ayarlarının mobile eklenmesi (`DMSD-2574`), kredi kartı ve debit kart geçici kapama/dondurma, kart yenileme ve mobil uygulama üzerinden kredi kartı kapatma akışlarının geliştirilmesi.
* **Nakit Akışı, Taksitlendirme ve Ekstre Fonksiyonları:** İşlem erteleme, ekstre erteleme/böldür fonksiyonları, **KDİG (Kredi Değerliliği İzleme)** yeni yasal düzenlemeleri (`PPY-1752`), ekstre ve dönem içi iade gösterimlerinin iyileştirilmesi, kart harcama kategorilendirme geliştirmeleri.
* **Yeni Kart Ürünleri, Cüzdan ve Kanal Entegrasyonları:**
  * **Troy Axess** kartların başvuruya açılması (`PPY-1733`) ve **Metal Kart** ürün geliştirmeleri.
  * **NFC ile Ödeme**, **Masterpass** ve **Açık Bankacılık** kart entegrasyonları.
  * Yeni mobil anasayfa için **Favori Kart Tanımı**, işlemler menüsü bileşenleri ve görme engelli müşteriler için **Voiceover (Erişilebilirlik)** geliştirmeleri (`PPY-2253`).
* **Kart Altyapı Dönüşümü ve Uluslararası Kuruluş (MC/Visa/BKM) Uyumu:**
  * Kredi kartı ana altyapı dönüşüm projesi (**Dgpays dönüşümü**) hazırlık ve entegrasyon çalışmaları.
  * **Mastercard, Visa ve BKM** dönemsel zorunlu bülten geliştirmeleri ve sertifikasyonları.
  * Harcama ve ekstre **SMS & Push Bildirim** altyapılarının yönetimi, mobil kampanya tracking altyapısı ve servis performans iyileştirmeleri.

### 2.4. BaaS (Banking as a Service), Ticari Kredi Kartı ve SME (Ödeal) Ortaklıkları
Bankanın dış ekosistemlere açılan gömülü finans (BaaS) ve KOBİ/Ticari kart projelerinin servis mimarisini ve entegrasyonlarını kurguluyoruz:
* **BaaS Projeleri (TYF / Odea):** TYF/Odea Bireysel Debit ve Ticari Debit kart projelerinin yürütülmesi; akıllı hesaplar hiyerarşisi, placement, TYF nots modülü ve servis performans çalışmalarının yönetilmesi (`PPY-2212`).
* **SME – Ödeal İş Ortaklığı Projesi:**
  * Ödeal iş ortaklığı kapsamında ticari/SME müşteriler için uçtan uca kart yaşam döngüsü API'lerinin ve Inhouse BE servislerinin geliştirilmesi.
  * Ek kredi kartı kontak müşteri edinimi (`ODEA-13441`), kart oluşturma (`PODEAECR-789`), kart bilgisi ve statü yönetimi (`PODEAECR-790`), e-ticaret/MOTO/temassız kart ayarları (`PODEAECR-791`), karta para transferleri (`PODEAECR-792`) ve kart hareketleri/ekstre gösterimi (`PODEAECR-793`) servislerinin sağlanması ve UAT testlerinin yürütülmesi.
* **Ticari Kredi Kartı:** Ticari kredi kartı başvuru süreçlerinin analizi, Inhouse Backend ve Frontend başvuru ekranlarının geliştirilmesi ve limit tahsis entegrasyonlarının yapılması.

### 2.5. Bireysel Debit Kart ve Güvenlik / Kriptografik Uyum
Banka kartı (Debit) ürünlerinin hem müşteri yüzündeki fonksiyonlarını hem de kritik bilgi güvenliği ve kartlı ödeme standartlarını yönetiyoruz:
* **Kriptografik Güvenlik ve Hassas Veri Koruma:**
  * **Key Block (`DMTY-3875`):** Kart PIN ve kriptografik anahtar yapısının uluslararası güvenlik standartlarına (Key Block) taşınması.
  * **Hassas Veri Maskeleme:** Fortuna (Core) ile iç servisler arasındaki ve core servislerin kendi içindeki hassas kart verilerinin maskelenmesi.
  * **8 Haneli BIN Geçişi:** Uluslararası kart kuruluşu standartları doğrultusunda 6/8/9 haneli yeni BIN yapısına geçiş analiz ve geliştirmeleri.
* **Debit Kart Ürün ve İşlem Geliştirmeleri:**
  * **Fiziki Troy Debit** kart geliştirmeleri ve **Sanal POS On-Us (`PPY-1860`)** entegrasyonları.
  * Debit kart iadelerinin online olarak müşteri hesabına yansıtılması.
  * Debit kart geçici kapama, MOTO (Mail Order / Telephone Order) işlemlerine açma, debit kampanya kazanım geliştirmeleri ve Masterpass altyapısına debit kartların eklenmesi.

### 2.6. ATM Kanalı, QR İşlemleri ve Operasyonel Sistemler
Bankanın fiziksel temas noktası olan ATM ağının yazılım, donanım yenileme, QR işlem ve operasyonel izleme süreçlerini yönetiyoruz:
* **Yeni ATM Alımı ve Yaygınlaştırma:** Yeni nesil ATM'lerin alımı, yazılım uyarlamaları, prod pilot süreçleri, saha yaygınlaştırması ve izleme (monitoring) altyapısının kurulması (`PPY-2118`).
* **ATM QR ve Limit Yönetimi:**
  * **ATM QR Tekilleştirme (`DMTY-4356`)** ve BKM talebi kapsamında karekodla para yatırma işlemlerine kart numarası ve PIN sorgu mesajlarının eklenmesi.
  * **ATM Limit Yönetimi (`PPY-2864`):** ATM para çekme/yatırma limitlerinin Fortuna, 4arc ve Cubetron katmanlarında uçtan uca doğru çalışması ve kesinti/reverse (ters işlem) senaryolarında müşteri mağduriyetini önleyecek yapıların kurulması.
* **ATM Operasyon, İzleme ve Ekran Yönetimi:**
  * **Bantaş HYS** (Nakit Yönetimi) manuel süreçlerinin iyileştirilmesi ve otomatize edilmesi (`PPY-3045`).
  * ATM performans ve sistem izleme (monitoring/alert) ekranlarının iyileştirilmesi.
  * ATM banner ve şube ekranları yönetimi, şube/ATM **Google Haritalar** konumlandırma entegrasyonu, ATM SMS işlemleri ve lokasyon/sözleşme fizibilite takibi.

### 2.7. Veri Analitiği, Raporlama, Kampanya ve Operasyonel Süreçler
Kartlı ödeme sistemlerinin finansal, operasyonel ve müşteri davranışı metriklerinin izlenmesi ve yapay zekâ ile verimlilik sağlanması çalışmalarını yürütüyoruz:
* **Performans, KPI ve Harcama Analitiği:** Kredi kartı ve debit kart harcama analizleri, kart KPI takibi, QR ödeme ve para çekme/yatırma raporlaması, **Dataroid** kullanıcı davranış analizleri, Ayrıcalıklar Dünyası / GastroClub / Wee Kart kullanım analizleri, ATM işlem performansı ve faiz dışı gelir analizleri.
* **Yapay Zekâ (AI) ile Otomasyon:** **Mastercard ve BKM fatura analizlerinin** yapay zekâ destekli olarak otomatize edilmesi, Visa/Mastercard bülten analizleri.
* **Kampanya ve Operasyon Yönetimi:** Aylık kampanya analiz/planlama süreçleri, Mastercard ve Axess kampanya/ödül yönetimi, ekstre/sarf malzemesi/kart basım stok takibi, **OPEX gider ve yıllık bütçe yönetimi** ile yasal mevzuat takibi.

---

## 3. Ana Başlık – Modül / Sistem ve Paydaş Eşleştirme Tablosu (Mapping)

Yöneticilik ve IT/İş Birimi iletişimlerinde "Hangi alanda, hangi sistem/modül üzerinde, hangi iç/dış paydaşla ne yapıyoruz?" sorusunun özet haritası aşağıdadır:

| Sorumluluk Alanı (Ana Başlık) | İlgili Modül / Sistem Katmanı | HoC Ekibi Rolü | Çalışılan Vendorlar | İç Paydaşlar / Ekipler |
| :--- | :--- | :--- | :--- | :--- |
| **Kredi Kartı Başvuru & Tahsis** | Mobil/Web Başvuru, Gelir Belgesi, Ön Onay, Kurye | Analiz, Inhouse BE, Inhouse FE, Test | Developus, DZD, Paycore, Intertech, PTT | Bireysel Ödeme Sist., Tahsis, Strategy, ITLoans, CRM, Ops |
| **Ayrıcalıklar Dünyası & POS QR** | Loyalty Modülü, Mobil/Web Kart Bileşenleri, POS QR | Analiz, Inhouse BE, Inhouse FE, Test | Paycore, Alterna, Akbank POS | Bireysel Ödeme Sist., Dijital Kanallar, CRM |
| **Kredi Kartı Ürün & Altyapı** | Kart/Limit/Ekstre Yönetimi, Bültenler, Bildirim, Core Kart | Analiz, Inhouse BE, Inhouse FE, UAT | Paycore, Intertech, Dgpays, 4arc | Bireysel Ödeme Sist., Cash Man., Açık Bankacılık |
| **BaaS & SME (TYF / Ödeal)** | BaaS API Katmanı, SME Kart Servisleri, Ticari Kart | Analiz, Inhouse BE, Inhouse FE, UAT | Paycore, Intertech, 4arc, Ödeal, TYF | Bireysel/Ticari Ödeme Sist., Tahsis |
| **Bireysel Debit Kart & Güvenlik** | Debit Kart Modülü, HSM/Key Block, Veri Maskeleme, BIN | Analiz, Inhouse BE, Inhouse FE, UAT | 4arc, DZD (Fortuna), Paycore | Bireysel Ödeme Sist., Cash Man., Bilgi Güvenliği |
| **ATM Kanalı & Nakit Yönetimi** | ATM Uygulaması, ATM QR, Limit, Bantaş HYS, İzleme | Analiz, Inhouse BE, Inhouse FE, Monitoring | Cubetron, 4arc, DZD | Bireysel Ödeme Sist., ATM Operasyon, Nakit Yönetimi |
| **Analitik, Kampanya & Operasyon** | DWH Raporlama, Dataroid, Fatura AI, Stok & OPEX | Veri Analizi, Süreç ve Bütçe Yönetimi | Mastercard, Visa, BKM, GastroClub | Bireysel Ödeme Sist., DWH, Finans/Bütçe |

---

## 4. Özet Sayısal Göstergelerle Ekip İş Yükü

Portföyümüzdeki **154 aktif iş kalemi / proje / faaliyetin** dağılımı ekibimizin kapsam genişliğini göstermektedir:
* **84 Kalem** – Bireysel Kredi Kartı (Başvuru, Tahsis, Ayrıcalıklar Dünyası, QR, Ürün ve Altyapı)
* **21 Kalem** – BaaS (TYF), SME (Ödeal) ve Ticari Kredi Kartı Projeleri
* **12 Kalem** – Bireysel Debit Kart, Key Block, BIN ve Veri Güvenliği Projeleri
* **12 Kalem** – ATM Kanalı, ATM QR, Limit ve Operasyonel İyileştirme Projeleri
* **25 Kalem** – Veri Analitiği, KPI Raporlama, AI Fatura Otomasyonu, Kampanya ve OPEX/Stok Yönetimi

