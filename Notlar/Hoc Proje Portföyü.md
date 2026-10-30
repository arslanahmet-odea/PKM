# HoC Proje Portföyü

> **Kaynak:** `Hoc Proje Portföyü.xlsx`  
> **İş Kolu:** Bireysel Bankacılık Ödeme Sistemleri  
> **Dahili Ekip:** House of Cards (HoC)  
> **Toplam Kayıt:** 155 satır  

## 📊 Yönetici Özeti ve İstatistikler

### Statü Dağılımı

| Statü | Kayıt Sayısı | Oran (%) |
| :--- | :---: | :---: |
| **Beklemede** | 19 | %19.4 |
| **Backlog** | 19 | %19.4 |
| **Geliştirme** | 17 | %17.3 |
| **Analiz** | 8 | %8.2 |
| **Tamamlandı** | 5 | %5.1 |
| **UAT** | 5 | %5.1 |
| **Pilot** | 5 | %5.1 |
| **Hold** | 5 | %5.1 |
| **Değerlendirme** | 3 | %3.1 |
| **Teklif** | 2 | %2 |
| **Planlama** | 2 | %2 |
| **Talep Açıldı** | 2 | %2 |
| **Test** | 2 | %2 |
| **Prod pilot** | 1 | %1 |
| **Stratejik Öncelik Bekleniyor** | 1 | %1 |
| **Kapsam Netleşecek** | 1 | %1 |
| **Onay Bekliyor** | 1 | %1 |
| *Belirtilmemiş / Başlık / Süreç* | 57 | - |

### Portföy Alanları Dağılımı

| # | Domain / Alan | Satır Aralığı | Toplam | Proje | Alt İş Paketi |
| :---: | :--- | :---: | :---: | :---: | :---: |
| 1 | [BaaS](#1-baas) | R2-R4 | 3 | 2 | 0 |
| 2 | [Bireysel Kredi Kartı](#2-bireysel-kredi-karti) | R5-R91 | 84 | 25 | 66 |
| 3 | [Ticari Kredi Kartı](#3-ticari-kredi-karti) | R92-R96 | 5 | 0 | 0 |
| 4 | [SME (Ödeal)](#4-sme-ödeal) | R97-R109 | 13 | 6 | 0 |
| 5 | [Bireysel Debit Kart](#5-bireysel-debit-kart) | R110-R121 | 12 | 7 | 10 |
| 6 | [ATM](#6-atm) | R122-R133 | 12 | 5 | 11 |
| 7 | [Raporlama, Analiz ve Performans Takibi](#7-raporlama-analiz-ve-performans-takibi) | R135-R150 | 16 | 0 | 15 |
| 8 | [Kampanyalar](#8-kampanyalar) | R151-R155 | 5 | 0 | 4 |
| 9 | [Operasyonel ve Yasal Geliştirmeler](#9-operasyonel-ve-yasal-geliştirmeler) | R156-R159 | 4 | 0 | 3 |

---

## 📑 İçindekiler

- [HoC Proje Portföyü](#hoc-proje-portföyü)
  - [📊 Yönetici Özeti ve İstatistikler](#-yönetici-özeti-ve-i̇statistikler)
    - [Statü Dağılımı](#statü-dağılımı)
    - [Portföy Alanları Dağılımı](#portföy-alanları-dağılımı)
  - [📑 İçindekiler](#-i̇çindekiler)
  - [1. BaaS](#1-baas)
  - [2. Bireysel Kredi Kartı](#2-bireysel-kredi-kartı)
    - [2.1 Kredi Kartı - Başvuru Projeleri](#21-kredi-kartı---başvuru-projeleri)
    - [2.2 Kredi Kartı - Ayrıcalıklar Dünyası Loyalty Programı](#22-kredi-kartı---ayrıcalıklar-dünyası-loyalty-programı)
    - [2.3 Kredi Kartı - Paydaşlık Gerektiren Projeler](#23-kredi-kartı---paydaşlık-gerektiren-projeler)
    - [2.4 Kredi Kartı - Ürün Özelliği Geliştirmeleri](#24-kredi-kartı---ürün-özelliği-geliştirmeleri)
  - [3. Ticari Kredi Kartı](#3-ticari-kredi-kartı)
  - [4. SME (Ödeal)](#4-sme-ödeal)
  - [5. Bireysel Debit Kart](#5-bireysel-debit-kart)
  - [6. ATM](#6-atm)
  - [7. Raporlama, Analiz ve Performans Takibi](#7-raporlama-analiz-ve-performans-takibi)
  - [8. Kampanyalar](#8-kampanyalar)
  - [9. Operasyonel ve Yasal Geliştirmeler](#9-operasyonel-ve-yasal-geliştirmeler)
  - [10. Ham Portföy Tablosu (Tüm Liste)](#10-ham-portföy-tablosu-tüm-liste)

---

## 1. BaaS

| PPY No | Talep / İş Paketi Adı | Tür | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Efor | Açıklama | Sonraki Aksiyon |
| :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- |
| `PPY-2212` | TYF/Odea Bireysel Debit Kalan İşler | Proje | 🔴 **Hold** | 95% | 4arc | XL | HoC | XL | Analist, BE |  | Placementlar, TYF nots modülü, akıllı hesaplar hiyerarşisi geliştirilmeye başlanmadı.Performans çalışmaları TYF'de devam ediyordu. |  |
| `PPY-2212` | TYF/Odea Ticari Debit | Proje | 🔵 **Geliştirme** | 70% | 4arc | M | HoC | XL | Analist, BE |  | %90 tamamlandı. Kalanı Odea SME için tamamlanacak, Odea SME'de kullanılacak. |  |

## 2. Bireysel Kredi Kartı

### 2.1 Kredi Kartı - Başvuru Projeleri

| PPY No | Talep / İş Paketi Adı | Tür | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Efor | Açıklama | Sonraki Aksiyon |
| :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- |
| `PPY-2948` | Dijital gelir belgesi - E-devlet Entegrasyonu(Yasal) | Proje | 🔵 **Geliştirme** | 50% | Developus, DZD | M | HoC, Strategy, ITLoans | L | Analist,BE,FE | 60% |  |  |
|  | ↳ e-Devlet entegrasyonu ve gelir belgesi yönlendirme akışı | Alt İş Paketi | 🟣 **Pilot** | 50% | Developus, DZD |  | HoC, Strategy, ITLoans |  |  |  | Gelir Beyanı menüsünden e-Devlet yönlendirmesi ve 4A/4C SGK hizmet dökümlerinin bankaya iletilmesi. | Pilot bulgularını takip et |
|  | ↳ Pilot çalışmalarının yürütülmesi ve MND akışları | Alt İş Paketi | 🟣 **Pilot** |  | Developus, DZD |  |  |  |  |  | Eylül ortasında başlayan pilotta MND akışları test ediliyor; başarılı örnekler elde ediliyor. | Pilot sonuçlarını değerlendir |
|  | ↳ Robot öğrenme ve gelir hesaplama iyileştirmeleri | Alt İş Paketi | 🟣 **Pilot** |  | Developus, DZD |  |  |  |  |  | Pilot sürecinde robotun öğrenmesi ve hesaplama mantığına yönelik iyileştirmeler yapıldı; çalışmalar sürüyor. | İyileştirmeleri takip et |
|  | ↳ Tahsis stratejisi ve canlıya geçiş değerlendirmesi | Alt İş Paketi | Değerlendirme |  | Developus, DZD |  |  |  |  |  | Tahsis stratejisindeki değişiklik nedeniyle e-Devlet akışlarının kapalı tutulması seçeneği de değerlendiriliyor. | Tahsis kararını netleştir |
|  | ↳ DWH raporlama veri eşleştirmesi | Alt İş Paketi | ⚪ Beklemede |  | Developus, DZD |  |  |  |  |  | Pilot verisinin maplenmesi DWH ekibine iletildi; geri dönüş bekleniyor. | DWH geri dönüşünü takip et |
| `-` | Dijital gelir belgesi - Ek gelir belgesi entegrasyonu(Yasal) | Proje | 📦 Backlog | - | Developus, DZD | M | HoC, Strategy, ITLoans | L | Analist,BE,FE | 60% |  |  |
|  | ↳ Kapsam dokümanının hazırlanması | Alt İş Paketi | Analiz | 25% | Developus, DZD |  |  |  |  |  | Serbest meslek sahipleri ve çalışmaya devam eden emeklilerin gelir belgeleri de kapsama alınacak. | Kapsamı netleştir |
|  | ↳ Teknik ön analiz | Alt İş Paketi | Analiz | 5% | Developus, DZD |  |  |  |  |  | Gelir belgesi kaynakları ve sistem bağımlılıkları ön analiz kapsamında değerlendiriliyor. | Ön analizi ilerlet |
|  | ↳ Tasarım çalışmalarının hazırlanması | Alt İş Paketi | 📦 Backlog | 0% | Developus, DZD |  |  |  |  |  | Kapsam netleştikten sonra tasarım süreci başlatılacak. | Kapsam sonrası tasarımı başlat |
|  | ↳ Tahsis stratejisi ve belge kurallarının netleştirilmesi | Alt İş Paketi | Değerlendirme |  | Developus, DZD |  |  |  |  |  | Vergi levhası, maaş bordrosu/yazısı ve çalışan emekli belgesi için belge talep ve hesaplama kuralları netleştirilecek. | Tahsis ile kural setini netleştir |
|  | ↳ Entegrasyon ve teknik geliştirmeler | Alt İş Paketi | 📦 Backlog |  | Developus |  | HoC, ITLoans, Strategy |  |  |  | ITLoans, Developus, HoC, RPA ve DYS geliştirmeleri; belgeye göre otomatik okuma veya manuel kontrol akışları. | Teknik kapsam ve bağımlılıkları netleştir |
|  | ↳ DWH raporlama kapsamının oluşturulması | Alt İş Paketi | 📦 Backlog |  | Developus, DZD |  |  |  |  |  | Yeni gelir belgesi akışlarına ilişkin raporlama ihtiyaçları değerlendirilecek. | DWH gereksinimlerini netleştir |
| `PPY-2118` | Mobilde gelir beyanlaştırma | Proje | 🔵 **Geliştirme** | 50% | - | - | HoC | S | Analist,BE,FE | 100% |  |  |
|  | ↳ Model gelir ile otomatik gelir beyanlaştırma | Alt İş Paketi | 🟣 **Pilot** |  | Developus, DZD |  |  |  |  |  | Son 60 gün içinde hesaplanan model gelir ile aylık net gelir alanının otomatik doldurulması hedefleniyor. | Pilot sonuçlarını takip et |
|  | ↳ MND akışları ve pilot doğrulama | Alt İş Paketi | 🟣 **Pilot** |  | Developus, DZD |  |  |  |  |  | MND akışları üzerinden pilot çalışmaları yürütülüyor. | Pilot çıktıları değerlendir |
|  | ↳ Tahsis strateji değişikliği ve hesaplama akışının revizyonu | Alt İş Paketi | Talep Açıldı |  | Developus, DZD |  |  |  |  |  | Tahsis talebiyle hesaplamanın SM tarafında yönetilmesi, sonucun output alanına yazılması ve ILoan’a iletilmesi planlanıyor. | Geliştirme talebini takip et |
|  | ↳ Model gelir ve son beyan geliri karşılaştırma kuralı | Alt İş Paketi | Değerlendirme |  | Developus, DZD |  |  |  |  |  | Model gelir son beyan gelirinden yüksekse otomatik doldurmama önerisi için HoC kontrolleri devam ediyor. | Kuralın uygulanabilirliğini netleştir |
| `PPY-1124` | Kart gönderim PTT Entegrasyonu | Proje | 🔵 **Geliştirme** | 50% | Paycore | M | HoC, Cash Man., Ops | M | Analist | 100% |  |  |
|  | ↳ Kredi ve debit kart entegrasyon geliştirmeleri | Alt İş Paketi | 🔵 **Geliştirme** |  | Paycore, Intertech |  | HoC, Ops |  |  |  | Paycore kredi kartı geliştirmesi %100; Intertech debit kart geliştirmesi %90 tamamlandı. | Debit kalan geliştirmeleri takip et |
|  | ↳ PTT ile ortak entegrasyon testleri | Alt İş Paketi | 🟡 **Test** | 50% | PTT, Paycore, Intertech |  | HoC, Ops |  |  |  | PTT ile ortak testlerde ilerleme %50. | Test senaryolarını tamamla |
|  | ↳ SMS bildirim senaryoları ve ilave ihtiyaç analizi | Alt İş Paketi | Analiz | 10% |  |  | HoC |  |  |  | Yeni geliştirmeyle tetiklenecek SMS’ler ve ek SMS ihtiyaçları belirleniyor. | SMS kapsamını netleştir |
| `-` | Başvuru Takip Menüsü Düzenleme | Proje | 📦 Backlog | - | - | - | HoC | M | Analist,BE,FE | 100% |  |  |
|  | ↳ Kapsam dokümanının hazırlanması | Alt İş Paketi | Onay Bekliyor | 100% |  |  |  |  |  |  | Kapsam hazırlandı; Duygu’nun onayı bekleniyor. | Kapsam onayını al |
|  | ↳ Teknik ön analiz | Alt İş Paketi | Analiz | 100% |  |  |  |  |  |  | Barış tarafından ön analiz tamamlandı; kapsamın son haline göre güncellenebilir. | Kapsam sonrası analizi gözden geçir |
|  | ↳ Menü ve ekran tasarımlarının hazırlanması | Alt İş Paketi | 📦 Backlog | 0% |  |  |  |  |  |  | Kapsam finalize edildikten sonra tasarım ekibine iletilecek. | Tasarım talebini ilet |
|  | ↳ Uçtan uca teknik geliştirmeler | Alt İş Paketi | 📦 Backlog |  |  |  |  |  |  |  | Backend ve frontend geliştirmeleri aynı iş paketi altında takip edilecek. | Tasarım sonrası geliştirme planını netleştir |
| `-` | Ön Onay Süreçlerinin Otomatize Edilmesi | Proje | 📦 Backlog | - | - | - | HoC | M | Analist,BE | 100% |  |  |
|  | ↳ Kapsam çalışmalarının tamamlanması | Alt İş Paketi | 🟢 **Tamamlandı** | 100% |  |  |  |  |  |  | Kapsam hazır. | Teknik planlamayı takip et |
|  | ↳ Günlük ön onay çalıştırma kapasitesinin artırılması | Alt İş Paketi | Planlama |  |  |  |  |  |  |  | Mevcut günlük 120K run kapasitesinin artırılması hedefleniyor. | Hedef kapasiteyi netleştir |
|  | ↳ Ön onay otomasyon geliştirmeleri | Alt İş Paketi | Planlama |  |  |  | Tahsis, CRM, HoC |  |  |  | Tahsis, CRM ve HoC tarafındaki geliştirmeler birlikte takip edilecek. | Ekipler arası geliştirme planını netleştir |
| `-` | Yeni Bir Teslimat Adresinin Manuel Girilebilmesi | Proje | 📦 Backlog | - | Paycore | S | HoC | S | Analist,BE,FE | 100% |  |  |
|  | ↳ Geliştirme taleplerinin oluşturulması | Alt İş Paketi | Talep Açıldı |  | Paycore, Intertech |  |  |  |  |  | Paycore talebi açıldı ve düşük öncelikle listeye alındı; Intertech talebi de açıldı. | Önceliklendirme durumunu takip et |
|  | ↳ Manuel adres girişi teknik geliştirmeleri | Alt İş Paketi | 📦 Backlog |  |  |  |  |  |  |  | Backend ve frontend ihtiyaçları aynı iş paketi kapsamında takip edilecek. | Geliştirme kapsamını netleştir |
|  | ↳  Diğer Kanallar(IM,Şube) Başvuru Süreç İyileştirme | Proje | 📦 Backlog | - | Developus | L | HoC | L | Analist,BE,FE | 100% | İletişim merkezi tarafında başvuru sürecinin mobile entegrasyonu için çalışmalara başlandı. | Selin ve Buse ile  kapsamı netleştir |
|  | ↳ İM ve şube başvuru akışlarının değerlendirilmesi | Alt İş Paketi | Kapsam Netleşecek |  |  |  |  |  |  |  | Rota+ kapsamındaki süreç iyileştirmeleri henüz netleştirilmedi. | Selin ile kapsamı netleştir |

### 2.2 Kredi Kartı - Ayrıcalıklar Dünyası Loyalty Programı

| PPY No | Talep / İş Paketi Adı | Tür | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Efor | Açıklama | Sonraki Aksiyon |
| :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- |
| `PPY-2118` | Ayrıcalklar Dünyası Mobil Ek Geliştirmeler |  |  |  | - |  |  |  |  |  |  |  |
|  | ↳ Kilitli ayrıcalıklar ve üst program gösterimi | Proje | Analiz | 10% | - | - | HoC | L | Analist,BE,FE | 100% |  |  |
|  | ↳ Bankacılık ayrıcalıkları mobil kullanım takip |  | Analiz | 20% |  |  | HoC |  |  |  |  |  |
|  | ↳ Kilitli ayrıcalıklar ve üst program gösterimi |  | 🟢 **Tamamlandı** | 100% |  |  | HoC |  |  |  |  |  |
|  | ↳ Ayrıcalıklar Dünyası stroy geliştirmesi |  | 🔴 **Hold** | 30% |  |  | HoC |  |  |  |  |  |
|  | ↳ Segment Varlık Barem Güncellemeleri | Proje | 🔵 **Geliştirme** | 100% | - | - | HoC | S | Analist | 100% |  |  |
|  | ↳ Kazanım oranı ve maksimum cap güncellemeleri |  | 🔵 **Geliştirme** | 100% |  |  | HoC |  |  |  |  |  |
|  | ↳ Mobil uygulama ve web güncellemeleri |  | 🔵 **Geliştirme** | 100% |  |  | HoC |  |  |  |  |  |
|  | ↳ Ayrıcalıklar Dünyası Alterna anket geliştirmesi |  | 🟡 **Test** | 80% |  |  | HoC |  |  |  |  |  |
|  | ↳ Ayrıcalıklar Dünyası aile varlık ve kart harcama kazanım değerlendirme yönetimi |  | 🔴 **Hold** | 0% |  |  | HoC |  |  |  |  |  |
| `PPY-1954` | POS QR Ödeme Geliştirmeleri | Proje | 🟡 **UAT** | 80% | Paycore | S | HoC | M | Analist,BE,FE | 100% |  |  |
|  | ↳  QR ödeme mobil uygulama geliştirmeleri |  |  | 90% |  |  |  |  |  |  |  |  |
|  | ↳  QR ödeme Akbank Pos entegrasyonu |  |  | 50% |  |  |  |  |  |  |  |  |
|  | ↳ QR Ödeme Kampanya kurgu çalışmaları |  |  | 30% |  |  |  |  |  |  |  |  |
|  | ↳ QR ödeme raporlama |  |  | 20% |  |  |  |  |  |  |  |  |
|  | ↳ QR Ödeme Alterna anket geliştirmesi |  |  | 100% |  |  |  |  |  |  |  |  |

### 2.3 Kredi Kartı - Paydaşlık Gerektiren Projeler

| PPY No | Talep / İş Paketi Adı | Tür | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Efor | Açıklama | Sonraki Aksiyon |
| :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- |
| `PPY-2253` | ↳  Voiceover | Proje | 🔵 **Geliştirme** | 80% | - | - | HoC | S | Analist,BE,FE | 100% |  |  |
| `PPY-2118` | ↳  Favori kart tanımı(yeni anasayfa için) | Proje | 🔵 **Geliştirme** | 40% | - | - | HoC | S | Analist,BE,FE | 100% |  |  |
|  | ↳ Yeni anasayfa işlemler menüsü geliştirmeleri |  | 🟢 **Tamamlandı** |  |  |  |  |  |  |  |  |  |
| `PPY-1752` | ↳ KDIG Yeni Düzenleme | Proje | 🔵 **Geliştirme** | - | - | - | HoC | M | Analist,BE,FE | 100% |  |  |
|  | ↳ Açık Bankacılık Geliştirmeleri |  |  |  |  |  |  |  |  |  |  |  |

### 2.4 Kredi Kartı - Ürün Özelliği Geliştirmeleri

| PPY No | Talep / İş Paketi Adı | Tür | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Efor | Açıklama | Sonraki Aksiyon |
| :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- |
| `DMSD-2574` | ↳  Temassız – yurt dışı fiziki mobile eklenmesi | Proje | 🟡 **UAT** | 100% | Paycore | S | HoC | S | Analist,BE,FE | 100% |  |  |
| `PPY-2118` | ↳  Sanal kart limiti düzenleme geliştirmesi | Proje | 🔵 **Geliştirme** | 50% | Paycore | S | HoC | S | Analist,BE,FE | 100% |  |  |
|  | ↳ Ek kart limit düzenleme geliştirmesi |  |  |  |  |  |  |  |  |  |  |  |
|  | ↳ Mobil uygulama kredi kartı componentlerinin düzenlenmesi |  |  |  |  |  |  |  |  |  |  |  |
| `PPY-1733` | ↳  Troy Axess'in Başvuruya Açılması | Proje | Teklif | - | Intertech | S | HoC | S | Analist | 100% |  |  |
| `-` | ↳ Kart Harcama Kategorilendirme | Proje | 📦 Backlog | - | Paycore | M | HoC | L | Analist,BE,FE | 100% |  |  |
| `-` | ↳ Kart geçiçi kapama/yenileme(KK+Debit) | Proje | 📦 Backlog | - | Paycore, 4arc | M | HoC | M | Analist,BE,FE | 100% |  |  |
| `-` | ↳ İşlem erteleme fonksiyonunun geliştirilmesi | Proje | 📦 Backlog | - | Paycore | M | HoC | M | Analist,BE,FE | 100% |  |  |
| `-` | ↳ Ekstre erteleme/böldür fonksiyonunun geliştirilmesi | Proje | 📦 Backlog | - | Paycore | M | HoC | M | Analist,BE,FE | 100% |  |  |
|  | ↳ Kart Yenileme Süreçlerinin İyileştirilmesi |  |  |  |  |  |  |  |  |  |  |  |
| `-` | ↳ Kredi Kartı Altyapı Dönüşümü(Dgpays) | Proje | ⚪ Beklemede | - | Dgpays, Intertech | XXL | HoC | XXL | Analist,BE,FE | 100% |  |  |
| `-` | Altyapı performans iyileştirmeleri | Proje | 🔵 **Geliştirme** | - | - | - | HoC | - | Analist,BE,FE | 100% |  |  |
|  | NFC Ödeme Geliştirmesi |  | ⚪ Beklemede |  |  |  |  |  |  |  |  |  |
|  | Aİ |  |  |  |  |  |  |  |  |  |  |  |
| `-` | MC-Visa-BKM Bültenleri | Proje | 🔵 **Geliştirme** | - | Paycore | - | HoC, Cash Man. | - | - |  |  |  |
|  | ↳ POS QR Dijital Slip, İşlem Sonuç ve KDİG Geliştirmeleri |  | 🔵 **Geliştirme** |  |  |  |  |  |  |  |  |  |
|  | ↳ Kredi Kartı Mobil Kapatma Akışı |  | 🟢 **Tamamlandı** |  |  |  |  |  |  |  |  |  |
|  | ↳ Metal Kart Geliştirmeleri |  | 🔴 **Hold** |  |  |  |  |  |  |  |  |  |
|  | ↳ Mobil Kampanya Tracking Altyapısı |  | 🔴 **Hold** |  |  |  |  |  |  |  |  |  |
|  | ↳ Harcama ve Ekstre Push Bildirimlerinin Yeniden Devreye Alınması |  | Analiz |  |  |  |  |  |  |  |  |  |
|  | ↳ Harcama ve Ekstre SMS Bildirimlerinin Yeniden Devreye Alınması |  | 🟢 **Tamamlandı** |  |  |  |  |  |  |  |  |  |
|  | ↳ Ekstre ve Dönem İçi İade Gösteriminin Düzenlenmesi |  | Analiz |  |  |  |  |  |  |  |  |  |

## 3. Ticari Kredi Kartı

| PPY No | Talep / İş Paketi Adı | Tür | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Efor | Açıklama | Sonraki Aksiyon |
| :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- |
|  | Ticari kredi kartı başvuru analiz |  | ⚪ Beklemede | 0% | House of Cards | L | HoC |  | Analist | 100% |  |  |
|  | Başvuru BE Geliştirmeleri |  | ⚪ Beklemede | 0% | House of Cards | L | HoC |  | BE | 100% |  |  |
|  | Başvuru FE Geliştirmeleri |  | ⚪ Beklemede | 0% | House of Cards | L | HoC |  | FE | 100% |  |  |
|  | Limit Tahsis Geliştirmeleri |  | ⚪ Beklemede | 0% | Tahsis | L | HoC |  | Tahsis | 100% |  |  |

## 4. SME (Ödeal)

| PPY No | Talep / İş Paketi Adı | Tür | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Efor | Açıklama | Sonraki Aksiyon |
| :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- |
|  | Kart Tasarım Süreçleri |  | ⚪ Beklemede | 0% | Bireysel Bankacılık Ödeme Sistemleri | L | HoC |  |  |  |  |  |
|  | Kart Sipariş ve Stoklara alınması |  | ⚪ Beklemede | 0% | Bireysel Bankacılık Ödeme Sistemleri | L | HoC |  |  |  |  |  |
|  | API Geliştirmeleri |  | ⚪ Beklemede | 0% | Paycore & Intertech | L | HoC |  | Analist,BE,FE |  |  |  |
| `ODEA-13441` | Ek Kredi Kartı Kontak Müşteri Edinimi |  | ⚪ Beklemede | 0% | Intertech | L | HoC |  | Analist,BE,FE |  |  |  |
| `PODEAECR-789` | Kart oluşturma |  | ⚪ Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |
| `PODEAECR-790` | Kart Bilgisi Gösterimi ve Kart Statü Yönetim |  | ⚪ Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |
| `PODEAECR-791` | Kart Ayarları (Ecom-Moto-Temassız) |  | ⚪ Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |
| `PODEAECR-792` | Karta Para Transferleri |  | ⚪ Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |
| `PODEAECR-793` | Kart Hareketleri ve Ekstre Gösterim |  | ⚪ Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |
|  | Inhouse BE Geliştirmeleri&Test |  | ⚪ Beklemede | 0% | House of Cards | L | HoC |  | Analist,BE,FE |  |  |  |
|  | Ödeal Geliştirmeleri |  | ⚪ Beklemede | 0% | Ödeal | L | HoC |  | Analist,BE,FE |  |  |  |
|  | UAT Testleri |  | ⚪ Beklemede | 0% | House of Cards | L | HoC |  | Analist,BE,FE |  |  |  |

## 5. Bireysel Debit Kart

| PPY No | Talep / İş Paketi Adı | Tür | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Efor | Açıklama | Sonraki Aksiyon |
| :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- |
|  | ↳ Fortuna ile core servisler arasındaki hassas verilerin maskelenmesi | Proje | 🟡 **UAT** | 90% | DZD | M | HoC | S | BE | 100% |  |  |
| `DMTY-3875` | ↳ Key Block | Proje | 🔵 **Geliştirme** | 20% | 4arc | XXL | HoC, Cash Man. | L | Analist | 100% |  |  |
| `-` | ↳ Core servislerdeki hassas verilerin maskelenmesi | Proje | 📦 Backlog | - | 4arc | - | HoC | - | - |  |  |  |
| `-` | ↳ Fiziki Troy debit geliştirmesi | Proje | 📦 Backlog | - | 4arc | M | HoC | M | Analist,BE,FE | 100% |  |  |
| `-` | ↳ Debit kart iadelerinin online yapılması | Proje | 📦 Backlog | - | 4arc | M | HoC | M | Analist | 100% |  |  |
| `-` | ↳ 8 haneli BIN yapısına geçiş | Proje | Stratejik Öncelik Bekleniyor | - | 4arc | XL | HoC, Cash Man. | L | Analist | 100% |  |  |
| `PPY-1860` | ↳ Sanal POS NoU geliştirmeleri | Proje | 🔵 **Geliştirme** | 25% | Paycore | XL | HoC, Cash Man. | S | Analist | 100% |  |  |
|  | ↳ Debit Kart Geçici Kapama | Başlık |  |  |  |  |  |  |  |  |  |  |
|  | ↳ Debit Kart Kampanya Geliştirmeleri | Başlık |  |  |  |  |  |  |  |  |  |  |
|  | ↳ Debit Kart MOTO İşlemlerine Açılması |  |  |  |  |  |  |  |  |  |  |  |
|  | Masterpass Altyapısına debit kartların eklenmesi |  |  |  |  |  |  |  |  |  |  |  |

## 6. ATM

| PPY No | Talep / İş Paketi Adı | Tür | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Efor | Açıklama | Sonraki Aksiyon |
| :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- |
| `DMTY-4356` | ↳ ATM QR tekilleştirme | Proje | 🟡 **UAT** | 80% | 4arc | M | HoC | M | Analist,BE,FE | 100% |  |  |
| `PPY-2118` | ↳ Yeni ATM alımı | Proje | 🟣 **Prod pilot** | 95% | Cubetron | XXL | HoC | XL | Analist | 100% | Geriye yaygınlaştırması ve monitoring kaldı. |  |
| `PPY-2864` | ↳ ATM limitlerinin doğru çalışır hale getirilmesi | Proje | 🟡 **UAT** | 80% | 4arc,DZD,Cubetron | M | HoC | M | Analist,BE,FE | 100% |  |  |
| `PPY-3045` | ↳ Bantaş HYS Manuel Süreç İyileştirmeleri | Proje | Teklif | 10% | 4arc | M | HoC | M | Analist | 100% |  |  |
| `-` | ↳ ATM performans/sistem izleme ekranlarının iyileştirmesi | Proje | 📦 Backlog | - | Cubetron | M | HoC | S | Analist | 100% |  |  |
|  | ↳ ATM Banner Ekran Yönetimi |  |  |  |  |  |  |  |  |  |  |  |
|  | ↳ Şube Ekranları Yönetimi |  |  |  |  |  |  |  |  |  |  |  |
|  | ↳ Şube ve ATM Google Haritalar Konumlandırma |  |  |  |  |  |  |  |  |  |  |  |
|  | ↳ ATM SMS İşlemleri |  |  |  |  |  |  |  |  |  |  |  |
|  | ↳ ATM Yeni Kira Lokasyonları İletişimi ve Eski Lokasyonların Sözleşme Devamlılığı Fizibilitesi |  |  |  |  |  |  |  |  |  |  |  |
|  | ↳ BKM Talebi – Karekod Para Yatırmada Kart No ve PIN Sorgu Mesajı Eklenmesi |  |  |  |  |  |  |  |  |  |  |  |

## 7. Raporlama, Analiz ve Performans Takibi

| Talep / Faaliyet Adı | Tür | Talep Sahibi İş Kolu |
| :--- | :--- | :--- |
| ↳ Debit Kart Harcama Analizleri | Başlık | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Kredi Kartı Harcama Analizleri | Başlık | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Kredi ve Banka Kartı KPI Takibi | Başlık | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ QR Ödeme İşlemleri Analizi | Başlık | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ QR Para Çekme ve Yatırma İşlemleri Raporlaması | Başlık | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Dataroid Kullanıcı Davranış Analizleri | Başlık | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ GastroClub Kullanım ve Performans Analizleri | Başlık | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ ATM İşlem ve Performans Raporlaması |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Visa ve Mastercard Bülten Analizleri |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Odea Ayrıcalıklar Dünyası Kullanım, Kazanım ve KPI Analizleri |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Faiz Dışı Gelir Takibi ve Analizi |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Kart Stok Takibi |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Yasal Duyuru ve Mevzuat Takibi |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Wee Kart Ayrıcalık Kullanım Analizleri |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Mastercard ve BKM Fatura Analizlerinin Yapay Zekâ ile Otomatize Edilmesi |  |  |

## 8. Kampanyalar

| Talep / Faaliyet Adı | Tür | Talep Sahibi İş Kolu |
| :--- | :--- | :--- |
| ↳ Aylık Kampanya Analizi, Planlama ve İletişimi |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Mastercard Kampanya Süreçleri ve Ödül Uygulamalarının Yönetimi |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ Axess Kampanya Takibi ve Mobil/Web Yayın Süreçleri |  | Bireysel Bankacılık Ödeme Sistemleri |
| ↳ GastroClub Kampanya İletişimi ve Koordinasyonu |  | Bireysel Bankacılık Ödeme Sistemleri |

## 9. Operasyonel ve Yasal Geliştirmeler

| Talep / Faaliyet Adı | Tür | Talep Sahibi İş Kolu |
| :--- | :--- | :--- |
| ↳ Ekstre, Sarf Malzemeleri ve Kart Basım Stoklarının Analizi, Takibi ve Yenileme Süreçleri |  |  |
| ↳ OPEX Gider Takibi ve Bütçe Yönetimi |  |  |
| ↳ Yıllık Bütçe Analizi ve Takibi |  |  |

---

## 10. Ham Portföy Tablosu (Tüm Liste)

<details>
<summary><strong>Tüm Excel Satırlarını Görüntüle (155 Satır)</strong></summary>

| Satır | PPY No | Talep Adı | Talep Sahibi İş Kolu | Statü | % | Vendor | V.Size | Internal | I.Size | Headcount | Takım Eforu | Açıklama | Tür | Sonraki Aksiyon |
| :---: | :--- | :--- | :--- | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :---: | :--- | :--- | :--- |
| 2 |  | BaaS |  |  |  |  |  |  |  |  |  |  | Başlık |  |
| 3 | PPY-2212 | TYF/Odea Bireysel Debit Kalan İşler | Bireysel Bankacılık Ödeme Sistemleri | Hold | 95% | 4arc | XL | HoC | XL | Analist, BE |  | Placementlar, TYF nots modülü, akıllı hesaplar hiyerarşisi geliştirilmeye başlanmadı.Performans çalışmaları TYF'de devam ediyordu. | Proje |  |
| 4 | PPY-2212 | TYF/Odea Ticari Debit | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 70% | 4arc | M | HoC | XL | Analist, BE |  | %90 tamamlandı. Kalanı Odea SME için tamamlanacak, Odea SME'de kullanılacak. | Proje |  |
| 5 |  | Bireysel Kredi Kartı |  |  |  |  |  |  |  |  |  |  | Başlık |  |
| 6 |  | Kredi Kartı - Başvuru Projeleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 7 | PPY-2948 | Dijital gelir belgesi - E-devlet Entegrasyonu(Yasal) | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 50% | Developus, DZD | M | HoC, Strategy, ITLoans | L | Analist,BE,FE | 60% |  | Proje |  |
| 8 |  | ↳ e-Devlet entegrasyonu ve gelir belgesi yönlendirme akışı | Bireysel Bankacılık Ödeme Sistemleri | Pilot | 50% | Developus, DZD |  | HoC, Strategy, ITLoans |  |  |  | Gelir Beyanı menüsünden e-Devlet yönlendirmesi ve 4A/4C SGK hizmet dökümlerinin bankaya iletilmesi. | Alt İş Paketi | Pilot bulgularını takip et |
| 9 |  | ↳ Pilot çalışmalarının yürütülmesi ve MND akışları | Bireysel Bankacılık Ödeme Sistemleri | Pilot |  | Developus, DZD |  |  |  |  |  | Eylül ortasında başlayan pilotta MND akışları test ediliyor; başarılı örnekler elde ediliyor. | Alt İş Paketi | Pilot sonuçlarını değerlendir |
| 10 |  | ↳ Robot öğrenme ve gelir hesaplama iyileştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Pilot |  | Developus, DZD |  |  |  |  |  | Pilot sürecinde robotun öğrenmesi ve hesaplama mantığına yönelik iyileştirmeler yapıldı; çalışmalar sürüyor. | Alt İş Paketi | İyileştirmeleri takip et |
| 11 |  | ↳ Tahsis stratejisi ve canlıya geçiş değerlendirmesi | Bireysel Bankacılık Ödeme Sistemleri | Değerlendirme |  | Developus, DZD |  |  |  |  |  | Tahsis stratejisindeki değişiklik nedeniyle e-Devlet akışlarının kapalı tutulması seçeneği de değerlendiriliyor. | Alt İş Paketi | Tahsis kararını netleştir |
| 12 |  | ↳ DWH raporlama veri eşleştirmesi | Bireysel Bankacılık Ödeme Sistemleri | Beklemede |  | Developus, DZD |  |  |  |  |  | Pilot verisinin maplenmesi DWH ekibine iletildi; geri dönüş bekleniyor. | Alt İş Paketi | DWH geri dönüşünü takip et |
| 13 | - | Dijital gelir belgesi - Ek gelir belgesi entegrasyonu(Yasal) | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | Developus, DZD | M | HoC, Strategy, ITLoans | L | Analist,BE,FE | 60% |  | Proje |  |
| 14 |  | ↳ Kapsam dokümanının hazırlanması | Bireysel Bankacılık Ödeme Sistemleri | Analiz | 25% | Developus, DZD |  |  |  |  |  | Serbest meslek sahipleri ve çalışmaya devam eden emeklilerin gelir belgeleri de kapsama alınacak. | Alt İş Paketi | Kapsamı netleştir |
| 15 |  | ↳ Teknik ön analiz | Bireysel Bankacılık Ödeme Sistemleri | Analiz | 5% | Developus, DZD |  |  |  |  |  | Gelir belgesi kaynakları ve sistem bağımlılıkları ön analiz kapsamında değerlendiriliyor. | Alt İş Paketi | Ön analizi ilerlet |
| 16 |  | ↳ Tasarım çalışmalarının hazırlanması | Bireysel Bankacılık Ödeme Sistemleri | Backlog | 0% | Developus, DZD |  |  |  |  |  | Kapsam netleştikten sonra tasarım süreci başlatılacak. | Alt İş Paketi | Kapsam sonrası tasarımı başlat |
| 17 |  | ↳ Tahsis stratejisi ve belge kurallarının netleştirilmesi | Bireysel Bankacılık Ödeme Sistemleri | Değerlendirme |  | Developus, DZD |  |  |  |  |  | Vergi levhası, maaş bordrosu/yazısı ve çalışan emekli belgesi için belge talep ve hesaplama kuralları netleştirilecek. | Alt İş Paketi | Tahsis ile kural setini netleştir |
| 18 |  | ↳ Entegrasyon ve teknik geliştirmeler | Bireysel Bankacılık Ödeme Sistemleri | Backlog |  | Developus |  | HoC, ITLoans, Strategy |  |  |  | ITLoans, Developus, HoC, RPA ve DYS geliştirmeleri; belgeye göre otomatik okuma veya manuel kontrol akışları. | Alt İş Paketi | Teknik kapsam ve bağımlılıkları netleştir |
| 19 |  | ↳ DWH raporlama kapsamının oluşturulması | Bireysel Bankacılık Ödeme Sistemleri | Backlog |  | Developus, DZD |  |  |  |  |  | Yeni gelir belgesi akışlarına ilişkin raporlama ihtiyaçları değerlendirilecek. | Alt İş Paketi | DWH gereksinimlerini netleştir |
| 20 | PPY-2118 | Mobilde gelir beyanlaştırma | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 50% | - | - | HoC | S | Analist,BE,FE | 100% |  | Proje |  |
| 21 |  | ↳ Model gelir ile otomatik gelir beyanlaştırma | Bireysel Bankacılık Ödeme Sistemleri | Pilot |  | Developus, DZD |  |  |  |  |  | Son 60 gün içinde hesaplanan model gelir ile aylık net gelir alanının otomatik doldurulması hedefleniyor. | Alt İş Paketi | Pilot sonuçlarını takip et |
| 22 |  | ↳ MND akışları ve pilot doğrulama | Bireysel Bankacılık Ödeme Sistemleri | Pilot |  | Developus, DZD |  |  |  |  |  | MND akışları üzerinden pilot çalışmaları yürütülüyor. | Alt İş Paketi | Pilot çıktıları değerlendir |
| 23 |  | ↳ Tahsis strateji değişikliği ve hesaplama akışının revizyonu | Bireysel Bankacılık Ödeme Sistemleri | Talep Açıldı |  | Developus, DZD |  |  |  |  |  | Tahsis talebiyle hesaplamanın SM tarafında yönetilmesi, sonucun output alanına yazılması ve ILoan’a iletilmesi planlanıyor. | Alt İş Paketi | Geliştirme talebini takip et |
| 24 |  | ↳ Model gelir ve son beyan geliri karşılaştırma kuralı | Bireysel Bankacılık Ödeme Sistemleri | Değerlendirme |  | Developus, DZD |  |  |  |  |  | Model gelir son beyan gelirinden yüksekse otomatik doldurmama önerisi için HoC kontrolleri devam ediyor. | Alt İş Paketi | Kuralın uygulanabilirliğini netleştir |
| 25 | PPY-1124 | Kart gönderim PTT Entegrasyonu | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 50% | Paycore | M | HoC, Cash Man., Ops | M | Analist | 100% |  | Proje |  |
| 26 |  | ↳ Kredi ve debit kart entegrasyon geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme |  | Paycore, Intertech |  | HoC, Ops |  |  |  | Paycore kredi kartı geliştirmesi %100; Intertech debit kart geliştirmesi %90 tamamlandı. | Alt İş Paketi | Debit kalan geliştirmeleri takip et |
| 27 |  | ↳ PTT ile ortak entegrasyon testleri | Bireysel Bankacılık Ödeme Sistemleri | Test | 50% | PTT, Paycore, Intertech |  | HoC, Ops |  |  |  | PTT ile ortak testlerde ilerleme %50. | Alt İş Paketi | Test senaryolarını tamamla |
| 28 |  | ↳ SMS bildirim senaryoları ve ilave ihtiyaç analizi | Bireysel Bankacılık Ödeme Sistemleri | Analiz | 10% |  |  | HoC |  |  |  | Yeni geliştirmeyle tetiklenecek SMS’ler ve ek SMS ihtiyaçları belirleniyor. | Alt İş Paketi | SMS kapsamını netleştir |
| 29 | - | Başvuru Takip Menüsü Düzenleme | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | - | - | HoC | M | Analist,BE,FE | 100% |  | Proje |  |
| 30 |  | ↳ Kapsam dokümanının hazırlanması | Bireysel Bankacılık Ödeme Sistemleri | Onay Bekliyor | 100% |  |  |  |  |  |  | Kapsam hazırlandı; Duygu’nun onayı bekleniyor. | Alt İş Paketi | Kapsam onayını al |
| 31 |  | ↳ Teknik ön analiz | Bireysel Bankacılık Ödeme Sistemleri | Analiz | 100% |  |  |  |  |  |  | Barış tarafından ön analiz tamamlandı; kapsamın son haline göre güncellenebilir. | Alt İş Paketi | Kapsam sonrası analizi gözden geçir |
| 32 |  | ↳ Menü ve ekran tasarımlarının hazırlanması | Bireysel Bankacılık Ödeme Sistemleri | Backlog | 0% |  |  |  |  |  |  | Kapsam finalize edildikten sonra tasarım ekibine iletilecek. | Alt İş Paketi | Tasarım talebini ilet |
| 33 |  | ↳ Uçtan uca teknik geliştirmeler | Bireysel Bankacılık Ödeme Sistemleri | Backlog |  |  |  |  |  |  |  | Backend ve frontend geliştirmeleri aynı iş paketi altında takip edilecek. | Alt İş Paketi | Tasarım sonrası geliştirme planını netleştir |
| 34 | - | Ön Onay Süreçlerinin Otomatize Edilmesi | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | - | - | HoC | M | Analist,BE | 100% |  | Proje |  |
| 35 |  | ↳ Kapsam çalışmalarının tamamlanması | Bireysel Bankacılık Ödeme Sistemleri | Tamamlandı | 100% |  |  |  |  |  |  | Kapsam hazır. | Alt İş Paketi | Teknik planlamayı takip et |
| 36 |  | ↳ Günlük ön onay çalıştırma kapasitesinin artırılması | Bireysel Bankacılık Ödeme Sistemleri | Planlama |  |  |  |  |  |  |  | Mevcut günlük 120K run kapasitesinin artırılması hedefleniyor. | Alt İş Paketi | Hedef kapasiteyi netleştir |
| 37 |  | ↳ Ön onay otomasyon geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Planlama |  |  |  | Tahsis, CRM, HoC |  |  |  | Tahsis, CRM ve HoC tarafındaki geliştirmeler birlikte takip edilecek. | Alt İş Paketi | Ekipler arası geliştirme planını netleştir |
| 38 | - | Yeni Bir Teslimat Adresinin Manuel Girilebilmesi | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | Paycore | S | HoC | S | Analist,BE,FE | 100% |  | Proje |  |
| 39 |  | ↳ Geliştirme taleplerinin oluşturulması | Bireysel Bankacılık Ödeme Sistemleri | Talep Açıldı |  | Paycore, Intertech |  |  |  |  |  | Paycore talebi açıldı ve düşük öncelikle listeye alındı; Intertech talebi de açıldı. | Alt İş Paketi | Önceliklendirme durumunu takip et |
| 40 |  | ↳ Manuel adres girişi teknik geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Backlog |  |  |  |  |  |  |  | Backend ve frontend ihtiyaçları aynı iş paketi kapsamında takip edilecek. | Alt İş Paketi | Geliştirme kapsamını netleştir |
| 41 |  | ↳  Diğer Kanallar(IM,Şube) Başvuru Süreç İyileştirme | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | Developus | L | HoC | L | Analist,BE,FE | 100% | İletişim merkezi tarafında başvuru sürecinin mobile entegrasyonu için çalışmalara başlandı. | Proje | Selin ve Buse ile  kapsamı netleştir |
| 42 |  | ↳ İM ve şube başvuru akışlarının değerlendirilmesi | Bireysel Bankacılık Ödeme Sistemleri | Kapsam Netleşecek |  |  |  |  |  |  |  | Rota+ kapsamındaki süreç iyileştirmeleri henüz netleştirilmedi. | Alt İş Paketi | Selin ile kapsamı netleştir |
| 43 |  | Kredi Kartı - Ayrıcalıklar Dünyası Loyalty Programı | Bireysel Bankacılık Ödeme Sistemleri |  |  | - |  |  |  |  |  |  | Başlık |  |
| 44 | PPY-2118 | Ayrıcalklar Dünyası Mobil Ek Geliştirmeler |  |  |  | - |  |  |  |  |  |  |  |  |
| 45 |  | ↳ Kilitli ayrıcalıklar ve üst program gösterimi | Bireysel Bankacılık Ödeme Sistemleri | Analiz | 10% | - | - | HoC | L | Analist,BE,FE | 100% |  | Proje |  |
| 46 |  | ↳ Bankacılık ayrıcalıkları mobil kullanım takip | Bireysel Bankacılık Ödeme Sistemleri | Analiz | 20% |  |  | HoC |  |  |  |  |  |  |
| 47 |  | ↳ Kilitli ayrıcalıklar ve üst program gösterimi | Bireysel Bankacılık Ödeme Sistemleri | Tamamlandı | 100% |  |  | HoC |  |  |  |  |  |  |
| 48 |  | ↳ Ayrıcalıklar Dünyası stroy geliştirmesi | Bireysel Bankacılık Ödeme Sistemleri | Hold | 30% |  |  | HoC |  |  |  |  |  |  |
| 49 |  | ↳ Segment Varlık Barem Güncellemeleri | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 100% | - | - | HoC | S | Analist | 100% |  | Proje |  |
| 50 |  | ↳ Kazanım oranı ve maksimum cap güncellemeleri | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 100% |  |  | HoC |  |  |  |  |  |  |
| 51 |  | ↳ Mobil uygulama ve web güncellemeleri | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 100% |  |  | HoC |  |  |  |  |  |  |
| 52 |  | ↳ Ayrıcalıklar Dünyası Alterna anket geliştirmesi | Bireysel Bankacılık Ödeme Sistemleri | Test | 80% |  |  | HoC |  |  |  |  |  |  |
| 53 |  | ↳ Ayrıcalıklar Dünyası aile varlık ve kart harcama kazanım değerlendirme yönetimi | Bireysel Bankacılık Ödeme Sistemleri | Hold | 0% |  |  | HoC |  |  |  |  |  |  |
| 54 | PPY-1954 | POS QR Ödeme Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | UAT | 80% | Paycore | S | HoC | M | Analist,BE,FE | 100% |  | Proje |  |
| 55 |  | ↳  QR ödeme mobil uygulama geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri |  | 90% |  |  |  |  |  |  |  |  |  |
| 56 |  | ↳  QR ödeme Akbank Pos entegrasyonu | Bireysel Bankacılık Ödeme Sistemleri |  | 50% |  |  |  |  |  |  |  |  |  |
| 57 |  | ↳ QR Ödeme Kampanya kurgu çalışmaları | Bireysel Bankacılık Ödeme Sistemleri |  | 30% |  |  |  |  |  |  |  |  |  |
| 58 |  | ↳ QR ödeme raporlama | Bireysel Bankacılık Ödeme Sistemleri |  | 20% |  |  |  |  |  |  |  |  |  |
| 59 |  | ↳ QR Ödeme Alterna anket geliştirmesi | Bireysel Bankacılık Ödeme Sistemleri |  | 100% |  |  |  |  |  |  |  |  |  |
| 60 |  | Kredi Kartı - Paydaşlık Gerektiren Projeler |  |  |  |  |  |  |  |  |  |  | Başlık |  |
| 61 | PPY-2253 | ↳  Voiceover | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 80% | - | - | HoC | S | Analist,BE,FE | 100% |  | Proje |  |
| 62 | PPY-2118 | ↳  Favori kart tanımı(yeni anasayfa için) | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 40% | - | - | HoC | S | Analist,BE,FE | 100% |  | Proje |  |
| 63 |  | ↳ Yeni anasayfa işlemler menüsü geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Tamamlandı |  |  |  |  |  |  |  |  |  |  |
| 64 | PPY-1752 | ↳ KDIG Yeni Düzenleme | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | - | - | - | HoC | M | Analist,BE,FE | 100% |  | Proje |  |
| 65 |  | ↳ Açık Bankacılık Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 68 |  | Kredi Kartı - Ürün Özelliği Geliştirmeleri |  |  |  |  |  |  |  |  |  |  | Başlık |  |
| 69 | DMSD-2574 | ↳  Temassız – yurt dışı fiziki mobile eklenmesi | Bireysel Bankacılık Ödeme Sistemleri | UAT | 100% | Paycore | S | HoC | S | Analist,BE,FE | 100% |  | Proje |  |
| 70 | PPY-2118 | ↳  Sanal kart limiti düzenleme geliştirmesi | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 50% | Paycore | S | HoC | S | Analist,BE,FE | 100% |  | Proje |  |
| 71 |  | ↳ Ek kart limit düzenleme geliştirmesi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 72 |  | ↳ Mobil uygulama kredi kartı componentlerinin düzenlenmesi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 73 | PPY-1733 | ↳  Troy Axess'in Başvuruya Açılması | Bireysel Bankacılık Ödeme Sistemleri | Teklif | - | Intertech | S | HoC | S | Analist | 100% |  | Proje |  |
| 74 | - | ↳ Kart Harcama Kategorilendirme | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | Paycore | M | HoC | L | Analist,BE,FE | 100% |  | Proje |  |
| 75 | - | ↳ Kart geçiçi kapama/yenileme(KK+Debit) | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | Paycore, 4arc | M | HoC | M | Analist,BE,FE | 100% |  | Proje |  |
| 76 | - | ↳ İşlem erteleme fonksiyonunun geliştirilmesi | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | Paycore | M | HoC | M | Analist,BE,FE | 100% |  | Proje |  |
| 77 | - | ↳ Ekstre erteleme/böldür fonksiyonunun geliştirilmesi | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | Paycore | M | HoC | M | Analist,BE,FE | 100% |  | Proje |  |
| 78 |  | ↳ Kart Yenileme Süreçlerinin İyileştirilmesi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 79 | - | ↳ Kredi Kartı Altyapı Dönüşümü(Dgpays) | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | - | Dgpays, Intertech | XXL | HoC | XXL | Analist,BE,FE | 100% |  | Proje |  |
| 80 | - | Altyapı performans iyileştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | - | - | - | HoC | - | Analist,BE,FE | 100% |  | Proje |  |
| 81 |  | NFC Ödeme Geliştirmesi | Bireysel Bankacılık Ödeme Sistemleri | Beklemede |  |  |  |  |  |  |  |  |  |  |
| 82 |  | Aİ | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 83 | - | MC-Visa-BKM Bültenleri | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | - | Paycore | - | HoC, Cash Man. | - | - |  |  | Proje |  |
| 84 |  | ↳ POS QR Dijital Slip, İşlem Sonuç ve KDİG Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme |  |  |  |  |  |  |  |  |  |  |
| 85 |  | ↳ Kredi Kartı Mobil Kapatma Akışı | Bireysel Bankacılık Ödeme Sistemleri | Tamamlandı |  |  |  |  |  |  |  |  |  |  |
| 86 |  | ↳ Metal Kart Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Hold |  |  |  |  |  |  |  |  |  |  |
| 87 |  | ↳ Mobil Kampanya Tracking Altyapısı | Bireysel Bankacılık Ödeme Sistemleri | Hold |  |  |  |  |  |  |  |  |  |  |
| 88 |  | ↳ Harcama ve Ekstre Push Bildirimlerinin Yeniden Devreye Alınması | Bireysel Bankacılık Ödeme Sistemleri | Analiz |  |  |  |  |  |  |  |  |  |  |
| 89 |  | ↳ Harcama ve Ekstre SMS Bildirimlerinin Yeniden Devreye Alınması | Bireysel Bankacılık Ödeme Sistemleri | Tamamlandı |  |  |  |  |  |  |  |  |  |  |
| 90 |  | ↳ Ekstre ve Dönem İçi İade Gösteriminin Düzenlenmesi | Bireysel Bankacılık Ödeme Sistemleri | Analiz |  |  |  |  |  |  |  |  |  |  |
| 92 |  | Ticari Kredi Kartı*** |  |  |  |  |  |  |  |  |  |  | Başlık |  |
| 93 |  | Ticari kredi kartı başvuru analiz | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | House of Cards | L | HoC |  | Analist | 100% |  |  |  |
| 94 |  | Başvuru BE Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | House of Cards | L | HoC |  | BE | 100% |  |  |  |
| 95 |  | Başvuru FE Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | House of Cards | L | HoC |  | FE | 100% |  |  |  |
| 96 |  | Limit Tahsis Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Tahsis | L | HoC |  | Tahsis | 100% |  |  |  |
| 97 |  | SME (ÖDEAL)*** |  |  |  |  |  |  |  |  |  |  | Başlık |  |
| 98 |  | Kart Tasarım Süreçleri | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Bireysel Bankacılık Ödeme Sistemleri | L | HoC |  |  |  |  |  |  |
| 99 |  | Kart Sipariş ve Stoklara alınması | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Bireysel Bankacılık Ödeme Sistemleri | L | HoC |  |  |  |  |  |  |
| 100 |  | API Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Paycore & Intertech | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 101 | ODEA-13441 | Ek Kredi Kartı Kontak Müşteri Edinimi | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Intertech | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 102 | PODEAECR-789 | Kart oluşturma | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 103 | PODEAECR-790 | Kart Bilgisi Gösterimi ve Kart Statü Yönetim | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 104 | PODEAECR-791 | Kart Ayarları (Ecom-Moto-Temassız) | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 105 | PODEAECR-792 | Karta Para Transferleri | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 106 | PODEAECR-793 | Kart Hareketleri ve Ekstre Gösterim | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Paycore | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 107 |  | Inhouse BE Geliştirmeleri&Test | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | House of Cards | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 108 |  | Ödeal Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | Ödeal | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 109 |  | UAT Testleri | Bireysel Bankacılık Ödeme Sistemleri | Beklemede | 0% | House of Cards | L | HoC |  | Analist,BE,FE |  |  |  |  |
| 110 |  | Bireysel Debit Kart |  |  |  |  |  |  |  |  |  |  | Başlık |  |
| 111 |  | ↳ Fortuna ile core servisler arasındaki hassas verilerin maskelenmesi | Bireysel Bankacılık Ödeme Sistemleri | UAT | 90% | DZD | M | HoC | S | BE | 100% |  | Proje |  |
| 112 | DMTY-3875 | ↳ Key Block | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 20% | 4arc | XXL | HoC, Cash Man. | L | Analist | 100% |  | Proje |  |
| 113 | - | ↳ Core servislerdeki hassas verilerin maskelenmesi | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | 4arc | - | HoC | - | - |  |  | Proje |  |
| 114 | - | ↳ Fiziki Troy debit geliştirmesi | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | 4arc | M | HoC | M | Analist,BE,FE | 100% |  | Proje |  |
| 115 | - | ↳ Debit kart iadelerinin online yapılması | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | 4arc | M | HoC | M | Analist | 100% |  | Proje |  |
| 116 | - | ↳ 8 haneli BIN yapısına geçiş | Bireysel Bankacılık Ödeme Sistemleri | Stratejik Öncelik Bekleniyor | - | 4arc | XL | HoC, Cash Man. | L | Analist | 100% |  | Proje |  |
| 117 | PPY-1860 | ↳ Sanal POS NoU geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Geliştirme | 25% | Paycore | XL | HoC, Cash Man. | S | Analist | 100% |  | Proje |  |
| 118 |  | ↳ Debit Kart Geçici Kapama | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 119 |  | ↳ Debit Kart Kampanya Geliştirmeleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 120 |  | ↳ Debit Kart MOTO İşlemlerine Açılması | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 121 |  | Masterpass Altyapısına debit kartların eklenmesi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 122 |  | ATM |  |  |  |  |  |  |  |  |  |  | Başlık |  |
| 123 | DMTY-4356 | ↳ ATM QR tekilleştirme | Bireysel Bankacılık Ödeme Sistemleri | UAT | 80% | 4arc | M | HoC | M | Analist,BE,FE | 100% |  | Proje |  |
| 124 | PPY-2118 | ↳ Yeni ATM alımı | Bireysel Bankacılık Ödeme Sistemleri | Prod pilot | 95% | Cubetron | XXL | HoC | XL | Analist | 100% | Geriye yaygınlaştırması ve monitoring kaldı. | Proje |  |
| 125 | PPY-2864 | ↳ ATM limitlerinin doğru çalışır hale getirilmesi | Bireysel Bankacılık Ödeme Sistemleri | UAT | 80% | 4arc,DZD,Cubetron | M | HoC | M | Analist,BE,FE | 100% |  | Proje |  |
| 126 | PPY-3045 | ↳ Bantaş HYS Manuel Süreç İyileştirmeleri | Bireysel Bankacılık Ödeme Sistemleri | Teklif | 10% | 4arc | M | HoC | M | Analist | 100% |  | Proje |  |
| 127 | - | ↳ ATM performans/sistem izleme ekranlarının iyileştirmesi | Bireysel Bankacılık Ödeme Sistemleri | Backlog | - | Cubetron | M | HoC | S | Analist | 100% |  | Proje |  |
| 128 |  | ↳ ATM Banner Ekran Yönetimi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 129 |  | ↳ Şube Ekranları Yönetimi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 130 |  | ↳ Şube ve ATM Google Haritalar Konumlandırma | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 131 |  | ↳ ATM SMS İşlemleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 132 |  | ↳ ATM Yeni Kira Lokasyonları İletişimi ve Eski Lokasyonların Sözleşme Devamlılığı Fizibilitesi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 133 |  | ↳ BKM Talebi – Karekod Para Yatırmada Kart No ve PIN Sorgu Mesajı Eklenmesi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 134 |  |  | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 135 |  | Raporlama, Analiz ve Performans Takibi |  |  |  |  |  |  |  |  |  |  | Başlık |  |
| 136 |  | ↳ Debit Kart Harcama Analizleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 137 |  | ↳ Kredi Kartı Harcama Analizleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 138 |  | ↳ Kredi ve Banka Kartı KPI Takibi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 139 |  | ↳ QR Ödeme İşlemleri Analizi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 140 |  | ↳ QR Para Çekme ve Yatırma İşlemleri Raporlaması | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 141 |  | ↳ Dataroid Kullanıcı Davranış Analizleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 142 |  | ↳ GastroClub Kullanım ve Performans Analizleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  | Başlık |  |
| 143 |  | ↳ ATM İşlem ve Performans Raporlaması | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 144 |  | ↳ Visa ve Mastercard Bülten Analizleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 145 |  | ↳ Odea Ayrıcalıklar Dünyası Kullanım, Kazanım ve KPI Analizleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 146 |  | ↳ Faiz Dışı Gelir Takibi ve Analizi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 147 |  | ↳ Kart Stok Takibi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 148 |  | ↳ Yasal Duyuru ve Mevzuat Takibi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 149 |  | ↳ Wee Kart Ayrıcalık Kullanım Analizleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 150 |  | ↳ Mastercard ve BKM Fatura Analizlerinin Yapay Zekâ ile Otomatize Edilmesi |  |  |  |  |  |  |  |  |  |  |  |  |
| 151 |  | Kampanyalar |  |  |  |  |  |  |  |  |  |  |  |  |
| 152 |  | ↳ Aylık Kampanya Analizi, Planlama ve İletişimi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 153 |  | ↳ Mastercard Kampanya Süreçleri ve Ödül Uygulamalarının Yönetimi | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 154 |  | ↳ Axess Kampanya Takibi ve Mobil/Web Yayın Süreçleri | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 155 |  | ↳ GastroClub Kampanya İletişimi ve Koordinasyonu | Bireysel Bankacılık Ödeme Sistemleri |  |  |  |  |  |  |  |  |  |  |  |
| 156 |  | Operasyonel ve Yasal Geliştirmeler |  |  |  |  |  |  |  |  |  |  |  |  |
| 157 |  | ↳ Ekstre, Sarf Malzemeleri ve Kart Basım Stoklarının Analizi, Takibi ve Yenileme Süreçleri |  |  |  |  |  |  |  |  |  |  |  |  |
| 158 |  | ↳ OPEX Gider Takibi ve Bütçe Yönetimi |  |  |  |  |  |  |  |  |  |  |  |  |
| 159 |  | ↳ Yıllık Bütçe Analizi ve Takibi |  |  |  |  |  |  |  |  |  |  |  |  |

</details>

