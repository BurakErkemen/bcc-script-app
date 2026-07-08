Proje Detay

Amacı
Proje, bir Chat Müşteri Hizmetleri firması için Windows üzerinde çalışan, hazır scriptleri yönetmeye ve hızlıca kullanmaya yönelik bir WPF uygulaması olarak tasarlanacak.

Genel Gereksinimler
- Hazır scriptleri ekleyip çıkarma, düzenleme, silme
- Scriptleri hızlıca seçip kopyalama
- Local database ile veri depolama
- Aynı anda en az 3 chat senaryosuna uygun bir UI
- WPF tabanlı, yalın ama etkin bir kullanıcı arayüzü

Mimari Tasarım

1) Veri Katmanı
- Local database: SQLite önerisi
- ORM: Entity Framework Core veya Dapper
- Temel tablolar:
  - Script: Id, Başlık, İçerik, Kategori, Etiketler, OluşturmaTarihi, GüncellemeTarihi, Favori
  - Kategori: Id, Ad
  - ChatSekmesi: Id, Ad, VarsayılanScript, SonKullanılanScriptId

2) Uygulama Katmanı
- MVVM mimarisi önerisi
- ViewModel'ler:
  - MainViewModel: uygulama ana durumu, seçili chat, script listesi, kategori seçimi
  - ScriptListViewModel: script filtreleme, arama, kategori bazlı listeleme
  - ScriptDetailViewModel: seçilen script içeriği, kopyala komutu
  - ChatTabsViewModel: 3 chat sekmesi yönetimi

3) UI Katmanı
- Ana bölümler:
  - Sol paneller: script listesi, filtreleme, arama
  - Orta panel: seçili script detayları ve kopyala butonu
  - Sağ üst/üst panel: 3 chat sekmesi veya hedef seçimi

UI Önerisi 1: Klasik script paneli + 3 chat sekmesi
- Üst kısımda 3 ayrı chat sekmesi
- Solda kategori / favori / arama paneli
- Orta kısımda seçili kategoriye ait script listesi
- Sağda seçilen script detay kutusu ve kopyala düğmesi
- Chat hedefi için her scriptte "Chat 1 / Chat 2 / Chat 3" seçimi veya kopyala sonrası hedef belirtme

UI Önerisi 2: Sağ dar panel + hamburger menü
- Sağda ekranın yaklaşık 1/6-1/7'si genişliğinde dar bir panel
- Bu panelde hamburger menü ve kategoriler/favoriler için butonlar
- Menüde seçilen kategoriye göre orta/sol alanda script listesi görünür
- Script seçildiğinde detay alanı açılır ve "Kopyala" butonu gösterilir
- Bu tasarımda kullanım hızlı ve alan sade kalır

3 Chat Senaryosu İçin Öneri
- Aynı anda 3 chat işleyebilecek şekilde sekmeli kullanım
- Her chat için ayrı sekme veya buton
- Script seçilip kopyalandığında hedef chat seçimi yapılabilir
- Alternatif: script kopyala işlemi sistem panosuna eklenir, kullanıcı aktif chat uygulamasına yapıştırır

Detaylandırma
- Script yönetimi: yeni script ekleme, başlık/etiket/kategori düzenleme, silme
- Filtreleme ve arama: kategori, etiket, başlığa göre
- Favoriler: sık kullanılan scriptleri hızlıca erişilebilir yapma
- Kopyalama: tek tıklama ile metin clipboard'a kopyalama, kullanıcıya geri bildirim

Teknoloji Önerisi
- WPF (.NET 6 veya .NET 8)
- SQLite + EF Core
- MVVM kütüphanesi: Prism, MVVM Light, CommunityToolkit.Mvvm
- UI: modern, yüksek kontrastlı, hızlı erişimli

Sonuç
Bu mimari ile hem klasik script listesi yaklaşımı hem de sağ dar panel + hamburger menü yaklaşımı desteklenebilir. Uygulama, 3 chat senaryosunda hızlı script seçme ve kopyalama deneyimi sağlayacak şekilde tasarlanmalı.

