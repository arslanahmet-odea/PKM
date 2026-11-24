Ai-asistant geri bildirimler:
Göker:
30 Ekim'de hatalı işlemleri doğru tablodan çekip getirdi. 4 Kasım'da aynı sohbeti devam ettirdiğimde farklı tablodan hatalı işlemleri aramaya çalıştı hata verdi.
Sohbet içinde ESB Tool'dan veriler getiriyordu. Sohbet devam ederken birden ESB Tool ile erişimi kesildi, tekrar aktif olunca bakabiliriz dedi yarım kalmış oldu.
Ara ara sıraya aldığı mesajları unutuyor devam ettirmiyor ama nadir / Bazen 504 hataları da gelebiliyor.

Barış:
her girişte kurulum. kurulum sonrası yine login sayfası 3-4  kere enter
 
jira tokenlarını değiştirmeme ragmen 2-3 kere girmedi.
 
Skiller sadece 4IT filtreleme göremiyorum. Herkes kendi yeteneğini tanımlayabilmeli ve bence özel olmalı.
 
Agentler kime göre ne göre hangi mcp neyi kullanıyor default veriler benim için anlamsız tanımlayacağım 3 tane ajan elk ve jira için farklı da bir işime yaramayacak.
 
Her seferinde mcp seçimi bunu ben kendimde ayarladım. Genel bir agent bilgiye göre ilgili agenta atıyor.
 
db bağlantısı  yok genelde bilgileri dbden kullanıyorum oracle da eklenmesi gerekiyor. (sql)
 
sohbette durdurmada yok takılıyor.
Yanlış tool seçimi ai yok pass
Uat logları inceletme çok yavaş

Batuhan:
Selam abi bu ai platform kullandım biraz, review ettirmede güzel oldu, elk log baktırdım birinde güzel buldu ama bazılarında network hatası Verdi ya da belki tam detaylı prompt verilmediğinden uzun sürdü biraz daha deneyimle daha güzel feedbackler verilirim

Avni:
ai assistantta kullanacağım tek alan şuan için log inceleme olacaktır. onun dışındakileri zaten localde vscode üzerinden yapabiliyorum. sadece dün bi ufak tanıtılan, uat ortamda çalışan token alma kullanıcı bilgileri gibi alanların olduğu yeni eklenti kullanılabilir. onu da çok deneyimlemedim. ai assistanttaki log inceleme çok yavaş çalışıyor veya bazen hiç çalışmıyor.. istediğimiz sonuçları almak için henüz yeteri kadar iyi değil. belki kalıp promtlar çıkarılırsa veya standard queryleri işleyecek skiller eklenip, elk sorguları gibi kullanabileceğimiz bir chat yapısına bürünürse faydalı olacaktır. şuan için elk dan devam etmekteyim

Ahmet:
ekrandaki sohbet geçmişini getirdiğimde çok defa hata aldı, ekran kapandı.
log'ları getirmesi çok uzun sürüyor, saniyesinde elk'dan bakacağım iş için dakikalarca beklemek zorunda kalınıyor, bazen getiriyor, bazen veri getirmiyor
jira bağlantısı var görünüyor ama bana bağlı taskları getir dediğimde çalışmadı.

Ömer:
bir developer gözünden kullanılacak elk ve gitlab dışında tool un çok olduğunu düşünmüyorum. elk official mcp kullanılmadığı için olabilir çok yavaş ve tutarsız çalışıyor. Aynı sorguya bazen cevap dönerken bazen hiç dönmüyor bazen ise yavaş dönüyor (ki çok açıklayıcı skill girmeme ragmen). gitlabda ise review kısmını biraz kurcaladım orada da network error alıyorum changes sayısı düşük ya da fazla da olsa
gitlab review tutarlılık gösterdiğinde iyi review ediyor abi bu arada; ama elk'da hiç verim alamadım

kısacası tutarlı davranış çok göstermediğinden kullanımına alışamadım 


Co-driver
Çağlar:
	Eksiler
		- Kodlama için ide kullanımı çok öznel. Herkesin rahat ettiği ide'yi kullanması optimum.
		- Model seçimi kısıtlı sadece claude ve qwen kullanılabiliyor. Oysa ki kullanabileceğimiz bir çok model mevcut. Ben özellikle gpt-6 luna'yı tercih ediyorum. Çok verimli, hızlı ve az kredi tüketiyor.
		- Geliştirme template'leri tanımlanmış, çeşitli ajanlar tanımlanmış, bunlara ihtiyaç duymuyorum. Bu ve benzeri ajanları vscode'a zaten implemente edebiliyoruz ve bunlar benim için çok daha uygun.
	Artılar
		- Uygulama içi entegrasyonlar gayet kullanışlı
	Tavsiye
		- CMS menüsü de eklenebilir. Manuel ekleme ve düzenleme işleri çok vakit alıyor.
AI-assistant
	Bu tool iş birimine aktarılırsa üretkenlik açısından çok faydalı olur. 
	Sprint planlama ve dokümantasyon modülleri iş birimi ve analistlerin işini kolaylaştıracaktır.
	Jira entegrasyonunu kullanıyorum. Bunun haricinde ihtiyaç duyduğum bir konu yok. Buradaki model ve ajanlara zaten vscode üzerinden erişebiliyorum ve kullanıyorum.

Sefa:
Geliştirici ekipte, sorun olursa ekleme yapıyor, 4 tane de öneri sunmuş.


